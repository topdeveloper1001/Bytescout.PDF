using System.IO;

namespace Bytescout.PDF
{
    internal class FlateDecoder
    {
        public static void Code(Stream input, Stream output)
        {
            ZOutputStream outputStream = new ZOutputStream(output, -1);
            byte[] buf = new byte[4096];
            int numRead;
            while ((numRead = input.Read(buf, 0, buf.Length)) > 0)
                outputStream.Write(buf, 0, numRead);
            outputStream.finish();
        }

        public static MemoryStream Decode(Stream stream, PDFDictionary decodeParms)
        {
            MemoryStream decoded = new MemoryStream();
            FlateStream flate = new FlateStream(stream, new PredictorParameters(decodeParms));
            byte[] buf = new byte[4096];
            int numRead;
            while ((numRead = flate.Read(buf, 0, buf.Length)) > 0)
                decoded.Write(buf, 0, numRead);
            return decoded;
        }
    }
}
