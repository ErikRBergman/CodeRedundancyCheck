namespace CodeRedundancyCheck.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CodeMatchContainer
    {
        private readonly List<CodeMatch> codeMatches = new List<CodeMatch>();

        private readonly object lockObject = new object();

        private readonly CodeMatchContainerKey key;

        public CodeMatchContainer(CodeMatchContainerKey key)
        {
            this.key = key;
        }

        // not thread safe, no need while noone is reading while we're adding
        public IReadOnlyCollection<CodeMatch> CodeMatches => this.codeMatches;

        public CodeMatch GetOrAddCodeMatch(IReadOnlyCollection<CodeLine> codeLines)
        {
            lock (this.lockObject)
            {
                using (var thisEnumerator = codeLines.GetEnumerator())
                {
                    // At this point, we must assume there are the same number of matching lines in both collections 
                    foreach (var match in this.codeMatches)
                    {
                        bool isMatch = true;
                        using (var compareEnumerator = match.MatchingCodeLines.GetEnumerator())
                        {
                            while (compareEnumerator.MoveNext())
                            {
                                thisEnumerator.MoveNext();

                                if (AreCodeLinesEqual(thisEnumerator.Current, compareEnumerator.Current) == false)
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                        }

                        if (isMatch)
                        {
                            // an existing block was found
                            return match;
                        }

                        thisEnumerator.Reset();
                    }

                    // No match was found
                    var codeMatch = new CodeMatch(codeLines)
                                    {
                                        LineCount = codeLines.Count
                                    };

                    this.codeMatches.Add(codeMatch);

                    return codeMatch;

                }
            }
        }


        private static bool AreCodeLinesEqual(CodeLine codeLine1, CodeLine codeLine2)
        {
            return codeLine1.WashedLineHashCode == codeLine2.WashedLineHashCode && string.Compare(codeLine1.WashedLineText, codeLine2.WashedLineText, StringComparison.Ordinal) == 0;
        }
    }


    public struct CodeMatchContainerKey
    {
        private readonly int firstLineHash;

        private readonly string firstLineText;

        private readonly uint next4Hash;

        private readonly int linesOfCode;

        private readonly int hashCode;

        public CodeMatchContainerKey(int firstLineHash, string firstLineText, uint next4Hash, int linesOfCode)
        {
            this.firstLineHash = firstLineHash;
            this.firstLineText = firstLineText;
            this.next4Hash = next4Hash;
            this.linesOfCode = linesOfCode;

            this.hashCode = firstLineHash + linesOfCode + (int)next4Hash;
        }

        public override int GetHashCode() => this.hashCode;

        public override bool Equals(object obj)
        {
            var other = (CodeMatchContainerKey)obj;
            return other.firstLineHash == this.firstLineHash && other.next4Hash == this.next4Hash && other.linesOfCode == this.linesOfCode;
        }

        public override string ToString()
        {
            return "Hash: " + this.hashCode + ", FirstlineHash: " + this.firstLineHash + ", next4hash: " + this.next4Hash + ", linesOfCode: " + this.linesOfCode + ", line text:" + this.firstLineText;
        }
    }

}
