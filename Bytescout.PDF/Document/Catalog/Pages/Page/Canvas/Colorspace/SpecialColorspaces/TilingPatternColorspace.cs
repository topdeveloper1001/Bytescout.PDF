namespace Bytescout.PDF
{
    internal abstract class TilingPatternColorspace : PatternColorspace
    {
        public override PDFPatternType PatternType
        {
            get 
            {
                return PDFPatternType.TilingPattern;
            }
        }
        
        public abstract PDFTilingPaintType PaintType
        {
            get;
        }
        
        public abstract TilingType TilingType
        {
            get;
            set;
        }
        
        public abstract float Height
        {
            get;
            set;
        }
        
        public abstract float Width
        {
            get;
            set;
        }
        
        public abstract float XStep
        {
            get;
            set;
        }
        
        public abstract float YStep
        {
            get;
            set;
        }
        
        public abstract float[] Matrix
        {
            get;
            set;
        }
    }
}
