namespace Bytescout.PDF
{
    internal class ResourcesBase
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary newDict = new PDFDictionary();
            string[] keys = { "ExtGState", "ColorSpace", "Pattern", "Shading", "XObject", "Font", "Properties" };
            for (int i = 0; i < keys.Length; ++i)
            {
                PDFDictionary res = dict[keys[i]] as PDFDictionary;
                if (res != null)
                    newDict.AddItem(keys[i], res.Clone());
            }

            PDFArray procSet = dict["ProcSet"] as PDFArray;
            if (procSet != null)
                newDict.AddItem("ProcSet", procSet.Clone());

            return newDict;
        }
    }
}
