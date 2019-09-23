using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
    internal class EMF2PDF
    {
	    private GraphicsTemplate _graphicsTemplate;

	    public GraphicsTemplate GraphicsTemplate
	    {
		    get { return _graphicsTemplate; }
	    }

	    public EMF2PDF (string filename)
        {
            IntPtr hEmf = GDI32.GetEnhMetaFile(filename);
            if ((int)hEmf != 0)
            {
                int sizeHeader = Marshal.SizeOf(typeof(GDI32.ENHMETAHEADER));
                IntPtr emfHeader = Marshal.AllocHGlobal(sizeHeader);
                GDI32.GetEnhMetaFileHeader(hEmf, (uint)sizeHeader, emfHeader);
                GDI32.ENHMETAHEADER enhMetaHeader = (GDI32.ENHMETAHEADER)Marshal.PtrToStructure(emfHeader, typeof(GDI32.ENHMETAHEADER));
                Marshal.FreeHGlobal(emfHeader);
                //emfHeader = new IntPtr((int)emfHeader + 4);
                GDI32.EnumEnhMetaFile(IntPtr.Zero, hEmf, EnhMetaFileProc, IntPtr.Zero, IntPtr.Zero);
            }
        }

	    private int EnhMetaFileProc(IntPtr hdc, IntPtr lpHTable, IntPtr lpEFMR, int nObj, IntPtr lpData)
        {
            GDI32.EMR emr = (GDI32.EMR)Marshal.PtrToStructure(lpEFMR, typeof(GDI32.EMR));
            switch (emr.iType)
            { 
                case GDI32.EMR_HEADER:
                    GDI32.ENHMETAHEADER emh = (GDI32.ENHMETAHEADER)Marshal.PtrToStructure(lpEFMR, typeof(GDI32.ENHMETAHEADER));
                    uint width = (uint)((float)Math.Abs(emh.rclFrame.right - emh.rclFrame.left) * (float)emh.szlDevice.cx * 10.0f / ((float)emh.szlMillimeters.cx * 1000.0f));
                    uint height = (uint)((float)Math.Abs(emh.rclFrame.bottom - emh.rclFrame.top) * (float)emh.szlDevice.cy * 10.0f / ((float)emh.szlMillimeters.cy * 1000.0f));
                    _graphicsTemplate = new GraphicsTemplate(width, height);
                    break;
                case GDI32.EMR_ARCTO:
                    GDI32.EMRARCTO emrArc = (GDI32.EMRARCTO)Marshal.PtrToStructure(lpEFMR, typeof(GDI32.EMRARCTO));
                    _graphicsTemplate.DrawArc(new SolidPen(), (float)(emrArc.rclBox.right - emrArc.rclBox.left) / 2, (float)(emrArc.rclBox.bottom - emrArc.rclBox.top) / 2, (float)(emrArc.rclBox.right - emrArc.rclBox.left) / 2, (float)(emrArc.rclBox.bottom - emrArc.rclBox.top) / 2, 0, 270);
                    break;
                case GDI32.EMR_EOF:
                    break;
            }
            return 1;
        }
    }
}
