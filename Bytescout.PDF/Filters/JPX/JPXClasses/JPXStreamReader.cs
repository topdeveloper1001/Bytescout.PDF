using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class JPXStreamReader
    {
        public JPXStreamReader(Stream stream)
        {
            // First step, check the file.
            if (!checkTheFile(stream))
                throw new NotImplementedException();

            // Create image.
            m_image = new JPXImage();

            // Read the main boxes.
            if (!readBoxes(stream))
                throw new NotImplementedException();

            // Finish decoding Image.
            if (!finishDecodingImage())
                throw new NotImplementedException();

            m_stream = buildStream();
        }

        internal MemoryStream Stream
        {
            get { return m_stream; }
        }

        private MemoryStream buildStream()
        {
            MemoryStream ms = new MemoryStream();

            foreach (JPXTile tile in m_image.TileCollection)
                for (int i = 0; i < m_image.XSize * m_image.YSize; ++i)
                    for (int j = 0; j < m_image.NComps; ++j)
                        ms.WriteByte((byte)(tile.TileCompCollection[j].Data[i]));

            return ms;
        }

        private bool checkTheFile(Stream stream)
        {
            // Get the first box. This should be a JP box. 
            JPXBox box = new JPXBox();
            box.CreateBox(stream);
            if (box.Type != JPXBoxType.JP2_BOX_JP)
                return false;

            // Get the second box. This should be a FTYP box.
            box.CreateBox(stream);
            if (box.Type != JPXBoxType.JP2_BOX_FTYP)
                return false;

            return true;
        }


        private bool readBoxes(Stream stream)
        {
            JPXBox box = new JPXBox();
            bool found = false;

            while (box.CreateBox(stream))
            {
                switch (box.Type)
                {
                    case JPXBoxType.JP2_BOX_JP2C:
                        if (!readCodeStream(stream))
                            return false;
                        found = true;
                        break;
                    case JPXBoxType.JP2_BOX_IHDR:
                        if (!readImageHeader(box))
                            return false;
                        break;
                    case JPXBoxType.JP2_BOX_BPCC:

                        break;
                    case JPXBoxType.JP2_BOX_CDEF:

                        break;
                    case JPXBoxType.JP2_BOX_PCLR:

                        break;
                    case JPXBoxType.JP2_BOX_CMAP:

                        break;
                    case JPXBoxType.JP2_BOX_COLR:
                        if (!readColorSpecification(box))
                            return false;
                        break;
                    case JPXBoxType.JP2_BOX_RESD:
                        readResolutionDefault(box);
                        break;
                }
                if (found)
                    break;
            }
            return true;
        }

        private void readResolutionDefault(JPXBox box)
        {
            int rn1 = 0, rn2 = 0, rd1 = 0, rd2 = 0, re1 = 0, re2 = 0;

            JPXUtilities.GetInt16(box.Data, ref rn1);
            JPXUtilities.GetInt16(box.Data, ref rd1);
            JPXUtilities.GetInt16(box.Data, ref rn2);
            JPXUtilities.GetInt16(box.Data, ref rd2);
            JPXUtilities.GetInt8(box.Data, ref re1);
            JPXUtilities.GetInt8(box.Data, ref re2);

            int vcr = (int)((rn1 / rd1) * Math.Pow(10, re1));
            int hcr = (int)((rn2 / rd2) * Math.Pow(10, re2));

            if (vcr == hcr)
                return;
        }

        private bool finishDecodingImage()
        {
            foreach (JPXTile tile in m_image.TileCollection)
            {
                foreach (JPXTileComp tileComp in tile.TileCompCollection)
                    inverseTransformation(tileComp);

                if (!inverseMultiComponent(tile)) return false;

                foreach (JPXTileComp tileComp in tile.TileCompCollection)
                    inverseDC(tileComp);
            }

            return true;
        }

        private bool inverseMultiComponent(JPXTile tile)
        {
            if (tile.MultiComponent == 1)
            {
                if (m_image.NComps < 3 ||
                    tile.TileCompCollection[0].HSeparation != tile.TileCompCollection[1].HSeparation ||
                    tile.TileCompCollection[0].VSeparation != tile.TileCompCollection[1].VSeparation ||
                    tile.TileCompCollection[2].HSeparation != tile.TileCompCollection[1].HSeparation ||
                    tile.TileCompCollection[2].VSeparation != tile.TileCompCollection[1].VSeparation
                )
                    return false;

                if (tile.TileCompCollection[0].Transform == 0)
                {
                    int j = 0;
                    for (int y = 0; y < tile.TileCompCollection[0].Y1 - tile.TileCompCollection[0].Y0; ++y)
                        for (int x = 0; x < tile.TileCompCollection[0].X1 - tile.TileCompCollection[0].X0; ++x)
                        {
                            int d0 = tile.TileCompCollection[0].Data[j];
                            int d1 = tile.TileCompCollection[1].Data[j];
                            int d2 = tile.TileCompCollection[2].Data[j];

                            tile.TileCompCollection[0].Data[j] = (int)(d0 + 1.402 * d2 + 0.5);
                            tile.TileCompCollection[1].Data[j] = (int)(d0 - 0.34413 * d1 - 0.71414 * d2 + 0.5);
                            tile.TileCompCollection[2].Data[j] = (int)(d0 + 1.772 * d1 + 0.5);

                            ++j;
                        }
                }

                else
                {
                    int j = 0;
                    int t = 0;
                    for (int y = 0; y < tile.TileCompCollection[0].Y1 - tile.TileCompCollection[0].Y0; ++y)
                        for (int x = 0; x < tile.TileCompCollection[0].X1 - tile.TileCompCollection[0].X0; ++x)
                        {
                            int d0 = tile.TileCompCollection[0].Data[j];
                            int d1 = tile.TileCompCollection[1].Data[j];
                            int d2 = tile.TileCompCollection[2].Data[j];

                            tile.TileCompCollection[1].Data[j] = t = d0 - ((d2 + d1) >> 2);
                            tile.TileCompCollection[2].Data[j] = d1 + t;
                            tile.TileCompCollection[0].Data[j] = d2 + t;

                            ++j;
                        }
                }
            }
            return true;
        }

        private void inverseDC(JPXTileComp tileComp)
        {
            if (tileComp.Signed)
            {
                int minVal = -(1 << (tileComp.Precision - 1));
                int maxVal = -minVal - 1;
                int data_i = 0;

                for (int y = 0; y < tileComp.Y1 - tileComp.Y0; ++y)
                    for (int x = 0; x < tileComp.X1 - tileComp.X0; ++x)
                    {
                        int coeff = tileComp.Data[data_i];

                        if (tileComp.Transform == 0) coeff >>= 16;

                        coeff = Math.Max(coeff, minVal);
                        coeff = Math.Min(coeff, maxVal);

                        tileComp.Data[data_i++] = coeff;
                    }
            }
            else
            {
                int maxVal = (1 << tileComp.Precision) - 1;
                int zeroVal = (1 << tileComp.Precision - 1);
                int data_i = 0;

                for (int y = 0; y < tileComp.Y1 - tileComp.Y0; ++y)
                    for (int x = 0; x < tileComp.X1 - tileComp.X0; ++x)
                    {
                        int coeff = tileComp.Data[data_i];

                        if (tileComp.Transform == 0) coeff >>= 16;

                        coeff += zeroVal;
                        coeff = Math.Max(coeff, 0);
                        coeff = Math.Min(coeff, maxVal);

                        tileComp.Data[data_i++] = coeff;
                    }
            }
        }

        private void inverseTransformation(JPXTileComp tileComp)
        {
            JPXResLevel resLevel = tileComp.ResLevelCollection[0];
            JPXPrecint precint = resLevel.Precint;
            JPXSubband subband = precint.Subbands[0];
            int qStyle = 0, guard = 0, eps = 0, shift;
            double mu = 0;

            qStyle = tileComp.QuantStyle & 0x1f;
            guard = (tileComp.QuantStyle >> 5) & 7;
            if (qStyle == 0)
            {
                eps = (tileComp.QuantSteps[0] >> 3) & 0x1f;
                shift = guard + eps - 1;
            }
            else
            {
                shift = guard - 1 + tileComp.Precision;
                mu = (double)(0x800 + (tileComp.QuantSteps[0] & 0x7ff)) / 2048.0;
            }
            if (tileComp.Transform == 0) shift += 16;

            copyNL(tileComp, subband, subband.CodeBlocks, shift, qStyle, mu);

            idwt(tileComp);
        }

        private void copyNL(JPXTileComp tileComp, JPXSubband subband, JPXCodeBlock[] cb,
            int shift, int qStyle, double mu)
        {
            int cb_i = 0;
            int cbW = (int)Math.Pow(2, tileComp.CodeBlockW);
            for (int cbY = 0; cbY < subband.NYCBs; ++cbY)
                for (int cbX = 0; cbX < subband.NXCBs; ++cbX)
                {
                    for (int y = cb[cb_i].Y0, coeff0_i = 0; y < cb[cb_i].Y1; ++y, coeff0_i += cbW)
                    {
                        int index = (y - subband.Y0) * (tileComp.X1 - tileComp.X0) + (cb[cb_i].X0 - subband.X0);
                        for (int x = cb[cb_i].X0, coeff_i = coeff0_i; x < cb[cb_i].X1; ++x, ++coeff_i)
                        {
                            int val = cb[cb_i].Coeff[coeff_i].Magnitude;
                            if (val != 0)
                            {
                                int shift2 = shift - (cb[cb_i].NZeroBitPlanes + cb[cb_i].Coeff[coeff_i].Length);
                                if (shift2 > 0)
                                    val = (val << shift2) + (1 << (shift2 - 1));
                                else
                                    val >>= -shift2;
                                if (qStyle == 0)
                                {
                                    if (tileComp.Transform == 0)
                                        val &= -1 << 16;
                                }
                                else
                                    val = (int)(val * mu);
                                if ((cb[cb_i].Coeff[coeff_i].Flags & (short)CoefficientFlags.jpxCoeffSign) != 0)
                                    val = -val;
                            }
                            tileComp.Data[index++] = val;
                        }
                    }
                    ++cb_i;
                }
        }

        // Inverse Discrete Wavelet Transformation
        private void idwt(JPXTileComp tileComp)
        {
            int nx0, nx1, ny0, ny1;
            for (int r = 1; r <= tileComp.NDecompositionLevel; ++r)
            {
                JPXResLevel resLevel = tileComp.ResLevelCollection[r];

                if (r == tileComp.NDecompositionLevel)
                {
                    nx0 = tileComp.X0;
                    nx1 = tileComp.X1;
                    ny0 = tileComp.Y0;
                    ny1 = tileComp.Y1;
                }
                else
                {
                    nx0 = tileComp.ResLevelCollection[r + 1].X0;
                    nx1 = tileComp.ResLevelCollection[r + 1].X1;
                    ny0 = tileComp.ResLevelCollection[r + 1].Y0;
                    ny1 = tileComp.ResLevelCollection[r + 1].Y1;
                }

                inverseTransformationLevel(tileComp, r, resLevel, nx0, ny0, nx1, ny1);
            }
        }

        private void inverseTransformationLevel(JPXTileComp tileComp, int r, JPXResLevel resLevel,
            int nx0, int ny0,
            int nx1, int ny1)
        {
            for (int yy = resLevel.Y1 - 1; yy >= resLevel.Y0; --yy)
                for (int xx = resLevel.X1 - 1; xx >= resLevel.X0; --xx)
                    tileComp.Data[(2 * yy - ny0) * (tileComp.X1 - tileComp.X0) + (2 * xx - nx0)] =
                        tileComp.Data[(yy - resLevel.Y0) * (tileComp.X1 - tileComp.X0) + (xx - resLevel.X0)];

            interLeave(tileComp, resLevel, r, nx0, ny0, nx1, ny1);

            // horizontal transforms
            int dataPtr_i = 0;
            for (int y = 0; y < ny1 - ny0; ++y)
            {
                inverseTransformation1D(tileComp, dataPtr_i, 1, nx0, nx1);
                dataPtr_i += tileComp.X1 - tileComp.X0;
            }

            // vertical transforms
            dataPtr_i = 0;
            for (int x = 0; x < nx1 - nx0; ++x)
            {
                inverseTransformation1D(tileComp, dataPtr_i, tileComp.X1 - tileComp.X0, ny0, ny1);
                ++dataPtr_i;
            }
        }

        private void inverseTransformation1D(JPXTileComp tileComp, int data_i, int stride, int i0, int i1)
        {
            if (i1 - i0 == 1)
            {
                if ((i0 & 1) != 0)
                    tileComp.Data[data_i] >>= 1;
            }
            else
            {
                int offset = 3 + (i0 & 1);
                int end = offset + i1 - i0;

                for (int i = 0; i < i1 - i0; ++i)
                    tileComp.Buffer[offset + i] = tileComp.Data[data_i + i * stride];

                tileComp.Buffer[end] = tileComp.Buffer[end - 2];

                extendRight(tileComp, i0, i1, offset, end);
                extendLeft(tileComp, offset);

                if (tileComp.Transform == 0)
                {
                    for (int i = 1; i <= end + 2; i += 2)
                        tileComp.Buffer[i] = (int)(JPXConsts.IDWTKappa * tileComp.Buffer[i]);

                    for (int i = 0; i <= end + 3; i += 2)
                        tileComp.Buffer[i] = (int)(JPXConsts.IDWTIKappa * tileComp.Buffer[i]);

                    for (int i = 1; i <= end + 2; i += 2)
                        tileComp.Buffer[i] = (int)(tileComp.Buffer[i] - JPXConsts.IDWTDelta
                            * (tileComp.Buffer[i - 1] + tileComp.Buffer[i + 1]));

                    for (int i = 2; i <= end + 1; i += 2)
                        tileComp.Buffer[i] = (int)(tileComp.Buffer[i] - JPXConsts.IDWTGamma
                            * (tileComp.Buffer[i - 1] + tileComp.Buffer[i + 1]));

                    for (int i = 3; i <= end; i += 2)
                        tileComp.Buffer[i] = (int)(tileComp.Buffer[i] - JPXConsts.IDWTBeta
                            * (tileComp.Buffer[i - 1] + tileComp.Buffer[i + 1]));

                    for (int i = 4; i <= end - 1; i += 2)
                        tileComp.Buffer[i] = (int)(tileComp.Buffer[i] - JPXConsts.IDWTAlpha
                            * (tileComp.Buffer[i - 1] + tileComp.Buffer[i + 1]));
                }
                else
                {
                    for (int i = 3; i <= end; i += 2)
                        tileComp.Buffer[i] -= (tileComp.Buffer[i - 1] + tileComp.Buffer[i + 1] + 2) >> 2;


                    for (int i = 4; i <= end; i += 2)
                        tileComp.Buffer[i] += (tileComp.Buffer[i - 1] + tileComp.Buffer[i + 1]) >> 1;
                }

                for (int i = 0; i < i1 - i0; ++i)
                    tileComp.Data[data_i + i * stride] = tileComp.Buffer[offset + i];
            }
        }

        private void extendRight(JPXTileComp tileComp, int i0, int i1, int offset, int end)
        {
            if (i1 - i0 == 2)
            {
                tileComp.Buffer[end + 1] = tileComp.Buffer[offset + 1];
                tileComp.Buffer[end + 2] = tileComp.Buffer[offset];
                tileComp.Buffer[end + 3] = tileComp.Buffer[offset + 1];
            }
            else
            {
                tileComp.Buffer[end + 1] = tileComp.Buffer[end - 3];
                if (i1 - i0 == 3)
                {
                    tileComp.Buffer[end + 2] = tileComp.Buffer[offset + 1];
                    tileComp.Buffer[end + 3] = tileComp.Buffer[offset + 2];
                }
                else
                {
                    tileComp.Buffer[end + 2] = tileComp.Buffer[end - 4];

                    tileComp.Buffer[end + 3] = i1 - i0 == 4 ? tileComp.Buffer[offset + 1] :
                        tileComp.Buffer[end + 3] = tileComp.Buffer[end - 5];
                }
            }
        }

        private void extendLeft(JPXTileComp tileComp, int offset)
        {
            tileComp.Buffer[offset - 1] = tileComp.Buffer[offset + 1];
            tileComp.Buffer[offset - 2] = tileComp.Buffer[offset + 2];
            tileComp.Buffer[offset - 3] = tileComp.Buffer[offset + 3];
            if (offset == 4)
                tileComp.Buffer[0] = tileComp.Buffer[offset + 4];
        }

        private void interLeave(JPXTileComp tileComp, JPXResLevel resLevel, int r,
            int nx0, int ny0,
            int nx1, int ny1)
        {
            int eps, shift;
            double mu;
            int qStyle = tileComp.QuantStyle & 0x1f;
            int guard = (tileComp.QuantStyle >> 5) & 7;

            JPXPrecint precint = resLevel.Precint;
            for (int sb = 0; sb < 3; ++sb)
            {
                if (qStyle == 0)
                {
                    eps = (tileComp.QuantSteps[3 * r - 2 + sb] >> 3) & 0x1f;
                    shift = guard + eps - 1;
                    mu = 0;
                }
                else
                {
                    shift = guard + tileComp.Precision;
                    if (sb == 2) ++shift;
                    int t = tileComp.QuantSteps[qStyle == 1 ? 0 : 3 * r - 2 + sb];
                    mu = (0x800 + (t & 0x7ff)) / 2048.0;
                }
                if (tileComp.Transform == 0) shift += 16;

                copySubbandCoefficient(tileComp, precint, sb, shift, qStyle, mu, nx0, ny0, nx1, ny1);
            }
        }

        private void copySubbandCoefficient(JPXTileComp tileComp, JPXPrecint precint, int sb,
            int shift, int qStyle, double mu,
            int nx0, int ny0,
            int nx1, int ny1)
        {
            int xo = (sb & 1) != 0 ? 0 : 1;
            int yo = sb > 0 ? 1 : 0;
            JPXSubband subband = precint.Subbands[sb];

            JPXCodeBlock[] cb = subband.CodeBlocks;
            int cb_i = 0;

            int cbW = (int)Math.Pow(2, tileComp.CodeBlockW);
            for (int cbY = 0; cbY < subband.NYCBs; ++cbY)
                for (int cbX = 0; cbX < subband.NXCBs; ++cbX)
                {
                    for (int y = cb[cb_i].Y0, coeff0_i = 0; y < cb[cb_i].Y1; ++y, coeff0_i += cbW)
                    {
                        int index = (2 * y + yo - ny0) * (tileComp.X1 - tileComp.X0) + (2 * cb[cb_i].X0 + xo - nx0);
                        for (int x = cb[cb_i].X0, coeff_i = coeff0_i; x < cb[cb_i].X1; ++x, ++coeff_i)
                        {
                            int val = cb[cb_i].Coeff[coeff_i].Magnitude;
                            if (val != 0)
                            {
                                int shift2 = shift - (cb[cb_i].NZeroBitPlanes + cb[cb_i].Coeff[coeff_i].Length);
                                if (shift2 > 0)
                                    val = (val << shift2) + (1 << (shift2 - 1));
                                else
                                    val >>= -shift2;
                                if (qStyle == 0)
                                {
                                    if (tileComp.Transform == 0)
                                        val &= -1 << 16;
                                }
                                else
                                    val = (int)(val * mu);
                                if ((cb[cb_i].Coeff[coeff_i].Flags & (short)CoefficientFlags.jpxCoeffSign) != 0)
                                    val = -val;
                            }
                            tileComp.Data[index] = val;
                            index += 2;
                        }
                    }
                    ++cb_i;
                }
        }

        private bool readImageHeader(JPXBox box)
        {
            int height = 0, width = 0, nComps = 0;
            int bpc1 = 0, compression = 0, unknownColorspace = 0, ipr = 0;

            JPXUtilities.GetInt32(box.Data, ref height);
            JPXUtilities.GetInt32(box.Data, ref width);
            JPXUtilities.GetInt16(box.Data, ref nComps);
            JPXUtilities.GetInt8(box.Data, ref bpc1);
            JPXUtilities.GetInt8(box.Data, ref compression);
            JPXUtilities.GetInt8(box.Data, ref unknownColorspace);
            JPXUtilities.GetInt8(box.Data, ref ipr);

            return true;
        }

        private bool readColorSpecification(JPXBox box)
        {
            int csApprox = 0, meth = 0, prec = 0;

            if (!JPXUtilities.GetInt8(box.Data, ref meth) ||
                !JPXUtilities.GetInt8(box.Data, ref prec) ||
                !JPXUtilities.GetInt8(box.Data, ref csApprox))
                return false;

            m_image.ColorSpec.Meth = meth;
            m_image.ColorSpec.Prec = (int)prec;

            switch (m_image.ColorSpec.Meth)
            {
                case 1:     // enumerated colorspace
                    if (!readEnumeratedColorspace(box))
                        return false;
                    break;
                case 2:     // restricted ICC profile
                    break;
                case 3:     // any ICC profile (JPX)
                    break;
                case 4:     // vendor color (JPX)
                    break;
            }

            return true;
        }

        private bool readEnumeratedColorspace(JPXBox box)
        {
            int csEnum = 0;
            if (!JPXUtilities.GetInt32(box.Data, ref csEnum))
                return false;

            m_image.ColorSpec.Enumerated.Type = (JPXColorSpaceType)csEnum;

            switch (m_image.ColorSpec.Enumerated.Type)
            {
                case JPXColorSpaceType.jpxCSBiLevel:
                case JPXColorSpaceType.jpxCSYCbCr1:
                case JPXColorSpaceType.jpxCSYCbCr2:
                case JPXColorSpaceType.jpxCSYCBCr3:
                case JPXColorSpaceType.jpxCSPhotoYCC:
                case JPXColorSpaceType.jpxCSCMY:
                case JPXColorSpaceType.jpxCSCMYK:
                case JPXColorSpaceType.jpxCSYCCK:
                case JPXColorSpaceType.jpxCSsRGB:
                case JPXColorSpaceType.jpxCSGrayscale:
                case JPXColorSpaceType.jpxCSBiLevel2:
                case JPXColorSpaceType.jpxCSCISesRGB:
                case JPXColorSpaceType.jpxCSROMMRGB:
                case JPXColorSpaceType.jpxCSsRGBYCbCr:
                case JPXColorSpaceType.jpxCSYPbPr1125:
                case JPXColorSpaceType.jpxCSYPbPr1250:
                    break;

                case JPXColorSpaceType.jpxCSCIELab:
                    if (box.DataLength == 7 + 7 * 4)
                    {
                        if (!JPXUtilities.GetInt32(box.Data, ref m_image.ColorSpec.Enumerated.CieLab.rl) ||
                            !JPXUtilities.GetInt32(box.Data, ref m_image.ColorSpec.Enumerated.CieLab.ol) ||
                            !JPXUtilities.GetInt32(box.Data, ref m_image.ColorSpec.Enumerated.CieLab.ra) ||
                            !JPXUtilities.GetInt32(box.Data, ref m_image.ColorSpec.Enumerated.CieLab.oa) ||
                            !JPXUtilities.GetInt32(box.Data, ref m_image.ColorSpec.Enumerated.CieLab.rb) ||
                            !JPXUtilities.GetInt32(box.Data, ref m_image.ColorSpec.Enumerated.CieLab.ob) ||
                            !JPXUtilities.GetInt32(box.Data, ref m_image.ColorSpec.Enumerated.CieLab.il))
                        {
                            return false;
                        }
                    }
                    else if (box.DataLength == 7)
                    {
                        //~ this assumes the 8-bit case
                        m_image.ColorSpec.Enumerated.CieLab.rl = 100;
                        m_image.ColorSpec.Enumerated.CieLab.ol = 0;
                        m_image.ColorSpec.Enumerated.CieLab.ra = 255;
                        m_image.ColorSpec.Enumerated.CieLab.oa = 128;
                        m_image.ColorSpec.Enumerated.CieLab.rb = 255;
                        m_image.ColorSpec.Enumerated.CieLab.ob = 96;
                        m_image.ColorSpec.Enumerated.CieLab.il = 0x00443530;
                    }
                    else
                    {
                        return false;
                    }
                    break;

                default:
                    return false;
            }

            return true;
        }

        private bool readCodeStream(Stream stream)
        {
            //bool findEOC = false;
            int segType = 0, segLen = 0;
            bool haveSIZ, haveCOD, haveQCD, haveSOT;

            //----- main header
            haveSIZ = haveCOD = haveQCD = haveSOT = false;

            do
            {
                if (!readMarkerSegment(stream, ref segType, ref segLen))
                    return false;

                switch (segType)
                {
                    case 0xff90:
                        haveSOT = true;
                        break;
                    case 0xff51:
                        haveSIZ = true;
                        break;
                    case 0xff52:
                        haveCOD = true;
                        break;
                    case 0xff5c:
                        haveQCD = true;
                        break;
                }

                if (!parseSegment(stream, segType, segLen))
                    return false;

            } while (!haveSOT);

            if (!haveCOD || !haveQCD || !haveSIZ || !haveSOT)
                return false;

            // read the tiles
            while (true)
            {
                if (!readTileParts(stream))
                    return false;
                if (!readMarkerSegment(stream, ref segType, ref segLen))
                    return false;
                // start of tile
                if (segType != 0xff90)
                    break;
            }

            if (segType != 0xffd9) // EOC
                return false;

            return true;
        }

        private bool readTileParts(Stream stream)
        {
            int tileId = 0, tilePartId = 0, nTilePart = 0;
            bool haveSOD = false;
            int segType = 0, segLen = 0;
            int tilePartLen = 0;

            if (!JPXUtilities.GetInt16(stream, ref tileId) ||
                !JPXUtilities.GetInt32(stream, ref tilePartLen) ||
                !JPXUtilities.GetInt8(stream, ref tilePartId) ||
                !JPXUtilities.GetInt8(stream, ref nTilePart))
                return false;

            if (tileId > m_image.NXTiles * m_image.NYTiles)
                return false;

            tilePartLen -= 12;
            do
            {
                if (!readMarkerSegment(stream, ref segType, ref segLen))
                    return false;

                tilePartLen -= segLen + 2;

                // start of data
                if (segType == 0xff93)
                    haveSOD = true;

                if (!parseSegment(stream, segType, segLen))
                    return false;

            } while (!haveSOD);

            // initialize the tile, precincts, and code-blocks
            if (tilePartId == 0)
                init(stream, tileId);

            return readTilePartData(stream, tileId, tilePartLen);
        }

        private bool readTilePartData(Stream stream, int tileId, int tilePartLen)
        {
            JPXTile tile = m_image.TileCollection[tileId];
            // read all packets from this tile - part
            while (true)
            {
                if (tilePartLen <= 0)
                    break;

                JPXTileComp tileComp = tile.TileCompCollection[tile.Component];
                JPXResLevel resLevel = tileComp.ResLevelCollection[tile.Resolution];
                JPXPrecint precint = resLevel.Precint;

                JPXBitBuffer.StartBitBuffer(tilePartLen);

                if (!readCodeBlocks(stream, tile, precint))
                    return false;

                tilePartLen = JPXBitBuffer.FinishBitBuffer(stream);

                readPacketData(stream, tileComp, resLevel, precint, tile.Resolution, ref tilePartLen);

                readNextPacket(tile);
            }
            return true;
        }

        private void readNextPacket(JPXTile tile)
        {
            switch (tile.ProgressionOrder)
            {
                case 0:// layer, resolution level, component, precinct
                    if (++tile.Component == m_image.NComps)
                    {
                        tile.Component = 0;
                        if (++tile.Resolution == tile.MaxNDecompositionLevel + 1)
                        {
                            tile.Resolution = 0;
                            if (++tile.Layer == tile.NLayers)
                                tile.Layer = 0;
                        }
                    }
                    break;
                case 1://resolution level, layer, component, precinct
                    if (++tile.Component == m_image.NComps)
                    {
                        tile.Component = 0;

                        if (++tile.Layer == tile.NLayers)
                        {
                            tile.Layer = 0;
                            if (++tile.Resolution == tile.MaxNDecompositionLevel + 1)
                                tile.Resolution = 0;
                        }
                    }
                    break;
            }
        }

        private bool readPacketData(Stream stream, JPXTileComp tileComp, JPXResLevel resLevel,
            JPXPrecint precint, int res, ref int tilePartLen)
        {
            for (int sb = 0; sb < (res == 0 ? 1 : 3); ++sb)
            {
                JPXSubband subband = precint.Subbands[sb];
                for (int cY = 0; cY < subband.NYCBs; ++cY)
                    for (int cX = 0; cX < subband.NXCBs; ++cX)
                    {
                        if (subband.CodeBlocks[cY * subband.NXCBs + cX].Included > 0)
                        {
                            readCodeBlockData(stream, subband.CodeBlocks[cY * subband.NXCBs + cX], tileComp, sb, res);

                            tilePartLen -= subband.CodeBlocks[cY * subband.NXCBs + cX].DataLength;
                            subband.CodeBlocks[cY * subband.NXCBs + cX].Seen = true;
                        }
                    }
            }

            return true;
        }

        private void readCodeBlockData(Stream stream, JPXCodeBlock cb, JPXTileComp tileComp,
            int sb, int res)
        {
            initArithDecoder(stream, cb);

            for (int i = 0; i < cb.NCodingPass; ++i)
                readPassMode(stream, cb, cb.NextPass, tileComp, sb, res);

            cb.ArithmeticDecoder.Cleanup();
        }

        private void initArithDecoder(Stream stream, JPXCodeBlock cb)
        {
            if (cb.ArithmeticDecoder != null)
                cb.ArithmeticDecoder.Restart((int)cb.DataLength);
            else
            {
                cb.ArithmeticDecoder = new JArithmeticDecoder();
                cb.ArithmeticDecoder.SetStream(stream, (int)cb.DataLength);
                cb.ArithmeticDecoder.Start();
                cb.ArithmeticDecoderStats = new JArithmeticDecoderStats((int)ContextSize.jpxNContexts);
                cb.ArithmeticDecoderStats.SetEntry((int)ContextSize.jpxContextSigProp, 4, 0);
                cb.ArithmeticDecoderStats.SetEntry((int)ContextSize.jpxContextRunLength, 3, 0);
                cb.ArithmeticDecoderStats.SetEntry((int)ContextSize.jpxContextUniform, 46, 0);
            }
        }

        private bool readPassMode(Stream stream, JPXCodeBlock cb, PassMode mode, JPXTileComp tileComp,
            int sb, int res)
        {
            switch (mode)
            {
                case PassMode.jpxPassSigProp:
                    readSignificancePropagationPass(cb, tileComp, sb, res);
                    ++cb.NextPass;
                    break;
                case PassMode.jpxPassMagRef:
                    readMagnitudeRefinementPass(cb, tileComp, sb, res);
                    ++cb.NextPass;
                    break;
                case PassMode.jpxPassCleanup:
                    readCleanupPass(cb, tileComp, sb, res);
                    cb.NextPass = PassMode.jpxPassSigProp;
                    break;
            }
            return true;
        }

        private void readSignificancePropagationPass(JPXCodeBlock cb, JPXTileComp tileComp,
            int sb, int res)
        {
            JPXCoeff[] coeff = cb.Coeff;
            int coeff0_i, coeff1_i, coeff_i;
            int y0, y1, x, horiz, vert, diag, cx;
            int horizSign, vertSign;
            int cbW = (int)Math.Pow(2, tileComp.CodeBlockW);

            for (y0 = cb.Y0, coeff0_i = 0; y0 < cb.Y1; y0 += 4, coeff0_i += 4 << (int)tileComp.CodeBlockW)
                for (x = cb.X0, coeff1_i = coeff0_i; x < cb.X1; ++x, ++coeff1_i)
                    for (y1 = 0, coeff_i = coeff1_i; y1 < 4 && y0 + y1 < cb.Y1; ++y1, coeff_i += cbW)
                        if ((coeff[coeff_i].Flags & (short)CoefficientFlags.jpxCoeffSignificant) == 0)
                        {
                            horiz = vert = diag = 0;
                            horizSign = vertSign = 2;

                            if (x > cb.X0)
                            {
                                if ((coeff[coeff_i - 1].Flags & (short)CoefficientFlags.jpxCoeffSignificant) != 0)
                                {
                                    ++horiz;
                                    horizSign +=
                                        (coeff[coeff_i - 1].Flags & (short)CoefficientFlags.jpxCoeffSign) != 0 ? -1 : 1;
                                }
                                if (y0 + y1 > cb.Y0)
                                    diag += (int)(coeff[coeff_i - cbW - 1].Flags >>
                                        (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                if (y0 + y1 < cb.Y1 - 1)
                                    diag += (int)(coeff[coeff_i + cbW - 1].Flags >>
                                        (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                            }

                            if (x < cb.X1 - 1)
                            {
                                if ((coeff[coeff_i + 1].Flags & (short)CoefficientFlags.jpxCoeffSignificant) != 0)
                                {
                                    ++horiz;
                                    horizSign +=
                                        (coeff[coeff_i + 1].Flags & (short)CoefficientFlags.jpxCoeffSign) != 0 ? -1 : 1;
                                }
                                if (y0 + y1 > cb.Y0)
                                    diag += (int)(coeff[coeff_i - cbW + 1].Flags >>
                                        (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                if (y0 + y1 < cb.Y1 - 1)
                                    diag += (int)(coeff[coeff_i + cbW + 1].Flags >>
                                        (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                            }

                            if (y0 + y1 > cb.Y0)
                                if ((coeff[coeff_i - cbW].Flags & (short)CoefficientFlags.jpxCoeffSignificant) != 0)
                                {
                                    ++vert;
                                    vertSign +=
                                        (coeff[coeff_i - cbW].Flags & (short)CoefficientFlags.jpxCoeffSign) != 0 ? -1 : 1;
                                }

                            if (y0 + y1 < cb.Y1 - 1)
                                if ((coeff[coeff_i + cbW].Flags & (short)CoefficientFlags.jpxCoeffSignificant) != 0)
                                {
                                    ++vert;
                                    vertSign +=
                                        (coeff[coeff_i + cbW].Flags & (short)CoefficientFlags.jpxCoeffSign) != 0 ? -1 : 1;
                                }

                            cx = JPXConsts.SigPropContext[horiz, vert, diag, res == 0 ? 1 : sb];
                            if (cx != 0)
                            {
                                if (cb.ArithmeticDecoder.DecodeBit((uint)cx, cb.ArithmeticDecoderStats) != 0)
                                {
                                    coeff[coeff_i].Flags |= (short)CoefficientFlags.jpxCoeffSignificant
                                        | (short)CoefficientFlags.jpxCoeffFirstMagRef;
                                    coeff[coeff_i].Magnitude = (coeff[coeff_i].Magnitude << 1) | 1;
                                    cx = JPXConsts.SignContext[horizSign, vertSign, 0];
                                    int xorBit = JPXConsts.SignContext[horizSign, vertSign, 1];
                                    if ((cb.ArithmeticDecoder.DecodeBit((uint)cx, cb.ArithmeticDecoderStats) ^ xorBit) != 0)
                                        coeff[coeff_i].Flags |= (short)CoefficientFlags.jpxCoeffSign;
                                }
                                ++coeff[coeff_i].Length;
                                coeff[coeff_i].Flags |= (short)CoefficientFlags.jpxCoeffTouched;
                            }
                        }
        }

        private void readMagnitudeRefinementPass(JPXCodeBlock cb, JPXTileComp tileComp, int sb, int res)
        {
            JPXCoeff[] coeff = cb.Coeff;
            int coeff0_i, coeff1_i, coeff_i;
            int y0, y1, x, cx, all;
            int cbW = (int)Math.Pow(2, tileComp.CodeBlockW);

            for (y0 = cb.Y0, coeff0_i = 0; y0 < cb.Y1; y0 += 4, coeff0_i += 4 << (int)tileComp.CodeBlockW)
                for (x = cb.X0, coeff1_i = coeff0_i; x < cb.X1; ++x, ++coeff1_i)
                    for (y1 = 0, coeff_i = coeff1_i; y1 < 4 && y0 + y1 < cb.Y1; ++y1, coeff_i += cbW)
                        if ((coeff[coeff_i].Flags & (short)CoefficientFlags.jpxCoeffSignificant) != 0
                            && (coeff[coeff_i].Flags & (short)CoefficientFlags.jpxCoeffTouched) == 0)
                        {
                            if ((coeff[coeff_i].Flags & (short)CoefficientFlags.jpxCoeffFirstMagRef) != 0)
                            {
                                all = 0;

                                if (x > cb.X0)
                                {
                                    all += (int)(coeff[coeff_i - 1].Flags >> (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                    if (y0 + y1 > cb.Y0)
                                        all += (int)(coeff[coeff_i - cbW - 1].Flags >>
                                            (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                    if (y0 + y1 < cb.Y1 - 1)
                                        all += (int)(coeff[coeff_i + cbW - 1].Flags >>
                                            (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                }
                                if (x < cb.X1 - 1)
                                {
                                    all += (int)(coeff[coeff_i + 1].Flags >> (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                    if (y0 + y1 > cb.Y0)
                                        all += (int)(coeff[coeff_i - cbW + 1].Flags >>
                                            (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                    if (y0 + y1 < cb.Y1 - 1)
                                        all += (int)(coeff[coeff_i + cbW + 1].Flags >>
                                            (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                }
                                if (y0 + y1 > cb.Y0)
                                    all += (int)(coeff[coeff_i - cbW].Flags >>
                                        (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                if (y0 + y1 < cb.Y1 - 1)
                                    all += (int)(coeff[coeff_i + cbW].Flags >>
                                        (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                cx = (int)(all != 0 ? 15 : 14);
                            }
                            else
                                cx = 16;

                            coeff[coeff_i].Magnitude = ((int)(coeff[coeff_i].Magnitude << 1) |
                                cb.ArithmeticDecoder.DecodeBit((uint)cx, cb.ArithmeticDecoderStats));
                            ++coeff[coeff_i].Length;
                            coeff[coeff_i].Flags |= (short)CoefficientFlags.jpxCoeffTouched;
                            coeff[coeff_i].Flags &= ~(short)CoefficientFlags.jpxCoeffFirstMagRef;
                        }
        }

        private void readCleanupPass(JPXCodeBlock cb, JPXTileComp tileComp, int sb, int res)
        {
            JPXCoeff[] coeff = cb.Coeff;
            int coeff0_i, coeff1_i, coeff_i;
            int y0, y1, y2, x, horiz, vert, diag, cx, xorBit;
            int horizSign, vertSign;
            int cbW = (int)Math.Pow(2, tileComp.CodeBlockW);

            for (y0 = cb.Y0, coeff0_i = 0; y0 < cb.Y1; y0 += 4, coeff0_i += 4 << (int)tileComp.CodeBlockW)
                for (x = cb.X0, coeff1_i = coeff0_i; x < cb.X1; ++x, ++coeff1_i)
                {
                    y1 = 0;
                    if (y0 + 3 < cb.Y1 &&
                      (coeff[coeff1_i].Flags & (short)CoefficientFlags.jpxCoeffTouched) == 0 &&
                      (coeff[coeff1_i + cbW].Flags & (short)CoefficientFlags.jpxCoeffTouched) == 0 &&
                      (coeff[coeff1_i + 2 * cbW].Flags & (short)CoefficientFlags.jpxCoeffTouched) == 0 &&
                      (coeff[coeff1_i + 3 * cbW].Flags & (short)CoefficientFlags.jpxCoeffTouched) == 0 &&
                      (x == cb.X0 || y0 == cb.Y0 ||
                       (coeff[coeff1_i - (int)cbW - 1].Flags
                     & (short)CoefficientFlags.jpxCoeffSignificant) == 0) &&
                      (y0 == cb.Y0 ||
                       (coeff[coeff1_i - (int)cbW].Flags
                     & (short)CoefficientFlags.jpxCoeffSignificant) == 0) &&
                      (x == cb.X1 - 1 || y0 == cb.Y0 ||
                       (coeff[coeff1_i - (int)cbW + 1].Flags
                     & (short)CoefficientFlags.jpxCoeffSignificant) == 0) &&
                      (x == cb.X0 ||
                       ((coeff[coeff1_i - 1].Flags & (short)CoefficientFlags.jpxCoeffSignificant) == 0 &&
                    (coeff[coeff1_i + cbW - 1].Flags
                      & (short)CoefficientFlags.jpxCoeffSignificant) == 0 &&
                    (coeff[coeff1_i + 2 * cbW - 1].Flags
                      & (short)CoefficientFlags.jpxCoeffSignificant) == 0 &&
                    (coeff[coeff1_i + 3 * cbW - 1].Flags
                      & (short)CoefficientFlags.jpxCoeffSignificant) == 0)) &&
                      (x == cb.X1 - 1 ||
                       ((coeff[coeff1_i + 1].Flags & (short)CoefficientFlags.jpxCoeffSignificant) == 0 &&
                    (coeff[coeff1_i + cbW + 1].Flags
                      & (short)CoefficientFlags.jpxCoeffSignificant) == 0 &&
                    (coeff[coeff1_i + 2 * cbW + 1].Flags
                      & (short)CoefficientFlags.jpxCoeffSignificant) == 0 &&
                    (coeff[coeff1_i + 3 * cbW + 1].Flags
                      & (short)CoefficientFlags.jpxCoeffSignificant) == 0)) &&
                      (x == cb.X0 || y0 + 4 == cb.Y1 ||
                       (coeff[coeff1_i + 4 * cbW - 1].Flags & (short)CoefficientFlags.jpxCoeffSignificant) == 0) &&
                      (y0 + 4 == cb.Y1 ||
                       (coeff[coeff1_i + 4 * cbW].Flags & (short)CoefficientFlags.jpxCoeffSignificant) == 0) &&
                      (x == cb.X1 - 1 || y0 + 4 == cb.Y1 ||
                       (coeff[coeff1_i + 4 * cbW + 1].Flags
                     & (short)CoefficientFlags.jpxCoeffSignificant) == 0))
                    {
                        if (cb.ArithmeticDecoder.DecodeBit(17, cb.ArithmeticDecoderStats) != 0)
                        {
                            y1 = (int)cb.ArithmeticDecoder.DecodeBit(18, cb.ArithmeticDecoderStats);
                            y1 = (y1 << 1) | (int)cb.ArithmeticDecoder.DecodeBit(18, cb.ArithmeticDecoderStats);

                            for (y2 = 0, coeff_i = coeff1_i; y2 < y1; ++y2, coeff_i += cbW)
                                ++coeff[coeff_i].Length;

                            coeff[coeff_i].Flags |= (short)CoefficientFlags.jpxCoeffSignificant |
                                (short)CoefficientFlags.jpxCoeffFirstMagRef;
                            coeff[coeff_i].Magnitude = (coeff[coeff_i].Magnitude << 1) | 1;
                            ++coeff[coeff_i].Length;
                            cx = JPXConsts.SignContext[2, 2, 0];
                            xorBit = JPXConsts.SignContext[2, 2, 1];

                            if ((cb.ArithmeticDecoder.DecodeBit((uint)cx, cb.ArithmeticDecoderStats) ^ xorBit) != 0)
                                coeff[coeff_i].Flags |= (short)CoefficientFlags.jpxCoeffSign;
                            ++y1;
                        }
                        else
                        {
                            for (y1 = 0, coeff_i = coeff1_i; y1 < 4; ++y1, coeff_i += cbW)
                                ++coeff[coeff_i].Length;
                            y1 = 4;
                        }
                    }

                    for (coeff_i = (int)(coeff1_i + (y1 << (int)tileComp.CodeBlockW)); y1 < 4 && y0 + y1 < cb.Y1;
                        ++y1, coeff_i += cbW)
                    {
                        if ((coeff[coeff_i].Flags & (short)CoefficientFlags.jpxCoeffTouched) == 0)
                        {
                            horiz = vert = diag = 0;
                            horizSign = vertSign = 2;

                            if (x > cb.X0)
                            {
                                if ((coeff[coeff_i - 1].Flags & (uint)CoefficientFlags.jpxCoeffSignificant) != 0)
                                {
                                    ++horiz;
                                    horizSign +=
                                            (coeff[coeff_i - 1].Flags & (uint)CoefficientFlags.jpxCoeffSign) != 0 ? -1 : 1;
                                }
                                if (y0 + y1 > cb.Y0)
                                    diag += (int)(coeff[coeff_i - cbW - 1].Flags >>
                                        (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                if (y0 + y1 < cb.Y1 - 1)
                                    diag += (int)(coeff[coeff_i + cbW - 1].Flags >>
                                        (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                            }
                            if (x < cb.X1 - 1)
                            {
                                if ((coeff[coeff_i + 1].Flags & (uint)CoefficientFlags.jpxCoeffSignificant) != 0)
                                {
                                    ++horiz;
                                    horizSign +=
                                        (coeff[coeff_i + 1].Flags & (uint)CoefficientFlags.jpxCoeffSign) != 0 ? -1 : 1;
                                }
                                if (y0 + y1 > cb.Y0)
                                    diag += (int)(coeff[coeff_i - cbW + 1].Flags >>
                                        (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                                if (y0 + y1 < cb.Y1 - 1)
                                    diag += (int)(coeff[coeff_i + cbW + 1].Flags >>
                                        (int)CoefficientFlags.jpxCoeffSignificantB) & 1;
                            }
                            if (y0 + y1 > cb.Y0)
                                if ((coeff[coeff_i - cbW].Flags & (uint)CoefficientFlags.jpxCoeffSignificant) != 0)
                                {
                                    ++vert;
                                    vertSign +=
                                        (coeff[coeff_i - cbW].Flags & (uint)CoefficientFlags.jpxCoeffSign) != 0 ? -1 : 1;
                                }

                            if (y0 + y1 < cb.Y1 - 1)
                                if ((coeff[coeff_i + cbW].Flags & (uint)CoefficientFlags.jpxCoeffSignificant) != 0)
                                {
                                    ++vert;
                                    vertSign +=
                                        (coeff[coeff_i + cbW].Flags & (uint)CoefficientFlags.jpxCoeffSign) != 0 ? -1 : 1;
                                }

                            cx = JPXConsts.SigPropContext[horiz, vert, diag, res == 0 ? 1 : sb];
                            if (cb.ArithmeticDecoder.DecodeBit((uint)cx, cb.ArithmeticDecoderStats) != 0)
                            {
                                coeff[coeff_i].Flags |= (short)CoefficientFlags.jpxCoeffSignificant
                                    | (short)CoefficientFlags.jpxCoeffFirstMagRef;
                                coeff[coeff_i].Magnitude = (coeff[coeff_i].Magnitude << 1) | 1;
                                cx = JPXConsts.SignContext[horizSign, vertSign, 0];
                                xorBit = JPXConsts.SignContext[horizSign, vertSign, 1];
                                if ((cb.ArithmeticDecoder.DecodeBit((uint)cx, cb.ArithmeticDecoderStats) ^ xorBit) != 0)
                                    coeff[coeff_i].Flags |= (short)CoefficientFlags.jpxCoeffSign;
                            }
                            ++coeff[coeff_i].Length;
                        }
                        else
                        {
                            coeff[coeff_i].Flags &= ~(short)CoefficientFlags.jpxCoeffTouched;
                        }
                    }
                }
        }

        private bool readCodeBlocks(Stream stream, JPXTile tile, JPXPrecint precint)
        {
            int bits = 0;
            if (!JPXBitBuffer.GetBits(stream, 1, ref bits))
                return false;

            if (bits == 0)
                clearAllCodeBlock(tile, precint);
            else
                if (!fillCodeBlocks(stream, tile, precint))
                    return false;

            return true;
        }

        private void clearAllCodeBlock(JPXTile tile, JPXPrecint precint)
        {
            for (int sb = 0; sb < (tile.Resolution == 0 ? 1 : 3); ++sb)
            {
                JPXSubband subband = precint.Subbands[sb];
                for (int cY = 0; cY < subband.NYCBs; ++cY)
                    for (int cX = 0; cX < subband.NXCBs; ++cX)
                        subband.CodeBlocks[cY * subband.NXCBs + cX].Included = 0;
            }
        }

        private bool fillCodeBlocks(Stream stream, JPXTile tile, JPXPrecint precint)
        {
            for (int sb = 0; sb < (tile.Resolution == 0 ? 1 : 3); ++sb)
            {
                JPXSubband subband = precint.Subbands[sb];
                for (int cY = 0; cY < subband.NYCBs; ++cY)
                    for (int cX = 0; cX < subband.NXCBs; ++cX)
                    {
                        JPXCodeBlock cb = subband.CodeBlocks[cY * subband.NXCBs + cX];
                        // skip code block with no coefficient
                        if (cb.X0 >= cb.X1 || cb.Y0 >= cb.Y1)
                        {
                            cb.Included = 0;
                            continue;
                        }

                        if (!codeBlockInclusion(stream, tile, subband, cb, cX, cY))
                            return false;

                        if (cb.Included > 0)
                            if (!fillDataCodeBlock(stream, tile, subband, cb, cX, cY))
                                return false;
                    }
            }
            return true;
        }

        private bool codeBlockInclusion(Stream stream, JPXTile tile, JPXSubband subband, JPXCodeBlock cb,
            int cbX, int cbY)
        {
            int bits = 0;
            if (cb.Seen)
            {
                if (!JPXBitBuffer.GetBits(stream, 1, ref bits))
                    return false;
                cb.Included = bits;
            }
            else
            {
                int ttVal = 0;
                int i = 0, j = 0, level = 0;
                for (level = (int)subband.MaxTTLevel; level >= 0; --level)
                {
                    int nx = (int)JPXUtilities.CeilDivPow2(subband.NXCBs, level);
                    int ny = (int)JPXUtilities.CeilDivPow2(subband.NYCBs, level);
                    j = i + (cbY >> level) * nx + (cbX >> level);

                    if (!subband.Inclusion[j].IsFinished && subband.Inclusion[j].Value == 0)
                        subband.Inclusion[j].Value = ttVal;
                    else
                        ttVal = subband.Inclusion[j].Value;

                    while (!subband.Inclusion[j].IsFinished && ttVal <= tile.Layer)
                    {
                        if (!JPXBitBuffer.GetBits(stream, 1, ref bits))
                            return false;
                        if (bits == 1)
                            subband.Inclusion[j].IsFinished = true;
                        else
                            ++ttVal;
                    }
                    subband.Inclusion[j].Value = ttVal;
                    if (ttVal > tile.Layer)
                        break;
                    i += nx * ny;
                }
                cb.Included = (level < 0 ? 1 : 0);
            }

            return true;
        }

        private bool fillDataCodeBlock(Stream stream, JPXTile tile, JPXSubband subband, JPXCodeBlock cb,
            int cbX, int cbY)
        {
            int bits = 0;
            if (!cb.Seen)
            {
                int ttVal = 0;
                int i = 0, j = 0, level = 0;
                for (level = (int)subband.MaxTTLevel; level >= 0; --level)
                {
                    int nx = (int)JPXUtilities.CeilDivPow2(subband.NXCBs, level);
                    int ny = (int)JPXUtilities.CeilDivPow2(subband.NYCBs, level);
                    j = i + (cbY >> level) * nx + (cbX >> level);

                    if (!subband.ZeroBitPlane[j].IsFinished && subband.ZeroBitPlane[j].Value == 0)
                        subband.ZeroBitPlane[j].Value = ttVal;
                    else
                        ttVal = subband.ZeroBitPlane[j].Value;

                    while (!subband.ZeroBitPlane[j].IsFinished)
                    {
                        if (!JPXBitBuffer.GetBits(stream, 1, ref bits))
                            return false;
                        if (bits == 1)
                            subband.ZeroBitPlane[j].IsFinished = true;
                        else
                            ++ttVal;
                    }
                    subband.ZeroBitPlane[j].Value = ttVal;
                    i += nx * ny;
                }
                cb.NZeroBitPlanes = ttVal;
            }

            int pass = 0;
            if (!readNumberOfCodingPass(stream, ref pass))
                return false;
            cb.NCodingPass = pass;

            while (true)
            {
                if (!JPXBitBuffer.GetBits(stream, 1, ref bits))
                    return false;
                if (bits == 0) break;
                ++cb.LengthBlock;
            }
            int n, k;
            for (n = cb.LengthBlock, k = cb.NCodingPass >> 1; k != 0; ++n, k >>= 1) ;

            if (!JPXBitBuffer.GetBits(stream, (int)n, ref bits))
                return false;
            
            cb.DataLength = bits;

            return true;
        }

        private bool readNumberOfCodingPass(Stream stream, ref int pass)
        {
            int bits = 0;
            if (!JPXBitBuffer.GetBits(stream, 1, ref bits))
                return false;
            if (bits == 0)
                pass = 1;
            else
            {
                if (!JPXBitBuffer.GetBits(stream, 1, ref bits))
                    return false;
                if (bits == 0)
                    pass = 2;
                else
                {
                    if (!JPXBitBuffer.GetBits(stream, 2, ref bits))
                        return false;
                    if (bits < 3)
                        pass = 3 + bits;
                    else
                    {
                        if (!JPXBitBuffer.GetBits(stream, 5, ref bits))
                            return false;
                        if (bits < 31)
                            pass = 6 + bits;
                        else
                        {
                            if (!JPXBitBuffer.GetBits(stream, 7, ref bits))
                                return false;
                            pass = 37 + bits;
                        }
                    }
                }
            }
            return true;
        }

        private bool init(Stream stream, int tileId)
        {
            JPXTile tile = m_image.TileCollection[tileId];
            int i = (int)(tileId / m_image.NXTiles);
            int j = (int)(tileId % m_image.NYTiles);

            tile.X0 = (int)Math.Max(m_image.XTileOffset + j * m_image.XTileSize, m_image.XOffset);
            tile.Y0 = (int)Math.Max(m_image.YTileOffset + i * m_image.YTileSize, m_image.YOffset);
            tile.X1 = (int)Math.Min(m_image.XTileOffset + (j + 1) * m_image.XTileSize, m_image.XSize);
            tile.Y1 = (int)Math.Min(m_image.YTileOffset + (i + 1) * m_image.YTileSize, m_image.YSize);

            tile.MaxNDecompositionLevel = 0;

            for (uint comp = 0; comp < m_image.NComps; ++comp)
            {
                initTileComp(tile, comp);
            }

            return true;
        }

        private void initTileComp(JPXTile tile, uint comp)
        {
            JPXTileComp tileComp = tile.TileCompCollection[comp];

            tile.MaxNDecompositionLevel = Math.Max(tile.MaxNDecompositionLevel, tileComp.NDecompositionLevel);
            tileComp.X0 = JPXUtilities.CeilDiv(tile.X0, tileComp.HSeparation);
            tileComp.X1 = JPXUtilities.CeilDiv(tile.X1, tileComp.HSeparation);
            tileComp.Y0 = JPXUtilities.CeilDiv(tile.Y0, tileComp.VSeparation);
            tileComp.Y1 = JPXUtilities.CeilDiv(tile.Y1, tileComp.VSeparation);
            tileComp.Data = new int[(tileComp.X1 - tileComp.X0) * (tileComp.Y1 - tileComp.Y0)];
            tileComp.Buffer = new int[Math.Max(tileComp.X1 - tileComp.X0, tileComp.Y1 - tileComp.Y0) + 8];

            for (uint i = 0; i <= tileComp.NDecompositionLevel; ++i)
                initResolutionLevel(tileComp, i);
        }

        private void initResolutionLevel(JPXTileComp tileComp, uint n)
        {
            JPXResLevel resLevel = tileComp.ResLevelCollection[n];
            int k = (int)(n == 0 ? tileComp.NDecompositionLevel : tileComp.NDecompositionLevel - n + 1);
            resLevel.X0 = JPXUtilities.CeilDivPow2(tileComp.X0, k);
            resLevel.X1 = JPXUtilities.CeilDivPow2(tileComp.X1, k);
            resLevel.Y0 = JPXUtilities.CeilDivPow2(tileComp.Y0, k);
            resLevel.Y1 = JPXUtilities.CeilDivPow2(tileComp.Y1, k);

            if (n == 0)
            {
                resLevel.BX0[0] = resLevel.X0;
                resLevel.BX1[0] = resLevel.X1;
                resLevel.BY0[0] = resLevel.Y0;
                resLevel.BY1[0] = resLevel.Y1;
            }
            else
            {
                resLevel.BX0[0] = resLevel.BX0[2] =
                    JPXUtilities.CeilDivPow2((tileComp.X0 - (int)Math.Pow(2, k - 1)), k);
                resLevel.BX1[0] = resLevel.BX1[2] =
                    JPXUtilities.CeilDivPow2((tileComp.X1 - (int)Math.Pow(2, k - 1)), k);
                resLevel.BY0[0] = resLevel.Y0;
                resLevel.BY1[0] = resLevel.Y1;
                resLevel.BX0[1] = resLevel.X0;
                resLevel.BX1[1] = resLevel.X1;
                resLevel.BY0[1] = resLevel.BY0[2] =
                    JPXUtilities.CeilDivPow2((tileComp.Y0 - (int)Math.Pow(2, k - 1)), k);
                resLevel.BY1[1] = resLevel.BY1[2] =
                    JPXUtilities.CeilDivPow2((tileComp.Y1 - (int)Math.Pow(2, k - 1)), k);
            }

            resLevel.Precint = new JPXPrecint();
            resLevel.Precint.X0 = resLevel.X0;
            resLevel.Precint.X1 = resLevel.X1;
            resLevel.Precint.Y0 = resLevel.Y0;
            resLevel.Precint.Y1 = resLevel.Y1;
            initSubbands(tileComp, resLevel, n);

            // tileComp.ResLevelCollection[n] = resLevel;
        }

        private void initSubbands(JPXTileComp tileComp, JPXResLevel resLevel, uint n)
        {
            int nSBs = n == 0 ? 1 : 3;
            resLevel.Precint.Subbands = new JPXSubband[nSBs];
            for (int i = 0; i < nSBs; ++i)
            {
                JPXSubband subband = resLevel.Precint.Subbands[i] = new JPXSubband();
                subband.X0 = resLevel.BX0[i];
                subband.X1 = resLevel.BX1[i];
                subband.Y0 = resLevel.BY0[i];
                subband.Y1 = resLevel.BY1[i];
                subband.NXCBs = JPXUtilities.CeilDivPow2(subband.X1, tileComp.CodeBlockW)
                    - JPXUtilities.FloorDivPow2(subband.X0, tileComp.CodeBlockW);
                subband.NYCBs = JPXUtilities.CeilDivPow2(subband.Y1, tileComp.CodeBlockH)
                    - JPXUtilities.FloorDivPow2(subband.Y0, tileComp.CodeBlockH);

                int temp = Math.Max(subband.NXCBs, subband.NYCBs);
                for (subband.MaxTTLevel = 0, --temp; temp > 0; ++subband.MaxTTLevel, temp >>= 1) ;

                initTagTreeNode(subband);
                initCodeBlock(tileComp, subband);
            }
        }

        private void initTagTreeNode(JPXSubband subband)
        {
            int n = 0;
            for (int level = (int)subband.MaxTTLevel; level >= 0; --level)
                n += JPXUtilities.CeilDivPow2(subband.NXCBs, level) * JPXUtilities.CeilDivPow2(subband.NYCBs, level);

            subband.Inclusion = new JPXTagTreeNode[n];
            subband.ZeroBitPlane = new JPXTagTreeNode[n];

            for (int i = 0; i < n; ++i)
            {
                subband.Inclusion[i].IsFinished = subband.ZeroBitPlane[i].IsFinished
                    = false;
                subband.Inclusion[i].Value = subband.ZeroBitPlane[i].Value
                    = 0;
            }
        }

        private void initCodeBlock(JPXTileComp tileComp, JPXSubband subband)
        {
            subband.CodeBlocks = new JPXCodeBlock[subband.NXCBs * subband.NYCBs];
            for (int m = 0; m < subband.NXCBs * subband.NYCBs; ++m)
                subband.CodeBlocks[m] = new JPXCodeBlock();

            int sbx0 = JPXUtilities.FloorDivPow2(subband.X0, tileComp.CodeBlockW);
            int sby0 = JPXUtilities.FloorDivPow2(subband.Y0, tileComp.CodeBlockH);
            JPXCodeBlock[] cb = subband.CodeBlocks;

            uint cbW = (uint)Math.Pow(2, tileComp.CodeBlockW);
            uint cbH = (uint)Math.Pow(2, tileComp.CodeBlockH);
            int i = 0;
            for (int cbY = 0; cbY < subband.NYCBs; ++cbY)
                for (int cbX = 0; cbX < subband.NXCBs; ++cbX)
                {
                    cb[i].X0 =
                        Math.Max(subband.X0, (int)((sbx0 + cbX) << tileComp.CodeBlockW));
                    cb[i].X1 = (int)Math.Min(subband.X1, cb[i].X0 + cbW);
                    cb[i].Y0 =
                        Math.Max(subband.Y0, (int)((sby0 + cbY) << tileComp.CodeBlockH));
                    cb[i].Y1 = (int)Math.Min(subband.Y1, cb[i].Y0 + cbH);

                    cb[i].Seen = false;
                    cb[i].LengthBlock = 3;
                    cb[i].NextPass = PassMode.jpxPassCleanup;
                    cb[i].NZeroBitPlanes = 0;

                    initCoeff(cb[i], cbW * cbH);

                    ++i;
                }
        }

        private void initCoeff(JPXCodeBlock cb, uint length)
        {
            cb.Coeff = new JPXCoeff[length];

            for (int cbi = 0; cbi < length; ++cbi)
                cb.Coeff[cbi].Magnitude =
                    cb.Coeff[cbi].Flags =
                    cb.Coeff[cbi].Length = 0;
        }

        private bool parseSegment(Stream stream, int segType, int segLen)
        {
            switch (segType)
            {
                case 0xff4f: // start of codestream
                    break;
                case 0xff90: // start of tile-part
                    break;
                case 0xff93: // start of data
                    break;
                case 0xffd9: // end of codestream
                    break;
                case 0xff51: // image and tile size
                    if (!readSIZ(stream))
                        return false;
                    break;
                case 0xff52: // coding style of default
                    if (!readCOD(stream))
                        return false;
                    break;
                case 0xff53: // coding style component
                    if (!readCOC(stream))
                        return false;
                    break;
                case 0xff5e: // region-of-interest
                    break;
                case 0xff5c: // quantization default
                    if (!readQCD(stream, segLen))
                        return false;
                    break;
                case 0xff5d: // quantization component
                    if (!readQCC(stream, segLen))
                        return false;
                    break;
                case 0xff5f: // progression order default
                    break;
                case 0xff55: // tile-part lengths, main header
                    break;
                case 0xff57: // packet length, main header
                    break;
                case 0xff58: // packet length, tile-part header
                    break;
                case 0xff60: //packed packet header, main header
                    break;
                case 0xff61: //packed packet header, tile-part header
                    break;
                case 0xff91: // start of packet
                    break;
                case 0xff92: // end of packet header
                    break;
                case 0xff64: // comment and extension
                    readCME(stream, segLen);
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool readSIZ(Stream stream)
        {
            int capabilities = 0, Xsiz = 0, Ysiz = 0,
                XOsiz = 0, YOsiz = 0, XTsiz = 0, YTsiz = 0,
                XTOsiz = 0, YTOsiz = 0, nComps = 0;

            if (!JPXUtilities.GetInt16(stream, ref capabilities) ||
            !JPXUtilities.GetInt32(stream, ref Xsiz) ||
            !JPXUtilities.GetInt32(stream, ref Ysiz) ||
            !JPXUtilities.GetInt32(stream, ref XOsiz) ||
            !JPXUtilities.GetInt32(stream, ref YOsiz) ||
            !JPXUtilities.GetInt32(stream, ref XTsiz) ||
            !JPXUtilities.GetInt32(stream, ref YTsiz) ||
            !JPXUtilities.GetInt32(stream, ref XTOsiz) ||
            !JPXUtilities.GetInt32(stream, ref YTOsiz) ||
            !JPXUtilities.GetInt16(stream, ref nComps)
            )
                return false;
            m_image.SetImageAndTileSize(Xsiz, Ysiz, XOsiz, YOsiz, XTsiz, YTsiz,
                XTOsiz, YTOsiz, nComps);

            return createTiles(stream); ;
        }

        private bool createTiles(Stream stream)
        {
            int Ssiz = 0, XRsiz = 0, YRsiz = 0;

            m_image.CreateTileCollection(m_image.NXTiles * m_image.NYTiles);

            for (int comp = 0; comp < m_image.NComps; ++comp)
            {
                if (!JPXUtilities.GetInt8(stream, ref Ssiz) ||
                    !JPXUtilities.GetInt8(stream, ref XRsiz) ||
                    !JPXUtilities.GetInt8(stream, ref YRsiz))
                    return false;
                m_image.TileCollection[0].TileCompCollection[comp].Precision = Ssiz;
                m_image.TileCollection[0].TileCompCollection[comp].HSeparation = XRsiz;
                m_image.TileCollection[0].TileCompCollection[comp].VSeparation = YRsiz;

                m_image.TileCollection[0].TileCompCollection[comp].Signed =
                    (m_image.TileCollection[0].TileCompCollection[comp].Precision & 0x80) != 0;
                m_image.TileCollection[0].TileCompCollection[comp].Precision =
                    (m_image.TileCollection[0].TileCompCollection[comp].Precision & 0x7f) + 1;

                for (int i = 1; i < m_image.NXTiles * m_image.NYTiles; ++i)
                    m_image.TileCollection[i].TileCompCollection[comp] =
                        (JPXTileComp)m_image.TileCollection[0].TileCompCollection[comp].Clone();
            }
            return true;
        }

        private void readCME(Stream stream, int segLen)
        {
            for (int i = 0; i < segLen - 2; ++i)
                stream.ReadByte();
        }

        private bool readCOD(Stream stream)
        {
            int style = 0, progOrder = 0, nLayers = 0, multiComp = 0,
                nDecomp = 0, codeBlockW = 0, codeBlockH = 0,
                codeBlockStyle = 0, transform = 0;

            if (
                !JPXUtilities.GetInt8(stream, ref style) ||
                !JPXUtilities.GetInt8(stream, ref progOrder) ||
                !JPXUtilities.GetInt16(stream, ref nLayers) ||
                !JPXUtilities.GetInt8(stream, ref multiComp) ||
                !JPXUtilities.GetInt8(stream, ref nDecomp) ||
                !JPXUtilities.GetInt8(stream, ref codeBlockW) ||
                !JPXUtilities.GetInt8(stream, ref codeBlockH) ||
                !JPXUtilities.GetInt8(stream, ref codeBlockStyle) ||
                !JPXUtilities.GetInt8(stream, ref transform))
                return false;

            codeBlockW += 2;
            codeBlockH += 2;

            JPXResLevel[] resLevel = createResLevel(stream, style, nDecomp);

            for (int i = 0; i < m_image.NXTiles * m_image.NYTiles; ++i)
            {
                m_image.TileCollection[i].ProgressionOrder = progOrder;
                m_image.TileCollection[i].NLayers = nLayers;
                m_image.TileCollection[i].MultiComponent = multiComp;

                for (int comp = 0; comp < m_image.NComps; ++comp)
                {
                    m_image.TileCollection[i].TileCompCollection[comp].Style = style;
                    m_image.TileCollection[i].TileCompCollection[comp].NDecompositionLevel = nDecomp;
                    m_image.TileCollection[i].TileCompCollection[comp].CodeBlockW = codeBlockW;
                    m_image.TileCollection[i].TileCompCollection[comp].CodeBlockH = codeBlockH;
                    m_image.TileCollection[i].TileCompCollection[comp].CodeBlockStyle = codeBlockStyle;
                    m_image.TileCollection[i].TileCompCollection[comp].Transform = transform;

                    m_image.TileCollection[i].TileCompCollection[comp].ResLevelCollection = new JPXResLevel[nDecomp + 1];
                    for (int r = 0; r < nDecomp + 1; ++r)
                        m_image.TileCollection[i].TileCompCollection[comp].ResLevelCollection[r] = (JPXResLevel)resLevel[r].Clone();
                }
            }

            return true;
        }

        private JPXResLevel[] createResLevel(Stream stream, int style, int nDecomp)
        {
            JPXResLevel[] resLevel = new JPXResLevel[nDecomp + 1];
            for (int i = 0; i < resLevel.Length; ++i)
                resLevel[i] = new JPXResLevel();

            for (int i = 0; i < nDecomp; ++i)
            {
                if ((style & 0x0001) != 0)
                {
                    int precintSize = 0;
                    JPXUtilities.GetInt8(stream, ref precintSize);

                    resLevel[i].PrecintW = precintSize & 0x000f;
                    resLevel[i].PrecintH = (precintSize >> 4) & 0x000f;
                }
                else
                    resLevel[i].PrecintW = resLevel[i].PrecintH = 15;
            }

            return resLevel;
        }

        private bool readCOC(Stream stream)
        {
            int comp = 0;
            int style = 0, nDecomp = 0, codeBlockW = 0,
                codeBlockH = 0, codeBlockStyle = 0, transform = 0;

            if (m_image.NComps > 256)
            {
                if (!JPXUtilities.GetInt16(stream, ref comp))
                    return false;
            }
            else
            {
                if (!JPXUtilities.GetInt8(stream, ref comp))
                    return false;
            }

            if (
                !JPXUtilities.GetInt8(stream, ref style) ||
                !JPXUtilities.GetInt8(stream, ref nDecomp) ||
                !JPXUtilities.GetInt8(stream, ref codeBlockW) ||
                !JPXUtilities.GetInt8(stream, ref codeBlockH) ||
                !JPXUtilities.GetInt8(stream, ref codeBlockStyle) ||
                !JPXUtilities.GetInt8(stream, ref transform))
                return false;

            codeBlockW += 2;
            codeBlockH += 2;

            style = (int)(m_image.TileCollection[0].TileCompCollection[comp].Style & ~1) | (style & 1);

            JPXResLevel[] resLevel = createResLevel(stream, style, nDecomp);

            for (int i = 0; i < m_image.NXTiles * m_image.NYTiles; ++i)
            {
                m_image.TileCollection[i].TileCompCollection[comp].Style = style;
                m_image.TileCollection[i].TileCompCollection[comp].NDecompositionLevel = nDecomp;
                m_image.TileCollection[i].TileCompCollection[comp].CodeBlockW = codeBlockW;
                m_image.TileCollection[i].TileCompCollection[comp].CodeBlockH = codeBlockH;
                m_image.TileCollection[i].TileCompCollection[comp].CodeBlockStyle = codeBlockStyle;
                m_image.TileCollection[i].TileCompCollection[comp].Transform = transform;

                m_image.TileCollection[i].TileCompCollection[comp].ResLevelCollection = new JPXResLevel[nDecomp + 1];
                for (int r = 0; r < nDecomp + 1; ++r)
                    m_image.TileCollection[i].TileCompCollection[comp].ResLevelCollection[r] = (JPXResLevel)resLevel[r].Clone();//resLevel[r];
            }

            return true;
        }

        private bool readQCD(Stream stream, int segLen)
        {
            int quantStyle = 0;
            int[] quantSteps;
            JPXUtilities.GetInt8(stream, ref quantStyle);

            switch (quantStyle & 0x001f)
            {
                case 0x0000:
                    {
                        quantSteps = new int[segLen - 3];
                        for (int i = 0; i < quantSteps.Length; ++i)
                            JPXUtilities.GetInt8(stream, ref quantSteps[i]);

                        break;
                    }
                case 0x0001:
                    {
                        quantSteps = new int[1];
                        JPXUtilities.GetInt16(stream, ref quantSteps[0]);
                        break;
                    }
                case 0x0002:
                    {
                        quantSteps = new int[(segLen - 3) / 2];
                        for (int i = 0; i < quantSteps.Length; ++i)
                            JPXUtilities.GetInt16(stream, ref quantSteps[i]);
                        break;
                    }
                default:
                    return false;
            }

            for (int i = 0; i < m_image.NXTiles * m_image.NYTiles; ++i)
                for (int comp = 0; comp < m_image.NComps; ++comp)
                {
                    m_image.TileCollection[i].TileCompCollection[comp].QuantSteps = quantSteps;
                    m_image.TileCollection[i].TileCompCollection[comp].QuantStyle = quantStyle;
                }

            return true;
        }

        private bool readQCC(Stream stream, int segLen)
        {
            int comp = 0, quantStyle = 0;
            int[] quantSteps;

            if (m_image.NComps > 256)
            {
                if (!JPXUtilities.GetInt16(stream, ref comp))
                    return false;
            }
            else
            {
                if (!JPXUtilities.GetInt8(stream, ref comp))
                    return false;
            }

            if (comp > m_image.NComps)
                return false;

            JPXUtilities.GetInt8(stream, ref quantStyle);

            switch (quantStyle & 0x001f)
            {
                case 0x0000:
                    {
                        quantSteps = new int[segLen - (m_image.NComps > 256 ? 5 : 4)];
                        for (int i = 0; i < quantSteps.Length; ++i)
                            JPXUtilities.GetInt8(stream, ref quantSteps[i]);

                        break;
                    }
                case 0x0001:
                    {
                        quantSteps = new int[1];
                        JPXUtilities.GetInt16(stream, ref quantSteps[0]);
                        break;
                    }
                case 0x0002:
                    {
                        quantSteps = new int[(segLen - (m_image.NComps > 256 ? 5 : 4)) / 2];
                        for (int i = 0; i < quantSteps.Length; ++i)
                            JPXUtilities.GetInt16(stream, ref quantSteps[i]);
                        break;
                    }
                default:
                    return false;
            }

            for (int i = 0; i < m_image.NXTiles * m_image.NYTiles; ++i)
            {
                m_image.TileCollection[i].TileCompCollection[comp].QuantSteps = quantSteps;
                m_image.TileCollection[i].TileCompCollection[comp].QuantStyle = quantStyle;
            }

            return true;
        }

        private bool readMarkerSegment(Stream stream, ref int segType, ref int segLen)
        {
            JPXUtilities.GetInt16(stream, ref segType);

            if ((segType >= 0xff30 && segType <= 0xff3f) ||
                segType == 0xff4f || segType == 0xff92 || segType == 0xff93 || segType == 0xffd9)
            {
                segLen = 0;
                return true;
            }

            return JPXUtilities.GetInt16(stream, ref segLen);
        }

        private JPXImage m_image;
        private MemoryStream m_stream;
    }
}
