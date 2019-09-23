using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JPXResLevel : ICloneable
    {
        public JPXResLevel()
        {
            m_bx0 = new int[3];
            m_bx1 = new int[3];
            m_by0 = new int[3];
            m_by1 = new int[3];
        }

        public int PrecintW
        {
            get { return m_precintW; }
            set { m_precintW = value; }
        }

        public int PrecintH
        {
            get { return m_precintH; }
            set { m_precintH = value; }
        }

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

        public int[] BX0
        {
            get { return m_bx0; }
        }

        public int[] BX1
        {
            get { return m_bx1; }
        }

        public int[] BY0
        {
            get { return m_by0; }
        }

        public int[] BY1
        {
            get { return m_by1; }
        }

        public JPXPrecint Precint
        {
            get { return m_precint; }
            set { m_precint = value; }
        }

        public Object Clone()
        {
            JPXResLevel resLevel = new JPXResLevel();
            resLevel.m_precintW = m_precintW;
            resLevel.m_precintH = m_precintH;
            resLevel.m_x0 = m_x0;
            resLevel.m_x1 = m_x1;
            resLevel.m_y0 = m_y0;
            resLevel.m_y1 = m_y1;

            resLevel.m_bx0 = m_bx0;
            resLevel.m_bx1 = m_bx1;
            resLevel.m_by0 = m_by0;
            resLevel.m_by1 = m_by1;

            if (m_precint != null)
                resLevel.m_precint = (JPXPrecint)m_precint.Clone();
            return resLevel;
        }

        private int m_x0;
        private int m_x1;
        private int m_y0;
        private int m_y1;

        private int[] m_bx0;
        private int[] m_bx1;
        private int[] m_by0;
        private int[] m_by1;

        private int m_precintW;
        private int m_precintH;
        private JPXPrecint m_precint;
    }
}
