namespace CodeRedundancyCheck.Languages.CSharp
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using CodeRedundancyCheck.Common;
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
            var builder = new ThinList<char>(codeLength);
            var lastChar = char.MinValue;
            var lastCharIsWhiteSpace = false;

            const char NullChar = char.MinValue;

            for (var index = 0; index < codeLength; index++)
            {
                // var ch = char.ToLowerInvariant(sourceCode[index]);
                var ch = this.lowerLookup[sourceCode[index]];

                var isWhitespace = ch == ' ' || ch == '\t';

                // Remove leading and multiple white spaces
                if (isWhitespace && (builder.Length == 0 || (isInText == false && (lastChar == NullChar || lastCharIsWhiteSpace))))
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

                        builder.Add(ch);
                    }
                    else
                    {
                        builder.Add(ch);

                        var endText = true;

                        if (index < codeLength - 1)
                        {
                            var nextChar = sourceCode[index + 1];

                            if (nextChar == '\"')
                            {
                                // Escaped double quotes
                                index++;

                                // builder.Append(char.ToLowerInvariant(sourceCode[index]));
                                builder.Add(this.lowerLookup[sourceCode[index]]);

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
                    builder.Add(ch);
                }

                lastChar = ch;
                lastCharIsWhiteSpace = false;
            }

            var length = builder.length;

            // if last char was whitespace, ignore it
            if (lastCharIsWhiteSpace)
            {
                length--;

                if (length <= 0)
                {
                    return string.Empty;
                }
            }

            return new string(builder.array, 0, length);
        }

        private static bool IsWhiteSpace(char ch)
        {
            // return char.IsWhiteSpace(ch);
            return ch == ' ' || ch == '\t';
        }
    }
}