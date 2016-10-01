using System.Collections.Generic;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck.Interface
{
    public interface ISourceWash
    {
        IEnumerable<CodeLine> Wash(IEnumerable<CodeLine> loadFileData);
        string WashLine(string sourceCode);
    }
}