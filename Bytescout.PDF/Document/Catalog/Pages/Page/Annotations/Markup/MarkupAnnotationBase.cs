namespace Bytescout.PDF
{
    internal static class MarkupAnnotationBase
    {
        internal static void CopyTo(PDFDictionary sourceDict, PDFDictionary destinationDict)
        {
            string[] keys = { "Open", "T", "CA", "CreationDate", "Subj", "RT", "RC" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = sourceDict[keys[i]];
                if (obj != null)
                    destinationDict.AddItem(keys[i], obj.Clone());
            }

            //Popup - need set after adding
            //IRT, ExData - TODO
        }
    }
}
