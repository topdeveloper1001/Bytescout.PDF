using System.IO;

namespace Bytescout.PDF
{
    internal static class LZWDecoder
    {
        public static MemoryStream Decode(Stream stream, PDFDictionary decodeParms)
        {
            MemoryStream decoded = new MemoryStream();
            LZWStream lzw = new LZWStream(stream, new PredictorParameters(decodeParms), new LZWParameters(decodeParms));
            byte[] buf = new byte[4096];
            int numRead;
            while ((numRead = lzw.Read(buf, 0, buf.Length)) > 0)
                decoded.Write(buf, 0, numRead);
            return decoded;
        }
    }
}
