namespace Bytescout.PDF
{
    internal static class AnnotationBase
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary newDict = new PDFDictionary();
            string[] keys = { "Type", "Subtype", "Rect", "Contents", "NM", "M", "F", "Border", "C", "AS" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = dict[keys[i]];
                if (obj != null)
                    newDict.AddItem(keys[i], obj.Clone());
            }

            PDFDictionary ap = dict["AP"] as PDFDictionary;
            if (ap != null)
                newDict.AddItem("AP", copyAppearance(ap));

            // P, StructParent - do not
            // OS - still unknown

            return newDict;
        }

        private static PDFDictionary copyAppearance(PDFDictionary dict)
        {
            PDFDictionary result = new PDFDictionary();
            string[] keys = {"N", "R", "D" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = dict[keys[i]];
                if (obj is PDFDictionaryStream)
                {
                    result.AddItem(keys[i], GraphicsTemplate.Copy(obj as PDFDictionaryStream));
                }
                else if (obj is PDFDictionary)
                {
                    PDFDictionary newDict = new PDFDictionary();
                    string[] keys2 = (obj as PDFDictionary).GetKeys();
                    for (int j = 0; j < keys2.Length; ++j)
                    {
                        PDFDictionaryStream ds = (obj as PDFDictionary)[keys2[j]] as PDFDictionaryStream;
                        if (ds != null)
                            newDict.AddItem(keys2[j], GraphicsTemplate.Copy(ds));
                    }

                    result.AddItem(keys[i], newDict);
                }
            }

            return result;
        }
    }
}
