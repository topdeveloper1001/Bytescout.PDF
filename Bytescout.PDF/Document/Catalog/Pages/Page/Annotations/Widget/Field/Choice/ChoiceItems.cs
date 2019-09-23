using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ChoiceItems
#else
	/// <summary>
    /// Represents a collection of Bytescout.PDF.ChoiceField items.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class ChoiceItems
#endif
	{
	    private readonly PDFArray _items;

	    /// <summary>
	    /// Gets the element at the specified index.
	    /// </summary>
	    /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to get.</param>
	    /// <returns cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The System.String with specified index.</returns>
	    public string this[int index]
	    {
		    get
		    {
			    if (index < 0 || index >= Count)
				    throw new IndexOutOfRangeException();

			    PDFString item = _items[index] as PDFString;
			    if (item == null)
				    return "";
			    return item.GetValue();
		    }
	    }

	    /// <summary>
	    /// Gets the number of the elements in the collection.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Count
	    {
		    get { return _items.Count; }
	    }

	    internal event AddedChoiceItemEventHandler AddedChoiceItem;
	    internal event ChangedChoiceItemsEventHandler ChangedChoiceItems;

	    internal ChoiceItems(PDFArray arr)
        {
            _items = arr;
        }

	    /// <summary>
        /// Adds an object to the end of the collection.
        /// </summary>
        /// <param name="value" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The object to be added to the end of the collection.</param>
        public void Add(string value)
        {
            if (value == null)
                value = "";

            _items.AddItem(new PDFString(value));
            if (AddedChoiceItem != null)
                AddedChoiceItem(this, new AddedChoiceItemEvent(value));
            if (ChangedChoiceItems != null)
                ChangedChoiceItems(this);
        }

        /// <summary>
        /// Inserts an element into the collection at the specified index.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index at which item should be inserted.</param>
        /// <param name="value" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The object to insert.</param>
        public void Insert(int index, string value)
        {
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();
            if (value == null)
                value = "";

            _items.Insert(index, new PDFString(value));

            if (AddedChoiceItem != null)
                AddedChoiceItem(this, new AddedChoiceItemEvent(value));

            if (ChangedChoiceItems != null)
                ChangedChoiceItems(this);
        }

        /// <summary>
        /// Removes the element at the specified index of the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to remove.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
            _items.RemoveItem(index);

            if (ChangedChoiceItems != null)
                ChangedChoiceItems(this);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The object to remove from the collection</param>
        public void Remove(string item)
        {
            for (int i = 0; i < _items.Count; ++i)
            {
                if (this[i] == item)
                {
                    Remove(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            if (ChangedChoiceItems != null)
                ChangedChoiceItems(this);
        }

        /// <summary>
        /// Copies the elements of the collection to a new array.
        /// </summary>
        /// <returns cref="!:string[]" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">An array containing copies of the elements of the collection.</returns>
        public string[] ToArray()
        {
            string[] result = new string[Count];
            for (int i = 0; i < Count; ++i)
                result[i] = this[i];

            return result;
        }
    }
}
