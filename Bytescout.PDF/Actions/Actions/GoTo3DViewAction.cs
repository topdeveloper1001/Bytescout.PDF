using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class GoTo3DViewAction: Action
#else
    /// <summary>
    /// Represents an action which sets the current view of a 3D annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class GoTo3DViewAction: Action
#endif
	{
		private PDFDictionary _dictionary;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.GoTo3DView; } }

	    internal GoTo3DViewAction(PDFDictionary dict, IDocumentEssential owner)
            :base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("GoTo3DView"));

            string[] keys = { "TA", "V" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = _dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            GoTo3DViewAction action = new GoTo3DViewAction(dict, owner);

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
