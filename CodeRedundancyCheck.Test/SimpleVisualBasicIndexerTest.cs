using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeRedundancyCheck.Model;
using CodeRedundancyCheck.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeRedundancyCheck.Test
{
    [TestClass]
    public class SimpleVisualBasicIndexerTest
    {
        [TestMethod]
        public void TestWashLine()
        {
           
            var result = VisualBasicSourceWash.Singleton.WashLine("\t\tHaj\t\t\"HOJ\"\"FA     FA\"");
            Assert.AreEqual("Haj \"HOJ\"\"FA     FA\"", result);
        }


        [TestMethod]
        public void TestCompareFiles()
        {
            var indexer = new CodeFileComparer(new VisualBasicSourceWash());

            indexer.CodeLineFilters.Add(VisualBasicCodeLineFilter.Singleton);

            var files = Directory.GetFiles(@"C:\Projects\KCProjects\Claims\trunk\Inetpub\wwwroot\Claims\", "*.vb", SearchOption.AllDirectories);

            var codeFiles = files.Select(f => indexer.LoadFile(f));

            var matches = indexer.GetMatches(5, codeFiles).OrderByDescending(c => c.Lines * c.Matches.Count).ToList();

//            var matches = indexer.GetMatches(5, codeFiles).OrderByDescending(c => c.Matches.Count).ToList();

            var firstMatch = matches[0];

            var commenter = new CodeFileMatchCommenter(new CodeFileLineIndexer());

            string uniqueString = Guid.NewGuid().ToString("D") + ":" + DateTime.Now.ToString("s");
            commenter.CommentMatches(firstMatch, "' Start duplicate block " + uniqueString, "' End of duplicate block " + uniqueString );

            var writer = new VisualBasicCodeFileWriter();

            foreach (var file in firstMatch.Matches.Select(m => m.CodeFile).Distinct())
            {
                writer.WriteFile(file.Filename + ".new", file);
            }
            
            // Assert.AreEqual(1, matches.Count);

//             var compareFile = indexer.LoadFile(@"C:\Projects\KCProjects\Claims\trunk\Inetpub\wwwroot\Claims\Reg_Extern.aspx.vb");

        }
        
        [TestMethod]
        public void TestCompareFiles2()
        {
            var indexer = new CodeFileComparer(null);

            var sourceFile = new CodeFile
            {
                CodeLines = new List<CodeLine>(1000)
            };

            int line = 0;

            for (int i = 0; i < 8; i++)
            {
                int lineNumber = i%2;

                sourceFile.CodeLines.Add(
                    new CodeLine(
                        originalLineText: lineNumber.ToString(), 
                        originalLineNumber: ++line, 
                        originalLinePosition: 0)
                    {
                        WashedLineText = lineNumber.ToString()
                    });
            }
          
            sourceFile.CodeLines = VisualBasicSourceWash.Singleton.Wash(sourceFile.CodeLines).ToList();
            CodeFileIndexer.IndexCodeFile(sourceFile);

            var matches = indexer.GetMatches(2, sourceFile).OrderByDescending(c => c.Lines).ToList();
            // var matches = indexer.GetMatches(5, sourceFile).ToList();

            Assert.AreEqual(1, matches.Count);

            //             var compareFile = indexer.LoadFile(@"C:\Projects\KCProjects\Claims\trunk\Inetpub\wwwroot\Claims\Reg_Extern.aspx.vb");

        }


    }
}
