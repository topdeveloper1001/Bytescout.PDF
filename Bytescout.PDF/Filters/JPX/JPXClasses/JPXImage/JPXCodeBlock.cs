using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal enum PassMode
    {
        jpxPassSigProp = 0,
        jpxPassMagRef = 1,
        jpxPassCleanup = 2
    }

    internal enum ContextSize
    {
        jpxNContexts = 19,

        jpxContextSigProp = 0,	// 0 - 8: significance prop and cleanup
        jpxContextSign = 9,	// 9 - 13: sign
        jpxContextMagRef = 14,	// 14 -16: magnitude refinement
        jpxContextRunLength = 17,	// cleanup: run length
        jpxContextUniform = 18	// cleanup: first signif coeff
    }

    internal class JPXCodeBlock : ICloneable
    {
        public int X0
        {
            get { return m_x0; }
            set { m_x0 = value; }
        }

        public int X1
        {
            get { return m_x1; }
            set { m_x1 = value; }
        }

        public int Y0
        {
            get { return m_y0; }
            set { m_y0 = value; }
        }

        public int Y1
        {
            get { return m_y1; }
            set { m_y1 = value; }
        }

        public bool Seen
        {
            get { return m_seen; }
            set { m_seen = value; }
        }

        public int LengthBlock
        {
            get { return m_lBlock; }
            set { m_lBlock = value; }
        }

        public PassMode NextPass
        {
            get { return m_nextPass; }
            set { m_nextPass = value; }
        }

        public int NZeroBitPlanes
        {
            get { return m_nZeroBitPlanes; }
            set { m_nZeroBitPlanes = value; }
        }

        public JPXCoeff[] Coeff
        {
            get { return m_coeff; }
            set { m_coeff = value; }
        }

        public int Included
        {
            get { return m_included; }
            set { m_included = value; }
        }

        public int NCodingPass
        {
            get { return m_nCodingPass; }
            set { m_nCodingPass = value; }
        }

        public int DataLength
        {
            get { return m_dataLength; }
            set { m_dataLength = value; }
        }

        public JArithmeticDecoder ArithmeticDecoder
        {
            get { return m_arithDecoder; }
            set { m_arithDecoder = value; }
        }

        public JArithmeticDecoderStats ArithmeticDecoderStats
        {
            get { return m_arithDecoderStats; }
            set { m_arithDecoderStats = value; }
        }

        public Object Clone()
        {
            JPXCodeBlock codeblock = new JPXCodeBlock();
            codeblock.m_x0 = m_x0;
            codeblock.m_x1 = m_x1;
            codeblock.m_y0 = m_y0;
            codeblock.m_y1 = m_y1;

            codeblock.m_coeff = (JPXCoeff[])m_coeff.Clone();

            codeblock.m_included = m_included;
            codeblock.m_nCodingPass = m_nCodingPass;
            codeblock.m_dataLength = m_dataLength;
            codeblock.m_nZeroBitPlanes = m_nZeroBitPlanes;
            codeblock.m_nextPass = m_nextPass;
            codeblock.m_lBlock = m_lBlock;
            codeblock.m_seen = m_seen;
            return codeblock;
        }

        private int m_x0;
        private int m_x1;
        private int m_y0;
        private int m_y1;

        //----- persistent state
        private bool m_seen;			// true if this code-block has already
        //   been seen
        private int m_lBlock;			// base number of bits used for pkt data length
        private PassMode m_nextPass;		// next coding pass

        //---- info from first packet
        private int m_nZeroBitPlanes;		// number of zero bit planes

        private JPXCoeff[] m_coeff;

        private int m_included;
        private int m_nCodingPass;
        private int m_dataLength;

        private JArithmeticDecoder m_arithDecoder;
        private JArithmeticDecoderStats m_arithDecoderStats;
    }
}
