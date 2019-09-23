using System.IO;

namespace Bytescout.PDF
{
    internal class OffsetTable
    {
	    private string _tag;
	    private long _checksum;
	    private long _offset;
	    private long _length;
	    private TTFTable _table;

	    public string Tag { get { return _tag; } }

        public long Checksum 
        {
            get { return _checksum; }
            set { _checksum = value; }
        }

        public long Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        public long Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public TTFTable Table
        {
            get { return _table; }
            set { _table = value; }
        }

	    public void Read(Reader reader)
        {
            string tag = "";
            tag += (char)reader.ReadByte();
            tag += (char)reader.ReadByte();
            tag += (char)reader.ReadByte();
            tag += (char)reader.ReadByte();

            _tag = tag;
            _checksum = reader.ReadUint32();
            _offset = reader.ReadUint32();
            _length = reader.ReadUint32();
        }

        public void Write(Stream stream)
        {
            byte[] bTag = System.Text.Encoding.ASCII.GetBytes(_tag);

            stream.Write(bTag, 0, bTag.Length);
            stream.Write(BinaryUtility.UInt32ToBytes((uint)_checksum), 0, 4);
            stream.Write(BinaryUtility.UInt32ToBytes((uint)_offset), 0, 4);
            stream.Write(BinaryUtility.UInt32ToBytes((uint)_length), 0, 4);
        }

        public override string ToString()
        {
            return _tag;
        }
    }
}
