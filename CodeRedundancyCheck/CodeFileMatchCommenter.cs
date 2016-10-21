using System.Linq;
using CodeRedundancyCheck.Extensions;
using CodeRedundancyCheck.Interface;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    public class CodeFileMatchCommenter
    {
        private readonly ICodeFileLineIndexer codeFileLineIndexer;

        public CodeFileMatchCommenter(ICodeFileLineIndexer codeFileLineIndexer)
        {
            this.codeFileLineIndexer = codeFileLineIndexer;
        }

        public void CommentMatches(CodeMatch match, CodeFileMatch codeFileMatch, string startComment, string endComment)
        {
            var matchesInFileLookup = match.CodeFileMatches.ToLookup(m => m.CodeFile.Filename);

            var sourceComments = new Comments(startComment, endComment);

            var matchesInFileAsString = matchesInFileLookup[codeFileMatch.CodeFile.Filename].Count();

            var comments = GetComments(sourceComments, matchesInFileAsString, match.CodeFileMatches.Count, GetBlockSize(codeFileMatch));

            codeFileMatch.CodeFile.AllSourceLines.Insert(codeFileMatch.MatchingLines.LastItem().OriginalLineNumber + 1, CodeLine.CreateTargetLine(comments.EndComment));
            codeFileMatch.CodeFile.AllSourceLines.Insert(codeFileMatch.MatchingLines[0].OriginalLineNumber, CodeLine.CreateTargetLine(comments.StartComment));

            foreach (var instance in match.CodeFileMatches.Select(m => m.CodeFile).Distinct())
            {
                this.codeFileLineIndexer.IndexCodeFile(instance);
            }
        }

        private static int GetBlockSize(CodeFileMatch codeFileMatch)
        {
            return codeFileMatch.MatchingLines.LastItem().OriginalLineNumber - codeFileMatch.MatchingLines[0].OriginalLineNumber;
        }

        private static Comments GetComments(Comments sourceComments, int matchesInFile, int matchesInAllFiles, int blockSize)
        {
            var matchesInFileAsString = matchesInFile.ToString();
            var blockSizeString = blockSize.ToString();
            var matchesInAllFilesString = matchesInAllFiles.ToString();

            var matchesInOtherFilesString = (matchesInAllFiles - matchesInFile).ToString();

            var currentEndComment = ReplaceMacrosInString(sourceComments.EndComment, matchesInFileAsString, blockSizeString, matchesInOtherFilesString);
            var currentStartComment = ReplaceMacrosInString(sourceComments.StartComment, matchesInFileAsString, blockSizeString, matchesInOtherFilesString);

            return new Comments(currentStartComment, currentEndComment);
        }

        private static string ReplaceMacrosInString(string str, string matchesInFileAsString, string blockSizeString, string matchesInOtherFilesString)
        {
            return str.Replace("@MATCHESINFILE@", matchesInFileAsString).Replace("@BLOCKSIZE@", blockSizeString).Replace("@MATCHESINOTHERFILES@", matchesInOtherFilesString);
        }

        private struct Comments
        {
            public Comments(string startComment, string endComment)
            {
                this.StartComment = startComment;
                this.EndComment = endComment;
            }

            public string EndComment { get; set; }

            public string StartComment { get; set; }
        }

    }
}
