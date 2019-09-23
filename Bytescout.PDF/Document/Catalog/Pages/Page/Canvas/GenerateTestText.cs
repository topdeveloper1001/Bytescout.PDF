using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bytescout.PDF
{
    //q, Q, cm
    //BT, ET
    //Tc, Tw, Tz, TL, Tf, Tr, Ts
    //Td, TD, Tm, T*
    //Tj, TJ, ', "
    //Do

    //Tf, Tc, Tz, Tw, Tr, TL

    internal class GenerateTestText
    {
        public static void DrawRectangles(Document doc, Document docNew)
        {
            Random rnd = new Random(255);
            for (int i = 0; i < doc.Pages.Count; ++i)
                docNew.Pages.Add(doc.Pages[i]);
            for (int i = 0; i < doc.Pages.Count; ++i)
            {
                //docNew.Pages[i].Canvas.BlendMode = BlendMode.Screen;
                Canvas canvas = doc.Pages[i].Canvas;
                List<CoordinateText> listStrings = new List<CoordinateText>();
                TextExtractor.fillListStrings(listStrings, new float[] { 1, 0, 0, 1, 0, 0}, doc.Pages[i].GetDictionary()["Contents"], doc.Pages[i].Resources);
                for (int j = 0; j < listStrings.Count; ++j)
                {
                    docNew.Pages[i].Canvas.DrawRectangle(new SolidPen(new ColorRGB((byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255)), 0.01f), listStrings[j].Coordinate.X, doc.Pages[i].Canvas.Height - (listStrings[j].Coordinate.Y + listStrings[j].TextHeight), listStrings[j].TextWidth, listStrings[j].TextHeight * 1.2f);
                    //docNew.Pages[i].Canvas.DrawLine(new SolidPen(new ColorRGB(255, 0, 0), 3), listStrings[j].Coordinate.X, listStrings[j].Coordinate.Y, listStrings[j].Coordinate.X + 1, listStrings[j].Coordinate.Y + 1);
                }
            }
        }

        public static void SimpleTextIn10Lines ()
        {
            Document doc = new Document();
            //List<CoordinateText> listStrings = new List<CoordinateText>();
            //TextExtractor.fillListStrings(listStrings, new float[] { 1, 0, 0, 1, 0, 0 }, doc.Pages[i].Canvas. contents, resources);
            doc.Pages.Add(new Page(PaperFormat.A4));
            //init(doc);

            Canvas canvas = doc.Pages[0].Canvas;

            Font fontHelvetica12 = /*new Font("Arial", 12);// */new Font(StandardFonts.Helvetica, 12);
            Font fontHelvetica6 = new Font(StandardFonts.CourierBold, 6);
            //Font fontArial12 = new Font("Arial", 12);
            //fontHelvetica6.WriteParameters(canvas.Stream, canvas.Resources);
            //canvas.addOperation(new CharacterSpacing(100));
            //canvas.addOperation(new TextRise(-30));
            
            //canvas.addOperation(new SaveGraphicsState());
            //canvas.addOperation(new MoveTextPos(100, 0));
            //canvas.addOperation(new HorizontalScaling(50.0f));
            //canvas.addOperation(new MoveTo(100, 0));
            fontHelvetica12.BaseFont.AddStringToEncoding("hello, world");
            fontHelvetica12.WriteParameters(canvas.Stream, canvas.Resources);
            //canvas.addOperation(new RestoreGraphicsState());

            canvas.addOperation(new Transform(1.0f, 0.0f, 0.0f, -1.0f, 0.0f, canvas.Height));
            canvas.addOperation(new BeginText());
            
            //canvas.addOperation(new SaveGraphicsState());
            //canvas.addOperation(new Transform(2.0f, 0, 0, 2.0f, 100, 100));
            //canvas.addOperation(new TextMatrix(0.5f, 0, 0, 0.5f, -50, -50));
            canvas.addOperation(new TextMatrix(1.0f, 0, 0, -1.0f, 0, fontHelvetica12.GetTextHeight()));
            //canvas.addOperation(new MoveTextPos(0.0f, canvas.Height - fontHelvetica12.GetTextHeight()));
            //
            //canvas.addOperation(new RestoreGraphicsState());
            float space = fontHelvetica12.GetCharWidth(' ');
            fontHelvetica12.WriteParameters(canvas.Stream, canvas.Resources);
            canvas.addOperation(new TextLeading(fontHelvetica12.GetTextHeight()));
            //canvas.addOperation(new MoveTextPos(0.0f, -fontHelvetica12.GetTextHeight()));
            //canvas.addOperation(new ShowText(fontHelvetica12.BaseFont.ConvertStringToFontEncoding("hello, world")));
            //canvas.addOperation(new SaveGraphicsState());
            //canvas.addOperation(new WordSpacing(2 * space));
            //canvas.addOperation(new MoveTextPos(0.0f, (canvas.Height - fontHelvetica12.GetTextHeight())));
            //canvas.addOperation(new ShowText(fontHelvetica12.BaseFont.ConvertStringToFontEncoding("hello, world")));
            //canvas.addOperation(new CharacterSpacing(fontHelvetica12.GetCharWidth(' ')));
            //canvas.addOperation(new HorizontalScaling(33.33f));
            //canvas.addOperation(new ShowTextStrings(new object[] { fontHelvetica12.BaseFont.ConvertStringToFontEncoding(" "), -1000.0f}));
            //canvas.addOperation(new HorizontalScaling(99.0f));
            canvas.addOperation(new ShowText(fontHelvetica12.BaseFont.ConvertStringToFontEncoding("hello, world")));
            
            //canvas.addOperation(new RestoreGraphicsState());
            //canvas.addOperation(new Transform(3.0f, 0, 0, 3.0f, 0, -2 * (canvas.Height - fontHelvetica12.GetTextHeight())));
            //canvas.addOperation(new MoveTextPos(0.0f, (canvas.Height - fontHelvetica12.GetTextHeight()) / 2));
            //canvas.addOperation(new TextMatrix(2.0f, 0, 0, 2.0f, 0, 0));
            //canvas.addOperation(new MoveTextPosWithLeading(0, -10));
            
            //canvas.addOperation(new ShowTextFromNewLine(fontHelvetica12.BaseFont.ConvertStringToFontEncoding("hello, world")));
            //canvas.addOperation(new ShowText(fontHelvetica12.BaseFont.ConvertStringToFontEncoding("llo, world")));
            
            
            //canvas.addOperation(new WordSpacing(0.0f));
            /*canvas.addOperation(new CharacterSpacing(fontHelvetica12.GetCharWidth(' ') / 2));
            //canvas.addOperation(new TextLeading(fontHelvetica12.GetTextHeight()));
            canvas.addOperation(new ShowTextFromNewLine(fontHelvetica12.BaseFont.ConvertStringToFontEncoding("hello, world")));
            canvas.addOperation(new CharacterSpacing(fontHelvetica12.GetCharWidth(' ')));
            canvas.addOperation(new ShowTextFromNewLine(fontHelvetica12.BaseFont.ConvertStringToFontEncoding("hello, world")));*/
            canvas.addOperation(new EndText());

            /*canvas.addOperation(new BeginText());
            canvas.addOperation(new ShowText(fontHelvetica12.BaseFont.ConvertStringToFontEncoding("lo, world")));
            canvas.addOperation(new EndText());*/
            canvas.addOperation(new Linewidth(0.001f));
            canvas.addOperation(new MoveTo(fontHelvetica12.GetTextWidth("hello,"), canvas.Height));
            canvas.addOperation(new LineTo(fontHelvetica12.GetTextWidth("hello,"), 0.0f));
            canvas.addOperation(new FillStrokePathNonZero());
            canvas.addOperation(new MoveTo(fontHelvetica12.GetTextWidth("hello, "), canvas.Height));
            canvas.addOperation(new LineTo(fontHelvetica12.GetTextWidth("hello, "), 0.0f));
            canvas.addOperation(new FillStrokePathNonZero());
            canvas.addOperation(new MoveTo(fontHelvetica12.GetTextWidth("hello,  "), canvas.Height));
            canvas.addOperation(new LineTo(fontHelvetica12.GetTextWidth("hello,  "), 0.0f));
            canvas.addOperation(new FillStrokePathNonZero());
            canvas.addOperation(new MoveTo(fontHelvetica12.GetTextWidth("hello,   "), canvas.Height));
            canvas.addOperation(new LineTo(fontHelvetica12.GetTextWidth("hello,   "), 0.0f));
            canvas.addOperation(new FillStrokePathNonZero());
            
            //doc.Pages[0].Canvas.DrawString("Hello", new Font(StandardFonts.Helvetica, 12), new SolidBrush(), 0, 0);
            doc.Save("SimpleTextIn10Lines.pdf");
	        Process.Start("SimpleTextIn10Lines.pdf");
        }

        public static void GenerateCoordinateTests(string testName, bool open)
        {
            Document doc = new Document();
            doc.Pages.Add(new Page(PaperFormat.A4));
            Canvas canvas = doc.Pages[0].Canvas;
            Font fontHelvetica12 = new Font(StandardFonts.Helvetica, 12);
            //canvas.DrawString("hello world", fontHelvetica12, new SolidBrush(), 0, 0);
            string text = "1, 1";
            //canvas.DrawLine(new SolidPen(), 70, -70, 72, -72);
            //canvas.DrawLine(new SolidPen(), 50, -50, 52, -52);
            //canvas.addOperation(new Transform(1.0f, 0.0f, 0.0f, 1.0f, 0, canvas.Height));
            //canvas.addOperation(new Transform(1.0f, 0.0f, 0.0f, 1.0f, 10, 10));
            canvas.addOperation(new BeginText());
            fontHelvetica12.WriteParameters(canvas.Stream, canvas.Resources);
            //canvas.addOperation(new Transform(1.0f, 0.0f, 0.0f, 1.0f, -35, -35));
            //canvas.addOperation(new TextMatrix(1.0f, 0.0f, 0.0f, 1.0f, 0, 0));
            canvas.addOperation(new MoveTextPosWithLeading(100, -100));
            canvas.addOperation(new MoveTextPosToNextLine());
            canvas.addOperation(new MoveTextPos(0, -fontHelvetica12.GetTextHeight()));
            //canvas.addOperation(new TextMatrix(1.0f, 0.0f, 0.0f, 1.0f, 10, 10));
            
            fontHelvetica12.BaseFont.AddStringToEncoding(text);
            //canvas.addOperation(new ShowTextFromNewLineWithSpacing(5, 5, fontHelvetica12.BaseFont.ConvertStringToFontEncoding(text)));
            canvas.addOperation(new ShowText(fontHelvetica12.BaseFont.ConvertStringToFontEncoding(text)));
            canvas.addOperation(new EndText());
            doc.Save(testName + ".pdf");
			if (open)
				Process.Start(testName + ".pdf");
        }

        private static void init(Document doc)
        {
            
        }
    }
}
