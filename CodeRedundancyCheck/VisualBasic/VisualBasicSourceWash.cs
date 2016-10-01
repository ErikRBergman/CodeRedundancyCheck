using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeRedundancyCheck.Interface;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    public class VisualBasicSourceWash : ISourceWash
    {
        public static ISourceWash Singleton { get; } = new VisualBasicSourceWash();

        public IEnumerable<CodeLine> Wash(IEnumerable<CodeLine> lines)
        {
            int lineNumber = 0;

            foreach (var line in lines)
            {
                line.WashedLineText = this.WashLine(line.OriginalLineText);

                // Remove empty and comment lines
                if (line.WashedLineText.Length > 0 && line.WashedLineText[0] != '\'')
                {
                    line.CodeFileLineIndex = lineNumber;
                    lineNumber++;

                    yield return line;
                }
            }
        }

        public string WashLine(string sourceCode)
        {
            var isInText = false;
            var builder = new StringBuilder();

            var codeLength = sourceCode.Length;
            char? lastChar = null;

            for (int index = 0; index < codeLength; index++)
            {
                var ch = sourceCode[index];

                // Remove leading and multiple white spaces
                if (char.IsWhiteSpace(ch) && (builder.Length == 0 || isInText == false && (lastChar.HasValue == false || char.IsWhiteSpace(lastChar.Value))))
                {
                    lastChar = ch;
                    continue;
                }

                // allways use single space
                if (char.IsWhiteSpace(ch) && isInText == false)
                {
                    ch = ' ';
                }

                if (ch == '\"')
                {
                    if (isInText == false)
                    {
                        isInText = true;

                        builder.Append(ch);
                    }
                    else
                    {
                        builder.Append(ch);

                        var endText = true;

                        if (index < codeLength - 1)
                        {
                            var nextChar = sourceCode[index + 1];

                            if (nextChar == '\"')
                            {
                                // Escaped double quotes
                                index++;
                                builder.Append(sourceCode[index]);

                                endText = false;
                            }
                        }

                        if (endText)
                        {
                            isInText = false;
                        }
                    }
                }
                else
                {
                    builder.Append(ch);
                }

                lastChar = ch;
            }

            return builder.ToString().TrimEnd();
        }

    }
}
