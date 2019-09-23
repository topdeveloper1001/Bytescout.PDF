using System;
using System.Diagnostics;
using System.Drawing;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class Annotations
    {
		private const string TestDataPathRelative = "..\\..\\..\\..\\PDF Extractor SDK Tests";
		private const string File1PathRelative = "Sources-Other\\Images\\two_pilots.bmp";

		public string TestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, TestDataPathRelative)); } }
	    public string OutputFolder { get { return TestContext.CurrentContext.TestDirectory; } }
		public string Bitmap1 { get { return System.IO.Path.Combine(TestData, File1PathRelative); } }

		[SetUp]
		public void TestInitialize()
		{
		}

		[TearDown]
		public void TestCleanup()
		{
		}

		[Test]
        public void TestCaretAnnotation()
        {
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));
            CaretAnnotation annot = new CaretAnnotation(10, 10, 20, 20);
            annot.Color = new ColorRGB(0, 0, 0);
            annot.Symbol = CaretSymbol.Paragraph;
            document.Pages[0].Annotations.Add(annot);
            annot = new CaretAnnotation(40, 30, 20, 20);
            annot.Color = new ColorRGB(0, 255, 0);
            annot.Symbol = CaretSymbol.None;
            document.Pages[0].Annotations.Add(annot);
            document.Save(OutputFolder + @"\TestCaretAnnotation.pdf");
			document.Dispose();
			
			//Process.Start("TestCaretAnnotation.pdf");
        }

		[Test]
        public void TestFileAttachmentAnnotation()
        {
            Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
			FileAttachmentAnnotation annot = new FileAttachmentAnnotation(Bitmap1, 10, 10, 20, 20);
            annot.Icon = FileAttachmentAnnotationIcon.Graph;
            annot.Color = new ColorRGB(0, 0, 0);
            document.Pages[0].Annotations.Add(annot);
			annot = new FileAttachmentAnnotation(Bitmap1, 10, 50, 20, 20);
            annot.Icon = FileAttachmentAnnotationIcon.Paperclip;
            annot.Color = new ColorRGB(0, 0, 0);
            document.Pages[0].Annotations.Add(annot);
			annot = new FileAttachmentAnnotation(Bitmap1, 50, 10, 20, 20);
            annot.Icon = FileAttachmentAnnotationIcon.PushPin;
            annot.Color = new ColorRGB(0, 0, 0);
            document.Pages[0].Annotations.Add(annot);
			annot = new FileAttachmentAnnotation(Bitmap1, 50, 50, 20, 20);
            annot.Icon = FileAttachmentAnnotationIcon.Tag;
            annot.Color = new ColorRGB(0, 0, 0);
            document.Pages[0].Annotations.Add(annot);
            document.Save(OutputFolder + @"\TestFileAttachmentAnnotation.pdf");
			document.Dispose();
			
			//Process.Start("TestFileAttachmentAnnotation.pdf");
        }

        [Test]
        public void TestFreeTextAnnotation()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
            FreeTextAnnotation annot = new FreeTextAnnotation(100, 100, 150, 100);
            annot.Contents = "Free text annotation";

			document.Pages[0].Annotations.Add(annot);
			document.Save(OutputFolder + @"\TestFreeTextAnnotation.pdf");
			document.Dispose();
			
			//Process.Start("TestFreeTextAnnotation.pdf");
        }

        [Test]
        public void TestPolygonAndPolylineAnnotation()
        {
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));
            Random rnd = new Random();
            InkList list = new InkList();
            PointF[] f = new PointF[10];
            for (int j = 0; j < 10; ++j)
            {
                int min = rnd.Next(100);
                int max = rnd.Next(100, 600);
                for (int i = 0; i < f.Length; ++i)
                {
                    f[i].X = rnd.Next(min, max);
                    f[i].Y = rnd.Next(min, max);

                }
                list.AddArray(new PointsArray(f));
            }
            PolygonAnnotation annotation = new PolygonAnnotation(list[0]);
            PolylineAnnotation annotation1 = new PolylineAnnotation(list[1]);
            annotation1.StartLineStyle = LineEndingStyle.Circle;
            annotation1.EndLineStyle = LineEndingStyle.RClosedArrow;
            annotation1.BackgroundColor = new ColorRGB((byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255));
            annotation1.Contents = "PDF polygon annotation";
            annotation.BackgroundColor = new ColorRGB((byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255));
            annotation.Color = new ColorRGB((byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255));
            annotation1.Color = new ColorRGB((byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255));
            annotation1.Contents = "PDF polyline annotation";
			document.Pages[0].Annotations.Add(annotation);
			document.Pages[0].Annotations.Add(annotation1);
			document.Save(OutputFolder + @"\TestPolygonAndPolylineAnnotation.pdf");
			document.Dispose();
			
			//Process.Start("TestPolygonAndPolylineAnnotation.pdf");
        }

        [Test]
        public void TesPolylineAnnotation()
        {
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));
            PointsArray points = new PointsArray();
            points.AddPoint(new Point(150, 275));
            points.AddPoint(new Point(250, 150));
            points.AddPoint(new Point(350, 275));
            points.AddPoint(new Point(125, 200));
            points.AddPoint(new Point(375, 200));
            points.AddPoint(new Point(150, 275));
            PolylineAnnotation annot = new PolylineAnnotation(points);
            annot.Contents = "Polyline annotations...";

			document.Pages[0].Annotations.Add(annot);
			document.Save(OutputFolder + @"\TestPolylineAnnotation.pdf");
			document.Dispose();
			
			//Process.Start("TestPolylineAnnotation.pdf");
        }

        [Test]
        public void TesHighlightAnnotation()
        {
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));
            HighlightAnnotation annot = new HighlightAnnotation(100, 50, 75, 50);
            annot.Contents = "PDF highlight annotation";
            annot.Color = new ColorRGB(0, 250, 0);

			document.Pages[0].Annotations.Add(annot);
			document.Save(OutputFolder + @"\TestHighlightAnnotation.pdf");
			document.Dispose();
			
			//Process.Start("TestHighlightAnnotation.pdf");
        }

        [Test]
        public void TesUnderlineAnnotation()
        {
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));
            UnderlineAnnotation annot = new UnderlineAnnotation(100, 50, 75, 50);
            annot.Contents = "PDF underline annotation";
            annot.Color = new ColorRGB(0, 50, 150);

			document.Pages[0].Annotations.Add(annot);
			document.Save(OutputFolder + @"\TestUnderlineAnnotation.pdf");
			document.Dispose();

			//Process.Start("TestUnderlineAnnotation.pdf");
        }

        [Test]
        public void TesSquigglyAnnotation()
        {
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));
            SquigglyAnnotation annot = new SquigglyAnnotation(100, 50, 75, 50);
            annot.Contents = "PDF squiggly annotation";
            annot.Color = new ColorRGB(80, 50, 150);

			document.Pages[0].Annotations.Add(annot);
			document.Save(OutputFolder + @"\TestSquigglyAnnotation.pdf");
			document.Dispose();

			//Process.Start("TestSquigglyAnnotation.pdf");
        }

        [Test]
        public void TesStrikeOutAnnotation()
        {
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));
            StrikeOutAnnotation annot = new StrikeOutAnnotation(100, 50, 75, 50, 45);
            annot.Contents = "PDF strike out annotation";
            annot.Color = new ColorRGB(80, 0, 150);

			document.Pages[0].Annotations.Add(annot);
			document.Save(OutputFolder + @"\TestStrikeOutAnnotation.pdf");
			document.Dispose();
			
			//Process.Start("TestStrikeOutAnnotation.pdf");
        }

        [Test]
        public void TesRubberStampAnnotation()
        {
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));
            RubberStampAnnotation annot = new RubberStampAnnotation(100, 50, 75, 50);
            annot.Contents = "PDF rubber stamp annotation";
            annot.Color = new ColorRGB(80, 0, 150);

			document.Pages[0].Annotations.Add(annot);
			document.Save(OutputFolder + @"\TestRubberStamp.pdf");
			document.Dispose();

			//Process.Start("TestRubberStamp.pdf");
        }

        [Test]
        public void TestInkAnnotation()
        {
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));

            Random rnd = new Random();
            InkList list = new InkList();
            PointF[] f = new PointF[5];
            for (int j = 0; j < 5; ++j)
            {
                int min = rnd.Next(100);
                int max = rnd.Next(100, 600);
                for (int i = 0; i < f.Length; ++i)
                {
                    f[i].X = rnd.Next(min, max);
                    f[i].Y = rnd.Next(min, max);

                }
                list.AddArray(new PointsArray(f));
            }
            InkAnnotation annot = new InkAnnotation(list);
            annot.Contents = "PDF ink annotation";
            annot.Color = new ColorRGB(80, 80, 50);

			document.Pages[0].Annotations.Add(annot);
			document.Save(OutputFolder + @"\TestInkAnnotation.pdf");
			document.Dispose();

			//Process.Start("TestInkAnnotation.pdf");
        }
    }
}
