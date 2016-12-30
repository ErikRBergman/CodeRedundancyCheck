namespace CodeRedundancyCheck.Languages.CSharp
{
    using System;
    using System.Text;

    using CodeRedundancyCheck.Interface;
    using CodeRedundancyCheck.Model;

    public class CSharpCodeLineFilter : ICodeLineFilter
    {
        public static ICodeLineFilter Singleton { get; } = new CSharpCodeLineFilter();

        private static int endOfBlockHashCode = StringComparer.OrdinalIgnoreCase.GetHashCode("}");
        private static int elseHashCode = StringComparer.OrdinalIgnoreCase.GetHashCode("else");

        private static ulong elseBlock = 0;

        static unsafe CSharpCodeLineFilter()
        {
            unsafe
            {
                fixed (char* valuePtr = "else")
                {
                    elseBlock = *(ulong*)valuePtr;
                }
            }
        }

        public bool MayStartBlock(CodeLine codeLine)
        {
            if (codeLine.WashedLineHashCode == endOfBlockHashCode)
            {
                var washedLine = codeLine.WashedLineText;
                if (washedLine[0] == '}' && washedLine.Length == 1)
                {
                    return false;
                }
            }
            else if (elseHashCode == codeLine.WashedLineHashCode)
            {
                var washedLine = codeLine.WashedLineText;
                if (washedLine.Length == 4)
                {
                    unsafe
                    {
                        fixed (char* valuePtr = washedLine)
                        {
                            if (*(ulong*)valuePtr == elseBlock)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
