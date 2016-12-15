using System;
using System.Collections.Generic;
using CodeRedundancyCheck.Interface;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck
{
    using CodeRedundancyCheck.Common;

    public class CodeFileIndexer : ICodeFileIndexer
    {
        //public void IndexCodeFile(CodeFile codeFile)
        //{
        //    var dictionary = new Dictionary<int, ThinList<CodeLine>>(codeFile.CodeLines.Length);

        //    foreach (var line in codeFile.CodeLines)
        //    {
        //        ThinList<CodeLine> list;

        //        var washedLineText = line.WashedLineText;
        //        line.WashedLineHashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(washedLineText);

        //        if (!dictionary.TryGetValue(line.WashedLineHashCode, out list))
        //        {
        //            list = new ThinList<CodeLine>(10);
        //            dictionary.Add(line.WashedLineHashCode, list);
        //        }

        //        if (list.length >= list.Capacity)
        //        {
        //            list.Resize(list.Capacity + 10);
        //        }

        //        list.Add(line);
        //    }

        //    codeFile.CodeLinesDictionary = dictionary;
        //}

        public void IndexCodeFile(CodeFile codeFile)
        {
            var dictionary = new Dictionary<int, ThinList<CodeLine>>(codeFile.CodeLines.Length);

            foreach (var line in codeFile.CodeLines)
            {
                ThinList<CodeLine> list;

                var washedLineText = line.WashedLineText;
                line.WashedLineHashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(washedLineText);

                if (!dictionary.TryGetValue(line.WashedLineHashCode, out list))
                {
                    list = new ThinList<CodeLine>(10);
                    dictionary.Add(line.WashedLineHashCode, list);
                }

                if (list.length >= list.Capacity)
                {
                    list.Resize(list.Capacity + 10);
                }

                list.Add(line);
            }

            uint runningHash = 0;

            for (int index = codeFile.CodeLines.Length - 1; index > -1; index--)
            {
                var line = codeFile.CodeLines[index];

                if (line.IsCodeLine)
                {
                    line.Next4MiniHash = runningHash;
                    runningHash <<= 8;
                    runningHash |= (byte)(line.WashedLineHashCode & 0xFF);
                }
            }

            codeFile.CodeLinesDictionary = new DivideAndConquerDictionary<ThinList<CodeLine>>(dictionary);
        }
    }
}