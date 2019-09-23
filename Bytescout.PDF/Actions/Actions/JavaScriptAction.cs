using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class JavaScriptAction : Action
#else
	/// <summary>
    /// Represents an action which performs a JavaScript action in the PDF document.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class JavaScriptAction : Action
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.JavaScript; } }

	    /// <summary>
	    /// Gets or sets the javascript code to be executed when this action is executed.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string Script
	    {
		    get
		    {
			    IPDFObject js = _dictionary["JS"];
                
			    if (js is PDFString)
				    return (js as PDFString).GetValue();
	            
			    if (js is PDFDictionaryStream)
			    {
				    (js as PDFDictionaryStream).Decode();
				    Stream stream = (js as PDFDictionaryStream).GetStream();
				    stream.Position = 0;
				    byte[] buf = new byte[stream.Length];
				    stream.Read(buf, 0, buf.Length);

				    if (buf.Length >= 2 && buf[0] == 254 && buf[1] == 255)
					    return System.Text.Encoding.BigEndianUnicode.GetString(buf, 2, buf.Length - 2);
				    return Encoding.GetString(buf);
			    }

			    return "";
		    }
		    set
		    {
			    if (value == null)
				    value = "";
			    _dictionary.AddItem("JS", new PDFString(value));
		    }
	    }

	    /// <summary>
		/// Initializes a new instance of the Bytescout.PDF.JavaScriptAction.
        /// </summary>
        /// <param name="script" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The JavaScript code.</param>
        public JavaScriptAction(string script)
            : base(null)
        {
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("JavaScript"));
            Script = script;
        }

        internal JavaScriptAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("JavaScript"));

            IPDFObject js = _dictionary["JS"];
            if (js != null)
                dict.AddItem("JS", js.Clone());

            JavaScriptAction action = new JavaScriptAction(dict, owner);

            IPDFObject next = _dictionary["Next"];
            if (next != null)
            {
                for (int i = 0; i < Next.Count; ++i)
                    action.Next.Add(Next[i]);
            }

            return action;
        }

        internal override PDFDictionary GetDictionary()
        {
            return _dictionary;
        }
    }
}
