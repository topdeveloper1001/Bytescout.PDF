namespace Bytescout.PDF
{
    internal enum FileSystemType
    {
        F = 0,      //file
        UF = 1,     //url
        DOS = 2,    //windows
        Mac = 3,    //MacOS
        Unix = 4    //Unix
    }

    internal class FileSpecification
    {
	    private readonly PDFDictionary _dictionary;

	    public string FileName
        {
            get
            {
                PDFString f = _dictionary["F"] as PDFString;
                if (f == null)
                    return "";
                byte[] data = f.GetBytes();
                return System.Text.Encoding.Default.GetString(data);
            }
            internal set
            {
                _dictionary.AddItem("F", new PDFString(System.Text.Encoding.Default.GetBytes(value), false));
            }
        }

        internal FileSystemType FS
        {
            get
            {
                PDFName fs = _dictionary["FS"] as PDFName;
                if (fs == null)
                    return FileSystemType.DOS;
                return TypeConverter.StringToPDFFileSystem(fs.GetValue());
            }
            set
            {
                _dictionary.AddItem("FS", new PDFName(TypeConverter.PDFFileSystemToString(value)));
            }
        }

        internal string UF
        {
            get
            {
                PDFString uf = _dictionary["UF"] as PDFString;
                if (uf == null)
                    return "";
                return uf.GetValue();
            }
            set
            {
                _dictionary.AddItem("UF", new PDFString(System.Text.Encoding.Default.GetBytes(value), false));
            }
        }

        internal PDFDictionary EF
        {
            get
            {
                return _dictionary["EF"] as PDFDictionary;
            }
            set
            {
                _dictionary.AddItem("EF", value);
            }
        }

	    internal FileSpecification()
	    {
		    _dictionary = new PDFDictionary();
		    _dictionary.AddItem("Type", new PDFName("Filespec"));
	    }

	    internal FileSpecification(PDFDictionary dict)
	    {
		    _dictionary = dict;
	    }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }
    }
}
