using System;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class TestDocumentInformation
    {
        private Document m_doc;
        private DocumentInformation m_docInfo;

		[SetUp]
		public void TestInitialize()
        {
            m_doc = new Document();
            m_doc.Pages.Add(new Page(PaperFormat.A4));
            m_docInfo = m_doc.DocumentInfo;
        }

		[TearDown]
		public void TestCleanup()
		{
			m_doc.Dispose();
		}

        [Test]
        public void Author_UnitTest()
        {
            string Author = "Author";
            m_docInfo.Author = Author;
            
            Assert.AreEqual(Author, m_docInfo.Author);
        }

        [Test]
        public void CreationDate_UnitTest()
        {
            Assert.IsNotNull(m_docInfo.CreationDate);
        }

        [Test]
        public void Creator_UnitTest()
        {
            string Creator = "Creator";
            m_docInfo.Creator = Creator;

            Assert.AreEqual(Creator, m_docInfo.Creator);
        }

        [Test]
        public void Keywords_UnitTest()
        {
            string Keywords = "Keywords";
            m_docInfo.Keywords = Keywords;
            
            Assert.AreEqual(Keywords, m_docInfo.Keywords);
        }

        [Test]
        public void ModDate_UnitTest()
        {
            Assert.IsNotNull(m_docInfo.ModificationDate);
        }

        [Test]
        public void Producer_UnitTest()
        {
            Assert.IsNotNull(m_docInfo.Producer);
        }

        [Test]
        public void Subject_UnitTest()
        {
            string Subject = "Subject";
            m_docInfo.Subject = Subject;
            
            Assert.AreEqual(Subject, m_docInfo.Subject);
        }

        [Test]
        public void Title_UnitTest()
        {
            string Title = "Title";
            m_docInfo.Title = Title;

            Assert.AreEqual(Title, m_docInfo.Title);
        }

        [Test]
        public void Open_UnitTest()
        {
            Assert.IsNotNull(m_docInfo.Author);
            Assert.IsNotNull(m_docInfo.CreationDate);
            Assert.IsNotNull(m_docInfo.Creator);
            Assert.IsNotNull(m_docInfo.Keywords);
            Assert.IsNotNull(m_docInfo.ModificationDate);
            Assert.IsNotNull(m_docInfo.Producer);
            Assert.IsNotNull(m_docInfo.Subject);
            Assert.IsNotNull(m_docInfo.Title);
        }
    }
}
