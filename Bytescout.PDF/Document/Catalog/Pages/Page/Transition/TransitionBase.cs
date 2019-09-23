namespace Bytescout.PDF
{
    internal class TransitionBase
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary newDict = new PDFDictionary();
            string[] keys = { "Type", "S", "D", "Dm", "M", "Di", "SS", "B" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject val = dict[keys[i]];
                if (val != null)
                    newDict.AddItem(keys[i], val.Clone());
            }

            return newDict;
        }
    }
}
