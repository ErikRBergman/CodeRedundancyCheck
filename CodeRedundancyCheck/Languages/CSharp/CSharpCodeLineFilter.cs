namespace CodeRedundancyCheck.Languages.CSharp
{
    using System;
    using System.Text;

    using CodeRedundancyCheck.Interface;
    using CodeRedundancyCheck.Model;

    public class CSharpCodeLineFilter : ICodeLineFilter
    {
        public static ICodeLineFilter Singleton { get; } = new CSharpCodeLineFilter();

        private static readonly int EndOfBlockHashCode = StringComparer.OrdinalIgnoreCase.GetHashCode("}");
        private static readonly int ElseHashCode = StringComparer.OrdinalIgnoreCase.GetHashCode("else");

        private static readonly ulong ElseBlock;

        static unsafe CSharpCodeLineFilter()
        {
            unsafe
            {
                fixed (char* valuePtr = "else")
                {
                    ElseBlock = *(ulong*)valuePtr;
                }
            }
        }

        public bool MayStartBlock(CodeLine codeLine)
        {
            if (codeLine.WashedLineHashCode == EndOfBlockHashCode)
            {
                var washedLine = codeLine.WashedLineText;
                if (washedLine[0] == '}' && washedLine.Length == 1)
                {
                    return false;
                }
            }
            else if (ElseHashCode == codeLine.WashedLineHashCode)
            {
                var washedLine = codeLine.WashedLineText;
                if (washedLine.Length == 4)
                {
                    unsafe
                    {
                        fixed (char* valuePtr = washedLine)
                        {
                            if (*(ulong*)valuePtr == ElseBlock)
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
