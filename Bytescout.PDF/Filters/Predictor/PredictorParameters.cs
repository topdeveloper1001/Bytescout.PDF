namespace Bytescout.PDF
{
    internal struct PredictorParameters
    {
	    private int _predictor;
	    private int _colors;
	    private int _bitsPerComponent;
	    private int _columns;

	    public int Predictor { get { return _predictor; } }

        public int Colors { get { return _colors; } }

        public int BitsPerComponent { get { return _bitsPerComponent; } }

        public int Columns { get { return _columns; } }

	    public PredictorParameters(PDFDictionary dict)
	    {
		    _predictor = getPredictor(dict);
		    _colors = getColors(dict);
		    _bitsPerComponent = getBitsPerComponent(dict);
		    _columns = getColumns(dict);
	    }

	    private static int getPredictor(PDFDictionary dict)
        {
            if (dict == null)
                return 1;

            PDFNumber predictor = dict["Predictor"] as PDFNumber;
            if (predictor == null)
                return 1;
            return (int)predictor.GetValue();
        }

        private static int getColors(PDFDictionary dict)
        {
            if (dict == null)
                return 1;

            PDFNumber colors = dict["Colors"] as PDFNumber;
            if (colors == null)
                return 1;
            return (int)colors.GetValue();
        }

        private static int getBitsPerComponent(PDFDictionary dict)
        {
            if (dict == null)
                return 8;

            PDFNumber bitsPerComponent = dict["BitsPerComponent"] as PDFNumber;
            if (bitsPerComponent == null)
                return 8;
            return (int)bitsPerComponent.GetValue();
        }

        private static int getColumns(PDFDictionary dict)
        {
            if (dict == null)
                return 1;

            PDFNumber columns = dict["Columns"] as PDFNumber;
            if (columns == null)
                return 1;
            return (int)columns.GetValue();
        }
    }
}
