using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Bytescout.PDF
{
    internal class TextString
    {
        #region General methods

	    private const string WhiteSpace = " ";
	    private const string TabSpace = "   ";

	    private List<float> _restWordSpacingForScript;
	    private List<int> _lines; // contains index of start item for line
	    private List<Uniscribe.SCRIPT_ITEM> _items;
	    private bool _isComplex;

	    private string _shapedText;
	    private string _initialText;
	    private readonly List<string> _text;
        
	    private Font _font;
	    private StringFormat _sf;
	    private SizeF _size;
	    private bool _onlyLeftAndTop; // need for method of justification

	    public StringFormat TextFormat
        {
            get
            {
                return _sf;
            }
            set
            {
                _sf = value.Clone() as StringFormat;
                setList();
            }
        }

        public int Count
        {
            get
            {
                return _text.Count;
            }
        }

        public Font Font
        {
            get
            {
                return _font;
            }
            set
            {
                _font = value.Clone() as Font;
                setList();
            }
        }

        public SizeF Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                _onlyLeftAndTop = false;
                setList();
            }
        }

        public string[] Text
        {
            get
            {
                return _text.ToArray();
            }
        }

	    public TextString()
	    {
		    _text = new List<string>();
		    _sf = new StringFormat();

		    _font = new Font(StandardFonts.Helvetica, 8, false, false);
		    _size = new SizeF(0, 0);
		    _onlyLeftAndTop = true;

		    _initialText = string.Empty;
	    }

	    public TextString(string text, Font font, SizeF size, StringFormat sf)
	    {
		    _text = new List<string>();
		    _sf = sf;

		    _font = font;
		    _size = size;
		    _onlyLeftAndTop = false;

		    _initialText = text;
		    setList();
	    }

	    public TextString(string text, Font font, StringFormat sf)
	    {
		    _text = new List<string>();
		    _sf = sf;

		    _font = font;

		    int count = text.Length; // if text contains diacritic marks then it's false
		    bool isComplex = TextRenderer.ScriptIsComplex(text);
		    if (isComplex && Normalizer.ContainsDiacriticMarks(text))
			    count = Normalizer.GetTextLengthWithoutMarks(text);
            
		    float width  = font.BaseFont.FontBBox.Width * count;
		    float height = font.GetTextHeight();
		    _size = new SizeF(width, height);
		    _onlyLeftAndTop = true;

		    _initialText = text;
		    setList();
	    }

	    public void SetText(string text)
        {
            _initialText = text;
            setList();
        }

        public PointF[] GetCoordinate()
        {
            PointF[] points = new PointF[_text.Count];
            for (int i = 0; i < _text.Count; ++i)
                points[i] = getPoint(i);

            return points;
        }

        public static int GetSpaceCount(string text)
        {
            int countSpace = 0;
            for (int i = 0; i < text.Length; ++i)
            {
                int j = text.IndexOf(' ', i);
                if (j != -1)
                {
                    ++countSpace;
                    i = j;
                }
            }

            return countSpace;
        }

        public object[] GetArrayWords(int index)
        {
            if (_isComplex)
                return GetArrayWords_Complex(index);
            else
                return GetArrayWords_Simple(index);
        }

        private float getTextWidth(string text)
        {
            if (_isComplex && (Normalizer.ContainsDiacriticMarks(text)))
            {
                text = Normalizer.GetTextWithoutMarks(text);
            }

            return getTextWidth_Simple(text);
        }

        private PointF getPoint(int index)
        {
            //  When alignment means nothing.
            if (_onlyLeftAndTop && !_sf.DirectionRightToLeft)
            {
                if (_sf.HorizontalAlign != HorizontalAlign.Left)
                    _sf.HorizontalAlign = HorizontalAlign.Left;
            }

            if (_isComplex)
                return getPoint_Complex(index);
            else
                return getPoint_Simple(index);
        }

        private void setList()
        {
            // Lay Out Text Using Uniscribe
            // Assumes that the application has already divided the paragraph into runs.
            // Run == one font, one size, one color

            // 1. (Optional) Call ScriptIsComplex to determine if the paragraph requires complex processing.
            _isComplex = TextRenderer.ScriptIsComplex(_initialText);
            
            if (_isComplex)
            {
                if (!setList_Complex())
                    setList_Simple();
            }
            else
                setList_Simple();
        }

        #endregion
        
        #region Methods for simple script

        public object[] GetArrayWords_Simple(int index)
        {
            float wsOld = _sf.WordSpacing;
            string str = _text[index];
            int cSpace = GetSpaceCount(str);

            if (index != _text.Count - 1)
            {
                if (_sf.HorizontalAlign == HorizontalAlign.Justify)
                {
                    _sf.WordSpacing += (_size.Width - getTextWidth(str)) / cSpace;
                }
            }

            object[] arrayWords;
            if (_sf.WordSpacing == 0)
            {
                arrayWords = new object[1];
                arrayWords[0] = _font.BaseFont.ConvertStringToFontEncoding(str);
            }
            else
            {
                arrayWords = new object[cSpace * 2 + 1];
                int countSpace = 0;
                for (int i = 0; i < str.Length; ++i)
                {
                    int j = str.IndexOf(' ', i);
                    if (j != -1)
                    {
                        setEncodingWord(arrayWords, countSpace * 2, str.Substring(i, j - i));
                        setIndentForNext(arrayWords, countSpace * 2, _sf.WordSpacing + _font.GetTextWidth(" "));
                        ++countSpace;
                        i = j;
                        if (i == str.Length - 1)
                            arrayWords[countSpace * 2] = new PDFString("()");
                    }
                    else
                    {
                        setEncodingWord(arrayWords, countSpace * 2, str.Substring(i, str.Length - i));
                        i = str.Length;
                    }
                }
            }

            _sf.WordSpacing = wsOld;
            return arrayWords;
        }

        private void setEncodingWord(object[] arrayWords, int curIndex, string word)
        {
            arrayWords[curIndex] = _font.BaseFont.ConvertStringToFontEncoding(word);
            if (arrayWords[curIndex] == null)
				arrayWords[curIndex] = new PDFString("()");
        }

        private void setIndentForNext(object[] arrayWords, int curIndex, float indent)
        {
            arrayWords[curIndex + 1] = getPDFFormatValue(indent);
        }

        private float getTextWidth_Simple(string text)
        {
            return (_font.GetTextWidth(text) + _sf.CharacterSpacing * (text.Length - 1) + GetSpaceCount(text) * _sf.WordSpacing) * _sf.Scaling / 100;
        }

        private PointF getPoint_Simple(int index)
        {
            string str = _text[index].Trim();
            PointF result = new PointF(0.0f, (1 - _sf.Leading) * _font.GetTextHeight());

            if (_sf.HorizontalAlign == HorizontalAlign.Center)
            {
                result.X += (_size.Width - getTextWidth(str)) / 2;
            }
            if (_sf.HorizontalAlign == HorizontalAlign.Right)
            {
                result.X += _size.Width - getTextWidth(str);
            }

            if (_sf.VerticalAlign == VerticalAlign.Top)
            {
                result.Y += index * _sf.Leading * _font.GetTextHeight();
            }
            if (_sf.VerticalAlign == VerticalAlign.Center)
            {
                result.Y += _size.Height / 2 + (index - _text.Count) * _sf.Leading * _font.GetTextHeight() / 2;
            }
            if (_sf.VerticalAlign == VerticalAlign.Bottom)
            {
                result.Y += _size.Height + (index - _text.Count) * _sf.Leading * _font.GetTextHeight();
            }

            return result;
        }

        private string addString(string str)
        {
            if (getTextWidth(str) <= _size.Width)
            {
                _text.Add(str);
                return string.Empty;
            }
            else
                return divideAtChars(str);
        }

        private string divideAtChars(string str)
        {
            string result = string.Empty;
            float width = 0;

            for (int i = 0; i < str.Length; ++i)
            {
                float widthChar = _font.GetCharWidth(str[i]);
                if (width + widthChar > _size.Width)
                {
                    if (result != string.Empty)
                    {
                        _text.Add(result);
                        result = string.Empty;
                        width = 0;
                    }
                }

                result += str[i];
                width += widthChar;
            }

            return result;
        }

        private void setList_Simple()
        {
            string text = _initialText;

            _text.Clear();

            string currentText = string.Empty;
            string substring = string.Empty;

            for (int i = 0; i < text.Length; ++i)
            {
                if (text[i] == ' ' || text[i] == '\t')
                {
                    reachSpace(text, ref currentText, ref substring, i);
                }
                else if (text[i] == '\r' || text[i] == '\n')
                {
                    reachNewLine(text, ref currentText, ref substring, ref i);
                }
                else
                {
                    substring += text[i];
                }
            }

            addSubstringToCurrentText(ref currentText, ref substring);

            currentText = addString(currentText);
            if (currentText != string.Empty)
                _text.Add(currentText);
        }

        private void reachSpace(string text, ref string currentText, ref string substring, int i)
        {
            addSubstringToCurrentText(ref currentText, ref substring);
            currentText += (text[i] == ' ') ? WhiteSpace : TabSpace;
        }

        private void reachNewLine(string text, ref string currentText, ref string substring, ref int i)
        {
            addSubstringToCurrentText(ref currentText, ref substring);

            currentText = addString(currentText);
            if (currentText != string.Empty)
                _text.Add(currentText);

            if (i != text.Length - 1)
            {
                if (text[i] == '\r' && text[i + 1] == '\n')
                    ++i;
            }
        }

        private void addSubstringToCurrentText(ref string currentText, ref string substring)
        {
            if (getTextWidth(currentText) + getTextWidth(substring) > _size.Width)
            {
                if (currentText != string.Empty)
                {
                    currentText = addString(currentText);

					// remove starting space wrapped from the previous line
	                if (currentText == " ")
		                currentText = "";
                }
            }

            currentText += substring;
            substring = string.Empty;

            if (getTextWidth(currentText) > _size.Width)
            {
                currentText = addString(currentText);
            }
        }

        #endregion

        #region Methods for complex scripts

        public object[] GetArrayWords_Complex(int index)
        {
            float wsOld = _sf.WordSpacing;
            string str = _text[index].Trim();
            int cSpace = GetSpaceCount(str);

            if (index != _text.Count - 1)
            {
                if (_sf.HorizontalAlign == HorizontalAlign.Justify)
                {
                    // _sf.WordSpacing += (_size.Width - getTextWidth(str)) / cSpace;
                    _sf.WordSpacing += _restWordSpacingForScript[index];
                }
            }

            bool isRTLText = (Normalizer.GetRightToLeftTextType(str) != Normalizer.RightToLeftTextType.None);
            bool containsDiacrMarks = Normalizer.ContainsDiacriticMarks(str);

            float[] advances;
            float[] offsets;
            computeOffsets(str, isRTLText, containsDiacrMarks, out advances, out offsets);

            object[] arrayWords;
            if ((_sf.WordSpacing == 0) && (!isRTLText || !containsDiacrMarks))
            {
                arrayWords = new object[1];
                arrayWords[0] = _font.BaseFont.ConvertStringToFontEncoding(str);
            }
            else
            {
                if (!isRTLText || !containsDiacrMarks)
                    arrayWords = new object[cSpace * 2 + 1];
                else
                    arrayWords = new object[(str.Length - 1 - cSpace) * 2 + 1];

                int countSpace = 0;
                for (int i = 0; i < str.Length; ++i)
                {
                    int j = str.IndexOf(' ', i);
                    if (j != -1)
                    {
                        if (!isRTLText)
                        {
                            setEncodingWord(arrayWords, countSpace * 2, str.Substring(i, j - i));
                            setIndentForNext(arrayWords, countSpace * 2, _sf.WordSpacing + _font.GetTextWidth(" "));
                        }
                        else
                        {
                            if (!containsDiacrMarks)
                            {
                                setEncodingWord(arrayWords, countSpace * 2, str.Substring(i, j - i));
                                setIndentForNext(arrayWords, countSpace * 2, _restWordSpacingForScript[index] + _font.GetTextWidth(" "));
                            }
                            else
                            {
                                for (int k = i; k < j; k++)
                                {
                                    fillArrayWords(index, str, advances, offsets, arrayWords, countSpace, k, j - 1);
                                    ++countSpace;
                                }
                                --countSpace;
                            }
                        }
                        ++countSpace;
                        i = j;
                        if (i == str.Length - 1)
							arrayWords[countSpace * 2] = new PDFString("()");
                    }
                    else
                    {
                        if (!isRTLText)
                            setEncodingWord(arrayWords, countSpace * 2, str.Substring(i, str.Length - i));
                        else
                        {
                            if (!containsDiacrMarks)
                                setEncodingWord(arrayWords, countSpace * 2, str.Substring(i, str.Length - i));
                            else
                            {
                                for (int k = i; k < str.Length; k++)
                                {
                                    fillArrayWords(index, str, advances, offsets, arrayWords, countSpace, k, str.Length - 1);
                                    ++countSpace;
                                }
                                --countSpace;
                            }
                        }
                        i = str.Length;
                    }
                }
            }

            _sf.WordSpacing = wsOld;
            return arrayWords;
        }

        private void computeOffsets(string str, bool isRTLText, bool containsDiacrMarks, out float[] advances, out float[] offsets)
        {
            advances = null;
            offsets = null;

            if (!isRTLText || !containsDiacrMarks)
                return;

            advances = new float[str.Length];
            offsets = new float[str.Length];

            float baseCharWidth = 0;
            for (int i = 0; i < str.Length; i++)
            {
                offsets[i] = 0;
                advances[i] = _font.GetCharWidth(str[i]);

                if (!Normalizer.IsRightToLeftMark(str[i]))
                    baseCharWidth = advances[i];
                else
                {
                    // sometimes width of diacritic mark equals 0
                    if (advances[i] == 0)
                    {
                        offsets[i] = -(baseCharWidth + baseCharWidth * 2 / 3) / 2;
                    }
                    else
                    {
                        offsets[i] = -(baseCharWidth + advances[i]) / 2;
                        advances[i] *= -1;
                    }
                }
            }
        }

        private void fillArrayWords(int index, string str, float[] advances, float[] offsets, object[] arrayWords, int count, int k, int endOfWord)
        {
            string letterText = new string(new char[] { str[k] });
            setEncodingWord(arrayWords, count * 2, letterText);

            if (k != endOfWord)
            {
                if (advances[k] > 0) // base
                {
                    if (advances[k + 1] > 0) // base-base
                        setIndentForNext(arrayWords, count * 2, 0);
                    else // base-diacr
                        setIndentForNext(arrayWords, count * 2, offsets[k + 1]);
                }
                else // diacr
                {
                    if (advances[k + 1] > 0) // diacr-base
                        setIndentForNext(arrayWords, count * 2, -offsets[k] + advances[k]);
                    else // diacr-diacr
                    {
                        float baseCharWidth = getBaseCharWidth(advances, k);
                        setIndentForNext(arrayWords, count * 2, -baseCharWidth - 2 * offsets[k] + offsets[k + 1] + advances[k]);
                    }
                }
            }
            else
            {
                if (endOfWord != (str.Length - 1))
                {
                    if (advances[k] > 0) // base-space
                        setIndentForNext(arrayWords, count * 2, _restWordSpacingForScript[index] + _font.GetTextWidth(" "));
                    else // diacr-space
                        setIndentForNext(arrayWords, count * 2, -offsets[k] + advances[k] + _restWordSpacingForScript[index] + _font.GetTextWidth(" "));
                }
            }
        }

        private float getPDFFormatValue(float value)
        {
            return (float)(-(value) * 1000 / _font.Size);
        }

        private float getBaseCharWidth(float[] advances, int k)
        {
            float baseCharWidth = 0;
            for (int prev = k; prev >= 0; prev--)
            {
                if (advances[prev] > 0)
                {
                    baseCharWidth = advances[prev];
                    break;
                }
            }
            return baseCharWidth;
        }

        private PointF getPoint_Complex(int index)
        {
            HorizontalAlign oldAlign = _sf.HorizontalAlign;

            if (_sf.DirectionRightToLeft)
            {
                if ((_sf.HorizontalAlign != HorizontalAlign.Justify) || (index == _text.Count - 1))
                    _sf.HorizontalAlign = HorizontalAlign.Right;
            }

            string str = _text[index].Trim();
            PointF result = new PointF(0.0f, (1 - _sf.Leading) * _font.GetTextHeight());

            if (_onlyLeftAndTop && (_sf.HorizontalAlign == HorizontalAlign.Right))
            {
                _size.Width = getTextWidth(str);
                result.X -= _size.Width;
            }

            if (_sf.HorizontalAlign == HorizontalAlign.Center)
            {
                result.X += (_size.Width - getTextWidth(str)) / 2;
            }
            if (_sf.HorizontalAlign == HorizontalAlign.Right)
            {
                result.X += _size.Width - getTextWidth(str);
            }

            if (_sf.VerticalAlign == VerticalAlign.Top)
            {
                result.Y += index * _sf.Leading * _font.GetTextHeight();
            }
            if (_sf.VerticalAlign == VerticalAlign.Center)
            {
                result.Y += _size.Height / 2 + (index - _text.Count) * _sf.Leading * _font.GetTextHeight() / 2;
            }
            if (_sf.VerticalAlign == VerticalAlign.Bottom)
            {
                result.Y += _size.Height + (index - _text.Count) * _sf.Leading * _font.GetTextHeight();
            }

            _sf.HorizontalAlign = oldAlign;

            return result;
        }

        private string getOutPutLineFromLines(int index)
        {
            // Handle justification if need
            handleJastification(index);

            // Get line
            int count = _lines[index + 1] - _lines[index];
            string line = _shapedText.Substring(_items[_lines[index]].iCharPos,
                _items[_lines[index + 1]].iCharPos - _items[_lines[index]].iCharPos);

            // Get array of items for line and items order
            count++;
            Uniscribe.SCRIPT_ITEM[] items;
            int[] logical_to_visual;
            if (!getArrayOfItemsForLine(count, index, line, out items, out logical_to_visual))
                return line;

            // If line contains RTL block(s) then before drawing, reverse words in that block(s);
            return reverseWordsInLine(count, line, items, logical_to_visual);
        }

        private void handleJastification(int index)
        {
            clearRestSpacing(index);

            if (index == _lines.Count - 2)
                return;

            if (_sf.HorizontalAlign != HorizontalAlign.Justify)
                return;

            if (_onlyLeftAndTop)
                return;

            // handle justification for every RTL item
            // (adding kashidas (for Arabic only) or white spaces (for other))

            char justifingChar = ' ';
            if (_sf.DirectionRightToLeft)
                justifingChar = ArabicLigaturizer.Kashida;

            int startIndex = _lines[index];
            int endIndex = _lines[index + 1] - 1;

            float restWidth;
            visitAppropriateItems(justifingChar, startIndex, endIndex, out restWidth);
            if (restWidth != 0)
                visitAppropriateItems(justifingChar, startIndex, endIndex, out restWidth);

            _restWordSpacingForScript[index] = restWidth;

            // for adding kashida and white spaces there are two ways:
            // (a) do it yourself, filling table
            // (b) using uniscribe : 
            // call ScriptShapeForItem(...) and compare result with
            // call Normilazer.GetApprStringForm(...) and call ScriptGetFontProperties(...) and call GetCMap(...)
            // then call PlaceForItem(...), JustifyForItem(...)

            // (b) if we need of justification of complex script
            //       (1) we can call Uniscribe.ScriptShape(...) for every initial item
            //       (2) call Normalizer.GetAppropriateFormString(...) and Uniscribe.ScriptGetCMap(...)
            //       (3) compare results of (1) and (2): if (differ) then exit; else go to (4)
            //       (4) call Uniscribe.ScriptPlace(...) and Uniscribe.ScriptJustify(...)
            //       (5) look in results of (4): addition whitespace and kashidas(!)
        }

        private void visitAppropriateItems(char justifingChar, int startIndex, int endIndex, out float restWidth)
        {
            string line = _shapedText.Substring(_items[startIndex].iCharPos,
                            _items[endIndex + 1].iCharPos - _items[startIndex].iCharPos);

            float lineWidth = getTextWidth(line);
            float absWidth = adjustAbsWidth(startIndex, endIndex, lineWidth);

            float unengagedWidth = _size.Width - lineWidth;
            restWidth = unengagedWidth;

            visitJustifingItems(justifingChar, startIndex, endIndex, ref restWidth, absWidth);
        }

        private float adjustAbsWidth(int startIndex, int endIndex, float lineWidth)
        {
            float absWidth = lineWidth;

            char justifingChar = ' ';
            if (_sf.DirectionRightToLeft)
                justifingChar = ArabicLigaturizer.Kashida;

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (isAppopriateJustifingTypeItem(justifingChar, i))
                    continue;

                int length;
                string run = getRun(i, out length);

                absWidth -= getTextWidth(run);
            }

            return absWidth;
        }

        private void clearRestSpacing(int index)
        {
            if (index != 0)
                return;

            if (_restWordSpacingForScript != null)
                _restWordSpacingForScript.Clear();
            else
                _restWordSpacingForScript = new List<float>(_lines.Count - 1);

            for (int i = 0; i < _lines.Count - 1; i++)
            {
                _restWordSpacingForScript.Add(0);
            }
        }

        private void visitJustifingItems(char justifingChar, int startIndex, int endIndex, 
            ref float restWidth, float absWidth)
        {
            float justifingCharWidth = _font.GetCharWidth(justifingChar);

            float addedWidth = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!isAppopriateJustifingTypeItem(justifingChar, i))
                    continue;

                int length;
                string run = getRun(i, out length);

                float scale = getTextWidth(run) / absWidth;
                if (scale > 1) 
                    scale = 1 / scale;
                float partWidth = restWidth * scale;

                if (partWidth < justifingCharWidth)
                    continue;

                bool[] justifyPossiblePlaces;
                int possibleJustifingCharPlacesCount;
                computePossibleJustifingCharPlaces(justifingChar, length, run, out justifyPossiblePlaces, out possibleJustifingCharPlacesCount);

                if (possibleJustifingCharPlacesCount == 0)
                    continue;

                int justifingCharCount = (int)Math.Floor(partWidth / justifingCharWidth);
                if (possibleJustifingCharPlacesCount == 0)
                    justifingCharCount = 0;

                addedWidth += justifingCharCount * justifingCharWidth;
                handleJustifingCharPlaces(i, length, run, justifingChar, possibleJustifingCharPlacesCount, justifingCharCount, justifyPossiblePlaces);
            }

            restWidth -= addedWidth;
        }

        private bool isAppopriateJustifingTypeItem(char justifingChar, int i)
        {
            switch (justifingChar)
            {
                case ' ':
                    {
                        return !isRTLArabicItem(i);
                    }
                case ArabicLigaturizer.Kashida:
                    {
                        return isRTLArabicItem(i);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool isRTLArabicItem(int i)
        {
            if (!_items[i].a.fRTL && !_items[i].a.fLayoutRTL)
                return false;

            int length;
            string run = getRun(i, out length);

            if (Normalizer.GetRightToLeftTextType(run) != Normalizer.RightToLeftTextType.Arabic)
                return false;

            return true;
        }

        private void handleJustifingCharPlaces(int i, int length, string run, char justifingChar, int possibleJustifingPlacesCount, int justifingCount, bool[] justifyPossiblePlaces)
        {
            int[] justifingPlaces = getJustifingPlaces(possibleJustifingPlacesCount, ref justifingCount, justifyPossiblePlaces);
            string res_run = addJustifingCharsToLine(run, justifingChar, justifingPlaces);

            _shapedText = _shapedText.Substring(0, _items[i].iCharPos) + res_run + _shapedText.Substring(_items[i + 1].iCharPos);
            _font.BaseFont.AddStringToEncoding(new string(new char[] { justifingChar }));

            // After justification a length of string could change
            recomputeCharPos(i, length, res_run.Length);
        }

        private void computePossibleJustifingCharPlaces(char justifingChar, int length, string run, out bool[] justifyPossiblePlaces, out int possibleJustifingCharPlacesCount)
        {
            justifyPossiblePlaces = null;
            possibleJustifingCharPlacesCount = 0;

            switch(justifingChar)
            {
                case ' ':
                    {
                        computePossibleWhiteSpacePlaces(length, run, out justifyPossiblePlaces, out possibleJustifingCharPlacesCount);
                        break;
                    }
                case ArabicLigaturizer.Kashida:
                    {
                        computePossibleKahidaPlaces(length, run, out justifyPossiblePlaces, out possibleJustifingCharPlacesCount);
                        break;
                    }
            }
        }

        private void computePossibleWhiteSpacePlaces(int length, string run, out bool[] justifyPossiblePlaces, out int possibleWhiteSpacePlacesCount)
        {
            possibleWhiteSpacePlacesCount = 0;
            justifyPossiblePlaces = new bool[length];
            for (int j = 0; j < length - 1; j++)
            {
                if (!char.IsWhiteSpace(run[j]))
                    continue;

                if (char.IsWhiteSpace(run[j + 1]))
                    continue;

                justifyPossiblePlaces[j] = true;
                possibleWhiteSpacePlacesCount++;
            }
        }

        private void computePossibleKahidaPlaces(int length, string run, out bool[] justifyPossiblePlaces, out int possibleKashidaPlacesCount)
        {
            possibleKashidaPlacesCount = 0;
            justifyPossiblePlaces = new bool[length];
            for (int j = 0; j < length - 1; j++)
            {
                if (!ArabicLigaturizer.CanAddKashidaAfter(run[j]))
                    continue;

                if (!ArabicLigaturizer.CanAddKashidaBefore(run[j + 1]))
                    continue;

                justifyPossiblePlaces[j] = true;
                possibleKashidaPlacesCount++;
            }
        }

        private string addJustifingCharsToLine(string run, char justifingChar, int[] justifingPlaces)
        {
            string res_run = run;

            int index = 0;
            for (int k = 0; k < justifingPlaces.Length; k++)
            {
                if (justifingPlaces[k] != 0)
                {
                    res_run = res_run.Insert(index + 1, getCharAdding(justifingChar, justifingPlaces[k]));
                    index += justifingPlaces[k];
                }

                index++;
            }

            return res_run;
        }

        private int[] getJustifingPlaces(int possibleJustifingCharPlacesCount, ref int justifingCharCount, bool[] justifyPossiblePlaces)
        {
            int[] justifingPlaces = new int[justifyPossiblePlaces.Length];

            int denominator = 1;
            int intCount = denominator * justifingCharCount / possibleJustifingCharPlacesCount;

            while (justifingCharCount != 0)
            {
                if (intCount != 0)
                {
                    int account = 0;
                    for (int l = 0; l < justifingPlaces.Length; l++)
                    {
                        if (justifyPossiblePlaces[l])
                        {
                            if (account == 0)
                                justifingPlaces[l] += intCount;

                            account++;
                            if (account == denominator)
                                account = 0;
                        }
                    }
                    justifingCharCount -= intCount * possibleJustifingCharPlacesCount / denominator;
                }

                denominator++;
                intCount = denominator * justifingCharCount / possibleJustifingCharPlacesCount;
            }

            return justifingPlaces;
        }

        private void recomputeCharPos(int i, int beforeLength, int afterLength)
        {
            int delta = afterLength - beforeLength;
            if (delta != 0)
            {
                Uniscribe.SCRIPT_ITEM item;
                for (int j = i + 1; j < _items.Count; j++)
                {
                    item = _items[j];
                    item.iCharPos += delta;
                    _items[j] = item;
                }
            }
        }

        private string getCharAdding(char ch, int count)
        {
            StringBuilder sb = new StringBuilder(count);
            for (int i = 0; i < count; i++)
            {
                sb.Append(ch);
            }
            return sb.ToString();
        }

        private bool getArrayOfItemsForLine(int count, int index, string line, out Uniscribe.SCRIPT_ITEM[] items, out int[] logical_to_visual)
        {
            items = new Uniscribe.SCRIPT_ITEM[count];
            _items.CopyTo(_lines[index], items, 0, count);
            for (int i = 1; i < count; i++)
            {
                items[i].iCharPos -= items[0].iCharPos;
            }
            if (count > 0)
                items[0].iCharPos -= items[0].iCharPos;

            // 4. Call ScriptLayout

            int[] visual_to_logical = null;
            logical_to_visual = null;

            if (!TextRenderer.ScriptLayout(line, items, out visual_to_logical, out logical_to_visual))
                return false;

            return true;
        }

	    private string reverseWordsInLine(int count, string line, Uniscribe.SCRIPT_ITEM[] items, int[] logical_to_visual)
        {
            StringBuilder sb = new StringBuilder();

            // (1) change items order if need for line
            for (int i = 0; i < count - 1; i++)
            {
                int j = logical_to_visual[i];
                Uniscribe.SCRIPT_ITEM item = items[j];
                string str = line.Substring(item.iCharPos - items[0].iCharPos, items[j + 1].iCharPos - item.iCharPos);

                // for every RTL item
                if (item.a.fRTL/* || item.a.fLayoutRTL*/)
                {
                    // (2) change words order in item and char order in word if need 
                    str = Normalizer.ReverseTextWithoutReversingDigits(str);

                    // only for Arabic or Hebrew item
                    if (Normalizer.GetRightToLeftTextType(str) != Normalizer.RightToLeftTextType.None)
                    {
                        // (3) change order diacritical marks (diacritic marks must be after base symbol) 
                        str = Normalizer.ReverseOnlyDiacrMarks(str);
                    }
                }

                sb.Append(str);
            }

            return sb.ToString();
        }

        private bool setList_Complex()
        {
            _shapedText = _initialText;

            if (!itemize())
                return false;

            if (!containsRTLitems())
                return false;

            if (!mergeItemAndRunInfo())
                return false;

            if (!shapingRequires())
                return false;

            shape();

            replaceTabAndReturnCaretChars();
            breakParagraph();
            fillTextLines();

            computeAdditionalWordSpacing();

            return true;
        }

        private bool itemize()
        {
            // 2. Call ScriptItemize to divide the paragraph into items.
            // Item == one script, one direction

            Uniscribe.SCRIPT_ITEM[] items;
            if (!TextRenderer.ScriptItemize(_shapedText, out items, _sf.DirectionRightToLeft))
            {
                // TODO: how correctly handle such type of error
                return false;
            }

            if (_items != null)
                _items.Clear();
            else
                _items = new List<Uniscribe.SCRIPT_ITEM>(items.Length);

            _items.AddRange(items);
            return true;
        }

        private bool containsRTLitems()
        {
            bool containsRTLrun = false;
            for (int i = 0; i < _items.Count - 1; i++)
            {
                if (_items[i].a.fRTL || _items[i].a.fLayoutRTL)
                {
                    containsRTLrun = true;
                    break;
                }
            }

            if (!containsRTLrun)
            {
                // Script is complex but does not contain RTL run(s) (Arabic, Hebrew) that we can process
                return false;
            }

            return true;
        }

        private bool mergeItemAndRunInfo()
        {
            // 3. Merge the item information with the run information to produce ranges.
            // Range == intersection of Run and Item

            // Just skip that step cause one font

            // In dividing every item into ranges (intersection with runs) not need 
            // cause one font, size and color defined for all text

            return true;
        }

        private bool shapingRequires()
        {
            // Check before next step: is shaping needed?
            Uniscribe.SCRIPT_PROPERTIES[] ppScriptProperties;
            if (!TextRenderer.ScriptGetProperties(out ppScriptProperties))
                return false;

            // Determining If a Script Requires Glyph Shaping
            bool shaping = false;
            for (int i = 0; i < _items.Count; i++)
            {
                if (ppScriptProperties[_items[i].a.eScript].fComplex)
                {
                    shaping = true;
                    break;
                }
            }

            if (!shaping)
                return false;

            return true;
        }

        private void shape()
        {
            // 5. Call ScriptShape to identify clusters and generate glyphs.
            // Cluster == indivisable chars grouping

            // Font font = GetFontFromPDFFont(_font);

            // 6. If ScriptShape returns the code USP_E_SCRIPT_NOT_IN_FONT or S_OK 
            // with the output containing missing glyphs, select characters from a different font.
            // Either substitute another font or disable shaping by setting the eScript member 
            // of the SCRIPT_ANALYSIS structure passed to ScriptShape to SCRIPT_UNDEFINED. 
            // For more information, see Using Font Fallback.

            // Instead of Uniscribe.ScriptShape(...) use Normalizer.GetAppropriateFormString(...) for all items

            // Don't use the last item because it is a dummy that points
            // to the end of the string.
            for (int i = 0; i < _items.Count - 1; i++)
            {
                // For complex script words: 
                if (!_items[i].a.fRTL && !_items[i].a.fLayoutRTL)
                    continue;

                handleLigatures(i);
            }
        }

        private void breakParagraph()
        {
            // 7. Call ScriptPlace to generate advance widths and x and y positions for the glyphs 
            // in each successive range. This is the first step for which text size becomes a consideration.

            // 8. Sum the range sizes until the line overflows.

            // Compute size for all items
            List<float> items_size = computeItemsSize();

            // 9. Break the range on a word boundary by using the fSoftBreak and fWhiteSpace members 
            // in the logical attributes. To break a single character cluster off the run, 
            // use the information returned by calling ScriptBreak.

            // Note  Decide if the first code point of a range should be a word break point 
            // because the last character of the previous range requires it. 
            // For example, if one range ends in a comma, consider the first character of the next range 
            // to be a word break point.

            // Divide paragraph into lines (logical word order)
            breakParagraphIntoLines(items_size);

            // 10. Repeat steps 5 through 9 for each line in the paragraph. However, if breaking 
            // the last run on the line, call ScriptShape to reshape the remaining part 
            // of the run as the first run on the next line.
        }

        private void fillTextLines()
        {
            _text.Clear();

            // '\n' - new line

            for (int i = 0; i < _lines.Count - 1; i++)
            {
                string textLine = getOutPutLineFromLines(i);
                textLine = textLine.Replace(new string(new char[] { '\n' }), "");
                _text.Add(textLine);
            }
        }

        private void replaceTabAndReturnCaretChars()
        {
            // '\r' - carriage return, '\t' - horizontal tab

            for (int i = 0; i < _items.Count - 1; i++)
            {
                int length;
                string res_run = getRun(i, out length);

                res_run = res_run.Replace(new string(new char[] { '\r' }), "");
                res_run = res_run.Replace(new string(new char[] { '\t' }), TabSpace);

                _shapedText = _shapedText.Substring(0, _items[i].iCharPos) + res_run + _shapedText.Substring(_items[i + 1].iCharPos);

                // After replacements a length of string could change
                recomputeCharPos(i, length, res_run.Length);
            }
        }

        private void computeAdditionalWordSpacing()
        {
            for (int i = 0; i < _text.Count; i++)
            {
                string str = _text[i].Trim();

                if (!str.Equals(_text[i]))
                    _restWordSpacingForScript[i] += getTextWidth(_text[i]) - getTextWidth(str);

                int cSpace = GetSpaceCount(str);
                if (0 != cSpace)
                    _restWordSpacingForScript[i] /= cSpace;
            }
        }

        private void handleLigatures(int i)
        {
            int length;
            string run = getRun(i, out length);

            string res_run = Normalizer.GetAppropriateFormString(run, true);
            _font.BaseFont.AddStringToEncoding(res_run);

            _shapedText = _shapedText.Substring(0, _items[i].iCharPos) + res_run + _shapedText.Substring(_items[i + 1].iCharPos);

            // After shaping a length of string could change
            recomputeCharPos(i, length, res_run.Length);
        }

        private void breakParagraphIntoLines(List<float> items_size)
        {
            if (_lines != null)
                _lines.Clear();
            else
                _lines = new List<int>();

            // ' ' - white space

            if (_size.Width < getTextWidth("m"))
                throw new Exception("The width is less than necessary.");

            float lineSize = 0;
            for (int i = 0; i < _items.Count - 1; i++)
            {
                if (0 == lineSize)
                    _lines.Add(i);

                if (lineSize + items_size[i] < _size.Width) // no overflow
                    addWholeItemToLine(items_size, ref lineSize, i);
                else // overflow
                    breakItemBeforeAddingToLine(items_size, ref lineSize, ref i);
            }
            _lines.Add(_items.Count - 1);
        }

        private void breakItemBeforeAddingToLine(List<float> items_size, ref float lineSize, ref int i)
        {
            // check: does item contain ' '
            int length;
            string run = getRun(i, out length);
            int spaceIndex = run.IndexOfAny(new char[] { ' ' });

            if (-1 != spaceIndex)
                divideAtSpaces(items_size, ref lineSize, ref i, run, spaceIndex);
            else
            {
                if (0 == lineSize)
                    divideAtChars(items_size, ref lineSize, ref i, run);
                else
                    i--;
            }

            if (0 != lineSize)
                lineSize = 0;
        }

        private void divideAtChars(List<float> items_size, ref float lineSize, ref int i, string run)
        {
            int nextIndex = 0;
            while (nextIndex < run.Length)
            {
                string substring = new string(new char[] { run[nextIndex] });
                if ((lineSize + getTextWidth(substring) < _size.Width))
                {
                    lineSize += getTextWidth(substring);
                    nextIndex++;
                }
            }
            nextIndex--;

            int newLineIndex = run.IndexOf('\n');

            if (0 < nextIndex)
            {
                if ((0 < newLineIndex) && (newLineIndex < nextIndex))
                    divideItemAtIndex(i, newLineIndex, items_size);
                else
                    divideItemAtIndex(i, nextIndex, items_size);
            }
            else
            {
                if (0 < newLineIndex)
                    divideItemAtIndex(i, newLineIndex, items_size);
                else
                    i--;
            }
        }

        private void divideAtSpaces(List<float> items_size, ref float lineSize, ref int i, string run, int spaceIndex)
        {
            int nextIndex = spaceIndex;
            spaceIndex = -1;
            while ((nextIndex != -1) && (lineSize + getTextWidth(run.Substring(0, nextIndex)) < _size.Width))
            {
                spaceIndex = nextIndex;
                nextIndex = run.IndexOf(' ', nextIndex + 1);
            }

            int newLineIndex = run.IndexOf('\n');

            if (0 < spaceIndex)
            {
                if ((0 < newLineIndex) && (newLineIndex < spaceIndex))
                    divideItemAtIndex(i, newLineIndex, items_size);
                else
                {
                    while ((spaceIndex + 1 < run.Length) && (run[spaceIndex + 1] == ' '))
                        spaceIndex++;
                    divideItemAtIndex(i, spaceIndex, items_size);
                }
            }
            else
            {
                if (0 < newLineIndex)
                    divideItemAtIndex(i, newLineIndex, items_size);
                else
                    i--;
            }
        }

        private void addWholeItemToLine(List<float> items_size, ref float lineSize, int i)
        {
            lineSize += items_size[i];

            if (checkNewLine(items_size, i))
                lineSize = 0;
        }

        private bool checkNewLine(List<float> items_size, int i)
        {
            // check: does item contain '\n' ('\r')
            int length;
            string run = getRun(i, out length);
            int stopIndex = run.IndexOf('\n');

            if (-1 != stopIndex)
            {
                divideItemAtIndex(i, stopIndex, items_size);
                return true;
            }

            return false;
        }

        private List<float> computeItemsSize()
        {
            List<float> items_size = new List<float>(_items.Count - 1);
            for (int i = 0; i < _items.Count - 1; i++)
            {
                items_size.Add(getRunSize(i));
            }
            return items_size;
        }

        private string getRun(int i, out int length)
        {
            length = _items[i + 1].iCharPos - _items[i].iCharPos; // Length of this run.
            string run = _shapedText.Substring(_items[i].iCharPos, length);// Beginning of this run.
            return run;
        }

        private float getRunSize(int i)
        {
            int length;
            string run = getRun(i, out length);
            return getTextWidth(run);
        }

        private void divideItemAtIndex(int i, int stopIndex, List<float> items_size)
        {
            if (_items[i + 1].iCharPos - _items[i].iCharPos == 1)
                return;

            Uniscribe.SCRIPT_ITEM item = _items[i];
            item.iCharPos += stopIndex;
            _items.Insert(i + 1, item);

            items_size[i] = getRunSize(i);
            float size = getRunSize(i + 1);
            items_size.Insert(i + 1, size);
        }

        public static System.Drawing.Font GetFontFromPDFFont(Font font)
        {
            FontStyle style = FontStyle.Regular;

            if (font.Bold)
                style |= FontStyle.Bold;
            if (font.Italic)
                style |= FontStyle.Italic;
            if (font.Strikeout)
                style |= FontStyle.Strikeout;
            if (font.Underline)
                style |= FontStyle.Underline;

            System.Drawing.Font fnt = new System.Drawing.Font(font.Name, font.Size, style);
            return fnt;
        }

        #endregion
    }
}
