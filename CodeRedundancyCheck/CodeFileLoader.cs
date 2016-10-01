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
    public class CodeFileLoader : ICodeFileLoader
    {
        private readonly ISourceWash sourceWash;
        private readonly ICodeFileIndexer indexer;

        public CodeFileLoader(ISourceWash sourceWash, ICodeFileIndexer indexer)
        {
            this.sourceWash = sourceWash;
            this.indexer = indexer;
        }

        public async Task<CodeFile> LoadCodeFile(Stream codeFileStream, Encoding encoding, bool leaveStreamOpen = false)
        {
            var lines = new List<CodeLine>(10000);

            int lineNumber = 0;

            using (var reader = new StreamReader(codeFileStream, encoding, true, 4096, leaveStreamOpen))
            {
                while (reader.EndOfStream == false)
                {
                    var line = await reader.ReadLineAsync();
                    lines.Add(new CodeLine(line, ++lineNumber, 0));
                }
            }

            var codeFile = new CodeFile
            {
                CodeLines = this.sourceWash.Wash(lines).ToList()
            };

            this.indexer.IndexCodeFile(codeFile);

            return codeFile;

        }

    }
}
