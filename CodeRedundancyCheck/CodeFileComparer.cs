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
            var matchesDone = new bool[codeFileArray.Length, codeFileArray.Length];

            var tasks = new Task[concurrencyLevel];

            for (int i = 0; i < concurrencyLevel; i++)
            {
                tasks[i] = Task.Run(() => this.GetMatchesAsync(minimumMatchingLines, codeFileQueue, codeFileArray, result, matchesDone));
            }

            await Task.WhenAll(tasks);

            return result.Values.SelectMany(v => v.CodeMatches).ToArray();
        }

        private void GetMatchesAsync(
            int minimumMatchingLines,
            ConcurrentQueue<CodeFile> codeFileQueue,
            CodeFile[] codeFileArray,
            ConcurrentDictionary<CodeMatchContainerKey, CodeMatchContainer> result,
            bool[,] filesCompared
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
                    var max = Math.Max(sourceFile.UniqueId, compareFile.UniqueId);
                    var min = Math.Min(sourceFile.UniqueId, compareFile.UniqueId);

                    if (filesCompared[min, max] == true)
                    {
                        continue;
                    }

                    filesCompared[min, max] = true;

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
                                var nextCompareLine = compareLine;
                                var nextSourceLine = allSourceLines[sourceFileLineIndex];

                                // Precheck to ensure enough matching lines to pass minimum
                                while (nextSourceLine.WashedLineHashCode == nextCompareLine.WashedLineHashCode)
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
                                    nextCompareLine = allCompareFileLines[compareFileLineIndexTemp];
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
                                var nextCompareLine = compareLine;

                                sourceLines.Clear();
                                compareLines.Clear();

                                while (nextSourceLine.WashedLineHashCode == nextCompareLine.WashedLineHashCode && string.Compare(nextSourceLine.WashedLineText, nextCompareLine.WashedLineText, StringComparison.Ordinal) == 0)
                                {
                                    sourceLines.Add(nextSourceLine);
                                    compareLines.Add(nextCompareLine);

                                    matchingLineCount++;

                                    sourceFileLineIndexTemp++;
                                    compareFileLineIndexTemp++;

                                    if (sourceFileLineIndexTemp >= lastSourceLine || compareFileLineIndexTemp >= allCompareFileLinesCount)
                                    {
                                        break;
                                    }

                                    nextSourceLine = allSourceLines[sourceFileLineIndexTemp];
                                    nextCompareLine = allCompareFileLines[compareFileLineIndexTemp];
                                }

                                if (matchingLineCount >= minimumMatchingLines)
                                {
                                    // Link to compare line block
                                    var addedSource = sourceLine.AddBlockWithResult(compareFile, compareLines, 0);

                                    // Link to source line block
                                    var addedCompare = compareLine.AddBlockWithResult(sourceFile, sourceLines, 0);

                                    if (addedSource.WasAdded || addedCompare.WasAdded)
                                    {
                                        // Remove  blocks that are part of the added block, "inner blocks"
                                        for (var i = 1; i < matchingLineCount; i++)
                                        {
                                            var innerSourceLine = sourceLines.Item(i);
                                            var innerCompareLine = compareLines.Item(i);

                                            // Link to compare line block
                                            var addedInnerSourceBlock = innerSourceLine.AddBlockWithResult(compareFile, innerCompareLine, matchingLineCount - i);

                                            // Link to source line block
                                            var addedInnerCompareBlock = innerCompareLine.AddBlockWithResult(sourceFile, innerSourceLine, matchingLineCount - i);
                                        }

                                        var matchKey = new CodeMatchContainerKey(sourceLine.WashedLineHashCode, sourceLine.WashedLineText, sourceLine.Next4MiniHash, matchingLineCount);

                                        var codeMatchContainer = result.GetOrAdd(
                                            matchKey,
                                            key =>
                                                {
                                                    // var lfs = lineSummaryResult;

                                                    var newContainer = new CodeMatchContainer(key);
                                                    return newContainer;
                                                });

                                        var sourceLinesCollection = sourceLines.AsCollection();
                                        var codeMatch = codeMatchContainer.GetOrAddCodeMatch(sourceLinesCollection);

                                        var sourceFileKey = new CodeFileMatchKey(sourceFile, sourceLines.Item(0).CodeFileLineIndex, matchingLineCount);
//                                        codeMatch.CodeFileMatches.GetOrAdd(sourceFileKey, key => new CodeFileMatch(sourceFile, sourceLines.Item(0).CodeFileLineIndex, new List<CodeLine>(sourceLinesCollection), addedCompare.BlockKey.FullHash));
                                        codeMatch.CodeFileMatches.GetOrAdd(sourceFileKey, key => new CodeFileMatch(sourceFile, sourceLines.Item(0).CodeFileLineIndex, new List<CodeLine>(sourceLinesCollection), addedCompare.BlockKey));

                                        var compareFileKey = new CodeFileMatchKey(compareFile, compareLines.Item(0).CodeFileLineIndex, matchingLineCount);
//                                        codeMatch.CodeFileMatches.GetOrAdd(compareFileKey, key => new CodeFileMatch(compareFile, compareLines.Item(0).CodeFileLineIndex, new List<CodeLine>(compareLines.AsCollection()), addedSource.BlockKey.FullHash));
                                        codeMatch.CodeFileMatches.GetOrAdd(compareFileKey, key => new CodeFileMatch(compareFile, compareLines.Item(0).CodeFileLineIndex, new List<CodeLine>(compareLines.AsCollection()), addedSource.BlockKey));
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}