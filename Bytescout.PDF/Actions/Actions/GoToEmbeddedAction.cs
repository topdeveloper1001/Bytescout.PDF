using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class GoToEmbeddedAction : Action
#else
    /// <summary>
    /// Represents an action which goes to a destination in an embedded file.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class GoToEmbeddedAction : Action
#endif
	{
		private readonly PDFDictionary _dictionary;

	    /// <summary>
        /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
        /// </summary>
        /// <sq_access>read</sq_access>
        /// <sq_modifier>override</sq_modifier>
        /// <value cref="ActionType"></value>
        public override ActionType Type { get { return ActionType.GoToEmbedded; } }

	    internal GoToEmbeddedAction(PDFDictionary dict, IDocumentEssential owner)
		    : base(owner)
	    {
		    _dictionary = dict;
	    }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("GoToE"));

            PDFDictionary fs = _dictionary["F"] as PDFDictionary;
            if (fs != null)
                dict.AddItem("F", fs);

            string[] keys = { "D", "NewWindow", "T" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = _dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            GoToEmbeddedAction action = new GoToEmbeddedAction(dict, owner);

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
