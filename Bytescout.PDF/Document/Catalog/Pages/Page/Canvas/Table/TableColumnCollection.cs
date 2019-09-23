using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class TableColumnCollection : IEnumerable<TableColumn>
#else
	/// <summary>
    /// Represents a collection of columns of a table.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class TableColumnCollection : IEnumerable<TableColumn>
#endif
	{
	    private readonly List<TableColumn> _columns = new List<TableColumn>();
	    private readonly Hashtable _table = new Hashtable();
	    private readonly Table _tableHead;

	    /// <summary>
	    /// Gets the element at the specified index.
	    /// </summary>
	    /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to get.</param>
	    /// <returns cref="TableColumn">The Bytescout.PDF.TableColumn with the specified index.</returns>
	    public TableColumn this[int index]
	    {
		    get
		    {
			    if (index < 0 || index >= Count)
				    throw new IndexOutOfRangeException();
			    return _columns[index];
		    }
	    }

	    /// <summary>
	    /// Gets the number of elements in the collection.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Count
	    {
		    get
		    {
			    return _columns.Count;
		    }
	    }

	    internal Hashtable HashTable
	    {
		    get
		    {
			    return _table;
		    }
	    }

	    internal TableColumnCollection(Table table)
        {
            _tableHead = table;
        }

	    /// <summary>
        /// Adds the column to the end of the collection.
        /// </summary>
        /// <param name="column">Column to be added to the end of the collection.</param>
        public void Add(TableColumn column)
        {
            if (_table.Contains(column.ColumnName))
                return;
            _columns.Add(column);
            for (int i = 0; i < _tableHead.Rows.Count; ++i)
            {
                _tableHead.Rows[i].HashTable.Add(column.ColumnName, new TableElement());
            }
            _table[column.ColumnName] = "";
        }

        /// <summary>
        /// Removes the element with the specified index from the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to be removed.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
            for (int i = 0; i < _tableHead.Rows.Count; ++i)
            {
                _tableHead.Rows[i].HashTable.Remove(_columns[index].ColumnName);
            }
            _table.Remove(_columns[index].ColumnName);
            _columns.RemoveAt(index);
        }

		[ComVisible(false)]
	    public IEnumerator<TableColumn> GetEnumerator()
	    {
		    return _columns.GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }
    }
}
