namespace Bytescout.PDF
{
    internal static class TypeConverter
    {
        #region PageNumberingStyle
        public static PDFName PDFPageNumberingStyleToPDFName(PageNumberingStyle style)
        {
            switch (style)
            {
                case PageNumberingStyle.DecimalArabic:
                    return new PDFName("D");
                case PageNumberingStyle.UppercaseRoman:
                    return new PDFName("R");
                case PageNumberingStyle.LowercaseRoman:
                    return new PDFName("r");
                case PageNumberingStyle.UppercaseLetters:
                    return new PDFName("A");
                case PageNumberingStyle.LowercaseLetters:
                    return new PDFName("a");
            }
            return new PDFName("");
        }

        public static PageNumberingStyle PDFNameToPDFPageNumberingStyle(PDFName name)
        {
            if (name == null)
                return PageNumberingStyle.None;
            switch (name.GetValue())
            {
                case "D":
                    return PageNumberingStyle.DecimalArabic;
                case "R":
                    return PageNumberingStyle.UppercaseRoman;
                case "r":
                    return PageNumberingStyle.LowercaseRoman;
                case "A":
                    return PageNumberingStyle.UppercaseLetters;
                case "a":
                    return PageNumberingStyle.LowercaseLetters;
            }
            return PageNumberingStyle.None;
        }
        #endregion

        #region OptionalContentState
        public static PDFName PDFOptionalContentStateToPDFName(OptionalContentState state)
        {
            switch (state)
            {
                case OptionalContentState.Off:
                    return new PDFName("OFF");
                case OptionalContentState.Unchanged:
                    return new PDFName("Unchanged");
            }
            return new PDFName("ON");
        }

        public static OptionalContentState PDFNameToPDFOptionalContentState(PDFName name)
        {
            if (name == null)
                return OptionalContentState.On;
            switch (name.GetValue())
            {
                case "OFF":
                    return OptionalContentState.Off;
                case "Unchanged":
                    return OptionalContentState.Unchanged;
            }
            return OptionalContentState.On;
        }
        #endregion

        #region FileAttachmentAnnotationIcon
        public static FileAttachmentAnnotationIcon PDFNameToPDFFileAttachmentAnnotationIcon(PDFName name)
        {
            if (name == null)
                return FileAttachmentAnnotationIcon.PushPin;
            switch (name.GetValue())
            {
                case "Graph":
                    return FileAttachmentAnnotationIcon.Graph;
                case "Paperclip":
                    return FileAttachmentAnnotationIcon.Paperclip;
                case "Tag":
                    return FileAttachmentAnnotationIcon.Tag;
            }
            return FileAttachmentAnnotationIcon.PushPin;
        }

        public static PDFName PDFFileAttachmentAnnotationIconToPDFName(FileAttachmentAnnotationIcon icon)
        {
            switch (icon)
            {
                case FileAttachmentAnnotationIcon.Graph:
                    return new PDFName("Graph");
                case FileAttachmentAnnotationIcon.Paperclip:
                    return new PDFName("Paperclip");
                case FileAttachmentAnnotationIcon.Tag:
                    return new PDFName("Tag");
            }
            return new PDFName("PushPin");
        }
        #endregion

        #region CaretSymbol
        public static CaretSymbol PDFNameToPDFCaretSymbol(PDFName name)
        {
            if (name == null)
                return CaretSymbol.None;
            if (name.GetValue() == "P")
                return CaretSymbol.Paragraph;
            return CaretSymbol.None;
        }

        public static PDFName PDFCaretSymbolToPDFName(CaretSymbol symbol)
        {
            if (symbol == CaretSymbol.Paragraph)
                return new PDFName("P");
            return new PDFName("None");
        }
        #endregion

        #region SoundAnnotationIcon
        public static SoundAnnotationIcon PDFNameToPDFSoundAnnotationIcon(PDFName name)
        {
            if (name == null)
                return SoundAnnotationIcon.Speaker;

            switch (name.GetValue())
            {
                case "Ear":
                    return SoundAnnotationIcon.Ear;
                case "Mic":
                    return SoundAnnotationIcon.Mic;
            }

            return SoundAnnotationIcon.Speaker;
        }

