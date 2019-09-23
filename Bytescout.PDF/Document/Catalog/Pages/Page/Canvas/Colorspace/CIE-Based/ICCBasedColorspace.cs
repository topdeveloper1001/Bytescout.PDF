using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
    internal enum DeviceClassICC
    {
        icSigInputClass = 0,
        icSigDisplayClass = 1,
        icSigOutputClass = 2,
        icSigColorSpaceClass = 3
    }

    internal enum ColorSpaceICC
    {
        icSigGrayData = 0,
        icSigRgbData = 1,
        icSigCmykData = 2,
        icSigLabData = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal class ICCBasedColorspace : Colorspace
#else
	/// <summary>
    /// Represents class for an ICC color profile.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class ICCBasedColorspace : Colorspace
#endif
	{
	    private int _n;
	    private string _name;
	    private MemoryStream _stream;
	    private PDFDictionaryStream _dict;
	    private string[] _alternate;
	    private float[] _range;

	    /// <summary>
        /// Gets the number components of this color space.
        /// </summary>
        /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
        public override int N
        {
            get 
            {
                return _n;
            }
        }

        /// <summary>
        /// Gets the name of this color space.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public override string Name
        {
            get 
            {
                return "ICCBased";
            }
        }

	    /// <summary>
	    /// Creates a new ICC color profile initialized from a specified existing file.
	    /// </summary>
	    /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the file to read the color profile from.</param>
	    public ICCBasedColorspace(string fileName)
	    {
		    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		    init(fs);
		    fs.Close();
	    }

	    /// <summary>
	    /// Creates a new ICC color profile initialized from a specified stream.
	    /// </summary>
	    /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The stream to read the color profile from.</param>
	    public ICCBasedColorspace(Stream stream)
	    {
		    init(stream);
	    }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.ICCBasedColorspace.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.ICCBasedColorspace.</returns>
        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        internal override void WriteColorSpaceForStroking(MemoryStream stream, Resources resources)
        {
            PDFArray array = new PDFArray();
            array.AddItem(new PDFName(Name));
            array.AddItem(_dict);
            _name = resources.AddResources(ResourceType.ColorSpace, array);
            IPDFPageOperation operation = new ColorSpaceForStroking(_name);
            operation.WriteBytes(stream);
        }

        internal override void WriteColorSpaceForNotStroking(MemoryStream stream, Resources resources)
        {
            PDFArray array = new PDFArray();
            array.AddItem(new PDFName(Name));
            array.AddItem(_dict);
            _name = resources.AddResources(ResourceType.ColorSpace, array);
            IPDFPageOperation operation = new ColorSpaceForNonStroking(_name);
            operation.WriteBytes(stream);
        }

        internal override bool WriteChangesForStroking(Colorspace newCS, MemoryStream stream, Resources resources)
        {
            PDFArray array = new PDFArray();
            array.AddItem(new PDFName(Name));
            array.AddItem(_dict);
            _name = resources.AddResources(ResourceType.ColorSpace, array);
            if (newCS is ICCBasedColorspace)
            {
                if (!_name.Equals((newCS as ICCBasedColorspace)._name))
                { 
                    IPDFPageOperation operation = new ColorSpaceForStroking((newCS as ICCBasedColorspace)._name);
                    operation.WriteBytes(stream);
                }
            }
            else
            {
                newCS.WriteColorSpaceForStroking(stream, resources);
                return true;
            }
            return false;
        }

        internal override bool WriteChangesForNotStroking(Colorspace newCS, MemoryStream stream, Resources resources)
        {
            PDFArray array = new PDFArray();
            array.AddItem(new PDFName(Name));
            array.AddItem(_dict);
            _name = resources.AddResources(ResourceType.ColorSpace, array);
            if (newCS is ICCBasedColorspace)
            {
                if (!_name.Equals((newCS as ICCBasedColorspace)._name))
                {
                    IPDFPageOperation operation = new ColorSpaceForNonStroking((newCS as ICCBasedColorspace)._name);
                    operation.WriteBytes(stream);
                }
            }
            else
            {
                newCS.WriteColorSpaceForNotStroking(stream, resources);
                return true;
            }
            return false;
        }

        private string GetNameDeviceClass(DeviceClassICC dc)
        {
            switch (dc)
            { 
                case DeviceClassICC.icSigColorSpaceClass:
                    return "spac";
                case DeviceClassICC.icSigDisplayClass:
                    return "mntr";
                case DeviceClassICC.icSigInputClass:
                    return "scnr";
                case DeviceClassICC.icSigOutputClass:
                    return "prtr";
            }
            return "";
        }

        private string GetNameColorSpaceICC(ColorSpaceICC cs)
        {
            switch (cs)
            { 
                case ColorSpaceICC.icSigCmykData:
                    return "CMYK";
                case ColorSpaceICC.icSigGrayData:
                    return "GRAY";
                case ColorSpaceICC.icSigLabData:
                    return "Lab ";
                case ColorSpaceICC.icSigRgbData:
                    return "RGB ";
            }

            return "";
        }

        private DeviceClassICC GetDeviceClass(string name)
        {
            if (name == "mntr")
                return DeviceClassICC.icSigDisplayClass;
            else if (name == "scnr")
                return DeviceClassICC.icSigInputClass;
            else if (name == "prtr")
                return DeviceClassICC.icSigOutputClass;
            else if (name != "spac")
                throw new PDFInvalidICCException();

            return DeviceClassICC.icSigColorSpaceClass;
        }

        private ColorSpaceICC GetColorSpaceICC(string name)
        {
            if (name == "CMYK")
                return ColorSpaceICC.icSigCmykData;
            else if (name == "GRAY")
                return ColorSpaceICC.icSigGrayData;
            else if (name == "Lab ")
                return ColorSpaceICC.icSigLabData;
            else if (name != "RGB ")
                throw new PDFInvalidICCException();

            return ColorSpaceICC.icSigRgbData;
        }

        private void init(Stream stream)
        {
            _stream = new MemoryStream();
            _dict = new PDFDictionaryStream(new PDFDictionary(), _stream);

            _alternate = new string[1];
            byte[] buf = new byte[4];
            stream.Position = 12;
            stream.Read(buf, 0, 4);
            GetDeviceClass(Encoding.GetString(buf));
            stream.Position = 16;
            stream.Read(buf, 0, 4);
            _n = 0;
            switch (GetColorSpaceICC(Encoding.GetString(buf)))
            {
                case ColorSpaceICC.icSigRgbData:
                    _alternate[0] = "DeviceRGB";
                    _n = 3;

                    break;
                case ColorSpaceICC.icSigCmykData:
                    _alternate[0] = "DeviceCMYK";
                    _n = 4;
                    break;
                case ColorSpaceICC.icSigGrayData:
                    _alternate[0] = "DeviceGray";
                    _n = 1;
                    break;
            }

            _range = new float[_n * 2];
            for (int i = 0; i < _n; ++i)
            {
                _range[i * 2] = 0.0f;
                _range[i * 2 + 1] = 1.0f;
            }

            _dict.Dictionary.AddItem("N", new PDFNumber(_n));
            PDFArray array = new PDFArray();
            array.AddItem(new PDFName(_alternate[0]));
            _dict.Dictionary.AddItem("Alternate", array);
            array = new PDFArray();
            for (int i = 0; i < _n; ++i)
            {
                array.AddItem(new PDFNumber(_range[i * 2]));
                array.AddItem(new PDFNumber(_range[i * 2 + 1]));
            }
            _dict.Dictionary.AddItem("Range", array);
            buf = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(buf, 0, (int)stream.Length);
            _stream.Write(buf, 0, buf.Length);

            _name = "";
        }
    }
}
