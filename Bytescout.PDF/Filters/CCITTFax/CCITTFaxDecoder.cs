using System.IO;

namespace Bytescout.PDF
{
    internal class CCITTFaxDecoder
    {
        public static MemoryStream Decode(Stream stream, PDFDictionary decodeParms)
        {
            MemoryStream result = new MemoryStream();
            CCITTFaxStream ccittFax = new CCITTFaxStream(stream, new CCITTFaxParameters(decodeParms));
            byte[] buf = new byte[4096];
            int numRead;
            while ((numRead = ccittFax.Read(buf, 0, buf.Length)) > 0)
                result.Write(buf, 0, numRead);
            return result;
        }
    }
}