        public static PDFName PDFSoundAnnotationIconToPDFName(SoundAnnotationIcon icon)
        {
            switch (icon)
            {
                case SoundAnnotationIcon.Ear:
                    return new PDFName("Ear");
                case SoundAnnotationIcon.Mic:
                    return new PDFName("Mic");
            }

            return new PDFName("Speaker");
        }
        #endregion

        #region SoundEncoding
        public static SoundEncoding PDFNameToPDFSoundEncoding(PDFName name)
        {
            if (name == null)
                return SoundEncoding.Raw;

            switch (name.GetValue())
            {
                case "muLaw":
                    return SoundEncoding.MuLaw;
                case "ALaw":
                    return SoundEncoding.ALaw;
                case "Signed":
                    return SoundEncoding.Signed;
            }

            return SoundEncoding.Raw;
        }
        #endregion

        #region RubberStampAnnotationIcon
        public static PDFName PDFRubberStampAnnotationIconToPDFName(RubberStampAnnotationIcon icon)
        {
            switch (icon)
            {
                case RubberStampAnnotationIcon.Approved:
                    return new PDFName("Approved");
                case RubberStampAnnotationIcon.AsIs:
                    return new PDFName("AsIs");
                case RubberStampAnnotationIcon.Confidential:
                    return new PDFName("Confidential");
                case RubberStampAnnotationIcon.Departmental:
                    return new PDFName("Departmental");
                case RubberStampAnnotationIcon.Draft:
                    return new PDFName("Draft");
                case RubberStampAnnotationIcon.Experimental:
                    return new PDFName("Experimental");
                case RubberStampAnnotationIcon.Expired:
                    return new PDFName("Expired");
                case RubberStampAnnotationIcon.Final:
                    return new PDFName("Final");
                case RubberStampAnnotationIcon.ForComment:
                    return new PDFName("ForComment");
                case RubberStampAnnotationIcon.ForPublicRelease:
                    return new PDFName("ForPublicRelease");
                case RubberStampAnnotationIcon.NotApproved:
                    return new PDFName("NotApproved");
                case RubberStampAnnotationIcon.NotForPublicRelease:
                    return new PDFName("NotForPublicRelease");
                case RubberStampAnnotationIcon.Sold:
                    return new PDFName("Sold");
                case RubberStampAnnotationIcon.TopSecret:
                    return new PDFName("TopSecret");
            }
            return new PDFName("Draft");
        }

        public static RubberStampAnnotationIcon PDFNameToPDFRubberStampAnnotationIcon(PDFName name)
        {
            if (name == null)
                return RubberStampAnnotationIcon.Draft;
            switch (name.GetValue())
            {
                case "Approved":
                    return RubberStampAnnotationIcon.Approved;
                case "AsIs":
                    return RubberStampAnnotationIcon.AsIs;
                case "Confidential":
                    return RubberStampAnnotationIcon.Confidential;
                case "Departmental":
                    return RubberStampAnnotationIcon.Departmental;
                case "Experimental":
                    return RubberStampAnnotationIcon.Experimental;
                case "Expired":
                    return RubberStampAnnotationIcon.Expired;
                case "Final":
                    return RubberStampAnnotationIcon.Final;
                case "ForComment":
                    return RubberStampAnnotationIcon.ForComment;
                case "ForPublicRelease":
                    return RubberStampAnnotationIcon.ForPublicRelease;
                case "NotApproved":
                    return RubberStampAnnotationIcon.NotApproved;
                case "NotForPublicRelease":
                    return RubberStampAnnotationIcon.NotForPublicRelease;
                case "Sold":
                    return RubberStampAnnotationIcon.Sold;
                case "TopSecret":
                    return RubberStampAnnotationIcon.TopSecret;
            }

            return RubberStampAnnotationIcon.Draft;
        }
        #endregion

