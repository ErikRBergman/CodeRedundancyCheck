namespace CodeRedundancyCheck
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using CodeRedundancyCheck.Extensions;
    using CodeRedundancyCheck.Model;

    [DataContract]
    public class CodeMatch
    {
        private string uniqueId;

        public CodeMatch(IReadOnlyCollection<CodeLine> matchingCodeLines)
        {
            this.MatchingCodeLines = matchingCodeLines.ToArray();
        }

        public int ActualLines
        {
            get
            {
                if (this.CodeFileMatches == null)
                    return 0;

                var count = 0;

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

        public ConcurrentDictionary<CodeFileMatchKey, CodeFileMatch> CodeFileMatches { get; private set; } = new ConcurrentDictionary<CodeFileMatchKey, CodeFileMatch>();

        [DataMember]
        public int CodeFileMatchCount => this.CodeFileMatches.Count;

        [DataMember]
        public string CodeFileMatchSummary => string.Join(",", this.CodeFileMatches.Values.Select(v => v.CodeFile.Filename + "-" + v.FirstCodeFileLineNumber).OrderBy(v => v));

        [DataMember]
        public int LineCount { get; set; }

        // [DataMember]
        public IReadOnlyCollection<CodeLine> MatchingCodeLines { get; set; }

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

        private string lineSummary;

        [DataMember]
        public string LineSummary => this.lineSummary ?? (this.lineSummary = string.Join(",", this.MatchingCodeLines.Select(l => l.WashedLineText)));

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