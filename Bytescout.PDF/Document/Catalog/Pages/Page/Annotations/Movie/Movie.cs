using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Movie
#else
	/// <summary>
    /// Represents a movie used in the PDF document.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class Movie
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private FileSpecification _fileSpecification;

	    /// <summary>
        /// Gets the path to a movie file.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string FilePath
        {
            get
            {
                if (_fileSpecification == null)
                    loadFileSpecification();

                if (_fileSpecification == null)
                    return "";

                return _fileSpecification.FileName;
            }
        }

        /// <summary>
        /// Gets or sets the rotation angle.
        /// </summary>
        /// <value cref="RotationAngle"></value>
        public RotationAngle Rotation
        {
            get { return TypeConverter.PDFNumberToRotationAngle(_dictionary["Rotate"] as PDFNumber); }
            set { _dictionary.AddItem("Rotate", TypeConverter.RotationAngleToPDFNumber(value)); }
        }

        /// <summary>
        /// Gets or sets a poster image representing the movie.
        /// </summary>
        /// <value cref="Image"></value>
        public Image Poster
        {
            get
            {
                PDFDictionaryStream image = _dictionary["Poster"] as PDFDictionaryStream;
                if (image != null)
                    return new Image(image);
                return null;
            }
            set
            {
                _dictionary.AddItem("Poster", value.GetDictionary());
            }
        }

        internal float AspectWidth
        {
            get
            {
                PDFArray array = _dictionary["Aspect"] as PDFArray;
                if (array == null)
                {
                    return 1;
                }
                if (array.Count != 2)
                {
                    PDFArray aspect = new PDFArray();
                    _dictionary.AddItem("Aspect", aspect);
                    aspect.AddItem(new PDFNumber(1));
                    aspect.AddItem(new PDFNumber(1));
                    return 1;
                }
                PDFNumber number = array[0] as PDFNumber;
                if (number == null)
                    return 1;
                return (float)number.GetValue();
            }
            set
            {
                PDFArray aspect = _dictionary["Aspect"] as PDFArray;
                if (aspect == null)
                {
                    aspect = new PDFArray();
                    _dictionary.AddItem("Aspect", aspect);
                    aspect.AddItem(new PDFNumber(value));
                    aspect.AddItem(new PDFNumber(1));
                }
                aspect.RemoveItem(0);
                aspect.Insert(0, new PDFNumber(value));
            }
        }

        internal float AspectHeight
        {
            get
            {
                PDFArray array = _dictionary["Aspect"] as PDFArray;
                if (array == null)
                {
                    return 1;
                }
                if (array.Count != 2)
                {
                    array = new PDFArray();
                    _dictionary.AddItem("Aspect", array);
                    array.AddItem(new PDFNumber(1));
                    array.AddItem(new PDFNumber(1));
                    return 1;
                }
                PDFNumber number = array[1] as PDFNumber;
                if (number == null)
                    return 1;
                return (float)number.GetValue();
            }
            set
            {
                PDFArray aspect = _dictionary["Aspect"] as PDFArray;
                if (aspect == null)
                {
                    aspect = new PDFArray();
                    _dictionary.AddItem("Aspect", aspect);
                    aspect.AddItem(new PDFNumber(1));
                    aspect.AddItem(new PDFNumber(value));
                }
                aspect.RemoveItem(1);
                aspect.AddItem(new PDFNumber(value));
            }
        }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.Movie class.
	    /// </summary>
	    /// <param name="filePath" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The path to the movie.</param>
	    public Movie(string filePath)
	    {
		    if (filePath == null)
			    filePath = "";
		    _fileSpecification = new SimpleFileSpecification(filePath);
		    _dictionary = new PDFDictionary();
		    _dictionary.AddItem("F", _fileSpecification.GetDictionary());
		    _dictionary.AddItem("Poster", new PDFBoolean(true));
	    }

	    internal Movie(PDFDictionary dict) // ???
	    {
		    _dictionary = new PDFDictionary();
	    }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        private void loadFileSpecification()
        {
            PDFDictionary fs = _dictionary["F"] as PDFDictionary;
            if (fs != null)
            {
                _fileSpecification = new FileSpecification(fs);
            }
        }
    }
}
