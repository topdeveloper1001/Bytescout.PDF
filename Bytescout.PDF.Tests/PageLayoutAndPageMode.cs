using System;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class TestPageLayoutAndPageMode
    {
		private const string TestDataPathRelative = "..\\..\\..\\..\\PDF Extractor SDK Tests";
		private const string File1PathRelative = "PDFSources\\FromPilots\\macbookmanual.pdf";

		public string TestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, TestDataPathRelative)); } }
	    public string OutputFolder { get { return TestContext.CurrentContext.TestDirectory; } }
		public string TestFile1 { get { return System.IO.Path.Combine(TestData, File1PathRelative); } }

		[SetUp]
        public void TestLayoutOneColumn()
        {
            PageLayout layout = PageLayout.OneColumn;
            string path = OutputFolder + @"\TestPageLayoutOC.pdf";
            int numOfPage = 10;

			Document document = new Document(TestFile1);

            for(int i = 0; i < numOfPage; ++i)
                document.Pages.Add(new Page(document.Pages[0].Width, document.Pages[0].Height));

            document.PageLayout = layout;
            document.Save(path);
			document.Dispose();

			document = new Document(path);
            Assert.AreEqual(layout, document.PageLayout);
			document.Dispose();
        }

        [Test]
        public void TestLayoutTwoColumnLeft()
        {
            PageLayout layout = PageLayout.TwoColumnLeft;
            string path = OutputFolder + @"\TestPageLayoutTCL.pdf";
            int numOfPage = 10;

			Document document = new Document(TestFile1);

            for (int i = 0; i < numOfPage; ++i)
                document.Pages.Add(new Page(document.Pages[0].Width, document.Pages[0].Height));

			document.PageLayout = layout;
            document.Save(path);
			document.Dispose();

			document = new Document(path);
            Assert.AreEqual(layout, document.PageLayout);
			document.Dispose();
        }

        [Test]
        public void TestLayoutTwoColumnRight()
        {
            PageLayout layout = PageLayout.TwoColumnRight;
            string path = OutputFolder + @"\TestPageLayoutTCR.pdf";
            int numOfPage = 10;

			Document document = new Document(TestFile1);

            for (int i = 0; i < numOfPage; ++i)
                document.Pages.Add(new Page(document.Pages[0].Width, document.Pages[0].Height));
			
            document.PageLayout = layout;
            document.Save(path);
			document.Dispose();

			document = new Document(path);
            Assert.AreEqual(layout, document.PageLayout);
			document.Dispose();
        }

        [Test]
        public void TestLayoutTwoPageLeft()
        {
            PageLayout layout = PageLayout.TwoPageLeft;
            string path = OutputFolder + @"\TestPageLayoutTPL.pdf";
            int numOfPage = 10;

			Document document = new Document(TestFile1);

            for (int i = 0; i < numOfPage; ++i)
				document.Pages.Add(new Page(document.Pages[0].Width, document.Pages[0].Height));
			
            document.PageLayout = layout;
            document.Save(path);
			document.Dispose();

			document = new Document(path);

            Assert.AreEqual(layout, document.PageLayout);
			document.Dispose();
        }

        [Test]
        public void TestLayoutTwoPageRight()
        {
            PageLayout layout = PageLayout.TwoPageRight;
            string path = OutputFolder + @"\TestPageLayoutTPR.pdf";
            int numOfPage = 10;

			Document document = new Document(TestFile1);

            for (int i = 0; i < numOfPage; ++i)
				document.Pages.Add(new Page(document.Pages[0].Width, document.Pages[0].Height));

			document.PageLayout = layout;
	        document.Save(path);
			document.Dispose();

			document = new Document(path);
			Assert.AreEqual(layout, document.PageLayout);
			document.Dispose();
        }

        [Test]
        public void TestModeFullScreen()
        {
            PageMode mode = PageMode.FullScreen;
            string path = OutputFolder + @"\TestPageModeFS.pdf";
            int numOfPage = 10;

			Document document = new Document(TestFile1);

            for (int i = 0; i < numOfPage; ++i)
				document.Pages.Add(new Page(document.Pages[0].Width, document.Pages[0].Height));
			
            document.PageMode = mode;
            document.Save(path);
			document.Dispose();

			document = new Document(path);
			Assert.AreEqual(mode, document.PageMode);
			document.Dispose();
        }

        [Test]
        public void TestModeOutLines()
        {
            PageMode mode = PageMode.Outlines;
            string path = OutputFolder + @"\TestPageModeO.pdf";
            int numOfPage = 10;

			Document document = new Document(TestFile1);

            for (int i = 0; i < numOfPage; ++i)
				document.Pages.Add(new Page(document.Pages[0].Width, document.Pages[0].Height));
			
            document.PageMode = mode;
            document.Save(path);
			document.Dispose();

			document = new Document(path);
			Assert.AreEqual(mode, document.PageMode);
			document.Dispose();
        }

        [Test]
        public void TestModeThumbnail()
        {
            PageMode mode = PageMode.Thumbnail;
            string path = OutputFolder + @"\TestPageModeT.pdf";
            int numOfPage = 10;

			Document document = new Document(TestFile1);

            for (int i = 0; i < numOfPage; ++i)
				document.Pages.Add(new Page(document.Pages[0].Width, document.Pages[0].Height));
			
            document.PageMode = mode;
            document.Save(path);
			document.Dispose();

			document = new Document(path);
			Assert.AreEqual(mode, document.PageMode);
			document.Dispose();
        }

        [Test]
        public void TestModeAttachment()
        {
            PageMode mode = PageMode.Attachment;
            string path = OutputFolder + @"\TestPageModeA.pdf";
            int numOfPage = 10;

			Document document = new Document(TestFile1);

            for (int i = 0; i < numOfPage; ++i)
				document.Pages.Add(new Page(document.Pages[0].Width, document.Pages[0].Height));
			
            document.PageMode = mode;
            document.Save(path);
			document.Dispose();

			document = new Document(path);
			Assert.AreEqual(mode, document.PageMode);
			document.Dispose();
        }

        [Test]
        public void TestModeOptionalContent()
        {
            PageMode mode = PageMode.OptionalContent;
            string path = OutputFolder + @"\TestPageModeOC.pdf";
            int numOfPage = 10;

			Document document = new Document(TestFile1);

            for (int i = 0; i < numOfPage; ++i)
				document.Pages.Add(new Page(document.Pages[0].Width, document.Pages[0].Height));
			
            document.PageMode = mode;
            document.Save(path);
			document.Dispose();

			document = new Document(path);
			Assert.AreEqual(mode, document.PageMode);
			document.Dispose();
        }
    }
}
