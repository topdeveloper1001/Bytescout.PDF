using System;
using System.Diagnostics;
using System.Drawing.Text;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class TestFonts
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
        public void TestSystemFonts()
        {
	        Document document = new Document();
	        document.Pages.Add(new Page(500, 610));
	        Canvas canvas = document.Pages[0].Canvas;
	        
			InstalledFontCollection fc = new InstalledFontCollection();

	        float y = 10;
            for (int i = 0; i < fc.Families.Length; ++i)
            {
                if ((i + 1) % 10 == 0)
                {
                    document.Pages.Add(new Page(500, 610));
                    canvas = document.Pages[document.Pages.Count - 1].Canvas;
                    y = 10;
                }

                Font fnt = new Font(fc.Families[i].Name, 12);

                Font tmp = new Font(StandardFonts.HelveticaBold, 8, true, false);
                canvas.DrawString(fc.Families[i].Name, tmp, new SolidBrush(new ColorRGB(123, 0, 198)), 10, y);
                y += 10;

                canvas.DrawString("The quick brown fox jumps over, the lazy dog.", fnt, new SolidBrush(), 10, y);
                y += 15;
                canvas.DrawString("Съешь еще этих мягких французских булок, да выпей чаю.", fnt, new SolidBrush(), 10, y);
                y += 35;
            }

            document.Save(OutputFolder + @"\TestSystemFonts.pdf");
			document.Dispose();

			//Process.Start("TestSystemFonts.pdf");
        }

        [Test]
        public void TestANSIUnicodeCJKFonts()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
			Canvas canvas = document.Pages[0].Canvas;

            Brush brush = new SolidBrush();

            Font font = new Font("Arial", 12);

            canvas.DrawString("English -- Hello, World!", font, brush, 10, 10);
            canvas.DrawString("Russian -- Привет, Мир!", font, brush, 10, 30);
            canvas.DrawString("French -- Vous êtes le Phénix des hôtes de ces bois.", font, brush, 10, 50);
            canvas.DrawString("German -- In ihrem Aufgabenbereich lag die komplette Organisation des Sekretariat des Geschäftsführers", font, brush, 10, 70);
            canvas.DrawString("Spanish -- Texto de ejemplo", font, brush, 10, 90);
            canvas.DrawString("Arabic -- لوميا لاوج مداخ ىلا حاجنب لخد ديدج مدختسم", font, brush, 10, 110);
            canvas.DrawString("Turkish  -- Yukarda mavi gök, asağıda yağız yer yaratıldıkta", font, brush, 10, 130);

            font = new Font("Gulim", 12);
            canvas.DrawString("Korean -- 저쪽 갈밭머리에 갈꽃이 한 옴큼 움직였다. 소녀가 갈꽃을 안고 있었다. 그리고 이제는 천천한 걸음이었다. 유난히 맑은 가을 햇살이 소녀의 갈꽃머리에서 반짝거렸다. 소녀 아닌 갈꽃이 들길을 걸어가는 것만 같았다.", font, brush, 10, 150);
            canvas.DrawString("Chinese -- 久有归天愿", font, brush, 10, 180);

            font = new Font("MS Gothic", 12);
            canvas.DrawString("Japanese -- 道可道非常道，名可名非常名。", font, brush, 10, 210);

			document.Save(OutputFolder + @"\Fonts.pdf");
			document.Dispose();

			//Process.Start("Fonts.pdf");
        }
    }

}