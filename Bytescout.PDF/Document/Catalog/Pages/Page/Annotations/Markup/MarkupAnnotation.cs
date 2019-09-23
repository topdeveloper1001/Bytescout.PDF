using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
    internal enum AnnotationState
    {
        Marked = 0,
        Unmarked = 1,
        Accepted = 2,
        Rejected = 3,
        Cancelled = 4,
        Completed = 5,
        None = 6
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class MarkupAnnotation : Annotation
#else
	/// <summary>
    /// Represents an abstract class for markup annotations.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class MarkupAnnotation : Annotation
#endif
	{
	    private PopupAnnotation _popup;

	    /// <summary>
        /// Gets or sets the color of this annotation.
        /// </summary>
        /// <value cref="DeviceColor"></value>
        public DeviceColor Color
        {
            get
            {
                if (Dictionary["C"] == null)
                    return null;
                return TypeConverter.PDFArrayToPDFColor(Dictionary["C"] as PDFArray);
            }
            set
            {
                if (value == null)
                    Dictionary.RemoveItem("C");
                else
                    Dictionary.AddItem("C", TypeConverter.PDFColorToPDFArray(value));
                CreateApperance();
            }
        }

        /// <summary>
        /// Gets or sets the annotation contents.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Contents
        {
            get
            {
                PDFString str = Dictionary["Contents"] as PDFString;
                if (str != null)
                    return str.GetValue();
                return "";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Dictionary.RemoveItem("Contents");
                else
                    Dictionary.AddItem("Contents", new PDFString(value));
                CreateApperance();
            }
        }

        /// <summary>
        /// Gets or sets the date and time when the annotation was modified.
        /// </summary>
        /// <value cref="DateTime" href="http://msdn.microsoft.com/en-us/library/system.datetime.aspx"></value>
        public DateTime ModificationDate
        {
            get
            {
                PDFString m = Dictionary["M"] as PDFString;
                if (m == null)
                    return DateTime.Today;
                return StringUtility.ParseDateTime(m.GetValue());
            }
            set
            {
                string time = StringUtility.CreatePDFDateTime(value);
                Dictionary.AddItem("M", new PDFString(System.Text.Encoding.ASCII.GetBytes(time), false));
            }
        }

        /// <summary>
        /// Gets or sets a string uniquely identifying it among all the annotations on the page.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Name
        {
            get
            {
                PDFString nm = Dictionary["NM"] as PDFString;
                if (nm == null)
                    return "";
                return nm.GetValue();
            }
            set
            {
                if (value == null)
                    value = "";
                Dictionary.AddItem("NM", new PDFString(value));
            }
        }

        /// <summary>
        /// Gets or sets the text label to be displayed in the title bar of the annotations pop-up window when it is open and active.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Author
        {
            get
            {
                PDFString author = Dictionary["T"] as PDFString;
                if (author != null)
                    return author.GetValue();
                return "";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Dictionary.RemoveItem("T");
                else
                    Dictionary.AddItem("T", new PDFString(value));
            }
        }

        /// <summary>
        /// Gets or sets the opacity value to be used in painting the annotation.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Opacity
        {
            get
            {
                PDFNumber opacity = Dictionary["CA"] as PDFNumber;
                if (opacity != null)
                    return (float)(opacity.GetValue() * 100);
                return 100;
            }
            set
            {
                if (value < 0 || value > 100)
                    throw new PDFOpacityException();
                Dictionary.AddItem("CA", new PDFNumber(value / 100));
                CreateApperance();
            }
        }

        /// <summary>
        /// Gets or sets the date and time when the annotation was created.
        /// </summary>
        /// <value cref="DateTime" href="http://msdn.microsoft.com/en-us/library/system.datetime.aspx"></value>
        public DateTime CreationDate
        {
            get
            {
                PDFString date = Dictionary["CreationDate"] as PDFString;
                if (date == null)
                    return DateTime.Today;
                return StringUtility.ParseDateTime(date.GetValue());
            }
            set
            {
                string time = StringUtility.CreatePDFDateTime(value);
                Dictionary.AddItem("CreationDate", new PDFString(System.Text.Encoding.ASCII.GetBytes(time), false));
            }
        }

        /// <summary>
        /// Gets or set the text representing a short description of the subject being addressed by the annotation.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Subject
        {
            get
            {
                PDFString subject = Dictionary["Subj"] as PDFString;
                if (subject != null)
                    return subject.GetValue();
                return "";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Dictionary.RemoveItem("Subj");
                else
                    Dictionary.AddItem("Subj", new PDFString(value));
            }
        }

        internal PopupAnnotation Popup
        {
            get
            {
                if (_popup != null)
                    return _popup;
                PDFDictionary popup = Dictionary["Popup"] as PDFDictionary;
                if (popup != null)
                {
                    _popup = new PopupAnnotation(popup, Owner);
                    _popup.SetPage(Page, false);
                }
                return _popup;
            }
        }

        internal AnnotationState State
        {
            get
            {
                return TypeConverter.PDFStringToPDFAnnotationState(Dictionary["State"] as PDFString, Dictionary["StateModel"] as PDFString);
            }
            set
            {
                PDFString state = TypeConverter.PDFAnnotationStateToPDFString(value);
                Dictionary.AddItem("State", state);

                if (value == AnnotationState.Marked || value == AnnotationState.Unmarked)
                    Dictionary.AddItem("StateModel", new PDFString(System.Text.Encoding.ASCII.GetBytes("Marked"), false));
                else
                    Dictionary.AddItem("StateModel", new PDFString(System.Text.Encoding.ASCII.GetBytes("Review"), false));
            }
        }

	    internal MarkupAnnotation(IDocumentEssential owner)
		    : base(owner) { }

	    internal MarkupAnnotation(float left, float top, float width, float height, IDocumentEssential owner)
		    : base(left, top, width, height, owner) { }

	    internal MarkupAnnotation(PDFDictionary dict, IDocumentEssential owner)
		    : base(dict, owner) { }
    }
}
