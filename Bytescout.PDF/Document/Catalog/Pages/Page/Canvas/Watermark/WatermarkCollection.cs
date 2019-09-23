using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class WatermarkCollection : ICollection<Watermark>
#else
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[DebuggerDisplay("Count = {Count}")]
	public class WatermarkCollection : ICollection<Watermark>
#endif
	{
		private readonly List<Watermark> _watermarks = new List<Watermark>();


		/// <summary>
		/// Gets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get.</param>
		/// <returns cref="Annotation">The Bytescout.PDF.Watermark with specified index.</returns>
		public Watermark this[int index]
		{
			get
			{
				return _watermarks[index];
			}
		}

		/// <summary>
		/// Gets the number of the elements in the collection.
		/// </summary>
		/// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
		public int Count
		{
			get { return _watermarks.Count; }
		}

		public bool IsReadOnly 
		{
			get { return false; }
		}

		public void Add(Watermark item)
		{
			_watermarks.Add(item);
		}

		public void Clear()
		{
			_watermarks.Clear();
		}

		public bool Contains(Watermark item)
		{
			return _watermarks.Contains(item);
		}

		public void CopyTo(Watermark[] array, int arrayIndex)
		{
			_watermarks.CopyTo(array, arrayIndex);
		}

		public bool Remove(Watermark item)
		{
			return _watermarks.Remove(item);
		}

		[ComVisible(false)]
		public IEnumerator<Watermark> GetEnumerator()
		{
			return _watermarks.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
