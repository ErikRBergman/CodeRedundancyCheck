namespace CodeRedundancyCheck
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CodeRedundancyCheck.Common;
    using CodeRedundancyCheck.Extensions;
    using CodeRedundancyCheck.Model;

    public class CodeFileComparer
    {
        public const int MaxNumberOfLinesInBlock = 100000;

        public Task<IReadOnlyCollection<CodeMatch>> GetMatchesAsync(int minimumMatchingLines, CodeFile firstCodeFile, params CodeFile[] codeFiles)
        {
            var files = new List<CodeFile>(codeFiles)
                        {
                            firstCodeFile
                        };

            return this.GetMatchesAsync(minimumMatchingLines, files);
        }

        private struct InnerBlockKey
        {
            public readonly long Key1;
            public readonly long Key2;

            private readonly int hashCode;

            public InnerBlockKey(long key1, long key2)
            {
                this.Key1 = key1;
                this.Key2 = key2;

                this.hashCode = (int)(key1 + key2);
            }

            public override int GetHashCode() => this.hashCode;

            public override bool Equals(object obj)
            {
                var other = (InnerBlockKey)obj;
                return (other.Key1 == this.Key1 && other.Key2 == this.Key2) ||
                    (other.Key1 == this.Key2 && other.Key2 == this.Key1);
            }
        }

        public async Task<IReadOnlyCollection<CodeMatch>> GetMatchesAsync(
            int minimumMatchingLines,
            IEnumerable<CodeFile> codeFiles,
            int concurrencyLevel = -1)
        {
            var codeFileArray = codeFiles as CodeFile[] ?? codeFiles.ToArray();

            for (var i = 0; i < codeFileArray.Length; i++)
            {
                codeFileArray[i].UniqueId = i;
            }

            var codeFileQueue = new ConcurrentQueue<CodeFile>(codeFileArray);
            if (concurrencyLevel < 1)
            {
                concurrencyLevel = Environment.ProcessorCount * 4;
            }

            var result = new ConcurrentDictionary<CodeMatchContainerKey, CodeMatchContainer>(concurrencyLevel, 50000);
            var innerBlockFilter = new ConcurrentDictionary<InnerBlockKey, bool>(concurrencyLevel, 50000);
            var blockFilter = new ConcurrentDictionary<long, bool>(concurrencyLevel, 50000);

            var tasks = new Task[concurrencyLevel];

            for (int i = 0; i < concurrencyLevel; i++)
            {
                tasks[i] = Task.Run(() => this.GetMatchesAsync(minimumMatchingLines, codeFileQueue, codeFileArray, result, innerBlockFilter, blockFilter));
            }

            await Task.WhenAll(tasks);

            return result.Values.SelectMany(v => v.CodeMatches).ToArray();

            return result.Values.SelectMany(v => v.CodeMatches).ToArray();

            ///return result.Values.SelectMany(v => v.CodeMatches).ToArray();
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
            return (uniqueId << 32) + (codeFileLineIndex << 16) + matchingLineCount;
        }

        private void GetMatchesAsync(
            int minimumMatchingLines,
            ConcurrentQueue<CodeFile> codeFileQueue,
            CodeFile[] codeFileArray,
            ConcurrentDictionary<CodeMatchContainerKey, CodeMatchContainer> result,
            ConcurrentDictionary<InnerBlockKey, bool> innerBlockFilter,
            ConcurrentDictionary<long, bool> blockFilter
            )
        {
            var sourceLines = new ThinList<CodeLine>(MaxNumberOfLinesInBlock);
            var compareLines = new ThinList<CodeLine>(MaxNumberOfLinesInBlock);

            while (codeFileQueue.Count > 0)
            {
                CodeFile sourceFile;

                if (!codeFileQueue.TryDequeue(out sourceFile))
                {
                    break;
                }

                var allSourceLines = sourceFile.CodeLines;
                var allSourceLinesCount = allSourceLines.Length;

                foreach (var compareFile in codeFileArray)
                {
                    // avoid duplicate work as much as possible
                    //if (compareFile.IsDone)
                    //{
                    //    continue;
                    //}

                    var allCompareFileLines = compareFile.CodeLines;
                    var allCompareFileLinesCount = allCompareFileLines.Length;
                    var compareFileCodeLinesDictionary = compareFile.CodeLinesDictionary;

                    for (var sourceFileLineIndex = 0; sourceFileLineIndex < allSourceLinesCount; sourceFileLineIndex++)
                    {
                        var sourceLine = allSourceLines[sourceFileLineIndex];

                        if (sourceLine.MayStartBlock == false)
                        {
                            continue;
                        }

                        ThinList<CodeLine> lineMatches;

                        if (compareFileCodeLinesDictionary.TryGetValue(sourceLine.WashedLineHashCode, out lineMatches) == false)
                        {
                            continue;
                        }

                        var lineArrayMatches = lineMatches.array;
                        var lineMatchesCount = lineMatches.length;

                        uint sourceLineNext4MiniHash = sourceLine.Next4MiniHash;

                        for (var index = 0; index < lineMatchesCount; index++)
                        {
                            var lineMatch = lineArrayMatches[index];

                            if (sourceLineNext4MiniHash != lineMatch.Next4MiniHash)
                            {
                                continue;
                            }

                            var lineMatchIndex = lineMatch.CodeFileLineIndex;

                            if (compareFile == sourceFile)
                            {
                                if (lineMatchIndex <= sourceFileLineIndex)
                                {
                                    continue;
                                }
                            }

                            {
                                var sourceFileLineIndexTemp = sourceFileLineIndex;

                                // If comparing to ourself, start checking one line below the current one
                                var compareFileLineIndex = lineMatchIndex;

                                // Only compare to forward to the match we found or circular findings would occur
                                var lastSourceLine = compareFile == sourceFile ? lineMatchIndex : allSourceLinesCount;

                                var matchingLineCount = 0;

                                var compareFileLineIndexTemp = compareFileLineIndex;

                                var compareLine = allCompareFileLines[compareFileLineIndexTemp];
                                var nextSourceLine = allSourceLines[sourceFileLineIndex];

                                // Precheck to ensure enough matching lines to pass minimum
                                while (nextSourceLine.WashedLineHashCode == compareLine.WashedLineHashCode)
                                {
                                    matchingLineCount++;

                                    if (matchingLineCount >= minimumMatchingLines)
                                    {
                                        break;
                                    }

                                    sourceFileLineIndexTemp++;
                                    compareFileLineIndexTemp++;

                                    if (sourceFileLineIndexTemp >= lastSourceLine || compareFileLineIndexTemp >= allCompareFileLinesCount)
                                    {
                                        break;
                                    }

                                    nextSourceLine = allSourceLines[sourceFileLineIndexTemp];
                                    compareLine = allCompareFileLines[compareFileLineIndexTemp];
                                }

                                if (matchingLineCount < minimumMatchingLines)
                                {
                                    continue;
                                }
                            }

                            {
                                var sourceFileLineIndexTemp = sourceFileLineIndex;

                                // If comparing to ourself, start checking one line below the current one
                                var compareFileLineIndex = lineMatchIndex;

                                // Only compare to forward to the match we found or circular findings would occur
                                var lastSourceLine = compareFile == sourceFile ? lineMatchIndex : allSourceLinesCount;

                                var matchingLineCount = 0;

                                var compareFileLineIndexTemp = compareFileLineIndex;

                                var compareLine = allCompareFileLines[compareFileLineIndexTemp];

                                var nextSourceLine = allSourceLines[sourceFileLineIndex];

                                sourceLines.Clear();
                                compareLines.Clear();

                                while (nextSourceLine.WashedLineHashCode == compareLine.WashedLineHashCode && string.Compare(nextSourceLine.WashedLineText, compareLine.WashedLineText, StringComparison.Ordinal) == 0)
                                {
                                    sourceLines.Add(nextSourceLine);
                                    compareLines.Add(compareLine);

                                    matchingLineCount++;

                                    sourceFileLineIndexTemp++;
                                    compareFileLineIndexTemp++;

                                    if (sourceFileLineIndexTemp >= lastSourceLine || compareFileLineIndexTemp >= allCompareFileLinesCount)
                                    {
                                        break;
                                    }

                                    nextSourceLine = allSourceLines[sourceFileLineIndexTemp];
                                    compareLine = allCompareFileLines[compareFileLineIndexTemp];
                                }

                                if (matchingLineCount >= minimumMatchingLines)
                                {
                                    var sourceLinesKey = GetBlockKey(sourceFile.UniqueId, sourceLines, matchingLineCount);
                                    var compareLinesKey = GetBlockKey(compareFile.UniqueId, compareLines, matchingLineCount);

                                    var blockKey = new InnerBlockKey(sourceLinesKey, compareLinesKey);

                                    if (innerBlockFilter.TryAdd(blockKey, true))
                                    {


                                        //                                    if (innerBlockFilter.TryAdd(blockKey, true) && blockFilter.DoesNotContainAll())

                                        var addedSource = blockFilter.TryAdd(sourceLinesKey, true);
                                        var addedCompare = blockFilter.TryAdd(sourceLinesKey, true);

                                        if (addedSource || addedCompare)
                                        {
                                            // Remove  blocks that are part of the added block, "inner blocks"
                                            for (var i = 1; i < matchingLineCount; i++)
                                            {
                                                var innerSourceLinesKey = GetBlockKey(sourceFile.UniqueId, sourceLines.Item(0).CodeFileLineIndex + i, matchingLineCount - i);
                                                var innerCompareLinesKey = GetBlockKey(compareFile.UniqueId, compareLines.Item(0).CodeFileLineIndex + i, matchingLineCount - i);

                                                blockFilter.TryAddMultiple(false, innerSourceLinesKey, innerCompareLinesKey);

                                                //var innerBlockKey = new InnerBlockKey(innerSourceLinesKey, innerCompareLinesKey);
                                                //innerBlockFilter.TryAdd(innerBlockKey, false);


                                            }

                                            var matchKey = new CodeMatchContainerKey(sourceLine.WashedLineHashCode, sourceLine.WashedLineText, sourceLine.Next4MiniHash, matchingLineCount);

                                            var codeMatchContainer = result.GetOrAdd(
                                                matchKey,
                                                key =>
                                                    {
                                                        var newContainer = new CodeMatchContainer(key);
                                                        return newContainer;
                                                    });

                                            var sourceLinesCollection = sourceLines.AsCollection();
                                            var codeMatch = codeMatchContainer.GetOrAddCodeMatch(sourceLinesCollection);

                                            var sourceFileKey = new CodeFileMatchKey(sourceFile, sourceLines.Item(0).CodeFileLineIndex, matchingLineCount);
                                            codeMatch.CodeFileMatches.GetOrAdd(sourceFileKey, key => new CodeFileMatch(sourceFile, sourceLines.Item(0).CodeFileLineIndex, new List<CodeLine>(sourceLinesCollection), sourceLinesKey));

                                            var compareFileKey = new CodeFileMatchKey(compareFile, compareLines.Item(0).CodeFileLineIndex, matchingLineCount);
                                            codeMatch.CodeFileMatches.GetOrAdd(compareFileKey, key => new CodeFileMatch(compareFile, compareLines.Item(0).CodeFileLineIndex, new List<CodeLine>(compareLines.AsCollection()), compareLinesKey));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                sourceFile.IsDone = true;
            }
        }
    }
}