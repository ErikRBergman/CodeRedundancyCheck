namespace CodeRedundancyCheck.Model
{
    using System.Collections.Concurrent;

    using CodeRedundancyCheck.Common;

    public struct BlockKey
    {
        public readonly int FileUniqueId;

        public readonly int LineIndex;

        public readonly int NumberOfLines;

        public BlockKey(int fileUniqueId, int lineIndex, int numberOfLines)
        {
            this.FileUniqueId = fileUniqueId;
            this.LineIndex = lineIndex;
            this.NumberOfLines = numberOfLines;
        }
    }

    public class CodeLine
    {
        public readonly ConcurrentDictionary<long, bool> Blocks = new ConcurrentDictionary<long, bool>();

        public int CodeFileLineIndex;

        public bool IsBlockStart => this.Blocks.Count > 0;

        public bool MayStartBlock = false;

        public uint Next4MiniHash;

        public int WashedLineHashCode;

        public string WashedLineText;

        private string writableLine;

        public CodeLine(string originalLineText, int originalLineNumber, int originalLinePosition)
        {
            this.OriginalLineText = originalLineText;
            this.OriginalLineNumber = originalLineNumber;
            this.OriginalLinePosition = originalLinePosition;
        }

        public bool AddBlock(long key)
        {
            return this.Blocks.TryAdd(key, true);
        }

        public bool AddBlock(CodeFile codeFile, CodeLine codeLine, int numberOfLines)
        {
            return this.Blocks.TryAdd(GetBlockKey(codeFile.UniqueId, codeLine, numberOfLines), true);
        }

        public bool AddBlock(CodeFile codeFile, ThinList<CodeLine> codeLines, int index)
        {
            return this.Blocks.TryAdd(GetBlockKey(codeFile.UniqueId, codeLines.Item(index), codeLines.Length), true);
        }

        public struct AddBlockResult
        {
            public readonly bool WasAdded;

            public readonly long BlockKey;

            public AddBlockResult(bool wasAdded, long blockKey)
            {
                this.WasAdded = wasAdded;
                this.BlockKey = blockKey;
            }
        }

        public AddBlockResult AddBlockWithResult(CodeFile codeFile, ThinList<CodeLine> codeLines, int index)
        {
            var blockKey = GetBlockKey(codeFile.UniqueId, codeLines.Item(index), codeLines.Length);

            return new AddBlockResult(this.Blocks.TryAdd(blockKey, true), blockKey);
        }


        private CodeLine()
        {
        }

        public bool IsCodeLine { get; set; } = true;

        public int OriginalLineNumber { get; set; }

        public int OriginalLinePosition { get; set; }

        public string OriginalLineText { get; set; }

        public string WriteableLine
        {
            get
            {
                return this.writableLine ?? this.OriginalLineText;
            }

            set
            {
                this.writableLine = value;
            }
        }

        public static CodeLine CreateTargetLine(string targetLineText)
        {
            var line = new CodeLine
                       {
                           writableLine = targetLineText
                       };

            return line;
        }

        public override string ToString()
        {
            return this.OriginalLineNumber + ":" + this.WashedLineText;
        }

        private static long GetBlockKey(int uniqueId, CodeLine[] codeLines, int matchingLineCount)
        {
            return GetBlockKey(uniqueId, codeLines[0], matchingLineCount);
        }

        private static long GetBlockKey(int uniqueId, CodeLine codeLine, int matchingLineCount)
        {
            return GetBlockKey(uniqueId, codeLine.CodeFileLineIndex, matchingLineCount);
        }

        private static long GetBlockKey(int uniqueId, int codeFileLineIndex, int matchingLineCount)
        {
            return (((long)uniqueId) << 32) + (codeFileLineIndex << 16) + matchingLineCount;
        }

    }
}