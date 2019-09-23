using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class JPXDecoder
    {
        public static MemoryStream Decode(Stream stream)
        {
            JPXStreamReader jpx = new JPXStreamReader(stream);
            return jpx.Stream;
        }
    }
}
