using System;
using System.Collections.Generic;
using System.Linq;
using CodeRedundancyCheck.Extensions;

namespace CodeRedundancyCheck
{
    using System.Collections.Concurrent;

    public class CodeMatch
    {
        private string uniqueId;

        public string UniqueId
        {
            get
            {
                if (this.uniqueId == null)
                {
                    this.uniqueId = Guid.NewGuid().ToString("N");
                }

                return this.uniqueId;
            }
            set { this.uniqueId = value; }
        }

        public ConcurrentBag<CodeFileMatch> CodeFileMatches { get; set; }

        public int Lines { get; set; }

        public int ActualLines
        {
            get
            {
                if (this.CodeFileMatches == null)
                    return 0;

                int count = 0;

                foreach (var match in this.CodeFileMatches)
                {
                    if (match.MatchingLines != null && match.MatchingLines.Count > 0)
                    {
                        count = Math.Max(match.MatchingLines.LastItem().OriginalLineNumber - match.MatchingLines[0].OriginalLineNumber, count);
                    }
                }

                return count;
            }
        }

        public override string ToString()
        {
            string text = "Actual lines:" + this.ActualLines + ", Matched lines:" + this.Lines + ", Matches: " + this.CodeFileMatches.Count;

            if (this.CodeFileMatches != null && this.CodeFileMatches.Count > 0)
            {
                CodeFileMatch match;

                if (this.CodeFileMatches.TryPeek(out match))
                {
                    text = text + ", First match: " + match;
                }
            }


            return text;
        }
    }
}