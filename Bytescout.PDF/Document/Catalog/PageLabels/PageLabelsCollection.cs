using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class PageLabelsCollection : IEnumerable<PageLabel>
#else
	/// <summary>
    /// Represents the collection of page labels.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[DebuggerDisplay("Count = {Count}")]
    public class PageLabelsCollection : IEnumerable<PageLabel>
#endif
	{
	    private readonly NumberTree _numberTree;
	    private readonly List<PageLabel> _items = new List<PageLabel>();

	    /// <summary>
	    /// Gets the number of page labels in collection.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Count { get { return _items.Count; } }

	    internal PageLabelsCollection()
        {
            _numberTree = new NumberTree(new PDFDictionary());
        }

        internal PageLabelsCollection(PDFDictionary dict)
        {
            _numberTree = new NumberTree(dict);
            readKids();
        }

	    /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to get.</param>
        /// <returns cref="PageLabel">The Bytescout.PDF.PageLabel with the specified index.</returns>
        public PageLabel this[int index]
        {
            get
            {
                return _items[index];
            }
        }

        /// <summary>
        /// Adds the page label to the end of the collection.
        /// </summary>
        /// <param name="pageLabel">Page label to be added to the end of the collection.</param>
        public void Add(PageLabel pageLabel)
        {
            insert(Count, pageLabel);
        }

        /// <summary>
        /// Inserts an page label at the specified index to the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index at which the page label is to be inserted.</param>
        /// <param name="pageLabel">Page label to insert.</param>
        public void Insert(int index, PageLabel pageLabel)
        {
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();
            insert(index, pageLabel);
        }

        /// <summary>
        /// Removes the page label with the specified index from the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the page label to be removed.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            if (Count == 1)
            {
                Clear();
                return;
            }

            _numberTree.Remove(new PDFNumber(_items[index].FirstPageIndex));
            _items.RemoveAt(index);
        }

        /// <summary>
        /// Removes all page labels from the collection.
        /// </summary>
        public void Clear()
        {
            _numberTree.Clear();
            _items.Clear();
        }

        internal PDFDictionary GetDictionary()
        {
            return _numberTree.GetDictionary();
        }

        internal void Check(int pageCount)
        {
            if (_items.Count == 0)
                return;

            PageLabel pageLabel = null;
            bool startWithZeroIndex = false;

            for (int i = 0; i < Count; ++i)
            {
                if (null == pageLabel)
                    pageLabel = _items[i].Clone();

                if (pageCount <= _items[i].FirstPageIndex)
                {
                    Remove(i);
                    i--;
                }
                else
                    if (_items[i].FirstPageIndex == 0)
                        startWithZeroIndex = true;
            }

            if (!startWithZeroIndex)
            {
                pageLabel.FirstPageIndex = 0;
                Add(pageLabel);
            }
        }

        private void insert(int index, PageLabel pageLabel)
        {
            if (pageLabel == null)
                throw new ArgumentNullException("pageLabel");
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            PageLabel pageLabelCopy = pageLabel.Clone();

            _numberTree.InsertItem(new PDFNumber(pageLabelCopy.FirstPageIndex), pageLabelCopy.GetDictionary());
            _items.Insert(index, pageLabelCopy);
        }

        private void readKids()
        {
            _numberTree.GetAllLeafs(_items);
        }

		[ComVisible(false)]
	    public IEnumerator<PageLabel> GetEnumerator()
	    {
		    return _items.GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }
    }
}
