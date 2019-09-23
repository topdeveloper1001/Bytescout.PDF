using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class AnnotationBorderEffect
#else
	/// <summary>
    /// Represents a class for the border effect of annotations.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class AnnotationBorderEffect
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
	    /// Gets or sets the border effect.
	    /// </summary>
	    /// <value cref="BorderEffect"></value>
	    public BorderEffect Effect
	    {
		    get { return TypeConverter.PDFNameToPDFBorderEffect(_dictionary["S"] as PDFName); }
		    set
		    {
			    _dictionary.AddItem("S", TypeConverter.PDFBorderEffectToPDFName(value));
			    if (ChangedBorderEffect != null)
				    ChangedBorderEffect(this);
		    }
	    }

	    /// <summary>
	    /// Gets or sets the value indicating whether the intensity of the effect.
	    /// <remarks>Suggested values range from 0 to 2.</remarks>
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float Intensity
	    {
		    get
		    {
			    PDFNumber i = _dictionary["I"] as PDFNumber;
			    if (i == null)
				    return 0;
			    return (float)i.GetValue();
		    }
		    set
		    {
			    if (value < 0 || value > 2)
				    throw new BorderEffectIntensityException();
			    _dictionary.AddItem("I", new PDFNumber(value));
                
			    if (ChangedBorderEffect != null)
				    ChangedBorderEffect(this);
		    }
	    }

	    internal event ChangedBorderEffectEventHandler ChangedBorderEffect;

	    internal AnnotationBorderEffect()
        {
            _dictionary = new PDFDictionary();
        }

        internal AnnotationBorderEffect(PDFDictionary dict)
        {
            _dictionary = dict;
        }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }
    }
}
