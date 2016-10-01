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
            codeFile.CodeLinesDictionary = new Dictionary<string, List<CodeLine>>(codeFile.CodeLines.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var line in codeFile.CodeLines)
            {
                List<CodeLine> list;

                if (!codeFile.CodeLinesDictionary.TryGetValue(line.WashedLineText, out list))
                {
                    list = new List<CodeLine>();
                    codeFile.CodeLinesDictionary.Add(line.WashedLineText, list);
                }

                list.Add(line);
            }
        }
    }
}