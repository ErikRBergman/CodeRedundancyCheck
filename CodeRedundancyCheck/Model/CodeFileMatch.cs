using System.Collections.Generic;
using System.Linq;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    public class CodeFileMatch
    {
        public CodeFileMatch(CodeFile codeFile, int firstCompressedLineNumber, List<CodeLine> matchingLines)
        {
            this.MatchingLines = matchingLines;
            this.CodeFile = codeFile;
            this.FirstCompressedLineNumber = firstCompressedLineNumber;
        }

        public int FirstCompressedLineNumber { get; set; }

        public CodeFile CodeFile { get; set; }
        public List<CodeLine> MatchingLines { get; set; }

        public override string ToString()
        {
            if (this.MatchingLines == null)
            {
                return "*NO LINES*";
            }

            return this.MatchingLines[0] + " => " + this.MatchingLines.Last() + ", file: " + this.CodeFile.Filename;
        }
    }
}