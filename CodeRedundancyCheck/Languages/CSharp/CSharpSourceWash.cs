namespace CodeRedundancyCheck.Languages.CSharp
{
    using System.Collections.Generic;
    using System.Text;

    using CodeRedundancyCheck.Interface;
    using CodeRedundancyCheck.Model;

    public class CSharpSourceWash : ISourceWash
    {
        private readonly char[] lowerLookup;

        public CSharpSourceWash()
        {
            this.lowerLookup = new char[char.MaxValue];

            for (var i = char.MinValue; i < char.MaxValue; i++)
            {
                this.lowerLookup[i] = char.ToLowerInvariant(i);
            }
        }

        public IEnumerable<CodeLine> Wash(IEnumerable<CodeLine> lines)
        {
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

            var codeLength = sourceCode.Length;
            var builder = new StringBuilder(codeLength);
            var lastChar = char.MinValue;
            var lastCharIsWhiteSpace = false;

            var nullChar = char.MinValue;

            for (var index = 0; index < codeLength; index++)
            {
                // var ch = char.ToLowerInvariant(sourceCode[index]);
                var ch = this.lowerLookup[sourceCode[index]];

                var isWhitespace = ch == ' ' || ch == '\t';

                // Remove leading and multiple white spaces
                if (isWhitespace && (builder.Length == 0 || (isInText == false && (lastChar == nullChar || lastCharIsWhiteSpace))))
                {
                    lastChar = ch;
                    lastCharIsWhiteSpace = true;
                    continue;
                }

                // always use single space
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

                                // builder.Append(char.ToLowerInvariant(sourceCode[index]));
                                builder.Append(this.lowerLookup[sourceCode[index]]);

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

            var length = builder.Length;

            // if last char was whitespace, ignore it
            if (lastCharIsWhiteSpace)
            {
                length--;
            }

            return builder.ToString(0, length);
        }

        private static bool IsWhiteSpace(char ch)
        {
            // return char.IsWhiteSpace(ch);
            return ch == ' ' || ch == '\t';
        }
    }
}