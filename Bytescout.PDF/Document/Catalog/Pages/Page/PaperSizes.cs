using System.Drawing;

namespace Bytescout.PDF
{
    internal class PaperSizes
    {
        public static RectangleF GetRect(PaperFormat format, PaperOrientation orientation)
        {
            RectangleF rect = new RectangleF();
            switch (format)
            {
                case PaperFormat.A0:
                    rect.Size = new SizeF(2383.94f, 3370.39f);
                    break;
                case PaperFormat.A1:
                    rect.Size = new SizeF(1683.78f, 2383.94f);
                    break;
                case PaperFormat.A2:
                    rect.Size = new SizeF(1190.55f, 1683.78f);
                    break;
                case PaperFormat.A3:
                    rect.Size = new SizeF(841.89f, 1190.55f);
                    break;
                case PaperFormat.A4:
                    rect.Size = new SizeF(595.28f, 841.89f);
                    break;
                case PaperFormat.A5:
                    rect.Size = new SizeF(419.53f, 595.28f);
                    break;
                case PaperFormat.A6:
                    rect.Size = new SizeF(297.64f, 419.53f);
                    break;
                case PaperFormat.B4:
                    rect.Size = new SizeF(708.66f, 1000.63f);
                    break;
                case PaperFormat.B4Envelope:
                    rect.Size = new SizeF(708.66f, 1000.63f);
                    break;
                case PaperFormat.B5:
                    rect.Size = new SizeF(498.9f, 708.66f);
                    break;
                case PaperFormat.B5Envelope:
                    rect.Size = new SizeF(498.9f, 708.66f);
                    break;
                case PaperFormat.C6Envelope:
                    rect.Size = new SizeF(323.15f, 459.21f);
                    break;
                case PaperFormat.DLEnvelope:
                    rect.Size = new SizeF(311.81f, 623.62f);
                    break;
                case PaperFormat.Executive:
                    rect.Size = new SizeF(522, 756);
                    break;
                case PaperFormat.Folio:
                    rect.Size = new SizeF(612, 936);
                    break;
                case PaperFormat.Legal:
                    rect.Size = new SizeF(612, 1008);
                    break;
                case PaperFormat.Letter:
                    rect.Size = new SizeF(612, 792);
                    break;
                case PaperFormat.MonarchEnvelope:
                    rect.Size = new SizeF(279, 540);
                    break;
                case PaperFormat.Statement:
                    rect.Size = new SizeF(396, 612);
                    break;
            }

            if (orientation == PaperOrientation.Landscape)
                rect.Size = new SizeF(rect.Height, rect.Width);

            return rect;
        }
    }
}
