using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class URIAction : Action
#else
	/// <summary>
    /// Represents an action which resolves a unique resource identifier.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class URIAction : Action
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.URI; } }

	    /// <summary>
	    /// Gets or sets the unique resource identifier.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string URI
	    {
		    get
		    {
			    PDFString uri = _dictionary["URI"] as PDFString;
			    if (uri != null)
				    return uri.GetValue();
			    return "";
		    }
		    set
		    {
			    if (value == null)
				    value = "";
			    _dictionary.AddItem("URI", new PDFString(System.Text.Encoding.ASCII.GetBytes(value), false));
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.URIAction.
        /// </summary>
        /// <param name="uri" href="http://msdn.microsoft.com/en-us/library/system.uri.aspx">The unique resource identifier.</param>
        public URIAction(Uri uri)
            : base(null)
        {
            if (uri == null)
                throw new ArgumentNullException();
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("URI"));
            _dictionary.AddItem("URI", new PDFString(System.Text.Encoding.ASCII.GetBytes(uri.AbsoluteUri), false));
        }

        internal URIAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("URI"));

            string[] keys = { "URI", "IsMap" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = _dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            URIAction action = new URIAction(dict, owner);

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
