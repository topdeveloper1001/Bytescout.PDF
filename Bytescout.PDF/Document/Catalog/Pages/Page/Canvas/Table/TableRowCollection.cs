using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class TableRowCollection : IEnumerable<TableRow>
#else
	/// <summary>
    /// Represents a collection of rows of a table.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class TableRowCollection : IEnumerable<TableRow>
#endif
	{
		private readonly List<TableRow> _rows = new List<TableRow>();

        internal TableRowCollection()
        {
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to get.</param>
        /// <returns cref="TableRow">The Bytescout.PDF.TableRow with the specified index.</returns>
        /// <sq_access>read</sq_access>
        public TableRow this[int index]
        { 
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();
                return _rows[index];
            }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <sq_access>read</sq_access>
        /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
        public int Count
        {
            get
            {
                return _rows.Count;
            }
        }

        /// <summary>
        /// Adds the row to the end of the collection.
        /// </summary>
        /// <param name="row">Row to be added to the end of the collection.</param>
        public void Add(TableRow row)
        {
            _rows.Add(row);
        }

        /// <summary>
        /// Removes the element with the specified index from the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to be removed.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
            
            _rows.RemoveAt(index);
        }

		[ComVisible(false)]
	    public IEnumerator<TableRow> GetEnumerator()
	    {
		    return _rows.GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }
    }
}
