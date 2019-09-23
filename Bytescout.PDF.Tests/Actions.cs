using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
    public class TestActions
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
        public void TestAddURLAction()
        {
            Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));

            URIAction urlAction = new URIAction(new Uri(@"http://bytescout.com"));

            PushButton button = new PushButton(20, 40, 120, 25, "btn1");
            button.Caption = "Bytescout.com";
            button.OnActivated = urlAction;
            button.Font.Size = 8;
            document.Pages[0].Annotations.Add(button);
            document.Save(OutputFolder + @"\AddURLAction.pdf");
			document.Dispose();

			//Process.Start("AddURLAction.pdf");
        }
    }
}