        #region PDFMoveOperation
        public static MovieOperation PDFNameToPDFMoveOperation(PDFName name)
        {
            if (name == null)
                return MovieOperation.Play;

            switch (name.GetValue())
            {
                case "Pause":
                    return MovieOperation.Pause;
                case "Resume":
                    return MovieOperation.Resume;
                case "Stop":
                    return MovieOperation.Stop;
            }

            return MovieOperation.Play;
        }

        public static PDFName PDFMoveOperationToPDFName(MovieOperation operation)
        {
            switch (operation)
            {
                case MovieOperation.Pause:
                    return new PDFName("Pause");
                case MovieOperation.Resume:
                    return new PDFName("Resume");
                case MovieOperation.Stop:
                    return new PDFName("Stop");
            }

            return new PDFName("Play");
        }
        #endregion

        #region NamedActions
        public static PDFName PDFNamedActionToPDFName(NamedActions action)
        {
            switch (action)
            {
                case NamedActions.FirstPage:
                    return new PDFName("FirstPage");
                case NamedActions.LastPage:
                    return new PDFName("LastPage");
                case NamedActions.NextPage:
                    return new PDFName("NextPage");
                case NamedActions.PreviousPage:
                    return new PDFName("PrevPage");
            }

            return new PDFName("NextPage");
        }

        public static NamedActions PDFNameToPDFNamedAction(PDFName name)
        {
            if (name == null)
                return NamedActions.NextPage;
            switch (name.GetValue())
            {
                case "FirstPage":
                    return NamedActions.FirstPage;
                case "LastPage":
                    return NamedActions.LastPage;
                case "NextPage":
                    return NamedActions.NextPage;
                case "PrevPage":
                    return NamedActions.PreviousPage;
            }

            return NamedActions.NextPage;
        }
        #endregion

        #region FileSystemType
        public static string PDFFileSystemToString(FileSystemType fs)
        {
            switch (fs)
            {
                case FileSystemType.DOS:
                    return "DOS";
                case FileSystemType.F:
                    return "F";
                case FileSystemType.Mac:
                    return "Mac";
                case FileSystemType.UF:
                    return "UF";
                case FileSystemType.Unix:
                    return "Unix";
            }
            return "";
        }

        public static FileSystemType StringToPDFFileSystem(string str)
        {
            switch (str)
            {
                case "F":
                    return FileSystemType.F;
                case "Mac":
                    return FileSystemType.Mac;
                case "UF":
                    return FileSystemType.UF;
                case "Unix":
                    return FileSystemType.Unix;
            }
            return FileSystemType.DOS;
        }
        #endregion

        #region PDFMovieMode
        public static MoviePlayMode PDFNameToMovieMode(PDFName name)
        {
            if (name == null)
                return MoviePlayMode.Once;
            switch (name.GetValue())
            { 
                case "Once":
                    return MoviePlayMode.Once;
                case "Open":
                    return MoviePlayMode.Open;
                case "Repeat":
                    return MoviePlayMode.Repeat;
                case "Palindrome":
                    return MoviePlayMode.Palindrome;
            }
            return MoviePlayMode.Once;
        }

        public static PDFName MovieModeToPDFName(MoviePlayMode mm)
        {
            switch (mm)
            { 
                case MoviePlayMode.Once:
                    return new PDFName("Once");
                case MoviePlayMode.Open:
                    return new PDFName("Open");
                case MoviePlayMode.Palindrome:
                    return new PDFName("Palindrome");
                case MoviePlayMode.Repeat:
                    return new PDFName("Repeat");
            }
            return new PDFName("Once");
        }
        #endregion

        #region PDFLineEndingStyle
        public static PDFName PDFLineEndingStyleToPDFName(LineEndingStyle style)
        {
            switch (style)
            {
                case LineEndingStyle.Butt:
                    return new PDFName("Butt");
                case LineEndingStyle.Circle:
                    return new PDFName("Circle");
                case LineEndingStyle.ClosedArrow:
                    return new PDFName("ClosedArrow");
                case LineEndingStyle.Diamond:
                    return new PDFName("Diamond");
                case LineEndingStyle.OpenArrow:
                    return new PDFName("OpenArrow");
                case LineEndingStyle.RClosedArrow:
                    return new PDFName("RClosedArrow");
                case LineEndingStyle.ROpenArrow:
                    return new PDFName("ROpenArrow");
                case LineEndingStyle.Slash:
                    return new PDFName("Slash");
                case LineEndingStyle.Square:
                    return new PDFName("Square");
            }

            return new PDFName("None");
        }

