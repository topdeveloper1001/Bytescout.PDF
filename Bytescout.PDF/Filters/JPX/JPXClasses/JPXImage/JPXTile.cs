using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JPXTile
    {
        public JPXTile(int nComps)
        {
            m_tileCompCollection = new JPXTileComp[nComps];
            for (int i = 0; i < nComps; ++i)
                m_tileCompCollection[i] = new JPXTileComp();
            m_layer = m_precint = m_comp = m_res = 0;
        }

        public JPXTileComp[] TileCompCollection
        {
            get { return m_tileCompCollection; }
        }

        public int ProgressionOrder
        {
            get { return m_progOrder; }
            set { m_progOrder = value; }
        }

        public int NLayers
        {
            get { return m_nLayers; }
            set { m_nLayers = value; }
        }

        public int MultiComponent
        {
            get { return m_multiComp; }
            set { m_multiComp = value; }
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

        public int MaxNDecompositionLevel
        {
            get { return m_maxNDecompLevel; }
            set { m_maxNDecompLevel = value; }
        }

        public int Component
        {
            get { return m_comp; }
            set { m_comp = value; }
        }

        public int Resolution
        {
            get { return m_res; }
            set { m_res = value; }
        }

        public int Precint
        {
            get { return m_precint; }
            set { m_precint = value; }
        }

        public int Layer
        {
            get { return m_layer; }
            set { m_layer = value; }
        }

        private JPXTileComp[] m_tileCompCollection;

        private int m_progOrder;
        private int m_nLayers;
        private int m_multiComp;

        private int m_x0;
        private int m_x1;
        private int m_y0;
        private int m_y1;

        private int m_maxNDecompLevel;

        private int m_comp;
        private int m_res;
        private int m_precint;
        private int m_layer;
    }
}
