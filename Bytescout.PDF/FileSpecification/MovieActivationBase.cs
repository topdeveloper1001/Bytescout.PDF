namespace Bytescout.PDF
{
    internal class MovieActivationBase
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary result = new PDFDictionary();

            string[] keys = { "Start", "Duration", "Rate", "Volume", "ShowControls", "Mode", "Synchronous", "FWScale", "FWPosition" };
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
