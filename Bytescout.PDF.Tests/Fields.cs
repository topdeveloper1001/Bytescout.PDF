using System;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
	public class TestFields
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
        public void TestEditFields()
        {
			Document document = new Document();
            document.Pages.Add(new Page(PaperFormat.A4));

            EditBox edit1 = new EditBox(10, 10, 100, 20, "Edit1");
            edit1.Text = "Simple edit box";
            edit1.Font.Size = 8;

            EditBox edit2 = new EditBox(10, 40, 100, 20, "Edit2");
            edit2.Text = "Centered text";
            edit2.TextAlign = TextAlign.Center;
            edit2.Font.Size = 8;

            EditBox edit3 = new EditBox(10, 70, 100, 20, "Edit3");
            edit3.Text = "Right align";
            edit3.TextAlign = TextAlign.Right;
            edit3.Font.Size = 8;

            EditBox edit4 = new EditBox(10, 100, 100, 20, "Edit4");
            edit4.Text = "Simple edit box Simple edit box";
            edit4.Font.Size = 8;

            EditBox edit5 = new EditBox(10, 130, 100, 20, "Edit5");
            edit5.Text = "Centered text Centered text Centered text";
            edit5.TextAlign = TextAlign.Center;
            edit5.Font.Size = 8;

            EditBox edit6 = new EditBox(10, 160, 100, 20, "Edit6");
            edit6.Text = "Right align Right align Right align";
            edit6.TextAlign = TextAlign.Right;
            edit6.Font.Size = 8;

            //password
            EditBox edit7 = new EditBox(120, 10, 100, 20, "Edit7");
            edit7.Text = "Simple edit box";
            edit7.Font.Size = 8;
            edit7.Password = true;

            EditBox edit8 = new EditBox(120, 40, 100, 20, "Edit8");
            edit8.Text = "Centered text";
            edit8.TextAlign = TextAlign.Center;
            edit8.Font.Size = 8;
            edit8.Password = true;

            EditBox edit9 = new EditBox(120, 70, 100, 20, "Edit9");
            edit9.Text = "Right align";
            edit9.TextAlign = TextAlign.Right;
            edit9.Font.Size = 8;
            edit9.Password = true;

            EditBox edit10 = new EditBox(120, 100, 100, 20, "Edit10");
            edit10.Text = "Simple edit box Simple edit box";
            edit10.Font.Size = 8;
            edit10.Password = true;

            EditBox edit11 = new EditBox(120, 130, 100, 20, "Edit11");
            edit11.Text = "Centered text Centered text Centered text";
            edit11.TextAlign = TextAlign.Center;
            edit11.Font.Size = 8;
            edit11.Password = true;

            EditBox edit12 = new EditBox(120, 160, 100, 20, "Edit12");
            edit12.Text = "Right align Right align Right align";
            edit12.TextAlign = TextAlign.Right;
            edit12.Font.Size = 8;
            edit12.Password = true;

            //maxlength
            EditBox edit13 = new EditBox(230, 10, 100, 20, "Edit13");
            edit13.Text = "Simple edit box";
            edit13.Font.Size = 8;
            edit13.MaxLength = 8;

            EditBox edit14 = new EditBox(230, 40, 100, 20, "Edit14");
            edit14.Text = "Centered text";
            edit14.TextAlign = TextAlign.Center;
            edit14.Font.Size = 8;
            edit14.MaxLength = 8;

            EditBox edit15 = new EditBox(230, 70, 100, 20, "Edit15");
            edit15.Text = "Right align";
            edit15.TextAlign = TextAlign.Right;
            edit15.Font.Size = 8;
            edit15.MaxLength = 8;

            EditBox edit16 = new EditBox(230, 100, 100, 20, "Edit16");
            edit16.Text = "Simple edit box Simple edit box";
            edit16.Font.Size = 8;
            edit16.MaxLength = 8;

            EditBox edit17 = new EditBox(230, 130, 100, 20, "Edit17");
            edit17.Text = "Centered text Centered text Centered text";
            edit17.TextAlign = TextAlign.Center;
            edit17.Font.Size = 8;
            edit17.MaxLength = 8;

            EditBox edit18 = new EditBox(230, 160, 100, 20, "Edit18");
            edit18.Text = "Right align Right align Right align";
            edit18.TextAlign = TextAlign.Right;
            edit18.Font.Size = 8;
            edit18.MaxLength = 8;

            //comb
            EditBox edit19 = new EditBox(340, 10, 100, 20, "Edit19");
            edit19.Text = "Left";
            edit19.Font.Size = 8;
            edit19.MaxLength = 8;
            edit19.InsertSpaces = true;

            EditBox edit20 = new EditBox(340, 40, 100, 20, "Edit20");
            edit20.Text = "Center";
            edit20.TextAlign = TextAlign.Center;
            edit20.Font.Size = 8;
            edit20.MaxLength = 8;
            edit20.InsertSpaces = true;

            EditBox edit21 = new EditBox(340, 70, 100, 20, "Edit21");
            edit21.Text = "Right";
            edit21.TextAlign = TextAlign.Right;
            edit21.Font.Size = 8;
            edit21.MaxLength = 8;
            edit21.InsertSpaces = true;

            EditBox edit22 = new EditBox(340, 100, 100, 20, "Edit22");
            edit22.Text = "Simple edit box Simple edit box";
            edit22.Font.Size = 8;
            edit22.MaxLength = 8;
            edit22.InsertSpaces = true;

            EditBox edit23 = new EditBox(340, 130, 100, 20, "Edit23");
            edit23.Text = "Centered text Centered text Centered text";
            edit23.TextAlign = TextAlign.Center;
            edit23.Font.Size = 8;
            edit23.MaxLength = 8;
            edit23.InsertSpaces = true;

            EditBox edit24 = new EditBox(340, 160, 100, 20, "Edit24");
            edit24.Text = "Right align Right align Right align";
            edit24.TextAlign = TextAlign.Right;
            edit24.Font.Size = 8;
            edit24.MaxLength = 8;
            edit24.InsertSpaces = true;

            SolidPen pen = new SolidPen();
            pen.DashPattern = new DashPattern(new float[] { 3 });
            document.Pages[0].Canvas.DrawLine(pen, 10, 200, document.Pages[0].Width - 10, 200);
            Font fnt = new Font(StandardFonts.Helvetica, 16);
            SolidBrush br = new SolidBrush();
            document.Pages[0].Canvas.DrawString("Multiline Edit Box", fnt, br, 250, 210);


            //multiline
            EditBox edit25 = new EditBox(10, 230, 100, 30, "Edit25");
            edit25.Text = "Simple edit box Simple edit box";
            edit25.Font.Size = 8;
            edit25.Multiline = true;

            EditBox edit26 = new EditBox(10, 270, 100, 30, "Edit26");
            edit26.Text = "Centered text Centered text";
            edit26.TextAlign = TextAlign.Center;
            edit26.Font.Size = 8;
            edit26.Multiline = true;

            EditBox edit27 = new EditBox(10, 310, 100, 30, "Edit27");
            edit27.Text = "Right align Right align blah blah";
            edit27.TextAlign = TextAlign.Right;
            edit27.Font.Size = 8;
            edit27.Multiline = true;

            EditBox edit28 = new EditBox(10, 350, 100, 30, "Edit28");
            edit28.Text = "Simple edit box Simple edit box Simple edit box Simple edit box";
            edit28.Font.Size = 8;
            edit28.Multiline = true;

            EditBox edit29 = new EditBox(10, 390, 100, 30, "Edit29");
            edit29.Text = "Centered text Centered text Centered text Centered text Centered text Centered text";
            edit29.TextAlign = TextAlign.Center;
            edit29.Font.Size = 8;
            edit29.Multiline = true;

            EditBox edit30 = new EditBox(10, 430, 100, 30, "Edit30");
            edit30.Text = "Right align Right align Right align Right align Right align Right align";
            edit30.TextAlign = TextAlign.Right;
            edit30.Font.Size = 8;
            edit30.Multiline = true;

            //multiline password
            EditBox edit31 = new EditBox(120, 230, 100, 30, "Edit31");
            edit31.Text = "Simple edit box Simple edit box";
            edit31.Font.Size = 8;
            edit31.Multiline = true;
            edit31.Password = true;

            EditBox edit32 = new EditBox(120, 270, 100, 30, "Edit32");
            edit32.Text = "Centered text Centered text";
            edit32.TextAlign = TextAlign.Center;
            edit32.Font.Size = 8;
            edit32.Multiline = true;
            edit32.Password = true;

            EditBox edit33 = new EditBox(120, 310, 100, 30, "Edit33");
            edit33.Text = "Right align Right align blah blah";
            edit33.TextAlign = TextAlign.Right;
            edit33.Font.Size = 8;
            edit33.Multiline = true;
            edit33.Password = true;

            EditBox edit34 = new EditBox(120, 350, 100, 30, "Edit34");
            edit34.Text = "Simple edit box Simple edit box Simple edit box Simple edit box";
            edit34.Font.Size = 8;
            edit34.Multiline = true;
            edit34.Password = true;

            EditBox edit35 = new EditBox(120, 390, 100, 30, "Edit35");
            edit35.Text = "Centered text Centered text Centered text Centered text Centered text Centered text";
            edit35.TextAlign = TextAlign.Center;
            edit35.Font.Size = 8;
            edit35.Multiline = true;
            edit35.Password = true;

            EditBox edit36 = new EditBox(120, 430, 100, 30, "Edit36");
            edit36.Text = "Right align Right align Right align Right align Right align Right align";
            edit36.TextAlign = TextAlign.Right;
            edit36.Font.Size = 8;
            edit36.Password = true;
            edit36.Multiline = true;

            //maxlength
            EditBox edit37 = new EditBox(230, 230, 100, 30, "Edit37");
            edit37.Text = "Simple edit box Simple edit box";
            edit37.Font.Size = 8;
            edit37.Multiline = true;
            edit37.MaxLength = 8;

            EditBox edit38 = new EditBox(230, 270, 100, 30, "Edit38");
            edit38.Text = "Centered text Centered text";
            edit38.TextAlign = TextAlign.Center;
            edit38.Font.Size = 8;
            edit38.Multiline = true;
            edit38.MaxLength = 8;

            EditBox edit39 = new EditBox(230, 310, 100, 30, "Edit39");
            edit39.Text = "Right align Right align blah blah";
            edit39.TextAlign = TextAlign.Right;
            edit39.Font.Size = 8;
            edit39.Multiline = true;
            edit39.MaxLength = 8;

            EditBox edit40 = new EditBox(230, 350, 100, 30, "Edit40");
            edit40.Text = "Simple edit box Simple edit box Simple edit box Simple edit box";
            edit40.Font.Size = 8;
            edit40.Multiline = true;
            edit40.MaxLength = 8;

            EditBox edit41 = new EditBox(230, 390, 100, 30, "Edit41");
            edit41.Text = "Centered text Centered text Centered text Centered text Centered text Centered text";
            edit41.TextAlign = TextAlign.Center;
            edit41.Font.Size = 8;
            edit41.Multiline = true;
            edit41.MaxLength = 8;

            EditBox edit42 = new EditBox(230, 430, 100, 30, "Edit42");
            edit42.Text = "Right align Right align Right align Right align Right align Right align";
            edit42.TextAlign = TextAlign.Right;
            edit42.Font.Size = 8;
            edit42.Multiline = true;
            edit42.MaxLength = 8;

            document.Pages[0].Annotations.Add(edit1);
            document.Pages[0].Annotations.Add(edit2);
            document.Pages[0].Annotations.Add(edit3);
            document.Pages[0].Annotations.Add(edit4);
            document.Pages[0].Annotations.Add(edit5);
            document.Pages[0].Annotations.Add(edit6);
            document.Pages[0].Annotations.Add(edit7);
            document.Pages[0].Annotations.Add(edit8);
            document.Pages[0].Annotations.Add(edit9);
            document.Pages[0].Annotations.Add(edit10);
            document.Pages[0].Annotations.Add(edit11);
            document.Pages[0].Annotations.Add(edit12);
            document.Pages[0].Annotations.Add(edit13);
            document.Pages[0].Annotations.Add(edit14);
            document.Pages[0].Annotations.Add(edit15);
            document.Pages[0].Annotations.Add(edit16);
            document.Pages[0].Annotations.Add(edit17);
            document.Pages[0].Annotations.Add(edit18);
            document.Pages[0].Annotations.Add(edit19);
            document.Pages[0].Annotations.Add(edit20);
            document.Pages[0].Annotations.Add(edit21);
            document.Pages[0].Annotations.Add(edit22);
            document.Pages[0].Annotations.Add(edit23);
            document.Pages[0].Annotations.Add(edit24);
            document.Pages[0].Annotations.Add(edit25);
            document.Pages[0].Annotations.Add(edit26);
            document.Pages[0].Annotations.Add(edit27);
            document.Pages[0].Annotations.Add(edit28);
            document.Pages[0].Annotations.Add(edit29);
            document.Pages[0].Annotations.Add(edit30);
            document.Pages[0].Annotations.Add(edit31);
            document.Pages[0].Annotations.Add(edit32);
            document.Pages[0].Annotations.Add(edit33);
            document.Pages[0].Annotations.Add(edit34);
            document.Pages[0].Annotations.Add(edit35);
            document.Pages[0].Annotations.Add(edit36);
            document.Pages[0].Annotations.Add(edit37);
            document.Pages[0].Annotations.Add(edit38);
            document.Pages[0].Annotations.Add(edit39);
            document.Pages[0].Annotations.Add(edit40);
            document.Pages[0].Annotations.Add(edit41);
            document.Pages[0].Annotations.Add(edit42);

            document.Save(OutputFolder + @"\TestEditFields.pdf");
			document.Dispose();
        }
    }
}
