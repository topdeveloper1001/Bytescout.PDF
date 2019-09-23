using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Outline
#else
	/// <summary>
    /// Represents a PDF outline item (bookmark).
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class Outline
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private readonly IDocumentEssential _owner;
	    private OutlinesCollection _kids;
	    private Destination _dest;
	    private Action _action;

	    /// <summary>
	    /// Gets or sets the outline title.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string Title
	    {
		    get
		    {
			    PDFString title = _dictionary["Title"] as PDFString;
			    if (title == null)
				    return "";
			    return title.GetValue();
		    }
		    set
		    {
			    if (value == null)
				    value = "";
			    _dictionary.AddItem("Title", new PDFString(value));
		    }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether this Bytescout.PDF.Outline is open.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool HideChildren
	    {
		    get
		    {
			    PDFNumber count = _dictionary["Count"] as PDFNumber;
			    if (count == null || count.GetValue() < 0)
				    return false;
			    return true;
		    }
		    set
		    {
			    if(value)
				    _dictionary.AddItem("Count", new PDFNumber(-1));
			    else
				    _dictionary.AddItem("Count", new PDFNumber(_kids.Count));
		    }
	    }

	    /// <summary>
	    /// Gets the collection of the children.
	    /// </summary>
	    /// <value cref="OutlinesCollection"></value>
	    public OutlinesCollection Kids
	    {
		    get
		    {
			    if (_kids == null)
				    _kids = new OutlinesCollection(_dictionary, _owner);
			    return _kids;
		    }
	    }

	    /// <summary>
	    /// Gets or sets the outline color.
	    /// </summary>
	    /// <value cref="ColorRGB"></value>
	    public ColorRGB Color
	    {
		    get
		    {
			    PDFArray color = _dictionary["C"] as PDFArray;
			    if (color == null)
				    return new ColorRGB(0, 0, 0);

			    PDFNumber red = color[0] as PDFNumber;
			    PDFNumber green = color[1] as PDFNumber;
			    PDFNumber blue = color[2] as PDFNumber;

			    byte r = 0, g = 0, b = 0;
			    if (red != null)
				    r = (byte)(red.GetValue() * 255);
			    if (green != null)
				    g = (byte)(green.GetValue() * 255);
			    if (blue != null)
				    b = (byte)(blue.GetValue() * 255);

			    return new ColorRGB(r, g, b);
		    }
		    set
		    {
			    if (value == null)
			    {
				    _dictionary.RemoveItem("C");
				    return;
			    }

			    PDFArray color = new PDFArray();
			    color.AddItem(new PDFNumber(value.R * 1.0f / 255));
			    color.AddItem(new PDFNumber(value.G * 1.0f / 255));
			    color.AddItem(new PDFNumber(value.B * 1.0f / 255));
			    _dictionary.AddItem("C", color);
		    }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether the title is italic.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Italic
	    {
		    get { return getFlagAttribute(1); }
		    set { setFlagAttribute(1, value); }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether the title is bold.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Bold
	    {
		    get { return getFlagAttribute(2); }
		    set { setFlagAttribute(2, value); }
	    }

	    /// <summary>
	    /// Gets or sets the outline destination.
	    /// <remarks>If this value is set, the Action property will be ignored.</remarks>
	    /// </summary>
	    /// <value cref="PDF.Destination"></value>
	    public Destination Destination
	    {
		    get { return _dest; }
		    set { setDestination(value); }
	    }

	    /// <summary>
	    /// Gets or sets the action for this outline.
	    /// <remarks>If this value is set, the Destination property will be ignored.</remarks>
	    /// </summary>
	    /// <value cref="PDF.Action"></value>
	    public Action Action
	    {
		    get { return _action; }
		    set { setAction(value); }
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

	    internal PDFDictionary Parent
	    {
		    get { return _dictionary["Parent"] as PDFDictionary; }
		    set { _dictionary.AddItem("Parent", value); }
	    }

	    internal PDFDictionary Prev
	    {
		    get { return _dictionary["Prev"] as PDFDictionary; }
		    set { _dictionary.AddItem("Prev", value); }
	    }

	    internal PDFDictionary Next
	    {
		    get { return _dictionary["Next"] as PDFDictionary; }
		    set { _dictionary.AddItem("Next", value); }
	    }

	    internal PDFDictionary First
	    {
		    get { return _dictionary["First"] as PDFDictionary; }
		    set { _dictionary.AddItem("First", value); }
	    }

	    internal PDFDictionary Last
	    {
		    get { return _dictionary["Last"] as PDFDictionary; }
		    set { _dictionary.AddItem("Last", value); }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Outline.
        /// </summary>
        /// <param name="title" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The title of the outline.</param>
        public Outline(string title)
        {
            if (title == null)
                title = "";
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Title", new PDFString(title));
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Outline.
        /// </summary>
        /// <param name="title" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The title of the outline.</param>
        /// <param name="dest">The destination to be displayed when this item is activated.</param>
        public Outline(string title, Destination dest)
        {
            if (dest == null)
                throw new ArgumentNullException("dest");
            if (title == null)
                title = "";
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Title", new PDFString(title));
            Destination = dest;
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Outline.
        /// </summary>
        /// <param name="title" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The title of the outline.</param>
        /// <param name="action">The action to be performed when this item is activated.</param>
        public Outline(string title, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            if (title == null)
                title = "";
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Title", new PDFString(title));
            Action = action;
        }

        internal Outline(PDFDictionary dict, IDocumentEssential owner)
        {
            _owner = owner;
            _dictionary = dict;
            loadDestination();
            loadAction();
        }

	    /// <summary>
        /// Returns a System.String that represents this instance.
        /// </summary>
        /// <returns cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A System.String that represents this instance.</returns>
        public override string ToString()
        {
            return Title;
        }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        internal Outline Clone(IDocumentEssential owner)
        {
            Outline outline = new Outline(Copy(GetDictionary()), owner);
            outline.Destination = Destination;
            outline.Action = Action;

            for (int i = 0; i < Kids.Count; ++i)
                outline.Kids.Add(Kids[i]);

            return outline;
        }

        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary newDict = new PDFDictionary();
            PDFName type = dict["Type"] as PDFName;
            if (type != null)
                newDict.AddItem("Type", type.Clone());

            PDFNumber count = dict["Count"] as PDFNumber;
            if (count != null)
                newDict.AddItem("Count", count.Clone());

            PDFString title = dict["Title"] as PDFString;
            if (title != null)
                newDict.AddItem("Title", title.Clone());

            PDFArray c = dict["C"] as PDFArray;
            if (c != null)
                newDict.AddItem("C", c.Clone());

            PDFNumber f = dict["F"] as PDFNumber;
            if (f != null)
                newDict.AddItem("F", f.Clone());

            //First, Last, Parent, Prev, Next, SE - do not
            //Dest, A - need set after adding

            return newDict;
        }

        private bool getFlagAttribute(byte bytePosition)
        {
            return (Flag >> bytePosition - 1) % 2 != 0;
        }

        private void setFlagAttribute(byte bytePosition, bool value)
        {
            if (value)
                Flag = Flag | (uint)(1 << (bytePosition - 1));
            else
                Flag = Flag & (0xFFFFFFFF ^ (uint)(1 << (bytePosition - 1)));
        }

        private void setDestination(Destination dest)
        {
            if (dest == null)
            {
                GetDictionary().RemoveItem("Dest");
                return;
            }
            
            _dest = dest;
            Action = null;
            GetDictionary().AddItem("Dest", dest.GetArray());
        }

        private void setAction(Action action)
        {
            if (action == null)
            {
                _action = null;
                GetDictionary().RemoveItem("A");
                return;
            }

            _action = action.Clone(_owner);
            Destination = null;
            _dictionary.AddItem("A", _action.GetDictionary());
        }

        private void loadDestination()
        {
            IPDFObject dest = _dictionary["Dest"];
            if (dest == null)
                return;

            _dest = new Destination(dest, _owner);
        }

        private void loadAction()
        {
            PDFDictionary dict = _dictionary["A"] as PDFDictionary;
            if (dict == null)
                return;
            try
            {
                _action = Action.Create(dict, _owner);
            }
            catch(PDFException)
            {
                GetDictionary().RemoveItem("A");
            }
        }
    }
}
