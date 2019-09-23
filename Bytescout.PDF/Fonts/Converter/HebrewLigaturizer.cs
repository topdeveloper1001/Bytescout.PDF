using System.Text;

namespace Bytescout.PDF
{
    internal class HebrewLigaturizer
    {
        public static bool IsHebrewCantillationMark(char s)
        {
            if (('\u0591' <= s) && (s <= '\u05AF'))
                return true;

            return false;
        }

        public static bool IsHebrewPoint(char s)
        {
            if (('\u05B0' <= s) && (s <= '\u05C7') && 
                (s != '\u05BE') && (s != '\u05C0') && 
                (s != '\u05C3') && (s != '\u05C6') || (s == '\uFB1E'))
                return true;

            return false;
        }

        public static bool IsHebrewLetter(char s)
        {
            if (('\u05D0' <= s) && (s <= '\u05EA'))
                return true;

            return false;
        }

        // May be should to unshape such chars before processing?
        public static bool IsWideHebrewLetter(char s)
        {
            if (('\uFB21' <= s) && (s <= '\uFB28'))
                return true;

            return false;
        }

        public static bool IsHebrew(char s)
        {
            if (('\u0590' <= s) && (s <= '\u05FF') || ('\uFB1D' <= s) && (s <= '\uFB4F'))
                return true;

            return false;
        }

        private static char GetCharShape(char s, int which)
        {
            /* which 0 == general(initial&medial) 1 == final */

            // 22 letters includes 5 with two type faces (general & final)
            if (IsHebrewLetter(s))
            {
                switch(s)
                {
                    case KAF:
                    case MEM:
                    case NUN:
                    case PE:
                    case TSADI:
                            return (char)(s - which);
                    default:
                            return s;
                }
            }

            return s;
        }

        private static int ShapeCount(char s)
        {
            // 22 letters includes 5 with two type faces (general & final)
            if (IsHebrewLetter(s))
            {
                switch (s)
                {
                    case KAF:
                    case MEM:
                    case NUN:
                    case PE:
                    case TSADI:
                        return 2;
                    default:
                        return 1;
                }
            }

            return 1;
        }

        private class CharStruct
        {
            internal char BaseChar;
            internal int NumShapes = 1;
        };

