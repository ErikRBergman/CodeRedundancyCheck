using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeRedundancyCheck.Interface;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    public class CodeFileLineIndexer : ICodeFileLineIndexer
    {
        public void IndexCodeFile(CodeFile codeFile)
        {
            IndexCollection(codeFile.CodeLines, CollectionType.CodeFileLineIndex);
            IndexCollection(codeFile.AllSourceLines, CollectionType.OriginalFileLineIndex);
        }

        private enum CollectionType
        {
            CodeFileLineIndex = 1,
            OriginalFileLineIndex = 2
        }

        private static void IndexCollection(IEnumerable<CodeLine> lines, CollectionType collectionType)
        {
            int lineIndex = 0;
            foreach (var line in lines)
            {
                if (collectionType == CollectionType.CodeFileLineIndex)
                {
                    line.CodeFileLineIndex = lineIndex++;
                }
                else
                {
                    line.OriginalLineNumber = lineIndex++;
                }
            }
        }
    }
}
