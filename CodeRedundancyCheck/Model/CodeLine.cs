using System.Threading;

namespace CodeRedundancyCheck.Model
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CodeRedundancyCheck.Common;

    public class CodeLine
    {
        // public readonly ConcurrentDictionary<BlockKey, bool> Blocks = new ConcurrentDictionary<BlockKey, bool>();
        // public readonly ConcurrentDictionary<long, bool> Blocks = new ConcurrentDictionary<long, bool>();
        public readonly HashSet<long> Blocks = new HashSet<long>();

        private readonly object blockLock = new object();

        public int CodeFileLineIndex;

        public bool MayStartBlock = false;

        public int IsHandled = 0;

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

        private CodeLine()
        {
        }

        public bool IsBlockStart => this.Blocks.Count > 0;

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

        public CodeFile CodeFile;

        public static CodeLine CreateTargetLine(string targetLineText)
        {
            var line = new CodeLine
            {
                writableLine = targetLineText
            };

            return line;
        }

        public AddBlockResult AddBlockWithResult(CodeFile codeFile, ThinList<CodeLine> codeLines, int index)
        {
            var blockKey = GetBlockKey(codeFile.UniqueId, codeLines.Item(index), codeLines.Length - index);

            lock (this.blockLock)
            {
                var tryAddResult = this.Blocks.Add(blockKey);
                return new AddBlockResult(tryAddResult, blockKey);
            }

            // var blockKey = CreateBlockKey(codeFile, codeLines, index);
            // return new AddBlockResult(this.Blocks.TryAdd(blockKey, true), blockKey);
        }

        public AddBlockResult AddBlockWithResult(CodeFile codeFile, CodeLine codeLine, int numberOfLines)
        {
            var blockKey = GetBlockKey(codeFile.UniqueId, codeLine, numberOfLines);
            lock (this.blockLock)
            {
                var tryAddResult = this.Blocks.Add(blockKey);
                return new AddBlockResult(tryAddResult, blockKey);
            }

            // var blockKey = CreateBlockKey(codeFile, codeLine, index);
            // return new AddBlockResult(this.Blocks.TryAdd(blockKey, true), blockKey);
        }

        public override string ToString()
        {
            return this.OriginalLineNumber + ":" + this.WashedLineText;
        }

        private static BlockKey CreateBlockKey(CodeFile codeFile, CodeLine codeLine, int numberOfLines)
        {
            return new BlockKey(codeFile.UniqueId, codeLine.CodeFileLineIndex, numberOfLines);
        }

        private static BlockKey CreateBlockKey(CodeFile codeFile, ThinList<CodeLine> codeLines, int index)
        {
            return new BlockKey(codeFile.UniqueId, codeLines.Item(index).CodeFileLineIndex, codeLines.Length - index);
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

        // public bool AddBlock(CodeFile codeFile, ThinList<CodeLine> codeLines, int index)
        // {
        // return this.Blocks.TryAdd(GetBlockKey(codeFile.UniqueId, codeLines.Item(index), codeLines.Length - index), true);
        // //return this.Blocks.TryAdd(CreateBlockKey(codeFile, codeLines, index), true);
        // }
        public struct AddBlockResult
        {
            // public readonly BlockKey BlockKey;
            public readonly long BlockKey;

            public readonly bool WasAdded;

            // public AddBlockResult(bool wasAdded, BlockKey blockKey)
            public AddBlockResult(bool wasAdded, long blockKey)
            {
                this.WasAdded = wasAdded;
                this.BlockKey = blockKey;
            }
        }
    }
}