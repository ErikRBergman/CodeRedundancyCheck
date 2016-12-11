namespace CodeRedundancyCheck.Languages.CSharp
{
    using System.Collections.Generic;
    using System.Text;

    using Interface;
    using Model;

    public class CSharpSourceWash : ISourceWash
    {
        public static ISourceWash Singleton { get; } = new VisualBasicSourceWash();

        public IEnumerable<CodeLine> Wash(IEnumerable<CodeLine> lines)
        {
            int lineNumber = 0;

            foreach (var line in lines)
            {
                line.WashedLineText = this.WashLine(line.OriginalLineText);

                if (line.WashedLineText.Length == 0 || line.WashedLineText.StartsWith("//"))
                {
                    line.IsCodeLine = false;
                    line.CodeFileLineIndex = -1;
                }

                yield return line;
            }
        }

        public string WashLine(string sourceCode)
        {
            var isInText = false;
            var builder = new StringBuilder();

            var codeLength = sourceCode.Length;
            char? lastChar = null;
            bool lastCharIsWhiteSpace = false;

            for (int index = 0; index < codeLength; index++)
            {
                var ch = sourceCode[index];

                bool isWhitespace = char.IsWhiteSpace(ch);

                // Remove leading and multiple white spaces
                if (isWhitespace && (builder.Length == 0 || isInText == false && (lastChar.HasValue == false || lastCharIsWhiteSpace)))
                {
                    lastChar = ch;
                    lastCharIsWhiteSpace = true;
                    continue;
                }

                // allways use single space
                if (isWhitespace && isInText == false)
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
                lastCharIsWhiteSpace = false;
            }

            return builder.ToString().TrimEnd();
        }

    }
}
