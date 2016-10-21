using System.Collections.Generic;
using System.Linq;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    public class CodeFile
    {
        public string Filename { get; set; }

        public int UniqueId { get; set; }

        public CodeLine[] CodeLines { get; set; }

        public CodeLine[] AllSourceLines { get; set; }

        public Dictionary<int, List<CodeLine>> CodeLinesDictionary { get; set; }
    }

}