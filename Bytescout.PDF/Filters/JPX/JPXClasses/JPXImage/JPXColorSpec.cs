using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JPXColorSpec
    {
        public JPXColorSpec()
        {
            m_enumerated = new JPXColorSpecEnumerated();
        }

        public int Meth
        {
            get { return m_meth; }
            set { m_meth = value; }
        }

        public int Prec
        {
            get { return m_prec; }
            set { m_prec = value; }
        }

        public JPXColorSpecEnumerated Enumerated
        {
            get { return m_enumerated; }
            set { m_enumerated = value; }
        }

        private int m_meth;			// method
        private int m_prec;			// precedence
        private JPXColorSpecEnumerated m_enumerated;
    }

    internal class JPXColorSpecEnumerated
    {
        public JPXColorSpecEnumerated()
        {
            m_cieLab = new JPXColorSpecCIELab();
        }

        public JPXColorSpaceType Type
        {
            get { return m_type; }
            set { m_type = value; }
        }

        public JPXColorSpecCIELab CieLab
        {
            get { return m_cieLab; }
            set { m_cieLab = value; }
        }
        

        private JPXColorSpaceType m_type;	// color space type
        private JPXColorSpecCIELab m_cieLab;
    }

    internal class JPXColorSpecCIELab
    {
        public int rl, ol, ra, oa, rb, ob, il;
    }

    enum JPXColorSpaceType
    {
        jpxCSBiLevel = 0,
        jpxCSYCbCr1 = 1,
        jpxCSYCbCr2 = 3,
        jpxCSYCBCr3 = 4,
        jpxCSPhotoYCC = 9,
        jpxCSCMY = 11,
        jpxCSCMYK = 12,
        jpxCSYCCK = 13,
        jpxCSCIELab = 14,
        jpxCSsRGB = 16,
        jpxCSGrayscale = 17,
        jpxCSBiLevel2 = 18,
        jpxCSCIEJab = 19,
        jpxCSCISesRGB = 20,
        jpxCSROMMRGB = 21,
        jpxCSsRGBYCbCr = 22,
        jpxCSYPbPr1125 = 23,
        jpxCSYPbPr1250 = 24
    }
}
