using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeRedundancyCheck.Extensions;
using CodeRedundancyCheck.Interface;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    using CodeRedundancyCheck.Common;

    public class CodeFileComparer
    {
        public ICodeLineFilter CodeLineFilter { get; set; }

        public Task<List<CodeMatch>> GetMatchesAsync(int minimumMatchingLines, CodeFile firstCodeFile, params CodeFile[] codeFiles)
        {
            var files = new List<CodeFile>(codeFiles)
            {
                firstCodeFile
            };

            return this.GetMatchesAsync(minimumMatchingLines, files);
        }

        public Task<List<CodeMatch>> GetMatchesAsync(int minimumMatchingLines, IEnumerable<CodeFile> codeFiles)
        {
            var codeFileList = codeFiles as CodeFile[] ?? codeFiles.ToArray();

            for (int i = 0; i < codeFileList.Length; i++)
            {
                codeFileList[i].UniqueId = i;
            }

            var comparables = new HashSet<CodeFile>(codeFileList);

            if (comparables.Count < 1)
            {
                throw new Exception("At least one files must be matched (with itself)");
            }

            var codeFileQueue = new Queue<CodeFile>(codeFileList);

            var result = new Dictionary<long, CodeMatch>(50000);

            // Maximum 50k lines per code file
            var sourceLines = new ThinList<CodeLine>(50000);
            var compareLines = new ThinList<CodeLine>(50000);

            var addedBlocks = new HashSet<long>();

            var filter = this.CodeLineFilter;

            while (codeFileQueue.Count > 0)
            {
                var sourceFile = codeFileQueue.Dequeue();

                var allSourceLines = sourceFile.CodeLines;
                var allSourceLinesCount = sourceFile.CodeLines.Length;

                foreach (var compareFile in comparables)
                {
                    var allCompareFileLines = compareFile.CodeLines;
                    var allCompareFileLinesCount = allCompareFileLines.Length;
                    var compareFileCodeLinesDictionary = compareFile.CodeLinesDictionary;

                    for (var sourceFileLineIndex = 0; sourceFileLineIndex < allSourceLinesCount; sourceFileLineIndex++)
                    {
                        var sourceLine = allSourceLines[sourceFileLineIndex];

                        List<CodeLine> lineMatches;

                        if (compareFileCodeLinesDictionary.TryGetValue(sourceLine.WashedLineHashCode, out lineMatches) == false)
                        {
                            continue;
                        }

                        if (filter.MayStartBlock(sourceLine, sourceFile) == false)
                        {
                            continue;
                        }

                        foreach (var lineMatch in lineMatches)
                        {
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
                                int compareFileLineIndex = lineMatchIndex;

                                // Only compare to forward to the match we found or circular findings would occur
                                var lastSourceLine = compareFile == sourceFile ? lineMatchIndex : allSourceLinesCount;

                                int matchingLineCount = 0;

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
                                int compareFileLineIndex = lineMatchIndex;

                                // Only compare to forward to the match we found or circular findings would occur
                                var lastSourceLine = compareFile == sourceFile ? lineMatchIndex : allSourceLinesCount;

                                int matchingLineCount = 0;

                                var compareFileLineIndexTemp = compareFileLineIndex;

                                var compareLine = allCompareFileLines[compareFileLineIndexTemp];

                                sourceLine = allSourceLines[sourceFileLineIndex];


                                sourceLines.Clear();
                                compareLines.Clear();

                                while (sourceLine.WashedLineHashCode == compareLine.WashedLineHashCode && string.Compare(sourceLine.WashedLineText, compareLine.WashedLineText, StringComparison.OrdinalIgnoreCase) == 0)
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
                                        for (int i = 1; i < matchingLineCount; i++)
                                        {
                                            var compareLinesKeyTemp = GetBlockKey(compareFile.UniqueId, compareLines[0].CodeFileLineIndex + i, matchingLineCount - i);
                                            var sourceLinesKeyTemp = GetBlockKey(sourceFile.UniqueId, sourceLines[0].CodeFileLineIndex + i, matchingLineCount - i);
                                            addedBlocks.AddMultiple(compareLinesKeyTemp, sourceLinesKeyTemp);
                                        }

                                        CodeMatch match;
                                        if (result.TryGetValue(sourceLinesKey, out match) == false)
                                        {
                                            match = new CodeMatch
                                            {
                                                CodeFileMatches = new List<CodeFileMatch>(),
                                                Lines = matchingLineCount
                                            };

                                            match.CodeFileMatches.Add(new CodeFileMatch(sourceFile, sourceLines[0].CodeFileLineIndex, new List<CodeLine>(sourceLines.AsCollection())));
                                            result.Add(sourceLinesKey, match);
                                        }

                                        match.CodeFileMatches.Add(new CodeFileMatch(compareFile, compareLines[0].CodeFileLineIndex, new List<CodeLine>(compareLines.AsCollection())));
                                    }
                                }
                            }

                        }
                    }
                }
            }

            return Task.FromResult(result.Values.ToList());
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

            //            return uniqueId + "_" + codeFileLineIndex + "_" + matchingLineCount;
        }
        
        public IEnumerable<CodeLine> LoadFileData(string filename)
        {
            return File.ReadAllLines(filename).Select((text, lineNumber) =>
            new CodeLine(
                originalLineText: text,
                originalLineNumber: lineNumber + 1,
                originalLinePosition: 0));
        }


    }
}