        public static LineEndingStyle PDFNameToPDFLineEndingStyle(PDFName name)
        {
            if (name == null)
                return LineEndingStyle.None;

            switch (name.GetValue())
            {
                case "Butt":
                    return LineEndingStyle.Butt;
                case "Circle":
                    return LineEndingStyle.Circle;
                case "ClosedArrow":
                    return LineEndingStyle.ClosedArrow;
                case "Diamond":
                    return LineEndingStyle.Diamond;
                case "None":
                    return LineEndingStyle.None;
                case "OpenArrow":
                    return LineEndingStyle.OpenArrow;
                case "RClosedArrow":
                    return LineEndingStyle.RClosedArrow;
                case "ROpenArrow":
                    return LineEndingStyle.ROpenArrow;
                case "Slash":
                    return LineEndingStyle.Slash;
                case "Square":
                    return LineEndingStyle.Square;
            }

            return LineEndingStyle.None;
        }
        #endregion

        #region TextAlign
        public static TextAlign PDFNumberToPDFTextAlign(PDFNumber num)
        {
            if (num == null)
                return TextAlign.Left;
            switch ((int)num.GetValue())
            {
                case 1:
                    return TextAlign.Center;
                case 2:
                    return TextAlign.Right;
            }
            return TextAlign.Left;
        }

        public static PDFNumber PDFTextAlignToPDFNumber(TextAlign justification)
        {
            switch (justification)
            {
                case TextAlign.Center:
                    return new PDFNumber(1);
                case TextAlign.Right:
                    return new PDFNumber(2);
            }
            return new PDFNumber(0);
        }
        #endregion

        #region Color
        public static DeviceColor PDFArrayToPDFColor(PDFArray color)
        {
            if (color == null || color.Count == 0)
                return new ColorRGB(0, 0, 0);

            switch (color.Count)
            {
                case 1:
                    PDFNumber q = color[0] as PDFNumber;
                    if (q == null || q.GetValue() > 1)
                        return new ColorGray(0);
                    return new ColorGray((byte)(q.GetValue() * 255));
                case 3:
                    PDFNumber red = color[0] as PDFNumber;
                    PDFNumber green = color[1] as PDFNumber;
                    PDFNumber blue = color[2] as PDFNumber;
                    byte r = 0, g = 0, b = 0;
                    if (red != null)
                        r = (byte)(red.GetValue() * 255);
                    if (green != null)
                        g = (byte)(green.GetValue() * 255);
                    if (blue != null)
                        b = (byte)(blue.GetValue() * 255);
                    return new ColorRGB(r, g, b);
                default:
                    PDFNumber cyan = color[0] as PDFNumber;
                    PDFNumber magenta = color[1] as PDFNumber;
                    PDFNumber yellow = color[2] as PDFNumber;
                    PDFNumber black = color[3] as PDFNumber;
                    byte c = 0, m = 0, y = 0, k = 0;
                    if (cyan != null)
                        c = (byte)(cyan.GetValue() * 255);
                    if (magenta != null)
                        m = (byte)(magenta.GetValue() * 255);
                    if (yellow != null)
                        y = (byte)(yellow.GetValue() * 255);
                    if (black != null)
                        k = (byte)(black.GetValue() * 255);
                    return new ColorCMYK(c, m, y, k);
            }
        }

