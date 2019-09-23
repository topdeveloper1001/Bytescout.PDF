using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JPXConsts
    {
        internal static int[, , ,] SigPropContext = {
                                                      {{{ 0, 0, 0 },   // horiz=0, vert=0, diag=0
                                                        { 1, 1, 3 },   // horiz=0, vert=0, diag=1
                                                        { 2, 2, 6 },   // horiz=0, vert=0, diag=2
                                                        { 2, 2, 8 },   // horiz=0, vert=0, diag=3
                                                        { 2, 2, 8 }},  // horiz=0, vert=0, diag=4
                                                       {{ 5, 3, 1 },   // horiz=0, vert=1, diag=0
                                                        { 6, 3, 4 },   // horiz=0, vert=1, diag=1
                                                        { 6, 3, 7 },   // horiz=0, vert=1, diag=2
                                                        { 6, 3, 8 },   // horiz=0, vert=1, diag=3
                                                        { 6, 3, 8 }},  // horiz=0, vert=1, diag=4
                                                       {{ 8, 4, 2 },   // horiz=0, vert=2, diag=0
                                                        { 8, 4, 5 },   // horiz=0, vert=2, diag=1
                                                        { 8, 4, 7 },   // horiz=0, vert=2, diag=2
                                                        { 8, 4, 8 },   // horiz=0, vert=2, diag=3
                                                        { 8, 4, 8 }}}, // horiz=0, vert=2, diag=4
                                                      {{{ 3, 5, 1 },   // horiz=1, vert=0, diag=0
                                                        { 3, 6, 4 },   // horiz=1, vert=0, diag=1
                                                        { 3, 6, 7 },   // horiz=1, vert=0, diag=2
                                                        { 3, 6, 8 },   // horiz=1, vert=0, diag=3
                                                        { 3, 6, 8 }},  // horiz=1, vert=0, diag=4
                                                       {{ 7, 7, 2 },   // horiz=1, vert=1, diag=0
                                                        { 7, 7, 5 },   // horiz=1, vert=1, diag=1
                                                        { 7, 7, 7 },   // horiz=1, vert=1, diag=2
                                                        { 7, 7, 8 },   // horiz=1, vert=1, diag=3
                                                        { 7, 7, 8 }},  // horiz=1, vert=1, diag=4
                                                       {{ 8, 7, 2 },   // horiz=1, vert=2, diag=0
                                                        { 8, 7, 5 },   // horiz=1, vert=2, diag=1
                                                        { 8, 7, 7 },   // horiz=1, vert=2, diag=2
                                                        { 8, 7, 8 },   // horiz=1, vert=2, diag=3
                                                        { 8, 7, 8 }}}, // horiz=1, vert=2, diag=4
                                                      {{{ 4, 8, 2 },   // horiz=2, vert=0, diag=0
                                                        { 4, 8, 5 },   // horiz=2, vert=0, diag=1
                                                        { 4, 8, 7 },   // horiz=2, vert=0, diag=2
                                                        { 4, 8, 8 },   // horiz=2, vert=0, diag=3
                                                        { 4, 8, 8 }},  // horiz=2, vert=0, diag=4
                                                       {{ 7, 8, 2 },   // horiz=2, vert=1, diag=0
                                                        { 7, 8, 5 },   // horiz=2, vert=1, diag=1
                                                        { 7, 8, 7 },   // horiz=2, vert=1, diag=2
                                                        { 7, 8, 8 },   // horiz=2, vert=1, diag=3
                                                        { 7, 8, 8 }},  // horiz=2, vert=1, diag=4
                                                       {{ 8, 8, 2 },   // horiz=2, vert=2, diag=0
                                                        { 8, 8, 5 },   // horiz=2, vert=2, diag=1
                                                        { 8, 8, 7 },   // horiz=2, vert=2, diag=2
                                                        { 8, 8, 8 },   // horiz=2, vert=2, diag=3
                                                        { 8, 8, 8 }}}  // horiz=2, vert=2, diag=4
                                                    };
        internal static int[, ,] SignContext = {
                                                      {{ 13, 1 },  // horiz=-2, vert=-2
                                                       { 13, 1 },  // horiz=-2, vert=-1
                                                       { 12, 1 },  // horiz=-2, vert= 0
                                                       { 11, 1 },  // horiz=-2, vert=+1
                                                       { 11, 1 }}, // horiz=-2, vert=+2
                                                      {{ 13, 1 },  // horiz=-1, vert=-2
                                                       { 13, 1 },  // horiz=-1, vert=-1
                                                       { 12, 1 },  // horiz=-1, vert= 0
                                                       { 11, 1 },  // horiz=-1, vert=+1
                                                       { 11, 1 }}, // horiz=-1, vert=+2
                                                      {{ 10, 1 },  // horiz= 0, vert=-2
                                                       { 10, 1 },  // horiz= 0, vert=-1
                                                       {  9, 0 },  // horiz= 0, vert= 0
                                                       { 10, 0 },  // horiz= 0, vert=+1
                                                       { 10, 0 }}, // horiz= 0, vert=+2
                                                      {{ 11, 0 },  // horiz=+1, vert=-2
                                                       { 11, 0 },  // horiz=+1, vert=-1
                                                       { 12, 0 },  // horiz=+1, vert= 0
                                                       { 13, 0 },  // horiz=+1, vert=+1
                                                       { 13, 0 }}, // horiz=+1, vert=+2
                                                      {{ 11, 0 },  // horiz=+2, vert=-2
                                                       { 11, 0 },  // horiz=+2, vert=-1
                                                       { 12, 0 },  // horiz=+2, vert= 0
                                                       { 13, 0 },  // horiz=+2, vert=+1
                                                       { 13, 0 }}, // horiz=+2, vert=+2
                                                    };
        internal static double IDWTAlpha = -1.586134342059924;
        internal static double IDWTBeta = -0.052980118572961;
        internal static double IDWTGamma = 0.882911075530934;
        internal static double IDWTDelta = 0.443506852043971;
        internal static double IDWTKappa = 1.230174104914001;
        internal static double IDWTIKappa = (1.0 / IDWTKappa);
    }
}
