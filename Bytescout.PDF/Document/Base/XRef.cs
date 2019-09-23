using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Bytescout.PDF
{
    internal class XRef
    {
	    private Lexer _lexer;
	    private readonly List<Entry> _entries;

	    private PDFDictionary _info;
	    private PDFDictionary _catalog;

	    private Encryptor _encryptor;

	    internal List<Entry> Entries { get { return _entries; } }

	    internal Encryptor Encryptor
	    {
		    get { return _encryptor; }
		    set { _encryptor = value; }
	    }

	    internal XRef()
        {
            _entries = new List<Entry>();
        }

	    internal void SetLexer(Lexer lexer)
        {
            _lexer = lexer;
        }

        internal void SetTrailer(PDFDictionary dict)
        {
            _info = dict["Info"] as PDFDictionary;
            _catalog = dict["Root"] as PDFDictionary;
        }

        internal PDFDictionary GetInfo()
        {
            return _info;
        }

        internal void SetInfo(PDFDictionary dict)
        {
            _info = dict;
        }

        internal PDFDictionary GetCatalog()
        {
            return _catalog;
        }

        internal void SetCatalog(PDFDictionary dict)
        {
            _catalog = dict;
        }

        internal void AddEncryption(Security security)
        {
            if (security == null || security.EncryptionAlgorithm == EncryptionAlgorithm.None)
            {
                _encryptor = null;
                return;
            }

            Encryptor encryptor = new Encryptor();
            encryptor.UserPassword = security.UserPassword;
            encryptor.OwnerPassword = security.OwnerPassword;
            encryptor.Permissions = security.GetPermissions();
            encryptor.SetDocumentID(createID());

            if (security.EncryptionAlgorithm == EncryptionAlgorithm.RC4_40bit)
            {
                encryptor.KeyLength = 40;
                encryptor.Version = 1;
                encryptor.Revision = 2;
            }
            else if (security.EncryptionAlgorithm == EncryptionAlgorithm.RC4_128bit)
            {
                encryptor.KeyLength = 128;
                encryptor.Version = 2;
                encryptor.Revision = 3;
            }
            else if (security.EncryptionAlgorithm == EncryptionAlgorithm.AES_128bit)
            {
                encryptor.KeyLength = 128;
                encryptor.Version = 4;
                encryptor.Revision = 4;
            }
            else
            {
                encryptor.KeyLength = 256;
                encryptor.Version = 5;
                encryptor.Revision = 5;
            }

            encryptor.Reset();
            _encryptor = encryptor;
        }

        internal PDFDictionary CreateTrailer()
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Info", _info);
            dict.AddItem("Root", _catalog);
            dict.AddItem("ID", getID());
            dict.AddItem("Size", new PDFNumber(_entries.Count));
            if (_encryptor != null)
                dict.AddItem("Encrypt", createEncrypt());
            return dict;
        }

        internal IPDFObject GetObject(int index)
        {
            if (index < 0 || index >= _entries.Count)
                return null;

            Entry entry = _entries[index];

            if (entry == null)
                return null;

            if (entry.Object != null)
                return entry.Object;

            if (entry.EntryType == 2)
            {
                parseGenerationEntry(entry);
                if (entry.Object == null)
                    return entry.Object = new PDFNull();
                return entry.Object;
            }
            else
            {
                IPDFObject obj = _lexer.ParseEntry(index, entry.Offset, entry.Generation);
                if (obj == null)
                    obj = new PDFNull();

                entry.Object = obj;
                return obj;
            }
        }

        private void parseGenerationEntry(Entry entry)
        {
            PDFDictionaryStream dictStream = GetObject(entry.Offset) as PDFDictionaryStream;
            if (dictStream != null)
            {
                PDFNumber n = dictStream.Dictionary["N"] as PDFNumber;
                if (n == null)
                    return;
                int count = (int) n.GetValue();
                if (count <= 0)
                    return;

                n = dictStream.Dictionary["First"] as PDFNumber;
                if (n == null)
                    return;
                int first = (int) n.GetValue();
                if (first < 0)
                    return;

                dictStream.Decode();
                List<int> objects = new List<int>();
                List<int> offsets = new List<int>();

	            Lexer lexer = new Lexer(dictStream.GetStream(), this, null, 512);
                lexer.Position = 0;

                for (int i = 0; i < count; ++i)
                {
	                bool succes;
	                int objNo = lexer.ReadInteger(out succes);
                    if (!succes)
                        break;
                    int offset = lexer.ReadInteger(out succes);
                    if (!succes)
                        break;
                    objects.Add(objNo);
                    offsets.Add(offset);
                }

                int l = objects.Count;
                for (int i = 0; i < l; ++i)
                {
                    lexer.Position = offsets[i] + first;
                    IPDFObject obj = lexer.ReadObject();
                    if (obj == null)
                        obj = new PDFNull();

                    int index = objects[i];
                    if (index >= 0 && index < _entries.Count)
                    {
                        if (_entries[index] == null)
                            _entries[index] = new Entry(0, 0);
                        if (entry.Offset == _entries[index].Offset)
                            _entries[index].Object = obj;
                    }
                }
            }
        }

        private IPDFObject getID()
        {
            PDFArray arr = new PDFArray();
	        byte[] id = _encryptor != null ? _encryptor.GetDocumentID() : createID();

            arr.AddItem(new PDFString(id, true));
            arr.AddItem(new PDFString(id, true));
            return arr;
        }

        private byte[] createID()
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            return md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(DateTime.Now.ToString(CultureInfo.InvariantCulture)));
        }

        private IPDFObject createEncrypt()
        {
            PDFDictionary dict = new PDFDictionary();

            dict.AddItem("Filter", new PDFName("Standard"));
            dict.AddItem("P", new PDFNumber(_encryptor.Permissions));
            dict.AddItem("R", new PDFNumber(_encryptor.Revision));
            dict.AddItem("V", new PDFNumber(_encryptor.Version));
            dict.AddItem("Length", new PDFNumber(_encryptor.KeyLength));
            dict.AddItem("U", new PDFString(_encryptor.GetUserHash(), true));
            dict.AddItem("O", new PDFString(_encryptor.GetOwnerHash(), true));

            if (_encryptor.Revision >= 4)
            {
                dict.AddItem("EncryptMetadata", new PDFBoolean(_encryptor.EncryptMetadata));
                dict.AddItem("StmF", new PDFName("StdCF"));
                dict.AddItem("StrF", new PDFName("StdCF"));

                PDFDictionary stdCf = new PDFDictionary();
                stdCf.AddItem("AuthEvent", new PDFName("DocOpen"));
                if (_encryptor.Revision == 4)
                {
                    stdCf.AddItem("CFM", new PDFName("AESV2"));
                    stdCf.AddItem("Length", new PDFNumber(16));
                }
                else
                {
                    stdCf.AddItem("CFM", new PDFName("AESV3"));
                    stdCf.AddItem("Length", new PDFNumber(32));
                }

                PDFDictionary cf = new PDFDictionary();
                cf.AddItem("StdCF", stdCf);
                dict.AddItem("CF", cf);
            }

            if (_encryptor.Revision == 5)
            {
                dict.AddItem("OE" ,new PDFString(_encryptor.GetOE(), true));
                dict.AddItem("UE", new PDFString(_encryptor.GetUE(), true));
                dict.AddItem("Perms", new PDFString(_encryptor.GetPerms(), true));
            }

            Entry entry = new Entry(0, 0);
            dict.ObjNo = _entries.Count;
            entry.Object = dict;
            _entries.Add(entry);

            return dict;
        }
    }
}
