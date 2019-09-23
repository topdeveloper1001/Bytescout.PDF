namespace Bytescout.PDF
{
    internal struct CCITTFaxParameters
    {
	    private readonly int _k;
	    private readonly bool _endOfLine;
	    private readonly bool _encodedByteAlign;
	    private readonly int _columns;
	    private readonly int _rows;
	    private readonly bool _endOfBlock;
	    private readonly bool _blackIs1;
	    private readonly int _damagedRowsBeforeError;

	    public int K { get { return _k; } }

        public bool EndOfLine { get { return _endOfLine; } }

        public bool EncodedByteAlign { get { return _encodedByteAlign; } }

        public int Columns { get { return _columns; } }

        public int Rows { get { return _rows; } }

        public bool EndOfBlock { get { return _endOfBlock; } }

        public bool BlackIs1 { get { return _blackIs1; } }

        public int DamagedRowsBeforeError { get { return _damagedRowsBeforeError; } }

	    public CCITTFaxParameters(PDFDictionary dict)
	    {
		    _k = getK(dict);
		    _endOfLine = getEndOfLine(dict);
		    _encodedByteAlign = getEncodedByteAlign(dict);
		    _columns = getColumns(dict);
		    _rows = getRows(dict);
		    _endOfBlock = getEndOfBlock(dict);
		    _blackIs1 = getBlackIs1(dict);
		    _damagedRowsBeforeError = getDamagedRowsBeforeError(dict);
	    }

	    private static int getK(PDFDictionary dict)
        {
            if (dict == null)
                return 0;

            PDFNumber k = dict["K"] as PDFNumber;
            if (k == null)
                return 0;
            return (int)k.GetValue();
        }

        private static bool getEndOfLine(PDFDictionary dict)
        {
            if (dict == null)
                return false;

            PDFBoolean endOfLine = dict["EndOfLine"] as PDFBoolean;
            if (endOfLine == null)
                return false;
            return endOfLine.GetValue();
        }

        private static bool getEncodedByteAlign(PDFDictionary dict)
        {
            if (dict == null)
                return false;

            PDFBoolean encodedByteAlign = dict["EncodedByteAlign"] as PDFBoolean;
            if (encodedByteAlign == null)
                return false;
            return encodedByteAlign.GetValue();
        }

        private static int getColumns(PDFDictionary dict)
        {
            if (dict == null)
                return 1728;

            PDFNumber columns = dict["Columns"] as PDFNumber;
            if (columns == null)
                return 1728;
            return (int)columns.GetValue();
        }

        private static int getRows(PDFDictionary dict)
        {
            if (dict == null)
                return 0;

            PDFNumber rows = dict["Rows"] as PDFNumber;
            if (rows == null)
                return 0;
            return (int)rows.GetValue();
        }

        private static bool getEndOfBlock(PDFDictionary dict)
        {
            if (dict == null)
                return true;

            PDFBoolean endOfBlock = dict["EndOfBlock"] as PDFBoolean;
            if (endOfBlock == null)
                return true;
            return endOfBlock.GetValue();
        }

        private static bool getBlackIs1(PDFDictionary dict)
        {
            if (dict == null)
                return false;

            PDFBoolean blackIs1 = dict["BlackIs1"] as PDFBoolean;
            if (blackIs1 == null)
                return false;
            return blackIs1.GetValue();
        }

        private static int getDamagedRowsBeforeError(PDFDictionary dict)
        {
            if (dict == null)
                return 0;

            PDFNumber damagedRowsBeforeError = dict["DamagedRowsBeforeError"] as PDFNumber;
            if (damagedRowsBeforeError == null)
                return 0;
            return (int)damagedRowsBeforeError.GetValue();
        }
    }
}
