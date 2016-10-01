using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck.Interface
{
    public interface ICodeLineFilter
    {
        bool MayStartBlock(CodeLine codeLine, CodeFile codeFile);
    }
}
