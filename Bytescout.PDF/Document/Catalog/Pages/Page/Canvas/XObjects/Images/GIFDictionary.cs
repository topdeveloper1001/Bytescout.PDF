using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Bytescout.PDF
{
    internal class BitsReader
    {
	    private int _bitPos;
	    private byte _curByte;
	    private readonly Stream _stream;

	    public Stream BaseStream
	    {
		    get
		    {
			    return _stream;
		    }
	    }

	    public BitsReader(Stream stream)
        {
            _stream = stream;
            _bitPos = 0;
        }

	    public int ReadBits(int nBits)
        {
            if (nBits == 0)
                return 0;

            if (_bitPos == 0)
                _curByte = (byte)_stream.ReadByte();

            int result = 0;
            int nCurBitPos = 0;
            while (nBits != 0)
            {
                if (nBits + _bitPos <= 8)
                {
                    result |= (((_curByte >> _bitPos) & ((1 << nBits) - 1)) << nCurBitPos);
                    _bitPos += nBits;
                    _bitPos %= 8;
                    nBits = 0;
                }
                else
                {
                    result |= ((_curByte >> _bitPos) & ((1 << (8 - _bitPos)) - 1) << nCurBitPos);
                    nBits -= 8 - _bitPos;
                    nCurBitPos += 8 - _bitPos;
                    _bitPos = 0;
                    _curByte = (byte)_stream.ReadByte();
                }
            }

            return result;
        }
    }

    internal static class LZW
    {
	    private static Array<byte>[] _dictionary;
	    private static int _dictionaryLen;
	    private const int StandartNumOfBits = 12;

	    public static int Decode(Stream input, int nBitsPerDepth, Stream output)
        {
            input.Position = 0;
            BitsReader reader = new BitsReader(input);
            int clearCode = 1 << nBitsPerDepth;
            int endOfCode = clearCode + 1;
            int nBits = nBitsPerDepth + 1;
            initDict(nBitsPerDepth);
            int code = reader.ReadBits(nBits);
            if (code != (1 << nBitsPerDepth))
                return -1;
            code = reader.ReadBits(nBits);
            for (int i = 0; i < _dictionary[code].Length; ++i)
                output.WriteByte(_dictionary[code][i]);
            int old = code;
            code = reader.ReadBits(nBits);
            while (code != endOfCode)
            {
                if (_dictionaryLen == (1 << nBits) - 1)
                    nBits++;
                if (nBits > StandartNumOfBits)
                    nBits = 12;
                if (code < 0 || code > _dictionaryLen)
                    return -1;

                if (code == clearCode)
                {
                    reset(nBitsPerDepth);
                    nBits = nBitsPerDepth + 1;
                    code = reader.ReadBits(nBits);
                    for (int i = 0; i < _dictionary[code].Length; ++i)
                        output.WriteByte(_dictionary[code][i]);
                }
                else
                {
                    if (code == _dictionaryLen)
                    {
                        Array<byte> str = _dictionary[old];
                        str += str[0];
                        for (int i = 0; i < str.Length; ++i)
                            output.WriteByte(str[i]);
                        addInDict(str);
                    }
                    else
                    {
                        for (int i = 0; i < _dictionary[code].Length; ++i)
                            output.WriteByte(_dictionary[code][i]);
                        addInDict(_dictionary[old] + _dictionary[code][0]);
                    }
                }
                old = code;
                code = reader.ReadBits(nBits);
            }

            return 0;
        }

        private static void reset(int nBitsPerDepth)
        {
            _dictionaryLen = (1 << nBitsPerDepth) + 2;
        }

        private static void initDict(int nBitsPerDepth)
        {
            _dictionary = new Array<byte>[10000];
            int minSizeDict = (1 << nBitsPerDepth) + 2;
            for (int i = 0; i < minSizeDict; ++i)
                _dictionary[i] = new Array<byte>((byte)i);
            _dictionaryLen = minSizeDict;
        }

        private static void addInDict(Array<byte> ba)
        {
            _dictionary[_dictionaryLen++] = ba;
        }
    }

    internal class GIFDictionary : PDFDictionaryStream
    {
	    private int _width;
	    private int _height;

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

	    public GIFDictionary(FileStream fs) : base()
        {
            Dictionary.AddItem("Type", new PDFName("XObject"));
            Dictionary.AddItem("Subtype", new PDFName("Image"));
            initGIF(fs);
        }

	    private void initGIF(FileStream fs)
        {
            fs.Position = 6;
            byte[] buf = new byte[2];
            fs.Read(buf, 0, buf.Length);
            _width = BitConverter.ToInt16(buf, 0);
            fs.Read(buf, 0, buf.Length);
            _height = BitConverter.ToInt16(buf, 0);
            byte number = (byte)fs.ReadByte();
            int bitsPerDepth = (number & 7) + 1;
            Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
            Dictionary.AddItem("Width", new PDFNumber(_width));
            Dictionary.AddItem("Height", new PDFNumber(_height));
            fs.Position += 2;            
            buf = new byte[(1 << bitsPerDepth) * 3];
            fs.Read(buf, 0, buf.Length);
            PDFArray array = new PDFArray();
            array.AddItem(new PDFName("Indexed"));
            array.AddItem(new PDFName("DeviceRGB"));
            array.AddItem(new PDFNumber((1 << bitsPerDepth) - 1));
            array.AddItem(new PDFString(buf, true));
            Dictionary.AddItem("ColorSpace", array);
            number = (byte)fs.ReadByte();
            if (Encoding.GetString(new byte[1] {number}) == "!")
            {
                while (Encoding.GetString(new byte[1] { number }) != ",")
                    number = (byte)fs.ReadByte();
            }
            buf = new byte[2];
            fs.Read(buf, 0, buf.Length);
            int left = BitConverter.ToInt16(buf, 0);
            fs.Read(buf, 0, buf.Length);
            int top = BitConverter.ToInt16(buf, 0);
            fs.Read(buf, 0, buf.Length);
            int width = BitConverter.ToInt16(buf, 0);
            fs.Read(buf, 0, buf.Length);
            int height = BitConverter.ToInt16(buf, 0);
            number = (byte)fs.ReadByte();
            if ((number & 128) != 0)
            {
                bitsPerDepth = (number & 7) + 1;
                buf = new byte[(1 << bitsPerDepth) * 3];
                fs.Read(buf, 0, buf.Length);
                array = new PDFArray();
                array.AddItem(new PDFName("Indexed"));
                array.AddItem(new PDFName("DeviceRGB"));
                array.AddItem(new PDFNumber((1 << bitsPerDepth) - 1));
                array.AddItem(new PDFString(buf, true));
                Dictionary.AddItem("ColorSpace", array);
            }
            //int nBitPerDepth = fs.ReadByte();
            //MemoryStream stream = new MemoryStream();
            //number = (byte)fs.ReadByte();
            //while (number != 0)
            //{ 
            //    byte[] data = new byte[number];
            //    fs.Read(data, 0, data.Length);
            //    stream.Write(data, 0, data.Length);
            //    number = (byte)fs.ReadByte();
            //}
            //MemoryStream output = new MemoryStream();
            //LZW.Decode(stream, nBitPerDepth, GetStream());
            //LZW.Decode(stream, nBitPerDepth, output);
            //byte [] bufout = new byte[output.Length];
            //output.Position = 0;
            //output.Read(bufout, 0, bufout.Length);
            //fs.Position = 0;
            MemoryStream streamSMask = new MemoryStream();
            PDFDictionaryStream dictSMask = new PDFDictionaryStream(new PDFDictionary(), streamSMask);
            initSMask(dictSMask);
            Dictionary.AddItem("SMask", dictSMask);
            Dictionary.AddItem("ColorSpace", new PDFName("DeviceRGB"));
            Bitmap bmp = new Bitmap(fs);
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, _width, _height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            //byte* b = (byte*)bmpData.Scan0.ToPointer();
            //buf = new byte[bmpData.Stride * bmpData.Height];
            //System.Runtime.InteropServices.Marshal.Copy(ptr, buf, 0, buf.Length);
            for (int i = 0; i < buf.Length; i += 4)
            {
                GetStream().WriteByte(buf[i + 2]);
                GetStream().WriteByte(buf[i + 1]);
                GetStream().WriteByte(buf[i]);
                streamSMask.WriteByte(buf[i + 3]);
            }
                /*for (int i = 0; i < bmpData.Height; ++i)
                    for (int j = 0; j < bmpData.Width; ++j)
                    {
                        byte xor = buf[i * bmpData.Stride + j * 4];
                        buf[i * bmpData.Stride + j * 4] = buf[i * bmpData.Stride + j * 4 + 2];
                        buf[i * bmpData.Stride + j * 4 + 2] = xor;
                        GetStream().Write(buf, i * bmpData.Stride + j * 4, 3);
                        streamSMask.WriteByte(buf[i * bmpData.Stride + j * 4]);
                    }*/
        }

        private void initSMask(PDFDictionaryStream dictSMask)
        {
            dictSMask.Dictionary.AddItem("Type", new PDFName("XObject"));
            dictSMask.Dictionary.AddItem("Subtype", new PDFName("Image"));
            dictSMask.Dictionary.AddItem("Width", new PDFNumber(_width));
            dictSMask.Dictionary.AddItem("Height", new PDFNumber(_height));
            dictSMask.Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
            dictSMask.Dictionary.AddItem("ColorSpace", new PDFName("DeviceGray"));
        }
    }
}
