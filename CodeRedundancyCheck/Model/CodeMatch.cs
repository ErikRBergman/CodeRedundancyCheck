using System;
using System.Collections.Generic;
using System.Linq;
using CodeRedundancyCheck.Extensions;

namespace CodeRedundancyCheck
{
    using System.Collections.Concurrent;

    using CodeRedundancyCheck.Model;

    public class CodeMatch
    {
        private string uniqueId;

        public CodeMatch(IReadOnlyCollection<CodeLine> matchingCodeLines)
        {
            this.MatchingCodeLines = matchingCodeLines.ToArray();
        }

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
            set
            {
                this.uniqueId = value;
            }
        }

        public ConcurrentDictionary<CodeFileMatchKey, CodeFileMatch> CodeFileMatches { get; private set; } = new ConcurrentDictionary<CodeFileMatchKey, CodeFileMatch>();

        public IReadOnlyCollection<CodeLine> MatchingCodeLines { get; set; }

        public int LineCount { get; set; }

        public int ActualLines
        {
            get
            {
                if (this.CodeFileMatches == null)
                    return 0;

                int count = 0;

                foreach (var match in this.CodeFileMatches.Values)
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
            var text = "Actual lines:" + this.ActualLines + ", Matched lines:" + this.LineCount + ", Matches: " + this.CodeFileMatches.Count;

            if (this.CodeFileMatches != null && this.CodeFileMatches.Count > 0)
            {
                var match = this.CodeFileMatches.Values.FirstOrDefault();

                if (match != null)
                {
                    text = text + ", First match: " + match;
                }
            }


            return text;
        }
    }
}