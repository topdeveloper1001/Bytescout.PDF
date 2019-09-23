using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class GoToAction : Action
#else
    /// <summary>
    /// Represents an action which goes to a destination in the current document.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class GoToAction : Action
#endif
	{
		private Destination _dest;
		private readonly PDFDictionary _dictionary;

	    /// <summary>
	    /// Gets or sets the destination.
	    /// </summary>
	    /// <value cref="PDF.Destination"></value>
	    public Destination Destination
	    {
		    get { return _dest; }
		    set { setDestination(value); }
	    }

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.GoTo; } }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.GoToAction.
        /// </summary>
        /// <param name="dest">The destination to jump to.</param>
        public GoToAction(Destination dest)
            : base(null)
        {
            if (dest == null)
                throw new ArgumentNullException();

            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("GoTo"));
            Destination = dest;
        }

        internal GoToAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            _dictionary = dict;
            loadDestination();
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("GoTo"));

            GoToAction action = new GoToAction(dict, owner);
            action.Destination = Destination;

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

        private void setDestination(Destination dest)
        {
            if (dest == null)
                throw new NullReferenceException();
            _dictionary.AddItem("D", dest.GetArray());
            _dest = dest;
        }

        private void loadDestination()
        {
            IPDFObject dest = _dictionary["D"];
            if (dest == null)
                return;

            _dest = new Destination(dest, Owner);
        }
    }
}
