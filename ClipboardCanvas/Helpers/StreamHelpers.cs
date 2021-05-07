using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ClipboardCanvas.Helpers
{
    public static class StreamHelpers
    {
        public static Stream CreateStreamFromString(string str)
        {
            byte[] byteArray = Encoding.Unicode.GetBytes(str);
            MemoryStream memoryStream = new MemoryStream(byteArray);

            return memoryStream;
        }
    }
}
