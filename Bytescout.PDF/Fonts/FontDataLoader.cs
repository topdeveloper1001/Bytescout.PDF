using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

namespace Bytescout.PDF
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct LOGFONT
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName;
    }

    internal class FontDataLoader
    {
	    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	    private struct ENUMLOGFONTEX
	    {
		    public LOGFONT elfLogFont;
		    //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		    public string elfFullName;
		    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		    public string elfStyle;
		    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		    public string elfScript;
	    }

	    private class LOGFONTFinder
	    {
		    private readonly List<LOGFONT> _fonts;

		    public LOGFONTFinder()
		    {
			    _fonts = new List<LOGFONT>();
		    }

		    public int EnumFontFamExProc(ref ENUMLOGFONTEX lpelfe, IntPtr lpntme, int FontType, IntPtr lParam)
		    {
			    try
			    {
				    _fonts.Add(lpelfe.elfLogFont);
			    }
			    catch (Exception e)
			    {
				    System.Diagnostics.Trace.WriteLine(e.ToString());
			    }
			    return 0;
		    }

		    public LOGFONT Find(LOGFONT simple, out bool find)
		    {
			    if (_fonts.Count == 0)
			    {
				    find = false;
				    return new LOGFONT();
			    }

			    find = true;
			    if (_fonts.Count == 1)
				    return _fonts[0];

			    int index = findBoldItalic(simple.lfWeight, simple.lfItalic);
			    if (index >= 0)
				    return _fonts[index];

			    index = findItalic(simple.lfItalic);
			    if (index >= 0)
				    return _fonts[index];

			    index = findBold(simple.lfWeight);
			    if (index >= 0)
				    return _fonts[index];

			    return _fonts[0];
		    }

		    private int findBoldItalic(int weight, byte italic)
		    {
			    for (int i = 0; i < _fonts.Count; ++i)
			    {
				    if (_fonts[i].lfItalic == italic && _fonts[i].lfWeight == weight)
					    return i;
			    }

			    return -1;
		    }

		    private int findItalic(byte italic)
		    {
			    for (int i = 0; i < _fonts.Count; ++i)
			    {
				    if (_fonts[i].lfItalic == italic)
					    return i;
			    }

			    return -1;
		    }

		    private int findBold(int weight)
		    {
			    for (int i = 0; i < _fonts.Count; ++i)
			    {
				    if (_fonts[i].lfWeight >= weight)
					    return i;
			    }

			    return -1;
		    }
	    }

	    public static byte[] LoadFontFromFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] buf = new byte[fs.Length];
            fs.Read(buf, 0, buf.Length);
            fs.Close();
            return buf;
        }

        public static byte[] LoadFont(LOGFONT lf, out uint ttcSize)
        {
            IntPtr hfont = CreateFontIndirectW(ref lf);
            IntPtr HDC = Win32.GetDC(IntPtr.Zero);
            IntPtr oldHfont = Win32.SelectObject(HDC, hfont);

            uint sizettc = GetFontDataSize(HDC, 0x66637474, 0, IntPtr.Zero, 0);
            uint size = GetFontDataSize(HDC, 0, 0, IntPtr.Zero, 0);
            if (sizettc != uint.MaxValue)
                ttcSize = sizettc - size;
            else
                ttcSize = 0;

            if (size == uint.MaxValue)
            {
                Win32.ReleaseDC(IntPtr.Zero, HDC);
                Win32.DeleteObject(hfont);
                Win32.DeleteObject(oldHfont);
                throw new PDFUnableLoadFontException();
            }

            byte[] buf = new byte[size];
            GetFontData(HDC, 0, 0, buf, (int)size);

            Win32.ReleaseDC(IntPtr.Zero, HDC);
            Win32.DeleteObject(hfont);
            Win32.DeleteObject(oldHfont);
            return buf;
        }

	    public static LOGFONT GetLOGFONT(string fontName, bool bold, bool italic)
        {
            if (string.IsNullOrEmpty(fontName))
                fontName = "Arial";

            LOGFONT lf = new LOGFONT();
            lf.lfCharSet = 1;
            lf.lfFaceName = fontName;
            if (italic)
                lf.lfItalic = 255;
            if (bold)
                lf.lfWeight = 700;
            else
                lf.lfWeight = 400;

            IntPtr plogFont = Marshal.AllocHGlobal(Marshal.SizeOf(lf));
            Marshal.StructureToPtr(lf, plogFont, true);

            LOGFONTFinder finder = new LOGFONTFinder();

            try
            {
                IntPtr HDC = Win32.GetDC(IntPtr.Zero);
                EnumFontExDelegate del = new EnumFontExDelegate(finder.EnumFontFamExProc);

                int ret = 0;
                ret = EnumFontFamiliesEx(HDC, plogFont, del, IntPtr.Zero, 0);
                //System.Diagnostics.Trace.WriteLine("EnumFontFamiliesEx = " + ret.ToString());

                Win32.ReleaseDC(IntPtr.Zero, HDC);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            finally
            {
                Marshal.DestroyStructure(plogFont, typeof(LOGFONT));
            }

            bool find;
            LOGFONT result = finder.Find(lf, out find);
            if (find)
                return result;
            else
            {
                if (fontName.Equals("Arial"))
                    throw new PDFException("System fonts are not available.");
            }
            
            return GetLOGFONT("Arial", bold, italic);
        }

        //does not work with [DllImport("gdi32.dll", EntryPoint = "CreateFontIndirect")]
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateFontIndirectW([In] ref LOGFONT logfont);

        [DllImport("gdi32.dll", EntryPoint = "GetFontData")]
        private static extern uint GetFontData(IntPtr hdc, int dwTable, int dwOffset, [In, MarshalAs(UnmanagedType.LPArray)] byte[] data, int cbData);

        [DllImport("gdi32.dll", EntryPoint = "GetFontData")]
        private static extern uint GetFontDataSize(IntPtr hdc, int dwTable, int dwOffset, IntPtr data, int cbData);

        //does not work with [DllImport("gdi32.dll", EntryPoint = "EnumFontFamiliesEx")]
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        private static extern int EnumFontFamiliesEx(IntPtr hdc, [In] IntPtr lpLogfont, EnumFontExDelegate lpEnumFontFamExProc, IntPtr lParam, uint dwFlags);

        private delegate int EnumFontExDelegate(ref ENUMLOGFONTEX lpelfe, IntPtr lpntme, int FontType, IntPtr lParam);
        
        //[StructLayout(LayoutKind.Sequential)]
    }
}
