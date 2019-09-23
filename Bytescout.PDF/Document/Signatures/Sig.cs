using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Sig
#else
    /// <summary>
    /// Represents class for a PDF document's metadata.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    internal class Sig
#endif
    {
        private readonly PDFDictionary _dictionary;
        //private Certificate _certificate;

        internal Sig(Certificate certificate)
        {
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Sig"));
            _dictionary.AddItem("Filter", new PDFName("Adobe.PPKMS"));
            _dictionary.AddItem("SubFilter", new PDFName("adbe.pkcs7.sha1"));
            //_dictionary.AddItem("Filter", new PDFName("Adobe.PPKLite"));
            //_dictionary.AddItem("SubFilter", new PDFName("adbe.pkcs7.detached"));
            _dictionary.AddItem("ByteRange", new PDFArray());

            //_certificate = certificate;
            PKCS7 pkcs = new PKCS7(certificate.Akp, certificate.Chain, null, "SHA1", true);
            string name = PKCS7.GetSubjectFields(pkcs.SigningCertificate).GetField("CN");
            _dictionary.AddItem("Name", new PDFString(System.Text.Encoding.Default.GetBytes(name), false));
            byte[] buf = pkcs.GetEncodedPKCS7(null, DateTime.Now, /*null,*/ null);
            byte[] digest = new byte[buf.Length/* + 64*/];
            _dictionary.AddItem("Contents", new PDFString(digest, true));
        }


        internal Sig(Certificate certificate, string reason, string contact, string location)
            : this(certificate)
        {
            if (location != null && location != "")
                _dictionary.AddItem("Location", new PDFString(location));
            if (reason != null && reason != "")
                _dictionary.AddItem("Reason", new PDFString(reason));
            if (contact != null && contact != "")
                _dictionary.AddItem("ContactInfo", new PDFString(contact));
        }

        internal Sig(PDFDictionary dict)
        {
            _dictionary = dict;
        }

        internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        //internal ByteRange
        //internal Contents

        static internal int Write(SaveParameters param, PDFDictionary dict, out int[] byteRange)
        {
            int byteRangeOffset = -1;
            byteRange = new int[4];
            byteRange[0] = 0;

            Stream stream = param.Stream;

            stream.WriteByte((byte)'<');
            stream.WriteByte((byte)'<');

            string[] keys = dict.GetKeys();
            int count = keys.Length;
            for (int i = 0; i < count; ++i)
            {
                string key = keys[i];
                PDFName.Write(stream, key);

                stream.WriteByte((byte)' ');

                IPDFObject val = dict[key];

                if (key == "ByteRange")
                    byteRangeOffset = (int)stream.Position;
                else if (key == "Contents")
                    byteRange[1] = (int)stream.Position;

                if (val is PDFDictionary || val is PDFDictionaryStream)
                {
                    if (!param.WriteInheritableObjects)
                    {
                        StringUtility.WriteToStream(val.ObjNo, stream);
                        stream.WriteByte((byte)' ');
                        stream.WriteByte((byte)'0');
                        stream.WriteByte((byte)' ');
                        stream.WriteByte((byte)'R');
                    }
                    else
                        val.Write(param);
                }
                else
                {
                    val.Write(param);
                }

                if (key == "ByteRange")
                    for (int j = 0; j < 30; j++)
                        stream.WriteByte((byte)' ');
                else if (key == "Contents")
                    byteRange[2] = (int)stream.Position;

                if (i != count - 1)
                    stream.WriteByte((byte)'\n');
            }

            stream.WriteByte((byte)'>');
            stream.WriteByte((byte)'>');

            return byteRangeOffset;
        }
    }
}
