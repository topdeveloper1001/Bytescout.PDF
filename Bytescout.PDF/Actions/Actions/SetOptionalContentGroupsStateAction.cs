using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class SetOptionalContentGroupsStateAction : Action
#else
	/// <summary>
    /// Represents an action which sets the states of optional content groups.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class SetOptionalContentGroupsStateAction : Action
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
        /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
        /// </summary>
        /// <value cref="ActionType"></value>
        public override ActionType Type { get { return ActionType.SetOCGState; } }

	    internal SetOptionalContentGroupsStateAction(PDFDictionary dict, IDocumentEssential owner)
		    : base(owner)
	    {
		    _dictionary = dict;
	    }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("SetOCGState"));

            string[] keys = { "State", "PreserveRB" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = _dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            SetOptionalContentGroupsStateAction action = new SetOptionalContentGroupsStateAction(dict, owner);

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
