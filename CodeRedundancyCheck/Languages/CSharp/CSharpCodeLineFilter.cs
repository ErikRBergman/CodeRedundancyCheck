namespace CodeRedundancyCheck.Languages.CSharp
{
    using System;

    using CodeRedundancyCheck.Interface;
    using CodeRedundancyCheck.Model;

    public class CSharpCodeLineFilter : ICodeLineFilter
    {
        public static ICodeLineFilter Singleton { get; } = new CSharpCodeLineFilter();

        private static int endOfBlockHashCode = StringComparer.OrdinalIgnoreCase.GetHashCode("}");
        private static int elseHashCode = StringComparer.OrdinalIgnoreCase.GetHashCode("else");

        public bool MayStartBlock(CodeLine codeLine, CodeFile codeFile)
        {

            if (codeLine.WashedLineHashCode == endOfBlockHashCode && string.Compare(codeLine.WashedLineText, "}", StringComparison.Ordinal) == 0)
            {
                return false;
            }

            if (elseHashCode == codeLine.WashedLineHashCode && string.Compare(codeLine.WashedLineText, "else", StringComparison.Ordinal) == 0)
            {
                return false;
            }

            return true;
        }
    }
}
