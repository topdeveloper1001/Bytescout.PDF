using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JPXTileComp : ICloneable
    {
        public int Precision
        {
            get { return m_precision; }
            set { m_precision = value; }
        }

        public int HSeparation
        {
            get { return m_hSeparation; }
            set { m_hSeparation = value; }
        }

        public int VSeparation
        {
            get { return m_vSeparation; }
            set { m_vSeparation = value; }
        }

        public int Style
        {
            get { return m_style; }
            set { m_style = value; }
        }

        public int NDecompositionLevel
        {
            get { return m_nDecomp; }
            set { m_nDecomp = value; }
        }

        public int CodeBlockW
        {
            get { return m_codeBlockW; }
            set { m_codeBlockW = value; }
        }

        public int CodeBlockH
        {
            get { return m_codeBlockH; }
            set { m_codeBlockH = value; }
        }

        public int CodeBlockStyle
        {
            get { return m_codeBlockStyle; }
            set { m_codeBlockStyle = value; }
        }

        public int Transform
        {
            get { return m_transform; }
            set { m_transform = value; }
        }

        public int[] QuantSteps
        {
            get { return m_quantSteps; }
            set { m_quantSteps = (int[])value.Clone(); }
        }

        public int QuantStyle
        {
            get { return m_quantStyle; }
            set { m_quantStyle = value; }
        }

        public JPXResLevel[] ResLevelCollection
        {
            get { return m_resLevelCollection; }
            set { m_resLevelCollection = value; }
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

        public int[] Data
        {
            get { return m_data; }
            set { m_data = value; }
        }

        public int[] Buffer
        {
            get { return m_buf; }
            set { m_buf = value; }
        }

        public bool Signed
        {
            get { return m_signed; }
            set { m_signed = value; }
        }

        public Object Clone()
        {
            JPXTileComp tileComp = new JPXTileComp();
            tileComp.m_precision = m_precision;
            tileComp.m_hSeparation = m_hSeparation;
            tileComp.m_vSeparation = m_vSeparation;
            tileComp.m_style = m_style;
            tileComp.m_nDecomp = m_nDecomp;
            tileComp.m_codeBlockW = m_codeBlockW;
            tileComp.m_codeBlockH = m_codeBlockH;
            tileComp.m_codeBlockStyle = m_codeBlockStyle;
            tileComp.m_transform = m_transform;

            if (m_resLevelCollection != null)
            {
                tileComp.m_resLevelCollection = new JPXResLevel[m_resLevelCollection.Length];
                for (int i = 0; i < m_resLevelCollection.Length; ++i)
                    tileComp.m_resLevelCollection[i] = (JPXResLevel)m_resLevelCollection[i].Clone();
            }

            if (m_quantSteps != null) tileComp.m_quantSteps = (int[])m_quantSteps.Clone();
            if (m_data != null) tileComp.m_data = (int[])m_data.Clone();
            if (m_buf != null) tileComp.m_buf = (int[])m_buf.Clone();
            return tileComp;
        }

        private int m_precision;
        private int m_hSeparation;
        private int m_vSeparation;
        private int m_style;
        private int m_nDecomp;
        private int m_codeBlockW;
        private int m_codeBlockH;
        private int m_codeBlockStyle;
        private int m_transform;
        private bool m_signed;

        private JPXResLevel[] m_resLevelCollection;

        private int[] m_quantSteps;
        private int m_quantStyle;

        private int m_x0;
        private int m_x1;
        private int m_y0;
        private int m_y1;

        private int[] m_data;
        private int[] m_buf;
    }
}
