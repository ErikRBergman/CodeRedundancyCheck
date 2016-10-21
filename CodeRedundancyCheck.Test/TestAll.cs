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
    using CodeRedundancyCheck.Languages.CSharp;

    [TestClass]
    public class TestAll
    {
        [TestMethod]
        public void TestWashLine()
        {

            var result = VisualBasicSourceWash.Singleton.WashLine("\t\tHaj\t\t\"HOJ\"\"FA     FA\"");
            Assert.AreEqual("Haj \"HOJ\"\"FA     FA\"", result);
        }

        [TestMethod]
        public async Task CheckQRAsync()
        {
            var codeFileComparer = new CodeFileComparer();

            var loader = new CodeFileLoader(new CSharpSourceWash(), new CodeFileIndexer(), new CodeFileLineIndexer());
            codeFileComparer.CodeLineFilter = CSharpCodeLineFilter.Singleton;

            var files = Directory.GetFiles(@"C:\projects\Celsa\QR\Trunk\", "*.cs", SearchOption.AllDirectories);

            var codeFiles = new List<CodeFile>(files.Length);

            foreach (var filename in files)
            {
                var file = await loader.LoadCodeFile(File.OpenRead(filename), Encoding.Default);
                file.Filename = filename;
                codeFiles.Add(file);
            }

            var codeMatches = (await codeFileComparer.GetMatchesAsync(5, codeFiles)).OrderByDescending(c => c.Lines * c.CodeFileMatches.Count).ToList();
            var commenter = new CodeFileMatchCommenter(new CodeFileLineIndexer());

            var commentedMatches = new HashSet<CodeFile>();

            var commentedBlockCount = 0;
            var commentedLineCount = 0;

            var fullRefactoringLineSavings = 0;

            var thisFileLines = 0;

            var filenameToFind = "SpecificationController.cs";

            //            foreach (var match in matches.Where(m => m.Matches.Count(m2 => m2.CodeFile.Filename.EndsWith("Reg_Approval.aspx.vb")) > 1).SelectMany(m => m.Matches.Select(n => new { CodeMatch = m, CodeFileMatch = n })).OrderBy(m => m.CodeFileMatch.CodeFile.Filename).ThenByDescending(m => m.CodeFileMatch.MatchingLines[0].OriginalLineNumber))
            foreach (var matchFile in codeMatches.Where(m => m.CodeFileMatches.Count(m2 => m2.CodeFile.Filename.EndsWith(filenameToFind, StringComparison.OrdinalIgnoreCase)) > 1).SelectMany(codeMatch => codeMatch.CodeFileMatches.Select(codeFileMatch => new { CodeMatch = codeMatch, CodeFileMatch = codeFileMatch })).GroupBy(m => m.CodeFileMatch.CodeFile))
            {
                // Descending line to ensure we always add lines from the end.
                foreach (var match in matchFile.OrderByDescending(m => m.CodeFileMatch.FirstCodeFileLineNumber))
                {
                    if (match.CodeFileMatch.CodeFile.Filename.EndsWith(filenameToFind, StringComparison.OrdinalIgnoreCase))
                    {
                        thisFileLines += match.CodeFileMatch.MatchingLines.Count;
                    }

                    var uniqueString = match.CodeMatch.UniqueId + ":" + DateTime.Now.ToString("s") + ", matches in this file @MATCHESINFILE@ (@MATCHESINOTHERFILES@ in other files), block size: @BLOCKSIZE@ lines";
                    commenter.CommentMatches(match.CodeMatch, match.CodeFileMatch, "' Start duplicate block " + uniqueString, "' End of duplicate block " + uniqueString);

                    commentedBlockCount += match.CodeMatch.CodeFileMatches.Count;
                    fullRefactoringLineSavings += match.CodeMatch.ActualLines * (match.CodeMatch.CodeFileMatches.Count - 1);
                    commentedLineCount += match.CodeMatch.ActualLines * match.CodeMatch.CodeFileMatches.Count;

                    commentedMatches.Add(match.CodeFileMatch.CodeFile);
                }
            }


            //var writer = new CSharpCodeFileWriter();

            //foreach (var file in commentedMatches.Where(f => f.Filename.EndsWith("SpecificationController.cs", StringComparison.OrdinalIgnoreCase)).Distinct())
            //{
            //    File.Move(file.Filename, file.Filename + "." + DateTime.Now.ToString("s").Replace(":", "") + ".backup");
            //    await writer.WriteFile(file.Filename, file.AllSourceLines, Encoding.UTF8);

            //    //                await writer.WriteFile(file.Filename + "." + DateTime.Now.ToString("s").Replace(":", "") + ".result", file.AllSourceLines, Encoding.Default);
            //    //  await writer.WriteFile(file.Filename + ".result", file.AllSourceLines, Encoding.Default);
            //}


            // C:\projects\Celsa\QR\Trunk\


        }



        [TestMethod]
        public async Task TestCompareFiles()
        {
            var codeFileComparer = new CodeFileComparer();

            var loader = new CodeFileLoader(new VisualBasicSourceWash(), new CodeFileIndexer(), new CodeFileLineIndexer());
            codeFileComparer.CodeLineFilter = VisualBasicCodeLineFilter.Singleton;

            var files = Directory.GetFiles(@"C:\Projects\KCProjects\Claims\trunk\Inetpub\wwwroot\Claims\", "*.vb", SearchOption.AllDirectories);

            var codeFiles = new List<CodeFile>(files.Length);

            foreach (var filename in files)
            {
                var file = await loader.LoadCodeFile(File.OpenRead(filename), Encoding.Default);
                file.Filename = filename;
                codeFiles.Add(file);
            }

            var codeMatches = (await codeFileComparer.GetMatchesAsync(5, codeFiles)).OrderByDescending(c => c.Lines * c.CodeFileMatches.Count).ToList();
            var commenter = new CodeFileMatchCommenter(new CodeFileLineIndexer());

            var commentedMatches = new HashSet<CodeFile>();

            var commentedBlockCount = 0;
            var commentedLineCount = 0;

            var fullRefactoringLineSavings = 0;

            var thisFileLines = 0;

            //            foreach (var match in matches.Where(m => m.Matches.Count(m2 => m2.CodeFile.Filename.EndsWith("Reg_Approval.aspx.vb")) > 1).SelectMany(m => m.Matches.Select(n => new { CodeMatch = m, CodeFileMatch = n })).OrderBy(m => m.CodeFileMatch.CodeFile.Filename).ThenByDescending(m => m.CodeFileMatch.MatchingLines[0].OriginalLineNumber))
            foreach (var matchFile in codeMatches.Where(m => m.CodeFileMatches.Count(m2 => m2.CodeFile.Filename.EndsWith("Reg_Approval.aspx.vb")) > 1).SelectMany(codeMatch => codeMatch.CodeFileMatches.Select(codeFileMatch => new { CodeMatch = codeMatch, CodeFileMatch = codeFileMatch})).GroupBy(m => m.CodeFileMatch.CodeFile))
            {
                // Descending line to ensure we always add lines from the end.
                foreach (var match in matchFile.OrderByDescending(m => m.CodeFileMatch.FirstCodeFileLineNumber))
                {
                    if (match.CodeFileMatch.MatchingLines[0].WashedLineText == "Dim Cause As Integer")
                    {
                        var xxxa = 1;
                    }


                    if (match.CodeFileMatch.CodeFile.Filename.EndsWith("Reg_Approval.aspx.vb"))
                    {
                        thisFileLines += match.CodeFileMatch.MatchingLines.Count;
                    }

                    var uniqueString = match.CodeMatch.UniqueId + ":" + DateTime.Now.ToString("s") + ", matches in this file @MATCHESINFILE@ (@MATCHESINOTHERFILES@ in other files), block size: @BLOCKSIZE@ lines";
                    commenter.CommentMatches(match.CodeMatch, match.CodeFileMatch, "' Start duplicate block " + uniqueString, "' End of duplicate block " + uniqueString);

                    commentedBlockCount += match.CodeMatch.CodeFileMatches.Count;
                    fullRefactoringLineSavings += match.CodeMatch.ActualLines * (match.CodeMatch.CodeFileMatches.Count - 1);
                    commentedLineCount += match.CodeMatch.ActualLines * match.CodeMatch.CodeFileMatches.Count;

                    commentedMatches.Add(match.CodeFileMatch.CodeFile);
                }
            }


            var writer = new VisualBasicCodeFileWriter();

            foreach (var file in commentedMatches.Where(f => f.Filename.EndsWith("Reg_Approval.aspx.vb")).Distinct())
            {
                File.Move(file.Filename, file.Filename + "." + DateTime.Now.ToString("s").Replace(":", "") + ".backup");
                await writer.WriteFile(file.Filename, file.AllSourceLines, Encoding.UTF8);

                //                await writer.WriteFile(file.Filename + "." + DateTime.Now.ToString("s").Replace(":", "") + ".result", file.AllSourceLines, Encoding.Default);
                //  await writer.WriteFile(file.Filename + ".result", file.AllSourceLines, Encoding.Default);
            }

            // Assert.AreEqual(1, matches.Count);

            //             var compareFile = indexer.LoadFile(@"C:\Projects\KCProjects\Claims\trunk\Inetpub\wwwroot\Claims\Reg_Extern.aspx.vb");

        }

        [TestMethod]
        public async Task TestCompareFiles2()
        {
            var indexer = new CodeFileComparer();

            var codeLines = new List<CodeLine>(1000);

            int line = 0;

            for (int i = 0; i < 8; i++)
            {
                int lineNumber = i % 2;

                codeLines.Add(
                    new CodeLine(
                        originalLineText: lineNumber.ToString(),
                        originalLineNumber: ++line,
                        originalLinePosition: 0)
                    {
                        WashedLineText = lineNumber.ToString()
                    });
            }

            var sourceFile = new CodeFile
            {
                CodeLines = codeLines.ToArray()
            };


            sourceFile.CodeLines = VisualBasicSourceWash.Singleton.Wash(sourceFile.CodeLines).ToArray();
            new CodeFileIndexer().IndexCodeFile(sourceFile);

            var matches = (await indexer.GetMatchesAsync(2, sourceFile)).OrderByDescending(c => c.Lines).ToList();
            // var matches = indexer.GetMatches(5, sourceFile).ToList();

            Assert.AreEqual(1, matches.Count);

            //             var compareFile = indexer.LoadFile(@"C:\Projects\KCProjects\Claims\trunk\Inetpub\wwwroot\Claims\Reg_Extern.aspx.vb");

        }


    }

    public interface IFilter<T>
    {
        IEnumerable<T> Handle(IEnumerable<T> items);
    }

    public class ArticleFilter : IFilter<Article>
    {
        public IEnumerable<Article> Handle(IEnumerable<Article> items)
        {
            throw new NotImplementedException();
        }
    }

    public class Article
    {
    }
}
