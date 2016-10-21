namespace CodeRedundancyCheck.Languages.CSharp
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using CodeRedundancyCheck.Model;

    public class CSharpCodeFileWriter
    {
        public async Task WriteFile(string filename, IEnumerable<CodeLine> codeLines , Encoding encoding)
        {
            using (var fileStream = File.Create(filename))
            {
                await this.WriteStream(fileStream, codeLines, encoding);
            }
        }

        public async Task WriteStream(Stream stream, IEnumerable<CodeLine> codeLines, Encoding encoding, bool leaveStreamOpen = false)
        {
            using (var writer = new StreamWriter(stream, encoding, 4096, leaveStreamOpen))
            {
                foreach (var line in codeLines)
                {
                    await writer.WriteLineAsync(line.WriteableLine);
                }
            }
        }
    }
}
