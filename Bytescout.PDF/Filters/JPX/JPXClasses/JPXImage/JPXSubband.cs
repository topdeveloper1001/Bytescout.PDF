using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal struct JPXTagTreeNode
    {
        public bool IsFinished;
        public int Value;
    }

    internal class JPXSubband : ICloneable
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

        public int NXCBs
        {
            get { return m_nXCBs; }
            set { m_nXCBs = value; }
        }

        public int NYCBs
        {
            get { return m_nYCBs; }
            set { m_nYCBs = value; }
        }

        public int MaxTTLevel
        {
            get { return m_maxTTLevel; }
            set { m_maxTTLevel = value; }
        }

        public JPXTagTreeNode[] Inclusion
        {
            get { return m_inclusion; }
            set { m_inclusion = value; }
        }

        public JPXTagTreeNode[] ZeroBitPlane
        {
            get { return m_zeroBitPlane; }
            set { m_zeroBitPlane = value; }
        }

        public JPXCodeBlock[] CodeBlocks
        {
            get { return m_codeBlocks; }
            set { m_codeBlocks = value; }
        }

        public Object Clone()
        {
            JPXSubband subband = new JPXSubband();
            //subband = this;
            subband.m_x0 = m_x0;
            subband.m_x1 = m_x1;
            subband.m_y0 = m_y0;
            subband.m_y1 = m_y1;

            subband.m_nXCBs = m_nXCBs;
            subband.m_nYCBs = m_nYCBs;

            subband.m_maxTTLevel = m_maxTTLevel;

            subband.Inclusion = (JPXTagTreeNode[])m_inclusion.Clone();
            subband.ZeroBitPlane = (JPXTagTreeNode[])m_zeroBitPlane.Clone();
            subband.CodeBlocks = (JPXCodeBlock[])m_codeBlocks.Clone();
            return subband;
        }

        private int m_x0;
        private int m_x1;
        private int m_y0;
        private int m_y1;

        private int m_nXCBs;
        private int m_nYCBs;

        private int m_maxTTLevel;

        private JPXTagTreeNode[] m_inclusion;
        private JPXTagTreeNode[] m_zeroBitPlane;

        private JPXCodeBlock[] m_codeBlocks;
    }
}
