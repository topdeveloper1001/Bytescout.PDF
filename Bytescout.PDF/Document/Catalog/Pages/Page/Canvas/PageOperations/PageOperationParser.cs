using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class PageOperationParser
    {
        internal PageOperationParser(IPDFObject contents)
        {
            m_parseOperation = new ParseOperation(ParserMode.Full);
            m_contents = contents;
            m_lexer = new Lexer(Stream.Null, null, null, 512);
            m_stack = new Stack<IPDFObject>();
            nextStream();
        }

        internal PageOperationParser(Stream stream)
        {
            m_parseOperation = new ParseOperation(ParserMode.Full);
            stream.Position = 0;
            m_lexer = new Lexer(stream, null, null, 512);
            m_stack = new Stack<IPDFObject>();
        }

        internal void SetParserMode(ParserMode mode)
        {
            m_parseOperation = new ParseOperation(mode);
        }

        internal IPDFPageOperation Next()
        {
            for (; ; )
            {
                if (Lexer.IsEOL(m_lexer.LastParsedByte))
                    m_lexer.SkipEOL();

                switch (m_lexer.LastParsedByte)
                {
                    case -1:
                        if (!nextStream())
                            return null;
                        break;
                    case '%':
                        m_lexer.ReadComment();
                        break;
                    case '/':
                        parseName();
                        break;
                    case '<':
                        parseHexStringOrDictonary();
                        break;
                    case '[':
                        parseArray();
                        break;
                    case '(':
                        parseString();
                        break;
                    case '-':
                    case '+':
                    case '.':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        parseNumber();
                        break;
                    default:
                        IPDFPageOperation operation = parseOperator();
                        m_stack.Clear();
                        if (operation == null)
                            continue;
                        return operation;
                }
            }
        }

        private bool nextStream()
        {
            if (m_contents == null)
                return false;
            if (m_contents is PDFDictionaryStream)
            {
                PDFDictionaryStream dictStream = m_contents as PDFDictionaryStream;
                dictStream.Decode();
                Stream s = dictStream.GetStream();
                s.Position = 0;
                m_lexer.SetStream(s);
                m_contents = null;
                return true;
            }
            else if (m_contents is PDFArray)
            {
                PDFArray arr = m_contents as PDFArray;
                if (arr.Count <= m_curStreamIndex)
                {
                    m_contents = null;
                    return false;
                }

                PDFDictionaryStream dict = arr[m_curStreamIndex] as PDFDictionaryStream;
                if (dict == null)
                {
                    m_curStreamIndex++;
                    return nextStream();
                }

                dict.Decode();
                Stream s = dict.GetStream();
                s.Position = 0;
                m_lexer.SetStream(s);
                m_curStreamIndex++;
                return true;
            }

            m_contents = null;
            return false;
        }

        private void parseNumber()
        {
            m_stack.Push(m_lexer.ParseNumber(m_lexer.LastParsedByte));
        }

        private void parseArray()
        {
            m_stack.Push(m_lexer.ParseArray(0, 0));
        }

        private void parseHexStringOrDictonary()
        {
            m_stack.Push(m_lexer.ParseHexStringOrDictionary(0, 0));
        }

        private void parseString()
        {
            m_stack.Push(m_lexer.ParseString(0, 0));
        }

        private void parseName()
        {
            m_stack.Push(m_lexer.ParseName());
        }

        #region General graphics state
        //w
        private IPDFPageOperation parseLineWidth()
        {
            if (m_stack.Count < 1)
                return null;

            PDFNumber num = m_stack.Pop() as PDFNumber;
            if (num == null)
                return null;
            return new Linewidth((float)num.GetValue());
        }

        //J
        private IPDFPageOperation parseLineCap()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber num = m_stack.Pop() as PDFNumber;
            if (num == null)
                return null;

            return new LineCap((LineCapStyle)num.GetValue());
        }

        //j
        private IPDFPageOperation parseLineJoin()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber num = m_stack.Pop() as PDFNumber;
            if (num == null)
                return null;

            return new LineJoin((LineJoinStyle)num.GetValue());
        }

        //M
        private IPDFPageOperation parseMiterLimit()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber m = m_stack.Pop() as PDFNumber;
            if (m == null)
                return null;
            return new MiterLimit((float)m.GetValue());
        }

        //d
        private IPDFPageOperation parseLineDash()
        {
            if (m_stack.Count < 2)
                return null;
            PDFNumber phase = m_stack.Pop() as PDFNumber;
            if (phase == null)
                return null;

            PDFArray arr = m_stack.Pop() as PDFArray;
            if (arr == null)
                return null;
            float[] dash = new float[arr.Count];
            for (int i = 0; i < dash.Length; ++i)
            {
                PDFNumber num = arr[i] as PDFNumber;
                if (num == null)
                    return null;
                dash[i] = (float)num.GetValue();
            }

            DashPattern pattern = new DashPattern(dash, (float)phase.GetValue());
            return new LineDash(pattern);
        }

        //ri
        private IPDFPageOperation parseRenderingIntent()
        {
            if (m_stack.Count < 1)
                return null;
            PDFName name = m_stack.Pop() as PDFName;
            if (name == null)
                return null;

            return new RenderingIntent(name.GetValue());
        }

        //i
        private IPDFPageOperation parseFlatnessTolerance()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber i = m_stack.Pop() as PDFNumber;
            if (i == null)
                return null;

            return new FlatnessTolerance((float)i.GetValue());
        }

        //gs
        private IPDFPageOperation parseGraphicsState()
        {
            if (m_stack.Count < 1)
                return null;
            PDFName name = m_stack.Pop() as PDFName;
            if (name == null)
                return null;

            return new GraphicsState(name.GetValue());
        }
        #endregion

        #region Special graphics state
        //q
        private IPDFPageOperation parseSaveGraphicsState()
        {
            return new SaveGraphicsState();
        }

        //Q
        private IPDFPageOperation parseRestoreGraphicsState()
        {
            return new RestoreGraphicsState();
        }

        //cm
        private IPDFPageOperation parseTransform()
        {
            if (m_stack.Count < 6)
                return null;
            PDFNumber f = m_stack.Pop() as PDFNumber;
            if (f == null)
                return null;
            PDFNumber e = m_stack.Pop() as PDFNumber;
            if (e == null)
                return null;
            PDFNumber d = m_stack.Pop() as PDFNumber;
            if (d == null)
                return null;
            PDFNumber c = m_stack.Pop() as PDFNumber;
            if (c == null)
                return null;
            PDFNumber b = m_stack.Pop() as PDFNumber;
            if (b == null)
                return null;
            PDFNumber a = m_stack.Pop() as PDFNumber;
            if (a == null)
                return null;

            return new Transform((float)a.GetValue(), (float)b.GetValue(), (float)c.GetValue(), (float)d.GetValue(), (float)e.GetValue(), (float)f.GetValue());
        }
        #endregion

        #region Path construction
        //m
        private IPDFPageOperation parseMoveTo()
        {
            if (m_stack.Count < 2)
                return null;
            PDFNumber y = m_stack.Pop() as PDFNumber;
            if (y == null)
                return null;
            PDFNumber x = m_stack.Pop() as PDFNumber;
            if (x == null)
                return null;

            return new MoveTo((float)x.GetValue(), (float)y.GetValue());
        }

        //l
        private IPDFPageOperation parseLineTo()
        {
            if (m_stack.Count < 2)
                return null;
            PDFNumber y = m_stack.Pop() as PDFNumber;
            if (y == null)
                return null;
            PDFNumber x = m_stack.Pop() as PDFNumber;
            if (x == null)
                return null;

            return new LineTo((float)x.GetValue(), (float)y.GetValue());
        }

        //c
        private IPDFPageOperation parseBezierCurve()
        {
            if(m_stack.Count < 6)
                return null;
            PDFNumber y3 = m_stack.Pop() as PDFNumber;
            if (y3 == null)
                return null;
            PDFNumber x3 = m_stack.Pop() as PDFNumber;
            if (x3 == null)
                return null;
            PDFNumber y2 = m_stack.Pop() as PDFNumber;
            if (y2 == null)
                return null;
            PDFNumber x2 = m_stack.Pop() as PDFNumber;
            if (x2 == null)
                return null;
            PDFNumber y1 = m_stack.Pop() as PDFNumber;
            if (y1 == null)
                return null;
            PDFNumber x1 = m_stack.Pop() as PDFNumber;
            if (x1 == null)
                return null;

            return new BezierCurve((float)x1.GetValue(), (float)y1.GetValue(), (float)x2.GetValue(), (float)y2.GetValue(), (float)x3.GetValue(), (float)y3.GetValue());
        }

        //v
        private IPDFPageOperation parseBezierCurve2()
        {
            if (m_stack.Count < 4)
                return null;
            PDFNumber y3 = m_stack.Pop() as PDFNumber;
            if (y3 == null)
                return null;
            PDFNumber x3 = m_stack.Pop() as PDFNumber;
            if (x3 == null)
                return null;
            PDFNumber y2 = m_stack.Pop() as PDFNumber;
            if (y2 == null)
                return null;
            PDFNumber x2 = m_stack.Pop() as PDFNumber;
            if (x2 == null)
                return null;

            return new BezierCurve2((float)x2.GetValue(), (float)y2.GetValue(), (float)x3.GetValue(), (float)y3.GetValue());
        }

        //y
        private IPDFPageOperation parseBezierCurve3()
        {
            if (m_stack.Count < 6)
                return null;
            PDFNumber y3 = m_stack.Pop() as PDFNumber;
            if (y3 == null)
                return null;
            PDFNumber x3 = m_stack.Pop() as PDFNumber;
            if (x3 == null)
                return null;
            PDFNumber y1 = m_stack.Pop() as PDFNumber;
            if (y1 == null)
                return null;
            PDFNumber x1 = m_stack.Pop() as PDFNumber;
            if (x1 == null)
                return null;

            return new BezierCurve3((float)x1.GetValue(), (float)y1.GetValue(), (float)x3.GetValue(), (float)y3.GetValue());
        }

        //h
        private IPDFPageOperation parseCloseSubpath()
        {
            return new CloseSubpath();
        }

        //re
        private IPDFPageOperation parseRectangle()
        {
            if (m_stack.Count < 4)
                return null;
            PDFNumber h = m_stack.Pop() as PDFNumber;
            if (h == null)
                return null;
            PDFNumber w = m_stack.Pop() as PDFNumber;
            if (w == null)
                return null;
            PDFNumber y = m_stack.Pop() as PDFNumber;
            if (y == null)
                return null;
            PDFNumber x = m_stack.Pop() as PDFNumber;
            if (x == null)
                return null;

            return new Rectangle((float)x.GetValue(), (float)y.GetValue(), (float)w.GetValue(), (float)h.GetValue());
        }
        #endregion

        #region Path painting
        //S
        private IPDFPageOperation parseStrokePath()
        {
            return new StrokePath();
        }

        //s
        private IPDFPageOperation parseCloseStrokePath()
        {
            return new CloseStrokePath();
        }

        //f
        //F
        private IPDFPageOperation parseFillPathNonZero()
        {
            return new FillPathNonZero();
        }

        //f*
        private IPDFPageOperation parseFillPathEvenOdd()
        {
            return new FillPathEvenOdd();
        }

        //B
        private IPDFPageOperation parseFillStrokePathNonZero()
        {
            return new FillStrokePathNonZero();
        }

        //B*
        private IPDFPageOperation parseFillStrokePathEvenOdd()
        {
            return new FillStrokePathEvenOdd();
        }

        //b
        private IPDFPageOperation parseCloseFillStrokePathNonZero()
        {
            return new CloseFillStrokePathNonZero();
        }

        //b*
        private IPDFPageOperation parseCloseFillStrokePathEvenOdd()
        {
            return new CloseFillStrokePathEvenOdd();
        }

        //n
        private IPDFPageOperation parseEndPath()
        {
            return new EndPath();
        }
        #endregion

        #region Clipping paths
        //W
        private IPDFPageOperation parseClipPathNonZero()
        {
            return new ClipPathNonZero();
        }

        //W*
        private IPDFPageOperation parseClipPathEvenOdd()
        {
            return new ClipPathEvenOdd();
        }
        #endregion

        #region Text objects
        //BT
        private IPDFPageOperation parseBeginText()
        {
            return new BeginText();
        }

        //ET
        private IPDFPageOperation parseEndText()
        {
            return new EndText();
        }
        #endregion

        #region Text state
        //Tc
        private IPDFPageOperation parseCharacterSpacing()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber cs = m_stack.Pop() as PDFNumber;
            if (cs == null)
                return null;

            return new CharacterSpacing((float)cs.GetValue());
        }

        //Tw
        private IPDFPageOperation parseWordSpacing()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber ws = m_stack.Pop() as PDFNumber;
            if (ws == null)
                return null;

            return new WordSpacing((float)ws.GetValue());
        }

        //Tz
        private IPDFPageOperation parseHorizontalScaling()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber scale = m_stack.Pop() as PDFNumber;
            if (scale == null)
                return null;

            return new HorizontalScaling((float)scale.GetValue());
        }

        //TL
        private IPDFPageOperation parseTextLeading()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber leading = m_stack.Pop() as PDFNumber;
            if (leading == null)
                return null;

            return new TextLeading((float)leading.GetValue());
        }

        //Tf
        private IPDFPageOperation parseTextFont()
        {
            if (m_stack.Count < 2)
                return null;
            PDFNumber size = m_stack.Pop() as PDFNumber;
            if (size == null)
                return null;
            PDFName name = m_stack.Pop() as PDFName;
            if (name == null)
                return null;

            return new TextFont(name.GetValue(), (float)size.GetValue());
        }

        //Tr
        private IPDFPageOperation parseTextRenderingMode()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber mode = m_stack.Pop() as PDFNumber;
            if (mode == null)
                return null;

            return new TextRenderingMode((int)mode.GetValue());
        }

        //Ts
        private IPDFPageOperation parseTextRise()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber tr = m_stack.Pop() as PDFNumber;
            if (tr == null)
                return null;

            return new TextRise((float)tr.GetValue());
        }
        #endregion

        #region Text positioning
        //Td
        private IPDFPageOperation parseMoveTextPos()
        {
            if (m_stack.Count < 2)
                return null;
            PDFNumber ty = m_stack.Pop() as PDFNumber;
            if (ty == null)
                return null;
            PDFNumber tx = m_stack.Pop() as PDFNumber;
            if (tx == null)
                return null;

            return new MoveTextPos((float)tx.GetValue(), (float)ty.GetValue());
        }

        //TD
        private IPDFPageOperation parseMoveTextPosWithLeading()
        {
            if (m_stack.Count < 2)
                return null;
            PDFNumber ty = m_stack.Pop() as PDFNumber;
            if (ty == null)
                return null;
            PDFNumber tx = m_stack.Pop() as PDFNumber;
            if (tx == null)
                return null;

            return new MoveTextPosWithLeading((float)tx.GetValue(), (float)ty.GetValue());
        }

        //Tm
        private IPDFPageOperation parseTextMatrix()
        {
            if (m_stack.Count < 6)
                return null;
            PDFNumber f = m_stack.Pop() as PDFNumber;
            if (f == null)
                return null;
            PDFNumber e = m_stack.Pop() as PDFNumber;
            if (e == null)
                return null;
            PDFNumber d = m_stack.Pop() as PDFNumber;
            if (d == null)
                return null;
            PDFNumber c = m_stack.Pop() as PDFNumber;
            if (c == null)
                return null;
            PDFNumber b = m_stack.Pop() as PDFNumber;
            if (b == null)
                return null;
            PDFNumber a = m_stack.Pop() as PDFNumber;
            if (a == null)
                return null;

            return new TextMatrix((float)a.GetValue(), (float)b.GetValue(), (float)c.GetValue(), (float)d.GetValue(), (float)e.GetValue(), (float)f.GetValue());
        }

        //T*
        private IPDFPageOperation parseMoveTextPosToNextLine()
        {
            return new MoveTextPosToNextLine();
        }
        #endregion

        #region Text showing
        //Tj
        private IPDFPageOperation parseShowText()
        {
            if (m_stack.Count < 1)
                return null;
            PDFString str = m_stack.Pop() as PDFString;
            if (str == null)
                return null;

            return new ShowText(str);
        }

        //TJ
        private IPDFPageOperation parseShowTextStrings()
        {
            if (m_stack.Count < 1)
                return null;
            PDFArray arr = m_stack.Pop() as PDFArray;
            if (arr == null)
                return null;
            object[] strings = new object[arr.Count];
            for (int i = 0; i < strings.Length; ++i)
            {
                IPDFObject obj = arr[i];
                if (obj is PDFNumber)
                    strings[i] = (float)(obj as PDFNumber).GetValue();
                else if (obj is PDFString)
                    strings[i] = obj;
                else
                    return null;
            }

            return new ShowTextStrings(strings);
        }

        //'
        private IPDFPageOperation parseShowTextFromNewLine()
        {
            if (m_stack.Count < 1)
                return null;
            PDFString str = m_stack.Pop() as PDFString;
            if (str == null)
                return null;

            return new ShowTextFromNewLine(str);
        }


        //"
        private IPDFPageOperation parseShowTextFromNewLineWithSpacing()
        {
            if (m_stack.Count < 3)
                return null;
            PDFString str = m_stack.Pop() as PDFString;
            if (str == null)
                return null;
            PDFNumber cs = m_stack.Pop() as PDFNumber;
            if (cs == null)
                return null;
            PDFNumber ws = m_stack.Pop() as PDFNumber;
            if (ws == null)
                return null;

            return new ShowTextFromNewLineWithSpacing((float)ws.GetValue(), (float)cs.GetValue(), str);
        }
        #endregion

        #region Type 3 fonts
        //d0
        private IPDFPageOperation parseSetWidthForType3()
        {
            if (m_stack.Count < 2)
                return null;
            PDFNumber wy = m_stack.Pop() as PDFNumber;
            if (wy == null)
                return null;
            PDFNumber wx = m_stack.Pop() as PDFNumber;
            if (wx == null)
                return null;
            return new SetWidthForType3((float)wx.GetValue(), (float)wy.GetValue());
        }

        //d1
        private IPDFPageOperation parseSetWidthAndBBoxForType3()
        {
            if (m_stack.Count < 6)
                return null;
            PDFNumber ury = m_stack.Pop() as PDFNumber;
            if (ury == null)
                return null;
            PDFNumber urx = m_stack.Pop() as PDFNumber;
            if (urx == null)
                return null;
            PDFNumber lly = m_stack.Pop() as PDFNumber;
            if (lly == null)
                return null;
            PDFNumber llx = m_stack.Pop() as PDFNumber;
            if (llx == null)
                return null;
            PDFNumber wy = m_stack.Pop() as PDFNumber;
            if (wy == null)
                return null;
            PDFNumber wx = m_stack.Pop() as PDFNumber;
            if (wx == null)
                return null;

            return new SetWidthAndBBoxForType3((float)wx.GetValue(), (float)wy.GetValue(), (float)llx.GetValue(), (float)lly.GetValue(), (float)urx.GetValue(), (float)ury.GetValue());
        }
        #endregion

        #region Color
        //CS
        private IPDFPageOperation parseColorSpaceForStroking()
        {
            if (m_stack.Count < 1)
                return null;
            PDFName name = m_stack.Pop() as PDFName;
            if (name == null)
                return null;

            return new ColorSpaceForStroking(name.GetValue());
        }

        //cs
        private IPDFPageOperation parseColorSpaceForNonStroking()
        {
            if (m_stack.Count < 1)
                return null;
            PDFName name = m_stack.Pop() as PDFName;
            if (name == null)
                return null;

            return new ColorSpaceForNonStroking(name.GetValue());
        }

        //SC
        private IPDFPageOperation parseColorForStroking()
        {
            if (m_stack.Count > 4)
                return null;
            float[] color = new float[m_stack.Count];
            for (int i = color.Length - 1; i >= 0; --i)
            {
                PDFNumber num = m_stack.Pop() as PDFNumber;
                if (num == null)
                    return null;
                color[i] = (float)num.GetValue();
            }

            return new ColorForStroking(color);
        }

        //SCN
        private IPDFPageOperation parseColorForStrokingEx()
        {
            if (m_stack.Count < 1 || m_stack.Count > 5)
                return null;

            string name = null;

            IPDFObject obj = m_stack.Peek();
            if (obj is PDFName)
            {
                name = (obj as PDFName).GetValue();
                m_stack.Pop();
            }
            else
                name = "";

            float[] color = new float[m_stack.Count];
            for (int i = color.Length - 1; i >= 0; --i)
            {
                PDFNumber num = m_stack.Pop() as PDFNumber;
                if (num == null)
                    return null;
                color[i] = (float)num.GetValue();
            }

            return new ColorForStrokingEx(color, name);
        }

        //sc
        private IPDFPageOperation parseColorForNonStroking()
        {
            if (m_stack.Count > 4)
                return null;
            float[] color = new float[m_stack.Count];
            for (int i = color.Length - 1; i >= 0; --i)
            {
                PDFNumber num = m_stack.Pop() as PDFNumber;
                if (num == null)
                    return null;
                color[i] = (float)num.GetValue();
            }

            return new ColorForNonStroking(color);
        }

        //scn
        private IPDFPageOperation parseColorForNonStrokingEx()
        {
            if (m_stack.Count < 1 || m_stack.Count > 5)
                return null;

            string name = null;

            IPDFObject obj = m_stack.Peek();
            if (obj is PDFName)
            {
                name = (obj as PDFName).GetValue();
                m_stack.Pop();
            }
            else
                name = "";

            float[] color = new float[m_stack.Count];
            for (int i = color.Length - 1; i >= 0; --i)
            {
                PDFNumber num = m_stack.Pop() as PDFNumber;
                if (num == null)
                    return null;
                color[i] = (float)num.GetValue();
            }

            return new ColorForNonStrokingEx(color, name);
        }

        //G
        private IPDFPageOperation parseGrayColorSpaceForStroking()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber gray = m_stack.Pop() as PDFNumber;
            if (gray == null)
                return null;

            return new GrayColorSpaceForStroking((float)gray.GetValue());
        }

        //g
        private IPDFPageOperation parseGrayColorSpaceForNonStroking()
        {
            if (m_stack.Count < 1)
                return null;
            PDFNumber gray = m_stack.Pop() as PDFNumber;
            if (gray == null)
                return null;

            return new GrayColorSpaceForNonStroking((float)gray.GetValue());
        }

        //RG
        private IPDFPageOperation parseRGBColorSpaceForStroking()
        {
            if (m_stack.Count < 3)
                return null;
            PDFNumber b = m_stack.Pop() as PDFNumber;
            if (b == null)
                return null;
            PDFNumber g = m_stack.Pop() as PDFNumber;
            if (g == null)
                return null;
            PDFNumber r = m_stack.Pop() as PDFNumber;
            if (r == null)
                return null;

            return new RGBColorSpaceForStroking((float)r.GetValue(), (float)g.GetValue(), (float)b.GetValue());
        }

        //rg
        private IPDFPageOperation parseRGBColorSpaceForNonStroking()
        {
            if (m_stack.Count < 3)
                return null;
            PDFNumber b = m_stack.Pop() as PDFNumber;
            if (b == null)
                return null;
            PDFNumber g = m_stack.Pop() as PDFNumber;
            if (g == null)
                return null;
            PDFNumber r = m_stack.Pop() as PDFNumber;
            if (r == null)
                return null;

            return new RGBColorSpaceForNonStroking((float)r.GetValue(), (float)g.GetValue(), (float)b.GetValue());
        }

        //K
        private IPDFPageOperation parseCMYKColorSpaceForStroking()
        {
            if (m_stack.Count < 4)
                return null;
            PDFNumber k = m_stack.Pop() as PDFNumber;
            if (k == null)
                return null;
            PDFNumber y = m_stack.Pop() as PDFNumber;
            if (y == null)
                return null;
            PDFNumber m = m_stack.Pop() as PDFNumber;
            if (m == null)
                return null;
            PDFNumber c = m_stack.Pop() as PDFNumber;
            if (c == null)
                return null;

            return new CMYKColorSpaceForStroking((float)c.GetValue(), (float)m.GetValue(), (float)y.GetValue(), (float)k.GetValue());
        }

        //k
        private IPDFPageOperation parseCMYKColorSpaceForNonStroking()
        {
            if (m_stack.Count < 4)
                return null;
            PDFNumber k = m_stack.Pop() as PDFNumber;
            if (k == null)
                return null;
            PDFNumber y = m_stack.Pop() as PDFNumber;
            if (y == null)
                return null;
            PDFNumber m = m_stack.Pop() as PDFNumber;
            if (m == null)
                return null;
            PDFNumber c = m_stack.Pop() as PDFNumber;
            if (c == null)
                return null;

            return new CMYKColorSpaceForNonStroking((float)c.GetValue(), (float)m.GetValue(), (float)y.GetValue(), (float)k.GetValue());
        }
        #endregion

        #region Shading patterns
        //sh
        private IPDFPageOperation parseShading()
        {
            if (m_stack.Count < 1)
                return null;
            PDFName name = m_stack.Pop() as PDFName;
            if (name == null)
                return null;

            return new Shading(name.GetValue());
        }
        #endregion

        #region Inline image
        //BI
        private IPDFPageOperation parseInlineImage()
        {
            PDFDictionary dict = new PDFDictionary();
            for (; ; )
            {
                if (Lexer.IsEOL(m_lexer.LastParsedByte))
                    m_lexer.SkipEOL();

                if (m_lexer.LastParsedByte == 'I')
                {
                    if (m_lexer.ReadByte() != 'D')
                        return null;
                    m_lexer.ReadByte();
                    break;
                }

                if (m_lexer.LastParsedByte != '/')
                    return null;
                PDFName key = m_lexer.ParseName();
                if (key == null)
                    return null;

                IPDFObject value = m_lexer.ReadObjectWithLastParsedByte();
                if (value == null)
                    return null;

                dict.AddItem(key.GetValue(), value);
            }

            MemoryStream ms = new MemoryStream();
            if (containsASCII85Filter(dict))
            {
                int b1 = m_lexer.ReadByte(), b2 = m_lexer.ReadByte();
                if (b1 == -1 || b2 == -1)
                    return null;

                for (; ; )
                {
                    if (b1 == '~' && b2 == '>')
                    {
                        ms.WriteByte((byte)b1);
                        ms.WriteByte((byte)b2);
                        break;
                    }

                    ms.WriteByte((byte)b1);
                    b1 = b2;
                    b2 = m_lexer.ReadByte();

                    if (b2 == -1)
                        return null;
                }

                m_lexer.ReadLexeme();
                if (!m_lexer.CurrentLexemeEquals("EI"))
                    return null;
            }
            else
            {
                int b1 = m_lexer.ReadByte(), b2 = m_lexer.ReadByte(), b3 = m_lexer.ReadByte();
                if (b1 == -1 || b2 == -1)
                    return null;

                for (; ; )
                {
                    if (b1 == 'E' && b2 == 'I' && (Lexer.IsEOL(b3) || b3 == -1))
                        break;

                    ms.WriteByte((byte)b1);
                    b1 = b2;
                    b2 = b3;
                    b3 = m_lexer.ReadByte();

                    if (b2 == -1)
                        return null;
                }
            }

            m_lexer.LastParsedByte = m_lexer.ReadByte();
            return new InlineImage(dict, ms);
        }
        #endregion

        #region XObjects
        //Do
        private IPDFPageOperation parseDoXObject()
        {
            if (m_stack.Count < 1)
                return null;
            PDFName name = m_stack.Pop() as PDFName;
            if (name == null)
                return null;

            return new DoXObject(name.GetValue());
        }
        #endregion

        #region Marked content
        //MP
        private IPDFPageOperation parseMarkedContentPoint()
        {
            if (m_stack.Count < 1)
                return null;
            PDFName tag = m_stack.Pop() as PDFName;
            if (tag == null)
                return null;

            return new MarkedContentPoint(tag.GetValue());
        }

        //DP
        private IPDFPageOperation parseMarkedContentPointWithProperties()
        {
            if (m_stack.Count < 2)
                return null;
            IPDFObject properties = m_stack.Pop();
            if (!(properties is PDFName) && !(properties is PDFDictionary))
                return null;
            PDFName tag = m_stack.Pop() as PDFName;
            if (tag == null)
                return null;

            return new MarkedContentPointWithProperties(tag.GetValue(), properties);
        }

        //BMC
        private IPDFPageOperation parseBeginMarkedContentSequence()
        {
            if (m_stack.Count < 1)
                return null;
            PDFName tag = m_stack.Pop() as PDFName;
            if (tag == null)
                return null;

            return new BeginMarkedContentSequence(tag.GetValue());
        }

        //BDC
        private IPDFPageOperation parseBeginMarkedContentSequenceWithProperties()
        {
            if (m_stack.Count < 2)
                return null;
            IPDFObject properties = m_stack.Pop();
            if(!(properties is PDFName) && !(properties is PDFDictionary))
                return null;
            PDFName tag = m_stack.Pop() as PDFName;
            if(tag == null)
                return null;

            return new BeginMarkedContentSequenceWithProperties(tag.GetValue(), properties);
        }

        //EMC
        private IPDFPageOperation parseEndMarkedContentSequence()
        {
            return new EndMarkedContentSequence();
        }
        #endregion

        #region Compatibility
        //BX
        private IPDFPageOperation parseBeginCompatibilitySection()
        {
            return new BeginCompatibilitySection();
        }

        //EX
        private IPDFPageOperation parseEndCompatibilitySection()
        {
            return new EndCompatibilitySection();
        }
        #endregion

        private IPDFPageOperation parseOperator()
        {
            m_lexer.ReadLexemeWithLastParsedByte();
            string s = m_lexer.CurrentLexemeToString();
            switch (s.Length)
            {
                case 1:
                    switch (s)
                    {
                        case "w":
                            if (m_parseOperation.Linewidth)
                                return parseLineWidth();
                            return null;
                        case "J":
                            if (m_parseOperation.LineCap)
                                return parseLineCap();
                            return null;
                        case "j":
                            if (m_parseOperation.LineJoin)
                                return parseLineJoin();
                            return null;
                        case "M":
                            if (m_parseOperation.MiterLimit)
                                return parseMiterLimit();
                            return null;
                        case "d":
                            if (m_parseOperation.LineDash)
                                return parseLineDash();
                            return null;
                        case "i":
                            if (m_parseOperation.FlatnessTolerance)
                                return parseFlatnessTolerance();
                            return null;
                        case "q":
                            if (m_parseOperation.SaveGraphicsState)
                                return parseSaveGraphicsState();
                            return null;
                        case "Q":
                            if (m_parseOperation.RestoreGraphicsState)
                                return parseRestoreGraphicsState();
                            return null;
                        case "m":
                            if (m_parseOperation.MoveTo)
                                return parseMoveTo();
                            return null;
                        case "l":
                            if (m_parseOperation.LineTo)
                                return parseLineTo();
                            return null;
                        case "c":
                            if (m_parseOperation.BezierCurve)
                                return parseBezierCurve();
                            return null;
                        case "v":
                            if (m_parseOperation.BezierCurve2)
                                return parseBezierCurve2();
                            return null;
                        case "y":
                            if (m_parseOperation.BezierCurve3)
                                return parseBezierCurve3();
                            return null;
                        case "h":
                            if (m_parseOperation.CloseSubpath)
                                return parseCloseSubpath();
                            return null;
                        case "s":
                            if (m_parseOperation.CloseStrokePath)
                                return parseCloseStrokePath();
                            return null;
                        case "S":
                            if (m_parseOperation.StrokePath)
                                return parseStrokePath();
                            return null;
                        case "f":
                        case "F":
                            if (m_parseOperation.FillPathNonZero)
                                return parseFillPathNonZero();
                            return null;
                        case "b":
                            if (m_parseOperation.CloseFillStrokePathNonZero)
                                return parseCloseFillStrokePathNonZero();
                            return null;
                        case "n":
                            if (m_parseOperation.EndPath)
                                return parseEndPath();
                            return null;
                        case "W":
                            if (m_parseOperation.ClipPathNonZero)
                                return parseClipPathNonZero();
                            return null;
                        case "'":
                            if (m_parseOperation.ShowTextFromNewLine)
                                return parseShowTextFromNewLine();
                            return null;
                        case "\"":
                            if (m_parseOperation.ShowTextFromNewLineWithSpacing)
                                return parseShowTextFromNewLineWithSpacing();
                            return null;
                        case "g":
                            if (m_parseOperation.GrayColorSpaceForNonStroking)
                                return parseGrayColorSpaceForNonStroking();
                            return null;
                        case "G":
                            if (m_parseOperation.GrayColorSpaceForStroking)
                                return parseGrayColorSpaceForStroking();
                            return null;
                        case "k":
                            if (m_parseOperation.CMYKColorSpaceForNonStroking)
                                return parseCMYKColorSpaceForNonStroking();
                            return null;
                        case "K":
                            if (m_parseOperation.CMYKColorSpaceForStroking)
                                return parseCMYKColorSpaceForStroking();
                            return null;
                    }
                    break;
                case 2:
                    switch (s)
                    {
                        case "ri":
                            if (m_parseOperation.RenderingIntent)
                                return parseRenderingIntent();
                            return null;
                        case "gs":
                            if (m_parseOperation.GraphicsState)
                                return parseGraphicsState();
                            return null;
                        case "cm":
                            if (m_parseOperation.Transform)
                                return parseTransform();
                            return null;
                        case "re":
                            if (m_parseOperation.Rectangle)
                                return parseRectangle();
                            return null;
                        case "f*":
                            if (m_parseOperation.FillPathEvenOdd)
                                return parseFillPathEvenOdd();
                            return null;
                        case "B*":
                            if (m_parseOperation.FillStrokePathEvenOdd)
                                return parseFillStrokePathEvenOdd();
                            return null;
                        case "b*":
                            if (m_parseOperation.CloseFillStrokePathEvenOdd)
                                return parseCloseFillStrokePathEvenOdd();
                            return null;
                        case "W*":
                            if (m_parseOperation.ClipPathEvenOdd)
                                return parseClipPathEvenOdd();
                            return null;
                        case "BT":
                            if (m_parseOperation.BeginText)
                                return parseBeginText();
                            return null;
                        case "ET":
                            if (m_parseOperation.EndText)
                                return parseEndText();
                            return null;
                        case "Tc":
                            if (m_parseOperation.CharacterSpacing)
                                return parseCharacterSpacing();
                            return null;
                        case "Tw":
                            if (m_parseOperation.WordSpacing)
                                return parseWordSpacing();
                            return null;
                        case "Tz":
                            if (m_parseOperation.HorizontalScaling)
                                return parseHorizontalScaling();
                            return null;
                        case "TL":
                            if (m_parseOperation.TextLeading)
                                return parseTextLeading();
                            return null;
                        case "Tf":
                            if (m_parseOperation.TextFont)
                                return parseTextFont();
                            return null;
                        case "Tr":
                            if (m_parseOperation.TextRenderingMode)
                                return parseTextRenderingMode();
                            return null;
                        case "Ts":
                            if (m_parseOperation.TextRise)
                                return parseTextRise();
                            return null;
                        case "Td":
                            if (m_parseOperation.MoveTextPos)
                                return parseMoveTextPos();
                            return null;
                        case "TD":
                            if (m_parseOperation.MoveTextPosWithLeading)
                                return parseMoveTextPosWithLeading();
                            return null;
                        case "Tm":
                            if (m_parseOperation.TextMatrix)
                                return parseTextMatrix();
                            return null;
                        case "T*":
                            if (m_parseOperation.MoveTextPosToNextLine)
                                return parseMoveTextPosToNextLine();
                            return null;
                        case "Tj":
                            if (m_parseOperation.ShowText)
                                return parseShowText();
                            return null;
                        case "TJ":
                            if (m_parseOperation.ShowTextStrings)
                                return parseShowTextStrings();
                            return null;
                        case "d0":
                            if (m_parseOperation.SetWidthForType3)
                                return parseSetWidthForType3();
                            return null;
                        case "d1":
                            if (m_parseOperation.SetWidthAndBBoxForType3)
                                return parseSetWidthAndBBoxForType3();
                            return null;
                        case "cs":
                            if (m_parseOperation.ColorSpaceForNonStroking)
                                return parseColorSpaceForNonStroking();
                            return null;
                        case "CS":
                            if (m_parseOperation.ColorSpaceForStroking)
                                return parseColorSpaceForStroking();
                            return null;
                        case "SC":
                            if (m_parseOperation.ColorForStroking)
                                return parseColorForStroking();
                            return null;
                        case "sc":
                            if (m_parseOperation.ColorForNonStroking)
                                return parseColorForNonStroking();
                            return null;
                        case "rg":
                            if (m_parseOperation.RGBColorSpaceForNonStroking)
                                return parseRGBColorSpaceForNonStroking();
                            return null;
                        case "RG":
                            if (m_parseOperation.RGBColorSpaceForStroking)
                                return parseRGBColorSpaceForStroking();
                            return null;
                        case "sh":
                            if (m_parseOperation.Shading)
                                return parseShading();
                            return null;
                        case "BI":
                            if (m_parseOperation.InlineImage)
                                return parseInlineImage();
                            skipInlineImage();
                            return null;
                        case "Do":
                            if (m_parseOperation.DoXObject)
                                return parseDoXObject();
                            return null;
                        case "MP":
                            if (m_parseOperation.MarkedContentPoint)
                                return parseMarkedContentPoint();
                            return null;
                        case "DP":
                            if (m_parseOperation.MarkedContentPointWithProperties)
                                return parseMarkedContentPointWithProperties();
                            return null;
                        case "BX":
                            if (m_parseOperation.BeginCompatibilitySection)
                                return parseBeginCompatibilitySection();
                            return null;
                        case "EX":
                            if (m_parseOperation.EndCompatibilitySection)
                                return parseEndCompatibilitySection();
                            return null;
                    }
                    break;
                case 3:
                    switch (s)
                    {
                        case "SCN":
                            if (m_parseOperation.ColorForStrokingEx)
                                return parseColorForStrokingEx();
                            return null;
                        case "scn":
                            if (m_parseOperation.ColorForNonStrokingEx)
                                return parseColorForNonStrokingEx();
                            return null;
                        case "BMC":
                            if (m_parseOperation.BeginMarkedContentSequence)
                                return parseBeginMarkedContentSequence();
                            return null;
                        case "BDC":
                            if (m_parseOperation.BeginMarkedContentSequenceWithProperties)
                                return parseBeginMarkedContentSequenceWithProperties();
                            return null;
                        case "EMC":
                            if (m_parseOperation.EndMarkedContentSequence)
                                return parseEndMarkedContentSequence();
                            return null;
                    }
                    break;
            }

            return null;
        }

        private bool containsASCII85Filter(PDFDictionary dict)
        {
            IPDFObject filter = dict["F"];
            if (filter == null)
                return false;
            if (filter is PDFName)
            {
                if ((filter as PDFName).GetValue() == "A85")
                    return true;
                return false;
            }
            else if (filter is PDFArray)
            {
                PDFArray arr = filter as PDFArray;
                PDFName val = arr[0] as PDFName;
                if (val != null && val.GetValue() == "A85")
                    return true;
            }

            return false;
        }

        private void skipInlineImage()
        {
            PDFDictionary dict = new PDFDictionary();
            for (; ; )
            {
                if (Lexer.IsEOL(m_lexer.LastParsedByte))
                    m_lexer.SkipEOL();

                if (m_lexer.LastParsedByte == 'I')
                {
                    if (m_lexer.ReadByte() != 'D')
                        return;
                    m_lexer.ReadByte();
                    break;
                }

                if (m_lexer.LastParsedByte != '/')
                    return;
                PDFName key = m_lexer.ParseName();
                if (key == null)
                    return;

                IPDFObject value = m_lexer.ReadObjectWithLastParsedByte();
                if (value == null)
                    return;

                dict.AddItem(key.GetValue(), value);
            }
            
            if (containsASCII85Filter(dict))
            {
                int b1 = m_lexer.ReadByte(), b2 = m_lexer.ReadByte();
                if (b1 == -1 || b2 == -1)
                    return;

                for (; ; )
                {
                    if (b1 == '~' && b2 == '>')
                        break;
                    
                    b1 = b2;
                    b2 = m_lexer.ReadByte();

                    if (b2 == -1)
                        return;
                }

                m_lexer.ReadLexeme();
                if (!m_lexer.CurrentLexemeEquals("EI"))
                    return;
            }
            else
            {
                int b1 = m_lexer.ReadByte(), b2 = m_lexer.ReadByte(), b3 = m_lexer.ReadByte();
                if (b1 == -1 || b2 == -1)
                    return;

                for (; ; )
                {
                    if (b1 == 'E' && b2 == 'I' && (Lexer.IsEOL(b3) || b3 == -1))
                        break;
                    
                    b1 = b2;
                    b2 = b3;
                    b3 = m_lexer.ReadByte();

                    if (b2 == -1)
                        return;
                }
            }

            m_lexer.LastParsedByte = m_lexer.ReadByte();
        }

        private Stack<IPDFObject> m_stack;
        private Lexer m_lexer;
        private ParseOperation m_parseOperation;

        private IPDFObject m_contents;
        private int m_curStreamIndex = 0;

        internal enum ParserMode
        {
            Full = 0,
            TextExtraction = 1
        }

        private struct ParseOperation
        {
            public ParseOperation(ParserMode mode)
            {
                Linewidth = false;
                LineCap = false;
                LineJoin = false;
                MiterLimit = false;
                LineDash = false;
                RenderingIntent = false;
                FlatnessTolerance = false;
                GraphicsState = false;
                SaveGraphicsState = false;
                RestoreGraphicsState = false;
                Transform = false;
                MoveTo = false;
                LineTo = false;
                BezierCurve = false;
                BezierCurve2 = false;
                BezierCurve3 = false;
                CloseSubpath = false;
                Rectangle = false;
                StrokePath = false;
                CloseStrokePath = false;
                FillPathNonZero = false;
                FillPathEvenOdd = false;
                FillStrokePathNonZero = false;
                FillStrokePathEvenOdd = false;
                CloseFillStrokePathNonZero = false;
                CloseFillStrokePathEvenOdd = false;
                EndPath = false;
                ClipPathNonZero = false;
                ClipPathEvenOdd = false;
                BeginText = false;
                EndText = false;
                CharacterSpacing = false;
                WordSpacing = false;
                HorizontalScaling = false;
                TextLeading = false;
                TextFont = false;
                TextRenderingMode = false;
                TextRise = false;
                MoveTextPos = false;
                MoveTextPosWithLeading = false;
                TextMatrix = false;
                MoveTextPosToNextLine = false;
                ShowText = false;
                ShowTextStrings = false;
                ShowTextFromNewLine = false;
                ShowTextFromNewLineWithSpacing = false;
                SetWidthForType3 = false;
                SetWidthAndBBoxForType3 = false;
                ColorSpaceForStroking = false;
                ColorSpaceForNonStroking = false;
                ColorForStroking = false;
                ColorForStrokingEx = false;
                ColorForNonStroking = false;
                ColorForNonStrokingEx = false;
                GrayColorSpaceForStroking = false;
                GrayColorSpaceForNonStroking = false;
                RGBColorSpaceForStroking = false;
                RGBColorSpaceForNonStroking = false;
                CMYKColorSpaceForStroking = false;
                CMYKColorSpaceForNonStroking = false;
                Shading = false;
                InlineImage = false;
                DoXObject = false;
                MarkedContentPoint = false;
                MarkedContentPointWithProperties = false;
                BeginMarkedContentSequence = false;
                BeginMarkedContentSequenceWithProperties = false;
                EndMarkedContentSequence = false;
                BeginCompatibilitySection = false;
                EndCompatibilitySection = false;

                switch (mode)
                {
                    case ParserMode.TextExtraction:
                        setTextExtraction();
                        break;
                    default:
                        setFull();
                        break;
                }
            }

            private void setTextExtraction()
            {
                Transform = true;
                SaveGraphicsState = true;
                RestoreGraphicsState = true;
                BeginText = true;
                EndText = true;
                TextMatrix = true;
                TextFont = true;
                MoveTextPos = true;
                TextLeading = true;
                MoveTextPosToNextLine = true;
                MoveTextPosWithLeading = true;
                ShowText = true;
                ShowTextStrings = true;
                ShowTextFromNewLine = true;
                ShowTextFromNewLineWithSpacing = true;
                CharacterSpacing = true;
                WordSpacing = true;
                HorizontalScaling = true;
                TextRise = true;
                DoXObject = true;
            }

            private void setFull()
            {
                Linewidth = true;
                LineCap = true;
                LineJoin = true;
                MiterLimit = true;
                LineDash = true;
                RenderingIntent = true;
                FlatnessTolerance = true;
                GraphicsState = true;
                SaveGraphicsState = true;
                RestoreGraphicsState = true;
                Transform = true;
                MoveTo = true;
                LineTo = true;
                BezierCurve = true;
                BezierCurve2 = true;
                BezierCurve3 = true;
                CloseSubpath = true;
                Rectangle = true;
                StrokePath = true;
                CloseStrokePath = true;
                FillPathNonZero = true;
                FillPathEvenOdd = true;
                FillStrokePathNonZero = true;
                FillStrokePathEvenOdd = true;
                CloseFillStrokePathNonZero = true;
                CloseFillStrokePathEvenOdd = true;
                EndPath = true;
                ClipPathNonZero = true;
                ClipPathEvenOdd = true;
                BeginText = true;
                EndText = true;
                CharacterSpacing = true;
                WordSpacing = true;
                HorizontalScaling = true;
                TextLeading = true;
                TextFont = true;
                TextRenderingMode = true;
                TextRise = true;
                MoveTextPos = true;
                MoveTextPosWithLeading = true;
                TextMatrix = true;
                MoveTextPosToNextLine = true;
                ShowText = true;
                ShowTextStrings = true;
                ShowTextFromNewLine = true;
                ShowTextFromNewLineWithSpacing = true;
                SetWidthForType3 = true;
                SetWidthAndBBoxForType3 = true;
                ColorSpaceForStroking = true;
                ColorSpaceForNonStroking = true;
                ColorForStroking = true;
                ColorForStrokingEx = true;
                ColorForNonStroking = true;
                ColorForNonStrokingEx = true;
                GrayColorSpaceForStroking = true;
                GrayColorSpaceForNonStroking = true;
                RGBColorSpaceForStroking = true;
                RGBColorSpaceForNonStroking = true;
                CMYKColorSpaceForStroking = true;
                CMYKColorSpaceForNonStroking = true;
                Shading = true;
                InlineImage = true;
                DoXObject = true;
                MarkedContentPoint = true;
                MarkedContentPointWithProperties = true;
                BeginMarkedContentSequence = true;
                BeginMarkedContentSequenceWithProperties = true;
                EndMarkedContentSequence = true;
                BeginCompatibilitySection = true;
                EndCompatibilitySection = true;
            }

            public bool Linewidth;
            public bool LineCap;
            public bool LineJoin;
            public bool MiterLimit;
            public bool LineDash;
            public bool RenderingIntent;
            public bool FlatnessTolerance;
            public bool GraphicsState;
            public bool SaveGraphicsState;
            public bool RestoreGraphicsState;
            public bool Transform;
            public bool MoveTo;
            public bool LineTo;
            public bool BezierCurve;
            public bool BezierCurve2;
            public bool BezierCurve3;
            public bool CloseSubpath;
            public bool Rectangle;
            public bool StrokePath;
            public bool CloseStrokePath;
            public bool FillPathNonZero;
            public bool FillPathEvenOdd;
            public bool FillStrokePathNonZero;
            public bool FillStrokePathEvenOdd;
            public bool CloseFillStrokePathNonZero;
            public bool CloseFillStrokePathEvenOdd;
            public bool EndPath;
            public bool ClipPathNonZero;
            public bool ClipPathEvenOdd;
            public bool BeginText;
            public bool EndText;
            public bool CharacterSpacing;
            public bool WordSpacing;
            public bool HorizontalScaling;
            public bool TextLeading;
            public bool TextFont;
            public bool TextRenderingMode;
            public bool TextRise;
            public bool MoveTextPos;
            public bool MoveTextPosWithLeading;
            public bool TextMatrix;
            public bool MoveTextPosToNextLine;
            public bool ShowText;
            public bool ShowTextStrings;
            public bool ShowTextFromNewLine;
            public bool ShowTextFromNewLineWithSpacing;
            public bool SetWidthForType3;
            public bool SetWidthAndBBoxForType3;
            public bool ColorSpaceForStroking;
            public bool ColorSpaceForNonStroking;
            public bool ColorForStroking;
            public bool ColorForStrokingEx;
            public bool ColorForNonStroking;
            public bool ColorForNonStrokingEx;
            public bool GrayColorSpaceForStroking;
            public bool GrayColorSpaceForNonStroking;
            public bool RGBColorSpaceForStroking;
            public bool RGBColorSpaceForNonStroking;
            public bool CMYKColorSpaceForStroking;
            public bool CMYKColorSpaceForNonStroking;
            public bool Shading;
            public bool InlineImage;
            public bool DoXObject;
            public bool MarkedContentPoint;
            public bool MarkedContentPointWithProperties;
            public bool BeginMarkedContentSequence;
            public bool BeginMarkedContentSequenceWithProperties;
            public bool EndMarkedContentSequence;
            public bool BeginCompatibilitySection;
            public bool EndCompatibilitySection;
        }
    }
}
