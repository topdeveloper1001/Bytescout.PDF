using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    /*
    * Shape arabic characters. This code was inspired by an LGPL'ed C library:
    * Pango ( see http://www.pango.com/ ). Note that the code of this is the
    * original work of Paulo Soares. Hence it is perfectly justifiable to distribute
    * it under the MPL.
    *
    * @author Paulo Soares
    */

    internal class ArabicLigaturizer
    {
	    #region Constants for digit processing

	    /**
        * Digit shaping option: Replace European digits (U+0030...U+0039) by Arabic-Indic digits.
        */
	    public const int DIGITS_EN2AN = 0x20;

	    /**
        * Digit shaping option: Replace Arabic-Indic digits by European digits (U+0030...U+0039).
        */
	    public const int DIGITS_AN2EN = 0x40;

	    /**
        * Digit shaping option:
        * Replace European digits (U+0030...U+0039) by Arabic-Indic digits
        * if the most recent strongly directional character
        * is an Arabic letter (its Bidi direction value is RIGHT_TO_LEFT_ARABIC).
        * The initial state at the start of the text is assumed to be not an Arabic,
        * letter, so European digits at the start of the text will not change.
        * Compare to DIGITS_ALEN2AN_INIT_AL.
        */
	    public const int DIGITS_EN2AN_INIT_LR = 0x60;

	    /**
        * Digit shaping option:
        * Replace European digits (U+0030...U+0039) by Arabic-Indic digits
        * if the most recent strongly directional character
        * is an Arabic letter (its Bidi direction value is RIGHT_TO_LEFT_ARABIC).
        * The initial state at the start of the text is assumed to be an Arabic,
        * letter, so European digits at the start of the text will change.
        * Compare to DIGITS_ALEN2AN_INT_LR.
        */
	    public const int DIGITS_EN2AN_INIT_AL = 0x80;

	    /** Not a valid option value. */
	    private const int DIGITS_RESERVED = 0xa0;

	    /**
        * Bit mask for digit shaping options.
        */
	    public const int DIGITS_MASK = 0xe0;

	    /**
        * Digit type option: Use Arabic-Indic digits (U+0660...U+0669).
        */
	    public const int DIGIT_TYPE_AN = 0;

	    /**
        * Digit type option: Use Eastern (Extended) Arabic-Indic digits (U+06f0...U+06f9).
        */
	    public const int DIGIT_TYPE_AN_EXTENDED = 0x100;

	    /**
        * Bit mask for digit type options.
        */
	    public const int DIGIT_TYPE_MASK = '\u0100'; // '\u3f00'?

	    #endregion

	    #region Something with level

	    public const int ar_nothing = 0x0;
	    public const int ar_novowel = 0x1;
	    public const int ar_composedtaskheel = 0x4;
	    public const int ar_lig = 0x8;
        
	    #endregion

	    private const char ZWJ = '\u200D';

	    private const char ALEF = '\u0627';
	    private const char ALEF_MADDA_ABOVE = '\u0622';
	    private const char ALEF_HAMZA_ABOVE = '\u0623';
	    private const char ALEF_HAMZA_BELOW = '\u0625';

	    private const char HAMZA = '\u0621';
	    private const char WAW_HAMZA_ABOVE = '\u0624';
	    private const char YEH_HAMZA_ABOVE = '\u0626';

	    // U+0640 is "Arabic Tatweel" or Kashida
	    public const char Kashida = TATWEEL;
	    private const char TATWEEL = '\u0640';
	    private const char LAM = '\u0644';
	    private const char WAW = '\u0648';
	    private const char ALEF_MAKSURA = '\u0649';
	    private const char YEH = '\u064A';

	    #region POINTS
        
	    private const char FATHA = '\u064E';
	    private const char DAMMA = '\u064F';
	    private const char KASRA = '\u0650';
	    private const char SHADDA = '\u0651';

	    #endregion

		//Format characters are characters that do not have a visible appearance, 
		//but may have an effect on the appearance or behavior of neighboring characters. 
		//For example, U+200C ZERO WIDTH NON-JOINER and 
		//U+200D ZERO WIDTH JOINER may be used to change the default shaping behavior of adjacent characters 
		//(e.g. to inhibit ligatures or request ligature formation). There are 142 format characters in Unicode 5.2.

	    private const char MADDA_ABOVE = '\u0653';
	    private const char HAMZA_ABOVE = '\u0654';
	    private const char HAMZA_BELOW = '\u0655';

	    private const char FARSI_YEH = '\u06CC';

	    private const char LAM_ALEF_MADDA_ABOVE = '\uFEF5';
	    private const char LAM_ALEF_HAMZA_ABOVE = '\uFEF7';
	    private const char LAM_ALEF_HAMZA_BELOW = '\uFEF9';
	    private const char LAM_ALEF = '\uFEFB';

	    //table Unicode chars for letter (general, isolated, end, beginning, middle)
	    internal static char[][] CharTable = {
		    /*Arabic letter form*/
		    new char[]{'\u0621', '\uFE80'}, /* HAMZA */
		    new char[]{'\u0622', '\uFE81', '\uFE82'}, /* ALEF WITH MADDA ABOVE */
		    new char[]{'\u0623', '\uFE83', '\uFE84'}, /* ALEF WITH HAMZA ABOVE */
		    new char[]{'\u0624', '\uFE85', '\uFE86'}, /* WAW WITH HAMZA ABOVE */
		    new char[]{'\u0625', '\uFE87', '\uFE88'}, /* ALEF WITH HAMZA BELOW */
		    new char[]{'\u0626', '\uFE89', '\uFE8A', '\uFE8B', '\uFE8C'}, /* YEH WITH HAMZA ABOVE */
		    new char[]{'\u0627', '\uFE8D', '\uFE8E'}, /* ALEF */
		    new char[]{'\u0628', '\uFE8F', '\uFE90', '\uFE91', '\uFE92'}, /* BEH */
		    new char[]{'\u0629', '\uFE93', '\uFE94'}, /* TEH MARBUTA */
		    new char[]{'\u062A', '\uFE95', '\uFE96', '\uFE97', '\uFE98'}, /* TEH */
		    new char[]{'\u062B', '\uFE99', '\uFE9A', '\uFE9B', '\uFE9C'}, /* THEH */
		    new char[]{'\u062C', '\uFE9D', '\uFE9E', '\uFE9F', '\uFEA0'}, /* JEEM */
		    new char[]{'\u062D', '\uFEA1', '\uFEA2', '\uFEA3', '\uFEA4'}, /* HAH */
		    new char[]{'\u062E', '\uFEA5', '\uFEA6', '\uFEA7', '\uFEA8'}, /* KHAH */
		    new char[]{'\u062F', '\uFEA9', '\uFEAA'}, /* DAL */
		    new char[]{'\u0630', '\uFEAB', '\uFEAC'}, /* THAL */
		    new char[]{'\u0631', '\uFEAD', '\uFEAE'}, /* REH */
		    new char[]{'\u0632', '\uFEAF', '\uFEB0'}, /* ZAIN */
		    new char[]{'\u0633', '\uFEB1', '\uFEB2', '\uFEB3', '\uFEB4'}, /* SEEN */
		    new char[]{'\u0634', '\uFEB5', '\uFEB6', '\uFEB7', '\uFEB8'}, /* SHEEN */
		    new char[]{'\u0635', '\uFEB9', '\uFEBA', '\uFEBB', '\uFEBC'}, /* SAD */
		    new char[]{'\u0636', '\uFEBD', '\uFEBE', '\uFEBF', '\uFEC0'}, /* DAD */
		    new char[]{'\u0637', '\uFEC1', '\uFEC2', '\uFEC3', '\uFEC4'}, /* TAH */
		    new char[]{'\u0638', '\uFEC5', '\uFEC6', '\uFEC7', '\uFEC8'}, /* ZAH */
		    new char[]{'\u0639', '\uFEC9', '\uFECA', '\uFECB', '\uFECC'}, /* AIN */
		    new char[]{'\u063A', '\uFECD', '\uFECE', '\uFECF', '\uFED0'}, /* GHAIN */
		    new char[]{'\u0640', '\u0640', '\u0640', '\u0640', '\u0640'}, /* TATWEEL */
		    new char[]{'\u0641', '\uFED1', '\uFED2', '\uFED3', '\uFED4'}, /* FEH */
		    new char[]{'\u0642', '\uFED5', '\uFED6', '\uFED7', '\uFED8'}, /* QAF */
		    new char[]{'\u0643', '\uFED9', '\uFEDA', '\uFEDB', '\uFEDC'}, /* KAF */
		    new char[]{'\u0644', '\uFEDD', '\uFEDE', '\uFEDF', '\uFEE0'}, /* LAM */
		    new char[]{'\u0645', '\uFEE1', '\uFEE2', '\uFEE3', '\uFEE4'}, /* MEEM */
		    new char[]{'\u0646', '\uFEE5', '\uFEE6', '\uFEE7', '\uFEE8'}, /* NOON */
		    new char[]{'\u0647', '\uFEE9', '\uFEEA', '\uFEEB', '\uFEEC'}, /* HEH */
		    new char[]{'\u0648', '\uFEED', '\uFEEE'}, /* WAW */
		    new char[]{'\u0649', '\uFEEF', '\uFEF0', '\uFBE8', '\uFBE9'}, /* ALEF MAKSURA */
		    new char[]{'\u064A', '\uFEF1', '\uFEF2', '\uFEF3', '\uFEF4'}, /* YEH */
            
		    /*for Persian, Urdu, Sindhi, etc.*/
		    new char[]{'\u0671', '\uFB50', '\uFB51'}, /* ALEF WASLA */
		    new char[]{'\u0677', '\uFBDD'}, /* U WITH HAMZA ABOVE */
		    new char[]{'\u0679', '\uFB66', '\uFB67', '\uFB68', '\uFB69'}, /* TTEH */
		    new char[]{'\u067A', '\uFB5E', '\uFB5F', '\uFB60', '\uFB61'}, /* TTEHEH */
		    new char[]{'\u067B', '\uFB52', '\uFB53', '\uFB54', '\uFB55'}, /* BEEH */
		    new char[]{'\u067E', '\uFB56', '\uFB57', '\uFB58', '\uFB59'}, /* PEH */
		    new char[]{'\u067F', '\uFB62', '\uFB63', '\uFB64', '\uFB65'}, /* TEHEH */
		    new char[]{'\u0680', '\uFB5A', '\uFB5B', '\uFB5C', '\uFB5D'}, /* BEHEH */
		    new char[]{'\u0683', '\uFB76', '\uFB77', '\uFB78', '\uFB79'}, /* NYEH */
		    new char[]{'\u0684', '\uFB72', '\uFB73', '\uFB74', '\uFB75'}, /* DYEH */
		    new char[]{'\u0686', '\uFB7A', '\uFB7B', '\uFB7C', '\uFB7D'}, /* TCHEH */
		    new char[]{'\u0687', '\uFB7E', '\uFB7F', '\uFB80', '\uFB81'}, /* TCHEHEH */
		    new char[]{'\u0688', '\uFB88', '\uFB89'}, /* DDAL */
		    new char[]{'\u068C', '\uFB84', '\uFB85'}, /* DAHAL */
		    new char[]{'\u068D', '\uFB82', '\uFB83'}, /* DDAHAL */
		    new char[]{'\u068E', '\uFB86', '\uFB87'}, /* DUL */
		    new char[]{'\u0691', '\uFB8C', '\uFB8D'}, /* RREH */
		    new char[]{'\u0698', '\uFB8A', '\uFB8B'}, /* JEH */
		    new char[]{'\u06A4', '\uFB6A', '\uFB6B', '\uFB6C', '\uFB6D'}, /* VEH */
		    new char[]{'\u06A6', '\uFB6E', '\uFB6F', '\uFB70', '\uFB71'}, /* PEHEH */
		    new char[]{'\u06A9', '\uFB8E', '\uFB8F', '\uFB90', '\uFB91'}, /* KEHEH */

		    /*for Central Asian*/
		    new char[]{'\u06AD', '\uFBD3', '\uFBD4', '\uFBD5', '\uFBD6'}, /* NG */ 
		    new char[]{'\u06AF', '\uFB92', '\uFB93', '\uFB94', '\uFB95'}, /* GAF */
		    new char[]{'\u06B1', '\uFB9A', '\uFB9B', '\uFB9C', '\uFB9D'}, /* NGOEH */
		    new char[]{'\u06B3', '\uFB96', '\uFB97', '\uFB98', '\uFB99'}, /* GUEH */
		    new char[]{'\u06BA', '\uFB9E', '\uFB9F'}, /* NOON GHUNNA */
		    new char[]{'\u06BB', '\uFBA0', '\uFBA1', '\uFBA2', '\uFBA3'}, /* RNOON */
		    new char[]{'\u06BE', '\uFBAA', '\uFBAB', '\uFBAC', '\uFBAD'}, /* HEH DOACHASHMEE */
		    new char[]{'\u06C0', '\uFBA4', '\uFBA5'}, /* HEH WITH YEH ABOVE */
		    new char[]{'\u06C1', '\uFBA6', '\uFBA7', '\uFBA8', '\uFBA9'}, /* HEH GOAL */

		    /*for Central Asian*/
		    new char[]{'\u06C5', '\uFBE0', '\uFBE1'}, /* KIRGHIZ OE */
		    new char[]{'\u06C6', '\uFBD9', '\uFBDA'}, /* OE */
		    new char[]{'\u06C7', '\uFBD7', '\uFBD8'}, /* U */
		    new char[]{'\u06C8', '\uFBDB', '\uFBDC'}, /* YU */
		    new char[]{'\u06C9', '\uFBE2', '\uFBE3'}, /* KIRGHIZ YU */
		    new char[]{'\u06CB', '\uFBDE', '\uFBDF'}, /* VE */

		    /*Ligatures (two elements)*/ /*There are many such ligatures: range from uFBEA to uFD3D*/
		    new char[]{'\u06CC', '\uFBFC', '\uFBFD', '\uFBFE', '\uFBFF'}, /* FARSI YEH */ 

		    /*Ligatures (three elements)*/ /*There are many such ligatures: range from uFD50 to uFDC7*/

		    /*for Central Asian*/
		    new char[]{'\u06D0', '\uFBE4', '\uFBE5', '\uFBE6', '\uFBE7'}, /* E */ 
		    new char[]{'\u06D2', '\uFBAE', '\uFBAF'}, /* YEH BARREE */
		    new char[]{'\u06D3', '\uFBB0', '\uFBB1'} /* YEH BARREE WITH HAMZA ABOVE */
	    };

		// Table: Arabic Letters and Their Applicability for Kashida before and after the letter
	    internal static Dictionary<char, bool[]> LetterApplicabilityTable = new Dictionary<char, bool[]>();

	    static ArabicLigaturizer()
	    {
			LetterApplicabilityTable.Add('\u0622', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u0623', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u0624', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u0625', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u0626', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0627', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u0628', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0629', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u062A', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u062B', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u062C', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u062D', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u062E', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u062F', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u0630', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u0631', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u0632', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u0633', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0634', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0635', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0636', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0637', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0638', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0639', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u063A', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0641', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0642', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0643', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0644', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0645', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0646', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0647', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0648', new bool[] { true, false });
			LetterApplicabilityTable.Add('\u0649', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u064A', new bool[] { true, true });
			LetterApplicabilityTable.Add('\u0640', new bool[] { true, true }); // special for kashida
	    }

	    public static bool IsArabicPoint(char s)
        {
            if (('\u064B' <= s) && (s <= '\u0652') || ('\u0670' == s))
                return true;

            return false;
        }

        public static bool IsArabicMark(char s)
        {
            if (('\u0616' == s) || ('\u0653' <= s) && (s <= '\u0655') || 
                ('\u0656' <= s) && (s <= '\u065F'))
                return true;

            return false;
        }

        public static bool IsArabicAnnotationSigns(char s)
        {
            if (('\u0615' == s) || ('\u0617' <= s) && (s <= '\u061A') || 
                ('\u06D6' <= s) && (s <= '\u06ED'))
                return true;

            return false;
        }

        public static bool IsArabicHonorific(char s)
        {
            if (('\u0610' <= s) && (s <= '\u0614'))
                return true;

            return false;
        }

        public static bool IsArabicLetter(char s)
        {
            if (('\u0621' <= s) && (s <= '\u06D3'))
                return true;

            return false;
        }

        public static bool IsArabic(char s)
        {
            if (('\u0600' <= s) && (s <= '\u06FF') || ('\u0750' <= s) && (s <= '\u077F') ||
                ('\uFB50' <= s) && (s <= '\uFDFF') || ('\uFE70' <= s) && (s <= '\uFEFF'))
                return true;

            return false;
        }

        private static bool IsVowel(char s)
        {
            // Why? Where is description for vowels?
            return (('\u064B' <= s) && (s <= '\u0655')) || (s == '\u0670');
        }

        private static char GetCharShape(char s, int which)
        {
            /* which 0 == isolated 1 == final 2 == initial 3 == medial */

            // 28 letters including 3 long vowels. 
            // 13 total combinations of short vowels (diacritic signs)

            int left, right, middle;
            if (IsArabicLetter(s))
            {
                left = 0;
                right = ArabicLigaturizer.CharTable.Length - 1;
                while (left <= right)
                {
                    middle = (left + right) / 2;

                    if (s == ArabicLigaturizer.CharTable[middle][0])
                        return ArabicLigaturizer.CharTable[middle][which + 1];
                    else if (s < ArabicLigaturizer.CharTable[middle][0])
                        right = middle - 1;
                    else
                        left = middle + 1;
                }
            }
            else if (('\ufef5' <= s) && (s <= '\ufefb')) //ligature
                return (char)(s + which);
            return s;
        }

        private static int ShapeCount(char s)
        {
            int left, right, middle;
            if (IsArabicLetter(s) && !IsVowel(s))
            {
                left = 0;
                right = ArabicLigaturizer.CharTable.Length - 1;
                while (left <= right)
                {
                    middle = (left + right) / 2;

                    if (s == ArabicLigaturizer.CharTable[middle][0])
                        return ArabicLigaturizer.CharTable[middle].Length - 1;
                    else if (s < ArabicLigaturizer.CharTable[middle][0])
                        right = middle - 1;
                    else
                        left = middle + 1;
                }
            }
            else if (s == ZWJ) //ZERO WIDTH JOINER
                return 4;
            return 1;
        }

        private class CharStruct
        {
            internal char BaseChar;
            internal char Mark1; /* has to be initialized to zero */
            internal char Vowel; 
            internal int LigNum; /* is a ligature with LigNum additional characters */
            internal int NumShapes = 1;
        };

        private static int Ligature(char newChar, CharStruct oldChar)
        {
            /* 0 == no ligature possible; 1 == vowel; 2 == two chars; 3 == Lam+Alef */

            int retval = 0;

            if (oldChar.BaseChar == 0) // base char yet is not defined
                return 0;

            if (IsVowel(newChar))
            {
                retval = 1;
                
                if ((oldChar.Vowel != 0) && (newChar != SHADDA))
                    retval = 2; /* we eliminate the old vowel .. */

                switch (newChar)
                {
                    case SHADDA:
                        {
                            if (oldChar.Mark1 == 0)
                                oldChar.Mark1 = SHADDA;
                            else
                                return 0; /* no ligature possible */
                            break;
                        }
                    case HAMZA_BELOW:
                        {
                            switch (oldChar.BaseChar)
                            {
                                case ALEF: { oldChar.BaseChar = ALEF_HAMZA_BELOW; retval = 2; break; }
                                case LAM_ALEF: { oldChar.BaseChar = LAM_ALEF_HAMZA_BELOW; retval = 2; break; }
                                default: { oldChar.Mark1 = HAMZA_BELOW; break; }
                            }
                            break;
                        }
                    case HAMZA_ABOVE:
                        {
                            switch (oldChar.BaseChar)
                            {
                                case ALEF: { oldChar.BaseChar = ALEF_HAMZA_ABOVE; retval = 2; break; }
                                case LAM_ALEF: { oldChar.BaseChar = LAM_ALEF_HAMZA_ABOVE; retval = 2; break; }
                                case WAW: { oldChar.BaseChar = WAW_HAMZA_ABOVE; retval = 2; break; }
                                case YEH:
                                case ALEF_MAKSURA:
                                case FARSI_YEH: { oldChar.BaseChar = YEH_HAMZA_ABOVE; retval = 2; break; }
                                default: { oldChar.Mark1 = HAMZA_ABOVE; break; } /* whatever sense this may make .. */
                            }
                            break;
                        }
                    case MADDA_ABOVE:
                        {
                            switch (oldChar.BaseChar)
                            {
                                case ALEF: { oldChar.BaseChar = ALEF_MADDA_ABOVE; retval = 2; break; }
                            }
                            break;
                        }
                    default: { oldChar.Vowel = newChar; break; }
                }

                if (retval == 1)
                    oldChar.LigNum++;

                return retval;
            }

            if (oldChar.Vowel != 0) /* if we already joined a vowel, we can't join a Hamza */
                return 0;

            switch (oldChar.BaseChar)
            {
                case LAM:
                    {
                        switch (newChar)
                        {
                            case ALEF: { oldChar.BaseChar = LAM_ALEF; oldChar.NumShapes = 2; retval = 3; break; }
                            case ALEF_HAMZA_ABOVE: { oldChar.BaseChar = LAM_ALEF_HAMZA_ABOVE; oldChar.NumShapes = 2; retval = 3; break; }
                            case ALEF_HAMZA_BELOW: { oldChar.BaseChar = LAM_ALEF_HAMZA_BELOW; oldChar.NumShapes = 2; retval = 3; break; }
                            case ALEF_MADDA_ABOVE: { oldChar.BaseChar = LAM_ALEF_MADDA_ABOVE; oldChar.NumShapes = 2; retval = 3; break; }
                        }
                        break;
                    }
                case (char)0:
                    {
                        oldChar.BaseChar = newChar; oldChar.NumShapes = ShapeCount(newChar); retval = 1; break;
                    }
            }

            return retval;
        }

        private static void CopyCharStructToString(StringBuilder str, CharStruct s, int level)
        {
            /* s is a shaped CharStruct; level is the index into the string */
            if (s.BaseChar == 0)
                return;

            str.Append(s.BaseChar);
            s.LigNum--;

            if (s.Mark1 != 0)
            {
                if ((level & ar_novowel) == 0)
                    str.Append(s.Mark1);

                s.LigNum--;
            }

            if (s.Vowel != 0)
            {
                if ((level & ar_novowel) == 0)
                    str.Append(s.Vowel);

                s.LigNum--;
            }
        }

        private static void DoubleLig(StringBuilder str, int level)
        {
            /* Ok. We have presentation ligatures in our font. */
            // return len

            int len;
            int olen = len = str.Length;
            int j = 0, si = 1;
            char lapresult;

            while (si < olen)
            {
                lapresult = (char)0;

                if ((level & ar_composedtaskheel) != 0)
                {
                    switch (str[j])
                    {
                        case SHADDA:
                            {
                                switch (str[si])
                                {
                                    case KASRA: { lapresult = '\uFC62'; break; }
                                    case FATHA: { lapresult = '\uFC60'; break; }
                                    case DAMMA: { lapresult = '\uFC61'; break; }
                                    case '\u064C': { lapresult = '\uFC5E'; break; }
                                    case '\u064D': { lapresult = '\uFC5F'; break; }
                                }
                                break;
                            }
                        case KASRA:
                            {
                                if (str[si] == SHADDA)
                                    lapresult = '\uFC62';
                                break;
                            }
                        case FATHA:
                            {
                                if (str[si] == SHADDA)
                                    lapresult = '\uFC60';
                                break;
                            }
                        case DAMMA:
                            {
                                if (str[si] == SHADDA)
                                    lapresult = '\uFC61';
                                break;
                            }
                    }
                }

                if ((level & ar_lig) != 0)
                {
                    switch (str[j])
                    {
                        case '\uFEDF':       /* LAM initial */
                            {
                                switch (str[si])
                                {
                                    case '\uFE9E': { lapresult = '\uFC3F'; break; } /* JEEM final */
                                    case '\uFEA0': { lapresult = '\uFCC9'; break; } /* JEEM medial */
                                    case '\uFEA2': { lapresult = '\uFC40'; break; } /* HAH final */
                                    case '\uFEA4': { lapresult = '\uFCCA'; break; } /* HAH medial */
                                    case '\uFEA6': { lapresult = '\uFC41'; break; } /* KHAH final */
                                    case '\uFEA8': { lapresult = '\uFCCB'; break; } /* KHAH medial */
                                    case '\uFEE2': { lapresult = '\uFC42'; break; } /* MEEM final */
                                    case '\uFEE4': { lapresult = '\uFCCC'; break; } /* MEEM medial */
                                }
                                break;
                            }
                        case '\uFE97':       /* TEH initial */
                            {
                                switch (str[si])
                                {
                                    case '\uFEA0': { lapresult = '\uFCA1'; break; } /* JEEM medial */
                                    case '\uFEA4': { lapresult = '\uFCA2'; break; } /* HAH medial */
                                    case '\uFEA8': { lapresult = '\uFCA3'; break; } /* KHAH medial */
                                }
                                break;
                            }
                        case '\uFE91':       /* BEH initial */
                            {
                                switch (str[si])
                                {
                                    case '\uFEA0': { lapresult = '\uFC9C'; break; } /* JEEM medial */
                                    case '\uFEA4': { lapresult = '\uFC9D'; break; } /* HAH medial */
                                    case '\uFEA8': { lapresult = '\uFC9E'; break; } /* KHAH medial */
                                }
                                break;
                            }
                        case '\uFEE7':       /* NOON initial */
                            {
                                switch (str[si])
                                {
                                    case '\uFEA0': { lapresult = '\uFCD2'; break; } /* JEEM initial */
                                    case '\uFEA4': { lapresult = '\uFCD3'; break; } /* HAH medial */
                                    case '\uFEA8': { lapresult = '\uFCD4'; break; } /* KHAH medial */
                                }
                                break;
                            }
                        case '\uFEE8':       /* NOON medial */
                            {
                                switch (str[si])
                                {
                                    case '\uFEAE': { lapresult = '\uFC8A'; break; } /* REH final  */
                                    case '\uFEB0': { lapresult = '\uFC8B'; break; } /* ZAIN final */
                                }
                                break;
                            }
                        case '\uFEE3':       /* MEEM initial */
                            {
                                switch (str[si])
                                {
                                    case '\uFEA0': { lapresult = '\uFCCE'; break; } /* JEEM medial */
                                    case '\uFEA4': { lapresult = '\uFCCF'; break; } /* HAH medial */
                                    case '\uFEA8': { lapresult = '\uFCD0'; break; } /* KHAH medial */
                                    case '\uFEE4': { lapresult = '\uFCD1'; break; } /* MEEM medial */
                                }
                                break;
                            }
                        case '\uFED3':       /* FEH initial */
                            {
                                switch (str[si])
                                {
                                    case '\uFEF2': { lapresult = '\uFC32'; break; } /* YEH final */
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }                   /* end switch string[si] */
                }

                if (lapresult != 0)
                {
                    str[j] = lapresult;
                    len--;
                    si++;                 /* jump over one character */
                    /* we'll have to change this, too. */
                }
                else
                {
                    j++;
                    str[j] = str[si];
                    si++;
                }
            }

            str.Length = len;
        }

        private static bool ConnectsToLeft(CharStruct a)
        {
            return a.NumShapes > 2;
        }

        private static void Shape(char[] text, StringBuilder str, int level)
        {
            /* string is assumed to be empty and big enough.
            * text is the original text.
            * This routine does the basic arabic reshaping.
            * len the number of non-null characters.
            *
            * Note: We have to unshape each character first!
            */
            int join;
            int which;
            char nextLetter;

            int p = 0; /* initialize for output */
            CharStruct oldchar = new CharStruct();
            CharStruct curchar = new CharStruct();

            while (p < text.Length)
            {
                nextLetter = text[p++];
                //nextletter = unshape (nextletter);

                join = Ligature(nextLetter, curchar);
                if (join == 0)
                {                       
                    /* shape curchar */
                    int nc = ShapeCount(nextLetter); // from 1 to 4
                    //(*len)++;

                    /* which 0 == isolated 1 == final 2 == initial 3 == medial */
                    if (nc == 1)
                        which = 0;        /* final or isolated */
                    else
                        which = 2;        /* medial or initial */
                    if (ConnectsToLeft(oldchar))
                        which++;
                    which = which % (curchar.NumShapes);
                    curchar.BaseChar = GetCharShape(curchar.BaseChar, which);

                    /* get rid of oldchar */
                    CopyCharStructToString(str, oldchar, level);
                    oldchar = curchar;    /* new values in oldchar */

                    /* init new curchar */
                    curchar = new CharStruct();
                    curchar.BaseChar = nextLetter;
                    curchar.NumShapes = nc;
                    curchar.LigNum++;
                    //          (*len) += unligature (&curchar, level);
                }
                else if (join == 1)
                {
                }
                //      else
                //        {
                //          (*len) += unligature (&curchar, level);
                //        }
                //      p = g_utf8_next_char (p);
            }

            /* handle last char */
            which = 0;
            if (ConnectsToLeft(oldchar))
                which++;
            which = which % (curchar.NumShapes);
            curchar.BaseChar = GetCharShape(curchar.BaseChar, which);

            /* get rid of oldchar */
            CopyCharStructToString(str, oldchar, level);
            CopyCharStructToString(str, curchar, level);
        }

        public static int Arabic_Shape(char[] src, int srcOffset, int srcLength, char[] dest, int destOffset, int destLength, int level)
        {
            char[] str = new char[srcLength];

            for (int k = srcOffset + srcLength - 1; k >= srcOffset; --k)
                str[k - srcOffset] = src[k];

            StringBuilder str2 = new StringBuilder(srcLength);
            Shape(str, str2, level);

            if ((level & (ar_composedtaskheel | ar_lig)) != 0)
                DoubleLig(str2, level);

            //        string.Reverse();
            str2.CopyTo(0, dest, destOffset, str2.Length);

            return str2.Length;
        }

        public static void ProcessNumbers(char[] text, int offset, int length, int options)
        {
            int limit = offset + length;

            if ((options & DIGITS_MASK) != 0)
            {
                char digitBase = '\u0030'; // European digits

                switch (options & DIGIT_TYPE_MASK)
                {
                    case DIGIT_TYPE_AN:
                        digitBase = '\u0660';  // Arabic-Indic digits
                        break;

                    case DIGIT_TYPE_AN_EXTENDED:
                        digitBase = '\u06f0';  // Eastern Arabic-Indic digits (Persian and Urdu)
                        break;

                    default:
                        break;
                }

                switch (options & DIGITS_MASK)
                {
                    case DIGITS_EN2AN:
                        {
                            int digitDelta = digitBase - '\u0030';
                            for (int i = offset; i < limit; ++i)
                            {
                                char ch = text[i];
                                if (ch <= '\u0039' && ch >= '\u0030')
                                    text[i] += (char)digitDelta;
                            }
                        }
                        break;
                    case DIGITS_AN2EN:
                        {
                            char digitTop = (char)(digitBase + 9);
                            int digitDelta = '\u0030' - digitBase;
                            for (int i = offset; i < limit; ++i)
                            {
                                char ch = text[i];
                                if (ch <= digitTop && ch >= digitBase)
                                    text[i] += (char)digitDelta;
                            }
                        }
                        break;

                    case DIGITS_EN2AN_INIT_LR:
                        ShapeToArabicDigitsWithContext(text, 0, length, digitBase, false);
                        break;

                    case DIGITS_EN2AN_INIT_AL:
                        ShapeToArabicDigitsWithContext(text, 0, length, digitBase, true);
                        break;

                    default:
                        break;
                }
            }
        }

        public static void ShapeToArabicDigitsWithContext(char[] dest, int start, int length, char digitBase, bool lastStrongWasAL)
        {
            digitBase -= '0'; // move common adjustment out of loop

            int limit = start + length;
            for (int i = start; i < limit; ++i)
            {
                char ch = dest[i];
                /*switch (BidiOrder.GetDirection(ch))
                {
                    case BidiOrder.L:
                    case BidiOrder.R:
                        lastStrongWasAL = false;
                        break;
                    case BidiOrder.AL: // Arabic letter
                        lastStrongWasAL = true;
                        break;
                    case BidiOrder.EN: // European number
                        if (lastStrongWasAL && ch <= '\u0039')
                            dest[i] = (char)(ch + digitBase);
                        break;
                    default:
                        break;
                }*/
            }
        }

        public static bool CanAddKashidaBefore(char letter)
        {
            if (!TableContiansThisChar(ref letter))
                return false;

            return LetterApplicabilityTable[letter][0];
        }

        public static bool CanAddKashidaAfter(char letter)
        {
            if (!TableContiansThisChar(ref letter))
                return false;

            return LetterApplicabilityTable[letter][1];
        }

        private static bool TableContiansThisChar(ref char letter)
        {
            if (!ArabicLigaturizer.LetterApplicabilityTable.ContainsKey(letter))
            {
                letter = UnshapeChar(letter);
                if (!ArabicLigaturizer.LetterApplicabilityTable.ContainsKey(letter))
                    return false;
            }
            return true;
        }

        private static char UnshapeChar(char ch)
        {
            string str = new string(new char[] {ch});
            str = str.Normalize(NormalizationForm.FormKD);
            return str[0];
        }
    }
}
