using System.IO;

namespace Bytescout.PDF
{
    internal class ASCIIHexDecoder
    {
        public static MemoryStream Decode(Stream stream)
        {
            MemoryStream result = new MemoryStream();
            ASCIIHexStream hexStream = new ASCIIHexStream(stream);
            byte[] buf = new byte[4096];
            int numRead;
            while ((numRead = hexStream.Read(buf, 0, buf.Length)) > 0)
                result.Write(buf, 0, numRead);
            return result;
        }
    }
}