        public static PDFArray PDFColorToPDFArray(DeviceColor value)
        {
            PDFArray color = new PDFArray();
            switch (value.Colorspace.N)
            {
                case 1:
                    color.AddItem(new PDFNumber((value as ColorGray).G * 1.0f / 255));
                    break;
                case 3:
                    color.AddItem(new PDFNumber((value as ColorRGB).R * 1.0f / 255));
                    color.AddItem(new PDFNumber((value as ColorRGB).G * 1.0f / 255));
                    color.AddItem(new PDFNumber((value as ColorRGB).B * 1.0f / 255));
                    break;
                case 4:
                    color.AddItem(new PDFNumber((value as ColorCMYK).C * 1.0f / 255));
                    color.AddItem(new PDFNumber((value as ColorCMYK).M * 1.0f / 255));
                    color.AddItem(new PDFNumber((value as ColorCMYK).Y * 1.0f / 255));
                    color.AddItem(new PDFNumber((value as ColorCMYK).K * 1.0f / 255));
                    break;
            }
            return color;
        }
        #endregion

        #region PushButtonHighlightingMode
        public static PushButtonHighlightingMode PDFNameToPDFPushButtonHighlightingMode(PDFName name)
        {
            if (name == null)
                return PushButtonHighlightingMode.Invert;
            switch (name.GetValue())
            {
                case "N":
                    return PushButtonHighlightingMode.None;
                case "O":
                    return PushButtonHighlightingMode.Outline;
                case "P":
                    return PushButtonHighlightingMode.Push;
                case "T":
                    return PushButtonHighlightingMode.Toggle;
            }
            return PushButtonHighlightingMode.Invert;
        }

        public static PDFName PDFPushButtonHighlightingModeToPDFName(PushButtonHighlightingMode mode)
        {
            switch (mode)
            {
                case PushButtonHighlightingMode.None:
                    return new PDFName("N");
                case PushButtonHighlightingMode.Outline:
                    return new PDFName("O");
                case PushButtonHighlightingMode.Push:
                    return new PDFName("P");
                case PushButtonHighlightingMode.Toggle:
                    return new PDFName("T");
            }
            return new PDFName("I");
        }
        #endregion

        #region BorderStyle
        public static BorderStyle PDFNameToPDFBorderStyle(PDFName name)
        {
            if (name == null)
                return BorderStyle.Solid;
            switch (name.GetValue())
            {
                case "D":
                    return BorderStyle.Dashed;
                case "B":
                    return BorderStyle.Beveled;
                case "I":
                    return BorderStyle.Inset;
                case "U":
                    return BorderStyle.Underline;
            }
            return BorderStyle.Solid;
        }

        public static PDFName PDFBorderStyleToPDFName(BorderStyle style)
        {
            switch (style)
            {
                case BorderStyle.Beveled:
                    return new PDFName("B");
                case BorderStyle.Dashed:
                    return new PDFName("D");
                case BorderStyle.Inset:
                    return new PDFName("I");
                case BorderStyle.Underline:
                    return new PDFName("U");
            }
            return new PDFName("S");
        }
        #endregion

        #region BorderEffect
        public static BorderEffect PDFNameToPDFBorderEffect(PDFName name)
        {
            if (name == null)
                return BorderEffect.None;
            if (name.GetValue() == "C")
                return BorderEffect.Cloudy;
            return BorderEffect.None;
        }

        public static PDFName PDFBorderEffectToPDFName(BorderEffect effect)
        {
            if (effect == BorderEffect.Cloudy)
                return new PDFName("C");
            return new PDFName("S");
        }
        #endregion

        #region LinkAnnotationHighlightingMode
        public static LinkAnnotationHighlightingMode PDFNameToPDFLinkAnnotationHighlightingMode(PDFName name)
        {
            if (name == null)
                return LinkAnnotationHighlightingMode.Invert;
            switch (name.GetValue())
            {
                case "N":
                    return LinkAnnotationHighlightingMode.None;
                case "O":
                    return LinkAnnotationHighlightingMode.Outline;
                case "P":
                    return LinkAnnotationHighlightingMode.Push;
            }
            return LinkAnnotationHighlightingMode.Invert;
        }

        public static PDFName PDFLinkAnnotationHighlightingModeToPDFName(LinkAnnotationHighlightingMode mode)
        {
            switch (mode)
            {
                case LinkAnnotationHighlightingMode.None:
                    return new PDFName("N");
                case LinkAnnotationHighlightingMode.Outline:
                    return new PDFName("O");
                case LinkAnnotationHighlightingMode.Push:
                    return new PDFName("P");
            }
            return new PDFName("I");
        }
        #endregion

