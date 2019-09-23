using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
    internal delegate void ChangedInkListEventHandler(object sender);

#if PDFSDK_EMBEDDED_SOURCES
	internal class InkList
#else
	/// <summary>
    /// Represents a collection of Bytescout.PDF.PointsArray, each representing a
    /// stroked path of ink annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[DebuggerDisplay("Count = {Count}")]
    public class InkList
#endif
	{
	    private readonly PDFArray _array;
	    private readonly List<PointsArray> _listArrays;
	    private Page _page;

	    /// <summary>
	    /// Gets the element at the specified index.
	    /// </summary>
	    /// <param name="index">The zero-based index of the element to get.</param>
	    /// <returns cref="PointsArray">The element at the specified index.</returns>
	    public PointsArray this[int index]
	    {
		    get
		    {
			    return _listArrays[index];
		    }
	    }

	    /// <summary>
	    /// Gets number of the elements in the collection.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Count
	    {
		    get
		    {
			    return _array.Count;
		    }
	    }

	    internal PDFArray Array
	    {
		    get
		    {
			    return _array;
		    }
	    }

	    internal Page Page
	    {
		    get { return _page; }
		    set { _page = value; }
	    }

	    internal event ChangedInkListEventHandler ChangedInkList;

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.InkList class.
        /// </summary>
        public InkList()
        {
            _array = new PDFArray();
            _listArrays = new List<PointsArray>();
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.InkList class.
        /// </summary>
        /// <param name="arrays">An array of arrays, each representing a stroked path.</param>
        public InkList(IEnumerable<PointsArray> arrays)
        {
            if (arrays == null)
                throw new ArgumentNullException();

            _array = new PDFArray();
            _listArrays = new List<PointsArray>();

            IEnumerator<PointsArray> enumerator = arrays.GetEnumerator();
            enumerator.Reset();

            while (enumerator.MoveNext())
            {
                _array.AddItem(enumerator.Current.Array);
                _listArrays.Add(enumerator.Current);
                enumerator.Current.ChangedPointsArray += changedPointsArray;
            }
        }

        internal InkList(PDFArray array, Page page)
        {
            _array = array;
            _page = page;
            _listArrays = new List<PointsArray>();
            for (int i = 0; i < _array.Count; ++i)
            {
                PDFArray arrayI = array[i] as PDFArray ?? new PDFArray();
	            _listArrays.Add(new PointsArray(arrayI, page));
                _listArrays[i].ChangedPointsArray += changedPointsArray;
            }
        }

	    /// <summary>
        /// Adds an array of points to the end of the collection.
        /// </summary>
        /// <param name="array">The value to be added to the end of the collection.</param>
        public void AddArray(PointsArray array)
        {
            if (array == null)
                throw new ArgumentNullException();

            PointsArray newArr = new PointsArray();
            newArr.Page = _page;
            newArr.AddRange(array.ToArray());
            newArr.ChangedPointsArray += changedPointsArray;

            _array.AddItem(newArr.Array);
            _listArrays.Add(newArr);

            if (ChangedInkList != null)
                ChangedInkList(this);
        }

        /// <summary>
        /// Removes the element at the specified index of the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to remove.</param>
        public void RemoveArray(int index)
        {
            if (index < 0 || index >= _array.Count)
                throw new IndexOutOfRangeException();

            _listArrays[index].ChangedPointsArray -= changedPointsArray;
            _array.RemoveItem(index);
            _listArrays.RemoveAt(index);

            if (ChangedInkList != null)
                ChangedInkList(this);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            int count = Count;
            for (int i = 0; i < count; ++i)
                RemoveArray(0);
        }

	    private void changedPointsArray(object sender)
        {
            if (ChangedInkList != null)
                ChangedInkList(this);
        }
    }
}
