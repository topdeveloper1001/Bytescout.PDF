using System.IO;

namespace Bytescout.PDF
{
    internal interface IPDFObject
    {
        int ObjNo { get; set; }
        IPDFObject Clone();
        void Write(SaveParameters param);
        void Collect(XRef xref);
    }

    internal struct SaveParameters
    {
        internal SaveParameters(Stream stream)
        {
            Stream = stream;
            Encryptor = null;
            Buffer = null;
            StringBuffer = null;
            ObjNo = 0;
            GenNo = 0;
            Compression = PDF.Compression.None;
            WriteInheritableObjects = true;
        }

        public System.IO.MemoryStream Buffer;
        public System.IO.MemoryStream StringBuffer; //used for encryption
        public System.IO.Stream Stream;
        public Encryptor Encryptor;
        public int ObjNo;
        public int GenNo;
        public Compression Compression;
        public bool WriteInheritableObjects;
    }
}
