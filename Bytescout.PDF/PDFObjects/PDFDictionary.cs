using System.Collections.Generic;
using System.IO;

namespace Bytescout.PDF
{
    internal class PDFDictionary : IPDFObject
    {
	    private readonly List<KeyValuePair<string, IPDFObject>> _items = new List<KeyValuePair<string, IPDFObject>>();
	    private object _tag;
	    private int _objNo = -1;
	    public int Count { get { return _items.Count; } }

        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public int ObjNo
        {
            get { return _objNo; }
            set { _objNo = value; }
        }

        public void AddItem(string key, IPDFObject value)
        {
            if (value == null)
                return;

            int count = _items.Count;
            for (int i = 0; i < count; ++i)
            {
                KeyValuePair<string, IPDFObject> pair = _items[i];
                if (pair.Key == key)
                {
                    _items[i] = new KeyValuePair<string, IPDFObject>(pair.Key, value);
                    return;
                }
            }
            _items.Add(new KeyValuePair<string, IPDFObject>(key, value));
        }

        public void AddItemIfNotHave(string key, IPDFObject value)
        {
            if (value == null)
                return;

            int count = _items.Count;
            for (int i = 0; i < count; ++i)
            {
                KeyValuePair<string, IPDFObject> pair = _items[i];
                if (pair.Key == key)
                    return;
            }
            _items.Add(new KeyValuePair<string, IPDFObject>(key, value));
        }

        public void AddRange(PDFDictionary dict)
        {
            List<KeyValuePair<string, IPDFObject>> list = dict._items;
            int count = list.Count;
            for (int i = 0; i < count; ++i)
            {
                KeyValuePair<string, IPDFObject> pair = list[i];
                AddItemIfNotHave(pair.Key, pair.Value);
            }
        }

        public void RemoveItem(string key)
        {
            int count = _items.Count;
            for (int i = 0; i < count; ++i)
            {
                KeyValuePair<string, IPDFObject> pair = _items[i];
                if (pair.Key == key)
                {
                    _items.RemoveAt(i);
                    return;
                }
            }
        }

        public IPDFObject this[string key]
        {
            get { return getItem(key); }
        }

        public bool Contains(IPDFObject obj, out string key)
        {
            int count = _items.Count;
            for (int i = 0; i < count; ++i)
            {
                if (_items[i].Value == obj)
                {
                    key = _items[i].Key;
                    return true;
                }
            }
            key = "";
            return false;
        }

        public string[] GetKeys()
        {
            string[] keys = new string[_items.Count];
            for (int i = 0; i < keys.Length; ++i)
                keys[i] = _items[i].Key;
            return keys;
        }

        public void Write(SaveParameters param)
        {
            Stream stream = param.Stream;

            stream.WriteByte((byte)'<');
            stream.WriteByte((byte)'<');

            int count = _items.Count;
            for (int i = 0; i < count; ++i)
            {
                KeyValuePair<string, IPDFObject> pair = _items[i];
                PDFName.Write(stream, pair.Key);

                stream.WriteByte((byte)' ');

                IPDFObject val = pair.Value;
                if (val is PDFDictionary || val is PDFDictionaryStream)
                {
                    if (!param.WriteInheritableObjects)
                    {
                        StringUtility.WriteToStream(val.ObjNo, stream);
                        stream.WriteByte((byte)' ');
                        stream.WriteByte((byte)'0');
                        stream.WriteByte((byte)' ');
                        stream.WriteByte((byte)'R');
                    }
                    else
                        val.Write(param);
                }
                else
                {
                    val.Write(param);
                }

                if (i != count - 1)
                    stream.WriteByte((byte)'\n');
            }

            stream.WriteByte((byte)'>');
            stream.WriteByte((byte)'>');
        }

        public void Clear()
        {
            _items.Clear();
        }

        public IPDFObject Clone()
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddRange(this);
            return dict;
        }

        public override string ToString()
        {
            return "{Dictionary}";
        }

        public void Collect(XRef xref)
        {
            if (ObjNo != -1)
                return;

            Entry entry = new Entry(0, 0);
            entry.Object = this;
            xref.Entries.Add(entry);
            ObjNo = xref.Entries.Count - 1;

            int count = _items.Count;
            for (int i = 0; i < count; ++i)
            {
                KeyValuePair<string, IPDFObject> keyval = _items[i];
                if (keyval.Value is PDFLink)
                {
                    IPDFObject value = (keyval.Value as PDFLink).GetValue();
                    if (value != null)
                        _items[i] = new KeyValuePair<string, IPDFObject>(keyval.Key, value);
                    //_items[i] = new KeyValuePair<string, IPDFObject>(keyval.Key, (keyval.Value as PDFLink).GetValue());
                }
                _items[i].Value.Collect(xref);
            }
        }

        private IPDFObject getItem(string key)
        {
            int count = _items.Count;
            for (int i = 0; i < count; ++i)
            {
                if (_items[i].Key == key)
                {
                    IPDFObject obj = _items[i].Value;
                    if (obj is PDFLink)
                        _items[i] = new KeyValuePair<string,IPDFObject>(key, (obj as PDFLink).GetValue());
                    return _items[i].Value;
                }
            }
            return null;
        }
    }
}
