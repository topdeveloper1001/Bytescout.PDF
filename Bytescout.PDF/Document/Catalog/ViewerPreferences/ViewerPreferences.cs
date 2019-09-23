using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ViewerPreferences
#else
	/// <summary>
    /// Represents the class for the PDF viewer application preferences.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class ViewerPreferences
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
        /// Gets or sets a value indicating whether to hide the viewer applications tool bars when
        /// the document is active.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool HideToolbar
        {
            get
            {
                PDFBoolean hideToolbar = _dictionary["HideToolbar"] as PDFBoolean;
                if (hideToolbar == null)
                    return false;
                return hideToolbar.GetValue();
            }
            set { _dictionary.AddItem("HideToolbar", new PDFBoolean(value)); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the viewer applications menu bar when
        /// the document is active.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool HideMenubar
        {
            get
            {
                PDFBoolean hideMenubar = _dictionary["HideMenubar"] as PDFBoolean;
                if (hideMenubar == null)
                    return false;
                return hideMenubar.GetValue();
            }
            set { _dictionary.AddItem("HideMenubar", new PDFBoolean(value)); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to hide user interface elements in the documents window (such as scroll bars and navigation controls),
        /// leaving only the document’s contents displayed.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool HideWindowUI
        {
            get
            {
                PDFBoolean hideWindowUI = _dictionary["HideWindowUI"] as PDFBoolean;
                if (hideWindowUI == null)
                    return false;
                return hideWindowUI.GetValue();
            }
            set { _dictionary.AddItem("HideWindowUI", new PDFBoolean(value)); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to resize the document’s window to fit
        /// the size of the first displayed page.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool FitWindow
        {
            get
            {
                PDFBoolean fitWindow = _dictionary["FitWindow"] as PDFBoolean;
                if (fitWindow == null)
                    return false;
                return fitWindow.GetValue();
            }
            set { _dictionary.AddItem("FitWindow", new PDFBoolean(value)); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to position the document’s window in
        /// the center of the screen.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool CenterWindow
        {
            get
            {
                PDFBoolean centerWindow = _dictionary["CenterWindow"] as PDFBoolean;
                if (centerWindow == null)
                    return false;
                return centerWindow.GetValue();
            }
            set { _dictionary.AddItem("CenterWindow", new PDFBoolean(value)); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the window’s title bar should display
        /// the document title taken from the Title property of the document information.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool DisplayDocumentTitle
        {
            get
            {
                PDFBoolean displayDocTitle = _dictionary["DisplayDocTitle"] as PDFBoolean;
                if (displayDocTitle == null)
                    return false;
                return displayDocTitle.GetValue();
            }
            set { _dictionary.AddItem("DisplayDocTitle", new PDFBoolean(value)); }
        }

        /// <summary>
        /// Gets or sets how to display the document on exiting full-screen mode.
        /// <remarks>This property is meaningful only if the value of the PageMode property in the document is
        /// FullScreen; it is ignored otherwise.</remarks>
        /// </summary>
        /// <value cref="PDF.FullScreenPageMode"></value>
        public FullScreenPageMode FullScreenPageMode
        {
            get { return TypeConverter.PDFNameToPDFFullScreenPageMode(_dictionary["NonFullScreenPageMode"] as PDFName); }
            set { _dictionary.AddItem("NonFullScreenPageMode", TypeConverter.PDFFullScreenPageModeToPDFName(value)); }
        }

        /// <summary>
        /// Gets or sets the predominant reading order for text.
        /// <remarks>This property has no direct effect on the document’s contents or page numbering but can be used to
        /// determine the relative positioning of pages when displayed side by side or printed n-up.</remarks>
        /// </summary>
        /// <value cref="PDF.Direction"></value>
        public Direction Direction
        {
            get { return TypeConverter.PDFNameToPDFDirection(_dictionary["Direction"] as PDFName); }
            set { _dictionary.AddItem("Direction", TypeConverter.PDFDirectionToPDFName(value)); }
        }

	    internal ViewerPreferences()
	    {
		    _dictionary = new PDFDictionary();
	    }

	    internal ViewerPreferences(PDFDictionary dictionary)
	    {
		    _dictionary = dictionary;
	    }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }
    }
}
