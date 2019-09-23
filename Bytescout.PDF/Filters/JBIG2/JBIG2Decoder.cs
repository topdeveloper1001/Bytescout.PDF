using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class JBIG2Decoder
    {
        public static MemoryStream Decode(Stream stream, PDFDictionary decodeParms)
        {
            MemoryStream decoded = new MemoryStream();
            JBig2Stream jbig2 = new JBig2Stream(stream, new JBIG2Parameters(decodeParms).Stream);
            byte[] buf = new byte[4096];
            int numRead;
            while ((numRead = jbig2.Read(buf, 0, buf.Length)) > 0)
                decoded.Write(buf, 0, numRead);
            return decoded;
        }
    }
}
