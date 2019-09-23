using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace Bytescout.PDF
{
    internal class JBIG2InputStream
    {
        public JBIG2InputStream(Stream strA, Stream global)
        {
            m_pageBitmap = null;

            m_arithDecoder = new JArithmeticDecoder();
            m_genericRegionStats = new JArithmeticDecoderStats(1 << 1);
            m_refinementRegionStats = new JArithmeticDecoderStats(1 << 1);
            m_iadhStats = new JArithmeticDecoderStats(1 << 9);
            m_iadwStats = new JArithmeticDecoderStats(1 << 9);
            m_iaexStats = new JArithmeticDecoderStats(1 << 9);
            m_iaaiStats = new JArithmeticDecoderStats(1 << 9);
            m_iadtStats = new JArithmeticDecoderStats(1 << 9);
            m_iaitStats = new JArithmeticDecoderStats(1 << 9);
            m_iafsStats = new JArithmeticDecoderStats(1 << 9);
            m_iadsStats = new JArithmeticDecoderStats(1 << 9);
            m_iardxStats = new JArithmeticDecoderStats(1 << 9);
            m_iardyStats = new JArithmeticDecoderStats(1 << 9);
            m_iardwStats = new JArithmeticDecoderStats(1 << 9);
            m_iardhStats = new JArithmeticDecoderStats(1 << 9);
            m_iariStats = new JArithmeticDecoderStats(1 << 9);
            m_iaidStats = new JArithmeticDecoderStats(1 << 1);
            m_huffDecoder = new JBIG2HuffmanDecoder();
            m_mmrDecoder = new JBIG2MMRDecoder();

            m_segments = null;

            m_str = strA;
            m_globalStr = global;

            m_dataPtr = null;

            reset();
        }

        public byte[] GetData()
        {
            return m_dataPtr;
        }

        private void reset()
        {
            m_globalSegments = new List<Object>();
            if (m_globalStr != null)
            {
                m_segments = m_globalSegments;
                m_curStr = m_globalStr;

                m_arithDecoder.SetStream(m_curStr);
                m_huffDecoder.SetStream(m_curStr);
                m_mmrDecoder.SetStream(m_curStr);
                readSegments();
            }

            m_segments = new List<Object>();
            m_curStr = m_str;

            m_arithDecoder.SetStream(m_curStr);
            m_huffDecoder.SetStream(m_curStr);
            m_mmrDecoder.SetStream(m_curStr);
            readSegments();

            if (m_pageBitmap != null)
            {
                m_dataPtr = m_pageBitmap.GetDataPtr();
            }
        }

        private void readSegments()
        {
            uint segNum = 0, segFlags = 0, segType = 0, page = 0, segLength = 0;
            uint refFlags = 0, nRefSegs = 0;
            uint[] refSegs = null;
            
            while (readULong(ref segNum))
            {
                prepareSegmentType(ref segType, ref segNum, ref segLength, ref segFlags, ref refFlags,
                    ref refSegs, ref nRefSegs, ref page);
                // read the segment data
                switch (segType)
                {
                    case 0:
                        if (!readSymbolDictSeg(segNum, segLength, refSegs, nRefSegs))
                            return;
                        break;
                    case 4:
                        readTextRegionSeg(segNum, false, false, segLength, refSegs, nRefSegs);
                        break;
                    case 6:
                        readTextRegionSeg(segNum, true, false, segLength, refSegs, nRefSegs);
                        break;
                    case 7:
                        readTextRegionSeg(segNum, true, true, segLength, refSegs, nRefSegs);
                        break;
                    case 16:
                        readPatternDictSeg(segNum, segLength);
                        break;
                    case 20:
                        readHalftoneRegionSeg(segNum, false, false, segLength,
                                  refSegs, nRefSegs);
                        break;
                    case 22:
                        readHalftoneRegionSeg(segNum, true, false, segLength,
                                  refSegs, nRefSegs);
                        break;
                    case 23:
                        readHalftoneRegionSeg(segNum, true, true, segLength,
                                  refSegs, nRefSegs);
                        break;
                    case 36:
                        readGenericRegionSeg(segNum, false, false, segLength);
                        break;
                    case 38:
                        readGenericRegionSeg(segNum, true, false, segLength);
                        break;
                    case 39:
                        readGenericRegionSeg(segNum, true, true, segLength);
                        break;
                    case 40:
                        readGenericRefinementRegionSeg(segNum, false, false, segLength,
                                       refSegs, nRefSegs);
                        break;
                    case 42:
                        readGenericRefinementRegionSeg(segNum, true, false, segLength,
                                       refSegs, nRefSegs);
                        break;
                    case 43:
                        readGenericRefinementRegionSeg(segNum, true, true, segLength,
                                       refSegs, nRefSegs);
                        break;
                    case 48:
                        readPageInfoSeg(segLength);
                        break;
                    case 50:
                        readEndOfStripeSeg(segLength);
                        break;
                    case 52:
                        readProfilesSeg(segLength);
                        break;
                    case 53:
                        readCodeTableSeg(segNum, segLength);
                        break;
                    case 62:
                        readExtensionSeg(segLength);
                        break;
                    default:
                        for (uint i = 0; i < segLength; ++i)
                        {
                            if (m_curStr.ReadByte() == -1)
                            {
                                return;
                            }
                        }
                        break;
                }
            }
            return;
        }

        private void prepareSegmentType(ref uint segType, ref uint segNum, ref uint segLength, ref uint segFlags,
            ref uint refFlags, ref uint[] refSegs, ref uint nRefSegs, ref uint page)
        {
            int c1 = 0, c2 = 0, c3 = 0;

            // segment header flags
            if (!readUByte(ref segFlags))
            {
                return;
            }
            segType = segFlags & 0x3f;

            // referred-to segment count and retention flags
            if (!readUByte(ref refFlags))
            {
                return;
            }
            nRefSegs = refFlags >> 5;
            if (nRefSegs == 7)
            {
                if ((c1 = m_curStr.ReadByte()) == -1 ||
                (c2 = m_curStr.ReadByte()) == -1 ||
                (c3 = m_curStr.ReadByte()) == -1)
                {
                    return;
                }
                refFlags = (uint)((refFlags << 24) | (uint)(c1 << 16) | (uint)(c2 << 8) | (uint)c3);
                nRefSegs = refFlags & 0x1fffffff;
                for (uint i = 0; i < (nRefSegs + 9) >> 3; ++i)
                {
                    c1 = m_curStr.ReadByte();
                }
            }

            // referred-to segment numbers
            refSegs = new uint[nRefSegs];
            if (segNum <= 256)
            {
                for (uint i = 0; i < nRefSegs; ++i)
                {
                    if (!readUByte(ref refSegs[i]))
                    {
                        return;
                    }
                }
            }
            else if (segNum <= 65536)
            {
                for (uint i = 0; i < nRefSegs; ++i)
                {
                    if (!readUWord(ref refSegs[i]))
                    {
                        return;
                    }
                }
            }
            else
            {
                for (uint i = 0; i < nRefSegs; ++i)
                {
                    if (!readULong(ref refSegs[i]))
                    {
                        return;
                    }
                }
            }

            // segment page association
            if ((segFlags & 0x40) != 0)
            {
                if (!readULong(ref page))
                {
                    return;
                }
            }
            else
            {
                if (!readUByte(ref page))
                {
                    return;
                }
            }

            // segment data length
            if (!readULong(ref segLength))
            {
                return;
            }
        }

        private bool readSymbolDictSeg(uint segNum, uint length,
                     uint[] refSegs, uint nRefSegs)
        {
            JBIG2SymbolDict symbolDict;
            JBIG2HuffmanTable[] huffDHTable, huffDWTable;
            JBIG2HuffmanTable[] huffBMSizeTable, huffAggInstTable;
            JBIG2Segment seg;
            List<Object> codeTables;
            JBIG2SymbolDict inputSymbolDict;
            uint flags = 0, sdTemplate, sdrTemplate, huff, refAgg;
            uint huffDH, huffDW, huffBMSize, huffAggInst;
            uint contextUsed, contextRetained;
            int[] sdATX = new int[4], sdATY = new int[4], sdrATX = new int[2], sdrATY = new int[2];
            uint numExSyms = 0, numNewSyms = 0, numInputSyms, symCodeLen;
            JBIG2Bitmap[] bitmaps;
            JBIG2Bitmap collBitmap, refBitmap;
            uint[] symWidths;
            uint symHeight, symWidth, totalWidth, x, symID;
            int dh = 0, dw = 0, refAggNum = 0, refDX = 0, refDY = 0, bmSize = 0;
            bool ex;
            int run = 0, cnt;
            uint i, j, k;
            byte[] p;

            // symbol dictionary flags
            if (!readUWord(ref flags))
            {
                return false;
            }
            sdTemplate = (flags >> 10) & 3;
            sdrTemplate = (flags >> 12) & 1;
            huff = flags & 1;
            refAgg = (flags >> 1) & 1;
            huffDH = (flags >> 2) & 3;
            huffDW = (flags >> 4) & 3;
            huffBMSize = (flags >> 6) & 1;
            huffAggInst = (flags >> 7) & 1;
            contextUsed = (flags >> 8) & 1;
            contextRetained = (flags >> 9) & 1;

            // symbol dictionary AT flags
            if (huff == 0)
            {
                if (sdTemplate == 0)
                {
                    if (!readByte(ref sdATX[0]) ||
                    !readByte(ref sdATY[0]) ||
                    !readByte(ref sdATX[1]) ||
                    !readByte(ref sdATY[1]) ||
                    !readByte(ref sdATX[2]) ||
                    !readByte(ref sdATY[2]) ||
                    !readByte(ref sdATX[3]) ||
                    !readByte(ref sdATY[3]))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!readByte(ref sdATX[0]) ||
                    !readByte(ref sdATY[0]))
                    {
                        return false;
                    }
                }
            }

            // symbol dictionary refinement AT flags
            if (refAgg != 0 && sdrTemplate == 0)
            {
                if (!readByte(ref sdrATX[0]) ||
                !readByte(ref sdrATY[0]) ||
                !readByte(ref sdrATX[1]) ||
                !readByte(ref sdrATY[1]))
                {
                    return false;
                }
            }

            // SDNUMEXSYMS and SDNUMNEWSYMS
            if (!readULong(ref numExSyms) || !readULong(ref numNewSyms))
            {
                return false;
            }

            // get referenced m_segments: input symbol dictionaries and code tables
            codeTables = new List<Object>();
            numInputSyms = 0;
            for (i = 0; i < nRefSegs; ++i)
            {
                seg = findSegment(refSegs[i]);
                if (seg.Type == JBIG2SegmentType.jbig2SegSymbolDict)
                {
                    numInputSyms += (uint)((JBIG2SymbolDict)seg).Size;
                }
                else if (seg.Type == JBIG2SegmentType.jbig2SegCodeTable)
                {
                    codeTables.Add(seg);
                }
            }

            // compute symbol code length
            symCodeLen = 0;
            i = 1;
            while (i < numInputSyms + numNewSyms)
            {
                ++symCodeLen;
                i <<= 1;
            }

            // get the input symbol bitmaps
            bitmaps = new JBIG2Bitmap[numInputSyms + numNewSyms];

            for (i = 0; i < numInputSyms + numNewSyms; ++i)
            {
                bitmaps[i] = null;
            }
            k = 0;
            inputSymbolDict = null;
            for (i = 0; i < nRefSegs; ++i)
            {
                seg = findSegment(refSegs[i]);
                if (seg.Type == JBIG2SegmentType.jbig2SegSymbolDict)
                {
                    inputSymbolDict = (JBIG2SymbolDict)seg;
                    for (j = 0; j < inputSymbolDict.Size; ++j)
                    {
                        bitmaps[k++] = inputSymbolDict.GetBitmap(j);
                    }
                }
            }

            // get the Huffman tables
            huffDHTable = huffDWTable = null; // make gcc happy
            huffBMSizeTable = huffAggInstTable = null; // make gcc happy
            i = 0;
            if (huff != 0)
            {
                if (huffDH == 0)
                {
                    huffDHTable = JBIG2HuffmanTables.huffTableD;
                }
                else if (huffDH == 1)
                {
                    huffDHTable = JBIG2HuffmanTables.huffTableE;
                }
                else
                {
                    huffDHTable = ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
                if (huffDW == 0)
                {
                    huffDWTable = JBIG2HuffmanTables.huffTableB;
                }
                else if (huffDW == 1)
                {
                    huffDWTable = JBIG2HuffmanTables.huffTableC;
                }
                else
                {
                    huffDWTable = ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
                if (huffBMSize == 0)
                {
                    huffBMSizeTable = JBIG2HuffmanTables.huffTableA;
                }
                else
                {
                    huffBMSizeTable =
                    ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
                if (huffAggInst == 0)
                {
                    huffAggInstTable = JBIG2HuffmanTables.huffTableA;
                }
                else
                {
                    huffAggInstTable =
                    ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
            }

            // set up the Huffman decoder
            if (huff != 0)
            {
                m_huffDecoder.Reset();

                // set up the arithmetic decoder
            }
            else
            {
                if (contextUsed != 0 && inputSymbolDict != null)
                {
                    resetGenericStats(sdTemplate, inputSymbolDict.GetGenericRegionStats());
                }
                else
                {
                    resetGenericStats(sdTemplate, null);
                }
                resetIntStats((int)symCodeLen);
                m_arithDecoder.Start();
            }

            // set up the arithmetic decoder for refinement/aggregation
            if (refAgg != 0)
            {
                if (contextUsed != 0 && inputSymbolDict != null)
                {
                    resetRefinementStats(sdrTemplate,
                             inputSymbolDict.GetRefinementRegionStats());
                }
                else
                {
                    resetRefinementStats(sdrTemplate, null);
                }
            }

            // allocate symbol widths storage
            symWidths = null;
            if (huff != 0 && refAgg == 0)
            {
                symWidths = new uint[numNewSyms];
            }

            symHeight = 0;
            i = 0;
            while (i < numNewSyms)
            {

                // read the height class delta height
                if (huff != 0)
                {
                    m_huffDecoder.DecodeInt(ref dh, huffDHTable);
                }
                else
                {
                    m_arithDecoder.DecodeInt(ref dh, m_iadhStats);
                }
                if (dh < 0 && (int)-dh >= symHeight)
                {
                    return false;
                }
                symHeight += (uint)dh;
                symWidth = 0;
                totalWidth = 0;
                j = i;

                // read the symbols in this height class
                while (true)
                {

                    // read the delta width
                    if (huff != 0)
                    {
                        if (!m_huffDecoder.DecodeInt(ref dw, huffDWTable))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!m_arithDecoder.DecodeInt(ref dw, m_iadwStats))
                        {
                            break;
                        }
                    }
                    if (dw < 0 && (int)-dw >= symWidth)
                    {
                        return false;
                    }
                    symWidth += (uint)dw;

                    // using a collective bitmap, so don't read a bitmap here
                    if (huff != 0 && refAgg == 0)
                    {
                        symWidths[i] = symWidth;
                        totalWidth += symWidth;

                        // refinement/aggregate coding
                    }
                    else if (refAgg != 0)
                    {
                        if (huff != 0)
                        {
                            if (!m_huffDecoder.DecodeInt(ref refAggNum, huffAggInstTable))
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (!m_arithDecoder.DecodeInt(ref refAggNum, m_iaaiStats))
                            {
                                break;
                            }
                        }

                        if (refAggNum == 1)
                        {
                            if (huff != 0)
                            {
                                symID = m_huffDecoder.ReadBits(symCodeLen);
                                m_huffDecoder.DecodeInt(ref refDX, JBIG2HuffmanTables.huffTableO);
                                m_huffDecoder.DecodeInt(ref refDY, JBIG2HuffmanTables.huffTableO);
                                m_huffDecoder.DecodeInt(ref bmSize, JBIG2HuffmanTables.huffTableA);
                                m_huffDecoder.Reset();
                                m_arithDecoder.Start();
                            }
                            else
                            {
                                symID = m_arithDecoder.DecodeIAID(symCodeLen, m_iaidStats);
                                m_arithDecoder.DecodeInt(ref refDX, m_iardxStats);
                                m_arithDecoder.DecodeInt(ref refDY, m_iardyStats);
                            }
                            refBitmap = bitmaps[symID];
                            bitmaps[numInputSyms + i] =
                                readGenericRefinementRegion((int)symWidth, (int)symHeight,
                                            (int)sdrTemplate, false,
                                            refBitmap, refDX, refDY,
                                            sdrATX, sdrATY);
                        }
                        else
                        {
                            bitmaps[numInputSyms + i] =
                                readTextRegion(huff != 0, true, (int)symWidth, (int)symHeight,
                                       (uint)refAggNum, 0, (int)(numInputSyms + i), null,
                                       symCodeLen, bitmaps, 0, 0, 0, 1, 0,
                                       JBIG2HuffmanTables.huffTableF, JBIG2HuffmanTables.huffTableH,
                                       JBIG2HuffmanTables.huffTableK, JBIG2HuffmanTables.huffTableO,
                                       JBIG2HuffmanTables.huffTableO, JBIG2HuffmanTables.huffTableO,
                                       JBIG2HuffmanTables.huffTableO, JBIG2HuffmanTables.huffTableA,
                                       sdrTemplate, sdrATX, sdrATY);
                        }

                    }
                    else
                    {
                        bitmaps[numInputSyms + i] =
                            readGenericBitmap(false, (int)symWidth, (int)symHeight,
                                      (int)sdTemplate, false, false, null,
                                      sdATX, sdATY, 0);
                    }

                    ++i;
                }


                // read the collective bitmap
                if (huff != 0 && refAgg == 0)
                {
                    m_huffDecoder.DecodeInt(ref bmSize, huffBMSizeTable);
                    m_huffDecoder.Reset();
                    if (bmSize == 0)
                    {
                        collBitmap = new JBIG2Bitmap(0, (int)totalWidth, (int)symHeight);
                        bmSize = (int)(symHeight * ((totalWidth + 7) >> 3));
                        p = collBitmap.GetDataPtr();
                        for (k = 0; k < (int)bmSize; ++k)
                        {
                            p[i++] = (byte)m_curStr.ReadByte();
                        }
                    }
                    else
                    {
                        collBitmap = readGenericBitmap(true, (int)totalWidth, (int)symHeight,
                                           0, false, false, null, null, null,
                                           bmSize);
                    }
                    x = 0;
                    for (; j < i; ++j)
                    {
                        bitmaps[numInputSyms + j] =
                            collBitmap.GetSlice(x, 0, symWidths[j], symHeight);
                        x += symWidths[j];
                    }
                    collBitmap = null; ;
                }
            }

            // create the symbol dict object
            symbolDict = new JBIG2SymbolDict(segNum, (int)numExSyms);

            // exported symbol list
            i = j = 0;
            ex = false;
            while (i < numInputSyms + numNewSyms)
            {
                if (huff != 0)
                {
                    m_huffDecoder.DecodeInt(ref run, JBIG2HuffmanTables.huffTableA);
                }
                else
                {
                    m_arithDecoder.DecodeInt(ref run, m_iaexStats);
                }
                if (ex)
                {
                    for (cnt = 0; cnt < run; ++cnt)
                    {
                        symbolDict.SetBitmap(j++, bitmaps[i++].Copy());
                    }
                }
                else
                {
                    i += (uint)run;
                }
                ex = !ex;
            }


            // save the arithmetic decoder stats
            if (huff == 0 && contextRetained != 0)
            {
                symbolDict.SetGenericRegionStats(m_genericRegionStats.Copy());
                if (refAgg != 0)
                {
                    symbolDict.SetRefinementRegionStats(m_refinementRegionStats.Copy());
                }
            }

            // store the new symbol dict
            m_segments.Add(symbolDict);

            return true;
        }

        private void readTextRegionSeg(uint segNum, bool imm,
                    bool lossless, uint length,
                    uint[] refSegs, uint nRefSegs)
        {
            JBIG2Bitmap bitmap;
            JBIG2HuffmanTable[] runLengthTab = new JBIG2HuffmanTable[36];
            JBIG2HuffmanTable[] symCodeTab;
            JBIG2HuffmanTable[] huffFSTable, huffDSTable, huffDTTable;
            JBIG2HuffmanTable[] huffRDWTable, huffRDHTable;
            JBIG2HuffmanTable[] huffRDXTable, huffRDYTable, huffRSizeTable;
            JBIG2Segment seg;
            List<Object> codeTables;
            JBIG2SymbolDict symbolDict;
            JBIG2Bitmap[] syms;
            uint w = 0, h = 0, x = 0, y = 0, segInfoFlags = 0, extCombOp = 0;
            uint flags = 0, huff, refine, logStrips, refCorner, transposed;
            uint combOp, defPixel, templ;
            int sOffset;
            uint huffFlags = 0, huffFS, huffDS, huffDT;
            uint huffRDW, huffRDH, huffRDX, huffRDY, huffRSize;
            uint numInstances = 0, numSyms = 0, symCodeLen = 0;
            int[] atx = new int[2], aty = new int[2];
            uint i, k, kk;
            int j = 0;

            // region segment info field
            if (!readULong(ref w) || !readULong(ref h) ||
                !readULong(ref x) || !readULong(ref y) ||
                !readUByte(ref segInfoFlags))
            {
                return;
            }
            extCombOp = segInfoFlags & 7;

            // rest of the text region header
            if (!readUWord(ref flags))
            {
                return;
            }
            huff = flags & 1;
            refine = (flags >> 1) & 1;
            logStrips = (flags >> 2) & 3;
            refCorner = (flags >> 4) & 3;
            transposed = (flags >> 6) & 1;
            combOp = (flags >> 7) & 3;
            defPixel = (flags >> 9) & 1;
            sOffset = (int)(flags >> 10) & 0x1f;
            if ((sOffset & 0x10) != 0)
            {
                sOffset |= -1 - 0x0f;
            }
            templ = (flags >> 15) & 1;
            huffFS = huffDS = huffDT = 0; // make gcc happy
            huffRDW = huffRDH = huffRDX = huffRDY = huffRSize = 0; // make gcc happy
            if (huff != 0)
            {
                if (!readUWord(ref huffFlags))
                {
                    return;
                }
                huffFS = huffFlags & 3;
                huffDS = (huffFlags >> 2) & 3;
                huffDT = (huffFlags >> 4) & 3;
                huffRDW = (huffFlags >> 6) & 3;
                huffRDH = (huffFlags >> 8) & 3;
                huffRDX = (huffFlags >> 10) & 3;
                huffRDY = (huffFlags >> 12) & 3;
                huffRSize = (huffFlags >> 14) & 1;
            }
            if (refine != 0 && templ == 0)
            {
                if (!readByte(ref atx[0]) || !readByte(ref aty[0]) ||
                !readByte(ref atx[1]) || !readByte(ref aty[1]))
                {
                    return;
                }
            }
            if (!readULong(ref numInstances))
            {
                return;
            }

            // get symbol dictionaries and tables
            codeTables = new List<Object>();
            numSyms = 0;
            for (i = 0; i < nRefSegs; ++i)
            {
                if ((seg = findSegment(refSegs[i])) != null)
                {
                    if (seg.Type == JBIG2SegmentType.jbig2SegSymbolDict)
                    {
                        numSyms += (uint)((JBIG2SymbolDict)seg).Size;
                    }
                    else if (seg.Type == JBIG2SegmentType.jbig2SegCodeTable)
                    {
                        codeTables.Add(seg);
                    }
                }
                else
                {
                }
            }
            symCodeLen = 0;
            i = 1;
            while (i < numSyms)
            {
                ++symCodeLen;
                i <<= 1;
            }

            // get the symbol bitmaps
            syms = new JBIG2Bitmap[numSyms];
            kk = 0;
            for (i = 0; i < nRefSegs; ++i)
            {
                if ((seg = findSegment(refSegs[i])) != null)
                {
                    if (seg.Type == JBIG2SegmentType.jbig2SegSymbolDict)
                    {
                        symbolDict = (JBIG2SymbolDict)seg;
                        for (k = 0; k < symbolDict.Size; ++k)
                        {
                            syms[kk++] = symbolDict.GetBitmap(k);
                        }
                    }
                }
            }

            // get the Huffman tables
            huffFSTable = huffDSTable = huffDTTable = null; // make gcc happy
            huffRDWTable = huffRDHTable = null; // make gcc happy
            huffRDXTable = huffRDYTable = huffRSizeTable = null; // make gcc happy
            i = 0;
            if (huff != 0)
            {
                if (huffFS == 0)
                {
                    huffFSTable = JBIG2HuffmanTables.huffTableF;
                }
                else if (huffFS == 1)
                {
                    huffFSTable = JBIG2HuffmanTables.huffTableG;
                }
                else
                {
                    huffFSTable = ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
                if (huffDS == 0)
                {
                    huffDSTable = JBIG2HuffmanTables.huffTableH;
                }
                else if (huffDS == 1)
                {
                    huffDSTable = JBIG2HuffmanTables.huffTableI;
                }
                else if (huffDS == 2)
                {
                    huffDSTable = JBIG2HuffmanTables.huffTableJ;
                }
                else
                {
                    huffDSTable = ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
                if (huffDT == 0)
                {
                    huffDTTable = JBIG2HuffmanTables.huffTableK;
                }
                else if (huffDT == 1)
                {
                    huffDTTable = JBIG2HuffmanTables.huffTableL;
                }
                else if (huffDT == 2)
                {
                    huffDTTable = JBIG2HuffmanTables.huffTableM;
                }
                else
                {
                    huffDTTable = ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
                if (huffRDW == 0)
                {
                    huffRDWTable = JBIG2HuffmanTables.huffTableN;
                }
                else if (huffRDW == 1)
                {
                    huffRDWTable = JBIG2HuffmanTables.huffTableO;
                }
                else
                {
                    huffRDWTable = ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
                if (huffRDH == 0)
                {
                    huffRDHTable = JBIG2HuffmanTables.huffTableN;
                }
                else if (huffRDH == 1)
                {
                    huffRDHTable = JBIG2HuffmanTables.huffTableO;
                }
                else
                {
                    huffRDHTable = ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
                if (huffRDX == 0)
                {
                    huffRDXTable = JBIG2HuffmanTables.huffTableN;
                }
                else if (huffRDX == 1)
                {
                    huffRDXTable = JBIG2HuffmanTables.huffTableO;
                }
                else
                {
                    huffRDXTable = ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
                if (huffRDY == 0)
                {
                    huffRDYTable = JBIG2HuffmanTables.huffTableN;
                }
                else if (huffRDY == 1)
                {
                    huffRDYTable = JBIG2HuffmanTables.huffTableO;
                }
                else
                {
                    huffRDYTable = ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
                if (huffRSize == 0)
                {
                    huffRSizeTable = JBIG2HuffmanTables.huffTableA;
                }
                else
                {
                    huffRSizeTable =
                    ((JBIG2CodeTable)codeTables[(int)i++]).GetHuffTable();
                }
            }

            // symbol ID Huffman decoding table
            if (huff != 0)
            {
                m_huffDecoder.Reset();
                for (i = 0; i < 32; ++i)
                {
                    runLengthTab[i].val = (int)i;
                    runLengthTab[i].prefixLen = m_huffDecoder.ReadBits(4);
                    runLengthTab[i].rangeLen = 0;
                }
                runLengthTab[32].val = 0x103;
                runLengthTab[32].prefixLen = m_huffDecoder.ReadBits(4);
                runLengthTab[32].rangeLen = 2;
                runLengthTab[33].val = 0x203;
                runLengthTab[33].prefixLen = m_huffDecoder.ReadBits(4);
                runLengthTab[33].rangeLen = 3;
                runLengthTab[34].val = 0x20b;
                runLengthTab[34].prefixLen = m_huffDecoder.ReadBits(4);
                runLengthTab[34].rangeLen = 7;
                runLengthTab[35].prefixLen = 0;
                runLengthTab[35].rangeLen = JBIG2HuffmanTables.jbig2HuffmanEOT;
                m_huffDecoder.BuildTable(runLengthTab, 35);
                symCodeTab = new JBIG2HuffmanTable[numSyms + 1];
                
                for (i = 0; i < numSyms; ++i)
                {
                    symCodeTab[i].val = (int)i;
                    symCodeTab[i].rangeLen = 0;
                }
                i = 0;
                while (i < numSyms)
                {
                    m_huffDecoder.DecodeInt(ref j, runLengthTab);
                    if (j > 0x200)
                    {
                        for (j -= 0x200; j != 0 && i < numSyms; --j)
                        {
                            symCodeTab[i++].prefixLen = 0;
                        }
                    }
                    else if (j > 0x100)
                    {
                        for (j -= 0x100; j != 0 && i < numSyms; --j)
                        {
                            symCodeTab[i].prefixLen = symCodeTab[i - 1].prefixLen;
                            ++i;
                        }
                    }
                    else
                    {
                        symCodeTab[i++].prefixLen = (uint)j;
                    }
                }
                symCodeTab[numSyms].prefixLen = 0;
                symCodeTab[numSyms].rangeLen = JBIG2HuffmanTables.jbig2HuffmanEOT;
                m_huffDecoder.BuildTable(symCodeTab, numSyms);
                m_huffDecoder.Reset();

                // set up the arithmetic decoder
            }
            else
            {
                symCodeTab = null;
                resetIntStats((int)symCodeLen);
                m_arithDecoder.Start();
            }
            if (refine != 0)
            {
                resetRefinementStats(templ, null);
            }

            bitmap = readTextRegion(huff != 0, refine != 0, (int)w, (int)h, numInstances,
                        logStrips, (int)numSyms, symCodeTab, symCodeLen, syms,
                        defPixel, (int)combOp, transposed, (int)refCorner, sOffset,
                        huffFSTable, huffDSTable, huffDTTable,
                        huffRDWTable, huffRDHTable,
                        huffRDXTable, huffRDYTable, huffRSizeTable,
                        templ, atx, aty);

            

            // combine the region bitmap into the page bitmap
            if (imm)
            {
                uint temp = 0xffffffff;
                if (m_pageH == (int)temp && y + h > m_curPageH)
                {
                    m_pageBitmap.Expand((int)(y + h), m_pageDefPixel);
                }
                m_pageBitmap.Combine(bitmap, (int)x, (int)y, extCombOp);

                // store the region bitmap
            }
            else
            {
                bitmap.SegNum = segNum;
                m_segments.Add(bitmap);
            }

            return;
        }

        private void readPageInfoSeg(uint length)
        {
            uint xRes = 0, yRes = 0, flags = 0, striping = 0;

            if (!readULong(ref m_pageW) || !readULong(ref m_pageH) ||
                !readULong(ref xRes) || !readULong(ref yRes) ||
                !readUByte(ref flags) || !readUWord(ref striping))
            {
                return;
            }
            m_pageDefPixel = (flags >> 2) & 1;
            m_defCombOp = (flags >> 3) & 3;

            // allocate the page bitmap
            uint temp = 0xffffffff;
            if (m_pageH == (int)temp)
            {
                m_curPageH = striping & 0x7fff;
            }
            else
            {
                m_curPageH = m_pageH;
            }
            m_pageBitmap = new JBIG2Bitmap(0, (int)m_pageW, (int)m_curPageH);

            // default pixel value
            if (m_pageDefPixel != 0)
            {
                m_pageBitmap.ClearToOne();
            }
            else
            {
                m_pageBitmap.ClearToZero();
            }

            return;
        }

        private void readEndOfStripeSeg(uint length)
        {
            skipTheSegment(length);
        }

        private void readProfilesSeg(uint length)
        {
            skipTheSegment(length);
        }

        private void skipTheSegment(uint length)
        {
            uint i;

            // skip the segment
            for (i = 0; i < length; ++i)
            {
                m_curStr.ReadByte();
            }
        }

        private void readCodeTableSeg(uint segNum, uint length)
        {
            JBIG2HuffmanTable[] huffTab;
            uint flags = 0, oob, prefixBits, rangeBits;
            int lowVal = 0, highVal = 0, val;
            uint huffTabSize, i;

            if (!readUByte(ref flags) || !readLong(ref lowVal) || !readLong(ref highVal))
            {
                return;
            }
            oob = flags & 1;
            prefixBits = ((flags >> 1) & 7) + 1;
            rangeBits = ((flags >> 4) & 7) + 1;

            m_huffDecoder.Reset();
            huffTabSize = 8;
            huffTab = new JBIG2HuffmanTable[huffTabSize];
            
            i = 0;
            val = lowVal;
            while (val < highVal)
            {
                if (i == huffTabSize)
                {
                    huffTabSize *= 2;
                    JBIG2HuffmanTable[] temp = new JBIG2HuffmanTable[huffTabSize];
                    Array.Copy(huffTab, temp, huffTab.Length);
                    huffTab = temp;
                }
                huffTab[i].val = val;
                huffTab[i].prefixLen = m_huffDecoder.ReadBits(prefixBits);
                huffTab[i].rangeLen = m_huffDecoder.ReadBits(rangeBits);
                val += 1 << (int)huffTab[i].rangeLen;
                ++i;
            }
            if (i + oob + 3 > huffTabSize)
            {
                huffTabSize = i + oob + 3;

                JBIG2HuffmanTable[] temp = new JBIG2HuffmanTable[huffTabSize];
                Array.Copy(huffTab, temp, huffTab.Length);
            }
            huffTab[i].val = lowVal - 1;
            huffTab[i].prefixLen = m_huffDecoder.ReadBits(prefixBits);
            huffTab[i].rangeLen = JBIG2HuffmanTables.jbig2HuffmanLOW;
            ++i;
            huffTab[i].val = highVal;
            huffTab[i].prefixLen = m_huffDecoder.ReadBits(prefixBits);
            huffTab[i].rangeLen = 32;
            ++i;
            if (oob != 0)
            {
                huffTab[i].val = 0;
                huffTab[i].prefixLen = m_huffDecoder.ReadBits(prefixBits);
                huffTab[i].rangeLen = JBIG2HuffmanTables.jbig2HuffmanOOB;
                ++i;
            }
            huffTab[i].val = 0;
            huffTab[i].prefixLen = 0;
            huffTab[i].rangeLen = JBIG2HuffmanTables.jbig2HuffmanEOT;
            m_huffDecoder.BuildTable(huffTab, i);

            // create and store the new table segment
            m_segments.Add(new JBIG2CodeTable(segNum, huffTab));

            return;
        }

        private void readExtensionSeg(uint length)
        {
            skipTheSegment(length);
        }

        private JBIG2Bitmap readTextRegion(bool huff, bool refine,
                     int w, int h,
                     uint numInstances,
                     uint logStrips,
                     int numSyms,
                     JBIG2HuffmanTable[] symCodeTab,
                     uint symCodeLen,
                     JBIG2Bitmap[] syms,
                     uint defPixel, int combOp,
                     uint transposed, int refCorner,
                     int sOffset,
                     JBIG2HuffmanTable[] huffFSTable,
                     JBIG2HuffmanTable[] huffDSTable,
                     JBIG2HuffmanTable[] huffDTTable,
                     JBIG2HuffmanTable[] huffRDWTable,
                     JBIG2HuffmanTable[] huffRDHTable,
                     JBIG2HuffmanTable[] huffRDXTable,
                     JBIG2HuffmanTable[] huffRDYTable,
                     JBIG2HuffmanTable[] huffRSizeTable,
                     uint templ,
                     int[] atx, int[] aty)
        {
            JBIG2Bitmap bitmap;
            JBIG2Bitmap symbolBitmap;
            uint strips;
            int t = 0, dt = 0, tt = 0, s = 0, ds = 0, sFirst = 0, j = 0;
            int rdw = 0, rdh = 0, rdx = 0, rdy = 0, ri = 0, refDX = 0, refDY = 0, bmSize = 0;
            uint symID, inst, bw, bh;

            strips = (uint)(1 << (int)logStrips);

            // allocate the bitmap
            bitmap = new JBIG2Bitmap(0, w, h);
            if (defPixel != 0)
            {
                bitmap.ClearToOne();
            }
            else
            {
                bitmap.ClearToZero();
            }

            // decode initial T value
            if (huff)
            {
                m_huffDecoder.DecodeInt(ref t, huffDTTable);
            }
            else
            {
                m_arithDecoder.DecodeInt(ref t, m_iadtStats);
            }
            t *= -(int)strips;

            inst = 0;
            sFirst = 0;
            while (inst < numInstances)
            {

                // decode delta-T
                if (huff)
                {
                    m_huffDecoder.DecodeInt(ref dt, huffDTTable);
                }
                else
                {
                    m_arithDecoder.DecodeInt(ref dt, m_iadtStats);
                }
                t += (int)(dt * strips);

                // first S value
                if (huff)
                {
                    m_huffDecoder.DecodeInt(ref ds, huffFSTable);
                }
                else
                {
                    m_arithDecoder.DecodeInt(ref ds, m_iafsStats);
                }
                sFirst += ds;
                s = sFirst;

                // read the instances
                while (true)
                {

                    // T value
                    if (strips == 1)
                    {
                        dt = 0;
                    }
                    else if (huff)
                    {
                        dt = (int)m_huffDecoder.ReadBits(logStrips);
                    }
                    else
                    {
                        m_arithDecoder.DecodeInt(ref dt, m_iaitStats);
                    }
                    tt = t + dt;

                    // symbol ID
                    if (huff)
                    {
                        if (symCodeTab != null)
                        {
                            m_huffDecoder.DecodeInt(ref j, symCodeTab);
                            symID = (uint)j;
                        }
                        else
                        {
                            symID = m_huffDecoder.ReadBits(symCodeLen);
                        }
                    }
                    else
                    {
                        symID = m_arithDecoder.DecodeIAID(symCodeLen, m_iaidStats);
                    }

                    if (symID >= (int)numSyms)
                    {
                    }
                    else
                    {

                        // get the symbol bitmap
                        symbolBitmap = null;
                        if (refine)
                        {
                            if (huff)
                            {
                                ri = (int)m_huffDecoder.ReadBit();
                            }
                            else
                            {
                                m_arithDecoder.DecodeInt(ref ri, m_iariStats);
                            }
                        }
                        else
                        {
                            ri = 0;
                        }
                        if (ri != 0)
                        {
                            if (huff)
                            {
                                m_huffDecoder.DecodeInt(ref rdw, huffRDWTable);
                                m_huffDecoder.DecodeInt(ref rdh, huffRDHTable);
                                m_huffDecoder.DecodeInt(ref rdx, huffRDXTable);
                                m_huffDecoder.DecodeInt(ref rdy, huffRDYTable);
                                m_huffDecoder.DecodeInt(ref bmSize, huffRSizeTable);
                                m_huffDecoder.Reset();
                                m_arithDecoder.Start();
                            }
                            else
                            {
                                m_arithDecoder.DecodeInt(ref rdw, m_iardwStats);
                                m_arithDecoder.DecodeInt(ref rdh, m_iardhStats);
                                m_arithDecoder.DecodeInt(ref rdx, m_iardxStats);
                                m_arithDecoder.DecodeInt(ref rdy, m_iardyStats);
                            }
                            refDX = ((rdw >= 0) ? rdw : rdw - 1) / 2 + rdx;
                            refDY = ((rdh >= 0) ? rdh : rdh - 1) / 2 + rdy;

                            symbolBitmap =
                              readGenericRefinementRegion(rdw + syms[symID].Width,
                                          rdh + syms[symID].Height,
                                          (int)templ, false, syms[symID],
                                          refDX, refDY, atx, aty);
                            //~ do we need to use the bmSize value here (in Huffman mode)?
                        }
                        else
                        {
                            symbolBitmap = syms[symID];
                        }

                        // combine the symbol bitmap into the region bitmap
                        //~ something is wrong here - refCorner shouldn't degenerate into
                        //~   two cases
                        bw = (uint)symbolBitmap.Width - 1;
                        bh = (uint)symbolBitmap.Height - 1;
                        if (transposed != 0)
                        {
                            switch (refCorner)
                            {
                                case 0: // bottom left
                                    bitmap.Combine(symbolBitmap, (int)tt, (int)s, (uint)combOp);
                                    break;
                                case 1: // top left
                                    bitmap.Combine(symbolBitmap, (int)tt, (int)s, (uint)combOp);
                                    break;
                                case 2: // bottom right
                                    bitmap.Combine(symbolBitmap, (int)(tt - bw), (int)s, (uint)combOp);
                                    break;
                                case 3: // top right
                                    bitmap.Combine(symbolBitmap, (int)(tt - bw), s, (uint)combOp);
                                    break;
                            }
                            s += (int)bh;
                        }
                        else
                        {
                            switch (refCorner)
                            {
                                case 0: // bottom left
                                    bitmap.Combine(symbolBitmap, s, (int)(tt - bh), (uint)combOp);
                                    break;
                                case 1: // top left
                                    bitmap.Combine(symbolBitmap, s, (int)tt, (uint)combOp);
                                    break;
                                case 2: // bottom right
                                    bitmap.Combine(symbolBitmap, s, (int)(tt - bh), (uint)combOp);
                                    break;
                                case 3: // top right
                                    bitmap.Combine(symbolBitmap, s, (int)tt, (uint)combOp);
                                    break;
                            }
                            s += (int)bw;
                        }

                    }

                    // next instance
                    ++inst;

                    // next S value
                    if (huff)
                    {
                        if (!m_huffDecoder.DecodeInt(ref ds, huffDSTable))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!m_arithDecoder.DecodeInt(ref ds, m_iadsStats))
                        {
                            break;
                        }
                    }
                    s += sOffset + ds;
                }
            }

            return bitmap;
        }

        private void readGenericRegionSeg(uint segNum, bool imm,
                       bool lossless, uint length)
        {
            JBIG2Bitmap bitmap;
            uint w = 0, h = 0, x = 0, y = 0, segInfoFlags = 0, extCombOp;
            uint flags = 0, mmr, templ, tpgdOn;
            int[] atx = new int[4], aty = new int[4];

            // region segment info field
            if (!readULong(ref w) || !readULong(ref h) ||
                !readULong(ref x) || !readULong(ref y) ||
                !readUByte(ref segInfoFlags))
            {
                return;
            }
            extCombOp = segInfoFlags & 7;

            // rest of the generic region segment header
            if (!readUByte(ref flags))
            {
                return;
            }
            mmr = flags & 1;
            templ = (flags >> 1) & 3;
            tpgdOn = (flags >> 3) & 1;

            // AT flags
            if (mmr == 0)
            {
                if (templ == 0)
                {
                    if (!readByte(ref atx[0]) ||
                    !readByte(ref aty[0]) ||
                    !readByte(ref atx[1]) ||
                    !readByte(ref aty[1]) ||
                    !readByte(ref atx[2]) ||
                    !readByte(ref aty[2]) ||
                    !readByte(ref atx[3]) ||
                    !readByte(ref aty[3]))
                    {
                        return;
                    }
                }
                else
                {
                    if (!readByte(ref atx[0]) ||
                  !readByte(ref aty[0]))
                    {
                        return;
                    }
                }
            }

            // set up the arithmetic decoder
            if (mmr == 0)
            {
                resetGenericStats(templ, null);
                m_arithDecoder.Start();
            }

            // read the bitmap
            bitmap = readGenericBitmap(mmr != 0, (int)w, (int)h, (int)templ, tpgdOn != 0, false,
                           null, atx, aty, mmr != 0 ? 0 : (int)length - 18);

            // combine the region bitmap into the page bitmap
            if (imm)
            {
                uint temp = 0xffffffff;
                if (m_pageH == (int)temp && y + h > m_curPageH)
                {
                    m_pageBitmap.Expand((int)(y + h), m_pageDefPixel);
                }
                m_pageBitmap.Combine(bitmap, (int)x, (int)y, extCombOp);

                // store the region bitmap
            }
            else
            {
                bitmap.SegNum = segNum;
                m_segments.Add(bitmap);
            }

            return;
        }

        private void readHalftoneRegionSeg(uint segNum, bool imm,
                    bool lossless, uint length,
                    uint[] refSegs, uint nRefSegs)
        {
            JBIG2Bitmap bitmap;
            JBIG2Segment seg;
            JBIG2PatternDict patternDict;
            JBIG2Bitmap skipBitmap;
            uint[] grayImg;
            JBIG2Bitmap grayBitmap;
            JBIG2Bitmap patternBitmap;
            uint w = 0, h = 0, x = 0, y = 0, segInfoFlags = 0, extCombOp;
            uint flags = 0, mmr, templ, enableSkip, combOp;
            uint gridW = 0, gridH = 0, stepX = 0, stepY = 0, patW, patH;
            int[] atx = new int[4], aty = new int[4];
            int gridX = 0, gridY = 0, xx, yy, bit, j;
            uint bpp, m, n, i;

            // region segment info field
            if (!readULong(ref w) || !readULong(ref h) ||
                !readULong(ref x) || !readULong(ref y) ||
                !readUByte(ref segInfoFlags))
            {
                return;
            }
            extCombOp = segInfoFlags & 7;

            // rest of the halftone region header
            if (!readUByte(ref flags))
            {
                return;
            }
            mmr = flags & 1;
            templ = (flags >> 1) & 3;
            enableSkip = (flags >> 3) & 1;
            combOp = (flags >> 4) & 7;
            if (!readULong(ref gridW) || !readULong(ref gridH) ||
                !readLong(ref gridX) || !readLong(ref gridY) ||
                !readUWord(ref stepX) || !readUWord(ref stepY))
            {
                return;
            }
            if (w == 0 || h == 0 || w >= Int32.MaxValue / h)
            {
                return;
            }
            if (gridH == 0 || gridW >= Int32.MaxValue / gridH)
            {
                return;
            }

            // get pattern dictionary
            if (nRefSegs != 1)
            {
                return;
            }
            seg = findSegment(refSegs[0]);
            if (seg.Type != JBIG2SegmentType.jbig2SegPatternDict)
            {
                return;
            }
            patternDict = (JBIG2PatternDict)seg;
            bpp = 0;
            i = 1;
            while (i < patternDict.Size)
            {
                ++bpp;
                i <<= 1;
            }
            patW = (uint)patternDict.GetBitmap(0).Width;
            patH = (uint)patternDict.GetBitmap(0).Height;

            // set up the arithmetic decoder
            if (mmr == 0)
            {
                resetGenericStats(templ, null);
                m_arithDecoder.Start();
            }

            // allocate the bitmap
            bitmap = new JBIG2Bitmap(segNum, (int)w, (int)h);
            if ((flags & 0x80) != 0)
            { // HDEFPIXEL
                bitmap.ClearToOne();
            }
            else
            {
                bitmap.ClearToZero();
            }

            // compute the skip bitmap
            skipBitmap = null;
            if (enableSkip != 0)
            {
                skipBitmap = new JBIG2Bitmap(0, (int)gridW, (int)gridH);
                skipBitmap.ClearToZero();
                for (m = 0; m < gridH; ++m)
                {
                    for (n = 0; n < gridW; ++n)
                    {
                        xx = (int)(gridX + m * stepY + n * stepX);
                        yy = (int)(gridY + m * stepX - n * stepY);
                        if (((xx + (int)patW) >> 8) <= 0 || (xx >> 8) >= (int)w ||
                            ((yy + (int)patH) >> 8) <= 0 || (yy >> 8) >= (int)h)
                        {
                            skipBitmap.SetPixel((int)n, (int)m);
                        }
                    }
                }
            }

            // read the gray-scale image
            grayImg = new uint[gridW * gridH];

            Array.Clear(grayImg, 0, (int)(gridW * gridH));
            atx[0] = templ <= 1 ? 3 : 2; aty[0] = -1;
            atx[1] = -3; aty[1] = -1;
            atx[2] = 2; aty[2] = -2;
            atx[3] = -2; aty[3] = -2;
            for (j = (int)bpp - 1; j >= 0; --j)
            {
                grayBitmap = readGenericBitmap(mmr != 0, (int)gridW, (int)gridH, (int)templ, false,
                               enableSkip != 0, skipBitmap, atx, aty, -1);
                i = 0;
                for (m = 0; m < gridH; ++m)
                {
                    for (n = 0; n < gridW; ++n)
                    {
                        bit = (grayBitmap.GetPixel((int)n, (int)m) ^ (int)(grayImg[i] & 1));
                        grayImg[i] = (uint)((grayImg[i] << 1) | (uint)bit);
                        ++i;
                    }
                }
            }

            // decode the image
            i = 0;
            for (m = 0; m < gridH; ++m)
            {
                xx = (int)(gridX + m * stepY);
                yy = (int)(gridY + m * stepX);
                for (n = 0; n < gridW; ++n)
                {
                    if (!(enableSkip != 0 && skipBitmap.GetPixel((int)n, (int)m) != 0))
                    {
                        patternBitmap = patternDict.GetBitmap((int)grayImg[i]);
                        bitmap.Combine(patternBitmap, xx >> 8, yy >> 8, combOp);
                    }
                    xx += (int)stepX;
                    yy -= (int)stepY;
                    ++i;
                }
            }


            // combine the region bitmap into the page bitmap
            if (imm)
            {
                uint temp = 0xffffffff;
                if (m_pageH == (int)temp && y + h > m_curPageH)
                {
                    m_pageBitmap.Expand((int)(y + h), m_pageDefPixel);
                }
                m_pageBitmap.Combine(bitmap, (int)x, (int)y, extCombOp);

                // store the region bitmap
            }
            else
            {
                m_segments.Add(bitmap);
            }

            return;
        }

        private void readGenericRefinementRegionSeg(uint segNum, bool imm,
                         bool lossless, uint length,
                         uint[] refSegs,
                         uint nRefSegs)
        {
            JBIG2Bitmap bitmap, refBitmap;
            uint w = 0, h = 0, x = 0, y = 0, segInfoFlags = 0, extCombOp = 0;
            uint flags = 0, templ = 0, tpgrOn = 0;
            int[] atx = new int[2], aty = new int[2];
            JBIG2Segment seg;

            // region segment info field
            if (!readULong(ref w) || !readULong(ref h) ||
                !readULong(ref x) || !readULong(ref y) ||
                !readUByte(ref segInfoFlags))
            {
                return;
            }
            extCombOp = segInfoFlags & 7;

            // rest of the generic refinement region segment header
            if (!readUByte(ref flags))
            {
                return;
            }
            templ = flags & 1;
            tpgrOn = (flags >> 1) & 1;

            // AT flags
            if (templ == 0)
            {
                if (!readByte(ref atx[0]) || !readByte(ref aty[0]) ||
                !readByte(ref atx[1]) || !readByte(ref aty[1]))
                {
                    return;
                }
            }

            // resize the page bitmap if needed
            if (nRefSegs == 0 || imm)
            {
                uint temp = 0xffffffff;
                if (m_pageH == (int)temp && y + h > m_curPageH)
                {
                    m_pageBitmap.Expand((int)(y + h), m_pageDefPixel);
                }
            }

            // get referenced bitmap
            if (nRefSegs > 1)
            {
                return;
            }
            if (nRefSegs == 1)
            {
                seg = findSegment(refSegs[0]);
                if (seg.Type != JBIG2SegmentType.jbig2SegBitmap)
                {
                    return;
                }
                refBitmap = (JBIG2Bitmap)seg;
            }
            else
            {
                refBitmap = m_pageBitmap.GetSlice(x, y, w, h);
            }

            // set up the arithmetic decoder
            resetRefinementStats(templ, null);
            m_arithDecoder.Start();

            // read
            bitmap = readGenericRefinementRegion((int)w, (int)h, (int)templ, tpgrOn != 0,
                                 refBitmap, 0, 0, atx, aty);

            // combine the region bitmap into the page bitmap
            if (imm)
            {
                m_pageBitmap.Combine(bitmap, (int)x, (int)y, extCombOp);

                // store the region bitmap
            }
            else
            {
                bitmap.SegNum = segNum;
                m_segments.Add(bitmap);
            }

            // delete the referenced bitmap
            if (nRefSegs == 1)
            {
                discardSegment(refSegs[0]);
            }

            return;
        }

        private JBIG2Bitmap readGenericRefinementRegion(int w, int h,
                              int templ, bool tpgrOn,
                              JBIG2Bitmap refBitmap,
                              int refDX, int refDY,
                              int[] atx, int[] aty)
        {
            JBIG2Bitmap bitmap;
            bool ltp;
            uint ltpCX, cx, cx0, cx2, cx3, cx4, tpgrCX0, tpgrCX1, tpgrCX2;
            JBIG2BitmapPtr cxPtr0 = new JBIG2BitmapPtr(),
                cxPtr1 = new JBIG2BitmapPtr(),
                cxPtr2 = new JBIG2BitmapPtr(),
                cxPtr3 = new JBIG2BitmapPtr(),
                cxPtr4 = new JBIG2BitmapPtr(),
                cxPtr5 = new JBIG2BitmapPtr(),
                cxPtr6 = new JBIG2BitmapPtr();
            JBIG2BitmapPtr tpgrCXPtr0 = new JBIG2BitmapPtr(),
                tpgrCXPtr1 = new JBIG2BitmapPtr(),
                tpgrCXPtr2 = new JBIG2BitmapPtr();
            int x, y, pix;

            bitmap = new JBIG2Bitmap(0, w, h);
            bitmap.ClearToZero();

            // set up the typical row context
            if (templ != 0)
            {
                ltpCX = 0x008;
            }
            else
            {
                ltpCX = 0x0010;
            }

            ltp = false;
            for (y = 0; y < h; ++y)
            {

                if (templ != 0)
                {

                    // set up the context
                    bitmap.GetPixelPtr(0, y - 1,ref cxPtr0);
                    cx0 = (uint)bitmap.NextPixel(ref cxPtr0);
                    bitmap.GetPixelPtr(-1, y, ref cxPtr1);
                    refBitmap.GetPixelPtr(-refDX, y - 1 - refDY, ref cxPtr2);
                    refBitmap.GetPixelPtr(-1 - refDX, y - refDY, ref cxPtr3);
                    cx3 = (uint)refBitmap.NextPixel(ref cxPtr3);
                    cx3 = (cx3 << 1) | (uint)refBitmap.NextPixel(ref cxPtr3);
                    refBitmap.GetPixelPtr(-refDX, y + 1 - refDY, ref cxPtr4);
                    cx4 = (uint)refBitmap.NextPixel(ref cxPtr4);

                    // set up the typical prediction context
                    tpgrCX0 = tpgrCX1 = tpgrCX2 = 0; // make gcc happy
                    if (tpgrOn)
                    {
                        refBitmap.GetPixelPtr(-1 - refDX, y - 1 - refDY, ref tpgrCXPtr0);
                        tpgrCX0 = (uint)refBitmap.NextPixel(ref tpgrCXPtr0);
                        tpgrCX0 = (tpgrCX0 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr0);
                        tpgrCX0 = (tpgrCX0 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr0);
                        refBitmap.GetPixelPtr(-1 - refDX, y - refDY, ref tpgrCXPtr1);
                        tpgrCX1 = (uint)refBitmap.NextPixel(ref tpgrCXPtr1);
                        tpgrCX1 = (tpgrCX1 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr1);
                        tpgrCX1 = (tpgrCX1 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr1);
                        refBitmap.GetPixelPtr(-1 - refDX, y + 1 - refDY, ref tpgrCXPtr2);
                        tpgrCX2 = (uint)refBitmap.NextPixel(ref tpgrCXPtr2);
                        tpgrCX2 = (tpgrCX2 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr2);
                        tpgrCX2 = (tpgrCX2 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr2);
                    }

                    for (x = 0; x < w; ++x)
                    {

                        // update the context
                        cx0 = ((cx0 << 1) | (uint)bitmap.NextPixel(ref cxPtr0)) & 7;
                        cx3 = ((cx3 << 1) | (uint)refBitmap.NextPixel(ref cxPtr3)) & 7;
                        cx4 = ((cx4 << 1) | (uint)refBitmap.NextPixel(ref cxPtr4)) & 3;

                        if (tpgrOn)
                        {
                            // update the typical predictor context
                            tpgrCX0 = ((tpgrCX0 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr0)) & 7;
                            tpgrCX1 = ((tpgrCX1 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr1)) & 7;
                            tpgrCX2 = ((tpgrCX2 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr2)) & 7;

                            // check for a "typical" pixel
                            if (m_arithDecoder.DecodeBit(ltpCX, m_refinementRegionStats) != 0)
                            {
                                ltp = !ltp;
                            }
                            if (tpgrCX0 == 0 && tpgrCX1 == 0 && tpgrCX2 == 0)
                            {
                                bitmap.ClearPixel(x, y);
                                continue;
                            }
                            else if (tpgrCX0 == 7 && tpgrCX1 == 7 && tpgrCX2 == 7)
                            {
                                bitmap.SetPixel(x, y);
                                continue;
                            }
                        }

                        // build the context
                        cx = (uint)(cx0 << 7) | (uint)(bitmap.NextPixel(ref cxPtr1) << 6) |
                             (uint)(refBitmap.NextPixel(ref cxPtr2) << 5) |
                             (uint)(cx3 << 2) | (uint)cx4;

                        // decode the pixel
                        if ((pix = m_arithDecoder.DecodeBit(cx, m_refinementRegionStats)) != 0)
                        {
                            bitmap.SetPixel(x, y);
                        }
                    }

                }
                else
                {

                    // set up the context
                    bitmap.GetPixelPtr(0, y - 1, ref cxPtr0);
                    cx0 = (uint)bitmap.NextPixel(ref cxPtr0);
                    bitmap.GetPixelPtr(-1, y, ref cxPtr1);
                    refBitmap.GetPixelPtr(-refDX, y - 1 - refDY, ref cxPtr2);
                    cx2 = (uint)refBitmap.NextPixel(ref cxPtr2);
                    refBitmap.GetPixelPtr(-1 - refDX, y - refDY, ref cxPtr3);
                    cx3 = (uint)refBitmap.NextPixel(ref cxPtr3);
                    cx3 = (cx3 << 1) | (uint)refBitmap.NextPixel(ref cxPtr3);
                    refBitmap.GetPixelPtr(-1 - refDX, y + 1 - refDY, ref cxPtr4);
                    cx4 = (uint)refBitmap.NextPixel(ref cxPtr4);
                    cx4 = (cx4 << 1) | (uint)refBitmap.NextPixel(ref cxPtr4);
                    bitmap.GetPixelPtr(atx[0], y + aty[0], ref cxPtr5);
                    refBitmap.GetPixelPtr(atx[1] - refDX, y + aty[1] - refDY, ref cxPtr6);

                    // set up the typical prediction context
                    tpgrCX0 = tpgrCX1 = tpgrCX2 = 0; // make gcc happy
                    if (tpgrOn)
                    {
                        refBitmap.GetPixelPtr(-1 - refDX, y - 1 - refDY, ref tpgrCXPtr0);
                        tpgrCX0 = (uint)refBitmap.NextPixel(ref tpgrCXPtr0);
                        tpgrCX0 = (tpgrCX0 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr0);
                        tpgrCX0 = (tpgrCX0 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr0);
                        refBitmap.GetPixelPtr(-1 - refDX, y - refDY, ref tpgrCXPtr1);
                        tpgrCX1 = (uint)refBitmap.NextPixel(ref tpgrCXPtr1);
                        tpgrCX1 = (tpgrCX1 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr1);
                        tpgrCX1 = (tpgrCX1 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr1);
                        refBitmap.GetPixelPtr(-1 - refDX, y + 1 - refDY, ref tpgrCXPtr2);
                        tpgrCX2 = (uint)refBitmap.NextPixel(ref tpgrCXPtr2);
                        tpgrCX2 = (tpgrCX2 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr2);
                        tpgrCX2 = (tpgrCX2 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr2);
                    }

                    for (x = 0; x < w; ++x)
                    {

                        // update the context
                        cx0 = ((cx0 << 1) | (uint)bitmap.NextPixel(ref cxPtr0)) & 3;
                        cx2 = ((cx2 << 1) | (uint)refBitmap.NextPixel(ref cxPtr2)) & 3;
                        cx3 = ((cx3 << 1) | (uint)refBitmap.NextPixel(ref cxPtr3)) & 7;
                        cx4 = ((cx4 << 1) | (uint)refBitmap.NextPixel(ref cxPtr4)) & 7;

                        if (tpgrOn)
                        {
                            // update the typical predictor context
                            tpgrCX0 = ((tpgrCX0 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr0)) & 7;
                            tpgrCX1 = ((tpgrCX1 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr1)) & 7;
                            tpgrCX2 = ((tpgrCX2 << 1) | (uint)refBitmap.NextPixel(ref tpgrCXPtr2)) & 7;

                            // check for a "typical" pixel
                            if (m_arithDecoder.DecodeBit(ltpCX, m_refinementRegionStats) != 0)
                            {
                                ltp = !ltp;
                            }
                            if (tpgrCX0 == 0 && tpgrCX1 == 0 && tpgrCX2 == 0)
                            {
                                bitmap.ClearPixel(x, y);
                                continue;
                            }
                            else if (tpgrCX0 == 7 && tpgrCX1 == 7 && tpgrCX2 == 7)
                            {
                                bitmap.SetPixel(x, y);
                                continue;
                            }
                        }

                        // build the context
                        cx = (cx0 << 11) | (uint)bitmap.NextPixel(ref cxPtr1) << 10 |
                             (cx2 << 8) | (cx3 << 5) | (cx4 << 2) |
                             (uint)(bitmap.NextPixel(ref cxPtr5) << 1) |
                             (uint)refBitmap.NextPixel(ref cxPtr6);

                        // decode the pixel
                        if ((pix = m_arithDecoder.DecodeBit(cx, m_refinementRegionStats)) != 0)
                        {
                            bitmap.SetPixel(x, y);
                        }
                    }
                }
            }

            return bitmap;
        }

        private void discardSegment(uint segNum)
        {
            JBIG2Segment seg;
            int i;

            for (i = 0; i < m_globalSegments.Count; ++i)
            {
                seg = (JBIG2Segment)m_globalSegments[i];
                if (seg.SegNum == segNum)
                {
                    m_globalSegments.RemoveAt(i);
                    return;
                }
            }

            for (i = 0; i < m_segments.Count; ++i)
            {
                seg = (JBIG2Segment)m_segments[i];
                if (seg.SegNum == segNum)
                {
                    m_segments.RemoveAt(i);
                    return;
                }
            }
        }

        private JBIG2Segment findSegment(uint segNum)
        {
            JBIG2Segment seg;
            int i;

            for (i = 0; i < m_globalSegments.Count; ++i)
            {
                seg = (JBIG2Segment)m_globalSegments[i];
                if (seg.SegNum == segNum)
                {
                    return seg;
                }
            }

            if (m_segments == null)
                return null;
            for (i = 0; i < m_segments.Count; ++i)
            {
                seg = (JBIG2Segment)m_segments[i];
                if (seg.SegNum == segNum)
                {
                    return seg;
                }
            }
            return null;
        }

        private void readPatternDictSeg(uint segNum, uint length)
        {
            JBIG2PatternDict patternDict;
            JBIG2Bitmap bitmap;
            uint flags = 0, patternW = 0, patternH = 0, grayMax = 0, templ, mmr;
            int[] atx = new int[4], aty = new int[4];
            uint i, x;

            // halftone dictionary flags, pattern width and height, max gray value
            if (!readUByte(ref flags) ||
                !readUByte(ref patternW) ||
                !readUByte(ref patternH) ||
                !readULong(ref grayMax))
            {
                return;
            }
            templ = (flags >> 1) & 3;
            mmr = flags & 1;

            // set up the arithmetic decoder
            if (mmr == 0)
            {
                resetGenericStats(templ, null);
                m_arithDecoder.Start();
            }

            // read the bitmap
            atx[0] = -(int)patternW; aty[0] = 0;
            atx[1] = -3; aty[1] = -1;
            atx[2] = 2; aty[2] = -2;
            atx[3] = -2; aty[3] = -2;
            bitmap = readGenericBitmap(mmr != 0, (int)((grayMax + 1) * patternW), (int)patternH,
                           (int)templ, false, false, null,
                           atx, aty, (int)length - 7);

            // create the pattern dict object
            patternDict = new JBIG2PatternDict(segNum, (int)grayMax + 1);

            // split up the bitmap
            x = 0;
            for (i = 0; i <= grayMax; ++i)
            {
                patternDict.SetBitmap((int)i, bitmap.GetSlice(x, 0, patternW, patternH));
                x += patternW;
            }


            // store the new pattern dict
            m_segments.Add(patternDict);

            return;
        }

        private JBIG2Bitmap readGenericBitmap(bool mmr, int w, int h,
                        int templ, bool tpgdOn,
                        bool useSkip, JBIG2Bitmap skip,
                        int[] atx, int[] aty,
                        int mmrDataLength)
        {
            JBIG2Bitmap bitmap;
            bool ltp;
            uint ltpCX, cx, cx0, cx1, cx2;
            JBIG2BitmapPtr cxPtr0 = new JBIG2BitmapPtr(), cxPtr1 = new JBIG2BitmapPtr();
            JBIG2BitmapPtr atPtr0 = new JBIG2BitmapPtr(), atPtr1 = new JBIG2BitmapPtr(),
                atPtr2 = new JBIG2BitmapPtr(), atPtr3 = new JBIG2BitmapPtr();
            int[] refLine = null, codingLine = null;
            
            int x = 0, y, a0 = 0, pix, refI = 0, codingI = 0;

            bitmap = new JBIG2Bitmap(0, w, h);
            bitmap.ClearToZero();

            //----- MMR decode

            if (mmr)
            {

                mmrDecode(ref refLine, ref codingLine, ref w, ref h, ref refI, ref codingI, ref a0,
                    ref x, ref mmrDataLength, ref bitmap);
                //----- arithmetic decode
            }
            else
            {
                // set up the typical row context
                ltpCX = 0; // make gcc happy
                if (tpgdOn)
                {
                    switch (templ)
                    {
                        case 0:
                            ltpCX = 0x3953; // 001 11001 0101 0011
                            break;
                        case 1:
                            ltpCX = 0x079a; // 0011 11001 101 0
                            break;
                        case 2:
                            ltpCX = 0x0e3; // 001 1100 01 1
                            break;
                        case 3:
                            ltpCX = 0x18a; // 01100 0101 1
                            break;
                    }
                }

                ltp = false;
                cx = cx0 = cx1 = cx2 = 0; // make gcc happy
                for (y = 0; y < h; ++y)
                {

                    // check for a "typical" (duplicate) row
                    if (tpgdOn)
                    {
                        if (m_arithDecoder.DecodeBit(ltpCX, m_genericRegionStats) != 0)
                        {
                            ltp = !ltp;
                        }
                        if (ltp)
                        {
                            bitmap.DuplicateRow(y, y - 1);
                            continue;
                        }
                    }

                    switch (templ)
                    {
                        case 0:

                            // set up the context
                            bitmap.GetPixelPtr(0, y - 2, ref cxPtr0);
                            cx0 = (uint)bitmap.NextPixel(ref cxPtr0);
                            cx0 = (cx0 << 1) | (uint)bitmap.NextPixel(ref cxPtr0);
                            bitmap.GetPixelPtr(0, y - 1, ref cxPtr1);
                            cx1 = (uint)bitmap.NextPixel(ref cxPtr1);
                            cx1 = (cx1 << 1) | (uint)bitmap.NextPixel(ref cxPtr1);
                            cx1 = (cx1 << 1) | (uint)bitmap.NextPixel(ref cxPtr1);
                            cx2 = 0;
                            bitmap.GetPixelPtr(atx[0], y + aty[0], ref atPtr0);
                            bitmap.GetPixelPtr(atx[1], y + aty[1], ref atPtr1);
                            bitmap.GetPixelPtr(atx[2], y + aty[2], ref atPtr2);
                            bitmap.GetPixelPtr(atx[3], y + aty[3], ref atPtr3);

                            // decode the row
                            for (x = 0; x < w; ++x)
                            {

                                // build the context
                                cx = (cx0 << 13) | (cx1 << 8) | (cx2 << 4) |
                                     (uint)(bitmap.NextPixel(ref atPtr0) << 3) |
                                     (uint)(bitmap.NextPixel(ref atPtr1) << 2) |
                                     (uint)(bitmap.NextPixel(ref atPtr2) << 1) |
                                     (uint)bitmap.NextPixel(ref atPtr3);

                                // check for a skipped pixel
                                if (useSkip && skip.GetPixel(x, y) != 0)
                                {
                                    pix = 0;

                                    // decode the pixel
                                }
                                else if ((pix = m_arithDecoder.DecodeBit(cx, m_genericRegionStats)) != 0)
                                {
                                    bitmap.SetPixel(x, y);
                                }

                     
                                // update the context
                                cx0 = (uint)((cx0 << 1) | (uint)bitmap.NextPixel(ref cxPtr0)) & 0x07;
                                cx1 = (uint)((cx1 << 1) | (uint)bitmap.NextPixel(ref cxPtr1)) & 0x1f;
                                cx2 = (uint)((cx2 << 1) | (uint)pix) & 0x0f;
                            }
                            break;

                        case 1:

                            // set up the context
                            bitmap.GetPixelPtr(0, y - 2, ref cxPtr0);
                            cx0 = (uint)bitmap.NextPixel(ref cxPtr0);
                            cx0 = (uint)(cx0 << 1) | (uint)bitmap.NextPixel(ref cxPtr0);
                            cx0 = (uint)(cx0 << 1) | (uint)bitmap.NextPixel(ref cxPtr0);
                            bitmap.GetPixelPtr(0, y - 1, ref cxPtr1);
                            cx1 = (uint)bitmap.NextPixel(ref cxPtr1);
                            cx1 = (cx1 << 1) | (uint)bitmap.NextPixel(ref cxPtr1);
                            cx1 = (cx1 << 1) | (uint)bitmap.NextPixel(ref cxPtr1);
                            cx2 = 0;
                            bitmap.GetPixelPtr(atx[0], y + aty[0], ref atPtr0);

                            // decode the row
                            for (x = 0; x < w; ++x)
                            {

                                // build the context
                                cx = (cx0 << 9) | (cx1 << 4) | (cx2 << 1) |
                                     (uint)bitmap.NextPixel(ref atPtr0);

                                // check for a skipped pixel
                                if (useSkip && skip.GetPixel(x, y) != 0)
                                {
                                    pix = 0;

                                    // decode the pixel
                                }
                                else if ((pix = m_arithDecoder.DecodeBit(cx, m_genericRegionStats)) != 0)
                                {
                                    bitmap.SetPixel(x, y);
                                }

                                // update the context
                                cx0 = ((cx0 << 1) | (uint)bitmap.NextPixel(ref cxPtr0)) & 0x0f;
                                cx1 = ((cx1 << 1) | (uint)bitmap.NextPixel(ref cxPtr1)) & 0x1f;
                                cx2 = ((cx2 << 1) | (uint)pix) & 0x07;
                            }
                            break;

                        case 2:

                            // set up the context
                            bitmap.GetPixelPtr(0, y - 2, ref cxPtr0);
                            cx0 = (uint)bitmap.NextPixel(ref cxPtr0);
                            cx0 = (cx0 << 1) | (uint)bitmap.NextPixel(ref cxPtr0);
                            bitmap.GetPixelPtr(0, y - 1, ref cxPtr1);
                            cx1 = (uint)bitmap.NextPixel(ref cxPtr1);
                            cx1 = (cx1 << 1) | (uint)bitmap.NextPixel(ref cxPtr1);
                            cx2 = 0;
                            bitmap.GetPixelPtr(atx[0], y + aty[0], ref atPtr0);

                            // decode the row
                            for (x = 0; x < w; ++x)
                            {

                                // build the context
                                cx = (cx0 << 7) | (cx1 << 3) | (cx2 << 1) |
                                     (uint)bitmap.NextPixel(ref atPtr0);

                                // check for a skipped pixel
                                if (useSkip && skip.GetPixel(x, y) != 0)
                                {
                                    pix = 0;

                                    // decode the pixel
                                }
                                else if ((pix = m_arithDecoder.DecodeBit(cx, m_genericRegionStats)) != 0)
                                {
                                    bitmap.SetPixel(x, y);
                                }

                                // update the context
                                cx0 = ((cx0 << 1) | (uint)bitmap.NextPixel(ref cxPtr0)) & 0x07;
                                cx1 = ((cx1 << 1) | (uint)bitmap.NextPixel(ref cxPtr1)) & 0x0f;
                                cx2 = ((cx2 << 1) | (uint)pix) & 0x03;
                            }
                            break;

                        case 3:

                            // set up the context
                            bitmap.GetPixelPtr(0, y - 1, ref cxPtr1);
                            cx1 = (uint)bitmap.NextPixel(ref cxPtr1);
                            cx1 = (cx1 << 1) | (uint)bitmap.NextPixel(ref cxPtr1);
                            cx2 = 0;
                            bitmap.GetPixelPtr(atx[0], y + aty[0], ref atPtr0);

                            // decode the row
                            for (x = 0; x < w; ++x)
                            {

                                // build the context
                                cx = (cx1 << 5) | (cx2 << 1) |
                                     (uint)bitmap.NextPixel(ref atPtr0);

                                // check for a skipped pixel
                                if (useSkip && skip.GetPixel(x, y) != 0)
                                {
                                    pix = 0;

                                    // decode the pixel
                                }
                                else if ((pix = m_arithDecoder.DecodeBit(cx, m_genericRegionStats)) != 0)
                                {
                                    bitmap.SetPixel(x, y);
                                }

                                // update the context
                                cx1 = ((cx1 << 1) | (uint)bitmap.NextPixel(ref cxPtr1)) & 0x1f;
                                cx2 = ((cx2 << 1) | (uint)pix) & 0x0f;
                            }
                            break;
                    }
                }
            }

            return bitmap;
        }

        private void mmrDecode(ref int[] refLine, ref int[] codingLine, ref int w, ref int h, ref int refI, ref int codingI, 
            ref int a0, ref int x, ref int mmrDataLength, ref JBIG2Bitmap bitmap)
        {
            int code1 = 0, code2 = 0, code3 = 0;
            int i = 0;

            m_mmrDecoder.Reset();
            refLine = new int[w + 2];
            codingLine = new int[w + 2];
            codingLine[0] = codingLine[1] = w;

            for (int y = 0; y < h; ++y)
            {

                // copy coding line to ref line
                for (i = 0; codingLine[i] < w; ++i)
                {
                    refLine[i] = codingLine[i];
                }
                refLine[i] = refLine[i + 1] = w;

                // decode a line
                refI = 0;     // b1 = refLine[refI]
                codingI = 0;  // a1 = codingLine[codingI]
                a0 = 0;
                do
                {
                    code1 = m_mmrDecoder.Get2DCode();
                    switch (code1)
                    {
                        case CCITTCodeTables.TwoDimPass:
                            if (refLine[refI] < w)
                            {
                                a0 = refLine[refI + 1];
                                refI += 2;
                            }
                            break;
                        case CCITTCodeTables.TwoDimHoriz:
                            if ((codingI & 1) != 0)
                            {
                                code1 = 0;
                                do
                                {
                                    code1 += code3 = m_mmrDecoder.GetBlackCode();
                                } while (code3 >= 64);
                                code2 = 0;
                                do
                                {
                                    code2 += code3 = m_mmrDecoder.GetWhiteCode();
                                } while (code3 >= 64);
                            }
                            else
                            {
                                code1 = 0;
                                do
                                {
                                    code1 += code3 = m_mmrDecoder.GetWhiteCode();
                                } while (code3 >= 64);
                                code2 = 0;
                                do
                                {
                                    code2 += code3 = m_mmrDecoder.GetBlackCode();
                                } while (code3 >= 64);
                            }
                            if (code1 > 0 || code2 > 0)
                            {
                                a0 = codingLine[codingI++] = a0 + code1;
                                a0 = codingLine[codingI++] = a0 + code2;
                                while (refLine[refI] <= a0 && refLine[refI] < w)
                                {
                                    refI += 2;
                                }
                            }
                            break;
                        case CCITTCodeTables.TwoDimVert0:
                            a0 = codingLine[codingI++] = refLine[refI];
                            if (refLine[refI] < w)
                            {
                                ++refI;
                            }
                            break;
                        case CCITTCodeTables.TwoDimVertR1:
                            a0 = codingLine[codingI++] = refLine[refI] + 1;
                            if (refLine[refI] < w)
                            {
                                ++refI;
                                while (refLine[refI] <= a0 && refLine[refI] < w)
                                {
                                    refI += 2;
                                }
                            }
                            break;
                        case CCITTCodeTables.TwoDimVertR2:
                            a0 = codingLine[codingI++] = refLine[refI] + 2;
                            if (refLine[refI] < w)
                            {
                                ++refI;
                                while (refLine[refI] <= a0 && refLine[refI] < w)
                                {
                                    refI += 2;
                                }
                            }
                            break;
                        case CCITTCodeTables.TwoDimVertR3:
                            a0 = codingLine[codingI++] = refLine[refI] + 3;
                            if (refLine[refI] < w)
                            {
                                ++refI;
                                while (refLine[refI] <= a0 && refLine[refI] < w)
                                {
                                    refI += 2;
                                }
                            }
                            break;
                        case CCITTCodeTables.TwoDimVertL1:
                            a0 = codingLine[codingI++] = refLine[refI] - 1;
                            if (refI > 0)
                            {
                                --refI;
                            }
                            else
                            {
                                ++refI;
                            }
                            while (refLine[refI] <= a0 && refLine[refI] < w)
                            {
                                refI += 2;
                            }
                            break;
                        case CCITTCodeTables.TwoDimVertL2:
                            a0 = codingLine[codingI++] = refLine[refI] - 2;
                            if (refI > 0)
                            {
                                --refI;
                            }
                            else
                            {
                                ++refI;
                            }
                            while (refLine[refI] <= a0 && refLine[refI] < w)
                            {
                                refI += 2;
                            }
                            break;
                        case CCITTCodeTables.TwoDimVertL3:
                            a0 = codingLine[codingI++] = refLine[refI] - 3;
                            if (refI > 0)
                            {
                                --refI;
                            }
                            else
                            {
                                ++refI;
                            }
                            while (refLine[refI] <= a0 && refLine[refI] < w)
                            {
                                refI += 2;
                            }
                            break;
                        default:
                            break;
                    }
                } while (a0 < w);
                codingLine[codingI++] = w;

                // convert the run lengths to a bitmap line
                i = 0;
                while (codingLine[i] < w)
                {
                    for (x = codingLine[i]; x < codingLine[i + 1]; ++x)
                    {
                        bitmap.SetPixel(x, y);
                    }
                    i += 2;
                }
            }

            if (mmrDataLength >= 0)
            {
                m_mmrDecoder.skipTo(mmrDataLength);
            }
            else
            {
                if (m_mmrDecoder.Get24Bits() != 0x001001)
                {
                }
            }
        }

        private void resetGenericStats(uint templ,
                    JArithmeticDecoderStats prevStats)
        {
            int size;

            size = contextSize[templ];
            if (prevStats != null && prevStats.ContextSize == size)
            {
                if (m_genericRegionStats.ContextSize == size)
                {
                    m_genericRegionStats.CopyFrom(prevStats);
                }
                else
                {
                    m_genericRegionStats = prevStats.Copy();
                }
            }
            else
            {
                if (m_genericRegionStats.ContextSize == size)
                {
                    m_genericRegionStats.Reset();
                }
                else
                {
                    m_genericRegionStats = new JArithmeticDecoderStats(1 << size);
                }
            }
        }

        private void resetRefinementStats(uint templ,
                       JArithmeticDecoderStats prevStats)
        {
            int size;

            size = refContextSize[templ];
            if (prevStats != null && prevStats.ContextSize == size)
            {
                if (m_refinementRegionStats.ContextSize == size)
                {
                    m_refinementRegionStats.CopyFrom(prevStats);
                }
                else
                {
                    m_refinementRegionStats = prevStats.Copy();
                }
            }
            else
            {
                if (m_refinementRegionStats.ContextSize == size)
                {
                    m_refinementRegionStats.Reset();
                }
                else
                {
                    m_refinementRegionStats = new JArithmeticDecoderStats(1 << size);
                }
            }
        }

        private void resetIntStats(int symCodeLen)
        {
            m_iadhStats.Reset();
            m_iadwStats.Reset();
            m_iaexStats.Reset();
            m_iaaiStats.Reset();
            m_iadtStats.Reset();
            m_iaitStats.Reset();
            m_iafsStats.Reset();
            m_iadsStats.Reset();
            m_iardxStats.Reset();
            m_iardyStats.Reset();
            m_iardwStats.Reset();
            m_iardhStats.Reset();
            m_iariStats.Reset();
            if (m_iaidStats.ContextSize == 1 << (symCodeLen + 1))
            {
                m_iaidStats.Reset();
            }
            else
            {
                m_iaidStats = new JArithmeticDecoderStats(1 << (symCodeLen + 1));
            }
        }


        private bool readUByte(ref uint x)
        {
            int c0;

            if ((c0 = m_curStr.ReadByte()) == -1)
            {
                return false;
            }
            x = (uint)c0;
            return true;
        }

        private bool readByte(ref int x)
        {
            int c0;

            if ((c0 = m_curStr.ReadByte()) == -1)
            {
                return false;
            }
            x = c0;
            if ((c0 & 0x80) != 0)
            {
                x |= -1 - 0xff;
            }
            return true;
        }

        private bool readUWord(ref uint x)
        {
            int c0, c1;

            if ((c0 = m_curStr.ReadByte()) == -1 ||
                (c1 = m_curStr.ReadByte()) == -1)
            {
                return false;
            }
            x = (uint)((c0 << 8) | c1);
            return true;
        }

        private bool readULong(ref uint x)
        {
            int c0, c1, c2, c3;

            if ((c0 = m_curStr.ReadByte()) == -1 ||
                (c1 = m_curStr.ReadByte()) == -1 ||
                (c2 = m_curStr.ReadByte()) == -1 ||
                (c3 = m_curStr.ReadByte()) == -1)
            {
                return false;
            }
            x = (uint)((c0 << 24) | (c1 << 16) | (c2 << 8) | c3);
            return true;
        }

        private bool readLong(ref int x)
        {
            int c0, c1, c2, c3;

            if ((c0 = m_curStr.ReadByte()) == -1 ||
                (c1 = m_curStr.ReadByte()) == -1 ||
                (c2 = m_curStr.ReadByte()) == -1 ||
                (c3 = m_curStr.ReadByte()) == -1)
            {
                return false;
            }
            x = ((c0 << 24) | (c1 << 16) | (c2 << 8) | c3);
            if ((c0 & 0x80) != 0)
            {
                uint temp = 0xffffffff;
                x |= -1 - (int)temp;
            }
            return true;
        }

        private uint m_pageW, m_pageH, m_curPageH;
        private uint m_pageDefPixel;
        private JBIG2Bitmap m_pageBitmap;
        private uint m_defCombOp;
        private List<Object> m_segments;		// [JBIG2Segment]
        private List<Object> m_globalSegments;	// [JBIG2Segment]
        private Stream m_curStr;
        private Stream m_globalStr;
        private Stream m_str;
        private byte[] m_dataPtr;

        private JArithmeticDecoder m_arithDecoder;
        private JArithmeticDecoderStats m_genericRegionStats;
        private JArithmeticDecoderStats m_refinementRegionStats;
        private JArithmeticDecoderStats m_iadhStats;
        private JArithmeticDecoderStats m_iadwStats;
        private JArithmeticDecoderStats m_iaexStats;
        private JArithmeticDecoderStats m_iaaiStats;
        private JArithmeticDecoderStats m_iadtStats;
        private JArithmeticDecoderStats m_iaitStats;
        private JArithmeticDecoderStats m_iafsStats;
        private JArithmeticDecoderStats m_iadsStats;
        private JArithmeticDecoderStats m_iardxStats;
        private JArithmeticDecoderStats m_iardyStats;
        private JArithmeticDecoderStats m_iardwStats;
        private JArithmeticDecoderStats m_iardhStats;
        private JArithmeticDecoderStats m_iariStats;
        private JArithmeticDecoderStats m_iaidStats;
        private JBIG2HuffmanDecoder m_huffDecoder;
        private JBIG2MMRDecoder m_mmrDecoder;

        private static int[] contextSize = { 16, 13, 10, 10 };
        private static int[] refContextSize = { 13, 10 };
    }
}
