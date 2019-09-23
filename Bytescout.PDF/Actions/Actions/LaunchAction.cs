using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class LaunchAction: Action
#else
	/// <summary>
    /// Represents an action which launches an application or opens a document.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class LaunchAction: Action
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private FileSpecification _fileSpecification;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.Launch; } }

	    /// <summary>
	    /// Gets the path to the destination document or application.
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
        /// Initializes a new instance of the Bytescout.PDF.LaunchAction.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">Path to the application to be launched or the document to be opened.</param>
        public LaunchAction(string fileName)
            : base(null)
        {
            if (fileName == null)
                fileName = "";

            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("GoToR"));

            _fileSpecification = new SimpleFileSpecification(fileName);
            _dictionary.AddItem("F", _fileSpecification.GetDictionary());
        }

        internal LaunchAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("Launch"));

            PDFDictionary fs = _dictionary["F"] as PDFDictionary;
            if (fs != null)
                dict.AddItem("F", fs);

            string[] keys = { "Win", "Mac", "Unix", "NewWindow"};
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = _dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            LaunchAction action = new LaunchAction(dict, owner);

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
