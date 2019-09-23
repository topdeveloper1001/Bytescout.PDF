using System.IO;

namespace Bytescout.PDF
{
    internal class FullFileSpecification : FileSpecification
    {
        public FullFileSpecification(string filename) 
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            FileName = UF = System.IO.Path.GetFileName(filename);
            PDFDictionary embeddedFS = new PDFDictionary();
            PDFDictionaryStream dictF = new PDFDictionaryStream();
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            dictF.GetStream().Write(data, 0, data.Length);
            embeddedFS.AddItem("F", dictF);
            EF = embeddedFS;
            fs.Close();
        }

        public FullFileSpecification(Stream stream, string name)
        {
            FileName = UF = name;
            PDFDictionary embeddedFS = new PDFDictionary();
            PDFDictionaryStream dictF = new PDFDictionaryStream();
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            dictF.GetStream().Write(data, 0, data.Length);
            embeddedFS.AddItem("F", dictF);
            EF = embeddedFS;
        }
    }
}
