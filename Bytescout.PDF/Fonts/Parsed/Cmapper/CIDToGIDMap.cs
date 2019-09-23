namespace Bytescout.PDF
{
    internal class CIDToGIDMap
    {
	    private bool _isIdentity = true;

	    internal CIDToGIDMap(IPDFObject stream)
        {
            if (stream is PDFDictionaryStream)
                _isIdentity = false;
        }

        internal ushort GetGlyf(char c)
        {
            if (_isIdentity)
                return 0;
            return c;
        }

        internal char GetChar(ushort glyf)
        {
            if (_isIdentity)
                return '\0';
            return (char)glyf;
        }
    }
}
