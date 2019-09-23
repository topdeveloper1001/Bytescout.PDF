using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Bytescout.PDF
{
    internal class BMPDictionary : PDFDictionaryStream
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

	    public BMPDictionary(FileStream fs)
	    {
		    Dictionary.AddItem("Type", new PDFName("XObject"));
		    Dictionary.AddItem("Subtype", new PDFName("Image"));
		    Bitmap bmp = new Bitmap(fs);
		    initializeBMP(bmp);
	    }

	    private void initializeBMP(Bitmap bmp)
        {
            _width = bmp.Width;
            _height = bmp.Height;
            Dictionary.AddItem("Width", new PDFNumber(_width));
            Dictionary.AddItem("Height", new PDFNumber(_height));
            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                    initializeIndexedBMP(bmp);
                    break;
                case PixelFormat.Format24bppRgb:
                    initialize24BMP(bmp);
                    break;
                case PixelFormat.Format32bppRgb:
                    initialize32BMP(bmp);
                    break;
                case PixelFormat.Format64bppArgb:
                    initialize64BMP(bmp);
                    break;
                default:
                    throw new PDFUnsupportImageFormat();
            }
        }

        private void initializeIndexedBMP(Bitmap bmp)
        {
            int index = 32;
            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    index /= 1;
                    Dictionary.AddItem("BitsPerComponent", new PDFNumber(1));
                    break;
                case PixelFormat.Format4bppIndexed:
                    index /= 4;
                    Dictionary.AddItem("BitsPerComponent", new PDFNumber(4));
                    break;
                case PixelFormat.Format8bppIndexed:
                    index /= 8;
                    Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
                    break;
            }
            System.Drawing.Color[] colors = bmp.Palette.Entries;
            byte[] buf = new byte[colors.Length * 3];
            for (int i = 0; i < colors.Length; ++i)
            {
                buf[i * 3] = colors[i].R;
                buf[i * 3 + 1] = colors[i].G;
                buf[i * 3 + 2] = colors[i].B;
            }
            PDFArray array = new PDFArray();
            array.AddItem(new PDFName("Indexed"));
            array.AddItem(new PDFName("DeviceRGB"));
            array.AddItem(new PDFNumber(colors.Length - 1));
            array.AddItem(new PDFString(buf, true));
            Dictionary.AddItem("ColorSpace", array);
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, _width, _height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            buf = new byte[bmpData.Stride * bmpData.Height];
            System.Runtime.InteropServices.Marshal.Copy(ptr, buf, 0, buf.Length);
            int countByte = (_width - _width % index) / index + 1;
            if (_width % index == 0)
                --countByte;
            int width = _width - (countByte - 1) * index;
            int countByteW = (width - width % (index / 4)) / (index / 4) + 1;
            if (width % (index / 4) == 0)
                --countByteW;
            for (int i = 0; i < _height; ++i)
            {
                GetStream().Write(buf, i * bmpData.Stride, (countByte - 1) * 4 + countByteW);
            }

            bmp.UnlockBits(bmpData);
        }

        private void initialize24BMP(Bitmap bmp)
        {
            Dictionary.AddItem("ColorSpace", new PDFName("DeviceRGB"));
            Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, _width, _height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            byte[] buf = new byte[bmpData.Stride * bmpData.Height];
            System.Runtime.InteropServices.Marshal.Copy(ptr, buf, 0, buf.Length);
            byte xor;
            int k = bmpData.Stride - _width * 3;
            int nLine = 1;
            for (int i = 0; i < buf.Length - k; i += 3)
            {
                xor = buf[i];
                buf[i] = buf[i + 2];
                buf[i + 2] = xor;
                if ((i - k * (nLine - 1)) / 3 + 1 >= _width * nLine)
                {
                    i += k;
                    nLine++;
                }
            }

            for (int i = 0; i < _height; ++i)
            {
                GetStream().Write(buf, i * bmpData.Stride, _width * 3);
            }
            bmp.UnlockBits(bmpData);
        }

        private void initialize32BMP(Bitmap bmp)
        {
            MemoryStream streamSMask = new MemoryStream();
            PDFDictionaryStream dictSMask = new PDFDictionaryStream(new PDFDictionary(), streamSMask);
            dictSMask.Dictionary.AddItem("Type", new PDFName("XObject"));
            dictSMask.Dictionary.AddItem("Subtype", new PDFName("Image"));
            dictSMask.Dictionary.AddItem("Width", new PDFNumber(_width));
            dictSMask.Dictionary.AddItem("Height", new PDFNumber(_height));
            dictSMask.Dictionary.AddItem("ColorSpace", new PDFName("DeviceGray"));
            dictSMask.Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
            Dictionary.AddItem("SMask", dictSMask);
            Dictionary.AddItem("ColorSpace", new PDFName("DeviceRGB"));
            Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, _width, _height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            IntPtr ptr = bmpData.Scan0;
            byte[] buf = new byte[bmpData.Stride * bmpData.Height];
            System.Runtime.InteropServices.Marshal.Copy(ptr, buf, 0, buf.Length);
            byte xor;
            for (int i = 0; i < buf.Length; i += 4)
            {
                xor = buf[i];
                buf[i] = buf[i + 2];
                buf[i + 2] = xor;
                GetStream().Write(buf, i, 3);
                streamSMask.WriteByte(buf[i + 3]);
            }

            bmp.UnlockBits(bmpData);
        }

        private void initialize64BMP(Bitmap bmp)
        {
            throw new PDFUnsupportImageFormat();
        }
    }
}
