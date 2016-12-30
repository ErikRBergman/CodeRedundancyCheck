namespace CodeRedundancyCheck
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CodeRedundancyCheck.Common;
    using CodeRedundancyCheck.Extensions;
    using CodeRedundancyCheck.Interface;
    using CodeRedundancyCheck.Model;

    public class CodeFileComparer
    {
        const int MaxNumberOfLinesInBlock = 100000;

        public ICodeLineFilter CodeLineFilter { get; set; }

        public Task<IReadOnlyCollection<CodeMatch>> GetMatchesAsync(int minimumMatchingLines, CodeFile firstCodeFile, params CodeFile[] codeFiles)
        {
            var files = new List<CodeFile>(codeFiles)
                        {
                            firstCodeFile
                        };

            return this.GetMatchesAsync(minimumMatchingLines, files);
        }

        public async Task<IReadOnlyCollection<CodeMatch>> GetMatchesAsync(int minimumMatchingLines, IEnumerable<CodeFile> codeFiles, int concurrencyLevel = -1)
        {
            var codeFileArray = codeFiles as CodeFile[] ?? codeFiles.ToArray();

            for (var i = 0; i < codeFileArray.Length; i++)
            {
                codeFileArray[i].UniqueId = i;
            }

            var codeFileQueue = new ConcurrentQueue<CodeFile>(codeFileArray);
            var result = new ConcurrentDictionary<long, CodeMatch>(Environment.ProcessorCount * 4, 50000);

            if (concurrencyLevel < 1)
            {
                concurrencyLevel = Environment.ProcessorCount * 4;
            }

            var tasks = new Task[concurrencyLevel];

            for (int i = 0; i < concurrencyLevel; i++)
            {
                tasks[i] = Task.Run(() => this.GetMatchesAsync(minimumMatchingLines, codeFileQueue, codeFileArray, result));
            }

            await Task.WhenAll(tasks);

            return result.Values.ToArray();
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

        private void GetMatchesAsync(int minimumMatchingLines, ConcurrentQueue<CodeFile> codeFileQueue, CodeFile[] codeFileArray, ConcurrentDictionary<long, CodeMatch> result)
        {
            var sourceLines = new ThinList<CodeLine>(MaxNumberOfLinesInBlock);
            var compareLines = new ThinList<CodeLine>(MaxNumberOfLinesInBlock);

            var addedBlocks = new HashSet<long>();

            var filter = this.CodeLineFilter;

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
                                sourceLine = allSourceLines[sourceFileLineIndex];

                                // Precheck to ensure enough matching lines to pass minimum
                                while (sourceLine.WashedLineHashCode == compareLine.WashedLineHashCode)
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

                                    sourceLine = allSourceLines[sourceFileLineIndexTemp];
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

                                sourceLine = allSourceLines[sourceFileLineIndex];

                                sourceLines.Clear();
                                compareLines.Clear();

                                while (sourceLine.WashedLineHashCode == compareLine.WashedLineHashCode && string.Compare(sourceLine.WashedLineText, compareLine.WashedLineText, StringComparison.Ordinal) == 0)
                                {
                                    sourceLines.Add(sourceLine);
                                    compareLines.Add(compareLine);

                                    matchingLineCount++;

                                    sourceFileLineIndexTemp++;
                                    compareFileLineIndexTemp++;

                                    if (sourceFileLineIndexTemp >= lastSourceLine || compareFileLineIndexTemp >= allCompareFileLinesCount)
                                    {
                                        break;
                                    }

                                    sourceLine = allSourceLines[sourceFileLineIndexTemp];
                                    compareLine = allCompareFileLines[compareFileLineIndexTemp];
                                }

                                if (matchingLineCount >= minimumMatchingLines)
                                {
                                    var compareLinesKey = GetBlockKey(compareFile.UniqueId, compareLines, matchingLineCount);
                                    var sourceLinesKey = GetBlockKey(sourceFile.UniqueId, sourceLines, matchingLineCount);

                                    if (addedBlocks.DoesNotContainAll(compareLinesKey, sourceLinesKey))
                                    {
                                        addedBlocks.AddMultiple(sourceLinesKey, compareLinesKey);

                                        //// Remove target blocks that are part of the added block
                                        for (var i = 1; i < matchingLineCount; i++)
                                        {
                                            var compareLinesKeyTemp = GetBlockKey(compareFile.UniqueId, compareLines.Item(0).CodeFileLineIndex + i, matchingLineCount - i);
                                            var sourceLinesKeyTemp = GetBlockKey(sourceFile.UniqueId, sourceLines.Item(0).CodeFileLineIndex + i, matchingLineCount - i);
                                            addedBlocks.AddMultiple(compareLinesKeyTemp, sourceLinesKeyTemp);
                                        }

                                        var match = result.GetOrAdd(
                                            sourceLinesKey,
                                            key =>
                                                {
                                                    var cmatch = new CodeMatch
                                                    {
                                                        CodeFileMatches = new ConcurrentBag<CodeFileMatch>(),
                                                        Lines = matchingLineCount
                                                    };

                                                    cmatch.CodeFileMatches.Add(new CodeFileMatch(sourceFile, sourceLines.Item(0).CodeFileLineIndex, new List<CodeLine>(sourceLines.AsCollection())));
                                                    result.TryAdd(sourceLinesKey, cmatch);

                                                    return cmatch;
                                                });

                                        match.CodeFileMatches.Add(new CodeFileMatch(compareFile, compareLines.Item(0).CodeFileLineIndex, new List<CodeLine>(compareLines.AsCollection())));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static uint GetMinihashComparePattern(int minimumMatchingLines)
        {
            if (minimumMatchingLines >= 4)
            {
                return 0xFFFFFFFF;
            }

            if (minimumMatchingLines == 3)
            {
                return 0x0000FFFF;
            }

            if (minimumMatchingLines == 2)
            {
                return 0x000000FF;
            }

            return 0x0;
        }
    }
}