using System.IO;

namespace Bytescout.PDF
{
    internal class RunLengthDecoder
    {
        public static MemoryStream Decode(Stream stream)
        {
            MemoryStream decoded = new MemoryStream();
            RunLengthStream runLength = new RunLengthStream(stream);
            byte[] buf = new byte[4096];
            int numRead;
            while ((numRead = runLength.Read(buf, 0, buf.Length)) > 0)
                decoded.Write(buf, 0, numRead);
            return decoded;
        }
    }
}
