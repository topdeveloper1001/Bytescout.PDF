using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class MovieAnnotation: Annotation
#else
	/// <summary>
    /// Represents a movie annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class MovieAnnotation: Annotation
#endif
	{
	    private readonly Movie _movie;
	    private MovieActivation _movieActivation;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.Movie; } }

	    /// <summary>
	    /// Gets or sets the x-coordinate of the left edge of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Left
	    {
		    get { return base.Left; }
		    set { base.Left = value; }
	    }

	    /// <summary>
	    /// Gets or sets the y-coordinate of the top edge of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Top
	    {
		    get { return base.Top; }
		    set { base.Top = value; }
	    }

	    /// <summary>
	    /// Gets or sets the width of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Width
	    {
		    get { return base.Width; }
		    set { base.Width = value; }
	    }

	    /// <summary>
	    /// Gets or sets the height of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Height
	    {
		    get { return base.Height; }
		    set { base.Height = value; }
	    }

	    /// <summary>
	    /// Gets the movie to be played when the annotation is activated.
	    /// </summary>
	    /// <value cref="PDF.Movie"></value>
	    public Movie Movie
	    { 
		    get
		    {
			    return _movie;
		    }
	    }

	    /// <summary>
	    /// Gets the value specifying whether and how to play the movie when the annotation is activated.
	    /// </summary>
	    /// <value cref="PDF.MovieActivation"></value>
	    public MovieActivation MovieActivation
	    {
		    get
		    {
			    if (_movieActivation == null)
				    loadMovieActivation();
			    return _movieActivation;
		    }
	    }

	    /// <summary>
	    /// Gets or sets the title of the annotation.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string Title
	    {
		    get
		    {
			    PDFString str = Dictionary["T"] as PDFString;
			    if (str == null)
				    return "";
			    return str.GetValue();
		    }
		    set
		    {
			    if (string.IsNullOrEmpty(value))
				    Dictionary.RemoveItem("T");
			    Dictionary.AddItem("T", new PDFString(value));
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.MovieAnnotation class.
        /// </summary>
        /// <param name="movie">The movie to be played.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        public MovieAnnotation(Movie movie, float left, float top, float width, float height)
            : base(left, top, width, height, null)
        {
            if (movie == null)
                throw new ArgumentNullException("movie");

            Dictionary.AddItem("Subtype", new PDFName("Movie"));
            _movie = movie;
            Dictionary.AddItem("Movie", _movie.GetDictionary());
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.MovieAnnotation class.
        /// </summary>
        /// <param name="movie">The movie to be played.</param>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        public MovieAnnotation(Movie movie, RectangleF boundingBox)
            : this(movie, boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height) { }

        internal MovieAnnotation(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

	    internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                ApplyOwner(owner);
                SetPage(page, true);
                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);

            IPDFObject t = Dictionary["T"];
            if (t != null)
                res.AddItem("T", t.Clone());

            PDFDictionary movie = Dictionary["Movie"] as PDFDictionary;
            if (movie != null)
                res.AddItem("Movie", MovieBase.Copy(movie));

            IPDFObject a = Dictionary["A"];
            if (a != null)
            {
                if (a is PDFBoolean)
                    res.AddItem("A", a.Clone());
                else if (a is PDFDictionary)
                    res.AddItem("A", MovieActivationBase.Copy(a as PDFDictionary));
            }

            MovieAnnotation annot = new MovieAnnotation(res, owner);
            annot.SetPage(Page, false);
            annot.SetPage(page, true);
            return annot;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            Owner = owner;
        }

        internal override void CreateApperance()
        {
        }

        private void loadMovieActivation()
        {
            PDFDictionary a = Dictionary["A"] as PDFDictionary;
            if (a != null)
            {
                _movieActivation = new MovieActivation(a);
            }
            else
            {
                _movieActivation = new MovieActivation();
                Dictionary.AddItem("A", _movieActivation.GetDictionary());
            }

        }
    }
}
