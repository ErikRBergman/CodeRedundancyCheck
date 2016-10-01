using System.Linq;
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
            foreach (var instance in match.Matches.OrderByDescending(m => m.FirstCompressedLineNumber))
            {
                instance.CodeFile.CodeLines.Insert(instance.MatchingLines[instance.MatchingLines.Count-1].CodeFileLineIndex, new CodeLine(endComment, -1, 0 ));
                instance.CodeFile.CodeLines.Insert(instance.MatchingLines[0].CodeFileLineIndex, new CodeLine(startComment, -1, 0));
            }

            foreach (var instance in match.Matches.Select(m => m.CodeFile).Distinct())
            {
                this.codeFileLineIndexer.IndexCodeFile(instance);
            }
        }
    }
}
