using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using NUnit.Framework;

namespace Bytescout.PDF.Tests
{
	[TestFixture]
    public class LangRightToLeft
    {
		private const string TestDataPathRelative = @"..\..\..\..\PDF Extractor SDK Tests";
		private const string Fonts2PathRelative = @"Sources-Other\Fonts";

        private readonly Font _fontSansSerif = new Font("Microsoft Sans Serif", 14);
        private readonly Font _fontArialUnicodeMS = new Font("Arial Unicode MS", 14);
        private readonly Font _fontArial = new Font("Arial", 14);
        private readonly System.Drawing.Font _fontArialWindows = new System.Drawing.Font("Arial", 14);

		private Font _fileFontArabtype = Font.FromFile(System.IO.Path.Combine(FontPath2, "arabtype.ttf"), 14);
		private Font _fileFontArial = Font.FromFile(System.IO.Path.Combine(FontPath2, "arial.ttf"), 14);
		private Font _fileFontRod = Font.FromFile(System.IO.Path.Combine(FontPath2, "rod.ttf"), 14);
		private Font _fileFontHebrew = Font.FromFile(System.IO.Path.Combine(FontPath2, "hebrew.ttf"), 14);
		private Font _fileFontGisha = Font.FromFile(System.IO.Path.Combine(FontPath2, "gisha.ttf"), 14);

        private Brush _black_brush = new SolidBrush();
        private Brush _green_brush = new SolidBrush(new ColorRGB(0, 255, 0));
        private SolidPen _red_pen = new SolidPen(new ColorRGB(255, 0, 0));

        private Document m_doc;
        private Page m_page;
        private StringFormat m_sf_LTR;
        private StringFormat m_sf_RTL;
        private string m_str;

        private float m_left;
        private float m_top;
        private float m_width;
        private float m_height;

