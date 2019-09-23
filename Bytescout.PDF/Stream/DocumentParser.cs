using System.Collections.Generic;
using System.IO;

namespace Bytescout.PDF
{
    internal class DocumentParser
    {
	    private readonly XRef _xref;
	    private PDFDictionary _trailer;
	    private readonly Lexer _lexer;
	    private readonly List<int> _xrefPositions = new List<int>();
	    private readonly List<int> _parsedXrefTables = new List<int>();

	    public DocumentParser(Stream stream)
        {
            _xref = new XRef();
            _lexer = new Lexer(stream, _xref, null, 4096);
            _xref.SetLexer(_lexer);
        }

        public void Parse(string password)
        {
            long xrefPos = findXref();
            parseXref(xrefPos);

            PDFDictionary encrypt = _trailer["Encrypt"] as PDFDictionary;
            if (encrypt != null)
            {
                Encryptor encryptor = new Encryptor(encrypt, _trailer["ID"] as PDFArray);
                if (!encryptor.AuthenticatePassword(password))
                    throw new InvalidPasswordException(new PasswordManager(encryptor));
                encryptor.RecalculateEncryptionKey();
                _xref.Encryptor = encryptor;
                _lexer.SetEncryptor(encryptor);
            }

            _xref.Entries.TrimExcess();
            _xref.SetTrailer(_trailer);
        }

        internal XRef GetXref()
        {
            return _xref;
        }

        private long findXref()
        {
            long startxref = _lexer.FindLastSubstring("startxref");
            if (startxref < 0)
                throw new InvalidDocumentException();

            _lexer.Position = startxref + 9;
            bool succes;
            startxref = _lexer.ReadInteger(out succes);
            if (!succes || startxref < 0)
                return 0;
            return startxref;
        }

        private bool tryToRepairDocument()
        {
            long curPos = 0;
            List<long> eofs = new List<long>();
            eofs.Add(0);

            long eofPos;
            while ((eofPos = _lexer.FindSubstring("%%EOF", curPos, _lexer.Length)) > 0)
            {
                eofs.Add(eofPos);
                curPos = eofPos + 5;
            }

            for (int i = 0; i < eofs.Count - 1; ++i)
            {
                long xrefpos = _lexer.FindSubstring((char)256 + "xref", eofs[i], eofs[i + 1]);
                if (xrefpos > 0 && !_parsedXrefTables.Contains((int)xrefpos))
                {
                    _xrefPositions.Add((int)xrefpos);
                }
            }

            if (_xrefPositions.Count == 0 && _parsedXrefTables.Count == 0)
                return false;
            return true;
        }

        private void parseXref(long xrefPos)
        {
            _lexer.Position = xrefPos;
            _lexer.ReadLexeme();
            PDFDictionaryStream xref;

            if (_lexer.CurrentLexemeEquals("xref"))
            {
                readXrefTable();
                parseTrailer();
            }
            else if (isCrossReferenceStream(out xref))
            {
                readCrossReferencesEntries(xref);
                extractTrailer(xref.Dictionary);
            }
            else
            {
                if (!tryToRepairDocument())
                    throw new InvalidDocumentException();

                for (int i = 0; i < _xrefPositions.Count; ++i)
                    parseXref(_xrefPositions[i]);
                return;
            }

            parsePrevAndXrefStm();
        }

        private bool isCrossReferenceStream(out PDFDictionaryStream xref)
        {
            xref = _lexer.ParseCrossRefObject();
            return xref != null;
        }

        private void parsePrevAndXrefStm()
        {
            if (_xrefPositions.Count == 0)
            {

                PDFNumber Prev = _trailer["Prev"] as PDFNumber;
                PDFNumber XRefStm = _trailer["XRefStm"] as PDFNumber;

                if (XRefStm != null)
                {
                    int val = (int)XRefStm.GetValue();
                    _trailer.RemoveItem("XRefStm");
                    _parsedXrefTables.Add(val);
                    parseXref(val);
                }
                if (Prev != null)
                {
                    int val = (int)Prev.GetValue();
                    _trailer.RemoveItem("Prev");
                    _parsedXrefTables.Add(val);
                    parseXref(val);
                }
            }
        }

