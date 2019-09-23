using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ActionCollection : IEnumerable<Action>
#else
    /// <summary>
    /// Represents a collection of actions.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    [DebuggerDisplay("Count = {Count}")]
    public class ActionCollection : IEnumerable<Action>
#endif
    {
	    private readonly List<Action> _actions = new List<Action>();
	    private readonly PDFDictionary _parent;
	    private readonly string _key;
	    private readonly IDocumentEssential _owner;

	    /// <summary>
	    /// Gets the element at the specified index.
	    /// </summary>
	    /// <param name="index">The zero-based index of the element to get.</param>
	    /// <returns cref="Action">The Bytescout.PDF.PDFAction with the specified index.</returns>
	    public Action this[int index]
	    {
		    get { return _actions[index]; }
	    }

	    /// <summary>
        /// Gets the number of the elements in the collection.
        /// </summary>
        /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
        public int Count { get { return _actions.Count; } }

	    internal ActionCollection(IPDFObject obj, IDocumentEssential owner, PDFDictionary parent, string key)
	    {
		    _owner = owner;
		    _parent = parent;
		    _key = key;
		    if (obj == null)
			    return;

		    if (obj is PDFDictionary)
			    _actions.Add(Action.Create(obj as PDFDictionary, owner));
		    else if (obj is PDFArray)
		    {
			    PDFArray arr = obj as PDFArray;
			    for (int i = 0; i < arr.Count; ++i)
				    _actions.Add(Action.Create(arr[i] as PDFDictionary, owner));
		    }
	    }

	    /// <summary>
        /// Adds the specified action to the end of the collection.
        /// </summary>
        /// <param name="action">Action to be added.</param>
        public void Add(Action action)
        {
            if (action == null)
                throw new ArgumentNullException();

            IPDFObject obj = _parent[_key];
            if (obj == null || obj is PDFDictionary)
                _parent.AddItem(_key, new PDFArray());

            PDFArray arr = _parent[_key] as PDFArray;
            if (obj is PDFDictionary)
                arr.AddItem(obj);

            Action copy = action.Clone(_owner);
            _actions.Add(copy);
            arr.AddItem(copy.GetDictionary());
        }

        /// <summary>
        /// Removes the action with the specified index from the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the action to be removed.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            _actions.RemoveAt(index);
            if (_actions.Count == 0)
            {
                _parent.RemoveItem(_key);
            }
            else
            {
                PDFArray arr = _parent[_key] as PDFArray;
                arr.RemoveItem(index);
            }
        }

        /// <summary>
        /// Clears this collection.
        /// </summary>
        public void Clear()
        {
            _actions.Clear();
            _parent.RemoveItem(_key);
        }

		[ComVisible(false)]
	    public IEnumerator<Action> GetEnumerator()
	    {
		    return _actions.GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }
    }
}
