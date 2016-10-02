using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public async Task TestCompareFiles()
        {
            var codeFileComparer = new CodeFileComparer();

            var loader = new CodeFileLoader(new VisualBasicSourceWash(), new CodeFileIndexer(), new CodeFileLineIndexer());
            codeFileComparer.CodeLineFilters.Add(VisualBasicCodeLineFilter.Singleton);

            var files = Directory.GetFiles(@"C:\Projects\KCProjects\Claims\trunk\Inetpub\wwwroot\Claims\", "*.vb", SearchOption.AllDirectories);

            var codeFiles = new List<CodeFile>(files.Length);

            foreach (var filename in files)
            {
                var file = await loader.LoadCodeFile(File.OpenRead(filename), Encoding.Default);
                file.Filename = filename;
                codeFiles.Add(file);
            }

            var matches = codeFileComparer.GetMatches(5, codeFiles).OrderByDescending(c => c.Lines * c.Matches.Count).ToList();
            var commenter = new CodeFileMatchCommenter(new CodeFileLineIndexer());

            var commentedMatches = new List<CodeFile>(100);

            var commentedBlockCount = 0;
            var commentedLineCount = 0;

            var fullRefactoringLineSavings = 0;

            foreach (var firstMatch in matches.Where(m => m.Matches.All(m2 => m2.CodeFile.Filename.EndsWith("Reg_Approval.aspx.vb"))))
            {
                var uniqueString = Guid.NewGuid().ToString("D") + ":" + DateTime.Now.ToString("s") + ", matches in this file @MATCHESINFILE@, block size: @BLOCKSIZE@ lines";
                commenter.CommentMatches(firstMatch, "' Start duplicate block " + uniqueString, "' End of duplicate block " + uniqueString);

                commentedBlockCount += firstMatch.Matches.Count;
                fullRefactoringLineSavings += firstMatch.ActualLines * (firstMatch.Matches.Count-1);
                commentedLineCount += firstMatch.ActualLines * firstMatch.Matches.Count;

                commentedMatches.AddRange(firstMatch.Matches.Select(m => m.CodeFile).Distinct());
            }

            var writer = new VisualBasicCodeFileWriter();

            foreach (var file in commentedMatches.Distinct())
            {
                File.Move(file.Filename, file.Filename + "." + DateTime.Now.ToString("s").Replace(":", "") + ".backup");
                await writer.WriteFile(file.Filename , file.AllSourceLines, Encoding.Unicode);

                //                await writer.WriteFile(file.Filename + "." + DateTime.Now.ToString("s").Replace(":", "") + ".result", file.AllSourceLines, Encoding.Default);
                //  await writer.WriteFile(file.Filename + ".result", file.AllSourceLines, Encoding.Default);
            }

            // Assert.AreEqual(1, matches.Count);

            //             var compareFile = indexer.LoadFile(@"C:\Projects\KCProjects\Claims\trunk\Inetpub\wwwroot\Claims\Reg_Extern.aspx.vb");

        }

        [TestMethod]
        public void TestCompareFiles2()
        {
            var indexer = new CodeFileComparer();

            var sourceFile = new CodeFile
            {
                CodeLines = new List<CodeLine>(1000)
            };

            int line = 0;

            for (int i = 0; i < 8; i++)
            {
                int lineNumber = i % 2;

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
            new CodeFileIndexer().IndexCodeFile(sourceFile);

            var matches = indexer.GetMatches(2, sourceFile).OrderByDescending(c => c.Lines).ToList();
            // var matches = indexer.GetMatches(5, sourceFile).ToList();

            Assert.AreEqual(1, matches.Count);

            //             var compareFile = indexer.LoadFile(@"C:\Projects\KCProjects\Claims\trunk\Inetpub\wwwroot\Claims\Reg_Extern.aspx.vb");

        }


    }
}
