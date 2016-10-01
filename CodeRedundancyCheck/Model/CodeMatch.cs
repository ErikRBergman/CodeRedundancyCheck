using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRedundancyCheck
{
    public class CodeMatch
    {

        public List<CodeFileMatch> Matches { get; set; }

        public int Lines { get; set; }

        public int ActualLines
        {
            get
            {
                if (this.Matches == null)
                    return 0;

                int count = 0;

                foreach (var match in this.Matches)
                {
                    if (match.MatchingLines != null && match.MatchingLines.Count > 0)
                    {
                        count = Math.Max(match.MatchingLines[match.MatchingLines.Count - 1].OriginalLineNumber - match.MatchingLines[0].OriginalLineNumber, count);
                    }
                }

                return count;
            }
        }

        public override string ToString()
        {
            string text = "Actual lines:" + this.ActualLines + ", Matched lines:" + this.Lines + ", Matches: " + this.Matches.Count;

            if (this.Matches != null && this.Matches.Count > 0)
            {
                text = text + ", First match: " + this.Matches[0];
            }


            return text;
        }
    }
}