using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class Annotation
#else
	/// <summary>
    /// Represents an abstract class for annotation objects.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public abstract class Annotation
#endif
	{
	    private PDFDictionary _dictionary;
	    private Page _page;
	    private IDocumentEssential _owner;

	    /// <summary>
        /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
        /// </summary>
        /// <value cref="AnnotationType"></value>
        public abstract AnnotationType Type { get; }

        /// <summary>
        /// Gets or sets a value indicating  whether this annotation is invisible.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool Invisible
        {
            get { return getFlagAttribute(1); }
            set { setFlagAttribute(1, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating  whether this annotation is hidden.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool Hidden
        {
            get { return getFlagAttribute(2); }
            set { setFlagAttribute(2, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating  whether this annotation gets printed when the page is printed.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool Print
        {
            get { return getFlagAttribute(3); }
            set { setFlagAttribute(3, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to scale the annotations's appearance to match the magnification of the page.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool NoZoom
        {
            get { return getFlagAttribute(4); }
            set { setFlagAttribute(4, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to rotate the annotation's appearance to match the rotation of the page.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool NoRotate
        {
            get { return getFlagAttribute(5); }
            set { setFlagAttribute(5, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display the annotation on the screen and allow it to interact with the user.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool NoView
        {
            get { return getFlagAttribute(6); }
            set { setFlagAttribute(6, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow the annotation to interact with the user.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public virtual bool ReadOnly
        {
            get { return getFlagAttribute(7); }
            set { setFlagAttribute(7, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this annotation is locked.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool Locked
        {
            get { return getFlagAttribute(8); }
            set { setFlagAttribute(8, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to invert the interpretation of the annotation property for certain events.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool ToggleNoView
        {
            get { return getFlagAttribute(9); }
            set { setFlagAttribute(9, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow the contents of the annotation to be modified by the user.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool LockedContents
        {
            get { return getFlagAttribute(10); }
            set { setFlagAttribute(10, value); }
        }

        internal float Left
        {
            get
            {
                if (_page == null)
                    return getRectValue(0);
                return getRectValue(0) - _page.PageRect.Left;
            }
            set
            {
                if (_page == null)
                    setRect(value, getRectValue(1), value + Width, getRectValue(3));
                else
                {
                    RectangleF pageRect = _page.PageRect;
                    setRect(pageRect.Left + value, getRectValue(1), Width + pageRect.Left + value, getRectValue(3));
                }
            }
        }

        internal float Top
        {
            get
            {
                if (_page == null)
                    return getRectValue(1);
                return _page.PageRect.Bottom - getRectValue(1) - Height;
            }
            set
            {
                if (_page == null)
                    setRect(getRectValue(0), value, getRectValue(2), value + Height);
                else
                {
                    RectangleF pageRect = _page.PageRect;
                    setRect(getRectValue(0), pageRect.Bottom - value - Height, getRectValue(2), pageRect.Bottom - value);
                }
            }
        }

        internal float Width
        {
            get
            {
                float result = getRectValue(2) - getRectValue(0);
                if (result > 0)
                    return result;
                return 0;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException();
                setRect(getRectValue(0), getRectValue(1), getRectValue(0) + value, getRectValue(3));
            }
        }

        internal float Height
        {
            get
            {
                float result = getRectValue(3) - getRectValue(1);
                if (result > 0)
                    return result;
                return 0;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException();
                setRect(getRectValue(0), getRectValue(1) + Height - value, getRectValue(2), getRectValue(3));
            }
        }

        internal uint Flag
        {
            get
            {
                PDFNumber f = _dictionary["F"] as PDFNumber;
                if (f == null)
                    return 0;
                return (uint)f.GetValue();
            }
            set
            {
                _dictionary.AddItem("F", new PDFNumber(value));
            }
        }

        internal PDFDictionary Dictionary 
        {
            get { return _dictionary; }
            set { _dictionary = value; }
        }

        internal Page Page
        {
            get { return _page; }
        }

	    internal IDocumentEssential Owner
	    {
		    get { return _owner; }
		    set { _owner = value; }
	    }

	    internal Annotation(IDocumentEssential owner)
	    {
		    _dictionary = new PDFDictionary();
		    _dictionary.AddItem("Type", new PDFName("Annot"));
		    Print = true;
		    Owner = owner;
	    }

	    internal Annotation(float left, float top, float width, float height, IDocumentEssential owner)
	    {
		    if (width < 0)
			    throw new ArgumentOutOfRangeException("width");
		    if (height < 0)
			    throw new ArgumentOutOfRangeException("height");

		    _dictionary = new PDFDictionary();
		    _dictionary.AddItem("Type", new PDFName("Annot"));
		    setRect(left, top, left + width, top + height);
		    Print = true;
		    Owner = owner;
	    }

	    internal Annotation(PDFDictionary dict, IDocumentEssential owner)
	    {
		    _dictionary = dict;
		    Owner = owner;
	    }

	    internal void SetPage(Page page, bool needUpdateLocation)
        {
            if (needUpdateLocation)
            {
                RectangleF pageRect = page.PageRect;
                setRect(Left + pageRect.Left, pageRect.Bottom - Top - Height,
                        Left + pageRect.Left + Width, pageRect.Bottom - Top);
            }

            _page = page;
        }

        internal abstract Annotation Clone(IDocumentEssential owner, Page page);

        internal abstract void CreateApperance();

        internal abstract void ApplyOwner(IDocumentEssential owner);

	    private float getRectValue(byte num)
        {
            PDFArray rect = _dictionary["Rect"] as PDFArray;
            if (rect == null)
                return 0;

            PDFNumber val = rect[num] as PDFNumber;
            if (val == null)
                return 0;
            return (float)val.GetValue();
        }

        private void setRect(float left, float top, float right, float bottom)
        {
            PDFArray rect = new PDFArray();
            rect.AddItem(new PDFNumber(left));
            rect.AddItem(new PDFNumber(top));
            rect.AddItem(new PDFNumber(right));
            rect.AddItem(new PDFNumber(bottom));
            _dictionary.AddItem("Rect", rect);
        }

        private bool getFlagAttribute(byte bytePosition)
        {
            return (Flag >> bytePosition - 1) % 2 != 0;
        }

        private void setFlagAttribute(byte bytePosition, bool value)
        {
            if(value)
                Flag = Flag | (uint)(1 << (bytePosition - 1));
            else
                Flag = Flag & (0xFFFFFFFF ^ (uint)(1 << (bytePosition - 1)));
        }
    }
}
