using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CodeRedundancyCheck.Interface
{
    public interface ICodeFileLoader
    {
        Task<CodeFile> LoadCodeFileAsync(Stream codeFileStream, Encoding encoding, bool leaveStreamOpen = false);
    }
}