using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JBIG2HuffmanTable
    {
        public int val;
        public uint prefixLen;
        public uint rangeLen;		// can also be LOW, OOB, or EOT
        public uint prefix;

        public JBIG2HuffmanTable(int val, uint prefixLen, uint rangeLen, uint prefix)
        {
            this.val = val;
            this.prefixLen = prefixLen;
            this.rangeLen = rangeLen;
            this.prefix = prefix;
        }
    }

    internal class JBIG2HuffmanTables
    {
        public static uint jbig2HuffmanLOW = 0xfffffffd;
        public static uint jbig2HuffmanOOB = 0xfffffffe;
        public static uint jbig2HuffmanEOT = 0xffffffff;

        //public static int jbig2HuffmanLOW = (int)_jbig2HuffmanLOW;
        //public static int jbig2HuffmanOOB = (int)_jbig2HuffmanOOB;
        //public static int jbig2HuffmanEOT = (int)_jbig2HuffmanEOT;

        public static JBIG2HuffmanTable[] huffTableA = {
          new JBIG2HuffmanTable(     0, 1,  4,              0x000 ),
          new JBIG2HuffmanTable(    16, 2,  8,              0x002 ),
          new JBIG2HuffmanTable(   272, 3, 16,              0x006 ),
          new JBIG2HuffmanTable( 65808, 3, 32,              0x007 ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
        };

        public static JBIG2HuffmanTable[] huffTableB = {
          new JBIG2HuffmanTable(     0, 1,  0,              0x000 ),
          new JBIG2HuffmanTable(     1, 2,  0,              0x002 ),
          new JBIG2HuffmanTable(     2, 3,  0,              0x006 ),
          new JBIG2HuffmanTable(     3, 4,  3,              0x00e ),
          new JBIG2HuffmanTable(    11, 5,  6,              0x01e ),
          new JBIG2HuffmanTable(    75, 6, 32,              0x03e ),
          new JBIG2HuffmanTable(     0, 6, jbig2HuffmanOOB, 0x03f ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
        };

        public static JBIG2HuffmanTable[] huffTableC = {
          new JBIG2HuffmanTable(     0, 1,  0,              0x000 ),
          new JBIG2HuffmanTable(     1, 2,  0,              0x002 ),
          new JBIG2HuffmanTable(     2, 3,  0,              0x006 ),
          new JBIG2HuffmanTable(     3, 4,  3,              0x00e ),
          new JBIG2HuffmanTable(    11, 5,  6,              0x01e ),
          new JBIG2HuffmanTable(     0, 6, jbig2HuffmanOOB, 0x03e ),
          new JBIG2HuffmanTable(    75, 7, 32,              0x0fe ),
          new JBIG2HuffmanTable(  -256, 8,  8,              0x0fe ),
          new JBIG2HuffmanTable(  -257, 8, jbig2HuffmanLOW, 0x0ff ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableD = {
          new JBIG2HuffmanTable(     1, 1,  0,              0x000 ),
          new JBIG2HuffmanTable(     2, 2,  0,              0x002 ),
          new JBIG2HuffmanTable(     3, 3,  0,              0x006 ),
          new JBIG2HuffmanTable(     4, 4,  3,              0x00e ),
          new JBIG2HuffmanTable(    12, 5,  6,              0x01e ),
          new JBIG2HuffmanTable(    76, 5, 32,              0x01f ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableE = {
          new JBIG2HuffmanTable(     1, 1,  0,              0x000 ),
          new JBIG2HuffmanTable(     2, 2,  0,              0x002 ),
          new JBIG2HuffmanTable(     3, 3,  0,              0x006 ),
          new JBIG2HuffmanTable(     4, 4,  3,              0x00e ),
          new JBIG2HuffmanTable(    12, 5,  6,              0x01e ),
          new JBIG2HuffmanTable(    76, 6, 32,              0x03e ),
          new JBIG2HuffmanTable(  -255, 7,  8,              0x07e ),
          new JBIG2HuffmanTable(  -256, 7, jbig2HuffmanLOW, 0x07f ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableF = {
          new JBIG2HuffmanTable(     0, 2,  7,              0x000 ),
          new JBIG2HuffmanTable(   128, 3,  7,              0x002 ),
          new JBIG2HuffmanTable(   256, 3,  8,              0x003 ),
          new JBIG2HuffmanTable( -1024, 4,  9,              0x008 ),
          new JBIG2HuffmanTable(  -512, 4,  8,              0x009 ),
          new JBIG2HuffmanTable(  -256, 4,  7,              0x00a ),
          new JBIG2HuffmanTable(   -32, 4,  5,              0x00b ),
          new JBIG2HuffmanTable(   512, 4,  9,              0x00c ),
          new JBIG2HuffmanTable(  1024, 4, 10,              0x00d ),
          new JBIG2HuffmanTable( -2048, 5, 10,              0x01c ),
          new JBIG2HuffmanTable(  -128, 5,  6,              0x01d ),
          new JBIG2HuffmanTable(   -64, 5,  5,              0x01e ),
          new JBIG2HuffmanTable( -2049, 6, jbig2HuffmanLOW, 0x03e ),
          new JBIG2HuffmanTable(  2048, 6, 32,              0x03f ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableG = {
          new JBIG2HuffmanTable(  -512, 3,  8,              0x000 ),
          new JBIG2HuffmanTable(   256, 3,  8,              0x001 ),
          new JBIG2HuffmanTable(   512, 3,  9,              0x002 ),
          new JBIG2HuffmanTable(  1024, 3, 10,              0x003 ),
          new JBIG2HuffmanTable( -1024, 4,  9,              0x008 ),
          new JBIG2HuffmanTable(  -256, 4,  7,              0x009 ),
          new JBIG2HuffmanTable(   -32, 4,  5,              0x00a ),
          new JBIG2HuffmanTable(     0, 4,  5,              0x00b ),
          new JBIG2HuffmanTable(   128, 4,  7,              0x00c ),
          new JBIG2HuffmanTable(  -128, 5,  6,              0x01a ),
          new JBIG2HuffmanTable(   -64, 5,  5,              0x01b ),
          new JBIG2HuffmanTable(    32, 5,  5,              0x01c ),
          new JBIG2HuffmanTable(    64, 5,  6,              0x01d ),
          new JBIG2HuffmanTable( -1025, 5, jbig2HuffmanLOW, 0x01e ),
          new JBIG2HuffmanTable(  2048, 5, 32,              0x01f ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableH = {
          new JBIG2HuffmanTable(     0, 2,  1,              0x000 ),
          new JBIG2HuffmanTable(     0, 2, jbig2HuffmanOOB, 0x001 ),
          new JBIG2HuffmanTable(     4, 3,  4,              0x004 ),
          new JBIG2HuffmanTable(    -1, 4,  0,              0x00a ),
          new JBIG2HuffmanTable(    22, 4,  4,              0x00b ),
          new JBIG2HuffmanTable(    38, 4,  5,              0x00c ),
          new JBIG2HuffmanTable(     2, 5,  0,              0x01a ),
          new JBIG2HuffmanTable(    70, 5,  6,              0x01b ),
          new JBIG2HuffmanTable(   134, 5,  7,              0x01c ),
          new JBIG2HuffmanTable(     3, 6,  0,              0x03a ),
          new JBIG2HuffmanTable(    20, 6,  1,              0x03b ),
          new JBIG2HuffmanTable(   262, 6,  7,              0x03c ),
          new JBIG2HuffmanTable(   646, 6, 10,              0x03d ),
          new JBIG2HuffmanTable(    -2, 7,  0,              0x07c ),
          new JBIG2HuffmanTable(   390, 7,  8,              0x07d ),
          new JBIG2HuffmanTable(   -15, 8,  3,              0x0fc ),
          new JBIG2HuffmanTable(    -5, 8,  1,              0x0fd ),
          new JBIG2HuffmanTable(    -7, 9,  1,              0x1fc ),
          new JBIG2HuffmanTable(    -3, 9,  0,              0x1fd ),
          new JBIG2HuffmanTable(   -16, 9, jbig2HuffmanLOW, 0x1fe ),
          new JBIG2HuffmanTable(  1670, 9, 32,              0x1ff ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableI = {
          new JBIG2HuffmanTable(     0, 2, jbig2HuffmanOOB, 0x000 ),
          new JBIG2HuffmanTable(    -1, 3,  1,              0x002 ),
          new JBIG2HuffmanTable(     1, 3,  1,              0x003 ),
          new JBIG2HuffmanTable(     7, 3,  5,              0x004 ),
          new JBIG2HuffmanTable(    -3, 4,  1,              0x00a ),
          new JBIG2HuffmanTable(    43, 4,  5,              0x00b ),
          new JBIG2HuffmanTable(    75, 4,  6,              0x00c ),
          new JBIG2HuffmanTable(     3, 5,  1,              0x01a ),
          new JBIG2HuffmanTable(   139, 5,  7,              0x01b ),
          new JBIG2HuffmanTable(   267, 5,  8,              0x01c ),
          new JBIG2HuffmanTable(     5, 6,  1,              0x03a ),
          new JBIG2HuffmanTable(    39, 6,  2,              0x03b ),
          new JBIG2HuffmanTable(   523, 6,  8,              0x03c ),
          new JBIG2HuffmanTable(  1291, 6, 11,              0x03d ),
          new JBIG2HuffmanTable(    -5, 7,  1,              0x07c ),
          new JBIG2HuffmanTable(   779, 7,  9,              0x07d ),
          new JBIG2HuffmanTable(   -31, 8,  4,              0x0fc ),
          new JBIG2HuffmanTable(   -11, 8,  2,              0x0fd ),
          new JBIG2HuffmanTable(   -15, 9,  2,              0x1fc ),
          new JBIG2HuffmanTable(    -7, 9,  1,              0x1fd ),
          new JBIG2HuffmanTable(   -32, 9, jbig2HuffmanLOW, 0x1fe ),
          new JBIG2HuffmanTable(  3339, 9, 32,              0x1ff ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableJ = {
          new JBIG2HuffmanTable(    -2, 2,  2,              0x000 ),
          new JBIG2HuffmanTable(     6, 2,  6,              0x001 ),
          new JBIG2HuffmanTable(     0, 2, jbig2HuffmanOOB, 0x002 ),
          new JBIG2HuffmanTable(    -3, 5,  0,              0x018 ),
          new JBIG2HuffmanTable(     2, 5,  0,              0x019 ),
          new JBIG2HuffmanTable(    70, 5,  5,              0x01a ),
          new JBIG2HuffmanTable(     3, 6,  0,              0x036 ),
          new JBIG2HuffmanTable(   102, 6,  5,              0x037 ),
          new JBIG2HuffmanTable(   134, 6,  6,              0x038 ),
          new JBIG2HuffmanTable(   198, 6,  7,              0x039 ),
          new JBIG2HuffmanTable(   326, 6,  8,              0x03a ),
          new JBIG2HuffmanTable(   582, 6,  9,              0x03b ),
          new JBIG2HuffmanTable(  1094, 6, 10,              0x03c ),
          new JBIG2HuffmanTable(   -21, 7,  4,              0x07a ),
          new JBIG2HuffmanTable(    -4, 7,  0,              0x07b ),
          new JBIG2HuffmanTable(     4, 7,  0,              0x07c ),
          new JBIG2HuffmanTable(  2118, 7, 11,              0x07d ),
          new JBIG2HuffmanTable(    -5, 8,  0,              0x0fc ),
          new JBIG2HuffmanTable(     5, 8,  0,              0x0fd ),
          new JBIG2HuffmanTable(   -22, 8, jbig2HuffmanLOW, 0x0fe ),
          new JBIG2HuffmanTable(  4166, 8, 32,              0x0ff ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableK = {
          new JBIG2HuffmanTable(     1, 1,  0,              0x000 ),
          new JBIG2HuffmanTable(     2, 2,  1,              0x002 ),
          new JBIG2HuffmanTable(     4, 4,  0,              0x00c ),
          new JBIG2HuffmanTable(     5, 4,  1,              0x00d ),
          new JBIG2HuffmanTable(     7, 5,  1,              0x01c ),
          new JBIG2HuffmanTable(     9, 5,  2,              0x01d ),
          new JBIG2HuffmanTable(    13, 6,  2,              0x03c ),
          new JBIG2HuffmanTable(    17, 7,  2,              0x07a ),
          new JBIG2HuffmanTable(    21, 7,  3,              0x07b ),
          new JBIG2HuffmanTable(    29, 7,  4,              0x07c ),
          new JBIG2HuffmanTable(    45, 7,  5,              0x07d ),
          new JBIG2HuffmanTable(    77, 7,  6,              0x07e ),
          new JBIG2HuffmanTable(   141, 7, 32,              0x07f ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableL = {
          new JBIG2HuffmanTable(     1, 1,  0,              0x000 ),
          new JBIG2HuffmanTable(     2, 2,  0,              0x002 ),
          new JBIG2HuffmanTable(     3, 3,  1,              0x006 ),
          new JBIG2HuffmanTable(     5, 5,  0,              0x01c ),
          new JBIG2HuffmanTable(     6, 5,  1,              0x01d ),
          new JBIG2HuffmanTable(     8, 6,  1,              0x03c ),
          new JBIG2HuffmanTable(    10, 7,  0,              0x07a ),
          new JBIG2HuffmanTable(    11, 7,  1,              0x07b ),
          new JBIG2HuffmanTable(    13, 7,  2,              0x07c ),
          new JBIG2HuffmanTable(    17, 7,  3,              0x07d ),
          new JBIG2HuffmanTable(    25, 7,  4,              0x07e ),
          new JBIG2HuffmanTable(    41, 8,  5,              0x0fe ),
          new JBIG2HuffmanTable(    73, 8, 32,              0x0ff ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableM = {
          new JBIG2HuffmanTable(     1, 1,  0,              0x000 ),
          new JBIG2HuffmanTable(     2, 3,  0,              0x004 ),
          new JBIG2HuffmanTable(     7, 3,  3,              0x005 ),
          new JBIG2HuffmanTable(     3, 4,  0,              0x00c ),
          new JBIG2HuffmanTable(     5, 4,  1,              0x00d ),
          new JBIG2HuffmanTable(     4, 5,  0,              0x01c ),
          new JBIG2HuffmanTable(    15, 6,  1,              0x03a ),
          new JBIG2HuffmanTable(    17, 6,  2,              0x03b ),
          new JBIG2HuffmanTable(    21, 6,  3,              0x03c ),
          new JBIG2HuffmanTable(    29, 6,  4,              0x03d ),
          new JBIG2HuffmanTable(    45, 6,  5,              0x03e ),
          new JBIG2HuffmanTable(    77, 7,  6,              0x07e ),
          new JBIG2HuffmanTable(   141, 7, 32,              0x07f ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
          };

        public static JBIG2HuffmanTable[] huffTableN = {
          new JBIG2HuffmanTable(     0, 1,  0,              0x000 ),
          new JBIG2HuffmanTable(    -2, 3,  0,              0x004 ),
          new JBIG2HuffmanTable(    -1, 3,  0,              0x005 ),
          new JBIG2HuffmanTable(     1, 3,  0,              0x006 ),
          new JBIG2HuffmanTable(     2, 3,  0,              0x007 ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
                                                       };

        public static JBIG2HuffmanTable[] huffTableO = {
          new JBIG2HuffmanTable(     0, 1,  0,              0x000 ),
          new JBIG2HuffmanTable(    -1, 3,  0,              0x004 ),
          new JBIG2HuffmanTable(     1, 3,  0,              0x005 ),
          new JBIG2HuffmanTable(    -2, 4,  0,              0x00c ),
          new JBIG2HuffmanTable(     2, 4,  0,              0x00d ),
          new JBIG2HuffmanTable(    -4, 5,  1,              0x01c ),
          new JBIG2HuffmanTable(     3, 5,  1,              0x01d ),
          new JBIG2HuffmanTable(    -8, 6,  2,              0x03c ),
          new JBIG2HuffmanTable(     5, 6,  2,              0x03d ),
          new JBIG2HuffmanTable(   -24, 7,  4,              0x07c ),
          new JBIG2HuffmanTable(     9, 7,  4,              0x07d ),
          new JBIG2HuffmanTable(   -25, 7, jbig2HuffmanLOW, 0x07e ),
          new JBIG2HuffmanTable(    25, 7, 32,              0x07f ),
          new JBIG2HuffmanTable(     0, 0, jbig2HuffmanEOT, 0     )
                                                       };
    }
}