        private static int Ligature(char newChar, CharStruct oldChar)
        {
            /* 0 == no ligature possible; 2 == two chars; 3 == three chars*/

            int retval = 0;

            if (oldChar.BaseChar == 0) // base char yet is not defined
                return 0;

            switch (newChar)
            {
                case HIRIQ:
                    {
                        switch (oldChar.BaseChar)
                        {
                            case YOD: { oldChar.BaseChar = YOD_HIRIQ; retval = 2; break; }
                        }
                        break;
                    }
                case PATAH:
                    {
                        switch (oldChar.BaseChar)
                        {
                            case ALEF: { oldChar.BaseChar = ALEF_PATAH; retval = 2; break; }
                        }
                        break;
                    }
                case QAMATS:
                    {
                        switch (oldChar.BaseChar)
                        {
                            case ALEF: { oldChar.BaseChar = ALEF_QAMATS; retval = 2; break; }
                        }
                        break;
                    }
                case HOLAM:
                    {
                        switch (oldChar.BaseChar)
                        {
                            case VAV: { oldChar.BaseChar = VAV_HOLAM; retval = 2; break; }
                        }
                        break;
                    }
                case DAGESH: // == case MAPIQ: 
                    {
                        switch (oldChar.BaseChar)
                        {
                            case ALEF: { oldChar.BaseChar = ALEF_MAPIQ; retval = 2; break; }
                            case HE: { oldChar.BaseChar = HE_MAPIQ; retval = 2; break; }

                            case BET: { oldChar.BaseChar = BET_DAGESH; retval = 2; break; }
                            case GIMEL: { oldChar.BaseChar = GIMEL_DAGESH; retval = 2; break; }
                            case DALET: { oldChar.BaseChar = DALET_DAGESH; retval = 2; break; }
                            case VAV: { oldChar.BaseChar = VAV_DAGESH; retval = 2; break; }
                            case ZAYIN: { oldChar.BaseChar = ZAYIN_DAGESH; retval = 2; break; }
                            case TET: { oldChar.BaseChar = TET_DAGESH; retval = 2; break; }
                            case YOD: { oldChar.BaseChar = YOD_DAGESH; retval = 2; break; }
                            case FINAL_KAF: { oldChar.BaseChar = FINAL_KAF_DAGESH; retval = 2; break; }
                            case KAF: { oldChar.BaseChar = KAF_DAGESH; retval = 2; break; }
                            case LAMED: { oldChar.BaseChar = LAMED_DAGESH; retval = 2; break; }
                            case MEM: { oldChar.BaseChar = MEM_DAGESH; retval = 2; break; }
                            case NUN: { oldChar.BaseChar = NUN_DAGESH; retval = 2; break; }
                            case SAMEKH: { oldChar.BaseChar = SAMEKH_DAGESH; retval = 2; break; }
                            case FINAL_PE: { oldChar.BaseChar = FINAL_PE_DAGESH; retval = 2; break; }
                            case PE: { oldChar.BaseChar = PE_DAGESH; retval = 2; break; }
                            case TSADI: { oldChar.BaseChar = TSADI_DAGESH; retval = 2; break; }
                            case QOF: { oldChar.BaseChar = QOF_DAGESH; retval = 2; break; }
                            case RESH: { oldChar.BaseChar = RESH_DAGESH; retval = 2; break; }
                            case SHIN: { oldChar.BaseChar = SHIN_DAGESH; retval = 2; break; }
                            case TAV: { oldChar.BaseChar = TAV_DAGESH; retval = 2; break; }
                        }
                        break;
                    }
                case SHIN_DOT:
                    {
                        switch (oldChar.BaseChar)
                        {
                            case SHIN: { oldChar.BaseChar = SHIN_SHIN_DOT; retval = 2; break; }
                            case SHIN_DAGESH: { oldChar.BaseChar = SHIN_DAGESH_SHIN_DOT; retval = 3; break; } // 3 CHARS
                        }
                        break;
                    }
                case SIN_DOT:
                    {
                        switch (oldChar.BaseChar)
                        {
                            case SHIN: { oldChar.BaseChar = SHIN_SIN_DOT; retval = 2; break; }
                            case SHIN_DAGESH: { oldChar.BaseChar = SHIN_DAGESH_SIN_DOT; retval = 3; break; } // 3 CHARS
                        }
                        break;
                    }
                case RAFE:
                    {
                        switch (oldChar.BaseChar)
                        {
                            case BET: { oldChar.BaseChar = BET_RAFE; retval = 2; break; }
                            case KAF: { oldChar.BaseChar = KAF_RAFE; retval = 2; break; }
                            case PE: { oldChar.BaseChar = PE_RAFE; retval = 2; break; }
                        }
                        break;
                    }
                case LAMED:
                    {
                        switch (oldChar.BaseChar)
                        {
                            case ALEF: { oldChar.BaseChar = ALEF_LAMED; retval = 2; break; }
                        }
                        break;
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
                    int nc = ShapeCount(nextLetter); // from 1 to 2
                    //(*len)++;

                    /* which 0 == general(initial&medial) 1 == final */
                    which = 0;
                    curchar.BaseChar = GetCharShape(curchar.BaseChar, which);

                    /* get rid of oldchar */
                    CopyCharStructToString(str, oldchar, level);
                    oldchar = curchar;    /* new values in oldchar */

                    /* init new curchar */
                    curchar = new CharStruct();
                    curchar.BaseChar = nextLetter;
                    curchar.NumShapes = nc;
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
            if (curchar.NumShapes == 2)
            {
                which = 1;
                curchar.BaseChar = GetCharShape(curchar.BaseChar, which);
            }

            /* get rid of oldchar */
            CopyCharStructToString(str, oldchar, level);
            CopyCharStructToString(str, curchar, level);
        }

        public static int Hebrew_Shape(char[] src, int srcOffset, int srcLength, char[] dest, int destOffset, int destLength, int level)
        {
            char[] str = new char[srcLength];

            for (int k = srcOffset + srcLength - 1; k >= srcOffset; --k)
                str[k - srcOffset] = src[k];

            StringBuilder str2 = new StringBuilder(srcLength);
            Shape(str, str2, level);

            //        string.Reverse();
            str2.CopyTo(0, dest, destOffset, str2.Length);

            return str2.Length;
        }

        #region LIGATURES
        
        private const char YOD_HIRIQ = '\uFB1D';
        private const char SHIN_SHIN_DOT = '\uFB2A';
        private const char SHIN_SIN_DOT = '\uFB2B';
        private const char SHIN_DAGESH_SHIN_DOT = '\uFB2C'; // 3 CHARS
        private const char SHIN_DAGESH_SIN_DOT = '\uFB2D'; // 3 CHARS
        private const char ALEF_PATAH = '\uFB2E';
        private const char ALEF_QAMATS = '\uFB2F';
        private const char ALEF_MAPIQ = '\uFB30';
        private const char BET_DAGESH = '\uFB31';
        private const char GIMEL_DAGESH = '\uFB32';
        private const char DALET_DAGESH = '\uFB33';
        private const char HE_MAPIQ = '\uFB34';
        private const char VAV_DAGESH = '\uFB35';
        private const char ZAYIN_DAGESH = '\uFB36';
        private const char TET_DAGESH = '\uFB38';
        private const char YOD_DAGESH = '\uFB39';
        private const char FINAL_KAF_DAGESH = '\uFB3A';
        private const char KAF_DAGESH = '\uFB3B';
        private const char LAMED_DAGESH = '\uFB3C';
        private const char MEM_DAGESH = '\uFB3E';
        private const char NUN_DAGESH = '\uFB40';
        private const char SAMEKH_DAGESH = '\uFB41';
        private const char FINAL_PE_DAGESH = '\uFB43';
        private const char PE_DAGESH = '\uFB44';
        private const char TSADI_DAGESH = '\uFB46';
        private const char QOF_DAGESH = '\uFB47';
        private const char RESH_DAGESH = '\uFB48';
        private const char SHIN_DAGESH = '\uFB49';
        private const char TAV_DAGESH = '\uFB4A';
        private const char VAV_HOLAM = '\uFB4B';
        private const char BET_RAFE = '\uFB4C';
        private const char KAF_RAFE = '\uFB4D';
        private const char PE_RAFE = '\uFB4E';
        private const char ALEF_LAMED = '\uFB4F';

        #endregion

        #region POINTS

        private const char HIRIQ = '\u05B4';
        private const char PATAH = '\u05B7';
        private const char QAMATS = '\u05B8';
        private const char HOLAM = '\u05B9';
        private const char DAGESH = '\u05BC';
        private const char MAPIQ = '\u05BC';
        private const char RAFE = '\u05BF';
        private const char SHIN_DOT = '\u05C1';
        private const char SIN_DOT = '\u05C2';

        #endregion

        #region LETTERS

        private const char ALEF = '\u05D0';
        private const char BET = '\u05D1';
        private const char GIMEL = '\u05D2';
        private const char DALET = '\u05D3';
        private const char HE = '\u05D4';
        private const char VAV = '\u05D5';
        private const char ZAYIN = '\u05D6';
        private const char HET = '\u05D7';
        private const char TET = '\u05D8';
        private const char YOD = '\u05D9';
        private const char FINAL_KAF = '\u05DA';
        private const char KAF = '\u05DB';
        private const char LAMED = '\u05DC';
        private const char FINAL_MEM = '\u05DD';
        private const char MEM = '\u05DE';
        private const char FINAL_NUN = '\u05DF';
        private const char NUN = '\u05E0';
        private const char SAMEKH = '\u05E1';
        private const char AYIN = '\u05E2';
        private const char FINAL_PE = '\u05E3';
        private const char PE = '\u054E';
        private const char FINAL_TSADI = '\u05E5';
        private const char TSADI = '\u05E6';
        private const char QOF = '\u05E7';
        private const char RESH = '\u05E8';
        private const char SHIN = '\u05E9';
        private const char TAV = '\u05EA';

        #endregion

    }
}
