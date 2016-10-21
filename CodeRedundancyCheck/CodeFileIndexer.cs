using System;
using System.Collections.Generic;
using CodeRedundancyCheck.Interface;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    public class CodeFileIndexer : ICodeFileIndexer
    {
        public void IndexCodeFile(CodeFile codeFile)
        {
            codeFile.CodeLinesDictionary = new Dictionary<int, List<CodeLine>>(codeFile.CodeLines.Length);

            foreach (var line in codeFile.CodeLines)
            {
                List<CodeLine> list;

                var washedLineText = line.WashedLineText;
                line.WashedLineHashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(washedLineText);

                if (!codeFile.CodeLinesDictionary.TryGetValue(line.WashedLineHashCode, out list))
                {
                    list = new List<CodeLine>();
                    codeFile.CodeLinesDictionary.Add(line.WashedLineHashCode, list);

                }

                list.Add(line);
            }
        }
    }
}