namespace CodeRedundancyCheck
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using CodeRedundancyCheck.Interface;
    using CodeRedundancyCheck.Model;

    public class CodeFileLoader : ICodeFileLoader
    {
        private static readonly int ProcessorCount = Environment.ProcessorCount;

        private readonly ICodeFileIndexer indexer;

        private readonly ICodeLineFilter lineFilter;

        private readonly ICodeFileLineIndexer lineIndexer;

        private readonly ISourceWash sourceWash;

        public CodeFileLoader(ISourceWash sourceWash, ICodeFileIndexer indexer, ICodeFileLineIndexer lineIndexer, ICodeLineFilter lineFilter)
        {
            this.sourceWash = sourceWash;
            this.indexer = indexer;
            this.lineIndexer = lineIndexer;
            this.lineFilter = lineFilter;
        }

        public async Task<CodeFile> LoadCodeFileAsync(Stream codeFileStream, Encoding encoding, bool leaveStreamOpen = false)
        {
            var lines = new List<CodeLine>(10000);

            var lineNumber = 0;

            string fullSource;
            using (var reader = new StreamReader(codeFileStream, encoding, true, 4096, leaveStreamOpen))
            {
                fullSource = await reader.ReadToEndAsync();
            }

            var lineBuilder = new StringBuilder(4096);

            var lastNewLineChar = false;

            foreach (var ch in fullSource)
            {
                if (ch == '\r' || ch == '\n')
                {
                    if (lastNewLineChar)
                    {
                        lastNewLineChar = false;
                        continue;
                    }

                    lastNewLineChar = true;

                    var codeLine = new CodeLine(lineBuilder.ToString(), ++lineNumber, 0);
                    lines.Add(codeLine);

                    lineBuilder.Clear();
                }

                lineBuilder.Append(ch);
                lastNewLineChar = false;
            }

            if (lineBuilder.Length > 0)
            {
                var codeLine = new CodeLine(lineBuilder.ToString(), ++lineNumber, 0);
                lines.Add(codeLine);
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

        public async Task<IReadOnlyCollection<CodeFile>> LoadCodeFiles(IReadOnlyCollection<string> filenames, Encoding encoding, int concurrencyLevel = -1)
        {
            if (concurrencyLevel < 1)
            {
                concurrencyLevel = ProcessorCount;
            }

            var codeFileItems = new List<CodeFileItem>(filenames.Count);

            foreach (var filename in filenames.OrderBy(f => f))
            {
                codeFileItems.Add(new CodeFileItem(filename, () => this.LoadBufferedCodeFile(filename, encoding)));
            }

            var tasks = new List<Task>(concurrencyLevel);

            var codeFileItemQueue = new ConcurrentQueue<CodeFileItem>(codeFileItems);

            for (var i = 0; i < concurrencyLevel; i++)
            {
                tasks.Add(Task.Run(() => this.LoadCodeFileItemFromQueueAsync(codeFileItemQueue)));
            }

            await Task.WhenAll(tasks);

            return codeFileItems.Select(t => t.CodeFile).ToArray();
        }

        private async Task<CodeFile> LoadBufferedCodeFile(string filename, Encoding encoding)
        {
            MemoryStream buffer;

            using (var stream = File.OpenRead(filename))
            {
                var length = stream.Length;
                buffer = new MemoryStream((int)length);
                await stream.CopyToAsync(buffer);
                buffer.Position = 0;
            }

            return await this.LoadCodeFileAsync(buffer, encoding);
        }

        private async Task LoadCodeFileItemFromQueueAsync(ConcurrentQueue<CodeFileItem> items)
        {
            CodeFileItem item;

            while (items.TryDequeue(out item))
            {
                item.CodeFile = await item.Func.Invoke();
            }
        }

        private class CodeFileItem
        {
            public CodeFileItem(string filename, Func<Task<CodeFile>> codeFile)
            {
                this.Filename = filename;
                this.Func = codeFile;
            }

            public CodeFile CodeFile { get; set; }

            public string Filename { get; private set; }

            public Func<Task<CodeFile>> Func { get; private set; }
        }
    }
}