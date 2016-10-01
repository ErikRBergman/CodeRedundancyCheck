//// This is just an experiment

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Net.NetworkInformation;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace CodeRedundancyCheck
//{
//    public class VisualBasicWash
//    {
//        private static readonly Regex manualLineBreakRegex = new Regex("_\\s*$", RegexOptions.Compiled);
//        private static readonly Regex whiteSpacesRegex = new Regex("\\s*", RegexOptions.Compiled | RegexOptions.Multiline);
        
//        public List<CodeBlock> SplitIntoExpressions(string sourceCode)
//        {
//            var result = new List<CodeBlock>();

//            var isInText = false;
//            var builder = new Builder();

//            var codeLength = sourceCode.Length;

//            var currentLine = 1;
//            var currentPosition = 0;

//            for (int index = 0; index < codeLength; index++)
//            {
//                var ch = sourceCode[index];

//                currentPosition++;

//                if (ch == '\r' && index < codeLength - 1)
//                {
//                    var nextChar = sourceCode[index + 1];

//                    if (nextChar == '\n')
//                    {
//                        currentLine++;
//                        currentPosition = 0;
//                    }
//                }
                
//                if (char.IsWhiteSpace(ch) && isInText == false)
//                {
//                    if (builder.Length > 0)
//                    {
//                        result.Add(new CodeBlock(BlockType.Code, builder.ToString(), builder.Line, builder.Position));
//                        builder.Clear();
//                    }

//                    continue;
//                }

//                if (ch == '\"')
//                {
//                    if (isInText == false)
//                    {
//                        if (builder.Length > 0)
//                        {
//                            result.Add(new CodeBlock(BlockType.Code, builder.ToString(), builder.Line, builder.Position));
//                            builder.Clear();
//                        }

//                        isInText = true;

//                        builder.Append(ch, currentLine, currentPosition);
//                    }
//                    else
//                    {
//                        builder.Append(ch, currentLine, currentPosition);

//                        var endText = true;

//                        if (index < codeLength - 1)
//                        {
//                            var nextChar = sourceCode[index + 1];

//                            if (nextChar == '\"')
//                            {
//                                // Escaped double quotes
//                                index++;
//                                builder.Append(sourceCode[index], builder.Line, builder.Position);

//                                endText = false;
//                            }
//                        }

//                        if (endText)
//                        {
//                            result.Add(new CodeBlock(BlockType.Code, builder.ToString(), builder.Line, builder.Position));
//                            builder.Clear();

//                            isInText = false;
//                        }
//                    }
//                }
//                else
//                {
//                    builder.Append(ch, currentLine, currentPosition);
//                }
//            }

//            if (builder.Length > 0)
//            {
//                result.Add(new CodeBlock(BlockType.Code, builder.ToString(), builder.Line, builder.Position));
//            }

//            return result;
//        }

        
//        public enum BlockType
//        {
//            Code,
//            Text
//        }

//        public struct CodeBlock
//        {
//            public CodeBlock(BlockType blockType, string text, int line, int position)
//            {
//                this.BlockType = blockType;
//                this.Text = text;
//                this.Line = line;
//                this.Position = position;
//            }

//            public BlockType BlockType { get; set; }

//            public string Text { get; set; }

//            public int Line { get; set; }
//            public int Position { get; set; }
//        }


//        private class Builder
//        {
//            private readonly StringBuilder builder = new StringBuilder();

//            public int Line { get; private set; } = -1;
//            public int Position { get; private set; } = -1;

//            public override string ToString()
//            {
//                return this.builder.ToString();
//            }

//            public void Append(char ch, int line, int position)
//            {
//                if (this.builder.Length == 0)
//                {
//                    this.Line = line;
//                    this.Position = position;
//                }

//                this.builder.Append(ch);
//            }

//            public void Clear()
//            {
//                this.Line = -1;
//                this.Position = -1;
//                this.builder.Clear();
//            }

//            public int Length => this.builder.Length;

//        }

//    }


//}