		public static string TestData { get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, TestDataPathRelative)); } }
	    public string OutputFolder { get { return TestContext.CurrentContext.TestDirectory; } }
		public string FontPath1 { get { return @"C:\Windows\Fonts\"; } }
		public static string FontPath2 { get { return System.IO.Path.Combine(TestData, Fonts2PathRelative); } }

		[SetUp]
        public void TestInitialize()
        {
            m_doc = new Document();
            m_page = new Page(PaperFormat.A4);
            m_doc.Pages.Add(m_page);

            m_sf_LTR = new StringFormat();
            m_sf_RTL = new StringFormat();
            m_sf_RTL.DirectionRightToLeft = true;

            m_str = string.Empty;

            m_left = m_page.Width - 30;
            m_top = 0;
            m_width = m_page.Width - 2 * 30;
            m_height = 30;
        }

		[TearDown]
        public void TestCleanup()
        {
            m_doc.Dispose();
        }

        [Test]
        public void TestArabicString()
        {
            //             1. Text may be left to right direction or right to left direction (arabic, hebrew, etc.)
            //             2. Text may be longer then one string line.
            //             3. Text may contains numbers, braces and something else.

            // only arabic: one word, couple words, phrase, sentence
            {
                m_top += 30;
                // Hello
                m_str = "مرحبا";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                // Good day!
                m_str = "اليوم جيد!";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);
                // m_page.Canvas.DrawString(m_str, _fontArial, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                // Period (1990 - present)
                m_str = "فترة )1990 - الحاضر(";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                // Desert climate: to account for 80% of the total area. 
                m_str = "المناخ الصحراوي : يحتل نسبة 80 ٪ من المساحة الكلية.";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                // Have a nice day!
                m_str = "وقد لطيفة اليوم!";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);
            }

            {
                m_height = _fontArialUnicodeMS.GetTextHeight() + 12;

                // one arabic word
                m_top += 30;
                // Good
                m_str = "جيد";
                m_page.Canvas.DrawString(m_str, _fontArialUnicodeMS, _black_brush, m_left, m_top, m_sf_RTL);

                // one arabic word with diacritic mark
                // check for different fonts
                m_top += 30;
                m_str = "ډُډ٘ډؕډؐ";
                m_page.Canvas.DrawString(m_str, _fileFontArabtype, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                //Complex Arabic Example;
                m_str = "ﻏؤﺘىإﺦ ۏڼحؐ";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);
            }

            m_doc.Save(OutputFolder + @"\TestRightToLeft_Arabic.pdf");

			//Process.Start("TestRightToLeft_Arabic.pdf");
        }

        [Test]
        public void TestHebrewString()
        {
            //             1. Text may be left to right direction or right to left direction (arabic, hebrew, etc.)
            //             2. Text may be longer then one string line.
            //             3. Text may contains numbers, braces and something else.

            // only hebrew: one word, couple words, phrase, sentence
            {
                m_top += 30;
                // Hello
                m_str = "שלום";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                // Good day!
                m_str = "יום טוב!";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                // Period (1990 - present)
                m_str = "תקופה )1990 - היום(";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                m_str = "הלוח העברי חוזר על עצמו )ביחס למולד הירח וליום בשבוע( ";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                m_str = "כמעט במדויק מדי 13 מחזורים של 19 שנים, שהם 247 שנה.";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                m_str = "שיהיה לך יום טוב!";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);
            }

            {
                m_height = _fontArialUnicodeMS.GetTextHeight() + 12;

                // one hebrew word
                m_top += 30;
                // Good
                m_str = "טוב";
                m_page.Canvas.DrawString(m_str, _fontArialUnicodeMS, _black_brush, m_left, m_top, m_sf_RTL);
            }

            {
                m_top += 30;
                //Complex Hebrew Example;
                m_str = "כשּׂנקַֿסגּיבַף";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                //Hebrew String With Marks;
                m_str = "סֶס֕";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                // Year
                m_str = "הַשָנָה";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                m_str = "הַחדֶשׁ הַזֶה לָכֶם ראשׁ חֳדָשִים רִאשׁוֹן הוּא לָכֶם לְחָדְשֵי הַשָנָה";
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                m_str = "הַחדֶשׁ הַזֶה לָכֶם ראשׁ חֳדָשִים רִאשׁוֹן הוּא לָכֶם לְחָדְשֵי הַשָנָה";
                m_page.Canvas.DrawString(m_str, _fileFontRod, _black_brush, m_left, m_top, m_sf_RTL);
            }

            {
                // check for different fonts
                m_top += 30;
                m_page.Canvas.DrawString(m_str, _fileFontHebrew, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                m_page.Canvas.DrawString("\u05DC", _fileFontHebrew, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                m_page.Canvas.DrawString(m_str, _fileFontGisha, _black_brush, m_left, m_top, m_sf_RTL);

                m_top += 30;
                m_page.Canvas.DrawString(m_str, _fileFontRod, _black_brush, m_left, m_top, m_sf_RTL);
            }

            m_doc.Save(OutputFolder + @"\TestRightToLeft_Hebrew.pdf");

			//Process.Start("TestRightToLeft_Hebrew.pdf");
        }

        [Test]
        public void TestWithCoordinates()
        {
            // first part: Left To Right Text
            {
                m_left = 30;
                m_top = 0;

                //only left and top
                m_str = "Left & Top";

                // left
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Left, m_left, m_top, m_sf_LTR);
                // unknown
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Center, m_left, m_top, m_sf_LTR);
                // unknown
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Right, m_left, m_top, m_sf_LTR);
                // if 1 string then left else justify
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Justify, m_left, m_top, m_sf_LTR);

                // with width and height
                m_str = "Width & Height";

                // left
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Left, m_left, m_top, m_width, m_height, m_sf_LTR);
                // center
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Center, m_left, m_top, m_width, m_height, m_sf_LTR);
                // right
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Right, m_left, m_top, m_width, m_height, m_sf_LTR);
                // if 1 string then left else justify
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Justify, m_left, m_top, m_width, m_height, m_sf_LTR);
            }

            // second part: Right To Left Text
            {
                m_left = 30;
                m_top = 300;

                //only left and top
                //str = "Left & Top";
                m_str = "اليسار والأعلى";

                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Left, m_left, m_top, m_sf_RTL);
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Center, m_left, m_top, m_sf_RTL);
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Right, m_left, m_top, m_sf_RTL);
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Justify, m_left, m_top, m_sf_RTL);

                // with width and height
                //str = "Width & Height";
                m_str = "العرض والارتفاع";

                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Left, m_left, m_top, m_width, m_height, m_sf_RTL);
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Center, m_left, m_top, m_width, m_height, m_sf_RTL);
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Right, m_left, m_top, m_width, m_height, m_sf_RTL);
                m_top += 30;
                drawString(m_str, m_page, _fontSansSerif, _black_brush, _red_pen, HorizontalAlign.Justify, m_left, m_top, m_width, m_height, m_sf_RTL);

                m_top += 30;
                //DrawString(str, page, _fontSansSerif, br, pen, left, top, width, height);
                m_page.Canvas.DrawRoundedRectangle(_red_pen, new RectangleF(m_left, m_top, m_width, m_height), 5);
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, new RectangleF(m_left, m_top, m_width, m_height));

                m_top += 30;
                //DrawString(str, page, _fontSansSerif, br, pen, left, top);
                m_page.Canvas.DrawRoundedRectangle(_red_pen, new RectangleF(m_left, m_top, m_width, m_height), 5);
                m_page.Canvas.DrawString(m_str, _fontSansSerif, _black_brush, m_left, m_top);
            }

            m_doc.Save(OutputFolder + @"\TestWithCoordinates.pdf");

			//Process.Start("TestWithCoordinates.pdf");
        }

        [Test]
        public void TestRightToLeftForDocs()
        {
            Document doc = new Document();
            Page page = new Page(PaperFormat.A4);

            StringFormat sf = new StringFormat();
            sf.DirectionRightToLeft = true;

            float left = page.Width - 30;
            float top = 0;

            top += 30;
            page.Canvas.DrawString("وقد لطيفة اليوم!", _fontArial, _black_brush, left, top, sf);

            top += 30;
            page.Canvas.DrawString("שיהיה לך יום טוב!", _fontArial, _black_brush, left, top, sf);

            doc.Pages.Add(page);
            doc.Save(OutputFolder + @"\DrawRightToLeftText.pdf");
            doc.Dispose();

			//Process.Start("DrawRightToLeftText.pdf");
        }

        [Test]
        public void TestGraphicsBehaviour()
        {
            int width = 600;
            int height = 600;
            Bitmap bitmap = new Bitmap(width, height);

            Graphics gr = Graphics.FromImage(bitmap);
            gr.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.White), 0, 0, width, height);

            float left = 200;
            gr.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Red), left, 0, left, height);

            System.Drawing.Brush brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

            System.Drawing.StringFormat sf_LTR = new System.Drawing.StringFormat();
            System.Drawing.StringFormat sf_RTL = new System.Drawing.StringFormat(StringFormatFlags.DirectionRightToLeft);

            string str = "Hello - جيد טוב اليوم \n new line";

            // how to get justification
            gr.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Green), new RectangleF(left, 30, 300, 70));
            gr.DrawString(str, _fontArialWindows, brush, new PointF(left, 30), sf_LTR);
            // gr.DrawString(str, _fontArialWindows, brush, new RectangleF(left, 30, 300, 60), sf_LTR);

            gr.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Green), new RectangleF(left, 70, 300, 70));
            gr.DrawString(str, _fontArialWindows, brush, new PointF(left, 70), sf_RTL);
            // gr.DrawString(str, _fontArialWindows, brush, new RectangleF(left, 70, 300, 60), sf_RTL);

            str = "1. اليوم";
            gr.DrawString(str, _fontArialWindows, brush, new PointF(left, 110), sf_LTR);
            gr.DrawString(str, _fontArialWindows, brush, new PointF(left, 150), sf_RTL);

            str = "اليوم جيد!";
            gr.DrawString(str, _fontArialWindows, brush, new PointF(left, 190), sf_LTR);
            gr.DrawString(str, _fontArialWindows, brush, new PointF(left, 230), sf_RTL);

            gr.DrawString(str, _fontArialWindows, brush, new RectangleF(left, 270, 60, 60), sf_LTR);
            gr.DrawString(str, _fontArialWindows, brush, new RectangleF(left, 310, 60, 60), sf_RTL);

            bitmap.Save(OutputFolder + @"\TestGraphicsBehaviour.png");
        }

        [Test]
        public void TestGlyphsAtPDF()
        {
            m_str = "\ufb01";
            m_top += 30;
            m_page.Canvas.DrawString(m_str, _fontArialUnicodeMS, _black_brush, m_left, m_top, m_sf_RTL);

            m_doc.Save(OutputFolder + @"\TestGlyphsAtPDF.pdf");

			//Process.Start("TestGlyphsAtPDF.pdf");
        }

        [Test]
        public void TestMosaicWithUniscribe()
        {
            m_left = 30;
            m_width = 0;
            m_height = _fontArialUnicodeMS.GetTextHeight() + 12;

            // one arabic word with kashida justification
            m_top += 30;
            m_sf_RTL.HorizontalAlign = HorizontalAlign.Justify;
            m_str = "فترة جيد )1990 - الحاضر( ";
            m_width = _fontArialUnicodeMS.GetTextWidth(m_str) * 1.3f;
            m_str += "\n next line جيد";
            m_page.Canvas.DrawRectangle(_green_brush, m_left, m_top, m_width, m_height * 2);
            m_page.Canvas.DrawString(m_str, _fontArialUnicodeMS, _black_brush, new RectangleF(m_left, m_top, m_width, m_height * 2), m_sf_RTL);
            m_sf_RTL.HorizontalAlign = HorizontalAlign.Left;

            // mixed text (english, hebrew, arabic) (direction LTR)
            m_top += 60;
            m_sf_LTR.HorizontalAlign = HorizontalAlign.Justify;
            m_str = "Hello everybody - جيد טוב اليوم";
            m_width = _fontArialUnicodeMS.GetTextWidth(m_str) * 1.3f;
            m_str += "\n next line";
            m_page.Canvas.DrawRectangle(_green_brush, m_left, m_top, m_width, m_height * 2);
            m_page.Canvas.DrawString(m_str, _fontArialUnicodeMS, _black_brush, new RectangleF(m_left, m_top, m_width, m_height * 2), m_sf_LTR);
            m_sf_LTR.HorizontalAlign = HorizontalAlign.Left;

            // mixed text (english, hebrew, arabic) (direction RTL)
            m_top += 60;
            m_sf_RTL.HorizontalAlign = HorizontalAlign.Justify;
            m_str = "Hello everybody فترة )1990 - الحاضر( world";
            m_width = _fontArialUnicodeMS.GetTextWidth(m_str) * 1.3f;
            m_str += "\n next line";
            m_page.Canvas.DrawRectangle(_green_brush, m_left, m_top, m_width, m_height * 2);
            m_page.Canvas.DrawString(m_str, _fontArialUnicodeMS, _black_brush, new RectangleF(m_left, m_top, m_width, m_height * 2), m_sf_RTL);
            m_sf_RTL.HorizontalAlign = HorizontalAlign.Left;

            // mixed text
            m_top += 70;
            m_page.Canvas.DrawString("Hello - יום טוב!", _fontArial, _black_brush, 30, m_top, m_sf_LTR);

            m_doc.Save(OutputFolder + @"\TestMosaicWithUniscribe.pdf");

			//Process.Start("TestMosaicWithUniscribe.pdf");
        }

        [Test]
        public void TestParagraphs_Complex()
        {
            m_left = 30;
            m_width = 100;
            m_height = _fontArialUnicodeMS.GetTextHeight();

            // arabic paragraph
            m_top += 30;
            m_sf_RTL.HorizontalAlign = HorizontalAlign.Justify;
            m_str = "الوطن 1 العربي 2 يعرف 3 كذلك 4 باسم 5 الوطن 6 العربي 7 الكبير 8 والعالم 9 العربي 10";
            m_page.Canvas.DrawString(m_str, _fontArialUnicodeMS, _black_brush,
                new RectangleF(m_left, m_top, m_width, m_height * 7), m_sf_RTL);

            // hebrew paragraph
            m_top += m_height * 8;
            m_sf_RTL.HorizontalAlign = HorizontalAlign.Justify;
            m_str = "הלוח 1 העברי 2 הוא 3 לוח 4 שנה 5 המבוסס 6 על 7 שילוב 8 של 9 מחזור 10 הירח 11 ומחזור 12 השמש 13 (לוח 14 לוניסולארי 15 ).";
            m_page.Canvas.DrawString(m_str, _fontArialUnicodeMS, _black_brush,
                new RectangleF(m_left, m_top, m_width, m_height * 11), m_sf_RTL);

            m_doc.Save(OutputFolder + @"\TestParagraphs_Complex.pdf");

			//Process.Start("TestParagraphs_Complex.pdf");
        }

        [Test]
        public void TestParagraphs_Simple()
        {
            m_left = 30;

            m_top += 30;
            m_str = "123 \t11234567890 1234567890";
            m_page.Canvas.DrawString(m_str, _fontArialUnicodeMS, _black_brush,
                new RectangleF(m_left, m_top, _fontArialUnicodeMS.GetTextWidth("123 1"), 30 * 10), m_sf_LTR);

            m_doc.Save(OutputFolder + @"\TestParagraphs_Simple.pdf");

			//Process.Start("TestParagraphs_Simple.pdf");
        }

        [Test]
        public void TestComplexText()
        {
            m_left = 30;

            // arabic
            StringBuilder sb = new StringBuilder();
            sb.Append("عمومًا فإن الدولة العباسية بمعنى الدولة قد سقطت مع سقوط بغداد عام 1258، أما الخلافة العباسية فقد استمرت ");
            sb.Append("حتى 1517 كرمز للدولة ودورها الديني في كنف الدولة المملوكية؛ ولم تسقط الخلافة العباسية إلا بعد سقوط الدولة المملوكية، ففي أعقاب معركة مرج دابق التي ");
            sb.Append("انتصر بها السلطان سليم الأول العثماني على المماليك، توجه بجيشه نحو الجنوب، ففتح أغلب مدن بلاد الشام سلمًا ومنها انعطف نحو مصر حيث هزم آخر المماليك ");
            sb.Append(" الأشرف طومان باي بعد معركة الريدانية قرب القاهرة، وقد اصطحب معه لدى رجوعه من القاهرة آخر الخلفاء العباسيين المتوكل على الله الثالث، والذي تنازل له عن");
            sb.Append("الخلافة وسلّمه رموزها أي بردة النبي محمد وسيف عمر بن الخطاب.");
            m_str = sb.ToString();

            drawParagraph(5, 15, 1);
            drawParagraphImage("arabic_text_in_web_browser.png");

            // arabci next
            sb.Remove(0, sb.Length);
            sb.Append("هذه الدول العربية حصلت على استقلالها أثناء أو بعد الحرب العالمية الثانية، جمهورية لبنان في عام 1943، والجمهورية العربية السورية والمملكة الأردنية الهاشمية ");
            sb.Append("في عام 1946، ليبيا في عام 1951، وجمهورية مصر في عام 1952، والمملكة المغربية وتونس في عام 1956، والعراق في عام 1932 والكويت 1961، ");
            sb.Append("والجزائر عام 1962 والإمارات العربية المتحدة في عام 1971. على النقيض من ذلك، كانت المملكة العربية السعودية تسيطر على نجد والأحساء عندما هزم العثمانيون ");
            sb.Append("في الحرب العالمية الأولى ثم مدت سيطرتها إلى الحجاز حتى أعلان التأسيس عام 1932. المملكة المتوكلية اليمنية أنفصلت من الدولة العثمانية في عام 1918. أما ");
            sb.Append("سلطنة عمان مع أنها خضعت للحكم الفارسي لفترة قصيرة وبعدها من الاستعمار البرتغالي، إلا أنها تتمتع بحكم ذاتي منذ القرن الثامن.");
            m_str = sb.ToString();

            drawParagraph(5, 11, 1);
            drawParagraphImage("arabic_text_in_web_browser_next.png");

            // hebrew
            sb.Remove(0, sb.Length);
            sb.Append("החודש העברי מבוסס על מחזור שינוי צורתו של הירח, ממולד הירח, עבור במילואו ");
            sb.Append("וכלה במולד הבא. האופן שבו נראה הירח לצופה מכדור הארץ תלוי במיקומו של ");
            sb.Append("הירח ביחס לשמש, במיקומו ביחס לכדור הארץ ובמיקום שניהם ביחס לשמש. משך ");
            sb.Append("הזמן בין מולד ירח אחד למשנהו נקרא החודש הסינודי, ומשכו נובע הן מסיבוב ");
            sb.Append("הירח סביב כדור הארץ, והן מסיבוב כדור הארץ סביב השמש. משך החודש הסינודי ");
            sb.Append("אינו קבוע, ואורכו הממוצע הוא בזמננו 29.5305888531 ימים )29 ימים, 12 ");
            sb.Append("שעות, 44 דקות, ו-2.9 שניות בקירוב(.");
            m_str = sb.ToString();

            drawParagraph(7, 11.4f, 2.0f / 3);
            drawParagraphImage("hebrew_text_in_web_browser.png");

            m_doc.Save(OutputFolder + @"\TestComplexText.pdf");

			//Process.Start("TestComplexText.pdf");
        }

        private void drawParagraphImage(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                return;

            Image image = new Image(fileName);
            float coeff = image.Height / image.Width;
            m_page.Canvas.DrawImage(image, m_left, m_top, m_width, m_width * coeff);

            m_top += m_width * coeff;
        }

        private void drawParagraph(int count, float fontSize, float coeff)
        {
            m_top += 30;
            // m_sf_RTL.HorizontalAlign = HorizontalAlign.Justify;

            _fontArial.Size = fontSize;
            m_height = _fontArialUnicodeMS.GetTextHeight();

            m_page.Canvas.DrawString(m_str, _fontArial, _black_brush,
                new RectangleF(m_left + m_width * (1 - coeff), m_top, m_width * coeff, m_page.Height - 2 * 30), m_sf_RTL);
            m_top += m_height * count + 30;
        }

        private void drawString(string str, Page page, Font fnt, Brush br, Pen pen,
            HorizontalAlign halign, float left, float top, StringFormat sf)
        {
            page.Canvas.DrawRoundedRectangle(pen, new RectangleF(left, top, m_width, m_height), 5);
            sf.HorizontalAlign = halign;
            str = str + " and " + halign.ToString();
            page.Canvas.DrawString(str, fnt, br, left, top, sf);
            sf.HorizontalAlign = HorizontalAlign.Left;
        }

        private void drawString(string str, Page page, Font fnt, Brush br, Pen pen,
            HorizontalAlign halign, float left, float top, float width, float height, StringFormat sf)
        {
            page.Canvas.DrawRoundedRectangle(pen, new RectangleF(left, top, m_width, m_height), 5);
            sf.HorizontalAlign = halign;
            str = str + " and " + halign.ToString();
            page.Canvas.DrawString(str, fnt, br, new RectangleF(left, top, width, height), sf);
            sf.HorizontalAlign = HorizontalAlign.Left;
        }

    }
}
