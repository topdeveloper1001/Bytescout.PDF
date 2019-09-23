using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class FieldCollection
#else
	/// <summary>
    /// Represents collection of fields.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class FieldCollection
#endif
	{
	    private readonly PDFArray _fields;
	    private readonly PDFDictionary _parent;
	    private string _key;
	    private IDocumentEssential _owner;

	    internal IDocumentEssential Owner
	    {
		    get { return _owner; }
		    set { _owner = value; }
	    }

	    internal int Count { get { return _fields.Count; } }

	    internal FieldCollection(IPDFObject obj, IDocumentEssential owner, PDFDictionary parent, string key)
	    {
		    Owner = owner;
		    _parent = parent;
		    _key = key;

		    if (obj == null)
		    {
			    _fields = new PDFArray();
			    _parent.AddItem(key, _fields);
		    }
		    else if (obj is PDFArray)
		    {
			    _fields = obj as PDFArray;
		    }
		    else
		    {
			    _fields = new PDFArray();
			    _fields.AddItem(obj);
			    _parent.AddItem(key, _fields);
		    }
	    }

	    /// <summary>
        /// Adds the specified field.
        /// </summary>
        /// <param name="field">The field item to be added.</param>
        public void Add(Field field)
        {
            if (field == null)
                throw new ArgumentNullException();
            _fields.AddItem(field.Dictionary);
        }

        /// <summary>
        /// Adds the fields with their field name.
        /// </summary>
        /// <param name="fieldName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The field name.</param>
        public void Add(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException();
            _fields.AddItem(new PDFString(fieldName));
        }

        internal void Remove(int index)
        {
            if (index >= 0 && index < Count)
                _fields.RemoveItem(index);
        }

        internal void Clear()
        {
            _fields.Clear();
        }
    }
}
