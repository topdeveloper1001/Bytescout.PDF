using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class MovieAction : Action
#else
    /// <summary>
    /// Represents an action which plays a movie in a floating window or within the annotation
    /// rectangle of a movie annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class MovieAction : Action
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.Movie; } }

	    /// <summary>
	    /// Gets or sets the operation to be performed on the movie.
	    /// </summary>
	    /// <value cref="MovieOperation"></value>
	    public MovieOperation Operation
	    {
		    get { return TypeConverter.PDFNameToPDFMoveOperation(_dictionary["Operation"] as PDFName); }
		    set { _dictionary.AddItem("Operation", TypeConverter.PDFMoveOperationToPDFName(value)); }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.MovieAction.
        /// </summary>
        /// <param name="annotation">A movie annotation identifying the movie to be played.</param>
        /// <param name="operation">The operation to be performed on the movie.</param>
        public MovieAction(MovieAnnotation annotation, MovieOperation operation)
            : base(null)
        {
            if (annotation == null)
                throw new ArgumentNullException("annotation");
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("Movie"));
            _dictionary.AddItem("Annotation", annotation.Dictionary);
            Operation = operation;
        }

        internal MovieAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("Movie"));

            PDFDictionary annot = _dictionary["Annotation"] as PDFDictionary;
            if (annot != null)
                dict.AddItem("Annotation", annot);

            string[] keys = { "Operation", "T" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = _dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            MovieAction action = new MovieAction(dict, owner);

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
