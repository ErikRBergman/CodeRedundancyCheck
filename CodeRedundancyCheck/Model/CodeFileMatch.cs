namespace CodeRedundancyCheck
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using CodeRedundancyCheck.Model;

    [DataContract]
    public class CodeFileMatch
    {
        public CodeFileMatch(CodeFile codeFile, int firstCodeFileLineNumber, List<CodeLine> matchingLines, long lineKey)
        {
            this.MatchingLines = matchingLines;
            this.LineKey = lineKey;
            this.CodeFile = codeFile;
            this.FirstCodeFileLineNumber = firstCodeFileLineNumber;
        }

        [DataMember]
        public int FirstCodeFileLineNumber { get; set; }

        public CodeFile CodeFile { get; set; }

        [DataMember]
        public List<CodeLine> MatchingLines { get; set; }

        [DataMember]
        public long LineKey { get; set; }

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