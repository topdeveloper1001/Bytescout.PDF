using System;
using System.IO;

namespace Bytescout.PDF
{
    internal class PNGDictionary : PDFDictionaryStream
    {
	    private readonly byte[] _adam7_rows = { 1, 5, 1, 3, 1, 2, 1 };
	    private readonly byte[] _adam7_rowsN = { 8, 8, 4, 4, 2, 2, 1 };
	    private readonly byte[] _adam7_lines = { 1, 1, 5, 1, 3, 1, 2 };
	    private readonly byte[] _adam7_linesN = { 8, 8, 8, 4, 4, 2, 2 };

	    private int _width;
	    private int _height;
	    private byte _bitdepth;
	    private byte _colortype;
	    private byte _compression;
	    private byte _filter;
	    private byte _interface;

	    public int Width
        {
            get
            {
                return _width;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
        }

	    public PNGDictionary(FileStream fs)
	    {
		    Dictionary.AddItem("Type", new PDFName("XObject"));
		    Dictionary.AddItem("Subtype", new PDFName("Image"));
		    initPNG(fs);
	    }

	    private void initPNG(FileStream fs)
        {
            byte[] buf = new byte[4];
            fs.Position = 12;
            fs.Read(buf, 0, buf.Length);
            if (Encoding.GetString(buf) == "IHDR")
            {
                fs.Read(buf, 0, buf.Length);
                _width = toInt32(buf);
                fs.Read(buf, 0, buf.Length);
                _height = toInt32(buf);
                _bitdepth = (byte)fs.ReadByte();
                _colortype = (byte)fs.ReadByte();
                _compression = (byte)fs.ReadByte();
                _filter = (byte)fs.ReadByte();
                _interface = (byte)fs.ReadByte();
                fs.Position += 4;
                Dictionary.AddItem("Width", new PDFNumber(_width));
                Dictionary.AddItem("Height", new PDFNumber(_height));
                Dictionary.AddItem("BitsPerComponent", new PDFNumber(_bitdepth));
                long positionTRNS = initSettings (fs);
                switch (_colortype)
                {
                    case 6:
                        initialize6IDAT(fs);
                        break;
                    case 4:
                        initialize4IDAT(fs);
                        break;
                    case 3:
                        initialize3IDAT(fs, positionTRNS);
                        break;
                    case 2:
                        initialize2IDAT(fs, positionTRNS);
                        break;
                    case 0:
                        initialize0IDAT(fs, positionTRNS);
                        break;
                    default:
                        throw new PDFUnsupportImageFormat();
                }
            }
            else
                throw new PDFUnsupportImageFormat();
        }

        private long initSettings (FileStream fs)
        {
            long positionTRNS = 0;
            byte [] buf = new byte [4];
            int size = 0;
            string chunkname = "";
            while (chunkname != "IDAT" && chunkname != "IEND")
            {
                fs.Read(buf, 0, buf.Length);
                size = toInt32(buf);
                fs.Read(buf, 0, buf.Length);
                chunkname = Encoding.GetString(buf);
                byte[] data = null;
                switch (chunkname)
                {
                    case "PLTE":
                        data = new byte[size];
                        fs.Read(data, 0, size);
                        IPDFObject colorspace = null;
                        if (Dictionary["ColorSpace"] != null)
                        {
                            colorspace = Dictionary["ColorSpace"];
                        }
                        else
                            colorspace = new PDFName("DeviceRGB");
                        PDFArray array = new PDFArray();
                        array.AddItem(new PDFName("Indexed"));
                        array.AddItem(colorspace);
                        array.AddItem(new PDFNumber(size / 3 - 1));
                        array.AddItem(new PDFString(data, true));
                        Dictionary.AddItem("ColorSpace", array);
                        fs.Position += 4;
                        break;
                    case "gAMA":
                        data = new byte[size];
                        fs.Read(data, 0, size);
                        PDFArray arrayCal = new PDFArray();
                        if (_colortype == 0 || _colortype == 4)
                            initCalGray(arrayCal, toInt32(data));
                        else
                            initCalRGB(arrayCal, toInt32(data));
                        Dictionary.AddItem("ColorSpace", arrayCal);
                        fs.Position += 4;
                        break;
                    case "tRNS":
                        positionTRNS = fs.Position - 8;
                        fs.Position += 4 + size;
                        break;
                    case "IDAT":
                    case "IEND":
                        fs.Position -= 8;
                        break;
                    default:
                        fs.Position += 4 + size;
                        break;
                }
            }
            return positionTRNS;
        }

        private void initCalRGB(PDFArray array, int gamma)
        {
            PDFDictionary dict = new PDFDictionary();
            array.AddItem(new PDFName("CalRGB"));
            PDFArray arrayWhite = new PDFArray();            
            PDFArray arrayMatrix = new PDFArray();
            arrayWhite.AddItem(new PDFNumber(0.95044f));
            arrayWhite.AddItem(new PDFNumber(1));
            arrayWhite.AddItem(new PDFNumber(1.08893f));
            arrayMatrix.AddItem(new PDFNumber(0.4124f));
            arrayMatrix.AddItem(new PDFNumber(0.21264f));
            arrayMatrix.AddItem(new PDFNumber(0.01934f));
            arrayMatrix.AddItem(new PDFNumber(0.35759f));
            arrayMatrix.AddItem(new PDFNumber(0.71517f));
            arrayMatrix.AddItem(new PDFNumber(0.1192f));
            arrayMatrix.AddItem(new PDFNumber(0.18046f));
            arrayMatrix.AddItem(new PDFNumber(0.07219f));
            arrayMatrix.AddItem(new PDFNumber(0.9504f));
            dict.AddItem("WhitePoint", arrayWhite);
            dict.AddItem("Matrix", arrayMatrix);
            PDFArray gammaA = new PDFArray();
            gammaA.AddItem(new PDFNumber((float)100000 / gamma));
            gammaA.AddItem(new PDFNumber((float)100000 / gamma));
            gammaA.AddItem(new PDFNumber((float)100000 / gamma));
            dict.AddItem("Gamma", gammaA);
            array.AddItem(dict);
        }

        private void initCalGray(PDFArray array, int gamma)
        {
            PDFDictionary dict = new PDFDictionary();
            array.AddItem(new PDFName("CalGray"));
            PDFArray arrayWhite = new PDFArray();            
            arrayWhite.AddItem(new PDFNumber(0.95044f));
            arrayWhite.AddItem(new PDFNumber(1));
            arrayWhite.AddItem(new PDFNumber(1.08893f));
            dict.AddItem("WhitePoint", arrayWhite);
            dict.AddItem("Gamma", new PDFNumber((float)100000 / gamma));
            array.AddItem(dict);
        }

        private void initialize0IDAT(FileStream fs, long positionTRNS)
        {
            if (positionTRNS != 0)
            {
                long oldPosition = fs.Position;
                fs.Position = positionTRNS;
                byte[] buf = new byte[4];
                fs.Read(buf, 0, buf.Length);
                fs.Position += 4;
                byte[] data = new byte[toInt32(buf)];
                fs.Read(data, 0, data.Length);
                PDFArray array = new PDFArray();
                for (int i = 0; i < 1; ++i)
                {
                    int tRNS = data[i * 2 + 1];
                    if (_bitdepth == 16)
                        tRNS = (((int)data[i * 2] << 8) | data[i * 2 + 1]);
                    array.AddItem(new PDFNumber(tRNS));
                    array.AddItem(new PDFNumber(tRNS));
                }
                Dictionary.AddItem("Mask", array);
                fs.Position = oldPosition;
            }
            initNotAlphaIDAT(fs, "DeviceGray", 1);
        }

        private void initialize2IDAT(FileStream fs, long positionTRNS)
        {
            if (positionTRNS != 0)
            {
                long oldPosition = fs.Position;
                fs.Position = positionTRNS;
                byte[] buf = new byte[4];
                fs.Read(buf, 0, buf.Length);
                fs.Position += 4;
                byte[] data = new byte[toInt32(buf)];
                fs.Read(data, 0, data.Length);
                PDFArray array = new PDFArray();
                for (int i = 0; i < 3; ++i)
                {
                    int tRNS = data[i * 2 + 1];
                    if (_bitdepth == 16)
                        tRNS = (((int)data[i*2] << 8) | data[i * 2 + 1]);
                    array.AddItem(new PDFNumber(tRNS));
                    array.AddItem(new PDFNumber(tRNS));
                }
                Dictionary.AddItem("Mask", array);
                fs.Position = oldPosition;
            }
            initNotAlphaIDAT(fs, "DeviceRGB", 3);            
        }

        private void initialize3IDAT(FileStream fs, long positionTRNS)
        {
            if (positionTRNS != 0)
            {
                long oldPosition = fs.Position;
                fs.Position = positionTRNS;
                byte[] buf = new byte[4];
                fs.Read(buf, 0, buf.Length);
                fs.Position += 4;
                byte[] data = new byte[toInt32(buf)];
                fs.Read(data, 0, data.Length);
                fs.Position = oldPosition;
                byte max = 0;
                for (int i = 0; i < data.Length; ++i)
                    if (max < data[i])
                        max = data[i];
                PDFArray array = new PDFArray();
                array.AddItem(new PDFNumber(max));
                array.AddItem(new PDFNumber(max));
                Dictionary.AddItem("Mask", array);
            }
            if (_interface == 1)
            {
                MemoryStream compressionStream = new MemoryStream();
                compressionStream.Position = 0;
                writeIDAT(compressionStream, fs);
                MemoryStream decodeStream = FlateDecoder.Decode(compressionStream, null);
                byte[] pdfData = toPDFData(decodeStream, 1);
                GetStream().Write(pdfData, 0, pdfData.Length);
            }
            else
            {
                writeIDAT(GetStream(), fs);
                PDFDictionary dict = new PDFDictionary();
                dict.AddItem("Predictor", new PDFNumber(15));
                dict.AddItem("Colors", new PDFNumber(1));
                dict.AddItem("BitsPerComponent", new PDFNumber(_bitdepth));
                dict.AddItem("Columns", new PDFNumber(_width));
                Dictionary.AddItem("Filter", new PDFName("FlateDecode"));
                Dictionary.AddItem("DecodeParms", dict);
            }
        }

        private void initialize4IDAT(FileStream fs)
        {
            initAlphaIDAT(fs, "DeviceGray", 1);
        }

        private void initialize6IDAT(FileStream fs)
        {
            initAlphaIDAT(fs, "DeviceRGB", 3);
        }

        private void initNotAlphaIDAT(FileStream fs, string colorspace, int n)
        {
            if (Dictionary["ColorSpace"] == null)
                Dictionary.AddItem("ColorSpace", new PDFName(colorspace));
            if (_interface == 1)
            {
                MemoryStream compressionStream = new MemoryStream();
                compressionStream.Position = 0;
                writeIDAT(compressionStream, fs);
                MemoryStream decodeStream = FlateDecoder.Decode(compressionStream, null);
                if (_bitdepth == 16)
                    n *= 2;
                byte[] pdfData = toPDFData(decodeStream, n);
                GetStream().Write(pdfData, 0, pdfData.Length);
            }
            else
            {
                writeIDAT(GetStream(), fs);
                PDFDictionary dict = new PDFDictionary();
                dict.AddItem("Predictor", new PDFNumber(15));
                dict.AddItem("Colors", new PDFNumber(n));
                dict.AddItem("BitsPerComponent", new PDFNumber(_bitdepth));
                dict.AddItem("Columns", new PDFNumber(_width));
                Dictionary.AddItem("Filter", new PDFName("FlateDecode"));
                Dictionary.AddItem("DecodeParms", dict);
            }
        }

        private void writeIDAT(MemoryStream stream, FileStream fs)
        {
            byte[] buf = new byte[4];
            string chunkname = "";
            int size = 0;
            while (chunkname != "IEND")
            {
                fs.Read(buf, 0, buf.Length);
                size = toInt32(buf);
                fs.Read(buf, 0, buf.Length);
                chunkname = Encoding.GetString(buf);
                if (chunkname == "IDAT")
                {
                    byte[] data = new byte[size];
                    fs.Read(data, 0, data.Length);
                    stream.Write(data, 0, data.Length);
                    fs.Position += 4;
                }
                else
                {
                    fs.Position += 4 + size;
                }
            }
        }

        private void initAlphaIDAT(FileStream fs, string colorspace, int n)
        {
            if (Dictionary["ColorSpace"] == null)
                Dictionary.AddItem("ColorSpace", new PDFName(colorspace));
            MemoryStream streamSMask = new MemoryStream();
            PDFDictionaryStream dictSMask = new PDFDictionaryStream(new PDFDictionary(), streamSMask);
            initSMask(dictSMask);
            Dictionary.AddItem("SMask", dictSMask);
            MemoryStream compressionStream = new MemoryStream();
            compressionStream.Position = 0;
            writeIDAT(compressionStream, fs);
            MemoryStream decodeStream = FlateDecoder.Decode(compressionStream, null);
            byte[] pdfData = toPDFData(decodeStream, (n + 1)*(_bitdepth / 8));
            for (int i = 0; i < pdfData.Length; i += (n + 1) * (_bitdepth / 8))
            {
                GetStream().Write(pdfData, i, n * (_bitdepth / 8));
                byte b = pdfData[i + n * (_bitdepth / 8)];
                if (_bitdepth == 16)
                {
                    b = (byte)((((int)b << 8) | (int)pdfData[i + n * (_bitdepth / 8) + 1]) / 256);
                }
                streamSMask.WriteByte(b);
            }
        }

        private void initSMask (PDFDictionaryStream dictSMask)
        {
            dictSMask.Dictionary.AddItem("Type", new PDFName("XObject"));
            dictSMask.Dictionary.AddItem("Subtype", new PDFName("Image"));
            dictSMask.Dictionary.AddItem("Width", new PDFNumber(_width));
            dictSMask.Dictionary.AddItem("Height", new PDFNumber(_height));
            dictSMask.Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
            dictSMask.Dictionary.AddItem("ColorSpace", new PDFName("DeviceGray"));
        }

        private byte[] toPDFData(MemoryStream stream, int numberBytes)
        {
            byte[] pdfData = new byte[lenghtPDGData()];
            stream.Position = 0;
            if (_interface == 0)
            {
                int lenght = pdfData.Length / _height;
                for (int i = 0; i < _height; ++i)
                {
                    byte filter = (byte)stream.ReadByte();
                    recon(stream, numberBytes, i + 1, lenght, filter);
                    stream.Read(pdfData, lenght * i, lenght);
                }
            }
            else
            {
                byte data = 0;
                int lenghtImage = lenghtPDGData(_width, _height) / _height;
                for (int adam7 = 0; adam7 < 7; ++adam7)
                {
                    int countLine = countLineZn(_adam7_linesN[adam7], _adam7_lines[adam7]);
                    int countRow = countRowZn(_adam7_rowsN[adam7], _adam7_rows[adam7]);
                    int lenght = 0;
                    if (countLine != 0)
                        lenght = lenghtPDGData(countLine, countRow) / countLine;
                    int mask = 0;
                    for (int i = 0; i < countLine; ++i)
                    {
                        if (countRow != 0)
                        {
                            byte filter = (byte)stream.ReadByte();
                            recon(stream, numberBytes, i + 1, lenght, filter);
                            for (int j = 0; j < countRow; ++j)
                            {
                                if (_bitdepth < 8)
                                {
                                    if ((j * _bitdepth) % 8 == 0)
                                    {
                                        mask = _bitdepth == 1 ? 1 : _bitdepth * _bitdepth - 1;
                                        for (int k = 0; k < (8 / _bitdepth) - 1; ++k)
                                            mask = mask << _bitdepth;
                                        data = (byte)stream.ReadByte();
                                    }
                                    int startcoordinate = (i * _adam7_linesN[adam7] + _adam7_lines[adam7] - 1) * lenghtImage;
                                    startcoordinate += ((j * _adam7_rowsN[adam7] + _adam7_rows[adam7] - 1) * _bitdepth) / 8;
                                    byte dataRead = (byte)(data & mask);
                                    dataRead = (byte)(dataRead << ((j * _bitdepth) % 8));
                                    mask = mask >> _bitdepth;
                                    dataRead = (byte)(dataRead >> (((j * _adam7_rowsN[adam7] + _adam7_rows[adam7] - 1) * _bitdepth) % 8));
                                    pdfData[startcoordinate] = (byte)(pdfData[startcoordinate] | dataRead);
                                }
                                else
                                {
                                    stream.Read(pdfData, (i * _adam7_linesN[adam7] + _adam7_lines[adam7] - 1) * lenghtImage + (j * _adam7_rowsN[adam7] + _adam7_rows[adam7] - 1) * numberBytes, numberBytes);
                                }
                            }
                        }
                    }
                }
            }
            return pdfData;
        }

        private int toInt32(byte [] buf)
        {
            byte xor;

            for (int i = 0; i < buf.Length / 2; ++i)
            {
                xor = buf[i];
                buf[i] = buf[buf.Length - i - 1];
                buf[buf.Length - i - 1] = xor;
            }

            return BitConverter.ToInt32(buf, 0);
        }

        private int countLineZn(int n, int line)
        {
            int count = 0;

            if (_height >= line)
            {
                count += (_height - line + 1) / n;
                if ((_height - line + 1) % n != 0)
                    ++count;
            }

            return count;
        }

        private int countRowZn(int n, int row)
        {
            int count = 0;

            if (_width >= row)
            {
                count += (_width - row + 1) / n;
                if ((_width - row + 1) % n != 0)
                    ++count;
            }

            return count;
        }

        private int lenghtPDGData()
        {
            return lenghtPDGData(_width, _height);
        }

        private int lenghtPDGData(int width, int height)
        {
            int n = 0;
            switch (_colortype)
            { 
                case 0:
                    n = 1;
                    break;
                case 2:
                    n = 3;
                    break;
                case 3:
                    n = 1;
                    break;
                case 4:
                    n = 2;
                    break;
                case 6:
                    n = 4;
                    break;
            }
            if ((width * n * _bitdepth) % 8 == 0)
                return width * n * height * _bitdepth / 8;
            return (width * n * _bitdepth / 8 + 1) * height;
        }

        private void recon(MemoryStream stream, int numberBytes, int scanline, int lenght, int filter)
        {
            if (filter == 0)
                return;
            long oldPosition = stream.Position;
            for (int i = 0; i < lenght / numberBytes; ++i)
            {
                for (int j = 0; j < numberBytes; ++j)
                {
                    byte a = 0;
                    byte b = 0;
                    byte c = 0;
                    byte x = 0;
                    if (i != 0)
                    {
                        stream.Position -= numberBytes;
                        a = (byte)stream.ReadByte();
                        stream.Position += numberBytes - 1;
                        if (scanline != 1)
                        {
                            stream.Position -= numberBytes + lenght  + 1;
                            c = (byte)stream.ReadByte();
                            stream.Position += numberBytes + lenght;
                        }
                    }
                    if (scanline != 1)
                    {
                        stream.Position -= lenght + 1;
                        b = (byte)stream.ReadByte();
                        stream.Position += lenght;
                    }
                    x = (byte)stream.ReadByte();
                    stream.Position -= 1;
                    switch (filter)
                    {
                        case 1:
                            stream.WriteByte(sub(x, a));
                            break;
                        case 2:
                            stream.WriteByte(up(x, b));
                            break;
                        case 3:
                            stream.WriteByte(average(x, a, b));
                            break;
                        case 4:
                            stream.WriteByte(paeth(x, a, b, c));
                            break;
                    }
                }
            }
            stream.Position = oldPosition;
        }

        private byte sub(byte x, byte a)
        {
            return (byte)(x + a);
        }

        private byte up(byte x, byte b)
        {
            return (byte)(x + b);
        }

        private byte average(byte x, byte a, byte b)
        {
            return (byte)(x + (a + b) / 2);
        }

        private byte paeth(byte x, byte a, byte b, byte c)
        {
            return (byte)(x + paethPredictor(a, b, c));
        }

        private byte paethPredictor(byte a, byte b, byte c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);
            if (pa <= pb && pa <= pc)
                return a;
            else if (pb <= pc)
                return b;
            return c;
        }
    }
}
