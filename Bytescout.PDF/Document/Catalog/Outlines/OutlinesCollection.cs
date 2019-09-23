using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class OutlinesCollection : IEnumerable<Outline>
#else
	/// <summary>
    /// Represents the collection of outlines.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[DebuggerDisplay("Count = {Count}")]
    public class OutlinesCollection : IEnumerable<Outline>
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private readonly IDocumentEssential _owner;
		private readonly List<Outline> _items = new List<Outline>();

	    internal OutlinesCollection(IDocumentEssential owner)
        {
            _owner = owner;
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Count", new PDFNumber(0));
        }

        internal OutlinesCollection(PDFDictionary dict, IDocumentEssential owner)
        {
            _owner = owner;
            _dictionary = dict;
            readKids();
        }

        /// <summary>
        /// Gets the number of outlines in collection.
        /// </summary>
        /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
        public int Count { get { return _items.Count; } }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to get.</param>
        /// <returns cref="Outline">The Bytescout.PDF.Outline with the specified index.</returns>
        public Outline this[int index]
        {
            get
            {
                return _items[index];
            }
        }

        /// <summary>
        /// Adds the outline to the end of the collection.
        /// </summary>
        /// <param name="outline">Outline to be added to the end of the collection.</param>
        public void Add(Outline outline)
        {
            insert(Count, outline);
        }

        /// <summary>
        /// Inserts an outline at the specified index to the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index at which the outline is to be inserted.</param>
        /// <param name="outline">Outline to insert.</param>
        public void Insert(int index, Outline outline)
        {
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();
            insert(index, outline);
        }

        /// <summary>
        /// Removes the outline with the specified index from the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the outline to be removed.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            if (Count == 1)
            {
                Clear();
                return;
            }

            _items.RemoveAt(index);
            if (index == 0)
            {
                _dictionary.AddItem("First", _items[0].GetDictionary());
                _items[0].GetDictionary().RemoveItem("Prev");
            }
            else if (index == Count)
            {
                _dictionary.AddItem("Last", _items[Count - 1].GetDictionary());
                _items[Count - 1].GetDictionary().RemoveItem("Next");
            }
            else
            {
                _items[index - 1].Next = _items[index].GetDictionary();
                _items[index].Prev = _items[index - 1].GetDictionary();
            }

            addCount();
        }

        /// <summary>
        /// Removes all outlines from the collection.
        /// </summary>
        public void Clear()
        {
            GetDictionary().RemoveItem("First");
            GetDictionary().RemoveItem("Last");
            _items.Clear();
            addCount();
        }

        internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        private void insert(int index, Outline outline)
        {
            if (outline == null)
                throw new ArgumentNullException("outline");
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            Outline outlineCopy = outline.Clone(_owner);

            if (index == 0)
                _dictionary.AddItem("First", outlineCopy.GetDictionary());
            if (index == Count)
                _dictionary.AddItem("Last", outlineCopy.GetDictionary());

            if (index != Count)
            {
                outlineCopy.Next = _items[index].GetDictionary();
                _items[index].Prev = outlineCopy.GetDictionary();
            }
            if (index != 0)
            {
                outlineCopy.Prev = _items[index - 1].GetDictionary();
                _items[index - 1].Next = outlineCopy.GetDictionary();
            }

            outlineCopy.Parent = GetDictionary();

            _items.Insert(index, outlineCopy);
            addCount();
        }

        private void readKids()
        {
            PDFDictionary first = _dictionary["First"] as PDFDictionary;
            if (first != null)
            {
                Outline outline = new Outline(first, _owner);
                _items.Add(outline);

                while (outline.Next != null)
                {
                    outline = new Outline(outline.Next, _owner);
                    _items.Add(outline);
                }
            }
        }

        private void addCount()
        {
            PDFNumber count = _dictionary["Count"] as PDFNumber;
            if (count == null || count.GetValue() >= 0)
                _dictionary.AddItem("Count", new PDFNumber(Count));
        }

		[ComVisible(false)]
	    public IEnumerator<Outline> GetEnumerator()
	    {
		    return _items.GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }
    }
}
