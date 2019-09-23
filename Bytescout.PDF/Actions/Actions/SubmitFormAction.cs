using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class SubmitFormAction : Action
#else
	/// <summary>
    /// Represents the PDF form's submit action.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class SubmitFormAction : Action
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private FieldCollection _fields;
	    private FileSpecification _fileSpec;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.SubmitForm; } }

	    /// <summary>
	    /// Gets the fields.
	    /// </summary>
	    /// <value cref="FieldCollection"></value>
	    public FieldCollection Fields
	    {
		    get
		    {
			    if (_fields == null)
				    _fields = new FieldCollection(_dictionary["Fields"], Owner, _dictionary, "Fields");
			    return _fields;
		    }
	    }

	    /// <summary>
	    /// Gets the uniform resource identifier (URI) of the script at the Web server that will process the submission.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string URL
	    {
		    get
		    {
			    if (_fileSpec == null)
			    {
				    PDFDictionary f = _dictionary["F"] as PDFDictionary;
				    if (f == null)
					    return "";
				    _fileSpec = new FileSpecification(f);
			    }
			    return _fileSpec.FileName;
		    }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether fields contained in the Fields collection will be included for submitting.
	    /// <remarks>If Include property is true, only the fields in this collection will be submitted. If Include property is false,
	    /// the fields in this collection are not submitted and only the remaining form fields are submitted.  If the collection is empty,
	    /// then all the form fields are reset and the Include property is ignored. If the field has Export property set to false, it will
	    /// be not included for submitting in any case.</remarks>
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Include
	    {
		    get { return getFlagAttribute(1); }
		    set { setFlagAttribute(1, value); }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether to submit fields without a value.
	    /// <remarks>If set true, all fields designated by the Fields collection and the Bytescout.PDF.SubmitFormAction are submitted.
	    /// Include flags are submitted, regardless of whether they have a value. For fields without a value, only the field name is transmitted.</remarks>
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool IncludeNoValueFields
	    {
		    get { return getFlagAttribute(2); }
		    set { setFlagAttribute(2, value); }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether to submit mouse pointer coordinates. If set true, the coordinates
	    /// of the mouse click that caused the submit-form action are transmitted as part of the form data. The
	    /// coordinate values are relative to the upper-left corner of the field’s widget annotation rectangle.
	    /// <remarks>Meaningful only when the SubmitFormat is HTML.</remarks>
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool SubmitCoordinates
	    {
		    get { return getFlagAttribute(5); }
		    set { setFlagAttribute(5, value); }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether to submit the form's incremental updates.
	    /// If set true, the submitted FDF file includes the contents of all incremental updates to the underlying PDF document.
	    /// <remarks>Meaningful only when the SubmitFormat is FDF.</remarks>
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool IncludeIncrementalUpdates
	    {
		    get { return getFlagAttribute(7); }
		    set { setFlagAttribute(7, value); }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether to submit annotations.
	    /// If set true, the submitted FDF file includes all markup annotations in the underlying PDF document.
	    /// <remarks>Meaningful only when the SubmitFormat is FDF.</remarks>
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool IncludeAnnotations
	    {
		    get { return getFlagAttribute(8); }
		    set { setFlagAttribute(8, value); }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether date and time have canonical format.
	    /// <remarks>If set true, any submitted field values representing dates are converted to the standard format.
	    /// The interpretation of a form field as a date is not specified explicitly in the field itself but only
	    /// in the JavaScript code that processes it.</remarks>
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool CanonicalDateTimeFormat
	    {
		    get { return getFlagAttribute(10); }
		    set { setFlagAttribute(10, value); }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether to exclude non-user annotations from the submitted data stream.
	    /// <remarks>Meaningful only when the SubmitFormat is FDF.</remarks>
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool ExcludeNonUserAnnotations
	    {
		    get { return getFlagAttribute(11); }
		    set { setFlagAttribute(11, value); }
	    }

	    /// <summary>
	    /// Gets or sets the submit method.
	    /// </summary>
	    /// <value cref="PDF.SubmitMethod"></value>
	    public SubmitMethod SubmitMethod
	    {
		    get
		    {
			    if (getFlagAttribute(4))
				    return SubmitMethod.Get;
			    return SubmitMethod.Post;
		    }
		    set
		    {
			    if (value == SubmitMethod.Get)
				    setFlagAttribute(4, true);
			    else
				    setFlagAttribute(4, false);
		    }
	    }

	    /// <summary>
	    /// Gets or sets the submit format.
	    /// </summary>
	    /// <value cref="SubmitDataFormat"></value>
	    public SubmitDataFormat SubmitFormat
	    {
		    get
		    {
			    if (getFlagAttribute(9))
				    return SubmitDataFormat.PDF;
			    else if (getFlagAttribute(6))
				    return SubmitDataFormat.XFDF;

			    if (getFlagAttribute(3))
				    return SubmitDataFormat.HTML;
			    else
				    return SubmitDataFormat.FDF;
		    }
		    set
		    {
			    switch (value)
			    {
				    case SubmitDataFormat.FDF:
					    setFlagAttribute(3, false);
					    setFlagAttribute(6, false);
					    setFlagAttribute(9, false);
					    break;
				    case SubmitDataFormat.HTML:
					    setFlagAttribute(3, true);
					    setFlagAttribute(6, false);
					    setFlagAttribute(9, false);
					    break;
				    case SubmitDataFormat.PDF:
					    setFlagAttribute(3, false);
					    setFlagAttribute(6, false);
					    setFlagAttribute(9, true);
					    break;
				    case SubmitDataFormat.XFDF:
					    setFlagAttribute(3, false);
					    setFlagAttribute(6, true);
					    setFlagAttribute(9, false);
					    break;
			    }
		    }
	    }

	    internal uint Flags
	    {
		    get
		    {
			    PDFNumber f = _dictionary["Flags"] as PDFNumber;
			    if (f == null)
				    return 0;
			    return (uint)f.GetValue();
		    }
		    set
		    {
			    _dictionary.AddItem("Flags", new PDFNumber(value));
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.SubmitFormAction.
        /// </summary>
        /// <param name="uri" href="http://msdn.microsoft.com/en-us/library/system.uri.aspx">The uniform resource identifier.</param>
        public SubmitFormAction(Uri uri)
            : base(null)
        {
            if (uri == null)
                throw new ArgumentNullException();

            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("SubmitForm"));

            _fileSpec = new URLFileSpecification(uri);
            _dictionary.AddItem("F", _fileSpec.GetDictionary());
        }

        internal SubmitFormAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("SubmitForm"));

            IPDFObject fs = _dictionary["F"];
            if (fs != null)
                dict.AddItem("F", fs);

            string[] keys = { "Fields", "Flags" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = _dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            SubmitFormAction action = new SubmitFormAction(dict, owner);

            IPDFObject next = _dictionary["Next"];
            if (next != null)
            {
                for (int i = 0; i < Next.Count; ++i)
                    action.Next.Add(Next[i]);
            }

            return action;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            base.ApplyOwner(owner);
            if (_fields != null)
                _fields.Owner = owner;
        }

        internal override PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        private bool getFlagAttribute(byte bytePosition)
        {
            return (Flags >> bytePosition - 1) % 2 != 0;
        }

        private void setFlagAttribute(byte bytePosition, bool value)
        {
            if (value)
                Flags = Flags | (uint)(1 << (bytePosition - 1));
            else
                Flags = Flags & (0xFFFFFFFF ^ (uint)(1 << (bytePosition - 1)));
        }
    }
}
