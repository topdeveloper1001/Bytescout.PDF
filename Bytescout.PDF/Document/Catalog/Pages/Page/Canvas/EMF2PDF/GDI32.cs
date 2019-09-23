using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
    internal abstract class GDI32
    {
        public const uint ENHMETA_SIGNATURE = 0x464D4520;
        public const uint EMR_HEADER = 0x00000001;
        public const uint EMR_ARCTO = 0x00000037;
        public const uint EMR_EOF = 0x0000000E;

        [StructLayout(LayoutKind.Sequential)]
        public struct EMR
        {
            public uint iType;
            public uint nSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EMRARCTO
        {
            public EMR emr;
            public RECTL rclBox;
            public POINTL ptlStart;
            public POINTL ptlEnd;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECTL
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZEL
        {
            public int cx;
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTL
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ENHMETAHEADER
        {
            public uint iType;
            public uint nSize;
            public RECTL rclBounds;
            public RECTL rclFrame;
            public uint dSignature;
            public uint nVersion;
            public uint nBytes;
            public uint nRecords;
            public ushort nHandles;
            public ushort sReserved;
            public uint nDescription;
            public uint offDescription;
            public uint nPalEntries;
            public SIZEL szlDevice;
            public SIZEL szlMillimeters;
            public uint cbPixelFormat;
            public uint offPixelFormat;
            public uint bOpenGL;
            public SIZEL szlMicrometers;
        }

        public delegate int EnhMetaFileDelegate(IntPtr hdc, IntPtr lpHTable, IntPtr lpEFMR, int nObj, IntPtr lpData);

        [DllImport("gdi32.dll")]
        public static extern uint GetEnhMetaFileHeader(IntPtr hemf, uint cbBuffer, IntPtr lpemh);
        [DllImport("gdi32.dll", EntryPoint = "EnumEnhMetaFile")]
        public static extern bool EnumEnhMetaFile(IntPtr hdc, IntPtr hemf, EnhMetaFileDelegate lpEnhMetaFunc, IntPtr lpData, IntPtr lpRect);
        [DllImport("gdi32.dll", EntryPoint = "GetEnhMetaFile")]
        public static extern IntPtr GetEnhMetaFile(string lpszMetaFile);
        [DllImport("gdi32.dll", EntryPoint = "DeleteEnhMetaFile")]
        public static extern bool DeleteEnhMetaFile(IntPtr hemf);
    }
}
