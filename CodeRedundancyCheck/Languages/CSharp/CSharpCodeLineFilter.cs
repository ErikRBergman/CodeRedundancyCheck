namespace CodeRedundancyCheck.Languages.CSharp
{
    using System;

    using CodeRedundancyCheck.Interface;
    using CodeRedundancyCheck.Model;

    public class CSharpCodeLineFilter : ICodeLineFilter
    {
        public static ICodeLineFilter Singleton { get; } = new CSharpCodeLineFilter();

        public bool MayStartBlock(CodeLine codeLine, CodeFile codeFile)
        {

            if (string.Compare(codeLine.WashedLineText, "}", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }

            if (string.Compare(codeLine.WashedLineText, "else", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }

            return true;
        }
    }
}
