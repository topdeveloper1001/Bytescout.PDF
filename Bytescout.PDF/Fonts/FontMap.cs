using System;
using System.Collections.Generic;

namespace Bytescout.PDF
{
    internal class FontMap
    {
	    private ushort[] _map;
	    private readonly List<ushort> _additionalIndexes;
	    private readonly List<char> _characters;

	    public List<char> Characters { get { return _characters; } }

	    public FontMap()
	    {
		    _additionalIndexes = new List<ushort>();
		    _characters = new List<char>();
		    _additionalIndexes.Add(0);
		    _map = new ushort[255];
	    }

	    public ushort[] GetIndexes()
        {
            return _additionalIndexes.ToArray();
        }

        public ushort GetGlyfIndex(char ch)
        {
            if (ch < _map.Length)
                return _map[ch];
            return 0;
        }

        public void Add(char ch, ushort glyfIndex)
        {
            if (ch >= _map.Length)
            {
                int length = _map.Length;
                while (length <= ch)
                    length = (length + 1) * 2 - 1;

                ushort[] newMap = new ushort[length];
                Array.Copy(_map, newMap, _map.Length);
                _map = newMap;
            }

            if (_map[ch] == 0)
            {
                AddToAdditional(glyfIndex);
                _characters.Add(ch);
            }

            _map[ch] = glyfIndex;
        }

        public void AddToAdditional(ushort glyfIndex)
        {
            if (glyfIndex > _additionalIndexes[_additionalIndexes.Count - 1])
            {
                _additionalIndexes.Add(glyfIndex);
                return;
            }

            if (glyfIndex == _additionalIndexes[_additionalIndexes.Count - 1])
                return;

            for (int i = 1; i < _additionalIndexes.Count; ++i)
            {
                if (glyfIndex == _additionalIndexes[i])
                    return;

                if (glyfIndex < _additionalIndexes[i])
                {
                    _additionalIndexes.Insert(i, glyfIndex);
                    return;
                }
            }
        }

        public bool Contains(char c)
        {
            if (c < _map.Length)
                return _map[c] != 0;
            return false;
        }

        public PDFString Convert(string str)
        {
            byte[] buf = new byte[str.Length * 2];
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] < _map.Length)
                {
                    ushort val = _map[str[i]];
                    buf[i * 2] = (byte)(val / 256);
                    buf[i * 2 + 1] = (byte)(val - buf[i * 2] * 256);
                }
                else
                {
                    buf[i * 2] = 0;
                    buf[i * 2 + 1] = 0;
                }
            }
            return new PDFString(buf, false);
        }

        public string ConvertFrom(PDFString str)
        {
            byte[] data = str.GetBytes();
            char[] result = new char[data.Length];
            for (int i = 0; i < data.Length; i += 2)
            {
                ushort glyf = (ushort)(data[i] * 256 + data[i + 1]);
                for (int j = 0; j < _map.Length; ++j)
                {
                    if (_map[j] == glyf)
                    {
                        result[i] = (char)j;
                        break;
                    }
                }
            }

            return new string(result);
        }
    }
}
