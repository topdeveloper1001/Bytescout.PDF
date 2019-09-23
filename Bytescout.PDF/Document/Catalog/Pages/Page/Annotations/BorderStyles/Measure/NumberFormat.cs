namespace Bytescout.PDF
{
    internal class NumberFormat
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary result = new PDFDictionary();

            string[] keys = { "Type", "U", "C", "F", "D", "FD", "RT", "RD", "PS", "SS", "O" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = dict[keys[i]];
                if (obj != null)
                    result.AddItem(keys[i], obj.Clone());
            }

            return result;
        }
    }
}
