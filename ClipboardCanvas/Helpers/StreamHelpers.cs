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

        public static byte[] ReadAll(this Stream input, int streamLength)
        {
            byte[] result;
            //using (Stream stream = input)
            {
                var memoryStream = new MemoryStream();
                    input.CopyTo(memoryStream);
                    result = memoryStream.ToArray();
            }

            return result;
        }
    }
}
