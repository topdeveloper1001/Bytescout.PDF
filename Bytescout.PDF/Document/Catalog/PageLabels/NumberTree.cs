using System;
using System.Collections.Generic;

namespace Bytescout.PDF
{
    internal class NumberTree: TreeBase
    {
        internal NumberTree()
        {
            NodeType = "Nums";
        }

        internal NumberTree(PDFDictionary dict)
            : base(dict)
        {
            NodeType = "Nums";
        }

        public void InsertItem(IPDFObject key, IPDFObject value)
        {
            insertItem(Dictionary, key, value);
        }

        internal void Remove(IPDFObject key)
        {
            removeItem(Dictionary, key);
        }

        internal void GetAllLeafs(List<PageLabel> items)
        {
            getAllLeafs(Dictionary, items);
        }

        protected override bool isHitInRange(PDFArray limits, object key)
        {
            if (key.GetType() != System.Type.GetType("int"))
                throw new ArgumentException("Key must have a integer type for NumberTree.");

            int number = (int)key;

            PDFNumber first = limits[0] as PDFNumber;
            PDFNumber last = limits[1] as PDFNumber;
            if (first != null && last != null)
            {
                int comp1 = ((int)first.GetValue()).CompareTo(number);
                int comp2 = ((int)last.GetValue()).CompareTo(number);

                if (comp1 <= 0 && comp2 >= 0)
                    return true;
            }

            return false;
        }

        protected override IPDFObject findItem(PDFArray array, object key)
        {
            if (array == null)
                return null;

            int number = (int)key;

            for (int i = 0; i < array.Count; i += 2)
            {
                PDFNumber val = array[i] as PDFNumber;
                if (i + 2 < array.Count)
                {
                    PDFNumber val_next = array[i + 2] as PDFNumber;
                    if (val != null && val_next != null &&
                        ((int)val.GetValue()) <= number && number < ((int)val_next.GetValue()))
                        return array[i + 1];
                }
                else
                {
                    if (val != null && ((int)val.GetValue()) <= number)
                        return array[i + 1];
                }
            }

            return null;
        }

        private void getAllLeafs(PDFDictionary dict, List<PageLabel> items)
        {
            PDFArray values = dict[NodeType] as PDFArray;
            if (values != null)
            {
                int key;
                PDFDictionary value;
                for (int i = 0; i < values.Count; i += 2)
                { 
                    key = (int)(values[i] as PDFNumber).GetValue();
                    value = values[i + 1] as PDFDictionary;
                    items.Add(new PageLabel(key, value));
                }
            }

            PDFArray kids = dict["Kids"] as PDFArray;
            if (kids != null)
            {
                for (int i = 0; i < kids.Count; ++i)
                {
                    if (kids[i] as PDFDictionary != null)
                        getAllLeafs(kids[i] as PDFDictionary, items);
                }
            }
        }

        private void insertItem(PDFDictionary dict, IPDFObject key, IPDFObject value)
        {
            PDFArray values = dict[NodeType] as PDFArray;
            if (values != null)
            {
                addItem(values, key, value);
                return;
            }

            PDFArray kids = dict["Kids"] as PDFArray;
            if (kids != null)
            {
                insertIntoKids(kids, key, value);
                return;
            }

            addFirstItem(dict, key, value);
        }

        private void addFirstItem(PDFDictionary dict, IPDFObject key, IPDFObject value)
        {
            PDFArray nums = new PDFArray();
            nums.AddItem(key);
            nums.AddItem(value);
            dict.AddItem(NodeType, nums);
        }

        private void addItem(PDFArray array, IPDFObject key, IPDFObject value)
        {
            if (array == null)
                return;

            int number = (int)(key as PDFNumber).GetValue();

            for (int i = 0; i < array.Count; i += 2)
            {
                PDFNumber val = array[i] as PDFNumber;

                if (i + 2 < array.Count)
                {
                    PDFNumber val_next = array[i + 2] as PDFNumber;

                    if (val != null && val_next != null &&
                        ((int)val.GetValue()) <= number && number < ((int)val_next.GetValue()))
                    {
                        array.Insert(i + 2, key);
                        array.Insert(i + 3, value);
                        return;
                    }
                }
                else
                {
                    if (val != null && ((int)val.GetValue()) <= number)
                    {
                        array.AddItem(key);
                        array.AddItem(value);
                        return;
                    }
                    else
                    {
                        array.Insert(i, key);
                        array.Insert(i + 1, value);
                        return;
                    }
                }
            }
        }

        private void insertIntoKids(PDFArray kids, IPDFObject key, IPDFObject value)
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
                            insertItem(dict, key, value);
                    }
                }
            }
        }

        private void removeItem(PDFDictionary dict, IPDFObject key)
        {
            PDFArray values = dict[NodeType] as PDFArray;
            if (values != null)
            {
                deleteItem(values, key);
                return;
            }

            PDFArray kids = dict["Kids"] as PDFArray;
            if (kids != null)
            {
                removeFromKids(kids, key);
                return;
            }
        }

        private void deleteItem(PDFArray array, IPDFObject key)
        {
            if (array == null)
                return;

            int number = (int)(key as PDFNumber).GetValue();

            for (int i = 0; i < array.Count; i += 2)
            {
                PDFNumber val = array[i] as PDFNumber;
                if (val != null && ((int)val.GetValue()) == number)
                {
                    array.RemoveItem(i + 1);
                    array.RemoveItem(i);
                    return;
                }
            }
        }

        private void removeFromKids(PDFArray kids, IPDFObject key)
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
                            removeItem(dict, key);
                    }
                }
            }
        }

    }
}
