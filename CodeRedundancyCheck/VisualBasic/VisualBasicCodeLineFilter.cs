using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeRedundancyCheck.Interface;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck.VisualBasic
{
    public class VisualBasicCodeLineFilter : ICodeLineFilter
    {
        public static ICodeLineFilter Singleton { get; } = new VisualBasicCodeLineFilter();

        public bool MayStartBlock(CodeLine codeLine, CodeFile codeFile)
        {

            // never start with "end if"
            if (string.Compare(codeLine.WashedLineText, "end if", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }

            // never start with "end sub"
            if (string.Compare(codeLine.WashedLineText, "end sub", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }

            // never start with "end function"
            if (string.Compare(codeLine.WashedLineText, "end function", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }

            return true;
        }
    }
}
