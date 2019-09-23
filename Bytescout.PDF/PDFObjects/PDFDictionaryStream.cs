using System.Collections.Generic;
using System.IO;

namespace Bytescout.PDF
{
    internal enum Filter
    {
        ASCIIHex,
        ASCII85,
        Flate,
        LZW,
        RunLength,
        CCITTFax,
        JBIG2,
        DCT,
        JPX,
        None
    }

    internal class PDFDictionaryStream : IPDFObject
    {
	    private PDFDictionary _dictionary;
	    private MemoryStream _stream;

	    private static readonly byte[] StartStream = { 10, 115, 116, 114, 101, 97, 109, 10 };
	    private static readonly byte[] EndStream = { 10, 101, 110, 100, 115, 116, 114, 101, 97, 109 };

	    public int ObjNo
        {
            get { return _dictionary.ObjNo; }
            set { _dictionary.ObjNo = value; }
        }

        public PDFDictionary Dictionary
        {
            get
            {
                return _dictionary;
            }
            set
            {
                _dictionary = value;
            }
        }

	    public PDFDictionaryStream()
	    {
		    _dictionary = new PDFDictionary();
		    _stream = new MemoryStream();
	    }

	    public PDFDictionaryStream(PDFDictionary dict, MemoryStream stream)
	    {
		    _dictionary = dict;
		    _stream = stream;
	    }

	    public void Decode()
        {
            Filter[] filters = getFilters();
            if (filters.Length == 0)
                return;

            _stream.Position = 0;
            try
            {
                for (int i = 0; i < filters.Length; ++i)
                {
                    switch (filters[i])
                    {
                        case Filter.ASCII85:
                            _stream = ASCII85Decoder.Decode(_stream);
                            break;
                        case Filter.ASCIIHex:
                            _stream = ASCIIHexDecoder.Decode(_stream);
                            break;
                        case Filter.RunLength:
                            _stream = RunLengthDecoder.Decode(_stream);
                            break;
                        case Filter.JPX:
                            _stream = JPXDecoder.Decode(_stream);
                            break;
                        case Filter.CCITTFax:
                            _stream = CCITTFaxDecoder.Decode(_stream, getDecodeParameters(i));
                            break;
                        case Filter.DCT:
                            _stream = DCTDecoder.Decode(_stream, getDecodeParameters(i));
                            break;
                        case Filter.Flate:
                            _stream = FlateDecoder.Decode(_stream, getDecodeParameters(i));
                            break;
                        case Filter.JBIG2:
                            _stream = JBIG2Decoder.Decode(_stream, getDecodeParameters(i));
                            break;
                        case Filter.LZW:
                            _stream = LZWDecoder.Decode(_stream, getDecodeParameters(i));
                            break;
                    }
                    _stream.Position = 0;
                }
            }
            catch
            {
                return;
            }
            
            _dictionary.RemoveItem("Filter");
            _dictionary.RemoveItem("DecodeParms");
        }

        public MemoryStream GetStream()
        {
            return _stream;
        }

        public void Write(SaveParameters param)
        {
            _stream.Position = 0;
            MemoryStream output = _stream;

            //set compression
            bool filter = false;

            if (param.Compression == Compression.Flate)
            {
                PDFName subtype = (PDFName)_dictionary["Subtype"];
                Filter[] filters = getFilters();
                if (subtype != null && subtype.GetValue() == "Image" && filters.Length > 0 && filters[0] == Filter.DCT)
                {
                    output = param.Buffer;
                    output.SetLength(0);
                    _stream.WriteTo(output);

                    filter = true;
                }
                else if (param.Compression != Compression.None && getFilters().Length == 0)
                {
                    output = param.Buffer;
                    output.SetLength(0);
                    FlateDecoder.Code(_stream, output);
                    _dictionary.AddItem("Filter", new PDFName("FlateDecode"));
                    filter = true;
                }
            }
            //set encryption
            if (param.Encryptor != null)
            {
                param.Encryptor.ResetObjectReference(param.ObjNo, param.GenNo, DataType.Stream);
                if (filter)
                {
                    byte[] buffer = output.GetBuffer();
                    int length = (int)output.Length;
                    output.SetLength(0);
                    param.Encryptor.Encrypt(buffer, 0, length, output, DataType.Stream);
                }
                else
                {
                    output = param.Buffer;
                    output.SetLength(0);
                    param.Encryptor.Encrypt(_stream.GetBuffer(), 0, (int)_stream.Length, output, DataType.Stream);
                }
            }

            output.Position = 0;
            _dictionary.AddItem("Length", new PDFNumber(output.Length));

            _dictionary.Write(param);
            param.Stream.Write(StartStream, 0, StartStream.Length);
            param.Stream.Write(output.GetBuffer(), 0, (int)output.Length);
            param.Stream.Write(EndStream, 0, EndStream.Length);

            if (filter)
                _dictionary.RemoveItem("Filter");
        }

        public IPDFObject Clone()
        {
            _stream.Position = 0;
            MemoryStream ms = new MemoryStream();
            _stream.WriteTo(ms);
            return new PDFDictionaryStream(_dictionary.Clone() as PDFDictionary, ms);
        }

        public override string ToString()
        {
            return "{Stream}";
        }

        public void Collect(XRef xref)
        {
            if (ObjNo != -1)
                return;

            Entry entry = new Entry(0, 0);
            entry.Object = this;
            xref.Entries.Add(entry);

            ObjNo = xref.Entries.Count - 1;

            string[] keys = _dictionary.GetKeys();
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject item = _dictionary[keys[i]];
                item.Collect(xref);
            }
        }

        private Filter[] getFilters()
        {
            IPDFObject obj = _dictionary["Filter"];
            if (obj == null)
                return new Filter[0];

            List<Filter> filters = new List<Filter>();
            if (obj is PDFArray)
            {
                PDFArray arr = obj as PDFArray;
                for (int i = 0; i < arr.Count; ++i)
                {
                    PDFName filter = arr[i] as PDFName;
                    if (filter != null)
                        filters.Add(convertPDFNameToFilter(filter.GetValue()));
                }
            }
            else if (obj is PDFName)
            {
                filters.Add(convertPDFNameToFilter((obj as PDFName).GetValue()));
            }

            return filters.ToArray();
        }

        private Filter convertPDFNameToFilter(string val)
        {
            switch (val)
            {
                case "ASCIIHexDecode":
                    return Filter.ASCIIHex;
                case "ASCII85Decode":
                    return Filter.ASCII85;
                case "RunLengthDecode":
                    return Filter.RunLength;
                case "CCITTFaxDecode":
                    return Filter.CCITTFax;
                case "JBIG2Decode":
                    return Filter.JBIG2;
                case "DCTDecode":
                    return Filter.DCT;
                case "JPXDecode":
                    return Filter.JPX;
                case "FlateDecode":
                    return Filter.Flate;
                case "LZWDecode":
                    return Filter.LZW;
            }
            return Filter.None;
        }

        private PDFDictionary getDecodeParameters(int index)
        {
            IPDFObject param = _dictionary["DecodeParms"];
            if (param == null)
                return null;

            if (param is PDFDictionary && index == 0)
                return param as PDFDictionary;
            else if (param is PDFArray)
                return (param as PDFArray)[index] as PDFDictionary;

            return null;
        }
    }
}
