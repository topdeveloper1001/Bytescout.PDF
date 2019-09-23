using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class FileAttachmentAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents a file attachment annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class FileAttachmentAnnotation: MarkupAnnotation
#endif
	{
	    private FileSpecification _fileSpec;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.FileAttachment; } }

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
	    /// <value cref="FileAttachmentAnnotationIcon"></value>
	    public FileAttachmentAnnotationIcon Icon
	    {
		    get { return TypeConverter.PDFNameToPDFFileAttachmentAnnotationIcon(Dictionary["Name"] as PDFName); }
		    set
		    {
			    Dictionary.AddItem("Name", TypeConverter.PDFFileAttachmentAnnotationIconToPDFName(value));
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets the value specifying the name of embedded file.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string FileName
	    {
		    get
		    {
			    FileSpecification fs = FileSpecification;
			    if (fs == null)
				    return "";
			    return fs.FileName;
		    }
	    }

	    internal FileSpecification FileSpecification
	    {
		    get
		    {
			    if (_fileSpec == null)
			    {
				    PDFDictionary fs = Dictionary["FS"] as PDFDictionary;
				    if (fs == null)
					    return null;
				    _fileSpec = new FileSpecification(fs);
			    }
			    return _fileSpec;
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.FileAttachmentAnnotation class.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A path to the file to be embedded.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        public FileAttachmentAnnotation(string fileName, float left, float top, float width, float height) 
            : base(left, top, width, height, null)
        {
            init(new FullFileSpecification(fileName));
            Color = new ColorRGB(0, 0, 255);
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.FileAttachmentAnnotation class.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A path to the file to be embedded.</param>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        public FileAttachmentAnnotation(string fileName, RectangleF boundingBox)
            : this(fileName, boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height) { }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.FileAttachmentAnnotation class.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The stream specifying the content of the annotation's embedded file.</param>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A string value specifying the name of file to be embedded.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        public FileAttachmentAnnotation(Stream stream, string fileName, float left, float top, float width, float height)
            : base(left, top, width, height, null)
        {
            init(new FullFileSpecification(stream, fileName));
            Color = new ColorRGB(0, 0, 255);
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.FileAttachmentAnnotation class.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The stream specifying the content of the annotation's embedded file.</param>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A string value specifying the name of file to be embedded.</param>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        public FileAttachmentAnnotation(Stream stream, string fileName, RectangleF boundingBox)
            : this(stream, fileName, boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height) { }

        internal FileAttachmentAnnotation(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

	    /// <summary>
        /// Saves the contents of a file attachment annotation into the specified file.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the file where to save the data.</param>
        public void SaveEmbeddedFile(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Create);
            saveEmbedded(stream);
        }

        /// <summary>
        /// Saves the contents of a file attachment annotation to the specified stream.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The stream to save the data.</param>
        public void SaveEmbeddedFile(Stream stream)
        {
            saveEmbedded(stream);
        }
        
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

            IPDFObject fs = Dictionary["FS"];
            if (fs != null)
                res.AddItem("FS", fs.Clone());

            FileAttachmentAnnotation annot = new FileAttachmentAnnotation(res, owner);
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

        private void saveEmbedded(Stream stream)
        {
            try
            {
                FileSpecification fs = FileSpecification;
                PDFDictionaryStream dictStream = (PDFDictionaryStream) fs.EF["F"];
                dictStream.Decode();
                MemoryStream s = dictStream.GetStream();
                s.Position = 0;
                s.WriteTo(stream);
            }
            catch
            {
	            // ignored
            }
        }

        private void init(FileSpecification fs)
        {
            Dictionary.AddItem("Subtype", new PDFName("FileAttachment"));
            _fileSpec = fs;
            Dictionary.AddItem("FS", _fileSpec.GetDictionary());
        }
    }
}