        #region AnnotationState
        public static AnnotationState PDFStringToPDFAnnotationState(PDFString state, PDFString stateModel)
        {
            if (state == null)
            {
                if (stateModel == null)
                    return AnnotationState.None;

                if (stateModel.GetValue() == "Marked")
                    return AnnotationState.Unmarked;
                else
                    return AnnotationState.None;
            }

            switch (state.GetValue())
            {
                case "Accepted":
                    return AnnotationState.Accepted;
                case "Cancelled":
                    return AnnotationState.Cancelled;
                case "Completed":
                    return AnnotationState.Completed;
                case "Marked":
                    return AnnotationState.Marked;
                case "Rejected":
                    return AnnotationState.Rejected;
                case "Unmarked":
                    return AnnotationState.Unmarked;
            }

            return AnnotationState.None;
        }

        public static PDFString PDFAnnotationStateToPDFString(AnnotationState state)
        {
            switch (state)
            {
                case AnnotationState.Accepted:
                    return new PDFString(System.Text.Encoding.ASCII.GetBytes("Accepted"), false);
                case AnnotationState.Cancelled:
                    return new PDFString(System.Text.Encoding.ASCII.GetBytes("Cancelled"), false);
                case AnnotationState.Completed:
                    return new PDFString(System.Text.Encoding.ASCII.GetBytes("Completed"), false);
                case AnnotationState.Marked:
                    return new PDFString(System.Text.Encoding.ASCII.GetBytes("Marked"), false);
                case AnnotationState.Rejected:
                    return new PDFString(System.Text.Encoding.ASCII.GetBytes("Rejected"), false);
                case AnnotationState.Unmarked:
                    return new PDFString(System.Text.Encoding.ASCII.GetBytes("Unmarked"), false);
            }
            return new PDFString(System.Text.Encoding.ASCII.GetBytes("None"), false);
        }
        #endregion

        #region TextAnnotationIcon
        public static TextAnnotationIcon PDFNameToPDFTextAnnotationIcon(PDFName name)
        {
            if (name == null)
                return TextAnnotationIcon.Note;

            switch (name.GetValue())
            {
                case "Circle":
                    return TextAnnotationIcon.Circle;
                case "Comment":
                    return TextAnnotationIcon.Comment;
                case "Cross":
                    return TextAnnotationIcon.Cross;
                case "Help":
                    return TextAnnotationIcon.Help;
                case "Insert":
                    return TextAnnotationIcon.Insert;
                case "Key":
                    return TextAnnotationIcon.Key;
                case "NewParagraph":
                    return TextAnnotationIcon.NewParagraph;
                case "Paragraph":
                    return TextAnnotationIcon.Paragraph;
                case "RightArrow":
                    return TextAnnotationIcon.RightArrow;
                case "RightPointer":
                    return TextAnnotationIcon.RightPointer;
                case "Star":
                    return TextAnnotationIcon.Star;
                case "UpArrow":
                    return TextAnnotationIcon.UpArrow;
                case "UpLeftArrow":
                    return TextAnnotationIcon.UpLeftArrow;
                case "Check":
                    return TextAnnotationIcon.Check;
            }

            return TextAnnotationIcon.Note;
        }

        public static PDFName PDFTextAnnotationIconToPDFName(TextAnnotationIcon icon)
        {
            switch (icon)
            {
                case TextAnnotationIcon.Circle:
                    return new PDFName("Circle");
                case TextAnnotationIcon.Comment:
                    return new PDFName("Comment");
                case TextAnnotationIcon.Cross:
                    return new PDFName("Cross");
                case TextAnnotationIcon.Help:
                    return new PDFName("Help");
                case TextAnnotationIcon.Insert:
                    return new PDFName("Insert");
                case TextAnnotationIcon.Key:
                    return new PDFName("Key");
                case TextAnnotationIcon.NewParagraph:
                    return new PDFName("NewParagraph");
                case TextAnnotationIcon.Paragraph:
                    return new PDFName("Paragraph");
                case TextAnnotationIcon.RightArrow:
                    return new PDFName("RightArrow");
                case TextAnnotationIcon.RightPointer:
                    return new PDFName("RightPointer");
                case TextAnnotationIcon.Star:
                    return new PDFName("Star");
                case TextAnnotationIcon.UpArrow:
                    return new PDFName("UpArrow");
                case TextAnnotationIcon.UpLeftArrow:
                    return new PDFName("UpLeftArrow");
                case TextAnnotationIcon.Check:
                    return new PDFName("Check");
            }

            return new PDFName("Note");
        }
        #endregion

