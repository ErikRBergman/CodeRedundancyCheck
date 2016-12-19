using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeRedundancyCheck.WinForms.UI
{
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using CodeRedundancyCheck.Languages.CSharp;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new ResultForm());

            Stopwatch sw = Stopwatch.StartNew();

            // D:\projects\dynamaster6

            CheckQRAsync().Wait();

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds + " ms");
        }


        public static async Task CheckQRAsync()
        {
            var codeFileComparer = new CodeFileComparer();

            var loader = new CodeFileLoader(new CSharpSourceWash(), new CodeFileIndexer(0xFFFFFFFF), new CodeFileLineIndexer());
            codeFileComparer.CodeLineFilter = CSharpCodeLineFilter.Singleton;

            // C:\Projects\celsa
//            var files = Directory.GetFiles(@" D:\projects\dynamaster6\", "*.cs", SearchOption.AllDirectories)

            var files = Directory.GetFiles(@"C:\Projects\celsa\", "*.cs", SearchOption.AllDirectories)
                .Where(
                f => !f.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase)
                && !f.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase)
                && !f.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
                && !f.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase)
                ).ToArray();

            var codeFiles = new List<CodeFile>(files.Length);

            foreach (var filename in files.OrderBy(f => f))
            {
                var file = await loader.LoadCodeFile(File.OpenRead(filename), Encoding.Default);
                file.Filename = filename;
                codeFiles.Add(file);
            }

//            var codeMatches = (await codeFileComparer.GetMatchesAsync(5, codeFiles)).OrderByDescending(c => c.Lines * c.CodeFileMatches.Count).ToList();
            var codeMatches = (await codeFileComparer.GetMatchesAsync(5, codeFiles)).OrderByDescending(c => c.CodeFileMatches.Count).ToList();
            var commenter = new CodeFileMatchCommenter(new CodeFileLineIndexer());

            var commentedMatches = new HashSet<CodeFile>();

            var commentedBlockCount = 0;
            var commentedLineCount = 0;

            var fullRefactoringLineSavings = 0;

            var thisFileLines = 0;

            var filenameToFind = "SpecificationController.cs";

            //var matches = codeMatches
            //    .Where(m => m.CodeFileMatches.Count(m2 => m2.CodeFile.Filename.EndsWith(filenameToFind, StringComparison.OrdinalIgnoreCase)) > 1)
            //    .SelectMany(codeMatch => codeMatch.CodeFileMatches.Select(codeFileMatch => new
            //    {
            //        CodeMatch = codeMatch,
            //        CodeFileMatch = codeFileMatch
            //    }))
            //    .GroupBy(m => m.CodeFileMatch.CodeFile);

            //foreach (var matchFile in matches)
            //{
            //    // Descending line to ensure we always add lines from the end.
            //    foreach (var match in matchFile.OrderByDescending(m => m.CodeFileMatch.FirstCodeFileLineNumber))
            //    {
            //        if (match.CodeFileMatch.CodeFile.Filename.EndsWith(filenameToFind, StringComparison.OrdinalIgnoreCase))
            //        {
            //            thisFileLines += match.CodeFileMatch.MatchingLines.Count;
            //        }

            //        var uniqueString = match.CodeMatch.UniqueId + ":" + DateTime.Now.ToString("s") + ", matches in this file @MATCHESINFILE@ (@MATCHESINOTHERFILES@ in other files), block size: @BLOCKSIZE@ lines";
            //        commenter.CommentMatches(match.CodeMatch, match.CodeFileMatch, "' Start duplicate block " + uniqueString, "' End of duplicate block " + uniqueString);

            //        commentedBlockCount += match.CodeMatch.CodeFileMatches.Count;
            //        fullRefactoringLineSavings += match.CodeMatch.ActualLines * (match.CodeMatch.CodeFileMatches.Count - 1);
            //        commentedLineCount += match.CodeMatch.ActualLines * match.CodeMatch.CodeFileMatches.Count;

            //        commentedMatches.Add(match.CodeFileMatch.CodeFile);
            //    }
            //}


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

    }
}
