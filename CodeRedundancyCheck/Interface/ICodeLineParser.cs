using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeRedundancyCheck.Model;

namespace CodeRedundancyCheck.Interface
{
    public interface ICodeLineParser
    {
    }

    public class CodeFileParserResult
    {

        public CodeLineMeaning Meaning { get; set; }

        public MethodDefinition MethodDefinition { get; set; }
        
               
    }
}
