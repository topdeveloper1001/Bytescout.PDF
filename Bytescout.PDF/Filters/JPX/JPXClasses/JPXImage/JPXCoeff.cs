using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal enum CoefficientFlags
    {
        jpxCoeffSignificantB = 0,
        jpxCoeffTouchedB = 1,
        jpxCoeffFirstMagRefB = 2,
        jpxCoeffSignB = 7,
        jpxCoeffSignificant = (1 << jpxCoeffSignificantB),
        jpxCoeffTouched = (1 << jpxCoeffTouchedB),
        jpxCoeffFirstMagRef = (1 << jpxCoeffFirstMagRefB),
        jpxCoeffSign = (1 << jpxCoeffSignB)
    }

    internal struct JPXCoeff
    {
        public short Flags;
        public short Length;		
        public int Magnitude;		
    }
}
