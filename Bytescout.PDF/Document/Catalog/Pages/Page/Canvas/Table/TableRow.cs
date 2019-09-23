using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class TableRow
#else
	/// <summary>
    /// Represents the table row.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class TableRow
#endif
	{
        internal TableRow(Hashtable table, bool isKeys)
        {
            if (isKeys)
            {
                foreach (object key in table.Keys)
                {
                    m_table[key] = new TableElement();
                }
            }
            else
                m_table = table;
        }

        /// <summary>
        /// Gets the element row with the specified key.
        /// </summary>
        /// <param name="key" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The key of the element to get.</param>
        /// <returns cref="TableElement">The Bytescout.PDF.TableElement with the specified key.</returns>
        /// <sq_access>read</sq_access>
        public TableElement this[string key]
        {
            get
            {
                if (!m_table.ContainsKey(key))
                    return null;
                return m_table[key] as TableElement;
            }

            set
            {
                if (m_table.ContainsKey(key))
                    m_table[key] = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of a row.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        /// <sq_access>full</sq_access>
        public float Height
        {
            get
            {
                return m_height;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
                m_height = value;
            }
        }

        /// <summary>
        /// Gets the number of elements in the row.
        /// </summary>
        /// <sq_access>read</sq_access>
        /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
        public int Count
        {
            get
            {
                return m_table.Count;
            }
        }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <sq_access>full</sq_access>
        /// <value cref="DeviceColor"></value>
        public DeviceColor BackgroundColor
        {
            get
            {
                return m_colorBackground;
            }

            set
            {
                m_colorBackground = value;
            }
        }

        internal Hashtable HashTable
        {
            get
            {
                return m_table;
            }
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float m_height = 20.0f;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private Hashtable m_table = new Hashtable();
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private DeviceColor m_colorBackground = null;
    }
}
