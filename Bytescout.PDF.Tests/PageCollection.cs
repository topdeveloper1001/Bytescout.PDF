using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class TestPageCollection
    {
		private const string TestDataPathRelative = "..\\..\\..\\..\\PDF Extractor SDK Tests";
		private const string File1PathRelative = "Sources-Other\\Images\\two_pilots.bmp";

		public string TestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, TestDataPathRelative)); } }
	    public string OutputFolder { get { return TestContext.CurrentContext.TestDirectory; } }
		public string TestBitmap { get { return System.IO.Path.Combine(TestData, File1PathRelative); } }

        private Document m_doc;

		[SetUp]
		public void TestInitialize()
		{
			m_doc = new Document();
		}

		[TearDown]
		public void TestCleanup()
		{
			m_doc.Dispose();
		}

        // Test of number pages.
        [Test]
        public void AddRemovePages_UnitTest()
        {
            int count = m_doc.Pages.Count;

            Random rand = new Random();
            int numOfPage = rand.Next(100);

            for (int i = 0; i < numOfPage; ++i, ++count)
                m_doc.Pages.Add(new Page(100, 100));

            numOfPage = rand.Next(m_doc.Pages.Count);

            for (int i = 0; i < numOfPage; ++i, --count)
                m_doc.Pages.Remove(rand.Next(m_doc.Pages.Count));

            Assert.AreEqual(m_doc.Pages.Count, count);
        }

		[Test]
		public void AddPageThumbnails()
		{
			Document document = new Document();
			document.PageMode = PageMode.Thumbnail;
			document.Pages.Add(new Page(PaperFormat.A4));
			document.Pages.Add(new Page(PaperFormat.A4));

			Image thumbnail = new Image(TestBitmap);
			document.Pages[1].Thumbnail = thumbnail;
			document.Save(OutputFolder + @"\TestAddPageThumbnails.pdf");

			//Process.Start("TestAddPageThumbnails.pdf");
		}

		[Test]
		public void CustomPageSize()
		{
			Page page = new Page(595f, 842f, UnitOfMeasure.Point);
			Assert.AreEqual(Math.Round(page.Width), 595d, double.Epsilon);
			Assert.AreEqual(Math.Round(page.Height), 842d, double.Epsilon);

			page = new Page(8.27f, 11.69f, UnitOfMeasure.Inch);
			Assert.AreEqual(Math.Round(page.Width), 595d, double.Epsilon);
			Assert.AreEqual(Math.Round(page.Height), 842d, double.Epsilon);
			
			page = new Page(210f, 297f, UnitOfMeasure.Millimeter);
			Assert.AreEqual(Math.Round(page.Width), 595d, double.Epsilon);
			Assert.AreEqual(Math.Round(page.Height), 842d, double.Epsilon);
			
			page = new Page(21f, 29.7f, UnitOfMeasure.Centimeter);
			Assert.AreEqual(Math.Round(page.Width), 595d, double.Epsilon);
			Assert.AreEqual(Math.Round(page.Height), 842d, double.Epsilon);

			page = new Page(794f, 1122f, UnitOfMeasure.Pixel96DPI);
			Assert.AreEqual(Math.Round(page.Width, 1), 595d, .5d);
			Assert.AreEqual(Math.Round(page.Height, 1), 842d, .5d);

			page = new Page(992f, 1403f, UnitOfMeasure.Pixel120DPI);
			Assert.AreEqual(Math.Round(page.Width, 1), 595d, .5d);
			Assert.AreEqual(Math.Round(page.Height, 1), 842d, .5d);

			page = new Page(165.4f, 233.8f, UnitOfMeasure.Twip);
			Assert.AreEqual(Math.Round(page.Width), 595d, double.Epsilon);
			Assert.AreEqual(Math.Round(page.Height), 842d, double.Epsilon);

			page = new Page(2481f, 3507f, UnitOfMeasure.Document);
			Assert.AreEqual(Math.Round(page.Width), 595d, double.Epsilon);
			Assert.AreEqual(Math.Round(page.Height), 842d, double.Epsilon);
		}
    }
}
