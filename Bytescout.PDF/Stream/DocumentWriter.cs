using System.IO;

namespace Bytescout.PDF
{
    internal class DocumentWriter
    {
	    private readonly XRef _xref;
	    private long _startXref;
	    private readonly Compression _compression;

        private int _signature = -1;
        //private int _digestOffset = -1;
        private int _byteRangeOffset = -1;
        private int[] _byteRange;
        internal Certificate _certificate;
        //private bool _forced = false;

	    public DocumentWriter(XRef xref, Compression compression)
        {
            _xref = xref;
            _compression = compression;
        }


        //public bool Forced
        //{
        //    get { return _forced; }
        //    set { _forced = value; }
        //}

        public int signature
        {
            get { return _signature; }
            set { _signature = value; }
        }

        public void Write(string path)
        {
            createDirectoryIfNeed(path);

            FileStream writer = new FileStream(path, FileMode.Create);

            Write(writer);
            
            writer.Close();
        }

        public void Write(Stream writer)
        {
            string header = "%PDF-1.5\n%·ѕ­Є\n";
            writer.Write(System.Text.Encoding.Default.GetBytes(header), 0, header.Length);
            PDFDictionary trailer = _xref.CreateTrailer();

            writeObjects(writer);
            writeXref(writer);
            writeTrailer(writer, trailer);
            writeStartXref(writer);

            int count = _xref.Entries.Count;
            for (int i = 1; i < count; ++i)
                _xref.Entries[i].Object.ObjNo = -1;

            string eof = "%%EOF";
            writer.Write(System.Text.Encoding.ASCII.GetBytes(eof), 0, eof.Length);

            // подписываем документ
            //if (_signature > 0)
            if (_signature > 0 && _certificate != null)
            {
                // Записываем ByteRange
                _byteRange[3] = (int)(writer.Length - _byteRange[2]);
                writer.Position = _byteRangeOffset;
                writer.WriteByte((byte)'[');
                for (int i = 0; i < _byteRange.Length; i++)
                {
                    string integer = _byteRange[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
                    for (int j = 0; j < integer.Length; j++)
                    {
                        writer.WriteByte((byte)integer[j]);
                    }
                    writer.WriteByte((byte)' ');
                }
                writer.WriteByte((byte)']');

                Signing signing = new Signing(writer, _byteRange, _certificate);
                signing.WriteDigest();
                //// Записываем Contents
                //writer.Position = _byteRange[1]+1;
                //// Здесь записываем дайджест
                //writer.WriteByte((byte)'T');
                //writer.WriteByte((byte)'e');
                //writer.WriteByte((byte)'s');
                //writer.WriteByte((byte)'t');
            }
        }

        private void createDirectoryIfNeed(string fileName)
        {
            int length = fileName.LastIndexOf('\\');
            if (length == -1)
                return;

            string folderPath = fileName.Substring(0, length);
            if (folderPath == "")
                return;

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        private void writeObjects(Stream writer)
        {
            int count = _xref.Entries.Count;
            byte[] endObj = System.Text.Encoding.ASCII.GetBytes("\nendobj\n");
            SaveParameters param = new SaveParameters();
            param.Stream = writer;
            param.WriteInheritableObjects = false;
            param.Buffer = new MemoryStream(4096);
            param.StringBuffer = new MemoryStream(1024);
            param.Compression = _compression;
            param.Encryptor = _xref.Encryptor;
            if (_xref.Encryptor != null)
                count--;

            for (int i = 1; i < count; ++i)
            {
                Entry entry = _xref.Entries[i];
                entry.Offset = (int)writer.Position;

                string startObj = i.ToString() + " 0 obj\n";
                writer.Write(System.Text.Encoding.ASCII.GetBytes(startObj), 0, startObj.Length);
                param.ObjNo = i;

                // Если словарь подписи, то находим запись ByteRange и ее офсет 
                if (entry.Object.ObjNo == _signature)
                    _byteRangeOffset = Sig.Write(param, entry.Object as PDFDictionary, out _byteRange);
                else
                    entry.Object.Write(param);

                writer.Write(endObj, 0, endObj.Length);
            }

            if (_xref.Encryptor != null)
            {
                param.Encryptor = null;
                param.WriteInheritableObjects = true;
                Entry entry = _xref.Entries[count];
                entry.Offset = (int)writer.Position;
                string startObj = count.ToString() + " 0 obj\n";
                writer.Write(System.Text.Encoding.ASCII.GetBytes(startObj), 0, startObj.Length);
                entry.Object.Write(param);
                writer.Write(endObj, 0, endObj.Length);
            }

            param.Buffer.Dispose();
            param.StringBuffer.Dispose();
        }

        private void writeXref(Stream writer)
        {
            _startXref = writer.Position;
            string xrefHeader = "xref\n0 " + _xref.Entries.Count.ToString() + "\n";
            writer.Write(System.Text.Encoding.ASCII.GetBytes(xrefHeader), 0, xrefHeader.Length);

            int count = _xref.Entries.Count;
            byte[] offset = new byte[10];
            byte[] generation = new byte[5];

            for (int i = 0; i < count; ++i)
            {
                createOffset(_xref.Entries[i].Offset, offset);
                writer.Write(offset, 0, offset.Length);
                writer.WriteByte(32);
                createGeneration(_xref.Entries[i].Generation, generation);
                writer.Write(generation, 0, generation.Length);
                writer.WriteByte(32);

                if (i != 0)
                    writer.WriteByte((byte)'n');
                else
                    writer.WriteByte((byte)'f');
                writer.WriteByte((byte)'\r');
                writer.WriteByte((byte)'\n');
            }
        }

        private void writeTrailer(Stream writer, PDFDictionary trailer)
        {
            string trailerHeader = "trailer\n";
            writer.Write(System.Text.Encoding.ASCII.GetBytes(trailerHeader), 0, trailerHeader.Length);
            SaveParameters param = new SaveParameters(writer);
            param.WriteInheritableObjects = false;
            trailer.Write(param);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("\n"), 0, 1);
        }

        private void writeStartXref(Stream writer)
        {
            string startXref = "startxref\n";
            startXref += _startXref.ToString() + "\n";
            writer.Write(System.Text.Encoding.ASCII.GetBytes(startXref), 0, startXref.Length);
        }
       
        private void createOffset(long number, byte[] bytes)
        {
            for (int i = bytes.Length - 1; i >= 0; --i)
            {
                bytes[i] = (byte)(number % 10 + '0');
                number /= 10;
            }
        }

        private void createGeneration(int number, byte[] bytes)
        {
            for (int i = bytes.Length - 1; i >= 0; --i)
            {
                bytes[i] = (byte)(number % 10 + '0');
                number /= 10;
            }
        }
    }
}
