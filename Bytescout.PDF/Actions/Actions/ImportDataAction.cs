using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ImportDataAction: Action
#else
    /// <summary>
    /// Represents an action which imports Forms Data Format (FDF) data into the document’s
    /// interactive form from a specified file.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class ImportDataAction: Action
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private FileSpecification _fileSpecification;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.ImportData; } }

	    /// <summary>
	    /// Gets the path to the FDF file from which to import the data.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string FileName
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
        /// Initializes a new instance of the Bytescout.PDF.ImportDataAction.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The FDF file from which to import the data.</param>
        public ImportDataAction(string fileName)
            : base(null)
        {
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("ImportData"));

            if (fileName == null)
                fileName = "";
            _fileSpecification = new SimpleFileSpecification(fileName);
            _dictionary.AddItem("F", _fileSpecification.GetDictionary());
        }

        internal ImportDataAction(PDFDictionary dict, IDocumentEssential owner)
            :base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("ImportData"));

            PDFDictionary fs = _dictionary["F"] as PDFDictionary;
            if (fs != null)
                dict.AddItem("F", fs);

            ImportDataAction action = new ImportDataAction(dict, owner);

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
