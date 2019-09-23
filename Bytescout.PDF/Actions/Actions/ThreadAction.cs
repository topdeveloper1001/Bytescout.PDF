using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ThreadAction : Action
#else
	/// <summary>
    /// Represents an action which jumps to a specified bead on an article thread.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class ThreadAction : Action
#endif
	{
	    private PDFDictionary m_dictionary;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.Thread; } }

	    internal ThreadAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            m_dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("Thread"));

            IPDFObject fs = m_dictionary["F"];
            if (fs != null)
                dict.AddItem("F", fs);

            string[] keys = { "D", "B" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = m_dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            ThreadAction action = new ThreadAction(dict, owner);

            IPDFObject next = m_dictionary["Next"];
            if (next != null)
            {
                for (int i = 0; i < Next.Count; ++i)
                    action.Next.Add(Next[i]);
            }

            return action;
        }

        internal override PDFDictionary GetDictionary()
        {
            return m_dictionary;
        }
    }
}
