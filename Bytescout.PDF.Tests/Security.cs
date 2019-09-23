using System;
using System.Drawing;
using System.Diagnostics;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class TestSecurity
	{
		private const string TestDataPathRelative = "..\\..\\..\\..\\PDF Extractor SDK Tests";
		private const string File1PathRelative = "Output\\TestSecurity.pdf";
		
		private const string LocalTestFilesPathRelative = "..\\..\\..\\TestFiles";
		private const string CertificatePathRelative = "Sign\\ByteScout.pfx";

		private Document m_document;
		private const string ownerPassword = "owner";
        private const string userPassword = "user";

		public string TestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, TestDataPathRelative)); } }
		public string OutputFolder { get { return TestContext.CurrentContext.TestDirectory; } }
		public string File1 { get { return System.IO.Path.Combine(TestData, File1PathRelative); } }

		public string LocalTestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, LocalTestFilesPathRelative)); } }
		public string TestCertificate { get { return System.IO.Path.Combine(LocalTestData, CertificatePathRelative); } }

		[SetUp]
		public void TestInitialize()
        {
			m_document = new Document();
			m_document.Security.EncryptionAlgorithm = EncryptionAlgorithm.RC4_128bit;
			m_document.Security.OwnerPassword = ownerPassword;
			m_document.Security.UserPassword = userPassword;
			m_document.Save(File1);
        }

		[TearDown]
		public void TestCleanup()
		{
            if (m_document != null)
                m_document.Dispose();
		}

        // Test the ability of owner.
        [Test]
        public void OwnerPassword_UnitTest()
        {
			Document document = new Document(File1, ownerPassword);
			Assert.AreEqual(userPassword, m_document.Security.UserPassword);
			Assert.AreEqual(ownerPassword, m_document.Security.OwnerPassword);
            Assert.AreEqual(document.Security.EncryptionAlgorithm, EncryptionAlgorithm.RC4_128bit);
			document.Dispose();
        }

        // Test the ability of user.
        [Test]
        public void UserPassword_UnitTest()
        {
			Document document = new Document(File1, userPassword);
	        Assert.AreEqual(userPassword, document.Security.UserPassword);
            Assert.AreEqual("", document.Security.OwnerPassword);
            Assert.AreEqual(document.Security.EncryptionAlgorithm, EncryptionAlgorithm.RC4_128bit);
			document.Dispose();
        }

        [Test]
        public void Signature()
        {
            Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));

            Canvas canvas = document.Pages[0].Canvas;
            SolidBrush brush = new SolidBrush();
            Font font = new Font("Arial", 16);
            RectangleF rect = new RectangleF(0, 50, canvas.Width, 100);
            StringFormat sf = new StringFormat();
            sf.HorizontalAlign = HorizontalAlign.Center;
            canvas.DrawString("Signature Test", font, brush, rect, sf);

            RectangleF signrect = new RectangleF(400, 50, 150, 100);

            //document.Sign(pfxfile, "123"); // invisible sign
			document.Sign(TestCertificate, "123", signrect, "Reason text", "Contact TextAlign", "Location text");

            //document.Save("..\\..\\..\\..\\PDF\\sign\\SignatureTest.pdf");
            document.Save(OutputFolder + @"\SignatureTest.pdf");
            document.Dispose();

            //Process.Start("..\\..\\..\\..\\PDF\\sign\\SignatureTest.pdf");
			//Process.Start("SignatureTest.pdf");
        }

        [Test]
        public void Certificate()
        {
			Certificate certificate = new Certificate(TestCertificate, "123");
        }
    }
}
