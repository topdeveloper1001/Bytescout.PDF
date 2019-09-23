using System;
using System.Text;

namespace Bytescout.PDF
{
    internal class Normalizer
    {
        public enum RightToLeftTextType { None, Arabic, Hebrew };

        public static RightToLeftTextType GetRightToLeftTextType(string str)
        {
            string splitters = GetSplittersInThatString(str);
            string[] str_array = str.Split(splitters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < str_array.Length; ++i)
            {
                if (str_array[i].Equals(string.Empty))
                    continue;

                if (ArabicLigaturizer.IsArabic(str_array[i][0]))
                    return RightToLeftTextType.Arabic;

                if (HebrewLigaturizer.IsHebrew((str_array[i][0])))
                    return RightToLeftTextType.Hebrew;
            }

            return RightToLeftTextType.None;
        }

        public static string GetAppropriateFormString(string str, bool isRightToLeft)
        {
            string splitters = GetSplittersInThatString(str);
            string[] str_array = str.Split(splitters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            string ret_str = str;
            for (int i = 0; i < str_array.Length; ++i)
            {
                string change_str = GetAppropriateFormWord(str_array[i]);
                ret_str = SoftStringReplacing(ret_str, str_array[i], change_str); 
            }
            return ret_str;
        }

        public static string SoftStringReplacing(string str, string oldPart, string newPart)
        {
            // don't use simple replacing
            // str = str.Replace(oldPart, newPart); 
            // replacing can be a spot of confusing

            int startIndex = str.IndexOf(oldPart);
            int length = oldPart.Length;
            str = str.Substring(0, startIndex) + newPart + str.Substring(startIndex + length);
            return str;
        }

        public static string ReverseOnlyDiacrMarks(string str)
        {
            string diacrs = string.Empty;
            int startIndex = -1;

            for (int i = 0; i < str.Length; i++)
            {
                if (string.IsNullOrEmpty(diacrs))
                {
                    if (!Normalizer.IsRightToLeftMark(str[i]))
                        continue;

                    diacrs += str[i];
                    startIndex = i;
                }
                else
                {
                    if (Normalizer.IsRightToLeftMark(str[i]))
                    {
                        diacrs += str[i];
                        continue;
                    }

                    diacrs += str[i];

                    diacrs = SimpleReverseText(diacrs);
                    str = str.Substring(0, startIndex) + diacrs + str.Substring(startIndex + diacrs.Length);

                    diacrs = string.Empty;
                    startIndex = -1;
                }
            }

            return str;
        }

        public static string GetAppropriateFormWord(string word)
        {
            if (word.Length == 0)
                return word;

            string ret_word = word;
            char s = word[0];

            if (!HebrewLigaturizer.IsHebrew(s) && !ArabicLigaturizer.IsArabic(s))
                return ret_word;

            if (HebrewLigaturizer.IsHebrew(s))
                ret_word = Normalizer.RightToLeftConvert(word, RightToLeftTextType.Hebrew);
            else if (ArabicLigaturizer.IsArabic(s))
                ret_word = Normalizer.RightToLeftConvert(word, RightToLeftTextType.Arabic);

            if (!ret_word.Equals(word))
            {
                string norm_word = word.Normalize(NormalizationForm.FormKD);
                string norm_ret_word = ret_word.Normalize(NormalizationForm.FormKD);

                if (norm_ret_word != norm_word)
                {
                    // throw new Exception("Exception at the string normalization process.");
                    return word;
                }
            }

            return ret_word;
        }

        public static string SimpleReverseText(string s)
        {
            char[] chars = s.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static string ReverseTextWithoutReversingDigits(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length);

            string number = string.Empty;

            for (int i = s.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(s[i]))
                {
                    if (number != string.Empty)
                    {
                        sb.Append(number);
                        number = string.Empty;
                    }
                    sb.Append(s[i]);
                }
                else
                    number = s[i] + number;
            }

            if (number != string.Empty)
            {
                sb.Append(number);
                number = string.Empty;
            }

            return sb.ToString();
        }

        public static bool IsRightToLeftMark(char s)
        {
            if (HebrewLigaturizer.IsHebrewPoint(s) || 
                HebrewLigaturizer.IsHebrewCantillationMark(s))
                return true;

            if (ArabicLigaturizer.IsArabicHonorific(s) ||
                ArabicLigaturizer.IsArabicAnnotationSigns(s) ||
                ArabicLigaturizer.IsArabicPoint(s) ||
                ArabicLigaturizer.IsArabicMark(s))
                return true;

            return false;
        }
        
        public static string RightToLeftConvert(string init_str, RightToLeftTextType type)
        {
            if (init_str.Contains(" "))
                throw new ArgumentException("This is not one word!");

            // TODO: before shaping maybe unshape initial string

            char[] dest = new char[init_str.Length];
            int destLength = 0;

            switch (type)
            {
                case RightToLeftTextType.Arabic:
                    destLength = ArabicLigaturizer.Arabic_Shape(init_str.ToCharArray(), 0, init_str.Length, dest, 0, 0, 0);
                    break;
                case RightToLeftTextType.Hebrew:
                    destLength = HebrewLigaturizer.Hebrew_Shape(init_str.ToCharArray(), 0, init_str.Length, dest, 0, 0, 0);
                    break;
            }

            return new string(dest, 0, destLength);
        }
        
        public static string GetSplittersInThatString(string str)
        {
            StringBuilder splitters = new StringBuilder(str.Length);

            for (int i = 0; i < str.Length; ++i)
            {
                char ch = str[i];
                if (char.IsPunctuation(ch) || char.IsSeparator(ch) || char.IsWhiteSpace(ch))
                    splitters.Append(ch);
            }

            return splitters.ToString();
        }

        public static string GetTextWithoutMarks(string str)
        {
            char[] ret_str = new char[str.Length];
            int length = 0;

            for (int i = 0; i < str.Length; i++)
            {
                if (IsRightToLeftMark(str[i]))
                    continue;
                else
                {
                    ret_str[length] = str[i];
                    length++;
                }
            }

            return new string(ret_str, 0, length);
        }

        public static int GetTextLengthWithoutMarks(string str)
        {
            int length = 0;

            for (int i = 0; i < str.Length; i++)
            {
                if (IsRightToLeftMark(str[i]))
                    continue;
                else
                    length++;
            }

            return length;
        }

        public static bool ContainsDiacriticMarks(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (IsRightToLeftMark(str[i]))
                    return true;
            }

            return false;
        }

    }
}
