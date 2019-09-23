using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal enum ImageCompression
#else
    /// <summary>
    /// Specifies compression filter.
    /// </summary>
    public enum ImageCompression
#endif
	{
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0,
        /// <summary>
        /// An object is compressed using the "flate" method.
        /// </summary>
        Flate = 1,
        /// <summary>
        ///  An object is compressed using the JPEG baseline format.
        /// </summary>
        DCT = 2,
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal class Image
#else
	/// <summary>
    /// Represents class for an image that is suitable for use with the PDF document.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class Image
#endif
	{
	    private int _width;
	    private int _height;
	    private PDFDictionaryStream _dict;
	    private bool _isSMask;
        private ImageCompression _compression;

	    /// <summary>
	    /// Gets the width, in pixels, of this image.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float Width
	    {
		    get
		    {
			    return _width;
		    }
	    }

	    /// <summary>
	    /// Gets the height, in pixels, of this image.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float Height
	    {
		    get
		    {
			    return _height;
		    }
	    }

	    internal bool IsSMask
	    {
		    get
		    {
			    return _isSMask;
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Image class from the specified file or URL.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">Path or URL of the image file.</param>
        public Image(string fileName)
	    {
		    Bitmap bmp = null;

		    try
		    {
			    Uri uri;
			    bool isURL = Uri.TryCreate(fileName, UriKind.Absolute, out uri) &&
					(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

			    if (isURL)
			    {
				    using (System.Net.WebClient webClient = new System.Net.WebClient())
				    {
					    byte[] bytes = webClient.DownloadData(uri);
					    MemoryStream stream = new MemoryStream(bytes);
					    bmp = (Bitmap) System.Drawing.Image.FromStream(stream, false, false);
					    bmp.Tag = stream;
				    }
			    }
			    else
				    bmp = new Bitmap(fileName);

			    _isSMask = false;

			    init(bmp);
		    }
		    finally
		    {
			    if (bmp != null)
			    {
				    Stream stream = bmp.Tag as Stream;
				    bmp.Dispose();
					if (stream != null)
						stream.Dispose();
			    }
		    }
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Image class from the specified file or URL.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">Path or URL of the image file.</param>
        /// <param name="compression">The compression filter used for this image.</param>
        public Image(string fileName, ImageCompression compression)
            : this(fileName, compression, 75) { }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Image class from the specified file or URL.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">Path or URL of the image file.</param>
        /// <param name="compression">The compression filter used for this image.</param>
        /// <param name="jpegQuality">The JPEG quality.</param>
        public Image(string fileName, ImageCompression compression, int jpegQuality)
            : this(fileName)
        {
            Compression = compression;
            if (compression == ImageCompression.DCT)
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(fileName);
                DCTDecoder.Encode(image, _dict.GetStream(), jpegQuality);
                image.Dispose();
            }
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Image class from the specified data stream.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The data stream used to load the image.</param>
        public Image(Stream stream)
        {
			Bitmap bmp = null;

			try
			{
				bmp = (Bitmap) System.Drawing.Image.FromStream(stream, false, false);

				_isSMask = false;

		        init(bmp);
	        }
	        finally
			{
				if (bmp != null)
					bmp.Dispose();
			}
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Image class from the specified data stream.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The data stream used to load the image.</param>
        /// <param name="compression">The compression filter used for this image.</param>
        public Image(Stream stream, ImageCompression compression)
            : this(stream, compression, 75) { }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Image class from the specified data stream.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The data stream used to load the image.</param>
        /// <param name="compression">The compression filter used for this image.</param>
        /// <param name="jpegQuality">The JPEG quality.</param>
        public Image(Stream stream, ImageCompression compression, int jpegQuality)
            : this(stream)
        {
            Compression = compression;
            if (compression == ImageCompression.DCT)
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                DCTDecoder.Encode(image, _dict.GetStream(), jpegQuality);
                image.Dispose();
            }
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Image class from the specified existing image.
        /// </summary>
        /// <param name="image" href="http://msdn.microsoft.com/en-us/library/system.drawing.image.aspx">The System.Drawing.Image from which to create the new Bytescout.PDF.Image.</param>
        public Image(System.Drawing.Image image)
        {
			Bitmap bmp = null;

	        try
	        {
		        bmp = new Bitmap(image);

		        _isSMask = false;

		        init(bmp);
	        }
	        finally
	        {
		        if (bmp != null)
			        bmp.Dispose();
	        }
		}

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Image class from the specified existing image.
        /// </summary>
        /// <param name="image" href="http://msdn.microsoft.com/en-us/library/system.drawing.image.aspx">The System.Drawing.Image from which to create the new Bytescout.PDF.Image.</param>
        /// <param name="compression">The compression filter used for this image.</param>
        public Image(System.Drawing.Image image, ImageCompression compression)
            : this(image, compression, 75) { }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Image class from the specified existing image.
        /// </summary>
        /// <param name="image" href="http://msdn.microsoft.com/en-us/library/system.drawing.image.aspx">The System.Drawing.Image from which to create the new Bytescout.PDF.Image.</param>
        /// <param name="compression">The compression filter used for this image.</param>
        /// <param name="jpegQuality">The JPEG quality.</param>
        public Image(System.Drawing.Image image, ImageCompression compression, int jpegQuality)
            : this(image)
        {
            Compression = compression;
            if (compression == ImageCompression.DCT)
            {
                DCTDecoder.Encode(image, _dict.GetStream(), jpegQuality);
            }
        }

	    internal Image(PDFDictionaryStream stream)
        {
            if (stream == null)
                throw new PDFUnsupportImageFormat();
            _dict = stream;
            _width = (int)(stream.Dictionary["Width"] as PDFNumber).GetValue();
            _height = (int)(stream.Dictionary["Height"] as PDFNumber).GetValue();

            if (stream.Dictionary["SMask"] != null)
                _isSMask = true;
            else
                _isSMask = false;
        }

        internal void Save(string fileName, ImageFormat imageformat)
        {
            _dict.Decode();
            string nameColorSpace = getNameColorSpace();
            switch (nameColorSpace)
            {
                case "DeviceRGB":
                    saveRGB(fileName, imageformat);
                    break;
                case "DeviceCMYK":
                    saveCMYK(fileName, imageformat);
                    break;
                case "DeviceGRAY":
                    saveGRAY(fileName, imageformat);
                    break;
                case "Indexed":
                    saveIndexed(fileName, imageformat);
                    break;
                case "ICCBased":
                    saveICC(fileName, imageformat);
                    break;
                default:
                    throw new PDFUnsupportImageFormat();
            }
        }

	    internal PDFDictionaryStream GetDictionary()
        {
            return _dict;
        }

        internal ImageCompression Compression
        {
            get { return _compression; }
            set 
            {
                if (_dict == null)
                {
                    _dict = new PDFDictionaryStream(new PDFDictionary(), new MemoryStream());
                }
                _compression = value;
                if (value== ImageCompression.DCT)
                    _dict.Dictionary.AddItem("Filter", new PDFName("DCTDecode"));
            }
        }

        internal void WriteParameters(MemoryStream stream, Resources resources)
        {
            if (_dict != null)
            {                
                string name = resources.AddResources(ResourceType.XObject, _dict);            
                IPDFPageOperation operation = new DoXObject(name);
                operation.WriteBytes(stream);
            }
        }

        private void saveRGB(string fileName, ImageFormat imageformat)
        {
            Bitmap bmp = new Bitmap(_width, _height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            byte[] buf = new byte[bmpData.Stride * bmpData.Height];
            int nBitsPerDepth = (int)(_dict.Dictionary["BitsPerComponent"] as PDFNumber).GetValue();
            _dict.GetStream().Position = 0;
            BitsReader br = new BitsReader(_dict.GetStream());
            for (int i = 0; i < _height; ++i)
            {
                for (int j = 0; j < _width; ++j)
                {
                    buf[i * bmpData.Stride + j * 3 + 2] = (byte)br.ReadBits(nBitsPerDepth);
                    buf[i * bmpData.Stride + j * 3 + 1] = (byte)br.ReadBits(nBitsPerDepth);
                    buf[i * bmpData.Stride + j * 3] = (byte)br.ReadBits(nBitsPerDepth);
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(buf, 0, bmpData.Scan0, buf.Length);
            bmp.UnlockBits(bmpData);
            bmp.Save(fileName, imageformat);
			bmp.Dispose();
        }

        private void saveCMYK(string fileName, ImageFormat imageformat)
        {
            throw new PDFUnsupportImageFormat();
        }

        private void saveIndexed(string fileName, ImageFormat imageformat)
        {
            throw new PDFUnsupportImageFormat();
        }

        private void saveICC(string fileName, ImageFormat imageformat)
        {
            throw new PDFUnsupportImageFormat();
        }

        private void saveGRAY(string fileName, ImageFormat imageformat)
        {
            throw new PDFUnsupportImageFormat();
        }

        private string getNameColorSpace()
        {
            PDFName name = _dict.Dictionary["ColorSpace"] as PDFName;
            if (name != null)
                return name.GetValue();
            else
            {
                throw new PDFUnsupportImageFormat();
                /*PDFArray array = _dict.Dictionary["ColorSpace"] as PDFArray;
                name = array[0] as PDFName;
                if (name != null)
                    return name.GetValue();*/
            }
        }

        private void init(Bitmap bmp)
        {
            _width = bmp.Width;
            _height = bmp.Height;
            _dict = new PDFDictionaryStream(new PDFDictionary(), new MemoryStream());
            _dict.Dictionary.AddItem("Type", new PDFName("XObject"));
            _dict.Dictionary.AddItem("Subtype", new PDFName("Image"));
            _dict.Dictionary.AddItem("Width", new PDFNumber(bmp.Width));
            _dict.Dictionary.AddItem("Height", new PDFNumber(bmp.Height));
            switchFormat(bmp);
        }

        private void switchFormat(Bitmap bmp)
        {
            if (Array.IndexOf(bmp.PropertyIdList, 34675) != -1)
            {
                initICC(bmp);
            }
            else
            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    initFormat24bppRgb(bmp);
                    break;
                case PixelFormat.Format32bppArgb:
                    initFormat32bppArgb(bmp);
                    break;
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    initFormat32bppRgb(bmp);
                    break;
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:                    
                case PixelFormat.Indexed:
                    initIndexed(bmp);
                    break;
                case PixelFormat.Format16bppArgb1555:
                    break;
                case PixelFormat.Format16bppGrayScale:
                    break;
                case PixelFormat.Format16bppRgb555:
                    break;
                case PixelFormat.Format16bppRgb565:
                    break;
                case PixelFormat.Format48bppRgb:
                    break;
                case PixelFormat.Format64bppArgb:
                    break;
                case PixelFormat.Format64bppPArgb:
                    break;
                case PixelFormat.Gdi:
                    break;
                case PixelFormat.Undefined:
                    break;
                case PixelFormat.Max:
                    break;
                case PixelFormat.PAlpha:
                    break;
                case PixelFormat.Alpha:
                    break;
                case PixelFormat.Canonical:
                    break;
                case PixelFormat.Extended:
                    break;
                default:
                    BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                    byte[] buf = new byte[bmpData.Stride * bmpData.Height];
                    System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, buf, 0, buf.Length);
                    _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
                    _dict.Dictionary.AddItem("ColorSpace", new PDFName("DeviceCMYK"));
                    _dict.GetStream().Write(buf, 0, buf.Length);
                    bmp.UnlockBits(bmpData);
                    break;
            }
        }

        private void initFormat32bppArgb(Bitmap bmp)
        {
            int f = bmp.Flags;
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            byte[] buf = new byte[bmpData.Stride * bmpData.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, buf, 0, buf.Length);
            _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
            _dict.Dictionary.AddItem("ColorSpace", new PDFName("DeviceRGB"));
            PDFDictionaryStream dictSMask = new PDFDictionaryStream(new PDFDictionary(), new MemoryStream());
            initSMask(dictSMask);
            _dict.Dictionary.AddItem("SMask", dictSMask);
            _isSMask = true;
			MemoryStream stream = _dict.GetStream();
			stream.SetLength(buf.Length);
	        MemoryStream maskStream = dictSMask.GetStream();
			for (int i = 0; i < buf.Length; i += 4)
            {
	            stream.WriteByte(buf[i + 2]);
                stream.WriteByte(buf[i + 1]);
                stream.WriteByte(buf[i]);
                maskStream.WriteByte(buf[i + 3]);
            }
            bmp.UnlockBits(bmpData);
        }

        private void initFormat32bppRgb(Bitmap bmp)
        {
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            byte[] buf = new byte[bmpData.Stride * bmpData.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, buf, 0, buf.Length);
            _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
            _dict.Dictionary.AddItem("ColorSpace", new PDFName("DeviceRGB"));
			MemoryStream stream = _dict.GetStream();
	        stream.SetLength(buf.Length);
			for (int i = 0; i < buf.Length; i += 4)
            {
                stream.WriteByte(buf[i + 2]);
                stream.WriteByte(buf[i + 1]);
                stream.WriteByte(buf[i]);
            }
            bmp.UnlockBits(bmpData);
        }

        private void initFormat24bppRgb(Bitmap bmp)
        {
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            byte[] buf = new byte[bmpData.Stride * bmpData.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, buf, 0, buf.Length);
            _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
            _dict.Dictionary.AddItem("ColorSpace", new PDFName("DeviceRGB"));
	        MemoryStream stream = _dict.GetStream();
	        stream.SetLength(buf.Length);
			for (int i = 0; i < bmp.Height; ++i)
            {
                for (int j = 0; j < bmp.Width; ++j)
                {
                    stream.WriteByte(buf[i * bmpData.Stride + j * 3 + 2]);
                    stream.WriteByte(buf[i * bmpData.Stride + j * 3 + 1]);
                    stream.WriteByte(buf[i * bmpData.Stride + j * 3]);
                }
            }

            bmp.UnlockBits(bmpData);
        }

        private void initIndexed(Bitmap bmp)
        {
            int index = 32;
            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    index /= 1;
                    _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(1));
                    break;
                case PixelFormat.Format4bppIndexed:
                    index /= 4;
                    _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(4));
                    break;
                case PixelFormat.Indexed:
                case PixelFormat.Format8bppIndexed:
                    index /= 8;
                    _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
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
            PDFArray arrayICC = new PDFArray();
            arrayICC.AddItem(new PDFName("ICCBased"));
            PDFDictionaryStream stream = new PDFDictionaryStream();
            stream.Dictionary.AddItem("Alternate", new PDFName("DeviceRGB"));
            stream.Dictionary.AddItem("N", new PDFNumber(3));
			if (bmp.PropertyItems.Length > 0)
				stream.GetStream().Write(bmp.PropertyItems[0].Value, 0, bmp.PropertyItems[0].Value.Length);
            arrayICC.AddItem(stream);
            array.AddItem(new PDFName("DeviceRGB"));
            array.AddItem(new PDFNumber(colors.Length - 1));
            array.AddItem(new PDFString(buf, true));
            _dict.Dictionary.AddItem("ColorSpace", array);
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            buf = new byte[bmpData.Stride * bmpData.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, buf, 0, buf.Length);

            int countByte = (_width - _width % index) / index + 1;
            if (_width % index == 0)
                --countByte;
            int width = _width - (countByte - 1) * index;
            int countByteW = (width - width % (index / 4)) / (index / 4) + 1;
            if (width % (index / 4) == 0)
                --countByteW;

	        MemoryStream memoryStream = _dict.GetStream();
	        for (int i = 0; i < _height; ++i)
		        memoryStream.Write(buf, i * bmpData.Stride, (countByte - 1) * 4 + countByteW);

	        bmp.UnlockBits(bmpData);
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

        private void initICC(Bitmap bmp)
        {
            int indexProperty = Array.IndexOf(bmp.PropertyIdList, 34675);
            if (indexProperty == -1)
                throw new PDFInvalidICCException();
            ICCBased icc = new ICCBased(bmp.PropertyItems[indexProperty].Value);
            switch (bmp.PixelFormat)
            { 
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Indexed:
                    initICCIndexed(bmp, icc);
                    break;
                default:
                    initICCNotIndexed(bmp, icc);
                    break;
            }
        }

        private void initICCNotIndexed(Bitmap bmp, ICCBased icc)
        {
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            byte[] buf = new byte[bmpData.Stride * bmpData.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, buf, 0, buf.Length);
            _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
            _dict.Dictionary.AddItem("ColorSpace", icc);
	        MemoryStream stream = _dict.GetStream();
			if (icc.CountComponents == 4)
            {
                stream.Write(buf, 0, buf.Length);
            }
            else if (icc.CountComponents == 3)
            {
                for (int i = 0; i < bmp.Height; ++i)
                    for (int j = 0; j < bmp.Width; ++j)
                    {
                        stream.WriteByte(buf[i * bmpData.Stride + j * 3 + 2]);
                        stream.WriteByte(buf[i * bmpData.Stride + j * 3 + 1]);
                        stream.WriteByte(buf[i * bmpData.Stride + j * 3]);
                    }
            }
            else if (icc.CountComponents == 1)
            {
                for (int i = 0; i < bmp.Height; ++i)
                    for (int j = 0; j < bmp.Width; ++j)
                        stream.WriteByte(buf[i * bmpData.Stride + j]);                    
            }

            bmp.UnlockBits(bmpData);
        }

        private void initICCIndexed(Bitmap bmp, ICCBased icc)
        {
            int index = 32;
            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    index /= 1;
                    _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(1));
                    break;
                case PixelFormat.Format4bppIndexed:
                    index /= 4;
                    _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(4));
                    break;
                case PixelFormat.Indexed:
                case PixelFormat.Format8bppIndexed:
                    index /= 8;
                    _dict.Dictionary.AddItem("BitsPerComponent", new PDFNumber(8));
                    break;
            }
            System.Drawing.Color[] colors = bmp.Palette.Entries;
            byte[] buf;
            if (icc.CountComponents == 4)
            {
                buf = new byte[colors.Length * 4];
                for (int i = 0; i < colors.Length; ++i)
                {
                    buf[i * 3] = colors[i].A;
                    buf[i * 3 + 1] = colors[i].R;
                    buf[i * 3 + 2] = colors[i].G;
                    buf[i * 3 + 3] = colors[i].B;
                }
            }
            else
            {
                buf = new byte[colors.Length * 3];
                for (int i = 0; i < colors.Length; ++i)
                {
                    buf[i * 3] = colors[i].R;
                    buf[i * 3 + 1] = colors[i].G;
                    buf[i * 3 + 2] = colors[i].B;
                }
            }
            PDFArray array = new PDFArray();
            array.AddItem(new PDFName("Indexed"));
            array.AddItem(icc);
            array.AddItem(new PDFNumber(colors.Length - 1));
            array.AddItem(new PDFString(buf, true));
            _dict.Dictionary.AddItem("ColorSpace", array);
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            buf = new byte[bmpData.Stride * bmpData.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, buf, 0, buf.Length);

            int countByte = (_width - _width % index) / index + 1;
            if (_width % index == 0)
                --countByte;
            int width = _width - (countByte - 1) * index;
            int countByteW = (width - width % (index / 4)) / (index / 4) + 1;
            if (width % (index / 4) == 0)
                --countByteW;

	        MemoryStream stream = _dict.GetStream();
	        for (int i = 0; i < _height; ++i)
		        stream.Write(buf, i * bmpData.Stride, (countByte - 1) * 4 + countByteW);

	        bmp.UnlockBits(bmpData);
        }
    }
}