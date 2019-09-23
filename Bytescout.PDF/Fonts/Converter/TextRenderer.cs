using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Bytescout.PDF
{
    internal class TextRenderer
    {
        public static bool TestScriptIsComplexUniscribeFunction()
        {
            string str = "Hello World";
            if (ScriptIsComplex(str))
                return false;

            str = "Hello يُساوِي World";
            if (!ScriptIsComplex(str))
                return false;

            return true;
        }

        public static bool TestScriptGetPropertiesUniscribeFunction()
        {
            Uniscribe.SCRIPT_PROPERTIES[] ppScriptProperties;
            if (!ScriptGetProperties(out ppScriptProperties))
                return false;

            return true;
        }

        public static bool TestScriptItemizeUniscribeFunction()
        {
            string str = "Hello السعودية!";
            Uniscribe.SCRIPT_ITEM[] items;

            if (!ScriptItemize(str, out items, false))
                return false;

            if (items.Length != 4)
                return false;

            if ((items[0].iCharPos != 0) || (items[0].a.fRTL != false))
                return false;

            if ((items[1].iCharPos != 6) || (items[1].a.fRTL != true))
                return false;

            if ((items[2].iCharPos != 14) || (items[2].a.fRTL != false))
                return false;

            if ((items[3].iCharPos != 15) || (items[3].a.fRTL != false))
                return false;

            Uniscribe.SCRIPT_PROPERTIES[] ppScriptProperties;
            if (!ScriptGetProperties(out ppScriptProperties))
                return false;

            if (ppScriptProperties[items[1].a.eScript].fComplex)
            {
                // Item [i] is complex script text
                // requiring glyph shaping.
            }

            return true;
        }

        public static bool TestScriptLayoutUniscribeFunction()
        {
            string str = "Hello اليوم טוב world";
            Uniscribe.SCRIPT_ITEM[] items;

            if (!ScriptItemize(str, out items, false))
                return false;

            int[] res_visual_to_logical;
            int[] res_logical_to_visual;

            if (!ScriptLayout(str, items, out res_visual_to_logical, out res_logical_to_visual))
                return false;

            int[] vtl = new int[] { 0, 2, 1, 3, 4 };
            int[] ltv = new int[] { 0, 2, 1, 3, 4 };

            for (int i = 0; i < items.Length; i++)
            {
                if (res_visual_to_logical[i] != vtl[i])
                    return false;

                if (res_logical_to_visual[i] != ltv[i])
                    return false;
            }

            return true;
        }

        public static bool TestGetFontPropertiesUniscribeFunction()
        {
            System.Drawing.Font font = new System.Drawing.Font("Shruti", 16);
            Uniscribe.SCRIPT_FONTPROPERTIES sfp;

            if (!ScriptGetFontProperties(font, out sfp))
                return false;

            return true;
        }

        public static bool TestScriptGetCMapUniscribeFunction()
        {
            System.Drawing.Font font = new System.Drawing.Font("Arial Unicode MS", 16);
            Uniscribe.SCRIPT_FONTPROPERTIES sfp;

            if (!ScriptGetFontProperties(font, out sfp))
                return false;

            string str = "السعودية";

            if (!ScriptGetCMap(str, font, sfp))
                return false;

            return true;
        }

        public static bool TestScriptShapeUniscribeFunction()
        {
            string str = "fiancé";
            // string str = "\uFB01anc\u00E9";
            // string str = "écrit";

            Uniscribe.SCRIPT_ITEM[] items;

            if (!ScriptItemize(str, out items, false))
                return false;

            System.Drawing.Font font = new System.Drawing.Font("Arial Unicode MS", 16);

            List<ushort[]> pwOutGlyphsArray;
            List<int> cGlyphsArray;
            List<Uniscribe.SCRIPT_VISATTR[]> psvaArray;

            if (!ScriptShape(str, font, items, out pwOutGlyphsArray, out cGlyphsArray, out psvaArray))
                return false;

            return true;
        }

        public static bool TestScriptPlaceUniscribeFunction(string filePathSave)
        {
            //string str = "Hello everybody if you hear";
            
            // Check ScriptJustify (for Kashida)
            string str = "يُساوِي جيد";

            Uniscribe.SCRIPT_ITEM[] items;

            if (!ScriptItemize(str, out items, true))
                return false;

//             System.Drawing.Font font = new System.Drawing.Font("Arial Unicode MS", 16, FontStyle.Italic);
            System.Drawing.Font font = new System.Drawing.Font("Microsoft Sans Serif", 16, FontStyle.Italic);

            List<ushort[]> pwOutGlyphsArray;
            List<int> cGlyphsArray;
            List<Uniscribe.SCRIPT_VISATTR[]> psvaArray;

            if (!ScriptShape(str, font, items, out pwOutGlyphsArray, out cGlyphsArray, out psvaArray))
                return false;

            List<int[]> piAdvanceArray;
            List<Uniscribe.GOFFSET[]> pGoffsetArray;
            List<Uniscribe.ABC> pABCArray;

            if (!ScriptPlace(font, items, pwOutGlyphsArray, cGlyphsArray, psvaArray, out piAdvanceArray, out pGoffsetArray, out pABCArray))
                return false;

            Uniscribe.SCRIPT_FONTPROPERTIES sfp;
            if (!ScriptGetFontProperties(font, out sfp))
                return false;

            List<int> iDxArray = new List<int>(psvaArray.Count);
            List<int> iMinKashidaArray = new List<int>(psvaArray.Count);

            for (int i = 0; i < psvaArray.Count; i++)
            {
                iDxArray.Add(10);
                iMinKashidaArray.Add(sfp.iKashidaWidth);
            }

            List<int[]> piJustifyArray;

            if (!ScriptJustify(psvaArray, piAdvanceArray, cGlyphsArray, iDxArray, iMinKashidaArray, out piJustifyArray))
                return false;

            Uniscribe.SCRIPT_LOGATTR[] psla;

            if (!ScriptBreakForItem(str, str.Length, items[0].a, out psla))
                return false;

            List<int> xArray = new List<int>();
            List<int> yArray = new List<int>();

            int x = 30;
            int y = 30;
            for (int i = 0; i < psvaArray.Count; i++)
            {
                xArray.Add(x);
                yArray.Add(y);
            }

            List<Uniscribe.SCRIPT_ANALYSIS> psaArray = new List<Uniscribe.SCRIPT_ANALYSIS>();
            for (int i = 0; i < items.Length - 1; i++)
            {
                psaArray.Add(items[i].a);
            }

            Win32.RECT lprc = new Win32.RECT();

            if (!ScriptTextOut(filePathSave, font, xArray, yArray, 0, lprc, psaArray, null, 0, 
                pwOutGlyphsArray, cGlyphsArray, piAdvanceArray, piJustifyArray, pGoffsetArray))
                return false;

            return true;
        }

        public static bool ScriptGetCMap(string str, System.Drawing.Font font, Uniscribe.SCRIPT_FONTPROPERTIES sfp)
        {
            if (font == null)
                return false;

            uint res = 0;

            // HFONT hfont = initialize your font;
            IntPtr hfont = font.ToHfont();

            IntPtr psc = IntPtr.Zero; // Initialize to NULL, will be filled lazily.

            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr HDC = IntPtr.Zero; // Don't give it a DC unless we have to.

            // HFONT old_font = NULL;
            IntPtr old_font = IntPtr.Zero;

            ushort[] pwOutGlyphs = new ushort[str.Length];

            for (int max = 1000; max > 0; --max)
            {
                try
                {
                    res = 0;
                    res = Uniscribe.ScriptGetCMap(HDC, ref psc, str, str.Length, 0, pwOutGlyphs);

                    if (res != 0)
                    {
                        // Different types of failure...
                        if (res == Win32.E_PENDING)
                        {
                            // Need to select the font for the call. Don't do this if we don't have to
                            // since it may be slow.
                            throw new ArgumentException();
                        }
                        if (res == Win32.E_HANDLE)
                        {
                            // The font or the operating system does not support glyph indexes.
                            throw new Exception();
                        }
                        else if (res == Win32.S_FALSE)
                        {
                            // Some of the Unicode code points were mapped to the default glyph.
                            throw new Exception();
                        }
                    }

                    // call ScriptGetFontProperties
                    int defaultGlyph = sfp.wgDefault;

                    for (int i = 0; i < str.Length; i++)
                    {
                        if (pwOutGlyphs[i] == defaultGlyph)
                        {
                            //  character with that index is not available in selected font
                        }
                    }

                    break;
                }
                catch (ArgumentException)
                {
                    // ... select font into hdc ...

                    hdcSrc = Win32.GetDC(IntPtr.Zero);
                    HDC = Win32.CreateCompatibleDC(hdcSrc);

                    old_font = Win32.SelectObject(HDC, hfont);

                    // Loop again...
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }

            if (old_font != IntPtr.Zero)
                Win32.SelectObject(HDC, old_font);  // Put back the previous font.

            Win32.ReleaseDC(IntPtr.Zero, HDC);
            Win32.ReleaseDC(IntPtr.Zero, hdcSrc);

            // Need to tell Uniscribe to delete the cache we were using. If you are going
            // to keep the HFONT around, you should probably also keep the cache.
            //             ScriptFreeCache(psc);

            Win32.DeleteObject(hfont);

            return (res == 0);
        }

        public static bool ScriptGetProperties(out Uniscribe.SCRIPT_PROPERTIES[] ppScriptProperties)
        {
            ppScriptProperties = null;

            uint res = 0;

            try
            {
                IntPtr p = IntPtr.Zero;
                int piNumScripts = 0;

                res = 0;
                res = Uniscribe.ScriptGetProperties(ref p, ref piNumScripts);

                if (res != 0)
                {
                    throw new Exception(); // Some kind of error.
                }

                IntPtr[] destination = new IntPtr[piNumScripts];
                Marshal.Copy(p, destination, 0, piNumScripts);

                ppScriptProperties = new Uniscribe.SCRIPT_PROPERTIES[piNumScripts];
                for (int i = 0; i < piNumScripts; i++)
                {
                    Uniscribe.SCRIPT_PROPERTIES ppSpi = (Uniscribe.SCRIPT_PROPERTIES)Marshal.PtrToStructure(
                        destination[i], typeof(Uniscribe.SCRIPT_PROPERTIES));
                    ppScriptProperties[i] = ppSpi;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return (res == 0);
        }

        public static bool ScriptIsComplex(string str)
        {
            uint res = 0;

            try
            {
                // Determines whether a Unicode string requires complex script processing.
                res = 0;
                res = Uniscribe.ScriptIsComplex(str, str.Length, (uint)Uniscribe.SCRIPT_IS_COMPLEX_FLAG.SIC_COMPLEX);

                if ((res != 0) && (res != 1))
                {
                    throw new Exception(); // Some kind of error.
                }
            }
            catch (System.Exception ex)
            {
            	Console.WriteLine(ex.Message);
            }

            if (res == 0)
                return true;

            if (res == 1)
                return false;

            return false;
        }

        public static bool ScriptShape(string str, System.Drawing.Font font, Uniscribe.SCRIPT_ITEM[] items,
             out List<ushort[]> pwOutGlyphsArray, out List<int> cGlyphsArray, out List<Uniscribe.SCRIPT_VISATTR[]> psvaArray)
        {
            pwOutGlyphsArray = new List<ushort[]>();
            cGlyphsArray = new List<int>();
            psvaArray = new List<Uniscribe.SCRIPT_VISATTR[]>();

            if (font == null)
                return false;

            bool res = false;

            // HFONT hfont = initialize your font;
            IntPtr hfont = font.ToHfont();

            IntPtr psc = IntPtr.Zero; // Initialize to NULL, will be filled lazily.

            // Don't use the last item because it is a dummy that points
            // to the end of the string.
            for (int i = 0; i < items.Length - 1; i++)
            {
                ushort[] pwOutGlyphs;
                ushort[] pwLogClust;
                Uniscribe.SCRIPT_VISATTR[] psva;

                int length = items[i + 1].iCharPos - items[i].iCharPos; // Length of this run.
                string run = str.Substring(items[i].iCharPos, length);// Beginning of this run.

                res = callScriptShapeForItem(run, length, ref hfont, ref psc, items[i].a, out pwOutGlyphs, out pwLogClust, out psva);

                if (res)
                {
                    pwOutGlyphsArray.Add(pwOutGlyphs);
                    cGlyphsArray.Add(pwOutGlyphs.Length);
                    psvaArray.Add(psva);
                }
            }

            // Need to tell Uniscribe to delete the cache we were using. If you are going
            // to keep the HFONT around, you should probably also keep the cache.
//             ScriptFreeCache(psc);

            Win32.DeleteObject(hfont);

            return res;
        }

        // Called with the array output by callScriptItemize, this will 
        private static bool callScriptShapeForItem(string input, int input_length, // IN: characters
            ref IntPtr hfont, ref IntPtr psc, // IN: font info
            Uniscribe.SCRIPT_ANALYSIS sa, // IN: from ScriptItemize
            out ushort[] pwOutGlyphs, // OUT: one per glyph
            out ushort[] pwLogClust, // OUT: one per character
            out Uniscribe.SCRIPT_VISATTR[] psva) // OUT: one per glyph
        {
            // Initial size guess for the number of glyphs recommended by Uniscribe
            int cMaxGlyphs = input_length * 3 / 2 + 16;
            pwOutGlyphs = new ushort[cMaxGlyphs];
            psva = new Uniscribe.SCRIPT_VISATTR[cMaxGlyphs];

            // The logs array is the same size as the input.
            pwLogClust = new ushort[input_length];

            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr HDC = IntPtr.Zero; // Don't give it a DC unless we have to.
            
            // HFONT old_font = NULL;
            IntPtr old_font = IntPtr.Zero;
            
            uint res = 0;

            //while (true)
            for (int max = 1000; max > 0; --max)
            {
                try
                {
                    int pcGlyphs = 0;

                    res = 0;
                    res = Uniscribe.ScriptShape(HDC, ref psc, input, input_length, cMaxGlyphs, ref sa,
                        pwOutGlyphs, pwLogClust, psva, ref pcGlyphs);

                    if (res != 0)
                    {
                        // Different types of failure...
                        if (res == Win32.E_PENDING)
                        {
                            // Need to select the font for the call. Don't do this if we don't have to
                            // since it may be slow.
                            throw new ArgumentException();
                        }
                        else if (res == Win32.E_OUTOFMEMORY)
                        {
                            // The glyph buffer needs to be larger. Just double it every time.
                            throw new OutOfMemoryException();
                        }
                        else if (res == Win32.USP_E_SCRIPT_NOT_IN_FONT)
                        {
                            // The font you selected doesn't have enough information to display
                            // what you want. You'll have to pick another one somehow...
                            // For our cases, we'll just return failure.
                            //throw new Exception(); // Some other failure.

                            sa.eScript = 0;
                            // Loop again...
                            continue;
                        }
                        else
                            throw new Exception(); // Some other failure.
                    }

                    // It worked, resize the output list to the exact number it returned.
                    ushort[] glyphs = new ushort[pcGlyphs];
                    Array.Copy(pwOutGlyphs, glyphs, pcGlyphs);
                    pwOutGlyphs = glyphs;

                    ushort[] logs = new ushort[pcGlyphs];
                    Array.Copy(pwLogClust, logs, pcGlyphs);
                    pwLogClust = logs;

                    Uniscribe.SCRIPT_VISATTR[] visattr = new Uniscribe.SCRIPT_VISATTR[pcGlyphs];
                    Array.Copy(psva, visattr, pcGlyphs);
                    psva = visattr;

                    break;
                }
                catch (ArgumentException)
                {
                    // ... select font into hdc ...

                    hdcSrc = Win32.GetDC(IntPtr.Zero);
                    HDC = Win32.CreateCompatibleDC(hdcSrc);

                    old_font = Win32.SelectObject(HDC, hfont);

                    // Loop again...
                    continue;
                }
                catch (OutOfMemoryException)
                {
                    cMaxGlyphs = pwOutGlyphs.Length * 2;
                    pwOutGlyphs = new ushort[cMaxGlyphs];
                    psva = new Uniscribe.SCRIPT_VISATTR[cMaxGlyphs];
                    // Loop again...
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }

            if (old_font != IntPtr.Zero)
                Win32.SelectObject(HDC, old_font);  // Put back the previous font.

            Win32.ReleaseDC(IntPtr.Zero, HDC);
            Win32.ReleaseDC(IntPtr.Zero, hdcSrc);

            return (res == 0);
        }

        public static bool ScriptPlace(System.Drawing.Font font, Uniscribe.SCRIPT_ITEM[] items, 
            List<ushort[]> pwGlyphs, List<int> cGlyphs, List<Uniscribe.SCRIPT_VISATTR[]> psva,
            out List<int[]> piAdvanceArray, out List<Uniscribe.GOFFSET[]> pGoffsetArray, out List<Uniscribe.ABC> pABCArray)
        {
            piAdvanceArray = new List<int[]>();
            pGoffsetArray = new List<Uniscribe.GOFFSET[]>();
            pABCArray = new List<Uniscribe.ABC>();

            if (font == null)
                return false;

            bool res = false;

            // HFONT hfont = initialize your font;
            IntPtr hfont = font.ToHfont();

            IntPtr psc = IntPtr.Zero; // Initialize to NULL, will be filled lazily.

            // Don't use the last item because it is a dummy that points
            // to the end of the string.
            for (int i = 0; i < items.Length - 1; i++)
            {
                int[] piAdvance;
                Uniscribe.GOFFSET[] pGoffset;
                Uniscribe.ABC pABC;

                Uniscribe.SCRIPT_ANALYSIS sa = items[i].a;
                res = callScriptPlaceForItem(ref hfont, ref psc, pwGlyphs[i], cGlyphs[i], psva[i], ref sa, out piAdvance, out pGoffset, out pABC);

                if (res)
                {
                    items[i].a = sa;

                    piAdvanceArray.Add(piAdvance);
                    pGoffsetArray.Add(pGoffset);
                    pABCArray.Add(pABC);
                }
            }

            // Need to tell Uniscribe to delete the cache we were using. If you are going
            // to keep the HFONT around, you should probably also keep the cache.
            //             ScriptFreeCache(psc);

            Win32.DeleteObject(hfont);

            return res;
        }

        private static bool callScriptPlaceForItem(ref IntPtr hfont, ref IntPtr psc,
            ushort[] pwGlyphs, int cGlyphs, Uniscribe.SCRIPT_VISATTR[] psva,
            ref Uniscribe.SCRIPT_ANALYSIS psa,
            out int[] piAdvance, out Uniscribe.GOFFSET[] pGoffset, out Uniscribe.ABC pABC)
        {
            // All arrays are in visual order unless the fLogicalOrder member is set 
            // in the SCRIPT_ANALYSIS structure indicated by the psa parameter.
            psa.fLogicalOrder = true;

            piAdvance = new int[cGlyphs];
            pGoffset = new Uniscribe.GOFFSET[cGlyphs];

            pABC = new Uniscribe.ABC();

            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr HDC = IntPtr.Zero; // Don't give it a DC unless we have to.

            // HFONT old_font = NULL;
            IntPtr old_font = IntPtr.Zero;

            uint res = 0;

            //while (true)
            for (int max = 1000; max > 0; --max)
            {
                try
                {
                    res = 0;
                    res = Uniscribe.ScriptPlace(HDC, ref psc, pwGlyphs, cGlyphs, psva, ref psa, piAdvance, pGoffset, ref pABC);

                    if (res != 0)
                    {
                        // Different types of failure...
                        if (res == Win32.E_PENDING)
                        {
                            // Need to select the font for the call. Don't do this if we don't have to
                            // since it may be slow.
                            throw new ArgumentException();
                        }
                        else
                            throw new Exception(); // Some other failure.
                    }

                    break;
                }
                catch (ArgumentException)
                {
                    // ... select font into hdc ...

                    hdcSrc = Win32.GetDC(IntPtr.Zero);
                    HDC = Win32.CreateCompatibleDC(hdcSrc);

                    old_font = Win32.SelectObject(HDC, hfont);

                    // Loop again...
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }

            if (old_font != IntPtr.Zero)
                Win32.SelectObject(HDC, old_font);  // Put back the previous font.

            Win32.ReleaseDC(IntPtr.Zero, HDC);
            Win32.ReleaseDC(IntPtr.Zero, hdcSrc);

            return (res == 0);
        }

        public static bool ScriptJustify(List<Uniscribe.SCRIPT_VISATTR[]> psvaArray, List<int[]> piAdvanceArray, List<int> cGlyphsArray,
            List<int> iDxArray, List<int> iMinKashidaArray, out List<int[]> piJustifyArray)
        {
            piJustifyArray = new List<int[]>();

            bool res = false;

            for (int i = 0; i < psvaArray.Count; i++)
            {
                int[] piJustify = new int[cGlyphsArray[i]];

                res = callScriptJustifyForItem(psvaArray[i], piAdvanceArray[i], cGlyphsArray[i], iDxArray[i], iMinKashidaArray[i], out piJustify);
            
                if (res)
                {
                    piJustifyArray.Add(piJustify);
                }
            }

            return res;
        }

        private static bool callScriptJustifyForItem(Uniscribe.SCRIPT_VISATTR[] psva, int[] piAdvance, int cGlyphs, 
            int iDx, int iMinKashida, out int[] piJustify)
        {
            piJustify = new int[cGlyphs];

            uint res = 0;

            try
            {
                res = 0;
                res = Uniscribe.ScriptJustify(psva, piAdvance, cGlyphs, iDx, iMinKashida, piJustify);
            }
            catch (System.Exception)
            {
            	
            }

            return (res == 0);
        }

        public static bool ScriptBreakForItem(string input, int input_length, Uniscribe.SCRIPT_ANALYSIS psa,
            out Uniscribe.SCRIPT_LOGATTR[] psla)
        {
            psla = new Uniscribe.SCRIPT_LOGATTR[input_length];

            uint res = 0;

            try
            {
                res = 0;
                res = Uniscribe.ScriptBreak(input, input_length, psa, psla);

                if (res != 0)
                {
                    psla = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return (res == 0);
        }

        public static bool ScriptTextOut(string filePathSave, System.Drawing.Font font, List<int> xArray, List<int> yArray, uint fuOptions, Win32.RECT lprc,
            List<Uniscribe.SCRIPT_ANALYSIS> psaArray, string pwcReserved, int iReserved,
            List<ushort[]> pwGlyphsArray, List<int> cGlyphsArray,
            List<int[]> piAdvanceArray, List<int[]> piJustifyArray, List<Uniscribe.GOFFSET[]> pGoffsetArray)
        {
            bool res = false;

            int width = 500;
            int height = 400;

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, width, height);
            lprc = new Win32.RECT(rect);

            Bitmap bitmap = null;
            Graphics gr = null;
            IntPtr HDC = IntPtr.Zero;
            createBitmap(font, ref bitmap, width, height, ref gr);
            HDC = gr.GetHdc();

            // HFONT hfont = initialize your font;
            IntPtr hfont = font.ToHfont();

            // HFONT old_font = NULL;
            IntPtr old_font = IntPtr.Zero;

            // ... select font into hdc ...
            old_font = Win32.SelectObject(HDC, hfont);

            int old_color = 0;
            old_color = Win32.SetTextColor(HDC, ColorTranslator.ToWin32(System.Drawing.Color.Black));
            System.Drawing.Color color = ColorTranslator.FromWin32(old_color);
            old_color = Win32.SetBkColor(HDC, ColorTranslator.ToWin32(System.Drawing.Color.Yellow));
            color = ColorTranslator.FromWin32(old_color);

            string text = "It's me!";
            Win32.RECT bounds = new Win32.RECT(rect);
            int flags = Win32.DT_CENTER | Win32.DT_VCENTER | Win32.DT_SINGLELINE;
            uint result = 0;
            result = Win32.DrawText(HDC, text, text.Length, ref bounds, flags);

            IntPtr psc = IntPtr.Zero; // Initialize to NULL, will be filled lazily.

            for (int i = 0; i < psaArray.Count; i++)
            {
                res = callScriptTextOutForItem(HDC, psc, xArray[i], yArray[i], fuOptions, lprc, psaArray[i], pwcReserved, 
                    iReserved, pwGlyphsArray[i], cGlyphsArray[i], piAdvanceArray[i], piJustifyArray[i], pGoffsetArray[i]);

                if (!res)
                {

                }
            }

            if (old_font != IntPtr.Zero)
                Win32.SelectObject(HDC, old_font);  // Put back the previous font.

            gr.ReleaseHdc(HDC);
            saveBitmap(bitmap, gr, filePathSave);

            return res;
        }

        private static void createBitmap(System.Drawing.Font font, ref Bitmap bitmap, int width, int height, ref Graphics gr)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, width, height);
            bitmap = new Bitmap(width, height);
            gr = Graphics.FromImage(bitmap);
            gr.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.MediumSeaGreen), rect);
//             SolidBrush Brush = new SolidBrush(Color.Black);
//             gr.DrawString("Watermark", font, Brush, new PointF(300, 300));
        }

        private static void saveBitmap(Bitmap bitmap, Graphics gr, string filePathSave)
        {
            bitmap.Save(filePathSave + "script_image.bmp");
        }

        private static bool callScriptTextOutForItem(IntPtr HDC, IntPtr psc, int x, int y, uint fuOptions, Win32.RECT lprc,
            Uniscribe.SCRIPT_ANALYSIS psa, string pwcReserved, int iReserved,
            ushort[] pwGlyphs, int cGlyphs,
            int[] piAdvance, int[] piJustify, Uniscribe.GOFFSET[] pGoffset)
        {
            uint res = 0;

            try
            {
                res = 0;

                res = Uniscribe.ScriptTextOut(HDC, ref psc, /*x*/0, /*y*/0, /*fuOptions*/0, 
                    /*lprc*/null, psa, /*pwcReserved*/IntPtr.Zero, /*iReserved*/0,
                    pwGlyphs, cGlyphs, piAdvance, piJustify, pGoffset);

                if (res != 0)
                {

                }
            }
            catch (System.Exception)
            {

            }

            return (res == 0);
        }

        public static bool ScriptGetFontProperties(System.Drawing.Font font, out Uniscribe.SCRIPT_FONTPROPERTIES sfp)
        {
            sfp = new Uniscribe.SCRIPT_FONTPROPERTIES();

            if (font == null)
                return false;

            // HFONT hfont = initialize your font;
            IntPtr hfont = font.ToHfont();

            IntPtr psc = IntPtr.Zero; // Initialize to NULL, will be filled lazily.

            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr HDC = IntPtr.Zero; // Don't give it a DC unless we have to.

            // HFONT old_font = NULL;
            IntPtr old_font = IntPtr.Zero;

            uint res = 0;

            sfp = new Uniscribe.SCRIPT_FONTPROPERTIES();
            sfp.cBytes = Marshal.SizeOf(typeof(Uniscribe.SCRIPT_FONTPROPERTIES));

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    res = 0;
                    res = Uniscribe.ScriptGetFontProperties(HDC, ref psc, ref sfp);

                    if (res != 0)
                    {
                        if (res == Win32.E_PENDING)
                            throw new ArgumentException();
                        else
                            throw new Exception(); // Some kind of error.
                    }

                    break;
                }
                catch (ArgumentException)
                {
                    // ... select font into hdc ...

                    hdcSrc = Win32.GetDC(IntPtr.Zero);
                    HDC = Win32.CreateCompatibleDC(hdcSrc);

                    old_font = Win32.SelectObject(HDC, hfont);

                    // Loop again...
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }

            if (old_font != IntPtr.Zero)
                Win32.SelectObject(HDC, old_font);  // Put back the previous font.

            Win32.ReleaseDC(IntPtr.Zero, HDC);
            Win32.ReleaseDC(IntPtr.Zero, hdcSrc);

            // Need to tell Uniscribe to delete the cache we were using. If you are going
            // to keep the HFONT around, you should probably also keep the cache.
            //             ScriptFreeCache(psc);

            Win32.DeleteObject(hfont);

            return (res == 0);
        }

        public static bool ScriptLayout(string str, Uniscribe.SCRIPT_ITEM[] items, 
            out int[] res_visual_to_logical, out int[] res_logical_to_visual)
        {
//             res_visual_to_logical = null;
//             res_logical_to_visual = null;

            // Output arrays
            int[] visual_to_logical;
            int[] logical_to_visual;

            // Construct the "embedding level" array for our list of runs that tell
            // Uniscribe what direction they are. Here, we do NOT count the magic last item
            // that is empty, we manually add it to the end of the lookup tables to keep
            // everything consistent (it is always at the end). I'm not sure what
            // ScriptLayout does with this item, so I prefer to handle it myself.
            byte[] directions;
            directions = new byte[items.Length];
            for (int i = 0; i < items.Length - 1; i++)
                directions[i] = items[i].a.s.uBidiLevel;

            visual_to_logical = new int[items.Length - 1];
            logical_to_visual = new int[items.Length - 1];

            uint res = 0;

            try
            {
                res = 0;
                res = Uniscribe.ScriptLayout(items.Length - 1, directions, visual_to_logical, logical_to_visual);

                if (res != 0)
                {
                    throw new Exception(); // Some kind of error.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Now add the magic last item back
            res_visual_to_logical = new int[items.Length];
            Array.Copy(visual_to_logical, res_visual_to_logical, items.Length - 1);
            res_visual_to_logical[items.Length - 1] = items.Length - 1;

            res_logical_to_visual = new int[items.Length];
            Array.Copy(logical_to_visual, res_logical_to_visual, items.Length - 1);
            res_logical_to_visual[items.Length - 1] = items.Length - 1;

            return (res == 0);
        }

        public static bool ScriptItemize(string str, out Uniscribe.SCRIPT_ITEM[] items, bool directionRightToLeft)
        {
            items = null;

            Uniscribe.SCRIPT_CONTROL psControl = new Uniscribe.SCRIPT_CONTROL();

            Uniscribe.SCRIPT_STATE psState = new Uniscribe.SCRIPT_STATE();

            // 0 means that the surrounding text is left-to-right.
            psState.uBidiLevel = (byte)((directionRightToLeft) ? 1 : 0);

            int max_items = 16;
            uint res = 0;
            for (int max = 1000; max > 0; --max)
            {
                Uniscribe.SCRIPT_ITEM[] pItems = new Uniscribe.SCRIPT_ITEM[max_items + 1];
                int pcItems = 0;

                try
                {
                    // We subtract one from max_items to work around a buffer overflow on some
                    // older versions of Windows.
                    res = 0;
                    res = Uniscribe.ScriptItemize(str, str.Length, max_items - 1, ref psControl, ref psState, pItems, ref pcItems);

                    if (res != 0)
                    {
                        if (res == Win32.E_OUTOFMEMORY)
                            throw new OutOfMemoryException();
                        else
                            throw new Exception(); // Some kind of error.
                    }

                    // It generated some items, so resize the array. Note that we add
                    // one to account for the magic last item.
                    pcItems += 1; // pcItems doesn't include the terminal item by default

                    items = new Uniscribe.SCRIPT_ITEM[pcItems];
                    Array.Copy(pItems, items, pcItems);

                    break;
                }
                catch (OutOfMemoryException)
                {
                    // The input array isn't big enough, double and loop again.
                	// thrown by ScriptItemize if items wasn't large enough
                    max_items *= 2;
                    continue;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
            
            return (res == 0);
        }

    }
}
