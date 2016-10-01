using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRedundancyCheck.VisualBasic
{
    public class VisualBasicCodeFileWriter
    {
        public void WriteFile(string filename, CodeFile codeFile)
        {
            var builder = new StringBuilder(80 * 1024);
            var lines = File.ReadAllLines(codeFile.Filename, Encoding.Default);

            int currentLine = 0;

            foreach (var line in codeFile.CodeLines)
            {
                if (line.OriginalLineNumber != 0)
                {
                    while (currentLine < line.OriginalLineNumber)
                    {
                        builder.AppendLine(lines[currentLine].TrimEnd());
                        currentLine++;
                    }
                }
                else
                {
                    builder.AppendLine(line.OriginalLineText);
                }
            }

            File.WriteAllText(filename, builder.ToString(), Encoding.Unicode);
        }

    }
}