        #region ZoomMode
        public static ZoomMode PDFNameToPDFZoomMode(PDFName name)
        {
            if (name == null)
                return ZoomMode.FitPage;

            switch (name.GetValue())
            {
                case "FitB":
                    return ZoomMode.FitBounding;
                case "FitBH":
                    return ZoomMode.FitBoundingHorizontal;
                case "FitBV":
                    return ZoomMode.FitBoundingVertical;
                case "FitH":
                    return ZoomMode.FitHorizontal;
                case "Fit":
                    return ZoomMode.FitPage;
                case "FitR":
                    return ZoomMode.FitRectangle;
                case "FitV":
                    return ZoomMode.FitVertical;
                case "XYZ":
                    return ZoomMode.FitXYZ;
            }
            return ZoomMode.FitPage;
        }
        #endregion

        #region RenderingIntentType
        public static string PDFRenderingIntentToString(RenderingIntentType ri)
        {
            switch (ri)
            {
                case RenderingIntentType.AbsoluteColorimetric:
                    return "AbsoluteColorimetric";                    
                case RenderingIntentType.RelativeColorimetric:
                    return "RelativeColorimetric";
                case RenderingIntentType.Perceptual:
                    return "Perceptual";
                case RenderingIntentType.Saturation:
                    return "Saturation";
            }
            return "";
        }
        #endregion

        #region BlendMode
        public static string PDFBlendModeToString(BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Normal:
                    return "Normal";
                case BlendMode.Color:
                    return "Color";
                case BlendMode.ColorBurn:
                    return "ColorBurn";
                case BlendMode.ColorDodge:
                    return "ColorDodge";
                case BlendMode.Darken:
                    return "Darken";
                case BlendMode.Difference:
                    return "Difference";
                case BlendMode.Exclusion:
                    return "Exclusion";
                case BlendMode.HardLight:
                    return "HardLight";
                case BlendMode.Hue:
                    return "Hue";
                case BlendMode.Lighten:
                    return "Lighten";
                case BlendMode.Luminosity:
                    return "Luminosity";
                case BlendMode.Multiply:
                    return "Multiply";
                case BlendMode.Overlay:
                    return "Overlay";
                case BlendMode.Saturation:
                    return "Saturation";
                case BlendMode.Screen:
                    return "Screen";
                case BlendMode.SoftLight:
                    return "SortLight";
            }
            return "";
        }
        #endregion

        #region PageLayout
        public static PDFName PDFPageLayoutToPDFName(PageLayout type)
        {
            switch (type)
            {
                case PageLayout.SinglePage:
                    return new PDFName("SinglePage");
                case PageLayout.OneColumn:
                    return new PDFName("OneColumn");
                case PageLayout.TwoColumnLeft:
                    return new PDFName("TwoColumnLeft");
                case PageLayout.TwoColumnRight:
                    return new PDFName("TwoColumnRight");
                case PageLayout.TwoPageLeft:
                    return new PDFName("TwoPageLeft");
                case PageLayout.TwoPageRight:
                    return new PDFName("TwoPageRight");
            }
            return new PDFName("SinglePage");
        }

