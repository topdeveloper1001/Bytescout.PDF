namespace Bytescout.PDF
{
    internal class DocumentEssential: IDocumentEssential
    {
	    private readonly Catalog _catalog;

	    internal DocumentEssential(Catalog catalog)
        {
            _catalog = catalog;
        }

        public Page GetPage(PDFDictionary pageDictionary)
        {
            return _catalog.Pages.GetPage(pageDictionary);
        }

        public Page GetPage(int index)
        {
            if (index >= _catalog.Pages.Count || index < 0)
                return null;
            return _catalog.Pages[index];
        }

        public PDFArray GetDestinationFromNames(string name)
        {
            IPDFObject dest = _catalog.Names.Destinations.GetItem(name);
            if (dest != null)
            {
                if (dest is PDFArray)
                    return dest as PDFArray;
                else if(dest is PDFDictionary)
                    return (dest as PDFDictionary)["D"] as PDFArray;
            }
            return null;
        }

        public void AddField(Field field)
        {
            _catalog.AcroForm.AddField(field);
        }

        public void RemoveField(Field field)
        {
            _catalog.AcroForm.RemoveField(field.Dictionary);
        }

        public string AddFontAcroForm(Font font)
        {
            return _catalog.AcroForm.Resources.AddResources(ResourceType.Font, font.BaseFont.GetDictionary());
        }

        public PDFDictionary GetAcroFormFont(string name)
        {
            return _catalog.AcroForm.Resources.GetResource(name, ResourceType.Font) as PDFDictionary;
        }

        public PDFString GetAcroFormDefaultAttribute()
        {
            return _catalog.AcroForm.DefaultAttribute;
        }
    }
}
