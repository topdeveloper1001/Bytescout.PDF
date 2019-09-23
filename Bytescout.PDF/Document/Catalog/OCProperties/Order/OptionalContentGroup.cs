using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class OptionalContentGroup : OptionalContentGroupItem
#else
	/// <summary>
    /// Represents a group for optional content group.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class OptionalContentGroup : OptionalContentGroupItem
#endif
	{
	    private readonly PDFArray _array;
	    private readonly List<OptionalContentGroupItem> _items;

	    /// <summary>
	    /// Gets the number of elements contained in this collection.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Count { get { return _items.Count; } }

	    /// <summary>
	    /// Gets the Bytescout.PDF.OptionalContentGroupItemType value that specifies the type of this item.
	    /// </summary>
	    /// <value cref="OptionalContentGroupItemType"></value>
	    public override OptionalContentGroupItemType Type
	    {
		    get { return OptionalContentGroupItemType.Group; }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.OptionalContentGroup class.
        /// </summary>
        public OptionalContentGroup()
        {
            _array = new PDFArray();
            _items = new List<OptionalContentGroupItem>();
        }

        internal OptionalContentGroup(PDFArray arr)
        {
            _array = arr;
            _items = new List<OptionalContentGroupItem>();

            for (int i = 0; i < arr.Count; ++i)
            {
                IPDFObject obj = arr[i];
                if (obj is PDFString)
                {
                    _items.Add(new OptionalContentGroupLabel((obj as PDFString).GetValue()));
                }
                else if (obj is PDFDictionary)
                {
                    _items.Add(new OptionalContentGroupLayer(Layer.Instance(obj as PDFDictionary)));
                }
                else if (obj is PDFArray)
                {
                    _items.Add(new OptionalContentGroup(obj as PDFArray));
                }
            }
        }

	    /// <summary>
        /// Gets the element at the specified index in this collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to get.</param>
        /// <returns cref="OptionalContentGroupItem">Element with specified index.</returns>
        public OptionalContentGroupItem this[int index]
        {
            get { return _items[index]; }
        }

        /// <summary>
        /// Adds the element to the end of the collection.
        /// </summary>
        /// <param name="item">The element to be added.</param>
        public void Add(OptionalContentGroupItem item)
        {
            if (item == null)
                throw new ArgumentNullException();

            _items.Add(item);
            _array.AddItem(item.GetObject());
        }

        /// <summary>
        /// Inserts an element at the specified index to the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index at which the element is to be inserted.</param>
        /// <param name="item">The element to insert.</param>
        public void Insert(int index, OptionalContentGroupItem item)
        {
            if (item == null)
                throw new ArgumentNullException();
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            _items.Insert(index, item);
            _array.Insert(index, item.GetObject());
        }

        /// <summary>
        /// Removes the element with the specified index from this collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to be removed.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
            _items.RemoveAt(index);
            _array.RemoveItem(index);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            _array.Clear();
            _items.Clear();
        }

	    internal override IPDFObject GetObject()
        {
            return _array;
        }
    }
}