        public static PageLayout PDFNameToPDFPageLayout(PDFName name)
        {
            if (name == null)
                return PageLayout.SinglePage;

            switch (name.GetValue())
            {
                case "SinglePage":
                    return PageLayout.SinglePage;
                case "OneColumn":
                    return PageLayout.OneColumn;
                case "TwoColumnLeft":
                    return PageLayout.TwoColumnLeft;
                case "TwoColumnRight":
                    return PageLayout.TwoColumnRight;
                case "TwoPageLeft":
                    return PageLayout.TwoPageLeft;
                case "TwoPageRight":
                    return PageLayout.TwoPageRight;
            }
            return PageLayout.SinglePage;
        }
        #endregion

        #region PageMode
        public static PDFName PDFPageModeToPDFName(PageMode type)
        {
            switch (type)
            {
                case PageMode.None:
                    return new PDFName("UseNone");
                case PageMode.Outlines:
                    return new PDFName("UseOutlines");
                case PageMode.Thumbnail:
                    return new PDFName("UseThumbs");
                case PageMode.FullScreen:
                    return new PDFName("FullScreen");
                case PageMode.OptionalContent:
                    return new PDFName("UseOC");
                case PageMode.Attachment:
                    return new PDFName("UseAttachments");
            }
            return new PDFName("UseNone");
        }

        public static PageMode PDFNameToPDFPageMode(PDFName name)
        {
            if (name == null)
                return PageMode.None;

            string pageMode = name.GetValue();
            switch (pageMode)
            {
                case "UseNone":
                    return PageMode.None;
                case "UseOutlines":
                    return PageMode.Outlines;
                case "UseThumbs":
                    return PageMode.Thumbnail;
                case "FullScreen":
                    return PageMode.FullScreen;
                case "UseOC":
                    return PageMode.OptionalContent;
                case "UseAttachments":
                    return PageMode.Attachment;
            }
            return PageMode.None;
        }
        #endregion

        #region FullScreenPageMode
        public static PDFName PDFFullScreenPageModeToPDFName(FullScreenPageMode type)
        {
            switch (type)
            {
                case FullScreenPageMode.OptionalContent:
                    return new PDFName("UseOC");
                case FullScreenPageMode.Outlines:
                    return new PDFName("UseOutlines");
                case FullScreenPageMode.Thumbnail:
                    return new PDFName("UseThumbs");
            }
            return new PDFName("UseNone");
        }

        public static FullScreenPageMode PDFNameToPDFFullScreenPageMode(PDFName name)
        {
            if (name == null)
                return FullScreenPageMode.None;
            
            string mode = name.GetValue();
            switch (mode)
            {
                case "UseOutlines":
                    return FullScreenPageMode.Outlines;
                case "UseThumbs":
                    return FullScreenPageMode.Thumbnail;
                case "UseOC":
                    return FullScreenPageMode.OptionalContent;
            }
            return FullScreenPageMode.None;
        }
        #endregion

        #region Direction
        public static PDFName PDFDirectionToPDFName(Direction type)
        {
            if (type == Direction.RightToLeft)
                return new PDFName("L2R");
            return new PDFName("R2L");
        }

        public static Direction PDFNameToPDFDirection(PDFName name)
        {
            if (name == null)
                return Direction.LeftToRight;
            if (name.GetValue() == "R2L")
                return Direction.RightToLeft;
            return Direction.LeftToRight;
        }
        #endregion

        #region RotationAngle
        public static PDFNumber RotationAngleToPDFNumber(RotationAngle angle)
        {
            switch (angle)
            {
                case RotationAngle.Rotate180:
                    return new PDFNumber(180);
                case RotationAngle.Rotate270:
                    return new PDFNumber(270);
                case RotationAngle.Rotate90:
                    return new PDFNumber(90);
            }
            return new PDFNumber(0);
        }

        public static RotationAngle PDFNumberToRotationAngle(PDFNumber num)
        {
            if (num == null)
                return RotationAngle.Rotate0;

            int angle = (int)num.GetValue();
            angle = angle % 360;

            if (angle == 90 || angle == -270)
                return RotationAngle.Rotate90;
            else if (angle == 180 || angle == -180)
                return RotationAngle.Rotate180;
            else if (angle == 270 || angle == -90)
                return RotationAngle.Rotate270;

            return RotationAngle.Rotate0;
        }
        #endregion
    }
}
