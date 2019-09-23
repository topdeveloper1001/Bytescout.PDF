using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
    internal class Uniscribe
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_CONTROL
        {
            private uint data;

//             public uint uDefaultLanguage : 16;
            public int uDefaultLanguage     { get { return (int)(data & 0x0000FFFF) >> 0; } }
//             public uint fContextDigits : 1;
            public bool fContextDigits      { get { return (data & 0x00010000) != 0; } }
//             public uint fInvertPreBoundDir : 1;
            public bool fInvertPreBoundDir  { get { return (data & 0x00020000) != 0; } }
//             public uint fInvertPostBoundDir : 1;
            public bool fInvertPostBoundDir { get { return (data & 0x00040000) != 0; } }
//             public uint fLinkStringBefore : 1;
            public bool fLinkStringBefore   { get { return (data & 0x00080000) != 0; } }
//             public uint fLinkStringAfter : 1;
            public bool fLinkStringAfter    { get { return (data & 0x00100000) != 0; } }
//             public uint fNeutralOverride : 1;
            public bool fNeutralOverride    { get { return (data & 0x00200000) != 0; } }
//             public uint fNumericOverride : 1;
            public bool fNumericOverride    { get { return (data & 0x00400000) != 0; } }
//             public uint fLegacyBidiClass : 1;
            public bool fLegacyBidiClass    { get { return (data & 0x00800000) != 0; } }
//             public uint fMergeNeutralItems : 1;
            public bool fMergeNeutralItems  { get { return (data & 0x01000000) != 0; } }
//             public uint fReserved : 7;
//             public int fReserved         { get { } }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_STATE
        {
            public ushort Data;

//             public ushort uBidiLevel : 5;
            public byte uBidiLevel           { get { return (byte)((Data & 0x001F) >> 0); } 
                                               set { Data = (ushort)(Data - uBidiLevel + value); } }
//             public ushort fOverrideDirection : 1;
            public bool fOverrideDirection  { get { return (Data & 0x0020) != 0; } }
//             public ushort fInhibitSymSwap : 1;
            public bool fInhibitSymSwap     { get { return (Data & 0x0040) != 0; } }
//             public ushort fCharShape : 1;
            public bool fCharShape          { get { return (Data & 0x0080) != 0; }
                set 
                {
//                     if (value == fCharShape) 
//                         return;
// 
//                     // Not implemented (see MSDN)
//                     Data = (value) ? (ushort)(Data + 0x0080) : (ushort)(Data - 0x0080);
                }
            }
//             public ushort fDigitSubstitute : 1;
            public bool fDigitSubstitute    { get { return (Data & 0x0100) != 0; } }
//             public ushort fInhibitLigate : 1;
            public bool fInhibitLigate      { get { return (Data & 0x0200) != 0; } }
//             public ushort fDisplayZWG : 1;
            public bool fDisplayZWG         { get { return (Data & 0x0400) != 0; } }
//             public ushort fArabicNumContext : 1;
            public bool fArabicNumContext   { get { return (Data & 0x0800) != 0; } }
//             public ushort fGcpClusters : 1;
            public bool fGcpClusters        { get { return (Data & 0x1000) != 0; } }
//             public ushort fReserved : 1;
//             public bool fReserved        { get { } }
//             public ushort fEngineReserved : 2;
//             public int fEngineReserved   { get { } }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_ITEM
        {
            public int iCharPos;
            private ushort aData1, aData2; // corresponds to SCRIPT_ITEM member "SCRIPT_ANALYSIS a;"

            public SCRIPT_ANALYSIS a 
            { 
                get
                {
	                SCRIPT_STATE aS = new SCRIPT_STATE();
					aS.Data = aData2;

	                SCRIPT_ANALYSIS result = new SCRIPT_ANALYSIS();
					result.Data = aData1;
	                result.s = aS;

	                return result; 
                } 
                set 
                { 
                    aData1 = value.Data;
                    aData2 = value.s.Data;
                } 
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_ANALYSIS
        {
            public ushort Data;

//             public ushort eScript : 10;
            public int eScript          { get { return (Data & 0x03FF) >> 0; } 
                                          set { Data = (ushort)(Data - eScript + value); } }
//             public ushort fRTL : 1;
            public bool fRTL            { get { return (Data & 0x0400) != 0; } }
//             public ushort fLayoutRTL : 1;
            public bool fLayoutRTL      { get { return (Data & 0x0800) != 0; } }
//             public ushort fLinkBefore : 1;
            public bool fLinkBefore     { get { return (Data & 0x1000) != 0; } }
//             public ushort fLinkAfter : 1;
            public bool fLinkAfter      { get { return (Data & 0x2000) != 0; } }
//             public ushort fLogicalOrder : 1;
            public bool fLogicalOrder   { get { return (Data & 0x4000) != 0; } 
                set 
                { 
                    if (value == fLogicalOrder)
                        return;

                    Data = (value) ? (ushort)(Data + 0x4000) : (ushort)(Data - 0x4000);
                } 
            }
//             public ushort fNoGlyphIndex : 1;
            public bool fNoGlyphIndex   { get { return (Data & 0x8000) != 0; } }

            private ushort sdata; // corresponds to SCRIPT_STATE member "SCRIPT_STATE s;"
            public SCRIPT_STATE s
            {
	            get
	            {
		            SCRIPT_STATE result = new SCRIPT_STATE();
		            result.Data = sdata;
		            
					return result;
	            }
                set { sdata = value.Data; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_FONTPROPERTIES
        {
            public int cBytes;
            public ushort wgBlank;
            public ushort wgDefault;
            public ushort wgInvalid;
            public ushort wgKashida;
            public int iKashidaWidth;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_VISATTR
        {
            private ushort data;

//             public ushort uJustification  :4;
            public SCRIPT_JUSTIFY uJustification    { get { return (SCRIPT_JUSTIFY)((data & 0x000F) >> 0); } }
//             public ushort fClusterStart  :1;
            public bool fClusterStart               { get { return (data & 0x0010) != 0; } }
//             public ushort fDiacritic  :1;
            public bool fDiacritic                  { get { return (data & 0x0020) != 0; } }
//             public ushort fZeroWidth  :1;
            public bool fZeroWidth                  { get { return (data & 0x0040) != 0; } }
//             public ushort fReserved  :1;
//             public bool fReserved                { get { } }
//             public ushort fShapeReserved  :8;
//             public int fShapeReserved            { get { } }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GOFFSET 
        // Contains the x and y offsets of the combining glyph.
        { 
            public int DU; // x offset, in logical units, for the combining glyph.
            public int DV; // y offset, in logical units, for the combining glyph.

            public int DX { get { return DU; } set { DU = value; } }
            public int DY { get { return DV; } set { DV = value; } }

            public override string ToString()
            {
                return DU + "," + DV;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ABC
        // The ABC structure contains the width of a character in a TrueType font.
        {
            public int  abcA;   // The A spacing of the character. 
                                // The A spacing is the distance to add to the current position before drawing the character glyph.
            public uint abcB;   // The B spacing of the character. 
                                // The B spacing is the width of the drawn portion of the character glyph.
            public int abcC;    // The C spacing of the character. 
                                // The C spacing is the distance to add to the current position to provide white space to the right of the character glyph.

            public override string ToString()
            {
                return abcA + "," + abcB + "," + abcC;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_LOGATTR
        {
            private byte data;

//             public byte fSoftBreak  :1;
            public bool fSoftBreak      { get { return (data & 0x01) != 0; } }
//             public byte fWhiteSpace  :1;
            public bool fWhiteSpace     { get { return (data & 0x02) != 0; } }
//             public byte fCharStop  :1;
            public bool fCharStop       { get { return (data & 0x04) != 0; } }
//             public byte fWordStop  :1;
            public bool fWordStop       { get { return (data & 0x08) != 0; } }
//             public byte fInvalid  :1;
            public bool fInvalid        { get { return (data & 0x10) != 0; } }
            //             public byte fReserved  :3;
//             public byte fReserved    { get { } }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_PROPERTIES
        {
            private uint data1;
            private uint data2;

//             public uint langid  :16;
            public int langid                   { get { return (int)(data1 & 0x0000FFFF) >> 0; } }
//             public uint fNumeric  :1;
            public bool fNumeric                { get { return (data1 & 0x00010000) != 0; } }
//             public uint fComplex  :1;
            public bool fComplex                { get { return (data1 & 0x00020000) != 0; } }
//             public uint fNeedsWordBreaking  :1;
            public bool fNeedsWordBreaking      { get { return (data1 & 0x00040000) != 0; } }
//             public uint fNeedsCaretInfo  :1;
            public bool fNeedsCaretInfo         { get { return (data1 & 0x00080000) != 0; } }
//             public uint bCharSet  :8;
            public int bCharSet                 { get { return (int)(data1 & 0x0FF00000) >> 0; } }
//             public uint fControl  :1;
            public bool fControl                { get { return (data1 & 0x10000000) != 0; } }
//             public uint fPrivateUseArea  :1;
            public bool fPrivateUseArea         { get { return (data1 & 0x20000000) != 0; } }
//             public uint fNeedsCharacterJustify  :1;
            public bool fNeedsCharacterJustify  { get { return (data1 & 0x40000000) != 0; } }
//             public uint fInvalidGlyph  :1;
            public bool fInvalidGlyph           { get { return (data1 & 0x80000000) != 0; } }

//             public uint fInvalidLogAttr  :1;
            public bool fInvalidLogAttr     { get { return (data2 & 0x01) != 0; } }
//             public uint fCDM  :1;
            public bool fCDM                { get { return (data2 & 0x02) != 0; } }
//             public uint fAmbiguousCharSet  :1;
            public bool fAmbiguousCharSet   { get { return (data2 & 0x04) != 0; } }
//             public uint fClusterSizeVaries  :1;
            public bool fClusterSizeVaries  { get { return (data2 & 0x08) != 0; } }
//             public uint fRejectInvalid  :1;
            public bool fRejectInvalid      { get { return (data2 & 0x10) != 0; } }

        }

        public enum SCRIPT_JUSTIFY
        {
            SCRIPT_JUSTIFY_NONE = 0,            // Justification cannot be applied at the glyph.
            SCRIPT_JUSTIFY_ARABIC_BLANK = 1,    // The glyph represents a blank in an Arabic run.
            SCRIPT_JUSTIFY_CHARACTER = 2,       // An inter-character justification point follows the glyph.
            SCRIPT_JUSTIFY_RESERVED1 = 3,       // Reserved.
            SCRIPT_JUSTIFY_BLANK = 4,           // The glyph represents a blank outside an Arabic run.
            SCRIPT_JUSTIFY_RESERVED2 = 5,       // Reserved.
            SCRIPT_JUSTIFY_RESERVED3 = 6,       // Reserved.
            SCRIPT_JUSTIFY_ARABIC_NORMAL = 7,   // Normal middle-of-word glyph that connects to the right (begin).
            SCRIPT_JUSTIFY_ARABIC_KASHIDA = 8,  // Kashida (U+0640) in the middle of the word.
            SCRIPT_JUSTIFY_ARABIC_ALEF = 9,     // Final form of an alef-like (U+0627, U+0625, U+0623, U+0622).
            SCRIPT_JUSTIFY_ARABIC_HA = 10,      // Final form of Ha (U+0647).
            SCRIPT_JUSTIFY_ARABIC_RA = 11,      // Final form of Ra (U+0631).
            SCRIPT_JUSTIFY_ARABIC_BA = 12,      // Final form of Ba (U+0628).
            SCRIPT_JUSTIFY_ARABIC_BARA = 13,    // Ligature of alike (U+0628,U+0631).
            SCRIPT_JUSTIFY_ARABIC_SEEN = 14,    // Highest priority: initial shape of Seen class (U+0633).
            SCRIPT_JUSTIFY_ARABIC_SEEN_M = 15   // Highest priority: medial shape of Seen class (U+0633).
        }

        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptItemize(
            [MarshalAs(UnmanagedType.LPWStr)] string pwcInChars,
            int cInChars,
            int cMaxItems,
            ref SCRIPT_CONTROL psControl,
            ref SCRIPT_STATE psState,
            [In ,Out] SCRIPT_ITEM[] pItems,
            ref int pcItems
            );
        
        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptLayout(
            int cRuns,
            byte[] pbLevel,
            [In, Out] int[] piVisualToLogical,
            [In, Out] int[] piLogicalToVisual
            );

        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptGetFontProperties(
            IntPtr hdc, 
            ref IntPtr psc,
            [In, Out, MarshalAs(UnmanagedType.Struct)] ref SCRIPT_FONTPROPERTIES sfp
            );

        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptGetCMap(
            IntPtr hdc, 
            ref IntPtr psc,
            [MarshalAs(UnmanagedType.LPWStr)] string pwcInChars,
            int cChars,
            uint dwFlags,
            [In, Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwOutGlyphs // Each code point maps to a single glyph.
            );

        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptShape(
            IntPtr hdc, 
            ref IntPtr psc,
            [MarshalAs(UnmanagedType.LPWStr)] string pwcChars,
            int cChars,
            int cMaxGlyphs,
            ref SCRIPT_ANALYSIS psa,
            [In, Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwOutGlyphs,
            [In, Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwLogClust,
            [In, Out, MarshalAs(UnmanagedType.LPArray)] SCRIPT_VISATTR[] psva,
            ref int pcGlyphs
            );

        public enum SCRIPT_IS_COMPLEX_FLAG
        {
            SIC_COMPLEX = 1,    // Treat complex script letters as complex
            SIC_ASCIIDIGIT = 2, // Treat digits U+0030 through U+0039 as complex
            SIC_NEUTRAL = 4     // Treat neutrals as complex
        }

        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptIsComplex(
            [MarshalAs(UnmanagedType.LPWStr)] string pwcInChars,
            int cInChars,
            [MarshalAs(UnmanagedType.U4)] uint dwFlags
            );
        
        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptGetProperties(
            ref IntPtr ppSp,
            ref int piNumScripts
            );

        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptPlace(
            IntPtr hdc, 
            ref IntPtr psc, 
            [MarshalAs(UnmanagedType.LPArray)] ushort[] pwGlyphs, 
            int cGlyphs, 
            [MarshalAs(UnmanagedType.LPArray)] SCRIPT_VISATTR[] psva, 
            ref SCRIPT_ANALYSIS psa, 
            [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] piAdvance, 
            [In, Out, MarshalAs(UnmanagedType.LPArray)] GOFFSET[] pGoffset, 
            ref ABC pABC
            );

        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptJustify(
            [MarshalAs(UnmanagedType.LPArray)] SCRIPT_VISATTR[] psva, 
            [MarshalAs(UnmanagedType.LPArray)] int[] piAdvance, 
            int cGlyphs, 
            int iDx, 
            int iMinKashida, 
            [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] piJustify
            );

        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptBreak(
            [MarshalAs(UnmanagedType.LPWStr)] string pwcChars, 
            int cChars, 
            SCRIPT_ANALYSIS psa,
            [In, Out, MarshalAs(UnmanagedType.LPArray)] SCRIPT_LOGATTR[] psla
            );

        [DllImport("usp10.dll", CharSet = CharSet.Unicode)]
        public static extern uint ScriptTextOut(
            IntPtr hdc, 
            ref IntPtr psc, 
            int x, 
            int y, 
            uint fuOptions,
//            Win32.RECT lprc, 
//             IntPtr lprc,
           OptionalBase lprc,
            SCRIPT_ANALYSIS psa,
            IntPtr pwcReserved, 
            int iReserved,
            [MarshalAs(UnmanagedType.LPArray)] ushort[] pwGlyphs,
            int cGlyphs,
            [MarshalAs(UnmanagedType.LPArray)] int[] piAdvance,
            [MarshalAs(UnmanagedType.LPArray)] int[] piJustify,
            [MarshalAs(UnmanagedType.LPArray)] GOFFSET[] pGoffset
            );

    }

    [StructLayout(LayoutKind.Sequential)]
    internal class OptionalBase { }

//     [StructLayout(LayoutKind.Sequential)]
//     public class Optional<T> : OptionalBase where T : struct
//     {
//         public T Value;
//     }
// 
//     public static class OptionalExtensionMethods
//     {
//         public static OptionalBase AsOptional<T>(this T? value) where T : struct
//         {
//             return (value != null && value.HasValue)
//                 ? new Optional<T>() { Value = value.Value }
//                 : null
//                 ;
//         }
//     }
}
