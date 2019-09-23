namespace Bytescout.PDF
{
    internal abstract class TreeBase
    {
	    protected PDFDictionary Dictionary;
	    protected string NodeType;

	    internal TreeBase()
        {
        }

        internal TreeBase(PDFDictionary dict)
        {
            Dictionary = dict;
        }

        internal PDFDictionary GetDictionary()
        {
            return Dictionary;
        }

        internal void Clear()
        {
            Dictionary.Clear();
        }

        internal IPDFObject GetItem(object key)
        {
            return getItem(Dictionary, key);
        }

        protected IPDFObject getItem(PDFDictionary dict, object key)
        {
            PDFArray values = dict[NodeType] as PDFArray;
            if (values != null)
                return findItem(values, key);

            PDFArray kids = dict["Kids"] as PDFArray;
            if (kids != null)
                return findKids(kids, key);

            return null;
        }

        protected IPDFObject findKids(PDFArray kids, object key)
        {
            for (int i = 0; i < kids.Count; ++i)
            {
                PDFDictionary dict = kids[i] as PDFDictionary;
                if (dict != null)
                {
                    PDFArray limits = dict["Limits"] as PDFArray;
                    if (limits != null)
                    {
                        if (isHitInRange(limits, key))
                            return getItem(dict, key);
                    }
                }
            }

            return null;
        }

        protected abstract bool isHitInRange(PDFArray limits, object key);
        protected abstract IPDFObject findItem(PDFArray array, object key);
    }
}
