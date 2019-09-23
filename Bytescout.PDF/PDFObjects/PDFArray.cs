using System.Collections.Generic;
using System.IO;

namespace Bytescout.PDF
{
    internal class PDFArray : IPDFObject
    {
	    private readonly List<IPDFObject> _items = new List<IPDFObject>();

	    public IPDFObject this[int index]
	    {
		    get { return getItem(index); }
	    }

	    public int Count
	    {
		    get
		    {
			    return _items.Count;
		    }
	    }

	    public int ObjNo
	    {
		    get { return -1; }
		    set { }
	    }

	    public PDFArray()
        {
        }

	    public void AddItem(IPDFObject obj)
        {
            if (obj == null)
                return;
            _items.Add(obj);
        }

        public void RemoveItem(int index)
        {
            if (index < _items.Count && index >= 0)
                _items.RemoveAt(index);
        }

        public void Insert(int index, IPDFObject obj)
        {
            if (obj == null)
                return;
            _items.Insert(index, obj);
        }

	    public void Write(SaveParameters param)
        {
            Stream stream = param.Stream;

            stream.WriteByte((byte)'[');
            for (int i = 0; i < _items.Count; ++i)
            {
                if (i != 0)
                    stream.WriteByte((byte)' ');

                IPDFObject item = _items[i];
                if (item is PDFDictionary || item is PDFDictionaryStream)
                {
                    if (!param.WriteInheritableObjects)
                    {
                        StringUtility.WriteToStream(item.ObjNo, stream);
                        stream.WriteByte((byte)' ');
                        stream.WriteByte((byte)'0');
                        stream.WriteByte((byte)' ');
                        stream.WriteByte((byte)'R');
                    }
                    else
                        item.Write(param);
                }
                else
                {
                    item.Write(param);
                }
            }
            stream.WriteByte((byte)']');
        }

        public IPDFObject Clone()
        {
            PDFArray arr = new PDFArray();
            arr._items.AddRange(_items);
            return arr;
        }

        public override string ToString()
        {
            string result = "";
            result += '[';
            for (int i = 0; i < _items.Count; ++i)
            {
                if (i != 0)
                    result += ' ';
                result += _items[i].ToString();
            }
            result += ']';

            return result;
        }

        public void Collect(XRef xref)
        {
            for (int i = 0; i < Count; ++i)
            {
                IPDFObject item = getItem(i);
                item.Collect(xref);
            }
        }

        public void Clear()
        {
            _items.Clear();
        }

        private IPDFObject getItem(int index)
        {
            if (index < _items.Count && index >= 0)
            {
                IPDFObject obj = _items[index];
                if (obj is PDFLink)
                {
                    obj = (obj as PDFLink).GetValue();
                    _items.RemoveAt(index);
                    _items.Insert(index, obj);
                }
                return obj;
            }

            return null;
        }
    }
}
