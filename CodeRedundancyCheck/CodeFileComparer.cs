using System.Threading;

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

        public Task<IReadOnlyCollection<CodeMatch>> GetMatchesAsync(int minimumMatchingLines, DivideAndConquerDictionary<ThinList<CodeLine>> masterDictionary, CodeFile firstCodeFile, params CodeFile[] codeFiles)
        {
            var files = new List<CodeFile>(codeFiles)
                        {
                            firstCodeFile
                        };

            return this.GetMatchesAsync(minimumMatchingLines, files, masterDictionary);
        }
        public async Task<IReadOnlyCollection<CodeMatch>> GetMatchesAsync(int minimumMatchingLines, IEnumerable<CodeFile> codeFiles, DivideAndConquerDictionary<ThinList<CodeLine>> masterDictionary, int concurrencyLevel = -1)
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
                tasks[i] = Task.Run(() => this.GetMatchesAsync(minimumMatchingLines, codeFileQueue, codeFileArray, result, masterDictionary));
            }

            await Task.WhenAll(tasks);

            return result.Values.SelectMany(v => v.CodeMatches).ToArray();
        }

        private void GetMatchesAsync(int minimumMatchingLines, ConcurrentQueue<CodeFile> codeFileQueue, CodeFile[] codeFileArray, ConcurrentDictionary<CodeMatchContainerKey, CodeMatchContainer> result, DivideAndConquerDictionary<ThinList<CodeLine>> masterDictionary)
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

                for (var sourceFileLineIndex = 0; sourceFileLineIndex < allSourceLinesCount; sourceFileLineIndex ++)
//                    for (var sourceFileLineIndex = firstStep; sourceFileLineIndex < allSourceLinesCount; sourceFileLineIndex += stepSize)
                {
                    var sourceLine = allSourceLines[sourceFileLineIndex];

                    if (sourceLine.MayStartBlock == false)
                    {
                        continue;
                    }

                    //if (Interlocked.CompareExchange(ref sourceLine.IsHandled, 1, 0) == 1)
                    //{
                    //    continue;
                    //}

                    ThinList<CodeLine> lineMatches;

                    if (masterDictionary.TryGetValue(sourceLine.WashedLineHashCode, out lineMatches) == false)
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

                        var compareFile = lineMatch.CodeFile;

                        if (compareFile == sourceFile)
                        {
                            if (lineMatchIndex <= sourceLine.CodeFileLineIndex)
                            {
                                continue;
                            }
                        }

                        var allCompareFileLines = compareFile.CodeLines;
                        var allCompareFileLinesCount = allCompareFileLines.Length;


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

                        var innerSourceFileLineIndexTemp = sourceFileLineIndex;

                        // If comparing to ourself, start checking one line below the current one
                        var innerCompareFileLineIndex = lineMatchIndex;

                        // Only compare to forward to the match we found or circular findings would occur
                        var innerlastSourceLine = compareFile == sourceFile ? lineMatchIndex : allSourceLinesCount;

                        var innermatchingLineCount = 0;

                        var innercompareFileLineIndexTemp = innerCompareFileLineIndex;

                        var innercompareLine = allCompareFileLines[innercompareFileLineIndexTemp];

                        var innernextSourceLine = allSourceLines[sourceFileLineIndex];
                        var innernextCompareLine = innercompareLine;

                        sourceLines.Clear();
                        compareLines.Clear();

                        while (innernextSourceLine.WashedLineHashCode == innernextCompareLine.WashedLineHashCode && string.Compare(innernextSourceLine.WashedLineText, innernextCompareLine.WashedLineText, StringComparison.Ordinal) == 0)
                        {
                            sourceLines.Add(innernextSourceLine);
                            compareLines.Add(innernextCompareLine);

                            innermatchingLineCount++;

                            innerSourceFileLineIndexTemp++;
                            innercompareFileLineIndexTemp++;

                            if (innerSourceFileLineIndexTemp >= innerlastSourceLine || innercompareFileLineIndexTemp >= allCompareFileLinesCount)
                            {
                                break;
                            }

                            innernextSourceLine = allSourceLines[innerSourceFileLineIndexTemp];
                            innernextCompareLine = allCompareFileLines[innercompareFileLineIndexTemp];
                        }

                        if (innermatchingLineCount >= minimumMatchingLines)
                        {
                            // Link to compare line block
                            var addedSource = sourceLine.AddBlockWithResult(compareFile, compareLines, 0);

                            // Link to source line block
                            var addedCompare = innercompareLine.AddBlockWithResult(sourceFile, sourceLines, 0);

                            if (addedSource.WasAdded || addedCompare.WasAdded)
                            {
                                // Remove  blocks that are part of the added block, "inner blocks"
                                for (var i = 1; i < innermatchingLineCount; i++)
                                {
                                    var innerSourceLine = sourceLines.Item(i);
                                    var innerCompareLine = compareLines.Item(i);

                                    // Link to compare line block
                                    var addedInnerSourceBlock = innerSourceLine.AddBlockWithResult(compareFile, innerCompareLine, innermatchingLineCount - i);

                                    // Link to source line block
                                    var addedInnerCompareBlock = innerCompareLine.AddBlockWithResult(sourceFile, innerSourceLine, innermatchingLineCount - i);
                                }

                                var matchKey = new CodeMatchContainerKey(sourceLine.WashedLineHashCode, sourceLine.WashedLineText, sourceLine.Next4MiniHash, innermatchingLineCount);

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

                                var sourceFileKey = new CodeFileMatchKey(sourceFile, sourceLines.Item(0).CodeFileLineIndex, innermatchingLineCount);
                                //                                        codeMatch.CodeFileMatches.GetOrAdd(sourceFileKey, key => new CodeFileMatch(sourceFile, sourceLines.Item(0).CodeFileLineIndex, new List<CodeLine>(sourceLinesCollection), addedCompare.BlockKey.FullHash));

                                var sourceFileMatch = new CodeFileMatch(sourceFile, sourceLines.Item(0).CodeFileLineIndex, new List<CodeLine>(sourceLinesCollection), addedCompare.BlockKey);

                                codeMatch.CodeFileMatches.GetOrAdd(sourceFileKey, sourceFileMatch);

                                var compareFileMatch = new CodeFileMatch(compareFile, compareLines.Item(0).CodeFileLineIndex, new List<CodeLine>(compareLines.AsCollection()), addedSource.BlockKey);

                                var compareFileKey = new CodeFileMatchKey(compareFile, compareLines.Item(0).CodeFileLineIndex, innermatchingLineCount);
                                //                                        codeMatch.CodeFileMatches.GetOrAdd(compareFileKey, key => new CodeFileMatch(compareFile, compareLines.Item(0).CodeFileLineIndex, new List<CodeLine>(compareLines.AsCollection()), addedSource.BlockKey.FullHash));
                                codeMatch.CodeFileMatches.GetOrAdd(compareFileKey, compareFileMatch);
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