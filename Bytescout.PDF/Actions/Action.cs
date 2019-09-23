using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class Action
#else
	/// <summary>
    /// Represents an abstract class for the PDF actions.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class Action
#endif
    {
	    private IDocumentEssential _owner;
	    private ActionCollection _next;

	    /// <summary>
        /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
        /// </summary>
        /// <value cref="ActionType"></value>
        public abstract ActionType Type { get; }

        /// <summary>
        /// Gets the sequence of actions to be performed after the action represented by this instance.
        /// </summary>
        /// <value cref="ActionCollection"></value>
        public ActionCollection Next
        {
            get
            {
                if (_next == null)
                    _next = new ActionCollection(GetDictionary()["Next"], _owner, GetDictionary(), "Next");

                return _next;
            }
        }

        internal IDocumentEssential Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

	    internal Action(IDocumentEssential owner)
	    {
		    _owner = owner;
	    }

	    internal abstract Action Clone(IDocumentEssential owner);

        internal abstract PDFDictionary GetDictionary();

        internal virtual void ApplyOwner(IDocumentEssential owner)
        {
            _owner = owner;
            if (GetDictionary()["Next"] != null)
            {
                ActionCollection next = Next;
                for (int i = 0; i < next.Count; ++i)
                    next[i].ApplyOwner(owner);
            }
        }

        internal static Action Create(PDFDictionary dict, IDocumentEssential owner)
        {
            if (dict == null)
                return null;

            PDFName type = dict["S"] as PDFName;
            switch (type.GetValue())
            {
                case "GoTo":
                    return new GoToAction(dict, owner);
                case "GoToR":
                    return new GoToRemoteAction(dict, owner);
                case "GoToE":
                    return new GoToEmbeddedAction(dict, owner);
                case "Launch":
                    return new LaunchAction(dict, owner);
                case "Thread":
                    return new ThreadAction(dict, owner);
                case "URI":
                    return new URIAction(dict, owner);
                case "Sound":
                    return new SoundAction(dict, owner);
                case "Movie":
                    return new MovieAction(dict, owner);
                case "Hide":
                    return new HideAction(dict, owner);
                case "Named":
                    return new NamedAction(dict, owner);
                case "SubmitForm":
                    return new SubmitFormAction(dict, owner);
                case "ResetForm":
                    return new ResetFormAction(dict, owner);
                case "ImportData":
                    return new ImportDataAction(dict, owner);
                case "JavaScript":
                    return new JavaScriptAction(dict, owner);
                case "SetOCGState":
                    return new SetOptionalContentGroupsStateAction(dict, owner);
                case "Rendition":
                    return new RenditionAction(dict, owner);
                case "Trans":
                    return new TransitionAction(dict, owner);
                case "GoTo3DView":
                    return new GoTo3DViewAction(dict, owner);
            }

            return null;
        }
    }
}
