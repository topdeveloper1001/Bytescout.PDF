using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
    internal static class Win32
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool /*IntPtr*/ ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint DrawText(IntPtr hdc, string lpStr, int nCount, ref Win32.RECT lpRect, int wFormat);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        internal static extern int SetTextColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll")]
        internal static extern int SetBkColor(IntPtr hdc, int crColor);

        internal const uint S_OK            = 0x00000000; // Operation successful
        internal const uint E_ABORT         = 0x80004004; // Operation aborted
        internal const uint E_ACCESSDENIED  = 0x80070005; // General access denied error
        internal const uint E_FAIL          = 0x80004005; // Unspecified failure
        internal const uint E_HANDLE        = 0x80070006; // Handle that is not valid
        internal const uint E_INVALIDARG    = 0x80070057; // One or more arguments are not valid
        internal const uint E_NOINTERFACE   = 0x80004002; // No such interface supported
        internal const uint E_NOTIMPL       = 0x80004001; // Not implemented
        internal const uint E_OUTOFMEMORY   = 0x8007000E; // Failed to allocate necessary memory
        internal const uint E_POINTER       = 0x80004003; // Pointer that is not valid
        internal const uint E_UNEXPECTED    = 0x8000FFFF; // Unexpected failure

        internal const uint E_PENDING       = 0x8000000A; // The data necessary to complete this operation is not yet available.
        
        internal const uint USP_E_SCRIPT_NOT_IN_FONT = (uint)((((uint)(1) << 31) | ((uint)(4) << 16) | ((uint)(0x200)))); // Script doesn't exist in font

        internal const uint S_FALSE         = 0x00000001; // Which is returned from functions that were successful, but in a different way than expected

        internal const int DT_CENTER        = 0x1;
        internal const int DT_VCENTER       = 0x4;
        internal const int DT_SINGLELINE    = 0x20;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(System.Drawing.Rectangle rect)
            {
                left = rect.Left;
                top = rect.Top;
                right = rect.Right;
                bottom = rect.Bottom;
            }
        }

    }
}
