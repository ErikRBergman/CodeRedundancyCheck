using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeRedundancyCheck.Interface;

namespace CodeRedundancyCheck
{
    public class CodeFileLineIndexer : ICodeFileLineIndexer
    {
        public void IndexCodeFile(CodeFile codeFile)
        {
            int lineIndex = 0;

            foreach (var line in codeFile.CodeLines)
            {
                line.CodeFileLineIndex = lineIndex++;
            }

        }
    }
}
