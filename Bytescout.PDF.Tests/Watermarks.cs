using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
    public class TestWatermarks
    {
        private Document m_doc;

	    private const string TestDataPathRelative = "..\\..\\..\\..\\PDF Extractor SDK Tests";

	    public string TestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, TestDataPathRelative)); } }
	    public string OutputFolder { get { return TestContext.CurrentContext.TestDirectory; } }

		[Test]
        public void TestDefaultTextWatermaks()
        {
            m_doc = new Document();
            m_doc.Pages.Add(new Page(PaperFormat.A4));

            TextWatermark watermark = new TextWatermark("Sample test watermark");
            Assert.AreEqual("Sample test watermark", watermark.Text);
            Assert.AreEqual(0.0f, watermark.Left);
            Assert.AreEqual(0.0f, watermark.Top);
            Assert.AreEqual(0.0f, watermark.Angle);
            Assert.AreEqual(16f, watermark.Font.Size);
            Assert.AreEqual(StandardFonts.Helvetica.ToString(), watermark.Font.Name);

            m_doc.Pages[0].Watermarks.Add(watermark);           
            m_doc.Save(OutputFolder + @"\TestDefaultTextWatermaks.pdf");

			//Process.Start("TestDefaultTextWatermaks.pdf");
        }

		[Test]
        public void TestCustomTextWatermaks()
        {
            m_doc = new Document();
            m_doc.Pages.Add(new Page(PaperFormat.A4));

            TextWatermark watermark = new TextWatermark("Sample test watermark");
            watermark.Angle = 45;
            watermark.Left = 150;
            watermark.Top = 250;
            watermark.Font.Size = 15;

            Assert.AreEqual("Sample test watermark", watermark.Text);
            Assert.AreEqual(150.0f, watermark.Left);
            Assert.AreEqual(250.0f, watermark.Top);
            Assert.AreEqual(45.0f, watermark.Angle);
            Assert.AreEqual(15, watermark.Font.Size);

            m_doc.Pages[0].Watermarks.Add(watermark);
            m_doc.Save(OutputFolder + @"\TestCustomTextWatermaks.pdf");

			//Process.Start("TestCustomTextWatermaks.pdf");
        }

		[Test]
        public void TestSeveralTextWatermaks()
        {
            m_doc = new Document();
            m_doc.Pages.Add(new Page(PaperFormat.A4));
            m_doc.Pages.Add(new Page(PaperFormat.A4));
            m_doc.Pages.Add(new Page(PaperFormat.A4));

            TextWatermark watermark1 = new TextWatermark("Sample test watermark 1");
            watermark1.Angle = 45;
            watermark1.Left = 250;
            watermark1.Top = 250;
            watermark1.Font.Size = 15;

            TextWatermark watermark2 = new TextWatermark("Sample test watermark 2");            
            watermark2.Left = 00;
            watermark2.Top = 500;
            watermark2.Font.Size = 10;

            TextWatermark watermark3 = new TextWatermark("Sample test watermark 3");
            watermark3.Angle = 45;
            watermark3.Left = 650;
            watermark3.Top = 150;
            watermark3.Font.Size = 25;
            
            m_doc.Pages[0].Watermarks.Add(watermark1);
            m_doc.Pages[1].Watermarks.Add(watermark2);
            m_doc.Pages[2].Watermarks.Add(watermark3);
            m_doc.Save(OutputFolder + @"\TestSeveralTextWatermaks.pdf");

			//Process.Start("TestSeveralTextWatermaks.pdf");
        }

		[Test]
        public void TestCenterpageTextWatermaks()
        {
            m_doc = new Document();
            for (int i = 0; i < 5; ++i)
            {
                m_doc.Pages.Add(new Page(PaperFormat.A4));
            }

            TextWatermark watermark = new TextWatermark("Bytescout PDF SDK");           
            watermark.Left = m_doc.Pages[0].Width / 2 - 175;
            watermark.Top = m_doc.Pages[0].Height / 2;
            watermark.Font.Size = 30;

            for (int i = 0; i < m_doc.Pages.Count; ++i)
            {
                m_doc.Pages[i].Watermarks.Add(watermark);
            }

            m_doc.Save(OutputFolder + @"\TestCenterpageTextWatermaks.pdf");

			//Process.Start("TestCenterpageTextWatermaks.pdf");
        }
    }
}
