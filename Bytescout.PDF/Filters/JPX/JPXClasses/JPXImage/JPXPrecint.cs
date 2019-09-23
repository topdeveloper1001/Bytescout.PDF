using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JPXPrecint : ICloneable
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

        public JPXSubband[] Subbands
        {
            get { return m_subbands; }
            set { m_subbands = value; }
        }

        public Object Clone()
        {
            JPXPrecint precint = new JPXPrecint();
            precint.Subbands = (JPXSubband[])m_subbands.Clone();
            precint.m_x0 = m_x0;
            precint.m_x1 = m_x1;
            precint.m_y0 = m_y0;
            precint.m_y1 = m_y1;
            return precint;
        }

        private int m_x0;
        private int m_x1;
        private int m_y0;
        private int m_y1;

        private JPXSubband[] m_subbands;
    }
}
