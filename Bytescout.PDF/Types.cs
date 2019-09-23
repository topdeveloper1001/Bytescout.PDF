namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal enum PageNumberingStyle
#else
	/// <summary>
    /// Specifies the numbering style to be used for the numeric portion of each page label.
    /// </summary>
    public enum PageNumberingStyle
#endif
	{
        /// <summary>
        /// Decimal arabic numerals.
        /// </summary>
        DecimalArabic = 0,
        /// <summary>
        /// Uppercase roman numerals.
        /// </summary>
        UppercaseRoman = 1,
        /// <summary>
        /// Lowercase roman numerals.
        /// </summary>
        LowercaseRoman = 2,
        /// <summary>
        /// Uppercase letters (A to Z for the first 26 pages, AA to ZZ for the next 26, and so on).
        /// </summary>
        UppercaseLetters = 3,
        /// <summary>
        /// Lowercase letters (a to z for the first 26 pages, aa to zz for the next 26, and so on).
        /// </summary>
        LowercaseLetters = 4,
        /// <summary>
        /// Missing
        /// </summary>
        None = 5
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum OptionalContentGroupItemType
#else
	/// <summary>
    /// Specifies the optional content group item type.
    /// </summary>
    public enum OptionalContentGroupItemType
#endif
	{
        /// <summary>
        /// The label.
        /// </summary>
        Label = 0,
        /// <summary>
        /// The layer.
        /// </summary>
        Layer = 1,
        /// <summary>
        /// The optional content group.
        /// </summary>
        Group = 2
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum OptionalContentState
#else
	/// <summary>
    /// Specifies the optional content group's state.
    /// </summary>
    public enum OptionalContentState
#endif
	{
        /// <summary>
        /// The state of the group is turned ON.
        /// </summary>
        On = 0,
        /// <summary>
        /// The state of the group is turned OFF.
        /// </summary>
        Off = 1,
        /// <summary>
        /// The state of the group is left unchanged.
        /// </summary>
        Unchanged = 2
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum TableBorderStyle
#else
	/// <summary>
    /// Specifies a border style for the table.
    /// </summary>
    public enum TableBorderStyle
#endif
	{
        /// <summary>
        /// A solid rectangle surrounding the cells.
        /// </summary>
        Solid = 0,
        /// <summary>
        /// A dashed rectangle surrounding the cells.
        /// </summary>
        Dashed = 1
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum PaperOrientation
#else
	/// <summary>
    /// Specifies paper orientation.
    /// </summary>
    public enum PaperOrientation
#endif
	{
        /// <summary>
        /// Paper is rotated into portrait orientation.
        /// </summary>
        Portrait = 0,
        /// <summary>
        /// Paper is rotated into landscape orientation.
        /// </summary>
        Landscape = 1
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum TilingType
#else
	/// <summary>
    /// Specifies adjustments to the spacing of tiles relative to the device pixel grid.
    /// </summary>
    public enum TilingType
#endif
	{
        /// <summary>
        /// Pattern cells are spaced consistently - that is, by a multiple of a device pixel.
        /// To achieve this, the application may need to distort the pattern cell slightly.
        /// The amount of distortion does not exceed 1 device pixel.
        /// </summary>
        ConstantSpacing = 1,
        /// <summary>
        /// The pattern cell is not distorted, but the spacing between pattern cells may vary
        /// by as much as 1 device pixel, both horizontally and vertically, when the pattern is painted.
        /// </summary>
        NoDistortion = 2,
        /// <summary>
        /// Pattern cells are spaced consistently as in Bytescout.PDF.TilingType.ConstantSpacing,
        /// but with additional distortion permitted to enable a more efficient implementation.
        /// </summary>
        ConstantSpacingAndFasterTiling = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum MoviePlayMode
#else
	/// <summary>
    /// Specifies the play mode for playing the movie.
    /// </summary>
    public enum MoviePlayMode
#endif
	{
        /// <summary>
        /// Play once and stop.
        /// </summary>
        Once = 0,
        /// <summary>
        /// Play and leave the movie controller bar open.
        /// </summary>
        Open = 1,
        /// <summary>
        /// Play repeatedly from beginning to end until stopped.
        /// </summary>
        Repeat = 2,
        /// <summary>
        /// Play continuously forward and backward until stopped.
        /// </summary>
        Palindrome = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum CaretSymbol
#else
	/// <summary>
    /// Specifies a symbol to be associated with the caret.
    /// </summary>
    public enum CaretSymbol
#endif
	{
        /// <summary>
        /// A new paragraph symbol (¶) should be associated with the caret.
        /// </summary>
        Paragraph = 0,
        /// <summary>
        /// No symbol should be associated with the caret.
        /// </summary>
        None = 1
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum LineCaptionType
#else
	/// <summary>
    /// Specifies the line caption type.
    /// </summary>
    public enum LineCaptionType
#endif
	{
        /// <summary>
        /// Indicates Inline as annotations caption positioning.
        /// </summary>
        Inline = 0,
        /// <summary>
        /// Indicates Top as annotations caption positioning.
        /// </summary>
        Top = 1
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum LineEndingStyle
#else
	/// <summary>
    /// Specifies the end of line style.
    /// </summary>
    public enum LineEndingStyle
#endif
	{
        /// <summary>
        /// A square.
        /// </summary>
        Square = 0,
        /// <summary>
        /// A circle.
        /// </summary>
        Circle = 1,
        /// <summary>
        /// A diamond shape.
        /// </summary>
        Diamond = 2,
        /// <summary>
        /// Two short lines meeting in an acute angle to form an open arrowhead.
        /// </summary>
        OpenArrow = 3,
        /// <summary>
        /// Two short lines meeting in an acute angle as in the Bytescout.PDF.PDFLineEndingStyle.OpenArrow style
        /// and connected by a third line to form a triangular closed arrowhead.
        /// </summary>
        ClosedArrow = 4,
        /// <summary>
        /// No line ending.
        /// </summary>
        None = 5,
        /// <summary>
        /// A short line at the endpoint perpendicular to the line itself.
        /// </summary>
        Butt = 6,
        /// <summary>
        /// Two short lines in the reverse direction from Bytescout.PDF.PDFLineEndingStyle.OpenArrow.
        /// </summary>
        ROpenArrow = 7,
        /// <summary>
        /// A triangular closed arrowhead in the reverse direction from Bytescout.PDF.PDFLineEndingStyle.ClosedArrow.
        /// </summary>
        RClosedArrow = 8,
        /// <summary>
        /// A short line at the endpoint approximately 30 degrees clockwise from perpendicular to the line itself.
        /// </summary>
        Slash = 9
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum SoundEncoding
#else
	/// <summary>
    /// Specifies the encoding format for the sample data.
    /// </summary>
    public enum SoundEncoding
#endif
	{
        /// <summary>
        /// Unspecified or unsigned values in the range 0 to 2^B − 1.
        /// </summary>
        Raw = 0,
        /// <summary>
        /// Twos-complement values.
        /// </summary>
        Signed = 1,
        /// <summary>
        /// μ-lawencoded samples.
        /// </summary>
        MuLaw = 2,
        /// <summary>
        /// A-lawencoded samples.
        /// </summary>
        ALaw = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum SubmitMethod
#else
	/// <summary>
    /// Specifies how control names and values are submitted.
    /// </summary>
    public enum SubmitMethod
#endif
	{
        /// <summary>
        /// Control names and values are submitted using an HTTP GET request.
        /// </summary>
        Get = 0,
        /// <summary>
        /// Control names and values are submitted using an HTTP POST request.
        /// </summary>
        Post = 1
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum SubmitDataFormat
#else
	/// <summary>
    /// Specifies a submit data format.
    /// </summary>
    public enum SubmitDataFormat
#endif
	{
        /// <summary>
        /// Control names and values are submitted in HTML Form format.
        /// </summary>
        HTML = 0,
        /// <summary>
        /// Control names and values are submitted in Forms Data Format (FDF).
        /// </summary>
        FDF = 1,
        /// <summary>
        /// Control names and values are submitted in XML Forms Data Format (XFDF).
        /// </summary>
        XFDF = 2,
        /// <summary>
        /// The whole document (not only control names and values) is submitted as a PDF, using the MIME content type application/pdf
        /// (described in Internet RFC 2045, Multipurpose Internet Mail Extensions (MIME), Part One: Format of Internet Message Bodies).
        /// </summary>
        PDF = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum MovieOperation
#else
	/// <summary>
    /// Specifies the operation to be performed on the movie.
    /// </summary>
    public enum MovieOperation
#endif
	{
        /// <summary>
        /// Start playing the movie.
        /// </summary>
        Play = 0,
        /// <summary>
        /// Stop playing the movie.
        /// </summary>
        Stop = 1,
        /// <summary>
        /// Pause a playing movie.
        /// </summary>
        Pause = 2,
        /// <summary>
        /// Resume a paused movie.
        /// </summary>
        Resume = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum NamedActions
#else
	/// <summary>
    /// Specifies the available named actions supported by the viewer.
    /// </summary>
    public enum NamedActions
#endif
	{
        /// <summary>
        /// Navigate to next page.
        /// </summary>
        NextPage = 0,
        /// <summary>
        /// Navigate to previous page.
        /// </summary>
        PreviousPage = 1,
        /// <summary>
        /// Navigate to first page.
        /// </summary>
        FirstPage = 2,
        /// <summary>
        /// Navigate to last page.
        /// </summary>
        LastPage = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum ActionType
#else
	/// <summary>
    /// Specifies type of Bytescout.PDF.PDFAction.
    /// </summary>
    public enum ActionType
#endif
	{
        /// <summary>
        /// Go to a destination in the current document.
        /// </summary>
        GoTo = 0,
        /// <summary>
        /// Go to a destination in another document.
        /// </summary>
        GoToRemote = 1,
        /// <summary>
        /// Go to a destination in an embedded file.
        /// </summary>
        GoToEmbedded = 2,
        /// <summary>
        /// Launch an application, usually to open a file.
        /// </summary>
        Launch = 3,
        /// <summary>
        /// Begin reading an article thread.
        /// </summary>
        Thread = 4,
        /// <summary>
        /// Resolve a uniform resource identifier.
        /// </summary>
        URI = 5,
        /// <summary>
        /// Play a sound.
        /// </summary>
        Sound = 6,
        /// <summary>
        /// Play a movie.
        /// </summary>
        Movie = 7,
        /// <summary>
        /// Set an annotations Hidden flag.
        /// </summary>
        Hide = 8,
        /// <summary>
        /// Execute an action predefined by the viewer application.
        /// </summary>
        Named = 9,
        /// <summary>
        /// Send data to a uniform resource locator.
        /// </summary>
        SubmitForm = 10,
        /// <summary>
        /// Set fields to their default values.
        /// </summary>
        ResetForm = 11,
        /// <summary>
        /// Import field values from a file.
        /// </summary>
        ImportData = 12,
        /// <summary>
        /// Execute a JavaScript script.
        /// </summary>
        JavaScript = 13,
        /// <summary>
        /// Set the states of optional content groups.
        /// </summary>
        SetOCGState = 14,
        /// <summary>
        /// Controls the playing of multimedia content.
        /// </summary>
        Rendition = 15,
        /// <summary>
        /// Uses a transition to update the display of a document.
        /// </summary>
        Transition = 16,
        /// <summary>
        /// Set the current view of a 3D annotation.
        /// </summary>
        GoTo3DView = 17
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum AnnotationType
#else
	/// <summary>
    /// Specifies type of Bytescout.PDF.Annotation.
    /// </summary>
    public enum AnnotationType
#endif
	{
        /// <summary>
        /// Text annotation.
        /// </summary>
        Text = 0,
        /// <summary>
        /// Link annotation.
        /// </summary>
        Link = 1,
        /// <summary>
        /// Free text annotation.
        /// </summary>
        FreeText = 2,
        /// <summary>
        /// Line annotation.
        /// </summary>
        Line = 3,
        /// <summary>
        /// Square annotation.
        /// </summary>
        Square = 4,
        /// <summary>
        /// Circle annotation.
        /// </summary>
        Circle = 5,
        /// <summary>
        /// Polygon annotation.
        /// </summary>
        Polygon = 6,
        /// <summary>
        /// Polyline annotation.
        /// </summary>
        PolyLine = 7,
        /// <summary>
        /// Highlight annotation.
        /// </summary>
        Highlight = 8,
        /// <summary>
        /// Underline annotation.
        /// </summary>
        Underline = 9,
        /// <summary>
        /// Squiggly-underline annotation.
        /// </summary>
        Squiggly = 10,
        /// <summary>
        /// Strikeout annotation.
        /// </summary>
        StrikeOut = 11,
        /// <summary>
        /// Rubber stamp annotation.
        /// </summary>
        Stamp = 12,
        /// <summary>
        /// Caret annotation.
        /// </summary>
        Caret = 13,
        /// <summary>
        /// Ink annotation.
        /// </summary>
        Ink = 14,
        /// <summary>
        /// File attachment annotation.
        /// </summary>
        FileAttachment = 15,
        /// <summary>
        /// Sound annotation.
        /// </summary>
        Sound = 16,
        /// <summary>
        /// Movie annotation.
        /// </summary>
        Movie = 17,
        /// <summary>
        /// Screen annotation.
        /// </summary>
        Screen = 18,
        /// <summary>
        /// 3D annotation.
        /// </summary>
        U3D = 19,
        /// <summary>
        /// Edit box.
        /// </summary>
        EditBox = 20,
        /// <summary>
        /// List box.
        /// </summary>
        ListBox = 21,
        /// <summary>
        /// Combo box.
        /// </summary>
        ComboBox = 22,
        /// <summary>
        /// Radio button.
        /// </summary>
        RadioButton = 23,
        /// <summary>
        /// Push button.
        /// </summary>
        PushButton = 24,
        /// <summary>
        /// Check box.
        /// </summary>
        CheckBox = 25,
        /// <summary>
        /// Signature.
        /// </summary>
        Signature = 26
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum RubberStampAnnotationIcon
#else
	/// <summary>
    /// Specifies the icon to be used in displaying Bytescout.PDF.RubberStampAnnotation.
    /// </summary>
    public enum RubberStampAnnotationIcon
#endif
	{
        /// <summary>
        /// Predefined "Approved" icon to be used.
        /// </summary>
        Approved = 0,
        /// <summary>
        /// Predefined "AsIs" icon to be used.
        /// </summary>
        AsIs = 1,
        /// <summary>
        /// Predefined "Confidential" icon to be used.
        /// </summary>
        Confidential = 2,
        /// <summary>
        /// Predefined "Departmental" icon to be used.
        /// </summary>
        Departmental = 3,
        /// <summary>
        /// Predefined "Draft" icon to be used.
        /// </summary>
        Draft = 4,
        /// <summary>
        /// Predefined "Experimental" icon to be used.
        /// </summary>
        Experimental = 5,
        /// <summary>
        /// Predefined "Expired" icon to be used.
        /// </summary>
        Expired = 6,
        /// <summary>
        /// Predefined "Final" icon to be used.
        /// </summary>
        Final = 8,
        /// <summary>
        /// Predefined "ForComment" icon to be used.
        /// </summary>
        ForComment = 9,
        /// <summary>
        /// Predefined "ForPublicRelease" icon to be used.
        /// </summary>
        ForPublicRelease = 10,
        /// <summary>
        /// Predefined "NotApproved" icon to be used.
        /// </summary>
        NotApproved = 11,
        /// <summary>
        /// Predefined "NotForPublicRelease" icon to be used.
        /// </summary>
        NotForPublicRelease = 12,
        /// <summary>
        /// Predefined "Sold" icon to be used.
        /// </summary>
        Sold = 13,
        /// <summary>
        /// Predefined "TopSecret" icon to be used.
        /// </summary>
        TopSecret = 14
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum FileAttachmentAnnotationIcon
#else
	/// <summary>
    /// Specifies the icon to be used in displaying Bytescout.PDF.FileAttachmentAnnotation.
    /// </summary>
    public enum FileAttachmentAnnotationIcon
#endif
	{
        /// <summary>
        /// Predefined "PushPin" icon to be used.
        /// </summary>
        PushPin = 0,
        /// <summary>
        /// Predefined "Graph" icon to be used.
        /// </summary>
        Graph = 1,
        /// <summary>
        /// Predefined "Paperclip" icon to be used.
        /// </summary>
        Paperclip = 2,
        /// <summary>
        /// Predefined "Tag" icon to be used.
        /// </summary>
        Tag = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum SoundAnnotationIcon
#else
	/// <summary>
    /// Specifies the icon to be used in displaying Bytescout.PDF.SoundAnnotation.
    /// </summary>
    public enum SoundAnnotationIcon
#endif
	{
        /// <summary>
        /// Predefined "Speaker" icon to be used.
        /// </summary>
        Speaker = 0,
        /// <summary>
        /// Predefined "Mic" icon to be used.
        /// </summary>
        Mic = 1,
        /// <summary>
        /// Predefined "Ear" icon to be used.
        /// </summary>
        Ear = 2
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum BorderStyle
#else
	/// <summary>
    /// Specifies a border style of the annotations.
    /// </summary>
    public enum BorderStyle
#endif
	{
        /// <summary>
        /// A solid rectangle surrounding the annotation.
        /// </summary>
        Solid = 0,
        /// <summary>
        /// A dashed rectangle surrounding the annotation.
        /// </summary>
        Dashed = 1,
        /// <summary>
        /// A simulated embossed rectangle that appears to be raised above the surface of the page.
        /// </summary>
        Beveled = 2,
        /// <summary>
        /// A simulated engraved rectangle that appears to be recessed below the surface of the page.
        /// </summary>
        Inset = 3,
        /// <summary>
        /// A single line along the bottom of the annotation rectangle.
        /// </summary>
        Underline = 4
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum BorderEffect
#else
	/// <summary>
    /// Specifies an effect to be applied to the border of the annotations.
    /// </summary>
    public enum BorderEffect
#endif
	{
        /// <summary>
        /// No effect.
        /// </summary>
        None = 0,
        /// <summary>
        /// The border should appear “cloudy”.
        /// </summary>
        Cloudy = 1
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum PushButtonHighlightingMode
#else
	/// <summary>
    /// Specifies the visual effect of the Bytescout.PDF.PushButton to be used when the mouse
    /// button is pressed or held down inside its active area.
    /// </summary>
    public enum PushButtonHighlightingMode
#endif
	{
        /// <summary>
        /// No highlighting.
        /// </summary>
        None = 0,
        /// <summary>
        /// Invert the contents of the annotation rectangle.
        /// </summary>
        Invert = 1,
        /// <summary>
        /// Invert the annotation’s border.
        /// </summary>
        Outline = 2,
        /// <summary>
        /// Display the annotation as if it were being pushed below the surface of the page.
        /// </summary>
        Push = 3,
        /// <summary>
        /// Same as Bytescout.PDF.PushButtonHighlightingMode.
        /// </summary>
        Toggle = 4
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum LinkAnnotationHighlightingMode
#else
	/// <summary>
    /// Specifies the visual effect of Bytescout.PDF.LinkAnnotation to be used when the mouse
    /// button is pressed or held down inside its active area.
    /// </summary>
    public enum LinkAnnotationHighlightingMode
#endif
	{
        /// <summary>
        /// No highlighting.
        /// </summary>
        None = 0,
        /// <summary>
        /// Invert the contents of the annotation rectangle.
        /// </summary>
        Invert = 1,
        /// <summary>
        /// Invert the annotation’s border.
        /// </summary>
        Outline = 2,
        /// <summary>
        /// Display the annotation as if it were being pushed below the surface of the page.
        /// </summary>
        Push = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum TextAnnotationIcon
#else
	/// <summary>
    /// Specifies the icon to be used in displaying Bytescout.PDF.TextAnnotation.
    /// </summary>
    public enum TextAnnotationIcon
#endif
	{
        /// <summary>
        /// Predefined "Comment" icon to be used.
        /// </summary>
        Comment = 0,
        /// <summary>
        /// Predefined "Help" icon to be used.
        /// </summary>
        Help = 1,
        /// <summary>
        /// Predefined "Insert" icon to be used.
        /// </summary>
        Insert = 2,
        /// <summary>
        /// Predefined "Key" icon to be used.
        /// </summary>
        Key = 3,
        /// <summary>
        /// Predefined "NewParagraph" icon to be used.
        /// </summary>
        NewParagraph = 4,
        /// <summary>
        /// Predefined "Note" icon to be used.
        /// </summary>
        Note = 5,
        /// <summary>
        /// Predefined "Paragraph" icon to be used.
        /// </summary>
        Paragraph = 6,
        /// <summary>
        /// Predefined "Circle" icon to be used.
        /// </summary>
        Circle = 7,
        /// <summary>
        /// Predefined "Cross" icon to be used.
        /// </summary>
        Cross = 8,
        /// <summary>
        /// Predefined "RightArrow" icon to be used.
        /// </summary>
        RightArrow = 9,
        /// <summary>
        /// Predefined "RightPointer" icon to be used.
        /// </summary>
        RightPointer = 10,
        /// <summary>
        /// Predefined "Star" icon to be used.
        /// </summary>
        Star = 11,
        /// <summary>
        /// Predefined "UpArrow" icon to be used.
        /// </summary>
        UpArrow = 12,
        /// <summary>
        /// Predefined "UpLeftArrow" icon to be used.
        /// </summary>
        UpLeftArrow = 13,
        /// <summary>
        /// Predefined "Check" icon to be used.
        /// </summary>
        Check = 14
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum TextAlign
#else
	/// <summary>
    /// Specifies the horizontal alignment of text.
    /// </summary>
    public enum TextAlign
#endif
	{
        /// <summary>
        /// Text is left justified.
        /// </summary>
        Left = 0,
        /// <summary>
        /// Text is centered.
        /// </summary>
        Center = 1,
        /// <summary>
        /// Text is right justified.
        /// </summary>
        Right = 2
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum HorizontalAlign
#else
	/// <summary>
    /// Specifies the horizontal alignment.
    /// </summary>
    public enum HorizontalAlign
#endif
	{ 
        /// <summary>
        /// Left justification.
        /// </summary>
        Left = 0,
        /// <summary>
        /// Center justification.
        /// </summary>
        Center = 1,
        /// <summary>
        /// Align on both the left and right side.
        /// </summary>
        Justify = 2,
        /// <summary>
        /// Right justification.
        /// </summary>
        Right = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum VerticalAlign
#else
	/// <summary>
    /// Specifies the vertical alignment.
    /// </summary>
    public enum VerticalAlign
#endif
	{
        /// <summary>
        /// Top justification.
        /// </summary>
        Top = 0,
        /// <summary>
        /// Center justification.
        /// </summary>
        Center = 1,
        /// <summary>
        /// Bottom justification.
        /// </summary>
        Bottom = 2
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum FillMode
#else
	/// <summary>
    /// Specifies the method used to fill a path.
    /// </summary>
    public enum FillMode
#endif
	{
        /// <summary>
        /// Determines whether a given point is inside a path by conceptually drawing a ray from that point
        /// to infinity in any direction and then examining the places where a segment of the path crosses
        /// the ray. Starting with a count of 0, the rule adds 1 each time a path segment crosses the ray from
        /// left to right and subtracts 1 each time a segment crosses from right to left. After counting all the
        /// crossings, if the result is 0, the point is outside the path; otherwise, it is inside.
        /// </summary>
        Winding = 0,
        /// <summary>
        /// Determines whether a point is inside a path by drawing a ray from that point in any direction
        /// and simply counting the number of path segments that cross the ray, regardless of direction.
        /// If this number is odd, the point is inside; if even, the point is outside. This yields the same
        /// results as the nonzero winding number rule for paths with simple shapes, but produces different
        /// results for more complex shapes.
        /// </summary>
        Alternate = 1
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum BlendMode
#else
	/// <summary>
    /// Specifies standard blend modes available in the PDF.
    /// </summary>
    public enum BlendMode
#endif
	{
        /// <summary>
        /// Selects the source color, ignoring the backdrop.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Multiplies the backdrop and source color values. The result color is always at least as dark
        /// as either of the two constituent colors.  Multiplying any color with black produces black;
        /// multiplying with white leaves the original color unchanged. Painting successive overlapping
        /// objects with a color other than black or white produces progressively darker colors.
        /// </summary>
        Multiply = 1,
        /// <summary>
        /// Multiplies the complements of the backdrop and source color values, then complements the result.
        /// The result color is always at least as light as either of the two constituent colors. Screening
        /// any color with white produces white;screening with black leaves the original color unchanged.
        /// The effect is similar to projecting multiple photographic slides simultaneously onto a single screen.
        /// </summary>
        Screen = 2,
        /// <summary>
        /// Multiplies or screens the colors, depending on the backdrop color value. Source colors overlay the
        /// backdrop while preserving its highlights and shadows. The backdrop color is not replaced but is mixed
        /// with the source color to reflect the lightness or darkness of the backdrop.
        /// </summary>
        Overlay = 3,
        /// <summary>
        /// Selects the darker of the backdrop and source colors. The backdrop is replaced with the source where
        /// the source is darker; otherwise, it is left unchanged.
        /// </summary>
        Darken = 4,
        /// <summary>
        /// Selects the lighter of the backdrop and source colors. The backdrop is replaced with the source where
        /// the source is lighter; otherwise, it is left unchanged.
        /// </summary>
        Lighten = 5,
        /// <summary>
        /// Brightens the backdrop color to reflect the source color. Painting with black produces no changes.
        /// </summary>
        ColorDodge = 6,
        /// <summary>
        /// Darkens the backdrop color to reflect the source color. Painting with white produces no change.
        /// </summary>
        ColorBurn = 7,
        /// <summary>
        /// Multiplies or screens the colors, depending on the source color value. The effect is similar to
        /// shining a harsh spotlight on the backdrop.
        /// </summary>
        HardLight = 8,
        /// <summary>
        /// Darkens or lightens the colors, depending on the source color value. The effect is similar to
        /// shining a diffused spotlight on the backdrop.
        /// </summary>
        SoftLight = 9,
        /// <summary>
        /// Subtracts the darker of the two constituent colors from the lighter color. Painting with white inverts
        /// the backdrop color; painting with black produces no change.
        /// </summary>
        Difference = 10,
        /// <summary>
        /// Produces an effect similar to that of the Bytescout.PDF.BlendMode.Difference mode but lower
        /// in contrast. Painting with white inverts the backdrop color; painting with black produces no change.
        /// </summary>
        Exclusion = 11,
        /// <summary>
        /// Creates a color with the hue of the source color and the saturation and luminosity of the backdrop color.
        /// </summary>
        Hue = 12,
        /// <summary>
        /// Creates a color with the saturation of the source color and the hue and luminosity of the backdrop color.
        /// Painting with this mode in an area of the backdrop that is a pure gray (no saturation) produces no change.
        /// </summary>
        Saturation = 13,
        /// <summary>
        /// Creates a color with the hue and saturation of the source color and the luminosity of the backdrop color.
        /// This preserves the gray levels of the backdrop and is useful for coloring monochrome images or tinting
        /// color images.
        /// </summary>
        Color = 14,
        /// <summary>
        /// Creates a color with the luminosity of the source color and the hue and saturation of the backdrop color.
        /// This produces an inverse effect to that of the Bytescout.PDF.BlendMode.Color mode.
        /// </summary>
        Luminosity = 15
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum RenderingIntentType
#else
	/// <summary>
    /// Specifies rendering intent.
    /// </summary>
    public enum RenderingIntentType
#endif
	{
        /// <summary>
        /// Colors are represented with respect to the combination of the light source and the output medium’s
        /// white point (such as the color of unprinted paper). Thus, for example, a monitor’s white point would
        /// be reproduced on a printer by simply leaving the paper unmarked, ignoring color differences between
        /// the two media. In-gamut colors are reproduced exactly; out-of-gamut colors are mapped to the nearest
        /// value within the reproducible gamut. This style of reproduction has the advantage of adapting for the
        /// varying white points of different output media. It has the disadvantage of not providing exact color
        /// matches from one medium to another. A typical use might be for vector graphics.
        /// </summary>
        RelativeColorimetric = 0,
        /// <summary>
        /// Colors are represented solely with respect to the light source; no correction is made for the
        /// output medium’s white point (such as the color of unprinted paper). Thus, for example, a
        /// monitor’s white point, which is bluish compared to that of a printer’s paper, would be reproduced
        /// with a blue cast. In-gamut colors are reproduced exactly; out-of-gamut colors are mapped to the
        /// nearest value within the reproducible gamut. This style of reproduction has the advantage of
        /// providing exact color matches from one output medium to another. It has the disadvantage of
        /// causing colors with Y values between the medium’s white point and 1.0 to be out of gamut.
        /// A typical use might be for logos and solid colors that require exact reproduction across different media.
        /// </summary>
        AbsoluteColorimetric = 1,
        /// <summary>
        /// Colors are represented in a manner that preserves or emphasizes saturation. Reproduction of in-gamut
        /// colors may or may not be colorimetrically accurate. A typical use might be for business graphics, where
        /// saturation is the most important attribute of the color.
        /// </summary>
        Saturation = 2,
        /// <summary>
        /// Colors are represented in a manner that provides a pleasing perceptual appearance. To preserve color
        /// relationships, both in-gamut and out-of-gamut colors are generally modified from their precise colorimetric
        /// values. A typical use might be for scanned images.
        /// </summary>
        Perceptual = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum LineCapStyle
#else
	/// <summary>
    /// Specifies the shape to be used at the ends of open subpaths (and dashes, if any) when they are stroked.
    /// </summary>
    public enum LineCapStyle
#endif
	{
        /// <summary>
        /// The stroke is squared off at the endpoint of the path. There is no projection beyond the end of the path.
        /// </summary>
        Butt = 0,
        /// <summary>
        /// A semicircular arc with a diameter equal to the line width is drawn around the endpoint and filled in.
        /// </summary>
        Round = 1,
        /// <summary>
        /// The stroke continues beyond the endpoint of the path for a distance equal to half the line width, and is squared off.
        /// </summary>
        ProjectingSquare = 2
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum LineJoinStyle
#else
	/// <summary>
    /// Specifies the shape of joints between connected segments of a stroked path.
    /// </summary>
    public enum LineJoinStyle
#endif
	{
        /// <summary>
        /// The outer edges of the strokes for the two segments are extended until they meet at an angle.
        /// If the segments meet at too sharp an angle (as defined by the miter limit parameter), a bevel
        /// join is used instead.
        /// </summary>
        Miter = 0,
        /// <summary>
        /// An arc of a circle with a diameter equal to the line width is drawn around the point where the
        /// two segments meet, connecting the outer edges of the strokes for the two segments.
        /// This pieslice-shaped figure is filled in, producing a rounded corner.
        /// </summary>
        Round = 1,
        /// <summary>
        /// The two segments are finished with butt caps (see Bytescout.PDF.LineCapStyle) and the resulting
        /// notch beyond the ends of the segments is filled with a triangle.
        /// </summary>
        Bevel = 2
    }
    
#if PDFSDK_EMBEDDED_SOURCES
	internal enum ZoomMode
#else
	/// <summary>
    /// Specifies zoom type.
    /// </summary>
    public enum ZoomMode
#endif
	{
        /// <summary>
        /// Display the page designated by page, with the coordinates (left, top) positioned at the upper-left corner of the window and the contents of the page magnified by the factor zoom.
        /// </summary>
        FitXYZ = 0,
        /// <summary>
        /// Display the page designated by page, with its contents magnified just enough to fit the entire page within the window both horizontally and vertically.
        /// </summary>
        FitPage = 1,
        /// <summary>
        /// Display the page designated by page, with its contents magnified just enough to fit the entire page within the window, both horizontally and vertically.
        /// </summary>
        FitHorizontal = 2,
        /// <summary>
        /// Display the page designated by page, with the horizontal coordinate left positioned at the left edge of the window and the contents of the page magnified just enough to fit
        /// the entire height of the page within the window.
        /// </summary>
        FitVertical = 3,
        /// <summary>
        /// Display the page designated by page, with its contents magnified just enough to fit the rectangle specified by the coordinates left, bottom, right, and top entirely within the window, both horizontally and vertically.
        /// </summary>
        FitRectangle = 4,
        /// <summary>
        /// Display the page designated by page, with its contents magnified just enough to fit its bounding box entirely within the window, both horizontally and vertically.
        /// </summary>
        FitBounding = 5,
        /// <summary>
        /// Display the page designated by page, with the vertical coordinate top positioned at the top edge of the window and the contents of the page magnified just enough to fit the
        /// entire width of its bounding box within the window.
        /// </summary>
        FitBoundingHorizontal = 6,
        /// <summary>
        /// Display the page designated by page, with the horizontal coordinate left positioned at the left edge of the window and the contents of the page magnified just enough to fit
        /// the entire height of its bounding box within the window.
        /// </summary>
        FitBoundingVertical = 7
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum Direction
#else
	/// <summary>
    /// Specifies predominant reading order for text.
    /// </summary>
    public enum Direction
#endif
	{
        /// <summary>
        /// Left to right.
        /// </summary>
        LeftToRight = 0,
        /// <summary>
        /// Right to left (including vertical writing systems, such as Chinese, Japanese, and Korean).
        /// </summary>
        RightToLeft = 1
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum FullScreenPageMode
#else
	/// <summary>
    /// Specifies how to display the document on exiting full-screen mode.
    /// </summary>
    public enum FullScreenPageMode
#endif
	{
        /// <summary>
        /// Neither the document outline nor thumbnail images visible.
        /// </summary>
        None = 0,
        /// <summary>
        /// Document outline visible.
        /// </summary>
        Outlines = 1,
        /// <summary>
        /// Thumbnail images visible.
        /// </summary>
        Thumbnail = 2,
        /// <summary>
        /// Optional content group panel visible.
        /// </summary>
        OptionalContent = 3
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum PageMode
#else
	/// <summary>
    /// Specifies how the document should be displayed when opened.
    /// </summary>
    public enum PageMode
#endif
	{
        /// <summary>
        /// Neither document outline nor thumbnail images visible.
        /// </summary>
        None = 0,
        /// <summary>
        /// Document outline visible.
        /// </summary>
        Outlines = 1,
        /// <summary>
        /// Thumbnail images visible.
        /// </summary>
        Thumbnail = 2,
        /// <summary>
        /// Full-screen mode, with no menu bar, window controls, or any other window visible.
        /// </summary>
        FullScreen = 3,
        /// <summary>
        /// Optional content group panel visible.
        /// </summary>
        OptionalContent = 4,
        /// <summary>
        /// Attachments panel visible.
        /// </summary>
        Attachment = 5
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum PageLayout
#else
	/// <summary>
    /// Specifies the page layout to be used when the document is opened.
    /// </summary>
    public enum PageLayout
#endif
	{
        /// <summary>
        /// Display one page at a time.
        /// </summary>
        SinglePage = 0,
        /// <summary>
        /// Display the pages in one column.
        /// </summary>
        OneColumn = 1,
        /// <summary>
        /// Display the pages in two columns, with odd-numbered pages on the left.
        /// </summary>
        TwoColumnLeft = 2,
        /// <summary>
        /// Display the pages in two columns, with odd-numbered pages on the right.
        /// </summary>
        TwoColumnRight = 3,
        /// <summary>
        /// Display the pages two at a time, with odd-numbered pages on the left.
        /// </summary>
        TwoPageLeft = 4,
        /// <summary>
        /// Display the pages two at a time, with odd-numbered pages on the right.
        /// </summary>
        TwoPageRight = 5
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum RotationAngle
#else
	/// <summary>
    /// Specifies the rotation angle.
    /// </summary>
    public enum RotationAngle
#endif
	{
        /// <summary>
        /// Not rotated.
        /// </summary>
        Rotate0 = 1,
        /// <summary>
        /// Rotate 90 degrees clockwise.
        /// </summary>
        Rotate90 = 2,
        /// <summary>
        /// Rotate 180 degrees clockwise.
        /// </summary>
        Rotate180 = 3,
        /// <summary>
        /// Rotate 270 degrees clockwise.
        /// </summary>
        Rotate270 = 4
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum Compression
#else
	/// <summary>
    /// Specifies compression filter.
    /// </summary>
    public enum Compression
#endif
	{
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0,
        /// <summary>
        /// An object is compressed using the "flate" method.
        /// </summary>
        Flate = 1
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum PrintQuality
#else
	/// <summary>
    /// Specifies print quality.
    /// </summary>
    public enum PrintQuality
#endif
	{
        /// <summary>
        /// High resolution.
        /// </summary>
        HightResolution = 0,
        /// <summary>
        /// Low resolution (150 DPI).
        /// </summary>
        LowResolution = 1
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum EncryptionAlgorithm
#else
	/// <summary>
    /// Specifies encryption algorithm.
    /// </summary>
    public enum EncryptionAlgorithm
#endif
	{
        /// <summary>
        /// Without encryption.
        /// </summary>
        None = 0,
        /// <summary>
        /// Low encryption (40-bit RC4). Compatible with Acrobat 3.0 and later.
        /// </summary>
        RC4_40bit = 1,
        /// <summary>
        /// High encryption (128-bit RC4). Compatible with Acrobat 5.0 and later.
        /// </summary>
        RC4_128bit = 2,
        /// <summary>
        /// High encryption (128-bit AES). Compatible with Acrobat 7.0 and later.
        /// </summary>
        AES_128bit = 3,
        /// <summary>
        /// Highest encryption (256-bit AES). Compatible with Acrobat 9.0 and later.
        /// </summary>
        AES_256bit = 4
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum StandardFonts
#else
	/// <summary>
    /// Specifies the standard PDF font families.
    /// </summary>
    public enum StandardFonts
#endif
	{
        /// <summary>
        /// Times New Roman font.
        /// </summary>
        Times = 0,
        /// <summary>
        /// Times New Roman bold font.
        /// </summary>
        TimesBold = 1,
        /// <summary>
        /// Times New Roman italic font.
        /// </summary>
        TimesItalic = 2,
        /// <summary>
        /// Times New Roman bold italic font.
        /// </summary>
        TimesBoldItalic = 3,
        /// <summary>
        /// Helvetica font.
        /// </summary>
        Helvetica = 4,
        /// <summary>
        /// Helvetica bold font.
        /// </summary>
        HelveticaBold = 5,
        /// <summary>
        /// Helvetica italic font.
        /// </summary>
        HelveticaOblique = 6,
        /// <summary>
        /// Helvetica bold italic font.
        /// </summary>
        HelveticaBoldOblique = 7,
        /// <summary>
        /// Courier font.
        /// </summary>
        Courier = 8,
        /// <summary>
        /// Courier bold font.
        /// </summary>
        CourierBold = 9,
        /// <summary>
        /// Courier italic font.
        /// </summary>
        CourierOblique = 10,
        /// <summary>
        /// Courier bold italic font.
        /// </summary>
        CourierBoldOblique = 11,
        /// <summary>
        /// Symbol font.
        /// </summary>
        Symbol = 12,
        /// <summary>
        /// Zapf Dingbats font.
        /// </summary>
        ZapfDingbats = 13
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum PaperFormat
#else
	/// <summary>
    /// Specifies the standard paper sizes.
    /// </summary>
    public enum PaperFormat
#endif
	{
        /// <summary>
        /// 841 mm by 1189 mm.
        /// </summary>
        A0 = 1,
        /// <summary>
        /// 594 mm by 841 mm.
        /// </summary>
        A1 = 2,
        /// <summary>
        /// 420 mm by 594 mm.
        /// </summary>
        A2 = 3,
        /// <summary>
        /// 297 mm by 420 mm.
        /// </summary>
        A3 = 4,
        /// <summary>
        /// 210 mm by 297 mm.
        /// </summary>
        A4 = 5,
        /// <summary>
        /// 148 mm by 210 mm.
        /// </summary>
        A5 = 6,
        /// <summary>
        /// 105 mm by 148 mm.
        /// </summary>
        A6 = 7,
        /// <summary>
        /// 250 mm by 353 mm.
        /// </summary>
        B4 = 8,
        /// <summary>
        /// 176 mm by 250 mm.
        /// </summary>
        B5 = 9,
        /// <summary>
        /// 8.5 in. by 11 in.
        /// </summary>
        Letter = 10,
        /// <summary>
        /// 8.5 in. by 14 in.
        /// </summary>
        Legal = 11,
        /// <summary>
        /// 8.5 in. by 13 in.
        /// </summary>
        Folio = 12,
        /// <summary>
        /// 7.25 in. by 10.5 in.
        /// </summary>
        Executive = 13,
        /// <summary>
        /// 250 mm by 353 mm.
        /// </summary>
        B4Envelope = 14,
        /// <summary>
        /// 176 mm by 250 mm.
        /// </summary>
        B5Envelope = 15,
        /// <summary>
        /// 114 mm by 162 mm.
        /// </summary>
        C6Envelope = 16,
        /// <summary>
        /// 110 mm by 220 mm.
        /// </summary>
        DLEnvelope = 17,
        /// <summary>
        /// 3.875 in. by 7.5 in.
        /// </summary>
        MonarchEnvelope = 18,
        /// <summary>
        /// 5.5 in. by 8.5 in.
        /// </summary>
        Statement = 19
    }
}