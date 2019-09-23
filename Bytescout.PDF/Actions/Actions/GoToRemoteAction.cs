using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class GoToRemoteAction: Action
#else
    /// <summary>
    /// Represents an action which goes to a destination in another document.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class GoToRemoteAction: Action
#endif
	{
		private readonly PDFDictionary _dictionary;
		private FileSpecification _fileSpecification;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.GoToRemote; } }

	    /// <summary>
	    /// Gets the path to the destination document.
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
	    /// Gets the index of the page to jump to.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int PageIndex
	    {
		    get
		    {
			    PDFArray d = _dictionary["D"] as PDFArray;
			    if (d != null)
			    {
				    PDFNumber page = d[0] as PDFNumber;
				    if (page != null)
					    return (int)page.GetValue();
			    }

			    return 0;
		    }
		    set
		    {
			    PDFArray arr = new PDFArray();
			    arr.AddItem(new PDFNumber(value));
			    arr.AddItem(new PDFName("XYZ"));
			    arr.AddItem(new PDFNull());
			    arr.AddItem(new PDFNull());
			    arr.AddItem(new PDFNull());
			    _dictionary.AddItem("D", arr);
		    }
	    }

	    /// <summary>
	    /// Gets or sets a value specifying whether to open the destination document in a new window.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool NewWindow
	    {
		    get
		    {
			    PDFBoolean val = _dictionary["NewWindow"] as PDFBoolean;
			    if (val == null)
				    return false;
			    return val.GetValue();
		    }
		    set
		    {
			    _dictionary.AddItem("NewWindow", new PDFBoolean(value));
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.GoToRemoteAction.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">Path to the destination document.</param>
        /// <param name="pageIndex" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The index of the page to jump to.</param>
        /// <param name="newWindow" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">A value specifying whether to open the destination document in a new window.</param>
        public GoToRemoteAction(string fileName, int pageIndex, bool newWindow)
            : base(null)
        {
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("GoToR"));
            NewWindow = newWindow;

            _fileSpecification = new SimpleFileSpecification(fileName);
            _dictionary.AddItem("F", _fileSpecification.GetDictionary());
            PageIndex = pageIndex;
        }

        internal GoToRemoteAction(PDFDictionary dict, IDocumentEssential owner)
            :base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("GoToR"));

            PDFDictionary fs = _dictionary["F"] as PDFDictionary;
            if (fs != null)
                dict.AddItem("F", fs);

            string[] keys = { "D", "NewWindow"};
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = _dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            GoToRemoteAction action = new GoToRemoteAction(dict, owner);

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

        private void loadFileSpecification()
        {
            PDFDictionary fs = _dictionary["F"] as PDFDictionary;
            if (fs != null)
                _fileSpecification = new FileSpecification(fs);
        }
    }
}
