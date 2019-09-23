using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal struct JBIG2Parameters
    {
        public JBIG2Parameters(PDFDictionary dict)
        {
            m_stream = null;
            if (dict == null)
                return;

            PDFDictionaryStream dictStream = dict["JBIG2Globals"] as PDFDictionaryStream;
            if (dictStream == null)
                return;

            dictStream.Decode();
            m_stream = dictStream.GetStream();
        }

        public Stream Stream { get { return m_stream; } }

        private MemoryStream m_stream;
    }
}
