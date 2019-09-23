namespace Bytescout.PDF
{
    internal enum PDFPatternType
    {
        TilingPattern = 1,
        ShadingPattern = 2
    }

    internal enum PDFTilingPaintType
    {
        ColoredTilingPattern = 1,
        UncoloredTilingPattern = 2
    }
    
    internal abstract class PatternColorspace : Colorspace
    {
        public override string Name
        {
            get
            {
                return "Pattern";
            }
        }
        
        public abstract PDFPatternType PatternType
        {
            get;
        }
    }
}
