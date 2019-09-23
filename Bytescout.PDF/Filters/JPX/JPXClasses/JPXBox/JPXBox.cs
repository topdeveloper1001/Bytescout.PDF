using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal enum JPXBoxType
    {
        JP2_BOX_JP = 0x6a502020,	/* Signature */
        JP2_BOX_FTYP = 0x66747970,	/* File Type */
        JP2_BOX_JP2H = 0x6a703268,	/* JP2 Header */
        JP2_BOX_IHDR = 0x69686472,	/* Image Header */
        JP2_BOX_BPCC = 0x62706363,	/* Bits Per Component */
        JP2_BOX_COLR = 0x636f6c72,	/* Color Specification */
        JP2_BOX_PCLR = 0x70636c72,	/* Palette */
        JP2_BOX_CMAP = 0x636d6170,	/* Component Mapping */
        JP2_BOX_CDEF = 0x63646566,	/* Channel Definition */
        JP2_BOX_RES = 0x72657320,	/* Resolution */
        JP2_BOX_RESC = 0x72657363,	/* Capture Resolution */
        JP2_BOX_RESD = 0x72657364,	/* Default Display Resolution */
        JP2_BOX_JP2C = 0x6a703263,	/* Contiguous Code Stream */
        JP2_BOX_JP2I = 0x6a703269,	/* Intellectual Property */
        JP2_BOX_XML = 0x786d6c20,	/* XML */
        JP2_BOX_UUID = 0x75756964,	/* UUID */
        JP2_BOX_UINF = 0x75696e66,	/* UUID Info */
        JP2_BOX_ULST = 0x75637374,	/* UUID List */
        JP2_BOX_URL = 0x75726c20,	/* URL */

        JP2_BOX_SUPER = 0x01,
        JP2_BOX_NODATA = 0x02
    }

    internal class JPXBox
    {
        public JPXBox()
        {
        }

        public bool CreateBox(Stream stream)
        {
            m_datalength = 0;
            m_data = null;

            int type = 0;
            if (!JPXUtilities.GetInt32(stream, ref m_length) || !JPXUtilities.GetInt32(stream, ref type))
                return false;

            m_type = (JPXBoxType)type;

            if (m_length == 1)
            {
                if (!JPXUtilities.GetInt64(stream, ref m_length))
                    return false;

                m_datalength = m_length - 16;
            }
            else if (m_length == 0)
                m_datalength = 0;
            else
                m_datalength = m_length - 8;

            if (dataFlags(m_type) == 0)
            {
                if(!getDataFromStream(stream, (int)m_datalength))
                    return false;
            }

            return true;
        }

        public JPXBoxType Type
        {
            get { return m_type; }
        }

        public MemoryStream Data
        {
            get { return m_data; }
        }

        public int DataLength
        {
            get { return m_datalength; }
        }

        private int dataFlags(JPXBoxType type)
        {
            switch (type)
            {
                case JPXBoxType.JP2_BOX_JP2H:
                case JPXBoxType.JP2_BOX_RES:
                case JPXBoxType.JP2_BOX_UINF:
                    return (int)JPXBoxType.JP2_BOX_SUPER;
                case JPXBoxType.JP2_BOX_JP2C:
                    return (int)JPXBoxType.JP2_BOX_NODATA;
                default:
                    return 0;
            }
        }

        private bool getDataFromStream(Stream stream, int len)
        {
            if (stream.Length - stream.Position < len)
                return false;

            byte[] buffer = new byte[len];
            stream.Read(buffer, 0, len);

            m_data = new MemoryStream();
            m_data.Write(buffer, 0, buffer.Length);
            m_data.Position = 0;

            return true;
        }

        private int m_length;
        private int m_datalength;
        private MemoryStream m_data;
        private JPXBoxType m_type;
    }
}
