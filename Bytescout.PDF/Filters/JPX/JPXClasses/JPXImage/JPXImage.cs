using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JPXImage
    {
        public JPXImage()
        {
            m_colorSpec = new JPXColorSpec();
        }

        public JPXColorSpec ColorSpec
        {
            get { return m_colorSpec; }
        }

        public int XSize
        {
            get { return m_xSize; }
            set { m_xSize = value; }
        }

        public int YSize
        {
            get { return m_ySize; }
            set { m_ySize = value; }
        }

        public int XOffset
        {
            get { return m_xOffset; }
            set { m_xOffset = value; }
        }

        public int YOffset
        {
            get { return m_yOffset; }
            set { m_yOffset = value; }
        }

        public int XTileSize
        {
            get { return m_xTileSize; }
            set { m_xTileSize = value; }
        }

        public int YTileSize
        {
            get { return m_yTileSize; }
            set { m_yTileSize = value; }
        }

        public int XTileOffset
        {
            get { return m_xTileOffset; }
            set { m_xTileOffset = value; }
        }

        public int YTileOffset
        {
            get { return m_yTileOffset; }
            set { m_yTileOffset = value; }
        }

        public int NComps
        {
            get { return m_nComps; }
            set { m_nComps = value; }
        }

        public int NXTiles
        {
            get 
            { 
                return (XSize - XTileOffset + XTileSize - 1) / XTileSize; 
            }
        }

        public int NYTiles
        {
            get 
            {
                return (YSize - YTileOffset + YTileSize - 1) / YTileSize; 
            }
        }

        public JPXTile[] TileCollection
        {
            get { return m_tileCollection; }
        }

        public void SetImageAndTileSize(int xSize,
            int ySize,
            int xOffset,
            int yOffset,
            int xTileSize,
            int yTileSize,
            int xTileOffset,
            int yTileOffset,
            int nComps)
        {
            m_xSize = xSize;
            m_ySize = ySize;
            m_xOffset = xOffset;
            m_yOffset = yOffset;
            m_xTileSize = xTileSize;
            m_yTileSize = yTileSize;
            m_xTileOffset = xTileOffset;
            m_yTileOffset = yTileOffset;
            m_nComps = nComps;
        }

        public void CreateTileCollection(int size)
        {
            m_tileCollection = new JPXTile[size];

            for (int i = 0; i < size; ++i)
                m_tileCollection[i] = new JPXTile(NComps);
        }

        private JPXColorSpec m_colorSpec;
        private int m_xSize;
        private int m_ySize;
        private int m_xOffset;
        private int m_yOffset;
        private int m_xTileSize;
        private int m_yTileSize;
        private int m_xTileOffset;
        private int m_yTileOffset;
        private int m_nComps;

        private JPXTile[] m_tileCollection;
    }
}
