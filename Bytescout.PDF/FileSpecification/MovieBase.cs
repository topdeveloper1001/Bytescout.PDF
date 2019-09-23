namespace Bytescout.PDF
{
    internal class MovieBase
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary result = new PDFDictionary();

            string[] keys = { "F", "Aspect", "Rotate" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = dict[keys[i]];
                if (obj != null)
                    result.AddItem(keys[i], obj.Clone());
            }

            IPDFObject poster = dict["Poster"];
            if (poster != null)
                result.AddItem("Poster", poster);

            return result;
        }
    }
}
