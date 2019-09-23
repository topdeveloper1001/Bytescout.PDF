using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ReadOnlyCollection<T>
#else
	/// <summary>
    /// Represents a read only collection.
    /// </summary>
	[DebuggerDisplay("Count = {Count}")]
	[ComVisible(false)]
    public class ReadOnlyCollection<T>
#endif
	{
	    private readonly List<T> _items;

	    /// <summary>
	    /// Gets the element at the specified index.
	    /// </summary>
	    /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to get.</param>
	    /// <returns cref="object">Element with specified index.</returns>
	    public T this[int index]
	    {
		    get
		    {
			    return _items[index];
		    }
	    }

	    /// <summary>
	    /// Gets the number of elements contained in the collection.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Count
	    {
		    get
		    {
			    return _items.Count;
		    }
	    }

	    internal ReadOnlyCollection()
        {
            _items = new List<T> ();
        }

        internal void AddItem(T item)
        {
            _items.Add(item);
        }
    }
}
