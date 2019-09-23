using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ComHelpers
#else
	/// <summary>
	/// Class containing helping methods to use the SDK from VBScript as ActiveX object.
	/// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class ComHelpers
#endif
	{
		private readonly Document _document;

		internal Document Document
		{
			get { return _document; }
		}

		// Constants emulation

		/// <summary> Times New Roman font. </summary>
		public int STANDARDFONTS_TIMES { get { return 0; }}
		/// <summary> Times New Roman bold font. </summary>
		public int STANDARDFONTS_TIMESBOLD { get { return 1; } }
		/// <summary> Times New Roman italic font. </summary>
		public int STANDARDFONTS_TIMESITALIC { get { return 2; } }
		/// <summary> Times New Roman bold italic font. </summary>
		public int STANDARDFONTS_TIMESBOLDITALIC { get { return 3; } }
		/// <summary> Helvetica font. </summary>
		public int STANDARDFONTS_HELVETICA { get { return 4; } }
		/// <summary> Helvetica bold font. </summary>
		public int STANDARDFONTS_HELVETICABOLD { get { return 5; } }
		/// <summary> Helvetica italic font. </summary>
		public int STANDARDFONTS_HELVETICAOBLIQUE { get { return 6; } }
		/// <summary> Helvetica bold italic font. </summary>
		public int STANDARDFONTS_HELVETICABOLDOBLIQUE { get { return 7; } }
		/// <summary> Courier font. </summary>
		public int STANDARDFONTS_COURIER { get { return 8; } }
		/// <summary> Courier bold font. </summary>
		public int STANDARDFONTS_COURIERBOLD { get { return 9; } }
		/// <summary> Courier italic font. </summary>
		public int STANDARDFONTS_COURIEROBLIQUE { get { return 10; } }
		/// <summary> Courier bold italic font. </summary>
		public int STANDARDFONTS_COURIERBOLDOBLIQUE { get { return 11; } }
		/// <summary> Symbol font. </summary>
		public int STANDARDFONTS_SYMBOL { get { return 12; } }
		/// <summary> Zapf Dingbats font. </summary>
		public int STANDARDFONTS_ZAPFDINGBATS { get { return 13; } }

		/// <summary> 841 mm by 1189 mm. </summary>
		public int PAPERFORMAT_A0 { get { return 1; } }
		/// <summary> 594 mm by 841 mm. </summary>
		public int PAPERFORMAT_A1 { get { return 2; } }
		/// <summary> 420 mm by 594 mm. </summary>
		public int PAPERFORMAT_A2 { get { return 3; } }
		/// <summary> 297 mm by 420 mm. </summary>
		public int PAPERFORMAT_A3 { get { return 4; } }
		/// <summary> 210 mm by 297 mm. </summary>
		public int PAPERFORMAT_A4 { get { return 5; } }
		/// <summary> 148 mm by 210 mm. </summary>
		public int PAPERFORMAT_A5 { get { return 6; } }
		/// <summary> 105 mm by 148 mm. </summary>
		public int PAPERFORMAT_A6 { get { return 7; } }
		/// <summary> 250 mm by 353 mm. </summary>
		public int PAPERFORMAT_B4 { get { return 8; } }
		/// <summary> 176 mm by 250 mm. </summary>
		public int PAPERFORMAT_B5 { get { return 9; } }
		/// <summary> 8.5 in. by 11 in. </summary>
		public int PAPERFORMAT_LETTER { get { return 10; } }
		/// <summary> 8.5 in. by 14 in. </summary>
		public int PAPERFORMAT_LEGAL { get { return 11; } }
		/// <summary> 8.5 in. by 13 in. </summary>
		public int PAPERFORMAT_FOLIO { get { return 12; } }
		/// <summary> 7.25 in. by 10.5 in. </summary>
		public int PAPERFORMAT_EXECUTIVE { get { return 13; } }
		/// <summary> 250 mm by 353 mm. </summary>
		public int PAPERFORMAT_B4ENVELOPE { get { return 14; } }
		/// <summary> 176 mm by 250 mm. </summary>
		public int PAPERFORMAT_B5ENVELOPE { get { return 15; } }
		/// <summary> 114 mm by 162 mm. </summary>
		public int PAPERFORMAT_C6ENVELOPE { get { return 16; } }
		/// <summary> 110 mm by 220 mm. </summary>
		public int PAPERFORMAT_DLENVELOPE { get { return 17; } }
		/// <summary> 3.875 in. by 7.5 in. </summary>
		public int PAPERFORMAT_MONARCHENVELOPE { get { return 18; } }
		/// <summary> 5.5 in. by 8.5 in. </summary>
		public int PAPERFORMAT_STATEMENT { get { return 19; } }

		/// <summary> </summary>
		public int PAPERORIENTATION_PORTRAIT { get { return 0; } }
		/// <summary> </summary>
		public int PAPERORIENTATION_LANDSCAPE { get { return 1; } }

		/// <summary> </summary>
		public int SUBMITDATAFORMAT_HTML { get { return 0; } }
		/// <summary> </summary>
		public int SUBMITDATAFORMAT_FDF { get { return 1; } }
		/// <summary> </summary>
		public int SUBMITDATAFORMAT_XFDF { get { return 2; } }
		/// <summary> </summary>
		public int SUBMITDATAFORMAT_PDF { get { return 3; } }

		/// <summary> </summary>
		public int SUBMITMETHOD_GET { get { return 0; } }
		/// <summary> </summary>
		public int SUBMITMETHOD_POST { get { return 1; } }

		/// <summary> </summary>
		public int FILEATTACHMENTANNOTATIONICON_PUSHPIN { get { return 0; } }
		/// <summary> </summary>
		public int FILEATTACHMENTANNOTATIONICON_GRAPH { get { return 1; } }
		/// <summary> </summary>
		public int FILEATTACHMENTANNOTATIONICON_PAPERCLIP { get { return 2; } }
		/// <summary> </summary>
		public int FILEATTACHMENTANNOTATIONICON_TAG { get { return 3; } }

		/// <summary> </summary>
		public int PAGEMODE_NONE { get { return 0; } }
		/// <summary> </summary>
		public int PAGEMODE_OUTLINES { get { return 1; } }
		/// <summary> </summary>
		public int PAGEMODE_THUMBNAIL { get { return 2; } }
		/// <summary> </summary>
		public int PAGEMODE_FULLSCREEN { get { return 3; } }
		/// <summary> </summary>
		public int PAGEMODE_OPTIONALCONTENT { get { return 4; } }
		/// <summary> </summary>
		public int PAGEMODE_ATTACHMENT { get { return 5; } }

		/// <summary> </summary>
		public int BORDERSTYLE_SOLID { get { return 0; } }
		/// <summary> </summary>
		public int BORDERSTYLE_DASHED { get { return 1; } }
		/// <summary> </summary>
		public int BORDERSTYLE_BEVELED { get { return 2; } }
		/// <summary> </summary>
		public int BORDERSTYLE_INSET { get { return 3; } }
		/// <summary> </summary>
		public int BORDERSTYLE_UNDERLINE { get { return 4; } }

		/// <summary> </summary>
		public int BORDEREFFECT_NONE { get { return 0; } }
		/// <summary> </summary>
		public int BORDEREFFECT_CLOUDY { get { return 1; } }

		/// <summary> </summary>
		public int LINEENDINGSTYLE_SQUARE { get { return 0; } }
		/// <summary> </summary>
		public int LINEENDINGSTYLE_CIRCLE { get { return 1; } }
		/// <summary> </summary>
		public int LINEENDINGSTYLE_DIAMOND { get { return 2; } }
		/// <summary> </summary>
		public int LINEENDINGSTYLE_OPENARROW { get { return 3; } }
		/// <summary> </summary>
		public int LINEENDINGSTYLE_CLOSEDARROW { get { return 4; } }
		/// <summary> </summary>
		public int LINEENDINGSTYLE_NONE { get { return 5; } }
		/// <summary> </summary>
		public int LINEENDINGSTYLE_BUTT { get { return 6; } }
		/// <summary> </summary>
		public int LINEENDINGSTYLE_ROPENARROW { get { return 7; } }
		/// <summary> </summary>
		public int LINEENDINGSTYLE_RCLOSEDARROW { get { return 8; } }
		/// <summary> </summary>
		public int LINEENDINGSTYLE_SLASH { get { return 9; } }

		/// <summary> </summary>
		public int TEXTANNOTATIONICON_COMMENT { get { return 0; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_HELP { get { return 1; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_INSERT { get { return 2; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_KEY { get { return 3; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_NEWPARAGRAPH { get { return 4; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_NOTE { get { return 5; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_PARAGRAPH { get { return 6; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_CIRCLE { get { return 7; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_CROSS { get { return 8; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_RIGHTARROW { get { return 9; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_RIGHTPOINTER { get { return 10; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_STAR { get { return 11; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_UPARROW { get { return 12; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_UPLEFTARROW { get { return 13; } }
		/// <summary> </summary>
		public int TEXTANNOTATIONICON_CHECK { get { return 14; } }

		/// <summary> </summary>
		public int BLENDMODE_NORMAL { get { return 0; } }
		/// <summary> </summary>
		public int BLENDMODE_MULTIPLY { get { return 1; } }
		/// <summary> </summary>
		public int BLENDMODE_SCREEN { get { return 2; } }
		/// <summary> </summary>
		public int BLENDMODE_OVERLAY { get { return 3; } }
		/// <summary> </summary>
		public int BLENDMODE_DARKEN { get { return 4; } }
		/// <summary> </summary>
		public int BLENDMODE_LIGHTEN { get { return 5; } }
		/// <summary> </summary>
		public int BLENDMODE_COLORDODGE { get { return 6; } }
		/// <summary> </summary>
		public int BLENDMODE_COLORBURN { get { return 7; } }
		/// <summary> </summary>
		public int BLENDMODE_HARDLIGHT { get { return 8; } }
		/// <summary> </summary>
		public int BLENDMODE_SOFTLIGHT { get { return 9; } }
		/// <summary> </summary>
		public int BLENDMODE_DIFFERENCE { get { return 10; } }
		/// <summary> </summary>
		public int BLENDMODE_EXCLUSION { get { return 11; } }
		/// <summary> </summary>
		public int BLENDMODE_HUE { get { return 12; } }
		/// <summary> </summary>
		public int BLENDMODE_SATURATION { get { return 13; } }
		/// <summary> </summary>
		public int BLENDMODE_COLOR { get { return 14; } }
		/// <summary> </summary>
		public int BLENDMODE_LUMINOSITY { get { return 15; } }

		/// <summary> </summary>
		public int PUSHBUTTONHIGHLIGHTINGMODE_NONE { get { return 0; } }
		/// <summary> </summary>
		public int PUSHBUTTONHIGHLIGHTINGMODE_INVERT { get { return 1; } }
		/// <summary> </summary>
		public int PUSHBUTTONHIGHLIGHTINGMODE_OUTLINE { get { return 2; } }
		/// <summary> </summary>
		public int PUSHBUTTONHIGHLIGHTINGMODE_PUSH { get { return 3; } }
		/// <summary> </summary>
		public int PUSHBUTTONHIGHLIGHTINGMODE_TOGGLE { get { return 4; } }

		/// <summary> </summary>
		public int TEXTALIGN_LEFT { get { return 0; } }
		/// <summary> </summary>
		public int TEXTALIGN_CENTER { get { return 1; } }
		/// <summary> </summary>
		public int TEXTALIGN_RIGHT { get { return 2; } }

		/// <summary> </summary>
		public int HORIZONTALALIGN_LEFT { get { return 0; } }
		/// <summary> </summary>
		public int HORIZONTALALIGN_CENTER { get { return 1; } }
		/// <summary> </summary>
		public int HORIZONTALALIGN_JUSTIFY { get { return 2; } }
		/// <summary> </summary>
		public int HORIZONTALALIGN_RIGHT { get { return 3; } }

		/// <summary> </summary>
		public int VERTICALLALIGN_TOP { get { return 0; } }
		/// <summary> </summary>
		public int VERTICALALIGN_CENTER { get { return 1; } }
		/// <summary> </summary>
		public int VERTICALALIGN_BOTTOM { get { return 2; } }

		/// <summary> </summary>
		public int PAGENUMBERINGSTYLE_DECIMALARABIC { get { return 0; } }
		/// <summary> </summary>
		public int PAGENUMBERINGSTYLE_UPPERCASEROMAN { get { return 1; } }
		/// <summary> </summary>
		public int PAGENUMBERINGSTYLE_LOWERCASEROMAN { get { return 2; } }
		/// <summary> </summary>
		public int PAGENUMBERINGSTYLE_UPPERCASELETTERS { get { return 3; } }
		/// <summary> </summary>
		public int PAGENUMBERINGSTYLE_LOWERCASELETTERS { get { return 4; } }
		/// <summary> </summary>
		public int PAGENUMBERINGSTYLE_NONE { get { return 5; } }

		/// <summary> </summary>
		public int ENCRYPTIONALGORITHM_NONE { get { return 0; } }
		/// <summary> </summary>
		public int ENCRYPTIONALGORITHM_RC4_40BIT { get { return 1; } }
		/// <summary> </summary>
		public int ENCRYPTIONALGORITHM_RC4_128BIT { get { return 2; } }
		/// <summary> </summary>
		public int ENCRYPTIONALGORITHM_AES_128BIT { get { return 3; } }
		/// <summary> </summary>
		public int ENCRYPTIONALGORITHM_AES_256BIT { get { return 4; } }

		/// <summary> </summary>
		public int PRINTQUALITY_HIGHTRESOLUTION { get { return 0; } }
		/// <summary> </summary>
		public int PRINTQUALITY_LOWRESOLUTION { get { return 1; } }

		/// <summary> </summary>
		public int TABLEBORDERSTYLE_SOLID { get { return 0; } }
		/// <summary> </summary>
		public int TABLEBORDERSTYLE_DASHED { get { return 1; } }

		/// <summary> </summary>
		public int TILINGTYPE_CONSTANTSPACING { get { return 1; } }
		/// <summary> </summary>
		public int TILINGTYPE_NODISTORTION { get { return 2; } }
		/// <summary> </summary>
		public int TILINGTYPE_CONSTANTSPACINGANDFASTERTILING { get { return 3; } }

		/// <summary> </summary>
		public int LINECAPTIONTYPE_INLINE { get { return 0; } }
		/// <summary> </summary>
		public int LINECAPTIONTYPE_TOP { get { return 1; } }

		/// <summary> </summary>
		public int SOUNDANNOTATIONICON_SPEAKER { get { return 0; } }
		/// <summary> </summary>
		public int SOUNDANNOTATIONICON_MIC { get { return 1; } }
		/// <summary> </summary>
		public int SOUNDANNOTATIONICON_EAR { get { return 2; } }

		/// <summary> </summary>
		public int LINKANNOTATIONHIGHLIGHTINGMODE_NONE { get { return 0; } }
		/// <summary> </summary>
		public int LINKANNOTATIONHIGHLIGHTINGMODE_INVERT { get { return 1; } }
		/// <summary> </summary>
		public int LINKANNOTATIONHIGHLIGHTINGMODE_OUTLINE { get { return 2; } }
		/// <summary> </summary>
		public int LINKANNOTATIONHIGHLIGHTINGMODE_PUSH { get { return 3; } }

		/// <summary> </summary>
		public int FILLMODE_WINDING { get { return 0; } }
		/// <summary> </summary>
		public int FILLMODE_ALTERNATE { get { return 1; } }

		/// <summary> </summary>
		public int LINECAPSTYLE_BUTT { get { return 0; } }
		/// <summary> </summary>
		public int LINECAPSTYLE_ROUND { get { return 1; } }
		/// <summary> </summary>
		public int LINECAPSTYLE_PROJECTINGSQUARE { get { return 2; } }

		/// <summary> </summary>
		public int LINEJOINSTYLE_MITER { get { return 0; } }
		/// <summary> </summary>
		public int LINEJOINSTYLE_ROUND { get { return 1; } }
		/// <summary> </summary>
		public int LINEJOINSTYLE_BEVEL { get { return 2; } }

		/// <summary> </summary>
		public int ZOOMMODE_FITXYZ { get { return 0; } }
		/// <summary> </summary>
		public int ZOOMMODE_FITPAGE { get { return 1; } }
		/// <summary> </summary>
		public int ZOOMMODE_FITHORIZONTAL { get { return 2; } }
		/// <summary> </summary>
		public int ZOOMMODE_FITVERTICAL { get { return 3; } }
		/// <summary> </summary>
		public int ZOOMMODE_FITRECTANGLE { get { return 4; } }
		/// <summary> </summary>
		public int ZOOMMODE_FITBOUNDING { get { return 5; } }
		/// <summary> </summary>
		public int ZOOMMODE_FITBOUNDINGHORIZONTAL { get { return 6; } }
		/// <summary> </summary>
		public int ZOOMMODE_FITBOUNDINGVERTICAL { get { return 7; } }

		/// <summary> </summary>
		public int DIRECTION_LEFTTORIGHT { get { return 0; } }
		/// <summary> </summary>
		public int DIRECTION_RIGHTTOLEFT { get { return 1; } }

		/// <summary> </summary>
		public int FULLSCREENPAGEMODE_NONE { get { return 0; } }
		/// <summary> </summary>
		public int FULLSCREENPAGEMODE_OUTLINES { get { return 1; } }
		/// <summary> </summary>
		public int FULLSCREENPAGEMODE_THUMBNAIL { get { return 2; } }
		/// <summary> </summary>
		public int FULLSCREENPAGEMODE_OPTIONALCONTENT { get { return 3; } }

		/// <summary> </summary>
		public int PAGELAYOUT_SINGLEPAGE { get { return 0; } }
		/// <summary> </summary>
		public int PAGELAYOUT_ONECOLUMN { get { return 1; } }
		/// <summary> </summary>
		public int PAGELAYOUT_TWOCOLUMNLEFT { get { return 2; } }
		/// <summary> </summary>
		public int PAGELAYOUT_TWOCOLUMNRIGHT { get { return 3; } }
		/// <summary> </summary>
		public int PAGELAYOUT_TWOPAGELEFT { get { return 4; } }
		/// <summary> </summary>
		public int PAGELAYOUT_TWOPAGERIGHT { get { return 5; } }

		/// <summary> </summary>
		public int ROTATIONANGLE_ROTATE0 { get { return 1; } }
		/// <summary> </summary>
		public int ROTATIONANGLE_ROTATE90 { get { return 2; } }
		/// <summary> </summary>
		public int ROTATIONANGLE_ROTATE180 { get { return 3; } }
		/// <summary> </summary>
		public int ROTATIONANGLE_ROTATE270 { get { return 4; } }

		/// <summary> </summary>
		public int COMPRESSION_NONE { get { return 0; } }
		/// <summary> </summary>
		public int COMPRESSION_FLATE { get { return 1; } }

		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_APPROVED { get { return 0; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_ASIS { get { return 1; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_CONFIDENTIAL { get { return 2; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_DEPARTMENTAL { get { return 3; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_DRAFT { get { return 4; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_EXPERIMENTAL { get { return 5; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_EXPIRED { get { return 6; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_FINAL { get { return 8; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_FORCOMMENT { get { return 9; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_FORPUBLICRELEASE { get { return 10; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_NOTAPPROVED { get { return 11; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_NOTFORPUBLICRELEASE { get { return 12; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_SOLD { get { return 13; } }
		/// <summary> </summary>
		public int RUBBERSTAMPANNOTATIONICON_TOPSECRET { get { return 14; } }

	    /// <summary> </summary>
        public int TEXTWATERMARKLOCATION_CUSTOM { get { return 0; } }
	    /// <summary> </summary>
        public int TEXTWATERMARKLOCATION_TOP { get { return 1; } }
	    /// <summary> </summary>
        public int TEXTWATERMARKLOCATION_BOTTOM { get { return 2; } }
	    /// <summary> </summary>
        public int TEXTWATERMARKLOCATION_CENTER { get { return 3; } }
	    /// <summary> </summary>
        public int TEXTWATERMARKLOCATION_VERTICALFROMTOPTOBOTTOM { get { return 4; } }
	    /// <summary> </summary>
        public int TEXTWATERMARKLOCATION_VERTICALFROMBOTTOMTOTOP { get { return 5; } }
	    /// <summary> </summary>
        public int TEXTWATERMARKLOCATION_DIAGONALFROMTOPLEFTTOBOTTOMRIGHT { get { return 6; } }
	    /// <summary> </summary>
        public int TEXTWATERMARKLOCATION_DIAGONALFROMBOTTOMLEFTTOTOPRIGHT { get { return 7; } }
	    /// <summary> </summary>
        public int TEXTWATERMARKLOCATION_TILED { get { return 8; } }


        internal ComHelpers(Document document)
		{
			_document = document;
		}

		/// <summary> </summary>
		public Page CreatePage(PaperFormat paperFormat)
		{
			return new Page(paperFormat);
		}

		/// <summary> </summary>
		public Page CreatePage2(PaperFormat paperFormat, PaperOrientation paperOrientation)
		{
			return new Page(paperFormat, paperOrientation);
		}

		/// <summary>
		/// Creates new document page with specified width and height.
		/// </summary>
		/// <param name="width">Page width.</param>
		/// <param name="height">Page height.</param>
		/// <returns></returns>
		public Page CreatePage3(float width, float height)
		{
			return new Page(width, height);
		}

		/// <summary> </summary>
		public Font CreateSystemFont(string fontName, float emSize)
		{
			return new Font(fontName, emSize);
		}

		/// <summary> </summary>
		public Font CreateSystemFont2(string fontName, float emSize, bool bold, bool italic, bool underline, bool strikeout)
		{
			return new Font(fontName, emSize, bold, italic, underline, strikeout);
		}

		/// <summary> </summary>
		public Font CreateStandardFont(int standardFontId, float emSize)
		{
			return new Font((StandardFonts) standardFontId, emSize);
		}

		/// <summary> </summary>
		public Font CreateStandardFont2(int standardFontId, float emSize, bool underline, bool strikeout)
		{
			return new Font((StandardFonts) standardFontId, emSize, underline, strikeout);
		}

		/// <summary> </summary>
		public Font LoadFontFromFile(string fileName, float emSize)
		{
			return Font.FromFile(fileName, emSize);
		}

		/// <summary> </summary>
		public Font LoadFontFromFile2(string fileName, float emSize, bool underline, bool strikeout)
		{
			return Font.FromFile(fileName, emSize, underline, strikeout);
		}

		/// <summary> </summary>
		public SolidPen CreateSolidPen(Color color, float width)
		{
			return new SolidPen(color, width);
		}

		/// <summary> </summary>
		public SolidBrush CreateSolidBrush(Color color)
		{
			return new SolidBrush(color);
		}

		/// <summary> </summary>
		public ColorRGB CreateColorRGB(byte red, byte green, byte blue)
		{
			return new ColorRGB(red, green, blue);
		}

		/// <summary> </summary>
		public ColorCMYK CreateColorCMYK(byte cyan, byte magenta, byte yellow, byte key)
		{
			return new ColorCMYK(cyan, magenta, yellow, key);
		}

		/// <summary> </summary>
		public ColorICC CreateColorICC(ICCBasedColorspace icc, DeviceColor color)
		{
			return new ColorICC(icc, color);
		}

		/// <summary> </summary>
		public ColorGray CreateColorGray(byte gray)
		{
			return new ColorGray(gray);
		}

		/// <summary> </summary>
		public PushButton CreatePushButton(float left, float top, float width, float height, string name)
		{
			return new PushButton(left, top, width, height, name);
		}

		/// <summary> </summary>
		public EditBox CreateEditBox(float left, float top, float width, float height, string name)
		{
			return new EditBox(left, top, width, height, name);
		}

		/// <summary> </summary>
		public CheckBox CreateCheckBox(float left, float top, float width, float height, string name)
		{
			return new CheckBox(left, top, width, height, name);
		}

		/// <summary> </summary>
		public ComboBox CreateComboBox(float left, float top, float width, float height, string name)
		{
			return new ComboBox(left, top, width, height, name);
		}

		/// <summary> </summary>
		public ListBox CreateListBox(float left, float top, float width, float height, string name)
		{
			return new ListBox(left, top, width, height, name);
		}

		/// <summary> </summary>
		public RadioButton CreateRadioButton(float left, float top, float width, float height, string name, string exportValue)
		{
			return new RadioButton(left, top, width, height, name, exportValue);
		}

		/// <summary> </summary>
		public GoToAction CreateGoToAction(Destination destination)
		{
			return new GoToAction(destination);
		}

		/// <summary> </summary>
		public JavaScriptAction CreateJavaScriptAction(string script)
		{
			return new JavaScriptAction(script);
		}

		/// <summary> </summary>
		public LaunchAction CreateLaunchAction(string fileName)
		{
			return new LaunchAction(fileName);
		}

		/// <summary> </summary>
		public HideAction CreateHideAction(bool hide)
		{
			return new HideAction(hide);
		}

		/// <summary> </summary>
		public SubmitFormAction CreateSubmitFormAction(string uri)
		{
			return new SubmitFormAction(new Uri(uri));
		}

		/// <summary> </summary>
		public ResetFormAction CreateResetFormAction()
		{
			return new ResetFormAction();
		}

		/// <summary> </summary>
		public URIAction CreateURIAction(string uri)
		{
			return new URIAction(new Uri(uri));
		}

		/// <summary> </summary>
		public ThreeDAnnotation CreateThreeDAnnotation(ThreeDData data, float left, float top, float width, float height)
		{
			return new ThreeDAnnotation(data, left, top, width, height);
		}

		/// <summary> </summary>
		public FileAttachmentAnnotation CreateFileAttachmentAnnotation(string fileName, float left, float top, float width, float height)
		{
			return new FileAttachmentAnnotation(fileName, left, top, width, height);
		}

		/// <summary> </summary>
		public SquareAnnotation CreateSquareAnnotation(float left, float top, float width, float height)
		{
			return new SquareAnnotation(left, top, width, height);
		}

		/// <summary> </summary>
		public CircleAnnotation CreateCircleAnnotation(float left, float top, float width, float height)
		{
			return new CircleAnnotation(left, top, width, height);
		}

		/// <summary> </summary>
		public LineAnnotation CreateLineAnnotation(float x1, float y1, float x2, float y2)
		{
			return new LineAnnotation(x1, y1, x2, y2);
		}

		/// <summary> </summary>
		public PolylineAnnotation CreatePolylineAnnotation(PointsArray pointsArray)
		{
			return new PolylineAnnotation(pointsArray);
		}

		/// <summary> </summary>
		public SoundAnnotation CreateSoundAnnotation(Sound sound, float left, float top, float width, float height)
		{
			return new SoundAnnotation(sound, left, top, width, height);
		}

		/// <summary> </summary>
		public TextAnnotation CreateTextAnnotation(float left, float top)
		{
			return new TextAnnotation(left, top);
		}

		/// <summary> </summary>
		public MovieAnnotation CreateMovieAnnotation(Movie movie, float left, float top, float width, float height)
		{
			return new MovieAnnotation(movie, left, top, width, height);
		}

		/// <summary> </summary>
		public RubberStampAnnotation CreateRubberStampAnnotation(float left, float top, float width, float height)
		{
			return new RubberStampAnnotation(left, top, width, height);
		}

		/// <summary> </summary>
		public Destination CreateDestination(Page page, float top)
		{
			return new Destination(page, top);
		}

		/// <summary> </summary>
		public ThreeDData CreateThreeDData(string filename)
		{
			return new ThreeDData(filename);
		}

		/// <summary> </summary>
		public DashPattern CreateDashPattern(object[] array, float dashPhase)
		{
			float[] floats = new float[array.Length];

			for (int i = 0; i < array.Length; i++)
				floats[i] = Convert.ToSingle(array[i]);

			return new DashPattern(floats, dashPhase);
		}

		/// <summary> </summary>
		public PointsArray CreatePointsArray(object[] array)
		{
			PointsArray result = new PointsArray();

			for (int i = 0; i < array.Length; i++)
			{
				object[] point = array[i] as object[];
				if (point != null && point.Length == 2)
					result.AddPoint(new PointF(Convert.ToSingle(point[0]), Convert.ToSingle(point[1])));
			}

			return result;
		}

		/// <summary> </summary>
		public Sound CreateSound(string filename)
		{
			return new Sound(filename);
		}

		/// <summary> </summary>
		public Movie CreateMovie(string filename)
		{
			return new Movie(filename);
		}

		/// <summary> </summary>
		public Path CreatePath()
		{
			return new Path();
		}

		/// <summary> </summary>
		public ICCBasedColorspace CreateICCBasedColorspace(string fileName)
		{
			return new ICCBasedColorspace(fileName);
		}

		/// <summary> </summary>
		public GraphicsTemplate CreateGraphicsTemplate(float width, float height)
		{
			return new GraphicsTemplate(width, height);
		}

		/// <summary> </summary>
		public UncoloredTilingBrush CreateUncoloredTilingBrush(float width, float height)
		{
			return new UncoloredTilingBrush(width, height);
		}

		/// <summary> </summary>
		public ColoredTilingBrush CreateColoredTilingBrush(float width, float height)
		{
			return new ColoredTilingBrush(width, height);
		}

		/// <summary> </summary>
		public Image CreateImage(string fileName)
		{
			return new Image(fileName);
		}
        
        /// <summary> </summary>
		public Image CreateImage2(byte[] imageBytes)
		{
			return new Image(new MemoryStream(imageBytes));
		}
        
        /// <summary> </summary>
		public Image CreateImage3(System.Drawing.Image image)
		{
			return new Image(image);
		}

		/// <summary> </summary>
		public Layer CreateLayer(string name)
		{
			return new Layer(name);
		}

		/// <summary> </summary>
		public OptionalContentGroup CreateOptionalContentGroup()
		{
			return new OptionalContentGroup();
		}

		/// <summary> </summary>
		public OptionalContentGroupLayer CreateOptionalContentGroupLayer(Layer layer)
		{
			return new OptionalContentGroupLayer(layer);
		}

		/// <summary> </summary>
		public Outline CreateOutline(string title)
		{
			return new Outline(title);
		}

		/// <summary> </summary>
		public Outline CreateOutline(string title, Destination destination)
		{
			return new Outline(title, destination);
		}

		/// <summary> </summary>
		public Outline CreateOutline(string title, Action action)
		{
			return new Outline(title, action);
		}

		/// <summary> </summary>
		public LinkAnnotation CreateLinkAnnotation(Destination destination, float left, float top, float width, float height)
		{
			return new LinkAnnotation(destination, left, top, width, height);
		}

		/// <summary> </summary>
		public LinkAnnotation CreateLinkAnnotation2(Action action, float left, float top, float width, float height)
		{
			return new LinkAnnotation(action, left, top, width, height);
		}

		/// <summary> </summary>
		public StringFormat CreateStringFormat()
		{
			return new StringFormat();
		}

		/// <summary> </summary>
		public PageLabel CreatePageLabel(int firstPageIndex, PageNumberingStyle style)
		{
			return new PageLabel(firstPageIndex, style);
		}

		/// <summary> </summary>
		public Table CreateTable()
		{
			return new Table();
		}

		/// <summary> </summary>
		public TableColumn CreateTableColumn(string columnName, string columnCaption)
		{
			return new TableColumn(columnName, columnCaption);
		}

		/// <summary> </summary>
		public TextWatermark CreateTextWatermark(string watermarkText)
		{
			return new TextWatermark(watermarkText);
		}

		/// <summary> </summary>
		public float GetPointX(PointF point)
		{
			return point.X;
		}

		/// <summary> </summary>
		public float GetPointY(PointF point)
		{
			return point.Y;
		}

		/// <summary> </summary>
		public float GetSizeWidth(SizeF size)
		{
			return size.Width;
		}

		/// <summary> </summary>
		public float GetSizeHeight(SizeF size)
		{
			return size.Height;
		}
	}
}
