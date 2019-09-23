using System;

namespace Bytescout.PDF
{
    internal class URLFileSpecification : FileSpecification
    {
        public URLFileSpecification(Uri uri)
        {
            GetDictionary().AddItem("FS", new PDFName("URL"));
            GetDictionary().AddItem("F", new PDFString(System.Text.Encoding.Default.GetBytes(uri.AbsoluteUri), false));
        }
    }
}
