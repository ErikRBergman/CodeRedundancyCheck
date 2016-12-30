using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeRedundancyCheck.Interface;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    using System.Collections.Concurrent;
    using System.Threading;

    public class CodeFileLoader : ICodeFileLoader
    {
        private readonly ISourceWash sourceWash;
        private readonly ICodeFileIndexer indexer;
        private readonly ICodeFileLineIndexer lineIndexer;
        private readonly ICodeLineFilter lineFilter;

        public CodeFileLoader(ISourceWash sourceWash, ICodeFileIndexer indexer, ICodeFileLineIndexer lineIndexer, ICodeLineFilter lineFilter)
        {
            this.sourceWash = sourceWash;
            this.indexer = indexer;
            this.lineIndexer = lineIndexer;
            this.lineFilter = lineFilter;
        }

        private class CodeFileItem
        {
            public CodeFileItem(string filename, Func<Task<CodeFile>> codeFile)
            {
                this.Filename = filename;
                this.Func = codeFile;
            }

            public string Filename { get; private set; }

            public Func<Task<CodeFile>> Func { get; private set; }

            public CodeFile CodeFile { get; set; }

        }

        public async Task<IReadOnlyCollection<CodeFile>> LoadCodeFiles(IReadOnlyCollection<string> filenames, Encoding encoding, int concurrencyLevel)
        {
            var loadFileTasks = new List<CodeFileItem>(filenames.Count);

            var semaphore = new SemaphoreSlim(1);

            foreach (var filename in filenames.OrderBy(f => f))
            {
                loadFileTasks.Add(new CodeFileItem(filename, () => this.LoadBufferedCodeFile(filename, encoding, semaphore)));
            }

            var tasks = new List<Task>(concurrencyLevel);

            for (var i = 0; i < concurrencyLevel; i++)
            {
                tasks.Add(Task.Run(() => this.Load(new ConcurrentQueue<CodeFileItem>(loadFileTasks), semaphore)));
            }

            await Task.WhenAll(tasks);

            return loadFileTasks.Select(t => t.CodeFile).ToArray();
        }

        private async Task<CodeFile> LoadBufferedCodeFile(string filename, Encoding encoding, SemaphoreSlim semaphore)
        {
            MemoryStream buffer;

            try
            {
                await semaphore.WaitAsync();

                using (var stream = File.OpenRead(filename))
                {
                    var length = stream.Length;
                    buffer = new MemoryStream((int)length);
                    await stream.CopyToAsync(buffer);
                    buffer.Position = 0;
                }
            }
            finally
            {
                semaphore.Release();
            }

            return await this.LoadCodeFileAsync(buffer, encoding);

        }

        private async Task Load(ConcurrentQueue<CodeFileItem> items, SemaphoreSlim semaphore)
        {
            CodeFileItem item;

            while (items.TryDequeue(out item))
            {
                item.CodeFile = await item.Func.Invoke();
            }
        }

        public async Task<CodeFile> LoadCodeFileAsync(Stream codeFileStream, Encoding encoding, bool leaveStreamOpen = false)
        {
            var lines = new List<CodeLine>(10000);

            int lineNumber = 0;

            using (var reader = new StreamReader(codeFileStream, encoding, true, 4096, leaveStreamOpen))
            {
                //var fullSource = reader.ReadToEndAsync();
                //var lineBuilder = new StringBuilder(256);




                while (reader.EndOfStream == false)
                {
                    var line = await reader.ReadLineAsync();
                    var codeLine = new CodeLine(line, ++lineNumber, 0);
                    lines.Add(codeLine);
                }
            }

            var allWashedLines = this.sourceWash.Wash(lines).ToArray();

            var codeFile = new CodeFile
            {
                CodeLines = allWashedLines.Where(line => line.IsCodeLine).ToArray(),
                AllSourceLines = allWashedLines
            };

            this.lineIndexer.IndexCodeFile(codeFile);
            this.indexer.IndexCodeFile(codeFile);

            foreach (var codeLine in allWashedLines)
            {
                codeLine.MayStartBlock = this.lineFilter.MayStartBlock(codeLine);
            }

            return codeFile;

        }

    }
}
