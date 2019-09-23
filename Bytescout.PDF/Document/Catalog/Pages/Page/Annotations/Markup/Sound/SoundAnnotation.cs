using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class SoundAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents a sound annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class SoundAnnotation: MarkupAnnotation
#endif
	{
	    private Sound _sound;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.Sound; } }

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
		    set
		    {
			    base.Width = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the height of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Height
	    {
		    get { return base.Height; }
		    set
		    {
			    base.Height = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the icon to be used in displaying the annotation.
	    /// </summary>
	    /// <value cref="SoundAnnotationIcon"></value>
	    public SoundAnnotationIcon Icon
	    {
		    get { return TypeConverter.PDFNameToPDFSoundAnnotationIcon(Dictionary["Name"] as PDFName); }
		    set
		    {
			    Dictionary.AddItem("Name", TypeConverter.PDFSoundAnnotationIconToPDFName(value));
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets the sound to be played when the annotation is activated.
	    /// </summary>
	    /// <value cref="PDF.Sound"></value>
	    public Sound Sound
	    {
		    get
		    {
			    if (_sound == null)
				    loadSound();
			    return _sound;
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.SoundAnnotation class.
        /// </summary>
        /// <param name="sound">The sound to be played when the annotation is activated.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        public SoundAnnotation(Sound sound, float left, float top, float width, float height)
            : base(left, top, width, height, null)
        {
            if (sound == null)
                throw new ArgumentNullException("sound");

            Dictionary.AddItem("Subtype", new PDFName("Sound"));
            _sound = sound;
            Dictionary.AddItem("Sound", _sound.GetDictionaryStream());
            Color = new ColorRGB(0, 255, 0);
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.SoundAnnotation class.
        /// </summary>
        /// <param name="sound">The sound to be played when the annotation is activated.</param>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        public SoundAnnotation(Sound sound, RectangleF boundingBox)
            : this(sound, boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height) { }

        internal SoundAnnotation(PDFDictionary dict, IDocumentEssential owner)
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
            MarkupAnnotationBase.CopyTo(Dictionary, res);

            IPDFObject name = Dictionary["Name"];
            if (name != null)
                res.AddItem("Name", name.Clone());

            IPDFObject sound = Dictionary["Sound"];
            if (sound != null)
                res.AddItem("Sound", sound);

            SoundAnnotation annot = new SoundAnnotation(res, owner);
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
            Dictionary.RemoveItem("AP");
        }

        private void loadSound()
        {
            PDFDictionaryStream sound = Dictionary["Sound"] as PDFDictionaryStream;
            if (sound != null)
                _sound = new Sound(sound);
        }
    }
}
