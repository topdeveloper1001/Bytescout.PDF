using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class TransitionAction: Action
#else
	/// <summary>
    /// Represents an action which can be used to control drawing during a sequence of actions.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class TransitionAction: Action
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.Transition; } }

	    internal TransitionAction(PDFDictionary dict, IDocumentEssential owner)
            :base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("Trans"));

            PDFDictionary trans = _dictionary["Trans"] as PDFDictionary;
            if (trans != null)
                dict.AddItem("Trans", TransitionBase.Copy(trans));

            TransitionAction action = new TransitionAction(dict, owner);

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
