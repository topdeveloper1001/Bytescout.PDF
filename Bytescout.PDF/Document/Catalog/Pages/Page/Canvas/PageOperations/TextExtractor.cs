using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace Bytescout.PDF
{
    internal class TextExtractor
    {
        public static string ExtractText(IPDFObject contents, Resources resources)
        {
            List<CoordinateText> listStrings = new List<CoordinateText>();
            fillListStrings(listStrings, new float[] { 1, 0, 0, 1, 0, 0 }, contents, resources);
            return convertText(listStrings);
        }

        public static void fillListStrings(List<CoordinateText> listStrings, float[] beginMatrix, IPDFObject contents, Resources resources)
        {
            PageOperationParser parser = new PageOperationParser(contents);
            parser.SetParserMode(PageOperationParser.ParserMode.TextExtraction);

            Stack<float[]> stackCM = new Stack<float[]>();
            Stack<float[]> stackTM = new Stack<float[]>();
            Stack<TextState> stackTextState = new Stack<TextState>();

            float[] currentCM = new float[6] { beginMatrix[0], beginMatrix[1], beginMatrix[2], beginMatrix[3], beginMatrix[4], beginMatrix[5] };
            float[] currentTM = new float[6] { 1, 0, 0, 1, 0, 0 };
            TextState currentTextState = new TextState();
            
            float[] currentAllTransform = new float[6] { 1, 0, 0, 1, 0, 0 };
            setAllTransform(currentTM, currentCM, ref currentAllTransform);

            int currentWeight = 0;
            bool isShowText = false;

            IPDFPageOperation operation = null;
            while ((operation = parser.Next()) != null)
            {
                switch (operation.Type)
                {
                    //CanvasState
                    case PageOperations.Transform:
                        transform(ref currentCM, (Transform)operation);
                        setAllTransform(currentTM, currentCM, ref currentAllTransform);
                        break;
                    case PageOperations.SaveGraphicsState:
                        stackTM.Push(new float[] { currentTM[0], currentTM[1], currentTM[2], currentTM[3], currentTM[4], currentTM[5] });
                        stackCM.Push(new float[] { currentCM[0], currentCM[1], currentCM[2], currentCM[3], currentCM[4], currentCM[5] });
                        stackTextState.Push(new TextState(currentTextState));
                        break;
                    case PageOperations.RestoreGraphicsState:
                        if (stackTM.Count != 0)
                        {
                            currentTM = stackTM.Pop();
                            currentCM = stackCM.Pop();
                            currentTextState = stackTextState.Pop();
                            setAllTransform(currentTM, currentCM, ref currentAllTransform);
                        }
                        break;
                    case PageOperations.DoXObject:
                        PDFDictionaryStream dict = resources.GetResource(((DoXObject)operation).Name, ResourceType.XObject) as PDFDictionaryStream;
                        if (dict != null)
                        {
                            PDFName type = dict.Dictionary["Subtype"] as PDFName;
                            if (type != null)
                            {
                                if (type.GetValue() == "Form")
                                {
                                    dict.Decode();
                                    float[] matrix = new float[6] { 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f };
                                    PDFArray array = dict.Dictionary["Matrix"] as PDFArray;
                                    if (array != null)
                                    {
                                        for (int i = 0; i < array.Count && i < 6; ++i)
                                        {
                                            PDFNumber number = array[i] as PDFNumber;
                                            if (number != null)
                                                matrix[i] = (float)number.GetValue();
                                        }
                                    }
                                    mulMatrix(currentCM, ref matrix);
                                    fillListStrings(listStrings, matrix, dict, new Resources(dict.Dictionary["Resources"] as PDFDictionary, false));
                                }
                            }
                        }
                        break;

                    //Text
                    case PageOperations.BeginText:
                        isShowText = false;
                        setDefaultTextMatrix(ref currentTM);
                        setAllTransform(currentTM, currentCM, ref currentAllTransform);
                        break;
                    case PageOperations.EndText:
                        isShowText = false;
                        setDefaultTextMatrix(ref currentTM);
                        setAllTransform(currentTM, currentCM, ref currentAllTransform);
                        break;

                    //TextPosition
                    case PageOperations.TextMatrix:
                        isShowText = false;
                        setTM(ref currentTM, (TextMatrix)operation);
                        setAllTransform(currentTM, currentCM, ref currentAllTransform);
                        break;
                    case PageOperations.MoveTextPos:
                        isShowText = false;
                        moveTextPos(ref currentTM, (MoveTextPos)operation);
                        setAllTransform(currentTM, currentCM, ref currentAllTransform);
                        break;
                    case PageOperations.MoveTextPosToNextLine:
                        isShowText = false;
                        moveTextPos(ref currentTM, currentTextState.Leading);
                        setAllTransform(currentTM, currentCM, ref currentAllTransform);
                        break;
                    case PageOperations.MoveTextPosWithLeading:
                        isShowText = false;
                        currentTextState.Leading = -((MoveTextPosWithLeading)operation).TY;
                        moveTextPos(ref currentTM, (MoveTextPosWithLeading)operation);
                        setAllTransform(currentTM, currentCM, ref currentAllTransform);
                        break;

                    //TextState
                    case PageOperations.TextFont:
                        changeFont(ref currentTextState, resources, (TextFont)operation);
                        break;
                    case PageOperations.TextLeading:
                        currentTextState.Leading = ((TextLeading)operation).Leading;
                        break;
                    case PageOperations.TextRise:
                        currentTextState.Rize = ((TextRise)operation).Rise;
                        break;
                    case PageOperations.WordSpacing:
                        currentTextState.WordSpace = ((WordSpacing)operation).WordSpace;
                        break;
                    case PageOperations.CharacterSpacing:
                        currentTextState.CharSpace = ((CharacterSpacing)operation).CharSpace;
                        break;
                    case PageOperations.HorizontalScaling:
                        currentTextState.Scale = ((HorizontalScaling)operation).Scale;
                        break;


                    //TextRead
                    case PageOperations.ShowText:
                        if (currentTextState.FontBase != null)
                        {
                            addShowText(listStrings, currentAllTransform, ref currentWeight, currentTextState, isShowText, (ShowText)operation);
                            isShowText = true;
                        }
                        break;
                    case PageOperations.ShowTextStrings:
                        if (currentTextState.FontBase != null)
                        {
                            addShowTextStrings(listStrings, currentAllTransform, ref currentWeight, currentTextState, isShowText, (ShowTextStrings)operation);
                            isShowText = true;
                        }
                        break;
                    case PageOperations.ShowTextFromNewLine:
                        if (currentTextState.FontBase != null)
                        {
                            moveTextPos(ref currentTM, currentTextState.Leading);
                            setAllTransform(currentTM, currentCM, ref currentAllTransform);
                            addShowText(listStrings, currentAllTransform, currentWeight, currentTextState, (ShowTextFromNewLine)operation);
                            ++currentWeight;
                            isShowText = true;
                        }
                        break;
                    case PageOperations.ShowTextFromNewLineWithSpacing:
                        if (currentTextState.FontBase != null)
                        {
                            currentTextState.CharSpace = ((ShowTextFromNewLineWithSpacing)operation).CharacterSpacing;
                            currentTextState.WordSpace = ((ShowTextFromNewLineWithSpacing)operation).WordSpacing;
                            moveTextPos(ref currentTM, currentTextState.Leading);
                            setAllTransform(currentTM, currentCM, ref currentAllTransform);
                            addShowText(listStrings, currentAllTransform, currentWeight, currentTextState, (ShowTextFromNewLineWithSpacing)operation);
                            ++currentWeight;
                            isShowText = true;
                        }
                        break;
                    
                }
            }
        }

        private class ComparerText : IComparer<CoordinateText>
        {
            public int Compare(CoordinateText textA, CoordinateText textB)
            {
                if (textA.Coordinate.Y > textB.Coordinate.Y)
                {
                    if (textA.Coordinate.X < textB.Coordinate.X)
                    {
                        return -1;
                    }
                    else if (textA.Coordinate.X > textB.Coordinate.X)
                    {
                        if (textB.Coordinate.Y + textB.TextHeight / 2 >= textA.Coordinate.Y)
                            return 1;
                        else
                            return -1;
                    }
                    else
                    {
                        if (textA.Weight < textB.Weight)
                            return -1;
                        if (textA.Weight > textB.Weight)
                            return 1;
                    }
                }
                if (textA.Coordinate.Y < textB.Coordinate.Y)
                {
                    if (textA.Coordinate.X < textB.Coordinate.X)
                    {
                        if (textA.Coordinate.Y + textA.TextHeight / 2 >= textB.Coordinate.Y)
                            return -1;
                        else
                            return 1;
                    }
                    else if (textA.Coordinate.X > textB.Coordinate.X)
                    {
                        return 1;
                    }
                    else
                    {
                        if (textA.Weight < textB.Weight)
                            return -1;
                        if (textA.Weight > textB.Weight)
                            return 1;
                    }
                }
                if (textA.Coordinate.X < textB.Coordinate.X)
                    return -1;
                if (textA.Coordinate.X > textB.Coordinate.X)
                    return 1;
                if (textA.Weight < textB.Weight)
                    return -1;
                if (textA.Weight > textB.Weight)
                    return 1;
                return 0;
            }
        }

        public static string convertText(List<CoordinateText> listStrings)
        {
            if (listStrings.Count == 0)
                return "";

            StringBuilder text = new StringBuilder();

            listStrings.Sort(new ComparerText());

            /*for (int i = 0; i < listStrings.Count - 1; ++i)
            {
                if (listStrings[i].Coordinate.X + listStrings[i].TextWidth > listStrings[i + 1].Coordinate.X)
                {
                    if (isNextInLine(listStrings[i], listStrings[i + 1]))
                    {
                        if (mergeString(listStrings[i], listStrings[i + 1]) == 0)
                            listStrings.RemoveAt(i + 1);
                        else
                            listStrings.RemoveAt(i);
                        --i;
                    }
                }
            }*/

            float middleX = 0.0f;
            float middleY = 0.0f;
            int count = 0;
            float minX = listStrings[0].Coordinate.X;
            float minY = listStrings[0].Coordinate.Y;

            for (int i = 0; i < listStrings.Count; ++i)
            {
                middleX += listStrings[i].TextWidth;
                count += listStrings[i].Text.Length;
                middleY += listStrings[i].TextHeight;
                if (listStrings[i].Coordinate.X < minX)
                    minX = listStrings[i].Coordinate.X;
                if (minY < listStrings[i].Coordinate.Y)
                    minY = listStrings[i].Coordinate.Y;
            }

            middleX /= count;

            for (int i = 0; i < listStrings.Count - 1; ++i)
            {
                if (listStrings[i].Coordinate.X + listStrings[i].TextWidth < listStrings[i + 1].Coordinate.X)
                    if (listStrings[i].TextWidth < listStrings[i].Text.Length * middleX)
                        if (isNextInLine(listStrings[i], listStrings[i + 1]))
                            if (listStrings[i + 1].Coordinate.X < listStrings[i].Coordinate.X + listStrings[i].Text.Length * middleX)
                                if ((float)Math.Abs(listStrings[i + 1].Coordinate.X - listStrings[i].Coordinate.X) != 0)
                                    middleX = (float)Math.Abs(listStrings[i + 1].Coordinate.X - listStrings[i].Coordinate.X) / listStrings[i].Text.Length;
            }

            middleY /= listStrings.Count;

            float currentCoordinateY = minY;
            int countSpace = 0;
            int countSimbols = 0;
            if (listStrings[0].Coordinate.X - minX > 0)
                countSpace = round((listStrings[0].Coordinate.X - minX) / middleX);
            text.Append(' ', countSpace);
            float currentHeight = listStrings[0].TextHeight;
            text.Append(listStrings[0].Text);
            countSimbols += listStrings[0].Text.Length + countSpace;
            FontBase currentFont = listStrings[0].FontBase;
            float currentFontSize = listStrings[0].FontSize;
            float currentCoordinateX = listStrings[0].Coordinate.X;
            float currentScale = listStrings[0].Scale;

            for (int i = 1; i < listStrings.Count; ++i)
            {
                if (currentCoordinateY != listStrings[i].Coordinate.Y)
                {
                    if (listStrings[i].Coordinate.X < listStrings[i - 1].Coordinate.X)
                    {
                        if (Math.Abs(currentCoordinateY - listStrings[i].Coordinate.Y) > middleY * 2)
                            text.Append("\r\n");
                        text.Append("\r\n");
                        countSimbols = 0;
                        countSpace = round((listStrings[i].Coordinate.X - minX) / middleX) - countSimbols;
                        if (countSpace < 0)
                            countSpace = 0;
                        text.Append(' ', countSpace);
                    }
                    else
                    {
                        if (currentCoordinateY - currentHeight / 2 < listStrings[i].Coordinate.Y)
                        {
                            if (currentFont == listStrings[i].FontBase)
                            {
                                countSpace = getCountSpaceEx(listStrings, i, middleX, minX, countSimbols);
                                if (countSpace < 0)
                                    countSpace = 0;
                                if (countSpace > 2)
                                    countSpace = round((listStrings[i].Coordinate.X - minX) / middleX) - countSimbols;
                                if (countSpace < 0)
                                    countSpace = 0;
                                text.Append(' ', countSpace);

                            }
                            else
                            {
                                countSpace = getCountSpaceEx(listStrings, i, middleX, minX, countSimbols);
                                if (countSpace < 0)
                                    countSpace = 0;
                                text.Append(' ', countSpace);
                            }
                        }
                        else
                        {
                            if (Math.Abs(currentCoordinateY - listStrings[i].Coordinate.Y) > middleY * 2)
                                text.Append("\r\n");
                            text.Append("\r\n");
                            countSimbols = 0;
                            countSpace = round((listStrings[i].Coordinate.X - minX) / middleX) - countSimbols;
                            if (countSpace < 0)
                                countSpace = 0;
                            text.Append(' ', countSpace);
                        }
                    }
                }
                else
                {
                    if (currentFont == listStrings[i].FontBase)
                    {
                        countSpace = getCountSpaceEx(listStrings, i, middleX, minX, countSimbols);
                        if (countSpace < 0)
                            countSpace = 0;
                        if (countSpace > 2)
                            countSpace = round((listStrings[i].Coordinate.X - minX) / middleX) - countSimbols;
                        if (countSpace < 0)
                            countSpace = 0;
                        text.Append(' ', countSpace);
                    }
                    else
                    {
                        countSpace = getCountSpaceEx(listStrings, i, middleX, minX, countSimbols);
                        if (countSpace < 0)
                            countSpace = 0;
                        text.Append(' ', countSpace);
                    }
                }

                text.Append(listStrings[i].Text);
                countSimbols += listStrings[i].Text.Length + countSpace;
                currentCoordinateY = listStrings[i].Coordinate.Y;
                currentHeight = listStrings[i].TextHeight;
                currentFont = listStrings[i].FontBase;
                currentCoordinateX = listStrings[i].Coordinate.X;
                currentFontSize = listStrings[i].FontSize;
                currentScale = listStrings[i].Scale;
            }

            return text.ToString();
        }

        private static int getCountSpaceEx(List<CoordinateText> listStrings, int i, float middleX, float minX, int countSimbols)
        {
            int countSpace = 0;
            if (listStrings[i - 1].FontSize == listStrings[i].FontSize && listStrings[i - 1].Scale == listStrings[i].Scale)
            {
                float scale = listStrings[i].Scale;
                countSpace = round((-listStrings[i - 1].TextWidth + listStrings[i].Coordinate.X - listStrings[i - 1].Coordinate.X) * 1000 / (listStrings[i].FontSize * 200 * scale));
                if (countSpace > 1)
                    countSpace = round((-listStrings[i - 1].TextWidth + listStrings[i].Coordinate.X - listStrings[i - 1].Coordinate.X) / middleX);
            }
            else
            {
                countSpace = round((listStrings[i].Coordinate.X - minX) / middleX) - countSimbols;
            }
            return countSpace;
        }

        private static int round(float number)
        {
            return (int)(number + 0.5f);
        }

        

        //New

        private static PointF getCoordinates(float[] matrix)
        {
            if (Math.Abs(matrix[0]) + Math.Abs(matrix[3]) > Math.Abs(matrix[1]) + Math.Abs(matrix[2]) && matrix[0] < 0 && matrix[3] < 0)
                return new PointF(-matrix[4], -matrix[5]);
            if (Math.Abs(matrix[0]) + Math.Abs(matrix[3]) < Math.Abs(matrix[1]) + Math.Abs(matrix[2]) && matrix[1] < 0)
                return new PointF(-matrix[5], matrix[4]);
            if (Math.Abs(matrix[0]) + Math.Abs(matrix[3]) < Math.Abs(matrix[1]) + Math.Abs(matrix[2]) && matrix[2] < 0)
                return new PointF(matrix[5], -matrix[4]);
            return new PointF(matrix[4], matrix[5]);
        }

        private static float eps()
        {
            return 0.01f;
        }

        private static int mergeString(CoordinateText ct, CoordinateText ctNext)
        {
            if (ct.TextWidth > ctNext.TextWidth)
            {
                int indexCt = (int)((float)Math.Abs(ctNext.Coordinate.X - ct.Coordinate.X) * ct.Text.Length / ct.TextWidth);
                int indexCtNext = 0;

                if (Math.Abs(ct.Coordinate.X - ctNext.Coordinate.X) < eps())
                {
                    indexCt = 0;
                }
                
                for (; indexCt < ct.Text.Length && indexCtNext < ctNext.Text.Length; ++indexCt, ++indexCtNext)
                {
                    if (!isSpace(ctNext.Text[indexCtNext]))
                        ct.Text[indexCt] = ctNext.Text[indexCtNext];
                }
                for (; indexCtNext < ctNext.Text.Length; ++indexCtNext)
                    ct.Text.Append(ctNext.Text[indexCtNext]);
                if (ctNext.Coordinate.X + ctNext.TextWidth > ct.Coordinate.X + ct.TextWidth)
                    ct.TextWidth += ctNext.Coordinate.X + ctNext.TextWidth - (ct.Coordinate.X + ct.TextWidth);
                return 0;
            }
            else if (ct.TextWidth < ctNext.TextWidth)
            {
                int indexCt = (int)((float)Math.Abs(ctNext.Coordinate.X - ct.Coordinate.X) * ct.Text.Length / ct.TextWidth);
                int indexCtNext = 0;

                if (Math.Abs(ct.Coordinate.X - ctNext.Coordinate.X) < eps())
                {
                    indexCt = 0;
                }
                
                for (; indexCt < ct.Text.Length && indexCtNext < ctNext.Text.Length; ++indexCt, ++indexCtNext)
                {
                    if (!isSpace(ct.Text[indexCt]))
                        ctNext.Text[indexCtNext] = ct.Text[indexCt];
                }

                indexCt = (int)((float)Math.Abs(ctNext.Coordinate.X - ct.Coordinate.X) * ct.Text.Length / ct.TextWidth);

                for (int i = 0; i < indexCt; ++i)
                    ctNext.Text.Insert(0, ct.Text[i]);
                ctNext.TextWidth += ctNext.Coordinate.X - ct.Coordinate.X;
                return 1;
            }
            
            if (getCountSpace(ct.Text.ToString()) < getCountSpace(ctNext.Text.ToString()))
                return 0;
            
            return 1;
        }

        private static bool isSpace(char a)
        {
            return (a == ' ' || a == '\t' || a == '\r');
        }

        private static bool isNextInLine(CoordinateText ct, CoordinateText ctNext)
        {
            if (ct.Coordinate.Y != ctNext.Coordinate.Y)
            {
                if (ctNext.Coordinate.X < ct.Coordinate.X)
                    return false;
                else
                {
                    if (ct.Coordinate.Y - ct.TextHeight / 2 < ctNext.Coordinate.Y)
                        return true;
                    else
                        return false;
                }
            }

            return true;
        }

        private static void addShowTextStrings(List<CoordinateText> listStrings, float[] currentAllTransform, ref int currentWeight, TextState currentTextState, bool isShowText, ShowTextStrings showTextStrings)
        {
            StringBuilder result = new StringBuilder();

            float scale = getScale(currentAllTransform);
            float height = currentTextState.FontBase.GetTextHeight(currentTextState.FontSize) * scale;
            float width = 0.0f;

            for (int i = 0; i < showTextStrings.Array.Length; ++i)   
            {
                if (showTextStrings.Array[i] is PDFString)
                {
                    string text = currentTextState.FontBase.ConvertFromFontEncoding(showTextStrings.Array[i] as PDFString);
                    float textWidth = currentTextState.FontBase.GetTextWidth(showTextStrings.Array[i] as PDFString, currentTextState.FontSize);
                    float widthWordSpace = getWidthWordSpacing(currentTextState, text);
                    StringBuilder textString = new StringBuilder(text);
                    float widthCharSpace = insertSpaces(currentTextState, textString);
                    width += (textWidth + widthWordSpace + widthCharSpace) * currentTextState.Scale * scale / 100;
                    result.Append(textString);
                }
                else
                {
                    float space = -(float)showTextStrings.Array[i] * currentTextState.FontSize / 1000;
                    if (i != showTextStrings.Array.Length - 1)
                    {
                        if (space > 0)
                        {
                            float spaceFont = getSpaceWidth(currentTextState);
                            if (space >= spaceFont / 2)
                                result.Append(' ', (int)(space / spaceFont + 0.5f));
                        }
                        width += space * currentTextState.Scale * scale / 100;
                    }
                }
            }
            if (!isShowText)
            {
                listStrings.Add(new CoordinateText(getCoordinates(currentAllTransform), result, currentTextState.FontBase, currentTextState.FontSize, width, height, currentWeight, scale));
                ++currentWeight;
            }
            else
            {
                if (currentTextState.WordSpace != 0.0f)
                {
                    listStrings[listStrings.Count - 1].Text.Append(' ', (int)(currentTextState.WordSpace / getSpaceWidth(currentTextState) + 0.5f));
                    width += currentTextState.WordSpace * currentTextState.Scale * scale / 100;
                }
                listStrings[listStrings.Count - 1].Text.Append(result);
                listStrings[listStrings.Count - 1].TextWidth += width;
            }
        }

        private static void addShowText(List<CoordinateText> listStrings, float[] currentAllTransform, ref int currentWeight, TextState currentTextState, bool isShowText, ShowText showText)
        {
            float textWidth = currentTextState.FontBase.GetTextWidth(showText.String, currentTextState.FontSize);
            string text = currentTextState.FontBase.ConvertFromFontEncoding(showText.String);
            float widthWordSpace = getWidthWordSpacing(currentTextState, text);
            StringBuilder textString = new StringBuilder(text);
            float widthCharSpace = insertSpaces(currentTextState, textString);
            float scale = getScale(currentAllTransform);
            float width = (textWidth + widthWordSpace + widthCharSpace) * currentTextState.Scale * scale / 100;
            float height = currentTextState.FontBase.GetTextHeight(currentTextState.FontSize) * scale;
            if (!isShowText)
            {
                listStrings.Add(new CoordinateText(getCoordinates(currentAllTransform), textString, currentTextState.FontBase, currentTextState.FontSize, width, height, currentWeight, scale));
                ++currentWeight;
            }
            else
            {
                if (currentTextState.WordSpace != 0.0f)
                {
                    listStrings[listStrings.Count - 1].Text.Append(' ', (int)(currentTextState.WordSpace / getSpaceWidth(currentTextState) + 0.5f));
                    width += currentTextState.WordSpace * currentTextState.Scale * scale / 100;
                }
                listStrings[listStrings.Count - 1].Text.Append(textString);
                listStrings[listStrings.Count - 1].TextWidth += width;
            }
        }

        private static void addShowText(List<CoordinateText> listStrings, float[] currentAllTransform, int currentWeight, TextState currentTextState, ShowTextFromNewLine showText)
        {
            float textWidth = currentTextState.FontBase.GetTextWidth(showText.String, currentTextState.FontSize);
            string text = currentTextState.FontBase.ConvertFromFontEncoding(showText.String);
            float widthWordSpace = getWidthWordSpacing(currentTextState, text);
            StringBuilder textString = new StringBuilder(text);
            float widthCharSpace = insertSpaces(currentTextState, textString);
            float scale = getScale(currentAllTransform);
            float width = (textWidth + widthWordSpace + widthCharSpace) * currentTextState.Scale * scale / 100;
            float height = currentTextState.FontBase.GetTextHeight(currentTextState.FontSize) * scale;
            listStrings.Add(new CoordinateText(getCoordinates(currentAllTransform), textString, currentTextState.FontBase, currentTextState.FontSize, width, height, currentWeight, scale));
        }

        private static void addShowText(List<CoordinateText> listStrings, float[] currentAllTransform, int currentWeight, TextState currentTextState, ShowTextFromNewLineWithSpacing showText)
        {
            float textWidth = currentTextState.FontBase.GetTextWidth(showText.String, currentTextState.FontSize);
            string text = currentTextState.FontBase.ConvertFromFontEncoding(showText.String);
            float widthWordSpace = getWidthWordSpacing(currentTextState, text);
            StringBuilder textString = new StringBuilder(text);
            float widthCharSpace = insertSpaces(currentTextState, textString);
            float scale = getScale(currentAllTransform);
            float width = (textWidth + widthWordSpace + widthCharSpace) * currentTextState.Scale * scale / 100;
            float height = currentTextState.FontBase.GetTextHeight(currentTextState.FontSize) * scale;
            listStrings.Add(new CoordinateText(getCoordinates(currentAllTransform), textString, currentTextState.FontBase, currentTextState.FontSize, width, height, currentWeight, scale));
        }

        private static float getWidthWordSpacing(TextState currentTextState, string text)
        {
            if (currentTextState.WordSpace != 0.0f)
            { 
                if (!(currentTextState.FontBase is TrueTypeFont) || !(currentTextState.FontBase is Type0Font))
                {
                    if (currentTextState.FontBase.Contains(' '))
                    {
                        return getCountSpace(text) * currentTextState.WordSpace;
                    }
                }
            }

            return 0.0f;
        }

        private static float insertSpaces(TextState currentTextState, StringBuilder text)
        { 
            float space = getSpaceWidth(currentTextState);
            float widthCharSpaces = 0.0f;
            if (currentTextState.CharSpace >= space / 2)
            {
                int countSpace = (int)(currentTextState.CharSpace / space + 0.5f);
                for (int i = 0; i < text.Length - 1; i += countSpace + 1)
                {
                    text.Insert(i + 1, " ", countSpace);
                    widthCharSpaces += currentTextState.CharSpace;
                }
            }
            else
                widthCharSpaces += currentTextState.CharSpace;
            return widthCharSpaces;
        }

        private static float getSpaceWidth(TextState currentTextState)
        {
            float space = currentTextState.FontBase.GetCharWidth(' ', currentTextState.FontSize);
            if (space == 0.0f)
                space = 0.4f * currentTextState.FontSize;
            return space;
        }

        private static int getCountSpace(string text)
        {
            int countSpace = 0;
            int i = 0;
            i = text.IndexOf(' ', 0);
            while (i != -1)
            {
                ++countSpace;
                i = text.IndexOf(' ', i + 1);
            }
            return countSpace;
        }

        private static float getScale(float[] matrix)
        {
            return (float)Math.Sqrt(((matrix[0] + matrix[2]) * (matrix[0] + matrix[2]) + (matrix[1] + matrix[3]) * (matrix[1] + matrix[3])) / 2);
        }

        private static void moveTextPos(ref float[] currentTM, MoveTextPos moveTo)
        {
            currentTM[4] += moveTo.TX * currentTM[0] + moveTo.TY * currentTM[2];
            currentTM[5] += moveTo.TX * currentTM[1] + moveTo.TY * currentTM[3];
        }

        private static void moveTextPos(ref float[] currentTM, MoveTextPosWithLeading moveTo)
        {
            currentTM[4] += moveTo.TX * currentTM[0] + moveTo.TY * currentTM[2];
            currentTM[5] += moveTo.TX * currentTM[1] + moveTo.TY * currentTM[3];
        }

        private static void moveTextPos(ref float[] currentTM, float leading)
        {
            currentTM[4] -= leading * currentTM[2];
            currentTM[5] -= leading * currentTM[3];
        }

        private static void changeFont(ref TextState currentTextState, Resources resources, TextFont textFont)
        {
            IPDFObject obj = resources.GetResource(textFont.FontName, ResourceType.Font);
            if (obj is PDFDictionary)
                currentTextState.FontBase = FontBase.Instance(obj as PDFDictionary);
            currentTextState.FontSize = textFont.FontSize;
        }

        private static void setTM(ref float[] currentTM, TextMatrix tm)
        {
            currentTM[0] = tm.A;
            currentTM[1] = tm.B;
            currentTM[2] = tm.C;
            currentTM[3] = tm.D;
            currentTM[4] = tm.E;
            currentTM[5] = tm.F;
        }

        private static void setDefaultTextMatrix(ref float[] currentTM)
        {
            currentTM[0] = 1;
            currentTM[1] = 0;
            currentTM[2] = 0;
            currentTM[3] = 1;
            currentTM[4] = 0;
            currentTM[5] = 0;
        }

        private static void transform(ref float[] currentCM, Transform transform)
        {
            mulMatrix(new float[6] { transform.A, transform.B, transform.C, transform.D, transform.E, transform.F }, ref currentCM);
        }

        private static void setAllTransform(float[] currentTM, float[] currentCM, ref float[] currentAllTransform)
        {
            currentAllTransform[0] = currentTM[0] * currentCM[0] + currentTM[1] * currentCM[2];
            currentAllTransform[1] = currentTM[0] * currentCM[1] + currentTM[1] * currentCM[3];
            currentAllTransform[2] = currentTM[2] * currentCM[0] + currentTM[3] * currentCM[2];
            currentAllTransform[3] = currentTM[2] * currentCM[1] + currentTM[3] * currentCM[3];
            currentAllTransform[4] = currentTM[4] * currentCM[0] + currentTM[5] * currentCM[2] + currentCM[4];
            currentAllTransform[5] = currentTM[4] * currentCM[1] + currentTM[5] * currentCM[3] + currentCM[5];
        }

        private static void mulMatrix(float[] matrix, ref float[] matrixTransform)
        {
            float a = matrix[0] * matrixTransform[0] + matrix[1] * matrixTransform[2];
            float b = matrix[0] * matrixTransform[1] + matrix[1] * matrixTransform[3];
            float c = matrix[2] * matrixTransform[0] + matrix[3] * matrixTransform[2];
            float d = matrix[2] * matrixTransform[1] + matrix[3] * matrixTransform[3];
            float e = matrix[4] * matrixTransform[0] + matrix[5] * matrixTransform[2] + matrixTransform[4];
            float f = matrix[4] * matrixTransform[1] + matrix[5] * matrixTransform[3] + matrixTransform[5];
            matrixTransform[0] = a;
            matrixTransform[1] = b;
            matrixTransform[2] = c;
            matrixTransform[3] = d;
            matrixTransform[4] = e;
            matrixTransform[5] = f;
        }

        private class TextState
        {
	        private float m_charSpace;
	        private float m_wordSpace;
	        private float m_scale;
	        private float m_leading;
	        private float m_fontSize;
	        private FontBase m_fontBase;
	        private TextRenderingModes m_render;
	        private float m_rize;

	        public float CharSpace
            {
                get
                {
                    return m_charSpace;
                }
                set
                {
                    m_charSpace = value;
                }
            }

            public float WordSpace
            {
                get
                {
                    return m_wordSpace;
                }
                set
                {
                    m_wordSpace = value;
                }
            }

            public float Scale
            {
                get
                {
                    return m_scale;
                }
                set
                {
                    m_scale = value;
                }
            }

            public float Leading
            {
                get
                {
                    return m_leading;
                }
                set
                {
                    m_leading = value;
                }
            }

            public float FontSize
            {
                get
                {
                    return m_fontSize;
                }
                set
                {
                    m_fontSize = value;
                }
            }

            public TextRenderingModes Render
            {
                get
                {
                    return m_render;
                }
                set
                {
                    m_render = value;
                }
            }

            public float Rize
            {
                get
                {
                    return m_rize;
                }
                set
                {
                    m_rize = value;
                }
            }

            public FontBase FontBase
            {
                get
                {
                    return m_fontBase;
                }
                set
                {
                    m_fontBase = value;
                }
            }

            public void setDefault()
            {
                m_charSpace = 0.0f;
                m_wordSpace = 0.0f;
                m_scale = 100.0f;
                m_leading = 0.0f;
                m_fontSize = 0.0f;
                m_render = TextRenderingModes.Fill;
                m_rize = 0.0f;
                m_fontBase = null;
            }

	        public TextState()
	        {
		        m_charSpace = 0.0f;
		        m_wordSpace = 0.0f;
		        m_scale = 100.0f;
		        m_leading = 0.0f;
		        m_fontSize = 0.0f;
		        m_render = TextRenderingModes.Fill;
		        m_rize = 0.0f;
		        m_fontBase = null;
	        }

	        public TextState(TextState textState)
	        {
		        m_charSpace = textState.m_charSpace;
		        m_wordSpace = textState.m_wordSpace;
		        m_scale = textState.m_scale;
		        m_leading = textState.m_leading;
		        m_fontSize = textState.m_fontSize;
		        m_render = textState.m_render;
		        m_rize = textState.m_rize;
		        m_fontBase = textState.m_fontBase;
	        }
        }
    }
}
