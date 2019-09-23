using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class JBig2Stream : FilterStream
    {
        internal JBig2Stream(Stream inputStream, Stream global)
        {
            JBIG2InputStream input = new JBIG2InputStream(inputStream, global);
            m_position = 0;
            m_data = input.GetData();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (m_position == m_data.Length)
                return 0;
            int numRead = Math.Min(count, m_data.Length - m_position);
            for (int i = 0; i < numRead; ++i)
                buffer[offset++] = (byte)((m_data[m_position++] ^ 0xff) & 0xff);
            return numRead;
        }

        public override int ReadByte()
        {
            if (m_position == m_data.Length)
                return -1;
            return (m_data[m_position++] ^ 0xff) & 0xff;
        }

        private byte[] m_data;
        private int m_position;
    }
}
