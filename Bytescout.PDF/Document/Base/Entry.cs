namespace Bytescout.PDF
{
    internal class Entry
    {
	    private byte _entryType;
	    private int _offset;
	    private int _generation;
	    private IPDFObject _object;

	    public byte EntryType
	    {
		    get { return _entryType; }
		    set { _entryType = value; }
	    }

	    public int Offset
	    {
		    get { return _offset; }
		    set { _offset = value; }
	    }

	    public int Generation
	    {
		    get { return _generation; }
		    set { _generation = value; }
	    }

	    public IPDFObject Object
	    {
		    get { return _object; }
		    set { _object = value; }
	    }

	    public Entry(int offset, int generation)
        {
            _offset = offset;
            _generation = generation;
            _entryType = 1;
        }

        public Entry(byte entryType, int offset, int generation)
        {
            _entryType = entryType;
            _offset = offset;
            _generation = generation;
        }

	    public override string ToString()
        {
            if (_object != null)
                return _object.ToString();
            return "";
        }
    }
}
