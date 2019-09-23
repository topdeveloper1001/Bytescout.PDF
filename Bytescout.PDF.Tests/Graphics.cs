using System;
using System.Diagnostics;
using System.Drawing;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class TestsCanvas
	{
		private const string TestDataPathRelative = "..\\..\\..\\..\\PDF Extractor SDK Tests";
		private const string File1PathRelative = "PDFSources\\FromPilots\\ALDORA 15C31359 poe.pdf";
		private const string File2PathRelative = "Sources-Other\\ICCProfile\\AppleRGB.icc";
		private const string File3PathRelative = "Sources-Other\\Images\\two_pilots.bmp";

		public string TestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, TestDataPathRelative)); } }
		public string OutputFolder { get { return TestContext.CurrentContext.TestDirectory; } }
		public string TestFile1 { get { return System.IO.Path.Combine(TestData, File1PathRelative); } }
		public string TestColorProfile { get { return System.IO.Path.Combine(TestData, File2PathRelative); } }
		public string TestBitmap { get { return System.IO.Path.Combine(TestData, File3PathRelative); } }

		[SetUp]
		public void TestInitialize()
		{
		}

		[TearDown]
		public void TestCleanup()
		{
		}

        [Test]
        public void TestLines()
        {
			Document document = new Document(TestFile1);
	        Canvas canvas = document.Pages[0].Canvas;
	        float translateY = 0.0f;
            const float step = 20.0f;
            SolidPen pen = new SolidPen(new ColorRGB(100, 100, 100), 10.0f);
            SolidBrush brush = new SolidBrush(new ColorRGB(255, 255, 255));
            Path path = new Path(FillMode.Winding);
            canvas.DrawLine(pen, new PointF(200, 100 + translateY), new PointF(250, 50 + translateY));
            canvas.DrawLine(pen, new PointF(250, 50 + translateY), new PointF(300, 100 + translateY));
            translateY += step;
            path.MoveTo(new PointF(200, 100 + translateY));
            path.AddLineTo(new PointF(250, 50 + translateY));
            path.AddLineTo(new PointF(300, 100 + translateY));
            canvas.DrawPath(pen, path);
            path.Reset();
            translateY += step;
            path.MoveTo(new PointF(200, 100 + translateY));
            path.AddLineTo(new PointF(250, 50 + translateY));
            path.AddLineTo(new PointF(300, 100 + translateY));
            pen.LineCap = LineCapStyle.Round;
            pen.LineJoin = LineJoinStyle.Round;
            canvas.DrawPath(pen, path);
            path.Reset();
            translateY += step;
            path.MoveTo(new PointF(200, 100 + translateY));
            path.AddLineTo(new PointF(250, 50 + translateY));
            path.AddLineTo(new PointF(300, 100 + translateY));
            pen.LineCap = LineCapStyle.ProjectingSquare;
            pen.LineJoin = LineJoinStyle.Bevel;
            canvas.DrawPath(pen, path);
            path.Reset();
            translateY += step;
            path.MoveTo(new PointF(200, 100 + translateY));
            path.AddLineTo(new PointF(250, 50 + translateY));
            path.AddLineTo(new PointF(300, 100 + translateY));
            pen.LineCap = LineCapStyle.Butt;
            pen.LineJoin = LineJoinStyle.Miter;
            pen.MiterLimit = 1.2f;
            canvas.DrawPath(pen, path);
            translateY += step;
            path.Reset();
            path.MoveTo(new PointF(200, 100 + translateY));
            path.AddLineTo(new PointF(250, 50 + translateY));
            path.AddLineTo(new PointF(300, 100 + translateY));
            pen.MiterLimit = 10.0f;
            pen.Opacity = 70;
            canvas.DrawPath(pen, path);
            translateY += step;
            path.Reset();
            path.MoveTo(new PointF(200, 100 + translateY));
            path.AddLineTo(new PointF(250, 50 + translateY));
            path.AddLineTo(new PointF(300, 100 + translateY));
            pen.Opacity = 30;
            canvas.DrawPath(pen, path);
            translateY += step;
            path.Reset();
            path.MoveTo(new PointF(200, 100 + translateY));
            path.AddLineTo(new PointF(250, 50 + translateY));
            path.AddLineTo(new PointF(300, 100 + translateY));
            pen.Opacity = 100;
            pen.Color = new ColorCMYK(255, 123, 0, 50);
            canvas.DrawPath(pen, path);
            translateY += step;
            path.Reset();
            path.MoveTo(new PointF(200, 100 + translateY));
            path.AddLineTo(new PointF(250, 50 + translateY));
            path.AddLineTo(new PointF(300, 100 + translateY));
            pen.DashPattern = new DashPattern(new float[] { 3, 3 }, 2);
            canvas.DrawPath(pen, path);
            translateY += step;
            path.Reset();
            path.MoveTo(new PointF(200, 100 + translateY));
            path.AddLineTo(new PointF(250, 50 + translateY));
            path.AddLineTo(new PointF(300, 100 + translateY));
            pen.DashPattern = new DashPattern();
            pen.Color = new ColorGray(100);
            canvas.DrawPath(pen, path);
            translateY += step;
            path.Reset();
            path.MoveTo(new PointF(200, 100 + translateY));
            path.AddLineTo(new PointF(250, 50 + translateY));
            path.AddLineTo(new PointF(300, 100 + translateY));
            path.ClosePath();
            pen.Color = new ColorGray(0);
            canvas.DrawPath(pen, path);

            document.Save(OutputFolder + @"\TestOPenLines.pdf");
			document.Dispose();
        }

        [Test]
        public void TestPolygons()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;

	        SolidPen pen = new SolidPen(new ColorRGB(100, 100, 100), 5.0f);
            SolidBrush brush = new SolidBrush(new ColorRGB(0, 255, 0));
            PointF start = new PointF(100, 100);
            int count = 5;
            PointF[] points = new PointF[count];
            for (int i = 0; i < count; ++i)
            {
                points[i].X = start.X + 50 * (float)Math.Cos((i * 2 * 72 - 90) * Math.PI / 180);
                points[i].Y = start.Y + 50 * (float)Math.Sin((i * 2 * 72 - 90) * Math.PI / 180);
            }
            Path path = new Path();
            path.AddPolygon(points);
            canvas.DrawPath(pen, brush, path);

            start = new PointF(200, 200);
            for (int i = 0; i < count; ++i)
            {
                points[i].X = start.X + 50 * (float)Math.Cos((i * 2 * 72 - 90) * Math.PI / 180);
                points[i].Y = start.Y + 50 * (float)Math.Sin((i * 2 * 72 - 90) * Math.PI / 180);
            }
            canvas.DrawPolygon(pen, brush, points);

            start = new PointF(0, 0);
            for (int i = 0; i < count; ++i)
            {
                points[i].X = start.X + 50 * (float)Math.Cos((i * 2 * 72 - 90) * Math.PI / 180);
                points[i].Y = start.Y + 50 * (float)Math.Sin((i * 2 * 72 - 90) * Math.PI / 180);
            }
            canvas.DrawPolygon(pen, brush, points);

            canvas.SaveGraphicsState();
            start = new PointF(300, 300);
            for (int i = 0; i < count; ++i)
            {
                points[i].X = start.X + 50 * (float)Math.Cos((i * 2 * 72 - 90) * Math.PI / 180);
                points[i].Y = start.Y + 50 * (float)Math.Sin((i * 2 * 72 - 90) * Math.PI / 180);
            }
            path.Reset();
            path.AddPolygon(points);
            canvas.SetClip(path);
            canvas.DrawRectangle(brush, start.X - 50, start.Y - 50, start.X + 50, start.Y + 50);
            canvas.RestoreGraphicsState();

            start = new PointF(300, 0);
            for (int i = 0; i < count; ++i)
            {
                points[i].X = start.X + 50 * (float)Math.Cos((i * 2 * 72 - 90) * Math.PI / 180);
                points[i].Y = start.Y + 50 * (float)Math.Sin((i * 2 * 72 - 90) * Math.PI / 180);
            }
            canvas.DrawPolygon(pen, brush, points);

            start = new PointF(600, 0);
            for (int i = 0; i < count; ++i)
            {
                points[i].X = start.X + 50 * (float)Math.Cos((i * 2 * 72 - 90) * Math.PI / 180);
                points[i].Y = start.Y + 50 * (float)Math.Sin((i * 2 * 72 - 90) * Math.PI / 180);
            }
            canvas.DrawPolygon(pen, brush, points);

            start = new PointF(600, 400);
            for (int i = 0; i < count; ++i)
            {
                points[i].X = start.X + 50 * (float)Math.Cos((i * 2 * 72 - 90) * Math.PI / 180);
                points[i].Y = start.Y + 50 * (float)Math.Sin((i * 2 * 72 - 90) * Math.PI / 180);
            }
            canvas.DrawPolygon(pen, brush, points);

            start = new PointF(0, 400);
            for (int i = 0; i < count; ++i)
            {
                points[i].X = start.X + 50 * (float)Math.Cos((i * 2 * 72 - 90) * Math.PI / 180);
                points[i].Y = start.Y + 50 * (float)Math.Sin((i * 2 * 72 - 90) * Math.PI / 180);
            }
            canvas.DrawPolygon(pen, brush, points);

            document.Save(OutputFolder + @"\TestPolygons.pdf");
        }

        [Test]
        public void TestBezierCurves()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;

	        Font font = new Font(StandardFonts.Helvetica, 8, false, false);
            SolidPen penCurve = new SolidPen();
            SolidPen penLine = new SolidPen();
            SolidBrush brush = new SolidBrush();
            penLine.DashPattern = new DashPattern(new float[] { 2, 2 }, 1);

            canvas.DrawCurve(penCurve, 100, 100, 200, 200, 100, 300, 200, 400);
            canvas.DrawLine(penLine, 100, 100, 200, 200);
            canvas.DrawLine(penLine, 100, 300, 200, 400);
            canvas.DrawCurve(penCurve, 300, 100, 200, 300, 400, 200, 400, 100);
            canvas.DrawLine(penLine, 300, 100, 200, 300);
            canvas.DrawLine(penLine, 400, 200, 400, 100);
            canvas.DrawString("100,100", font, brush, 100, 90);
            canvas.DrawString("200,200", font, brush, 200, 200);
            canvas.DrawString("100,300", font, brush, 100, 290);
            canvas.DrawString("200,400", font, brush, 200, 400);

            document.Save(OutputFolder + @"\TestBezierCurves.pdf");
			document.Dispose();
        }

        [Test]
        public void TestRectangles()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;
	        SolidBrush brush = new SolidBrush(new ColorCMYK(123, 255, 0, 50));

            brush.Opacity = 0;
            for (int i = 0; i < 10; ++i)
            {
                canvas.DrawRectangle(brush, i * 10, i * 10, i * 10 + 20, i * 10 + 20);
                brush.Opacity = i * 10 + 10;
            }

            brush.Opacity = 0;
            for (int i = 0; i < 10; ++i)
            {
                canvas.DrawRoundedRectangle(brush, 200 + i * 10, i * 10, i * 10 + 220, i * 10 + 20, i * 2);
                brush.Opacity = i * 10 + 10;
            }

            document.Save(OutputFolder + @"\TestRectangles.pdf");
			document.Dispose();
        }
        [Test]
        public void TestCirclesAndEllipses()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;
	        DeviceColor red = new ColorRGB(255, 0, 0);
            DeviceColor green = new ColorRGB(0, 255, 0);
            DeviceColor blue = new ColorRGB(0, 0, 255);
            SolidBrush brush = new SolidBrush(red);
            SolidPen pen = new SolidPen();
            canvas.BlendMode = BlendMode.Screen;
            canvas.DrawCircle(brush, 100, 100, 50);
            canvas.DrawEllipse(pen, brush, 300, 100, 200, 100);
            brush.Color = green;
            canvas.DrawCircle(brush, 100, 150, 50);
            canvas.DrawEllipse(pen, brush, 300, 200, 200, 100);
            brush.Color = blue;
            canvas.DrawCircle(brush, 150, 100, 50);
            canvas.DrawEllipse(pen, brush, 300, 300, 200, 100);
			
            document.Save(OutputFolder + @"\TestCirclesAndEllipses.pdf");
			document.Dispose();
        }
        [Test]
        public void TestTransformAndPie()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;
	        DeviceColor red = new ColorRGB(255, 0, 0);
            DeviceColor green = new ColorRGB(0, 255, 0);
            DeviceColor blue = new ColorRGB(0, 0, 255);

            SolidBrush brush = new SolidBrush(red);
            SolidPen pen = new SolidPen();
            pen.Width = 3;

            canvas.TranslateTransform(100, 200);
            canvas.DrawPie(brush, 0, 0, 50, 50, -90, 120);
            brush.Color = green;
            canvas.DrawPie(brush, 0, 0, 50, 50, 30, 120);
            brush.Color = blue;
            canvas.DrawPie(pen, brush, 0, 0, 50, 50, -90, -120);

            canvas.TranslateTransform(150, 0);
            canvas.ScaleTransform(1.5f, 1.5f);
            canvas.RotateTransform(90);

            brush.Color = red;
            canvas.DrawPie(brush, 0, 0, 50, 50, -90, 120);
            brush.Color = green;
            canvas.DrawPie(brush, 0, 0, 50, 50, 30, 120);
            brush.Color = blue;
            canvas.DrawPie(pen, brush, 0, 0, 50, 50, -90, -120);

            canvas.TranslateTransform(0, -150);
            canvas.ScaleTransform(1.5f, 1.5f);
            canvas.RotateTransform(45);

            brush.Color = red;
            canvas.DrawPie(brush, 0, 0, 50, 50, -90, 120);
            brush.Color = green;
            canvas.DrawPie(brush, 0, 0, 50, 50, 30, 120);
            brush.Color = blue;
            canvas.DrawPie(pen, brush, 0, 0, 50, 50, -90, -120);

            document.Save(OutputFolder + @"\TestTransformAndPie.pdf");
			document.Dispose();
        }

        [Test]
        public void TestTillingPattern()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;
	        ColorRGB red = new ColorRGB(255, 0, 0);
            ColorRGB green = new ColorRGB(0, 255, 0);
            ColorRGB blue = new ColorRGB(0, 0, 255);
            SolidPen pen = new SolidPen();

            ColoredTilingBrush tillingbrush = new ColoredTilingBrush(50, 50);
            UncoloredTilingBrush untillingbrush = new UncoloredTilingBrush(10, 10);
            canvas = untillingbrush.Canvas;
            canvas.DrawCircle(new SolidPen(green), new SolidBrush(blue), 5, 5, 4);
            untillingbrush.Color = green;
            canvas = tillingbrush.Canvas;
            canvas.DrawEllipse(pen, untillingbrush, 0, 0, 49, 25);
            canvas.RotateTransform(45);
            canvas.DrawRectangle(new SolidBrush(new ColorRGB(100, 100, 100)), 33, 5, 10, 20);
            canvas = document.Pages[0].Canvas;
            canvas.DrawRoundedRectangle(pen, tillingbrush, 100, 100, 200, 300, 30);
            document.Save(OutputFolder + @"\TestTillingPattern.pdf");
			document.Dispose();
        }

        [Test]
        public void TestTillingPatternPen()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;
	        ColorRGB red = new ColorRGB(255, 0, 0);
            ColorRGB green = new ColorRGB(0, 255, 0);
            ColorRGB blue = new ColorRGB(0, 0, 255);
            ColoredTilingBrush tillingbrush = new ColoredTilingBrush(3, 3);
            tillingbrush.Canvas.DrawEllipse(new SolidBrush(red), new RectangleF(0, 0, 2, 2));

            UncoloredTilingPen unctillingpen = new UncoloredTilingPen(10, 10);
            unctillingpen.Width = 15;

            unctillingpen.Canvas.DrawRectangle(tillingbrush, new RectangleF(0, 0, 5, 5));
            unctillingpen.Color = blue;

            ColoredTilingPen coltilpen = new ColoredTilingPen(20, 20);
            coltilpen.Width = 40;

            coltilpen.Canvas.DrawEllipse(tillingbrush, 0, 0, 15, 10);

            canvas.DrawEllipse(coltilpen, 50, 50, 200, 300);

            document.Save(OutputFolder + @"\TestTillingPatternPen.pdf");
			document.Dispose();
        }

        [Test]
        public void TestICCBased()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
	        Canvas canvas = document.Pages[0].Canvas;

	        SolidBrush brush = new SolidBrush();
            SolidPen pen = new SolidPen();
			ICCBasedColorspace icc = new ICCBasedColorspace(TestColorProfile);
            brush.Color = new ColorICC(icc, new ColorRGB(0, 255, 0));
            pen.Color = new ColorICC(icc, new ColorRGB(255, 0, 0));
            pen.Width = 5;
            canvas.DrawRectangle(pen, brush, 0, 0, 100, 100);

            document.Save(OutputFolder + @"\TestICCBased.pdf");
			document.Dispose();

			//Process.Start("TestICCBased.pdf");
        }

        [Test]
        public void TestImageManipulations()
        {
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));
			Canvas canvas = document.Pages[0].Canvas;
			Image image = new Image(TestBitmap);

            canvas.SkewTransform(25, 25);
            canvas.DrawImage(image, 10, 10, image.Width / 2, image.Height / 2);

            canvas.SkewTransform(-35, -35);
            canvas.DrawImage(image, image.Width * 2, image.Height, image.Width / 2, image.Height / 2);

            canvas.SkewTransform(15, 15);
            canvas.RotateTransform(45);
            canvas.DrawImage(image, 750, -300, image.Width / 2, image.Height / 2);

			document.Save(OutputFolder + @"\ImageManipulations.pdf");
			document.Dispose();

			//Process.Start("ImageManipulations.pdf");
        }

		[Test]
		public void TestCirlesEllipses()
		{
			Document document = new Document();
			document.Pages.Add(new Page(PaperFormat.A4));

			Canvas canvas = document.Pages[0].Canvas;
			Path path = new Path();
			path.AddEllipse(50, 50, document.Pages[0].Width - 100, document.Pages[0].Height - 100);
			canvas.SetClip(path);
			canvas.DrawPath(new SolidPen(), path);
			path.Reset();

			Random rand = new Random();
			for (int i = 0; i < 200; ++i)
			{
				float x = (float) rand.NextDouble() * (document.Pages[0].Width - 100 + 1);
				float y = (float) rand.NextDouble() * (document.Pages[0].Height - 100 + 1);
				float s = (float) rand.NextDouble() * (150 + 1);

				if (i % 2 == 0)
				{
					path.AddCircle(x, y, s);
				}
				else
				{
					float k = (float) rand.NextDouble() * (150 + 1);
					path.AddEllipse(x, y, s, k);
				}
				Color color = new ColorRGB((Byte) rand.Next(256), (Byte) rand.Next(256), (Byte) rand.Next(256));
				canvas.DrawPath(new SolidPen(color), new SolidBrush(color), path);
				path.Reset();
			}

			path.AddEllipse(50, 50, document.Pages[0].Width - 100, document.Pages[0].Height - 100);
			canvas.SetClip(path);
			SolidPen pen = new SolidPen();
			pen.Width = 2.5f;
			canvas.DrawPath(pen, path);

			document.Save(OutputFolder + @"\TestCirclesEllipses.pdf");

			//Process.Start("TestCirclesEllipses.pdf");
		}
    }
}
