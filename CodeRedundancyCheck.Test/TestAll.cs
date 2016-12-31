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

            var matches = (await indexer.GetMatchesAsync(2, sourceFile)).OrderByDescending(c => c.LineCount).ToList();
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
