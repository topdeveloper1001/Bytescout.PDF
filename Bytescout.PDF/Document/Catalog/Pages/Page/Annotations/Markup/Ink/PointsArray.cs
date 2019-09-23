using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
    internal delegate void ChangedPointsArrayEventHandler(object sender);

#if PDFSDK_EMBEDDED_SOURCES
	internal class PointsArray
#else
	/// <summary>
    /// Represents a collection of points.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class PointsArray
#endif
	{
	    private readonly PDFArray _array;
	    private Page _page;

	    /// <summary>
	    /// Gets or sets the element at the specified index.
	    /// </summary>
	    /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to get or set.</param>
	    /// <returns cref="PointF" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The element at the specified index.</returns>
	    public PointF this[int index]
	    {
		    get
		    {
			    if (index < 0 || index >= _array.Count / 2)
				    throw new IndexOutOfRangeException();
			    PDFNumber numberX = _array[index * 2] as PDFNumber;
			    PDFNumber numberY = _array[index * 2 + 1] as PDFNumber;
			    if (numberX == null)
				    numberX = new PDFNumber(0);
			    if (numberY == null)
				    numberY = new PDFNumber(0);
			    float numX = (float)numberX.GetValue();
			    float numY = (float)numberY.GetValue();
			    if (_page != null)
			    {
				    numX = numX - _page.PageRect.Left;
				    numY = _page.PageRect.Bottom - numY;
			    }
			    return new PointF(numX, numY);
		    }
		    set
		    {
			    if (index < 0 || index >= _array.Count / 2)
				    throw new IndexOutOfRangeException();
			    if (_page != null)
			    {
				    value.X = value.X + _page.PageRect.Left;
				    value.Y = _page.PageRect.Bottom - value.Y;
			    }
			    _array.RemoveItem(index * 2);
			    _array.RemoveItem(index * 2);
			    _array.Insert(index * 2, new PDFNumber(value.Y));
			    _array.Insert(index * 2, new PDFNumber(value.X));

			    if (ChangedPointsArray != null)
				    ChangedPointsArray(this);
		    }
	    }

	    /// <summary>
	    /// Gets the number of the elements in the collection.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Count
	    {
		    get
		    {
			    return _array.Count / 2;
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

	    internal event ChangedPointsArrayEventHandler ChangedPointsArray;

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.PointsArray class.
        /// </summary>
        public PointsArray()
        {
            _array = new PDFArray();
        }
        
        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.PointsArray class.
        /// </summary>
        /// <param name="points">The collection of points.</param>
        public PointsArray(IEnumerable<PointF> points)
        {
            if (points == null)
                throw new ArgumentNullException();
            _array = new PDFArray();
            IEnumerator<PointF> enumerator = points.GetEnumerator();
            enumerator.Reset();

            while (enumerator.MoveNext())
            {
                PointF current = enumerator.Current;
                _array.AddItem(new PDFNumber(current.X));
                _array.AddItem(new PDFNumber(current.Y));
            }
        }

        internal PointsArray(PDFArray array, Page page)
        {
            _array = array;
            _page = page;
        }

	    /// <summary>
        /// Adds a point to the end of the collection.
        /// </summary>
        public void AddPoint(float x, float y)
        {
            _array.AddItem(new PDFNumber(0));
            _array.AddItem(new PDFNumber(0));
            this[Count - 1] = new PointF(x, y);
        }
		
		/// <summary>
        /// Adds a point to the end of the collection.
        /// </summary>
        /// <param name="point" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The value to be added to the end of the collection.</param>
		[ComVisible(false)]
		public void AddPoint(PointF point)
        {
            _array.AddItem(new PDFNumber(0));
            _array.AddItem(new PDFNumber(0));
            this[Count - 1] = point;
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the collection.
        /// </summary>
        /// <param name="points">The collection of points whose elements should be added to the end of the collection.</param>
		[ComVisible(false)]
        public void AddRange(IEnumerable<PointF> points)
        {
            if (points == null)
                throw new ArgumentNullException();

            IEnumerator<PointF> enumerator = points.GetEnumerator();
            enumerator.Reset();
            while (enumerator.MoveNext())
                AddPoint(enumerator.Current);
        }

        /// <summary>
        /// Removes the element at the specified index of the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to remove.</param>
        public void RemovePoint(int index)
        {
            if (index < 0 || index >= _array.Count / 2)
                throw new IndexOutOfRangeException();
            _array.RemoveItem(index * 2);
            _array.RemoveItem(index * 2);

            if (ChangedPointsArray != null)
                ChangedPointsArray(this);
        }

        /// <summary>
        /// Inserts an element into the collection at the specified index.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index at which the item should be inserted.</param>
        /// <param name="point" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The point to insert.</param>
        public void Insert(int index, PointF point)
        {
            if (index < 0 || index >= _array.Count / 2)
                throw new IndexOutOfRangeException();
            _array.Insert(index * 2, new PDFNumber(0));
            _array.Insert(index * 2, new PDFNumber(0));

            this[index] = point;
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            _array.Clear();

            if (ChangedPointsArray != null)
                ChangedPointsArray(this);
        }

        /// <summary>
        /// Copies the elements of the collection to a new array.
        /// </summary>
        /// <returns cref="!:PointF[]" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">An array containing copies of the elements of the collection.</returns>
        public PointF[] ToArray()
        {
            PointF[] points = new PointF[Count];
            for (int i = 0; i < points.Length; ++i)
                points[i] = this[i];
            return points;
        }
    }
}
