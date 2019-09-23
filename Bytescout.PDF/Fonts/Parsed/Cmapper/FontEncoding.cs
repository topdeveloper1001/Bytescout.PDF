using System.Collections.Generic;

namespace Bytescout.PDF
{
    internal class FontEncoding
    {
	    private readonly List<char> _charset = new List<char>(256);

	    internal FontEncoding(IPDFObject encoding)
        {
            if (encoding is PDFDictionary)
            {
                initBaseEncoding((encoding as PDFDictionary)["BaseEncoding"] as PDFName);
                PDFArray differences = (encoding as PDFDictionary)["Differences"] as PDFArray;

                if (differences != null)
                {
                    int current = 0;
                    for (int i = 0; i < differences.Count; ++i)
                    {
                        if (differences[i] is PDFNumber)
                        {
                            current = (int)(differences[i] as PDFNumber).GetValue();
                        }
                        else
                        {
                            if (differences[i] is PDFName)
                            {
                                string glyfName = (differences[i] as PDFName).GetValue();
                                if (current < _charset.Count)
                                    _charset[current] = GlyfNames.GetChar(glyfName);
                            }

                            current++;
                        }
                    }
                }
            }
            else
                initBaseEncoding(encoding as PDFName);
        }

        internal char GetChar(byte glyf)
        {
            return _charset[glyf];
        }

        internal byte GetGlyf(char c)
        {
            int index = _charset.IndexOf(c);
            if (index >= 0)
                return (byte)index;
            return 0;
        }

        private void initBaseEncoding(PDFName baseEncoding)
        {
            if (baseEncoding == null || baseEncoding.GetValue() == "StandardEncoding")
            {
                for (int i = 0; i < 256; ++i)
                    _charset.Add(Encoding.GetChar((byte)i));
                return;
            }
            else if (baseEncoding.GetValue() == "WinAnsiEncoding")
            {
                for (int i = 0; i < 256; ++i)
                    _charset.Add(WinAnsiEncoding.GetChar((byte)i));
                return;
            }
            else if (baseEncoding.GetValue() == "MacRomanEncoding")
            {
                for (int i = 0; i < 256; ++i)
                    _charset.Add(MacRomanEncoding.GetChar((byte)i));
                return;
            }

            for (int i = 0; i < 256; ++i)
                _charset.Add(Encoding.GetChar((byte)i));
        }
    }
}
