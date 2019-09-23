namespace Bytescout.PDF
{
    internal class BoxColorInfoBase
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary newDict = new PDFDictionary();
            string[] styles = { "CropBox", "BleedBox", "TrimBox", "ArtBox" };
            for (int i = 0; i < styles.Length; ++i)
            {
                PDFDictionary style = dict[styles[i]] as PDFDictionary;
                if (style != null)
                    newDict.AddItem(styles[i], BoxStyleBase.Copy(style));
            }

            return newDict;
        }
    }
}
