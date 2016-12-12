using System.Collections.Generic;
using System.Linq;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    using CodeRedundancyCheck.Common;

    public class CodeFile
    {
        public string Filename { get; set; }

        public int UniqueId { get; set; }

        public CodeLine[] CodeLines { get; set; }

        public CodeLine[] AllSourceLines { get; set; }

//        public Dictionary<int, ThinList<CodeLine>> CodeLinesDictionary { get; set; }
        public DivideAndConquerDictionary<ThinList<CodeLine>> CodeLinesDictionary { get; set; }
    }

}