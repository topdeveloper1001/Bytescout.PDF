using System.IO;

namespace Bytescout.PDF
{
    internal class ICCBased : PDFArray, IColorspace
    {
        public ICCBased(Stream stream)
        {
            AddItem(new PDFName("ICCBased"));
            PDFDictionaryStream dstream = new PDFDictionaryStream();
            byte [] buf = new byte[4];
            stream.Position = 16;
            stream.Read(buf, 0, buf.Length);
            switch(Encoding.GetString(buf))
            {
                case "CMYK":
                    WriteDictionary(dstream.Dictionary, 4, "DeviceCMYK");
                    break;
                case "GRAY":
                    WriteDictionary(dstream.Dictionary, 1, "DeviceGRAY");
                    break;
                case "RGB ":
                    WriteDictionary(dstream.Dictionary, 3, "DeviceRGB");
                    break;
                case "Lab ":
                    //TODO
                    break;
            }
            buf = new byte[stream.Length];
            stream.Read(buf, 0, buf.Length);
            dstream.GetStream().Write(buf, 0, buf.Length);
            AddItem(dstream);
        }

        public ICCBased(byte[] array)
        {
            AddItem(new PDFName("ICCBased"));
            PDFDictionaryStream dstream = new PDFDictionaryStream();
            byte[] buf = new byte[4];
            for (int i = 16; i < 20; ++i)
                buf[i - 16] = array[i];
            switch (Encoding.GetString(buf))
            {
                case "CMYK":
                    WriteDictionary(dstream.Dictionary, 4, "DeviceCMYK");
                    break;
                case "GRAY":
                    WriteDictionary(dstream.Dictionary, 1, "DeviceGRAY");
                    break;
                case "RGB ":
                    WriteDictionary(dstream.Dictionary, 3, "DeviceRGB");
                    break;
                case "Lab ":
                    //TODO
                    break;
            }
            dstream.GetStream().Write(array, 0, array.Length);
            AddItem(dstream);
        }

        public ICCBased(string filename)
        {
            FileStream fstream = new FileStream(filename, FileMode.Open);
            AddItem(new PDFName("ICCBased"));
            PDFDictionaryStream dstream = new PDFDictionaryStream();
            byte[] buf = new byte[4];
            fstream.Position = 16;
            fstream.Read(buf, 0, buf.Length);
            switch (Encoding.GetString(buf))
            {
                case "CMYK":
                    WriteDictionary(dstream.Dictionary, 4, "DeviceCMYK");
                    break;
                case "GRAY":
                    WriteDictionary(dstream.Dictionary, 1, "DeviceGRAY");
                    break;
                case "RGB ":
                    WriteDictionary(dstream.Dictionary, 3, "DeviceRGB");
                    break;
                case "Lab ":
                    //TODO
                    break;
            }
            buf = new byte[fstream.Length];
            fstream.Read(buf, 0, buf.Length);
            dstream.GetStream().Write(buf, 0, buf.Length);
            AddItem(dstream);
        }

        public string Name
        {
            get
            {
                return (this[0] as PDFName).GetValue();
            }
        }

        public int CountComponents
        {
            get
            {
                return (int)((this[1] as PDFDictionaryStream).Dictionary["N"] as PDFNumber).GetValue();
            }
        }

        private void WriteDictionary(PDFDictionary dict, int n, string colorspace)
        {
            dict.AddItem("N", new PDFNumber(n));
            dict.AddItem("Alternate", new PDFName(colorspace));
        }
    }
}
