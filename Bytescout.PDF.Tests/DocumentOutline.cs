using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class TestDocumentOutline
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
        public void TestAddOutlines()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
            document.PageMode = PageMode.Outlines;

            Outline first = new Outline("First");
            Outline tmp = new Outline("Bold Title");

            tmp.Bold = true;
            first.Kids.Add(tmp);

            tmp.Bold = false;
            tmp.Italic = true;
            tmp.Title = "Italic Title";
            first.Kids.Add(tmp);

            tmp.Bold = true;
            tmp.Title = "Bold Italic Title";
            first.Kids.Add(tmp);
            document.Outlines.Add(first);

            Outline second = new Outline("Second");
            tmp.Bold = false;
            tmp.Italic = false;
            tmp.Title = "Red";
            tmp.Color = new ColorRGB(255, 0, 0);
            second.Kids.Add(tmp);

            tmp.Title = "Green";
            tmp.Color = new ColorRGB(0, 255, 0);
            second.Kids.Add(tmp);

            tmp.Title = "blue";
            tmp.Color = new ColorRGB(0, 0, 255);
            second.Kids.Add(tmp);
            document.Outlines.Add(second);

            Outline third = new Outline("Hidden Children");
            for (int i = 0; i < 10; ++i)
                third.Kids.Add(new Outline("Kid " + (i + 1).ToString()));
            third.HideChildren = true;
            document.Outlines.Add(third);

            document.Save(OutputFolder + @"\TestAddOutlines.pdf");
			document.Dispose();

			//Process.Start("TestAddOutlines.pdf");
        }

        [Test]
        public void TestOutlinesMode()
        {
			Document document = new Document();
            document.PageMode = PageMode.Outlines;

            Font fnt = new Font(StandardFonts.Courier, 14);
            SolidBrush br = new SolidBrush();
            for (int i = 0; i < 10; ++i)
            {
                Page page = new Page(PaperFormat.A4);
                page.Canvas.DrawString("Page " + (i + 1).ToString(), fnt, br, 250, 50);
                document.Pages.Add(page);
            }

            Outline outline = new Outline("Page=2 FitBounding");
            Destination dest = new Destination(document.Pages[1]);
            dest.SetFitBounding();
            outline.Destination = dest;
            document.Outlines.Add(outline);

            outline = new Outline("Page=3 FitBoundingHorizontal top=50");
            dest = new Destination(document.Pages[2]);
            dest.SetFitBoundingHorizontal(50);
            outline.Destination = dest;
            document.Outlines.Add(outline);

            outline = new Outline("Page=4 FitBoundingVertical left=250");
            dest = new Destination(document.Pages[3]);
            dest.SetFitBoundingVertical(250);
            outline.Destination = dest;
            document.Outlines.Add(outline);

            outline = new Outline("Page=5 FitHorizontal top=50");
            dest = new Destination(document.Pages[4]);
            dest.SetFitHorizontal(50);
            outline.Destination = dest;
            document.Outlines.Add(outline);

            outline = new Outline("Page=6 FitPage");
            dest = new Destination(document.Pages[5]);
            dest.SetFitPage();
            outline.Destination = dest;
            document.Outlines.Add(outline);

            outline = new Outline("Page=7 SetFitRectangle left=250 top=50 width=100 height=100");
            dest = new Destination(document.Pages[6]);
            dest.SetFitRectangle(250, 50, 100, 100);
            outline.Destination = dest;
            document.Outlines.Add(outline);

            outline = new Outline("Page=8 SetFitVertical left=250");
            dest = new Destination(document.Pages[7]);
            dest.SetFitVertical(250);
            outline.Destination = dest;
            document.Outlines.Add(outline);

            outline = new Outline("Page=9 SetFitXYZ left=250 top=50 zoom=90");
            dest = new Destination(document.Pages[8]);
            dest.SetFitXYZ(250, 50, 90);
            outline.Destination = dest;
            document.Outlines.Add(outline);

            document.Save(OutputFolder + @"\TestOutlinesMode.pdf");
			document.Dispose();

			//Process.Start("TestOutlinesMode.pdf");
        }
    }
}
