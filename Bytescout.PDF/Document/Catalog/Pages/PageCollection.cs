using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class PageCollection : IEnumerable<Page>
#else
	/// <summary>
    /// Represents a collection of pages of a document.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[DebuggerDisplay("Count = {Count}")]
    public class PageCollection : IEnumerable<Page>
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private readonly IDocumentEssential _owner;
	    private readonly List<Page> _pages = new List<Page>();

	    /// <summary>
	    /// Gets the element at the specified index.
	    /// </summary>
	    /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the element to get.</param>
	    /// <returns cref="Page">The Bytescout.PDF.Page with specified index.</returns>
	    public Page this[int index]
	    {
		    get
		    {
			    return _pages[index];
		    }
	    }

	    /// <summary>
	    /// Gets the number of pages in the collection.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Count
	    {
		    get { return _pages.Count; }
	    }

	    internal PageCollection(IDocumentEssential owner)
        {
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Pages"));
            _dictionary.AddItem("Count", new PDFNumber(0));
            _dictionary.AddItem("Kids", new PDFArray());
            _owner = owner;
        }

        internal PageCollection(PDFDictionary dict, IDocumentEssential owner)
        {
            _dictionary = dict;
            _owner = owner;
            parseKids(dict);
        }

	    /// <summary>
        /// Adds the page to the end of the collection of document pages.
        /// </summary>
        /// <param name="page">Page to be added to the end of the collection.</param>
        public void Add(Page page)
        {
            insertPage(Count, page);
        }

        /// <summary>
        /// Inserts a page at the specified index to the collection of document pages.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index at which page to be inserted.</param>
        /// <param name="page">Page to insert.</param>
        public void Insert(int index, Page page)
        {
            insertPage(index, page);
        }

        /// <summary>
        /// Removes the PDF page with the specified index from the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the PDF page to be removed.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            ((PDFArray) _dictionary["Kids"]).RemoveItem(index);
            _pages.RemoveAt(index);
            _dictionary.AddItem("Count", new PDFNumber(Count));
        }

        internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        internal Page GetPage(PDFDictionary pageDict)
        {
            PDFArray kids = _dictionary["Kids"] as PDFArray;
            int index = -1;
            for (int i = 0; i < kids.Count; ++i)
            {
                if (kids[i] == pageDict)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                return null;
            return this[index];
        }

        internal Page FindPageByID(string id)
        {
            for (int i = 0; i < _pages.Count; ++i)
            {
                if (_pages[i].PageID == id)
                    return _pages[i];
            }
            return null;
        }

        private void insertPage(int index, Page page)
        {
            if (page == null)
                throw new ArgumentNullException("page");
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            Page newPage = page.Clone(_owner);

            ((PDFArray) _dictionary["Kids"]).Insert(index, newPage.GetDictionary());
            _pages.Insert(index, newPage);
            newPage.Parent = this;
            _dictionary.AddItem("Count", new PDFNumber(Count));
        }

        private void parseKids(PDFDictionary dict)
        {
            IPDFObject resources = dict["Resources"];
            IPDFObject mediaBox = dict["MediaBox"];
            IPDFObject cropBox = dict["CropBox"];
            IPDFObject rotate = dict["Rotate"];

            PDFArray kids = _dictionary["Kids"] as PDFArray;
            PDFArray newKids = new PDFArray();

            readPages(kids, newKids, resources, mediaBox, cropBox, rotate);

            dict.RemoveItem("Resources");
            dict.RemoveItem("MediaBox");
            dict.RemoveItem("CropBox");
            dict.RemoveItem("Rotate");

            dict.AddItem("Kids", newKids);
        }

        private void readPages(PDFArray kids, PDFArray newKids, IPDFObject resources, IPDFObject mediaBox, IPDFObject cropBox, IPDFObject rotate)
        {
            if (kids != null)
            {
                for (int i = 0; i < kids.Count; ++i)
                {
                    PDFDictionary dict = kids[i] as PDFDictionary;
                    if (dict != null)
                    {
                        PDFName type = dict["Type"] as PDFName;
                        if (type != null)
                        {
                            if (type.GetValue() == "Pages")
                            {
                                IPDFObject tmp = dict["Resources"];
                                if (tmp != null)
                                    resources = tmp;

                                tmp = dict["MediaBox"];
                                if (tmp != null)
                                    mediaBox = tmp;
                                
                                tmp = dict["CropBox"];
                                if (tmp != null)
                                    cropBox = tmp;

                                tmp = dict["Rotate"];
                                if (tmp != null)
                                    rotate = tmp;

                                readPages(dict["Kids"] as PDFArray, newKids, resources, mediaBox, cropBox, rotate);
                            }
                            else if (type.GetValue() == "Page")
                            {
                                newKids.AddItem(dict);
                                dict.AddItem("Parent", GetDictionary());
                                dict.AddItemIfNotHave("Resources", resources);
                                dict.AddItemIfNotHave("MediaBox", mediaBox);
                                dict.AddItemIfNotHave("CropBox", cropBox);
                                dict.AddItemIfNotHave("Rotate", rotate);

                                Page page = new Page(dict, _owner);
                                _pages.Add(page);
                            }
                        }
                    }
                }
            }
        }

		[ComVisible(false)]
	    public IEnumerator<Page> GetEnumerator()
	    {
		    return _pages.GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }
    }
}
