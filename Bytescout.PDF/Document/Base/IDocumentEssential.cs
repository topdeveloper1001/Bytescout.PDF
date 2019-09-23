namespace Bytescout.PDF
{
    internal interface IDocumentEssential
    {
        Page GetPage(PDFDictionary pageDictionary);
        Page GetPage(int index);
        PDFArray GetDestinationFromNames(string name);
        void AddField(Field field);
        void RemoveField(Field field);
        string AddFontAcroForm(Font fnt);
        PDFDictionary GetAcroFormFont(string name);
        PDFString GetAcroFormDefaultAttribute();
    }
}
