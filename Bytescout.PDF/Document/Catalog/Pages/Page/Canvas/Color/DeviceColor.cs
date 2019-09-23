using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class DeviceColor : Color
#else
	/// <summary>
    /// Represents an abstract class for color in a Device color space.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class DeviceColor : Color
#endif
	{
    }
}
