using System.Collections.Generic;
using System.Linq;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    public class CodeFile
    {
        public string Filename { get; set; }

        public List<CodeLine> CodeLines { get; set; }

        public List<CodeLine> AllSourceLines { get; set; }

        public Dictionary<string, List<CodeLine>> CodeLinesDictionary { get; set; }
    }

}