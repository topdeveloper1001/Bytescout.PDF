using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class AnnotationBorderStyle
#else
	/// <summary>
    /// Represents a class for the border style of annotations.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class AnnotationBorderStyle
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
	    /// Gets or sets the border width.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float Width
	    {
		    get
		    {
			    PDFNumber w = _dictionary["W"] as PDFNumber;
			    if (w == null)
				    return 1;
			    return (float)w.GetValue();
		    }
		    set
		    {
			    if (value < 0)
				    throw new ArgumentOutOfRangeException();
			    _dictionary.AddItem("W", new PDFNumber(value));

			    if (ChangedBorderStyle != null)
				    ChangedBorderStyle(this);
		    }
	    }

	    /// <summary>
	    /// Gets or sets the border style.
	    /// </summary>
	    /// <value cref="BorderStyle"></value>
	    public BorderStyle Style
	    {
		    get { return TypeConverter.PDFNameToPDFBorderStyle(_dictionary["S"] as PDFName); }
		    set
		    {
			    _dictionary.AddItem("S", TypeConverter.PDFBorderStyleToPDFName(value));
			    if (value == BorderStyle.Dashed && _dictionary["D"] == null)
				    DashPattern = new DashPattern(new float[] { 3 });

			    if (ChangedBorderStyle != null)
				    ChangedBorderStyle(this);
		    }
	    }

	    /// <summary>
	    /// Gets or sets the dash pattern to be used in drawing a dashed border.
	    /// (Bytescout.PDF.BorderStyle.Dashed).
	    /// </summary>
	    /// <value cref="PDF.DashPattern"></value>
	    public DashPattern DashPattern
	    {
		    get
		    {
			    PDFArray array = _dictionary["D"] as PDFArray;
			    DashPattern dash = null;
			    if (array != null)
			    {
				    float[] pattern = new float[array.Count];
				    for (int i = 0; i < array.Count; ++i)
				    {
					    PDFNumber number = array[i] as PDFNumber;
					    if (number != null)
						    pattern[i] = (float)number.GetValue();
					    else
					    {
						    dash = new DashPattern(new float[] { 3 });
						    break;
					    }
				    }
				    dash = new DashPattern(pattern);
			    }
			    else
			    {
				    dash = new DashPattern(new float[]{3});
			    }

			    return dash;
		    }
		    set
		    {
			    if (value == null)
				    throw new NullReferenceException();

			    PDFArray array = new PDFArray();
			    float[] pattern = value.GetPattern();

			    for (int i = 0; i < pattern.Length; ++i)
				    array.AddItem(new PDFNumber(pattern[i]));
                
			    _dictionary.AddItem("D", array);

			    if (ChangedBorderStyle != null)
				    ChangedBorderStyle(this);
		    }
	    }

	    internal event ChangedBorderStyleEventHandler ChangedBorderStyle;

	    internal AnnotationBorderStyle()
        {
            _dictionary = new PDFDictionary();
        }

        internal AnnotationBorderStyle(PDFDictionary dict)
        {
            _dictionary = dict;
        }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary result = new PDFDictionary();
            string[] keys = {"Type", "W", "S" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = dict[keys[i]];
                if (obj != null)
                    result.AddItem(keys[i], obj.Clone());
            }

            PDFArray dash = dict["D"] as PDFArray;
            if (dash != null)
            {
                PDFArray arr = new PDFArray();
                for (int i = 0; i < dash.Count; ++i)
                    arr.AddItem(dash[i].Clone());
                result.AddItem("D", arr);
            }

            return result;
        }
    }
}
