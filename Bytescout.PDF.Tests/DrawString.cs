using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class TestDrawString
    {
	    private const string TestDataPathRelative = "..\\..\\..\\..\\PDF Extractor SDK Tests";

	    public string TestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, TestDataPathRelative)); } }
	    public string OutputFolder { get { return TestContext.CurrentContext.TestDirectory; } }

		[SetUp]
		public void TestInitialize()
		{
		}

		[TearDown]
		public void TestCleanup()
		{
		}

        [Test]
        public void TestRise()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;
	        SolidBrush brush = new SolidBrush();
            Font font = new Font("Arial", 16);
            
            float x = 0;
            StringFormat sf = new StringFormat();
            canvas.DrawString("Test string", font, brush, x, 0, sf);
            sf.Rise = -font.Size / 2;
            x += font.GetTextWidth("Test string");
            canvas.DrawString("Test string rise -8", font, brush, x, 0, sf);
            sf.Rise = -font.Size / 4;
            x += font.GetTextWidth("Test string rise -8");
            canvas.DrawString("Test string rise -4", font, brush, x, 0, sf);
            x += font.GetTextWidth("Test string rise -4");
            Pen pen = new SolidPen();
            canvas.DrawLine(pen, 0, font.Size * 1.5f + 3, x, font.Size * 1.5f + 3);
            font.Size = 10;
            canvas.DrawString("A line for visiblity", font, brush, x, font.Size * 1.5f + 5);
            document.Save(OutputFolder + @"\TestRiseString.pdf");
			document.Dispose();
        }

        [Test]
        public void TestWordSpace()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;
	        SolidBrush brush = new SolidBrush();
            Font fontStandart = new Font(StandardFonts.Helvetica, 16);
            Font fontWithFile = new Font("Helvetica", 16);
            
            StringFormat sf = new StringFormat();
            canvas.DrawString("Test string text for standart font(Word Space = 0.0)", fontStandart, brush, 0, 0, sf);
            sf.WordSpacing = 10.0f;
            canvas.DrawString("Test string text for standart font(Word Space = 10.0)", fontStandart, brush, 0, fontStandart.Size + 5, sf);
            sf.WordSpacing = 0.0f;
            canvas.DrawString("Test string text for font with file(Word Space = 0.0)", fontWithFile, brush, 0, fontWithFile.Size * 2 + 5 * 2, sf);
            sf.WordSpacing = 10.0f;
            canvas.DrawString("Test string text for font with file(Word Space = 10.0)", fontWithFile, brush, 0, fontWithFile.Size * 3 + 5 * 3, sf);
            document.Save(OutputFolder + @"\TestWordSpaceString.pdf");
			document.Dispose();
        }

        [Test]
        public void TestCharSpace()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;
	        SolidBrush brush = new SolidBrush();
            Font fontStandart = new Font(StandardFonts.Helvetica, 16);
            Font fontWithFile = new Font("Helvetica", 16);
            
            StringFormat sf = new StringFormat();
            canvas.DrawString("Test string text for standart font(Char Space = 0.0)", fontStandart, brush, 0, 0, sf);
            sf.CharacterSpacing = 3.0f;
            canvas.DrawString("Test string text for standart font(Char Space = 5.0)", fontStandart, brush, 0, fontStandart.Size + 5, sf);
            sf.CharacterSpacing = 0.0f;
            canvas.DrawString("Test string text for font with file(Char Space = 0.0)", fontWithFile, brush, 0, (fontStandart.Size + 5) * 2, sf);
            sf.CharacterSpacing = 3.0f;
            canvas.DrawString("Test string text for font with file(Char Space = 5.0)", fontWithFile, brush, 0, (fontStandart.Size + 5) * 3, sf);
            document.Save(OutputFolder + @"\TestCharSpaceString.pdf");
			document.Dispose();
        }

        [Test]
        public void TestScale()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;
	        SolidBrush brush = new SolidBrush();
            Font fontStandart = new Font(StandardFonts.Helvetica, 16);
            Font fontWithFile = new Font("Helvetica", 16);
            
            StringFormat sf = new StringFormat();
            canvas.DrawString("Test string text for standart font(Scale = 100%)", fontStandart, brush, 0, 0, sf);
            sf.Scaling = 50;
            canvas.DrawString("Test string text for standart font(Scale = 50%) Test string text for standart font(Scale = 50%)", fontStandart, brush, 0, fontStandart.Size + 5, sf);
            sf.Scaling = 100;
            canvas.DrawString("Test string text for font with file(Scale = 100%)", fontWithFile, brush, 0, (fontStandart.Size + 5) * 2, sf);
            sf.Scaling = 50;
            canvas.DrawString("Test string text for font with file(Scale = 50%) Test string text for font with file(Scale = 50%)", fontWithFile, brush, 0, (fontStandart.Size + 5) * 3, sf);
            document.Save(OutputFolder + @"\TestScaleString.pdf");
			document.Dispose();
        }

        [Test]
        public void TestStatusText()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;
	        SolidBrush brush = new SolidBrush(new ColorRGB(150, 150, 0));
            SolidPen pen = new SolidPen(new ColorRGB(200, 0, 0));
            Font fontStandart11 = new Font(StandardFonts.Helvetica, 8, true, true);
            Font fontStandart01 = new Font(StandardFonts.Helvetica, 8, false, true);
            Font fontStandart10 = new Font(StandardFonts.Helvetica, 8, true, false);
            Font fontStandart00 = new Font(StandardFonts.Helvetica, 8, false, false);

            List<Font> list_fontsWF = new List<Font>();
            for (int i = 0; i < 16; ++i)
            {
                list_fontsWF.Add(new Font("Helvetica", i+12, (((i >> 3) & 1) % 2 == 0 ? false : true), (((i >> 2) & 1) % 2 == 0 ? false : true), (((i >> 1) & 1) % 2 == 0 ? false : true), ((i & 1) % 2 == 0 ? false : true)));
            }
            fontStandart00.Size = 32;
            fontStandart01.Size = 24;
            fontStandart10.Size = 24;
            fontStandart11.Size = 24;
            canvas.DrawString("Normalize text", fontStandart00, brush, pen, 50, 0);
            canvas.DrawString("Underline text", fontStandart10, brush, 50, fontStandart00.Size * 1.5f);
            canvas.DrawString("Strokeout text", fontStandart01, pen, 50, fontStandart00.Size * 3.0f);
            canvas.DrawString("Strokeout and underline text", fontStandart11, brush, 50, fontStandart00.Size * 4.5f);

            Font[] fonts = list_fontsWF.ToArray();
            for (int i = 0; i < 16; ++i)
            {
                canvas.DrawString("Test string text", fonts[i], brush, 50, fontStandart00.Size * 6.0f + (fonts[i].Size + 10) * i);
            }

            document.Save(OutputFolder + @"\TestStatusString.pdf");
			document.Dispose();
        }

        [Test]
        public void TestAlignInRectangle()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;

	        Font font = new Font(StandardFonts.Courier, 8, false, false);
            StringFormat sf = new StringFormat();
            Brush brush = new SolidBrush();
            SolidPen pen = new SolidPen(new ColorRGB(0, 255, 0));
            pen.Opacity = 20;
            string str = "The test string";
            canvas.DrawString("Font = PDFBuiltInFont.Courier", font, brush, 50, 10);
            canvas.DrawString("Font = ByFontName(\"Arial\")", font, brush, 300, 10);
            for (int i = 0; i < 2; ++i)
            {
                if (i == 1)
                {
                    font = new Font("Arial", 8);
                    str = "Test test string. ";
                    str += str;
                    str += str;
                }
                canvas.DrawString("HAlign", font, brush, 10 + i * 250, 40);
                canvas.DrawRectangle(pen, 10 + i * 250, 40, 40, 10);
                canvas.DrawString("VAlign", font, brush, 10 + i * 250, 50);
                canvas.DrawRectangle(pen, 10 + i * 250, 50, 40, 10);
                canvas.DrawString("Left", font, brush, 50 + i * 250, 40);
                canvas.DrawRectangle(pen, 50 + i * 250, 40, 50, 10);
                canvas.DrawString("Center", font, brush, 100 + i * 250, 40);
                canvas.DrawRectangle(pen, 100 + i * 250, 40, 50, 10);
                canvas.DrawString("Width", font, brush, 150 + i * 250, 40);
                canvas.DrawRectangle(pen, 150 + i * 250, 40, 50, 10);
                canvas.DrawString("Right", font, brush, 200 + i * 250, 40);
                canvas.DrawRectangle(pen, 200 + i * 250, 40, 50, 10);
                canvas.DrawString("Top", font, brush, 10 + i * 250, 75);
                canvas.DrawRectangle(pen, 10 + i * 250, 60, 40, 40);
                canvas.DrawString("Center", font, brush, 10 + i * 250, 125);
                canvas.DrawRectangle(pen, 10 + i * 250, 100, 40, 50);
                canvas.DrawString("Bottom", font, brush, 10 + i * 250, 175);
                canvas.DrawRectangle(pen, 10 + i * 250, 150, 40, 50);
                sf.VerticalAlign = VerticalAlign.Top;
                sf.HorizontalAlign = HorizontalAlign.Left;
                canvas.DrawString(str, font, brush, new RectangleF(50 + i * 250, 50, 50, 50), sf);
                canvas.DrawRectangle(pen, 50 + i * 250, 50, 50, 50);
                sf.HorizontalAlign = HorizontalAlign.Center;
                canvas.DrawString(str, font, brush, new RectangleF(100 + i * 250, 50, 50, 50), sf);
                canvas.DrawRectangle(pen, 100 + i * 250, 50, 50, 50);
                sf.HorizontalAlign = HorizontalAlign.Justify;
                canvas.DrawString(str, font, brush, new RectangleF(150 + i * 250, 50, 50, 50), sf);
                canvas.DrawRectangle(pen, 150 + i * 250, 50, 50, 50);
                sf.HorizontalAlign = HorizontalAlign.Right;
                canvas.DrawString(str, font, brush, new RectangleF(200 + i * 250, 50, 50, 50), sf);
                canvas.DrawRectangle(pen, 200 + i * 250, 50, 50, 50);
                sf.VerticalAlign = VerticalAlign.Center;
                sf.HorizontalAlign = HorizontalAlign.Left;
                canvas.DrawString(str, font, brush, new RectangleF(50 + i * 250, 100, 50, 50), sf);
                canvas.DrawRectangle(pen, 50 + i * 250, 100, 50, 50);
                sf.HorizontalAlign = HorizontalAlign.Center;
                canvas.DrawString(str, font, brush, new RectangleF(100 + i * 250, 100, 50, 50), sf);
                canvas.DrawRectangle(pen, 100 + i * 250, 100, 50, 50);
                sf.HorizontalAlign = HorizontalAlign.Justify;
                canvas.DrawString(str, font, brush, new RectangleF(150 + i * 250, 100, 50, 50), sf);
                canvas.DrawRectangle(pen, 150 + i * 250, 100, 50, 50);
                sf.HorizontalAlign = HorizontalAlign.Right;
                canvas.DrawString(str, font, brush, new RectangleF(200 + i * 250, 100, 50, 50), sf);
                canvas.DrawRectangle(pen, 200 + i * 250, 100, 50, 50);
                sf.VerticalAlign = VerticalAlign.Bottom;
                sf.HorizontalAlign = HorizontalAlign.Left;
                canvas.DrawString(str, font, brush, new RectangleF(50 + i * 250, 150, 50, 50), sf);
                canvas.DrawRectangle(pen, 50 + i * 250, 150, 50, 50);
                sf.HorizontalAlign = HorizontalAlign.Center;
                canvas.DrawString(str, font, brush, new RectangleF(100 + i * 250, 150, 50, 50), sf);
                canvas.DrawRectangle(pen, 100 + i * 250, 150, 50, 50);
                sf.HorizontalAlign = HorizontalAlign.Justify;
                canvas.DrawString(str, font, brush, new RectangleF(150 + i * 250, 150, 50, 50), sf);
                canvas.DrawRectangle(pen, 150 + i * 250, 150, 50, 50);
                sf.HorizontalAlign = HorizontalAlign.Right;
                canvas.DrawString(str, font, brush, new RectangleF(200 + i * 250, 150, 50, 50), sf);
                canvas.DrawRectangle(pen, 200 + i * 250, 150, 50, 50);
            }

            document.Save(OutputFolder + @"\TestStringInRectangle.pdf");
			document.Dispose();
        }

        [Test]
        public void TestStateInRectangle()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        document.Save(OutputFolder + @"\TestStateInRectangle.pdf");
			document.Dispose();
        }

		[Test]
		public void TestMeasureString()
		{
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));
			
			Canvas canvas = document.Pages[0].Canvas;
			Font font = new Font("Times New Roman", 26);

			SizeF size = canvas.MeasureString("Test test test", font);
			Assert.AreEqual(129.766f, size.Width, float.Epsilon);
			Assert.AreEqual(23.166f, size.Height, float.Epsilon);

			document.Dispose();
		}
    }
}
