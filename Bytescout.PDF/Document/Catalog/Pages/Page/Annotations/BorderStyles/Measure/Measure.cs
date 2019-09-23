namespace Bytescout.PDF
{
    internal class Measure
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary result = new PDFDictionary();

            string[] keys = { "Type", "Subtype", "R", "CYX", "O" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = dict[keys[i]];
                if (obj != null)
                    result.AddItem(keys[i], obj.Clone());
            }

            string[] numberFormat = { "X", "Y", "D", "A", "T", "S" };
            for (int i = 0; i < numberFormat.Length; ++i)
            {
                PDFArray arr = dict[numberFormat[i]] as PDFArray;
                if (arr != null)
                {
                    PDFArray newArr = new PDFArray();
                    for (int j = 0; j < arr.Count; ++i)
                    {
                        PDFDictionary d = arr[i] as PDFDictionary;
                        if (d != null)
                            newArr.AddItem(NumberFormat.Copy(d));
                    }
                    result.AddItem(numberFormat[i], newArr);
                }
            }

            return result;
        }
    }
}
