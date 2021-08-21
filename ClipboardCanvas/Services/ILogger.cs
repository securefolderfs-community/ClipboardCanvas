using System.Threading.Tasks;

namespace ClipboardCanvas.Services
{
    public interface ILogger
    {
        void LogToFile(string text);
    }
}
