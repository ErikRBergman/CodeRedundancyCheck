using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeRedundancyCheck.Common;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck.WinForms.UI
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using CodeRedundancyCheck.Languages.CSharp;

    using Newtonsoft.Json;

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
            // D:\projects\dynamaster6

            CheckQRAsync().Wait();
        }


        public static async Task CheckQRAsync()
        {
            var codeFileComparer = new CodeFileComparer();

            var loader = new CodeFileLoader(new CSharpSourceWash(), new CodeFileIndexer(0xFFFFFFFF), new CodeFileLineIndexer(), CSharpCodeLineFilter.Singleton);

            // C:\Projects\celsa
            //            var files = Directory.GetFiles(@" D:\projects\dynamaster6\", "*.cs", SearchOption.AllDirectories)

            Stopwatch stopwatch = Stopwatch.StartNew();

            var files = Directory.GetFiles(@"C:\Projects\celsa\", "*.cs", SearchOption.AllDirectories)
                .Where(
                f => !f.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase)
                && !f.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase)
                && !f.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
                && !f.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

            Console.WriteLine("Loading files....");

            var loadFileStopwatch = Stopwatch.StartNew();

            var codeFiles = await loader.LoadCodeFiles(files, Encoding.Default);

            var masterDictionary = new Dictionary<int, ThinList<CodeLine>>();

            foreach (var file in codeFiles)
            {
                foreach (var line in file.CodeLines)
                {
                    line.CodeFile = file;
                }

                var keys = file.CodeLinesDictionary.Keys;
                var values = file.CodeLinesDictionary.ValuesArray;

                var length = file.CodeLinesDictionary.Length;

                for (int i = 0; i < length; i++)
                {
                    var key = keys[i];
                    var value = values[i];
                    if (value == null)
                    {
                        continue;
                    }

                    ThinList<CodeLine> list;

                    if (!masterDictionary.TryGetValue(key, out list))
                    {
                        list =  new ThinList<CodeLine>(50);
                        masterDictionary.Add(key, list);
                    }

                    if (list.Capacity < list.length + value.length)
                    {
                        list.Resize(list.Capacity + value.length);
                    }

                    list.AddRange(value.AsCollection());
                }
            }

            var realMasterDictionary = new DivideAndConquerDictionary<ThinList<CodeLine>>(masterDictionary);

            Console.WriteLine("Loaded " + codeFiles.Count + " files, with a total of " + codeFiles.Sum(cf => cf.CodeLines.Length) + " lines...");

            loadFileStopwatch.Stop();

            Console.WriteLine("Load files took " + loadFileStopwatch.ElapsedMilliseconds + "ms");

            var blockStopwatch = Stopwatch.StartNew();
            Console.WriteLine("Finding duplicate blocks..");

            var codeMatches = (await codeFileComparer.GetMatchesAsync(5, codeFiles, realMasterDictionary, 8)).OrderByDescending(c => c.CodeFileMatches.Count * c.LineCount).ThenBy(c => c.LineSummary).ToList();
            //var codeMatches = (await codeFileComparer.GetMatchesAsync(5, codeFiles)).OrderBy(c => c.LineSummary).ToList();

            var json = JsonConvert.SerializeObject(codeMatches, Formatting.Indented);

            System.IO.File.WriteAllText(@"D:\Temp\CodeRedundancyDebugHelp\" + DateTime.Now.ToString("yyMMdd HHmmss") + ".json", json);

            var commenter = new CodeFileMatchCommenter(new CodeFileLineIndexer());
            Console.WriteLine("Finding duplicate blocks took " + blockStopwatch.ElapsedMilliseconds + "ms");
            stopwatch.Stop();
            Console.WriteLine("Time: " + stopwatch.ElapsedMilliseconds);


            Console.WriteLine();
            Console.WriteLine(codeMatches.Count + " blocks found");


            if (false)
            {
                Console.WriteLine();
                Console.WriteLine("Top 10 blocks: ");

                foreach (var block in codeMatches.Take(10))
                {
                    Console.WriteLine("Block: " + block.UniqueId + ", Number of lines: " + block.LineCount + ", matches: " + block.CodeFileMatches.Count + ", first 5 lines:");

                    foreach (var line in block.MatchingCodeLines.Take(5))
                    {
                        Console.WriteLine(">> " + line.OriginalLineText);
                    }

                    Console.WriteLine();
                }
            }

            // Console.ReadKey(false);

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
