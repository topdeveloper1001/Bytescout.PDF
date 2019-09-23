using System;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class TestPageLabels
    {
	    private const string TestDataPathRelative = "..\\..\\..\\..\\PDF Extractor SDK Tests";

	    public string TestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, TestDataPathRelative)); } }
	    public string OutputFolder { get { return TestContext.CurrentContext.TestDirectory; } }

		[SetUp]
        public void TestEmptyPageLabels()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));
            document.Save(OutputFolder + @"\TestEmptyPageLabels.pdf");
			document.Dispose();
        }

        [Test]
        public void TestAddPageLabels()
        {
			Document document = new Document();

            for (int i = 0; i < 10; i++)
            {
                document.Pages.Add(new Page(PaperFormat.A4));
            }

            // first four pages will have labels i, ii, iii, and iv
            PageLabel pageLabel = new PageLabel(0, PageNumberingStyle.LowercaseRoman);
            document.PageLabels.Add(pageLabel);
            
            // next three pages will have labels 1, 2, and 3
            pageLabel.Style = PageNumberingStyle.DecimalArabic;
            pageLabel.FirstPageIndex = 4;
            document.PageLabels.Add(pageLabel);

            // next three pages will have labels A-8, A-9, and A-10
            pageLabel.Style = PageNumberingStyle.DecimalArabic;
            pageLabel.Prefix = "A-";
            pageLabel.StartPortion = 8;
            pageLabel.FirstPageIndex = 7;
            document.PageLabels.Add(pageLabel);

            document.Save(OutputFolder + @"\TestAddPageLabels.pdf");
			document.Dispose();
        }

        [Test]
        public void TestRemovePageLabels()
        {
			Document document = new Document();

            for (int i = 0; i < 3; i++)
            {
                document.Pages.Add(new Page(PaperFormat.A4));
            }

            PageLabel pageLabel = new PageLabel(0, PageNumberingStyle.DecimalArabic);
            document.PageLabels.Add(pageLabel);

            pageLabel.Style = PageNumberingStyle.LowercaseLetters;
            pageLabel.FirstPageIndex = 1;
            document.PageLabels.Add(pageLabel);

            pageLabel.Style = PageNumberingStyle.UppercaseLetters;
            pageLabel.FirstPageIndex = 2;
            document.PageLabels.Add(pageLabel);

            document.Save(OutputFolder + @"\TestRemovePageLabels_before.pdf");

            document.PageLabels.Remove(1);

            document.Save(OutputFolder + @"\TestRemovePageLabels_after.pdf");
			document.Dispose();
        }

        [Test]
        public void TestCountLessThenFirstPageIndexAtPageLabels()
        {
			Document document = new Document();

            for (int i = 0; i < 3; i++)
            {
                document.Pages.Add(new Page(PaperFormat.A4));
            }

            PageLabel pageLabel = new PageLabel(4, PageNumberingStyle.LowercaseLetters);
            document.PageLabels.Add(pageLabel);

            document.Save(OutputFolder + @"\TestCountLessThenFirstPageIndexAtPageLabels.pdf");
			document.Dispose();
        }

        [Test]
        public void TestZeroFirstPageIndexAtPageLabelsNotExists()
        {
			Document document = new Document();

            for (int i = 0; i < 3; i++)
            {
                document.Pages.Add(new Page(PaperFormat.A4));
            }

            PageLabel pageLabel = new PageLabel(1, PageNumberingStyle.LowercaseLetters);
            document.PageLabels.Add(pageLabel);

            document.Save(OutputFolder + @"\TestZeroFirstPageIndexAtPageLabelsNotExists.pdf");
			document.Dispose();
        }

        [Test]
        public void TestOpenDocument()
        {
	        Document document = new Document(OutputFolder + @"\TestAddPageLabels.pdf");

	        PageLabel pageLabel = new PageLabel(9, PageNumberingStyle.LowercaseLetters);
            document.PageLabels.Add(pageLabel);

            document.Save(OutputFolder + @"\TestOpenDocument.pdf");
			document.Dispose();
        }
    }
}