        private void readCrossReferencesEntries(PDFDictionaryStream xrefStream)
        {
            int[] W = getW(xrefStream.Dictionary);
            int[] Index = getIndex(xrefStream.Dictionary);

            xrefStream.Decode();
            Stream stream = xrefStream.GetStream();
            stream.Position = 0;

            byte[] bType = new byte[W[0]];
            byte[] bOffset = new byte[W[1]];
            byte[] bGeneration = new byte[W[2]];

            int prevCount, count, type, offset, generation;

            for (int i = 0; i < Index.Length / 2; ++i)
            {
                prevCount = Index[2 * i];
                count = Index[2 * i + 1];
                addEntries(prevCount + count);

                for (int j = 0; j < count; ++j)
                {
                    if (stream.Read(bType, 0, bType.Length) < 0)
                        return;
                    type = bytesToInt(bType);

                    if (stream.Read(bOffset, 0, bOffset.Length) < 0)
                        return;
                    offset = bytesToInt(bOffset);

                    if (stream.Read(bGeneration, 0, bGeneration.Length) < 0)
                        return;
                    if (W[2] == 0)
                        generation = 0;
                    else
                        generation = bytesToInt(bGeneration);

                    if (_xref.Entries[prevCount + j] == null)
                        _xref.Entries[prevCount + j] = new Entry((byte)type, offset, generation);
                }
            }
        }

        private void readXrefTable()
        {
            int prevCount, count, offset, generation;
            bool succes;

            for (; ; )
            {
                _lexer.ReadLexeme();
                if (_lexer.CurrentLexemeEquals("trailer"))
                    break;

                prevCount = _lexer.CurrentLexemeToInteger(out succes);
                if (!succes || prevCount < 0)
                    return;

                count = _lexer.ReadInteger(out succes);
                if (!succes || count < 0)
                    return;

                addEntries(prevCount + count);
                for (int i = 0; i < count; ++i)
                {
                    offset = _lexer.ReadInteger(out succes);
                    if (!succes)
                        return;

                    generation = _lexer.ReadInteger(out succes);
                    if (!succes)
                        return;

                    _lexer.ReadLexeme();
                    if (_xref.Entries[prevCount + i] == null)
                        _xref.Entries[prevCount + i] = new Entry(offset, generation);
                }
            }
        }

        private void parseTrailer()
        {
            PDFDictionary dict = _lexer.ReadObjectWithLastParsedByte() as PDFDictionary;
            extractTrailer(dict);
        }

        private void extractTrailer(PDFDictionary dict)
        {
            if (dict == null)
                throw new InvalidDocumentException();

            if (_trailer != null)
            {
                _trailer.RemoveItem("Prev");
                _trailer.AddRange(dict);
            }
            else
                _trailer = dict;
        }

        private void addEntries(int count)
        {
            if (count > _xref.Entries.Count)
            {
                if (_xref.Entries.Capacity < count)
                    _xref.Entries.Capacity = count;
                int newItemsCount = count - _xref.Entries.Count;
                for (int i = 0; i < newItemsCount; ++i)
                    _xref.Entries.Add(null);
            }
        }

        private int[] getW(PDFDictionary dict)
        {
            PDFArray W = dict["W"] as PDFArray;
            if (W == null)
                throw new InvalidDocumentException();
            int[] w = { 1, 1, 1 };
            for (int i = 0; i < W.Count; ++i)
            {
                PDFNumber number = W[i] as PDFNumber;
                if (number != null && number.GetValue() >= 0)
                    w[i] = (int)number.GetValue();
            }

            return w;
        }

        private int[] getIndex(PDFDictionary dict)
        {
            PDFArray Index = dict["Index"] as PDFArray;
            if (Index == null)
            {
                int[] ind = { 0, 0 };
                PDFNumber size = dict["Size"] as PDFNumber;
                if (size == null || size.GetValue() <= 0)
                    throw new InvalidDocumentException();

                ind[1] = (int)size.GetValue();
                return ind;
            }

            int[] index = new int[Index.Count];
            for (int i = 0; i < Index.Count; ++i)
            {
                PDFNumber number = Index[i] as PDFNumber;
                if (number == null || number.GetValue() < 0)
                    throw new InvalidDocumentException();

                index[i] = (int)number.GetValue();
            }

            return index;
        }

        private static int bytesToInt(byte[] buf)
        {
            if (buf.Length == 0)
                return 1;

            int result = buf[buf.Length - 1];
            int m = 256;
            for (int i = buf.Length - 2; i >= 0; i--)
            {
                result += buf[i] * m;
                m *= 256;
            }

            return result;
        }
    }
}
