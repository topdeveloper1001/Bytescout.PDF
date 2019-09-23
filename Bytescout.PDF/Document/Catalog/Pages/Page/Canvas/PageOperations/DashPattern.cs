using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class DashPattern
#else
	/// <summary>
    /// Represents class for a line dash pattern. The line dash pattern controls
    /// the pattern of dashes and gaps used to stroke paths.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class DashPattern
#endif
	{
	    private float[] _dashArray;
	    private float _dashPhase;

	    /// <summary>
	    /// Gets the distance from the start of a line to the beginning of the dash pattern.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float Phase
	    {
		    get
		    {
			    return _dashPhase;
		    }
		    set
		    {
			    _dashPhase = value;
		    }
	    }

	    /// <summary>
        /// Initializes a new empty instance of the Bytescout.PDF.DashPattern class.
        /// </summary>
        public DashPattern()
        {
            _dashArray = new float[0];
            _dashPhase = 0;
        }

        /// <summary>
        /// Initializes a new empty instance of the Bytescout.PDF.DashPattern class.
        /// </summary>
        /// <param name="dashArray" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The pattern (the array of numbers that specifies the lengths of alternating dashes and gaps).</param>
        /// <param name="dashPhase" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The distance from the start of a line to the beginning of the dash pattern.</param>
        public DashPattern(float[] dashArray, float dashPhase)
        {
            if (dashArray == null)
                throw new ArgumentNullException();

            _dashArray = new float[dashArray.Length];
            for (int i = 0; i < _dashArray.Length; ++i)
                if (dashArray[i] >= 0)
                    _dashArray[i] = dashArray[i];
                else
                    _dashArray[i] = 0;
            _dashPhase = dashPhase;
        }

        /// <summary>
        /// Initializes a new empty instance of the Bytescout.PDF.DashPattern class.
        /// </summary>
        /// <param name="dashArray" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The pattern (the array of numbers that specifies
        /// the lengths of alternating dashes and gaps).</param>
        public DashPattern(float[] dashArray)
            : this(dashArray, 0) { }

	    /// <summary>
        /// Gets the pattern (the array of numbers that specifies the lengths of alternating dashes and gaps).
        /// </summary>
        /// <returns cref="!:float[]" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The pattern (the array of numbers that specifies the lengths of alternating dashes and gaps).</returns>
        public float[] GetPattern()
        {
            return (float[])_dashArray.Clone();
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current Bytescout.PDF.DashPattern.
        /// </summary>
        /// <param name="obj" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">The System.Object to compare with the current Bytescout.PDF.DashPattern.</param>
        /// <returns cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">true if the specified System.Object is equal to the current Bytescout.PDF.DashPattern; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is DashPattern)
            { 
                DashPattern dash = obj as DashPattern;
                if (_dashPhase != dash._dashPhase)
                    return false;
                if (_dashArray.Length != dash._dashArray.Length)
                    return false;
                for (int i = 0; i < _dashArray.Length; ++i)
                    if (_dashArray[i] != dash._dashArray[i])
                        return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">A hash code for the current Bytescout.PDF.DashPattern.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
