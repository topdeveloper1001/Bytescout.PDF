namespace Bytescout.PDF
{
    internal class PageBase
    {
        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary newDict = new PDFDictionary();
            newDict.AddItem("Type", new PDFName("Page"));

            string[] keys = { "LastModified", "Rotate", "Dur", "Metadata", "ID", "PZ", "Tabs", "UserUnit" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = dict[keys[i]];
                if (obj != null)
                    newDict.AddItem(keys[i], obj.Clone());
            }

            PDFDictionary resources = dict["Resources"] as PDFDictionary;
            if (resources != null)
                newDict.AddItem("Resources", ResourcesBase.Copy(resources));

            string[] boxes = { "MediaBox", "CropBox", "BleedBox", "TrimBox", "ArtBox" };
            for (int i = 0; i < boxes.Length; ++i)
            {
                PDFArray bbox = dict[boxes[i]] as PDFArray;
                if (bbox != null)
                {
                    PDFArray newBbox = new PDFArray();
                    for (int j = 0; j < bbox.Count; ++j)
                    {
                        PDFNumber num = bbox[j] as PDFNumber;
                        newBbox.AddItem(num.Clone());
                    }
                    newDict.AddItem(boxes[i], newBbox);
                }
            }

            PDFDictionary boxColorInfo = dict["BoxColorInfo"] as PDFDictionary;
            if (boxColorInfo != null)
                newDict.AddItem("BoxColorInfo", BoxColorInfoBase.Copy(boxColorInfo));

            IPDFObject contents = dict["Contents"];
            if (contents != null)
            {
                IPDFObject newContents = null;
                if (contents is PDFDictionaryStream)
                    newContents = contents.Clone();
                else if (contents is PDFArray)
                {
                    newContents = new PDFArray();
                    for (int i = 0; i < (contents as PDFArray).Count; ++i)
                    {
                        PDFDictionaryStream contentsItem = (contents as PDFArray)[i] as PDFDictionaryStream;
                        if (contentsItem != null)
                            (newContents as PDFArray).AddItem(contentsItem.Clone());
                    }
                }
                newDict.AddItem("Contents", newContents);
            }

            PDFDictionary group = dict["Group"] as PDFDictionary;
            if (group != null)
                newDict.AddItem("Group", GroupBase.Copy(group));

            PDFDictionaryStream thumb = dict["Thumb"] as PDFDictionaryStream;
            if (thumb != null)
                newDict.AddItem("Thumb", thumb);

            PDFDictionary trans = dict["Trans"] as PDFDictionary;
            if (trans != null)
                newDict.AddItem("Trans", TransitionBase.Copy(trans));

            // Parent, B, StructParents - do not
            // Annots, AA - need set after adding page
            // PieceInfo, SeparationInfo, TemplateInstantiated, PresSteps, VP - still unknown

            return newDict;
        }
    }
}
