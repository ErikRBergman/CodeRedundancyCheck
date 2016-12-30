using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeRedundancyCheck.WinForms.UI
{
    using System.IO;

    using CodeRedundancyCheck.Languages.CSharp;

    public partial class ResultForm : Form
    {
        public ResultForm()
        {
            InitializeComponent();
        }

        private void ResultForm_Load(object sender, EventArgs e)
        {
            var duplicates = this.GetDuplicatesAsync().Result;

            foreach (var dupe in duplicates)
            {
                var pathParts = dupe.Key.Filename.Split('\\');

                var container = this.treeView1.Nodes;

                foreach (var part in pathParts)
                {
                    var node = container.Find(part, false);

                    TreeNode currentNode = null;

                    if (node == null || node.Length == 0)
                    {
                        currentNode = container.Add(part, part);
                    }
                    else
                    {
                        currentNode = node[0];
                    }

                }

            }

        }

        private async Task<IReadOnlyCollection<IGrouping<CodeFile, ResultMatch>>> GetDuplicatesAsync()
        {
            var codeFileComparer = new CodeFileComparer();

            var loader = new CodeFileLoader(new CSharpSourceWash(), new CodeFileIndexer(), new CodeFileLineIndexer(), CSharpCodeLineFilter.Singleton);
            codeFileComparer.CodeLineFilter = CSharpCodeLineFilter.Singleton;

            var files = Directory.GetFiles(@"C:\projects\Celsa\QR\Trunk\", "*.cs", SearchOption.AllDirectories);

            var codeFiles = new List<CodeFile>(files.Length);

            foreach (var filename in files)
            {
                var file = await loader.LoadCodeFileAsync(File.OpenRead(filename), Encoding.Default);
                file.Filename = filename;
                codeFiles.Add(file);
            }

            var codeMatches = (await codeFileComparer.GetMatchesAsync(5, codeFiles)).OrderByDescending(c => c.Lines * c.CodeFileMatches.Count).ToList();
            var commenter = new CodeFileMatchCommenter(new CodeFileLineIndexer());

            var commentedMatches = new HashSet<CodeFile>();

            var result = codeMatches.Where(m => m.CodeFileMatches.Count(m2 => m2.CodeFile.Filename.EndsWith("SpecificationController.cs", StringComparison.OrdinalIgnoreCase)) > 1).SelectMany(
                codeMatch => codeMatch.CodeFileMatches.Select(codeFileMatch => new ResultMatch
                                                                               {
                                                                                   CodeMatch = codeMatch,
                                                                                   CodeFileMatch = codeFileMatch
                                                                               })).GroupBy(m => m.CodeFileMatch.CodeFile);

            return result.ToList() as IReadOnlyCollection<IGrouping<CodeFile, ResultMatch>>;

        }
    }

    internal class ResultMatch
    {
        public CodeMatch CodeMatch { get; set; }

        public CodeFileMatch CodeFileMatch { get; set; }
    }
}
