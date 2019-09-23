using System.Drawing;

namespace Bytescout.PDF
{
    internal static class CloneUtility
    {
        internal static PDFArray CopyArrayCoordinates(PDFArray source, RectangleF oldPageRect, RectangleF newPageRect, bool oldPageIsNull)
        {
            PDFArray arr = new PDFArray();
            for (int i = 0; i < source.Count; ++i)
            {
                PDFNumber num = source[i] as PDFNumber;
                if (num == null)
                    arr.AddItem(new PDFNumber(0));
                else
                {
                    double val = num.GetValue();
                    if (i % 2 == 0)//x
                    {
                        if (oldPageIsNull)
                            arr.AddItem(new PDFNumber(val + newPageRect.Left));
                        else
                            arr.AddItem(new PDFNumber(-oldPageRect.Left + val + newPageRect.Left));
                    }
                    else//y
                    {
                        if (oldPageIsNull)
                            arr.AddItem(new PDFNumber(newPageRect.Bottom - val));
                        else
                            arr.AddItem(new PDFNumber(newPageRect.Bottom - (oldPageRect.Bottom - val)));
                    }
                }
            }

            return arr;
        }
    }
}
