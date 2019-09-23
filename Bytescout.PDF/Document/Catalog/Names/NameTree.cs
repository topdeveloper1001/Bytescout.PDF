using System;

namespace Bytescout.PDF
{
    internal class NameTree: TreeBase
    {
        internal NameTree()
        {
            NodeType = "Names";
        }

        internal NameTree(PDFDictionary dict)
            : base(dict)
        {
            NodeType = "Names";
        }

        protected override bool isHitInRange(PDFArray limits, object key)
        {
			string name = key as string;
            
			if (name == null)
                throw new ArgumentException("Key must have a string type for NameTree.");
			
            PDFString first = limits[0] as PDFString;
            PDFString last = limits[1] as PDFString;
            if (first != null && last != null)
            {
                int comp1 = String.Compare(first.GetValue(), name, StringComparison.Ordinal);
                int comp2 = String.Compare(last.GetValue(), name, StringComparison.Ordinal);

                if (comp1 <= 0 && comp2 >= 0)
                    return true;
            }

            return false;
        }

        protected override IPDFObject findItem(PDFArray array, object key)
        {
            if (array == null)
                return null;

            string name = (string)key;

            for (int i = 0; i < array.Count; i += 2)
            {
                PDFString val = array[i] as PDFString;
                if (val != null && val.GetValue() == name)
                    return array[i + 1];
            }

            return null;
        }
        
    }
}
