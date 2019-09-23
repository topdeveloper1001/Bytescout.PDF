using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class PageLabel
#else
	/// <summary>
    /// Represents a PDF page label item.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class PageLabel
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private int _firstPageIndex;

	    /// <summary>
	    /// Gets or sets the page label style.
	    /// </summary>
	    /// <value cref="PageNumberingStyle"></value>
	    public PageNumberingStyle Style
	    {
		    get
		    {
			    PDFName s = _dictionary["S"] as PDFName;
			    return TypeConverter.PDFNameToPDFPageNumberingStyle(s);
		    }
		    set
		    {
			    _dictionary.AddItem("S", TypeConverter.PDFPageNumberingStyleToPDFName(value));
		    }
	    }

	    /// <summary>
	    /// Gets or sets the page label prefix.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string Prefix
	    {
		    get
		    {
			    PDFString prefix = _dictionary["P"] as PDFString;
			    if (prefix == null)
				    return "";
			    return prefix.GetValue();
		    }
		    set
		    {
			    if (value == null)
				    value = "";
			    _dictionary.AddItem("P", new PDFString(value));
		    }
	    }

	    /// <summary>
	    /// Gets or sets the page label start portion.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int StartPortion
	    {
		    get
		    {
			    PDFNumber startPortion = _dictionary["St"] as PDFNumber;
			    if (null == startPortion)
				    return 1;
			    return (int)startPortion.GetValue();
		    }
		    set
		    {
			    if (value < 1)
				    value = 1;
			    _dictionary.AddItem("St", new PDFNumber(value));
		    }
	    }

	    /// <summary>
	    /// Gets or sets the first page index.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int FirstPageIndex
	    {
		    get
		    {
			    return _firstPageIndex;
		    }
		    set
		    {
			    if (value < 0)
				    value = 0;
			    _firstPageIndex = value;
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.PageLabel.
        /// </summary>
        /// <param name="firstPageIndex" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"> The page index of the first page in a labeling range.</param>
        /// <param name="style">The style of the page label.</param>
        public PageLabel(int firstPageIndex, PageNumberingStyle style)
        {
            if (firstPageIndex < 0)
                throw new IndexOutOfRangeException();

            _firstPageIndex = firstPageIndex;
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("S", TypeConverter.PDFPageNumberingStyleToPDFName(style));
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.PageLabel.
        /// </summary>
        /// <param name="firstPageIndex" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"> The page index of the first page in a labeling range.</param>
        /// <param name="style">The style of the page label.</param>
        /// <param name="startPortion" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The start portion of the page label.</param>
        public PageLabel(int firstPageIndex, PageNumberingStyle style, int startPortion)
        {
            if (firstPageIndex < 0)
                throw new IndexOutOfRangeException();
            if (startPortion < 1)
                throw new ArgumentOutOfRangeException("startPortion");

            _firstPageIndex = firstPageIndex;
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("S", TypeConverter.PDFPageNumberingStyleToPDFName(style));
            _dictionary.AddItem("St", new PDFNumber(startPortion));
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.PageLabel.
        /// </summary>
        /// <param name="firstPageIndex" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"> The page index of the first page in a labeling range.</param>
        /// <param name="prefix" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The prefix of the page label.</param>
        public PageLabel(int firstPageIndex, string prefix)
        {
            if (firstPageIndex < 0)
                throw new IndexOutOfRangeException();
            if (prefix == null)
                prefix = string.Empty;

            _firstPageIndex = firstPageIndex;
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("P", new PDFString(prefix));
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.PageLabel.
        /// </summary>
        /// <param name="firstPageIndex" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"> The page index of the first page in a labeling range.</param>
        /// <param name="style">The style of the page label.</param>
        /// <param name="prefix" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The prefix of the page label.</param>
        public PageLabel(int firstPageIndex, PageNumberingStyle style, string prefix)
        {
            if (firstPageIndex < 0)
                throw new IndexOutOfRangeException();
            if (prefix == null)
                prefix = string.Empty;

            _firstPageIndex = firstPageIndex;
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("S", TypeConverter.PDFPageNumberingStyleToPDFName(style));
            _dictionary.AddItem("P", new PDFString(prefix));
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.PageLabel.
        /// </summary>
        /// <param name="firstPageIndex" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"> The page index of the first page in a labeling range.</param>
        /// <param name="style">The style of the page label.</param>
        /// <param name="prefix" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The prefix of the page label.</param>
        /// <param name="startPortion" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The start portion of the page label.</param>
        public PageLabel(int firstPageIndex, PageNumberingStyle style, string prefix, int startPortion)
        {
            if (firstPageIndex < 0)
                throw new IndexOutOfRangeException();
            if (prefix == null)
                prefix = string.Empty;
            if (startPortion < 1)
                throw new ArgumentOutOfRangeException("startPortion");

            _firstPageIndex = firstPageIndex;
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("S", TypeConverter.PDFPageNumberingStyleToPDFName(style));
            _dictionary.AddItem("P", new PDFString(prefix));
            _dictionary.AddItem("St", new PDFNumber(startPortion));
        }

        internal PageLabel(int firstPageIndex, PDFDictionary dict)
        {
            if (firstPageIndex < 0)
                throw new IndexOutOfRangeException();

            _firstPageIndex = firstPageIndex;
            _dictionary = dict;
        }

	    internal static PDFDictionary Copy(PDFDictionary dict)
	    {
		    PDFDictionary newDict = new PDFDictionary();
		    PDFName type = dict["Type"] as PDFName;
		    if (type != null)
			    newDict.AddItem("Type", type.Clone());

		    PDFName s = dict["S"] as PDFName;
		    if (s != null)
			    newDict.AddItem("S", s.Clone());

		    PDFString p = dict["P"] as PDFString;
		    if (p != null)
			    newDict.AddItem("P", p.Clone());

		    PDFNumber st = dict["St"] as PDFNumber;
		    if (st != null)
			    newDict.AddItem("St", st.Clone());

		    return newDict;
	    }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        internal PageLabel Clone()
        {
            PageLabel pageLabel = new PageLabel(_firstPageIndex, PageLabel.Copy(GetDictionary()));
            return pageLabel;
        }
    }
}
