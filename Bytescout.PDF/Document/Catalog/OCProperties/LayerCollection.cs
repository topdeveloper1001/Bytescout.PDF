using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class LayerCollection : IEnumerable<Layer>
#else
	/// <summary>
    /// Represents a collection of layers.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    [DebuggerDisplay("Count = {Count}")]
    public class LayerCollection : IEnumerable<Layer>
#endif
	{
	    private readonly List<Layer> _layers = new List<Layer>();
	    private readonly PDFArray _array;

	    /// <summary>
	    /// Gets the number of the elements in the collection.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Count { get { return _layers.Count; } }

	    /// <summary>
	    /// Gets the element at the specified index.
	    /// </summary>
	    /// <param name="index">The zero-based index of the element to get.</param>
	    /// <returns cref="Layer">The Bytescout.PDF.Layer with the specified index.</returns>
	    public Layer this[int index]
	    {
		    get { return _layers[index]; }
	    }

	    internal LayerCollection(PDFArray arr)
        {
            _array = arr;

            for (int i = 0; i < _array.Count; ++i)
            {
                PDFDictionary dict = _array[i] as PDFDictionary;
                if (dict != null)
                    _layers.Add(Layer.Instance(dict));
            }
        }

	    /// <summary>
        /// Adds the specified action to the end of the collection.
        /// </summary>
        /// <param name="layer">Layer to be added.</param>
        public void Add(Layer layer)
        {
            if (layer == null)
                throw new NullReferenceException();
            
            _layers.Add(layer);
            _array.AddItem(layer.GetDictionary());
        }

        /// <summary>
        /// Inserts a layer at the specified index to the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index at which the outline is to be inserted.</param>
        /// <param name="layer">Layer to insert.</param>
        public void Insert(int index, Layer layer)
        {
            if (layer == null)
                throw new ArgumentNullException("page");
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            _layers.Insert(index, layer);
            _array.Insert(index, layer.GetDictionary());
        }

        /// <summary>
        /// Removes the action with the specified index from the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the action to be removed.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
            _layers.RemoveAt(index);
            _array.RemoveItem(index);
        }

        /// <summary>
        /// Clears this collection.
        /// </summary>
        public void Clear()
        {
            _layers.Clear();
            _array.Clear();
        }

		[ComVisible(false)]
	    public IEnumerator<Layer> GetEnumerator()
	    {
		    return _layers.GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }
    }
}
