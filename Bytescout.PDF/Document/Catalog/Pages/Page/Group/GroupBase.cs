namespace Bytescout.PDF
{
    internal class GroupBase
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary newDict = new PDFDictionary();

            PDFBoolean k = dict["K"] as PDFBoolean;
            if (k != null)
                newDict.AddItem("K", k.Clone());

            PDFBoolean i = dict["I"] as PDFBoolean;
            if (i != null)
                newDict.AddItem("I", i.Clone());

            PDFName s = dict["S"] as PDFName;
            if (s != null)
                newDict.AddItem("S", s.Clone());

            IPDFObject cs = dict["CS"];
            if (cs != null)
                newDict.AddItem("CS", cs.Clone());

            return newDict;
        }
    }
}
