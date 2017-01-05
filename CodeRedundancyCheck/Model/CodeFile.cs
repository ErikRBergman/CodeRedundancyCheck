using System.Collections.Generic;
using System.Linq;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    using CodeRedundancyCheck.Common;

    public class CodeFile
    {
        public string Filename;

        public int UniqueId;

        public CodeLine[] CodeLines;

        public CodeLine[] AllSourceLines;

        public DivideAndConquerDictionary<ThinList<CodeLine>> CodeLinesDictionary;
    }

}