using System;
using System.IO;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
    public class TestFlattening
    {
		private const string TestDataPathRelative = "..\\..\\..\\..\\PDF Extractor SDK Tests";

		private const string TestFile1PathRelative = "PDFSources\\FromSupport\\acroforms\\text_filled.pdf";
		private const string ExpectedFile1PathRelative = "Expected\\text_filled_fl.pdf";
		private const string OutputFile1PathRelative = "Output\\text_filled_fl.pdf";
		private const string TestFile2PathRelative = "PDFSources\\FromSupport\\long-footer-2.pdf";
		private const string ExpectedFile2PathRelative = "Expected\\long-footer-2_fl.pdf";
		private const string OutputFile2PathRelative = "Output\\long-footer-2_fl.pdf";
		private const string TestFile3PathRelative = "PDFSources\\FromSupport\\acroforms\\1074-acrofrom-with-text-fields-and-highlights.pdf";
		private const string ExpectedFile3PathRelative = "Expected\\1074-acrofrom-with-text-fields-and-highlights_fl.pdf";
		private const string OutputFile3PathRelative = "Output\\1074-acrofrom-with-text-fields-and-highlights_fl.pdf";
		private const string TestFile4PathRelative = "PDFSources\\FromSupport\\acroforms\\1087-umlauts-in-textfield-id-106.pdf";
		private const string ExpectedFile4PathRelative = "Expected\\1087-umlauts-in-textfield-id-106_fl.pdf";
		private const string OutputFile4PathRelative = "Output\\1087-umlauts-in-textfield-id-106_fl.pdf";
		private const string TestFile5PathRelative = "PDFSources\\FromSupport\\acroforms\\CES-D_V1m.pdf";
		private const string ExpectedFile5PathRelative = "Expected\\CES-D_V1m_fl.pdf";
		private const string OutputFile5PathRelative = "Output\\CES-D_V1m_fl.pdf";
		private const string TestFile6PathRelative = "PDFSources\\FromSupport\\FormWithAnnotationsAndControls.pdf";
		private const string ExpectedFile6PathRelative = "Expected\\FormWithAnnotationsAndControls_fl.pdf";
		private const string OutputFile6PathRelative = "Output\\FormWithAnnotationsAndControls_fl.pdf";
		private const string TestFile7PathRelative = "PDFSources\\FromSupport\\acroforms\\1229-interactiveform_enabled_m.pdf";
		private const string ExpectedFile7PathRelative = "Expected\\1229-interactiveform_enabled_m_fl.pdf";
		private const string OutputFile7PathRelative = "Output\\1229-interactiveform_enabled_m_fl.pdf";
		private const string TestFile8PathRelative = "PDFSources\\FromSupport\\acroforms\\883_m.pdf";
		private const string ExpectedFile8PathRelative = "Expected\\883_m_fl.pdf";
		private const string OutputFile8PathRelative = "Output\\883_m_fl.pdf";
		private const string TestFile9PathRelative = "PDFSources\\FromSupport\\rendering\\images\\944-barcode-image-as-2nd-object-with-same-index.pdf";
		private const string ExpectedFile9PathRelative = "Expected\\944-barcode-image-as-2nd-object-with-same-index_fl.pdf";
		private const string OutputFile9PathRelative = "Output\\944-barcode-image-as-2nd-object-with-same-index_fl.pdf";
		private const string TestFile10PathRelative = "PDFSources\\FromSupport\\acroforms\\Form1-2010_m.pdf";
		private const string ExpectedFile10PathRelative = "Expected\\Form1-2010_m_fl.pdf";
		private const string OutputFile10PathRelative = "Output\\Form1-2010_m_fl.pdf";

		public string TestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, TestDataPathRelative)); } }
		public string TestFile1 { get { return System.IO.Path.Combine(TestData, TestFile1PathRelative); } }
		public string ExpectedFile1 { get { return System.IO.Path.Combine(TestData, ExpectedFile1PathRelative); } }
		public string OutputFile1 { get { return System.IO.Path.Combine(TestData, OutputFile1PathRelative); } }
		public string TestFile2 { get { return System.IO.Path.Combine(TestData, TestFile2PathRelative); } }
		public string ExpectedFile2 { get { return System.IO.Path.Combine(TestData, ExpectedFile2PathRelative); } }
		public string OutputFile2 { get { return System.IO.Path.Combine(TestData, OutputFile2PathRelative); } }
		public string TestFile3 { get { return System.IO.Path.Combine(TestData, TestFile3PathRelative); } }
		public string ExpectedFile3 { get { return System.IO.Path.Combine(TestData, ExpectedFile3PathRelative); } }
		public string OutputFile3 { get { return System.IO.Path.Combine(TestData, OutputFile3PathRelative); } }
		public string TestFile4 { get { return System.IO.Path.Combine(TestData, TestFile4PathRelative); } }
		public string ExpectedFile4 { get { return System.IO.Path.Combine(TestData, ExpectedFile4PathRelative); } }
		public string OutputFile4 { get { return System.IO.Path.Combine(TestData, OutputFile4PathRelative); } }
		public string TestFile5 { get { return System.IO.Path.Combine(TestData, TestFile5PathRelative); } }
		public string ExpectedFile5 { get { return System.IO.Path.Combine(TestData, ExpectedFile5PathRelative); } }
		public string OutputFile5 { get { return System.IO.Path.Combine(TestData, OutputFile5PathRelative); } }
		public string TestFile6 { get { return System.IO.Path.Combine(TestData, TestFile6PathRelative); } }
		public string ExpectedFile6 { get { return System.IO.Path.Combine(TestData, ExpectedFile6PathRelative); } }
		public string OutputFile6 { get { return System.IO.Path.Combine(TestData, OutputFile6PathRelative); } }
		public string TestFile7 { get { return System.IO.Path.Combine(TestData, TestFile7PathRelative); } }
		public string ExpectedFile7 { get { return System.IO.Path.Combine(TestData, ExpectedFile7PathRelative); } }
		public string OutputFile7 { get { return System.IO.Path.Combine(TestData, OutputFile7PathRelative); } }
		public string TestFile8 { get { return System.IO.Path.Combine(TestData, TestFile8PathRelative); } }
		public string ExpectedFile8 { get { return System.IO.Path.Combine(TestData, ExpectedFile8PathRelative); } }
		public string OutputFile8 { get { return System.IO.Path.Combine(TestData, OutputFile8PathRelative); } }
		public string TestFile9 { get { return System.IO.Path.Combine(TestData, TestFile9PathRelative); } }
		public string ExpectedFile9 { get { return System.IO.Path.Combine(TestData, ExpectedFile9PathRelative); } }
		public string OutputFile9 { get { return System.IO.Path.Combine(TestData, OutputFile9PathRelative); } }
		public string TestFile10 { get { return System.IO.Path.Combine(TestData, TestFile10PathRelative); } }
		public string ExpectedFile10 { get { return System.IO.Path.Combine(TestData, ExpectedFile10PathRelative); } }
		public string OutputFile10 { get { return System.IO.Path.Combine(TestData, OutputFile10PathRelative); } }


        private byte[] fileRead(string fileName)
        {
            byte[] array;
            using (FileStream fs = File.OpenRead(fileName))
            {
                int n = 140; // отсекаем trailer с File Identifiers который зависит от текущего времени
                array = new byte[fs.Length - n];
                fs.Read(array, 0, array.Length);
            }
            return array;
        }

        private bool arrayCompare(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null)
                return false;
            if (a1.Length != a2.Length)
                return false;
            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false;
            }
            return true;
        }

        [Test]
        public void TestFlattening_1()
        {
			Document document = new Document(TestFile1);
            document.RegistrationName = "support@bytescout.com";
            document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
            document.FlattenDocument();

			document.Save(OutputFile1);
            document.Dispose();
			byte[] expected = fileRead(ExpectedFile1);
			byte[] actual = fileRead(OutputFile1);

#if FULL
            Assert.IsTrue(arrayCompare(expected, actual));
#endif
        }

        [Test]
        public void TestFlattening_2()
        {
			Document document = new Document(TestFile2);
            document.RegistrationName = "support@bytescout.com";
            document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
            document.FlattenDocument();

			document.Save(OutputFile2);
            document.Dispose();
            byte[] expected = fileRead(ExpectedFile2);
			byte[] actual = fileRead(OutputFile2);
#if FULL
			Assert.IsTrue(arrayCompare(expected, actual));
#endif
        }

        [Test]
        public void TestFlattening_3()
        {
            Document document = new Document(TestFile3);
            document.RegistrationName = "support@bytescout.com";
            document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
            document.FlattenDocument();

			document.Save(OutputFile3);
            document.Dispose();
            byte[] expected = fileRead(ExpectedFile3);
			byte[] actual = fileRead(OutputFile3);
#if FULL
            Assert.IsTrue(arrayCompare(expected, actual));
#endif
		}

        [Test]
        public void TestFlattening_4()
        {
            Document document = new Document(TestFile4);
            document.RegistrationName = "support@bytescout.com";
            document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
            document.FlattenDocument();

			document.Save(OutputFile4);
            document.Dispose();
            byte[] expected = fileRead(ExpectedFile4);
			byte[] actual = fileRead(OutputFile4);
#if FULL
            Assert.IsTrue(arrayCompare(expected, actual));
#endif
		}

        [Test]
        public void TestFlattening_5()
        {
            Document document = new Document(TestFile5);
            document.RegistrationName = "support@bytescout.com";
            document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
            document.FlattenDocument();

			document.Save(OutputFile5);
            document.Dispose();
            byte[] expected = fileRead(ExpectedFile5);
			byte[] actual = fileRead(OutputFile5);
#if FULL
            Assert.IsTrue(arrayCompare(expected, actual));
#endif
		}

        [Test]
        public void TestFlattening_6()
        {
            Document document = new Document(TestFile6);
            document.RegistrationName = "support@bytescout.com";
            document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
            document.FlattenDocument();

			document.Save(OutputFile6);
            document.Dispose();
            byte[] expected = fileRead(ExpectedFile6);
			byte[] actual = fileRead(OutputFile6);
#if FULL
            Assert.IsTrue(arrayCompare(expected, actual));
#endif
		}

        [Test]
        public void TestFlattening_7()
        {
            Document document = new Document(TestFile7);
            document.RegistrationName = "support@bytescout.com";
            document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
            document.FlattenDocument();

			document.Save(OutputFile7);
            document.Dispose();
            byte[] expected = fileRead(ExpectedFile7);
			byte[] actual = fileRead(OutputFile7);
#if FULL
            Assert.IsTrue(arrayCompare(expected, actual));
#endif
		}

        [Test]
        public void TestFlattening_8()
        {
            Document document = new Document(TestFile8);
            document.RegistrationName = "support@bytescout.com";
            document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
            document.FlattenDocument();

			document.Save(OutputFile8);
            document.Dispose();
            byte[] expected = fileRead(ExpectedFile8);
			byte[] actual = fileRead(OutputFile8);
#if FULL
            Assert.IsTrue(arrayCompare(expected, actual));
#endif
		}

        [Test]
        public void TestFlattening_9()
        {
            Document document = new Document(TestFile9);
            document.RegistrationName = "support@bytescout.com";
            document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
            document.FlattenDocument();

			document.Save(OutputFile9);
            document.Dispose();
            byte[] expected = fileRead(ExpectedFile9);
			byte[] actual = fileRead(OutputFile9);
#if FULL
            Assert.IsTrue(arrayCompare(expected, actual));
#endif
		}

        [Test]
        public void TestFlattening_10()
        {
            Document document = new Document(TestFile10);
            document.RegistrationName = "support@bytescout.com";
            document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
            document.FlattenDocument();

			document.Save(OutputFile10);
            document.Dispose();
            byte[] expected = fileRead(ExpectedFile10);
			byte[] actual = fileRead(OutputFile10);
#if FULL
            Assert.IsTrue(arrayCompare(expected, actual));
#endif
		}

        //[Test]
        //public void TestFlattening_11()
        //{
        //    string TestFile1 = @"..\..\..\..\PDF Extractor SDK - Test\PDFSources\FromSupport\acroforms\IMM0008ENU_2D_m.pdf";
        //    string TestFile2 = @"..\..\..\..\PDF Extractor SDK - Test\Expected\IMM0008ENU_2D_m_fl.pdf";
        //    string TestFile3 = @"..\..\..\..\PDF Extractor SDK - Test\Output\IMM0008ENU_2D_m_fl.pdf";
        //    Document document = new Document(TestFile1);
        //    document.RegistrationName = "support@bytescout.com";
        //    document.RegistrationKey = "CF75-1A80-601C-999C-105C-6166-01E";
        //    document.FlattenDocument();

        //    document.Save(TestFile3);
        //    document.Dispose();
        //    byte[] expected = fileRead(TestFile2);
        //    byte[] actual = fileRead(TestFile3);
        //    Assert.IsTrue(arrayCompare(expected, actual));
        //}
    }
}
