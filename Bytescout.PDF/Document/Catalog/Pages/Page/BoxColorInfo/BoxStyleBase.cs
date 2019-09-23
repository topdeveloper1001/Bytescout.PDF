namespace Bytescout.PDF
{
    internal class BoxStyleBase
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary newDict = new PDFDictionary();

            string[] arrays = { "C", "D" };
            for (int i = 0; i < arrays.Length; ++i)
            {
                PDFArray arr = dict[arrays[i]] as PDFArray;
                if (arr != null)
                {
                    PDFArray newArr = new PDFArray();
                    for (int j = 0; j < arr.Count; ++j)
                    {
                        PDFNumber num = arr[j] as PDFNumber;
                        newArr.AddItem(num.Clone());
                    }
                    newDict.AddItem(arrays[i], newArr);
                }
            }

            PDFNumber w = dict["W"] as PDFNumber;
            if (w != null)
                newDict.AddItem("W", w.Clone());

            PDFName s = dict["S"] as PDFName;
            if (s != null)
                newDict.AddItem("S", s.Clone());

            return newDict;
        }
    }
}
