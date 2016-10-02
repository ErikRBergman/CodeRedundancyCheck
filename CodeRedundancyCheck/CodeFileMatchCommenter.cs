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

        public void CommentMatches(CodeMatch match, string startComment, string endComment)
        {
            var matchesInFileLookup = match.Matches.ToLookup(m => m.CodeFile.Filename);

            var sourceComments = new Comments(startComment, endComment);

            foreach (var instance in match.Matches.OrderByDescending(m => m.FirstCompressedLineNumber))
            {
                var matchesInFileAsString = matchesInFileLookup[instance.CodeFile.Filename].Count();

                var comments = GetComments(sourceComments, matchesInFileAsString, GetBlockSize(instance));

                instance.CodeFile.AllSourceLines.Insert(instance.MatchingLines.LastItem().OriginalLineNumber + 1, CodeLine.CreateTargetLine(comments.EndComment));
                instance.CodeFile.AllSourceLines.Insert(instance.MatchingLines[0].OriginalLineNumber, CodeLine.CreateTargetLine(comments.StartComment));
            }

            foreach (var instance in match.Matches.Select(m => m.CodeFile).Distinct())
            {
                this.codeFileLineIndexer.IndexCodeFile(instance);
            }
        }

        private static int GetBlockSize(CodeFileMatch codeFileMatch)
        {
            return codeFileMatch.MatchingLines.LastItem().OriginalLineNumber - codeFileMatch.MatchingLines[0].OriginalLineNumber;
        }

        private static Comments GetComments(Comments sourceComments, int matchesInFile, int blockSize)
        {
            var matchesInFileAsString = matchesInFile.ToString();
            var blockSizeString = blockSize.ToString();
            var currentEndComment = sourceComments.EndComment.Replace("@MATCHESINFILE@", matchesInFileAsString).Replace("@BLOCKSIZE@", blockSizeString);
            var currentStartComment = sourceComments.StartComment.Replace("@MATCHESINFILE@", matchesInFileAsString).Replace("@BLOCKSIZE@", blockSizeString);
            return new Comments(currentStartComment, currentEndComment);
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
