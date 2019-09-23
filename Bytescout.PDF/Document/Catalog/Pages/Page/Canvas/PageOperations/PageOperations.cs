using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal enum PageOperations
    {
        Unknown = 0,
        Linewidth = 1,
        LineCap = 2,
        LineJoin = 3,
        MiterLimit = 4,
        LineDash = 5,
        RenderingIntent = 6,
        FlatnessTolerance = 7,
        GraphicsState = 8,
        SaveGraphicsState = 9,
        RestoreGraphicsState = 10,
        Transform = 11,
        MoveTo = 12,
        LineTo = 13,
        BezierCurve = 14,
        BezierCurve2 = 15,
        BezierCurve3 = 16,
        CloseSubpath = 17,
        Rectangle = 18,
        StrokePath = 19,
        CloseStrokePath = 20,
        FillPathNonZero = 21,
        FillPathEvenOdd = 22,
        FillStrokePathNonZero = 23,
        FillStrokePathEvenOdd = 24,
        CloseFillStrokePathNonZero = 25,
        CloseFillStrokePathEvenOdd = 26,
        EndPath = 27,
        ClipPathNonZero = 28,
        ClipPathEvenOdd = 29,
        BeginText = 30,
        EndText = 31,
        CharacterSpacing = 32,
        WordSpacing = 33,
        HorizontalScaling = 34,
        TextLeading = 35,
        TextFont = 36,
        TextRenderingMode = 37,
        TextRise = 38,
        MoveTextPos = 39,
        MoveTextPosWithLeading = 40,
        TextMatrix = 41,
        MoveTextPosToNextLine = 42,
        ShowText = 43,
        ShowTextStrings = 44,
        ShowTextFromNewLine = 45,
        ShowTextFromNewLineWithSpacing = 46,
        SetWidthForType3 = 47,
        SetWidthAndBBoxForType3 = 48,
        ColorSpaceForStroking = 49,
        ColorSpaceForNonStroking = 50,
        ColorForStroking = 51,
        ColorForStrokingEx = 52,
        ColorForNonStroking = 53,
        ColorForNonStrokingEx = 54,
        GrayColorSpaceForStroking = 55,
        GrayColorSpaceForNonStroking = 56,
        RGBColorSpaceForStroking = 57,
        RGBColorSpaceForNonStroking = 58,
        CMYKColorSpaceForStroking = 59,
        CMYKColorSpaceForNonStroking = 60,
        Shading = 61,
        InlineImage = 62,
        DoXObject = 63,
        MarkedContentPoint = 64,
        MarkedContentPointWithProperties = 65,
        BeginMarkedContentSequence = 66,
        BeginMarkedContentSequenceWithProperties = 67,
        EndMarkedContentSequence = 68,
        BeginCompatibilitySection = 69,
        EndCompatibilitySection = 70
    }

    internal static class PageOperation
    {
        public static void WriteBytes(MemoryStream stream, IPDFPageOperation operation)
        {
            if (stream.Length != 0)
            {
                stream.Position = stream.Length - 2;
                stream.WriteByte((byte)'\r');
                operation.Write(stream);
                stream.WriteByte((byte)'\r');
                stream.WriteByte((byte)'Q');
            }
            else
            {
                stream.Position = stream.Length;
                stream.WriteByte((byte)'q');
                stream.WriteByte((byte)'\r');
                operation.Write(stream);
                stream.WriteByte((byte)'\r');
                stream.WriteByte((byte)'Q');
            }
        }
    }

    internal interface IPDFPageOperation
    {
        void Write(MemoryStream stream);
        void WriteBytes(MemoryStream stream);
        PageOperations Type { get; }
    }

    internal class DrawArc : IPDFPageOperation
    {
	    private List<IPDFPageOperation> _operations;
	    public PageOperations Type { get { return PageOperations.Unknown; } }

	    public DrawArc(float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            _operations = new List<IPDFPageOperation>();

            if (radiusX < 0)
                throw new ArgumentOutOfRangeException("radiusX");
            if (radiusY < 0)
                throw new ArgumentOutOfRangeException("radiusY");
            normalizeAngles(ref startAngle, ref sweepAngle);
            const float step = (float)Math.PI / 2;
            float currentAngle = startAngle;

            _operations.Add(new MoveTo((float)(centerX + (radiusX) * Math.Cos(startAngle)), (float)(centerY + (radiusY) * Math.Sin(startAngle))));

            while (Math.Abs(sweepAngle) > step)
            {
                if (sweepAngle < 0)
                {
                    drawArcRadian(centerX, centerY, radiusX, radiusY, currentAngle, -step);
                    sweepAngle += step;
                    currentAngle -= step;
                }
                else
                {
                    drawArcRadian(centerX, centerY, radiusX, radiusY, currentAngle, step);
                    sweepAngle -= step;
                    currentAngle += step;
                }
            }
            drawArcRadian(centerX, centerY, radiusX, radiusY, currentAngle, sweepAngle);
        }

	    public void Write(MemoryStream stream)
        {
            for (int i = 0; i < _operations.Count; ++i)
            {
                if (i != 0)
                    stream.WriteByte((byte)'\r');
                _operations[i].Write(stream);
            }
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private void drawArcRadian(float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            float c = (float)Math.Cos(sweepAngle / 2);
            float b = (float)Math.Sin(sweepAngle / 2);
            float a = 1 - c;
            float x = a * 4.0f / 3.0f;
            float y = b - x * c / b;
            float cc = (float)Math.Cos(startAngle + sweepAngle / 2);
            float ss = (float)Math.Sin(startAngle + sweepAngle / 2);

            float x1 = centerX + radiusX * ((c + x) * cc + y * ss);
            float y1 = centerY + radiusY * ((c + x) * ss - y * cc);
            float x2 = centerX + radiusX * ((c + x) * cc - y * ss);
            float y2 = centerY + radiusY * ((c + x) * ss + y * cc);
            float x3 = centerX + radiusX * (c * cc - b * ss);
            float y3 = centerY + radiusY * (c * ss + b * cc);

            _operations.Add(new BezierCurve((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3));
        }

        private void normalizeAngles(ref float startAngle, ref float sweepAngle)
        {
            startAngle *= (float)Math.PI / 180;
            sweepAngle *= (float)Math.PI / 180;
            while (startAngle > 2 * (float)Math.PI)
                startAngle -= 2 * (float)Math.PI;
            while (startAngle < 0)
                startAngle += 2 * (float)Math.PI;
        }
    }

    internal class DrawRoundRectangle : IPDFPageOperation
    {
	    private List<IPDFPageOperation> _operations;

	    public PageOperations Type { get { return PageOperations.Unknown; } }

	    public DrawRoundRectangle(float left, float top, float width, float height, float radius)
        {
            _operations = new List<IPDFPageOperation>();

            if (radius < 0)
                throw new ArgumentOutOfRangeException("radius");
            if (width < 0)
                throw new ArgumentOutOfRangeException("width");
            if (height < 0)
                throw new ArgumentOutOfRangeException("height");
            
            if (radius > height / 2 || radius > width / 2)
                throw new ArgumentOutOfRangeException("radius");

            _operations.Add(new MoveTo(left + radius, top));
            _operations.Add(new LineTo(left + width - radius, top));
            drawArcRadian(left + width - radius, top + radius, radius, radius, 3 * (float)Math.PI / 2, (float)Math.PI / 2);
            _operations.Add(new LineTo(left + width, top + height - radius));
            drawArcRadian(left + width - radius, top + height - radius, radius, radius, 0, (float)Math.PI / 2);
            _operations.Add(new LineTo(left + radius, top + height));
            drawArcRadian(left + radius, top + height - radius, radius, radius, (float)Math.PI / 2, (float)Math.PI / 2);
            _operations.Add(new LineTo(left, top + radius));
            drawArcRadian(left + radius, top + radius, radius, radius, (float)Math.PI, (float)Math.PI / 2);
        }

	    public void Write(MemoryStream stream)
        {
            for (int i = 0; i < _operations.Count; ++i)
            {
                if (i != 0)
                    stream.WriteByte((byte)'\r');
                _operations[i].Write(stream);
            }
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private void drawArcRadian(float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            float c = (float)Math.Cos(sweepAngle / 2);
            float b = (float)Math.Sin(sweepAngle / 2);
            float a = 1 - c;
            float x = a * 4.0f / 3.0f;
            float y = b - x * c / b;
            float cc = (float)Math.Cos(startAngle + sweepAngle / 2);
            float ss = (float)Math.Sin(startAngle + sweepAngle / 2);

            float x1 = centerX + radiusX * ((c + x) * cc + y * ss);
            float y1 = centerY + radiusY * ((c + x) * ss - y * cc);
            float x2 = centerX + radiusX * ((c + x) * cc - y * ss);
            float y2 = centerY + radiusY * ((c + x) * ss + y * cc);
            float x3 = centerX + radiusX * (c * cc - b * ss);
            float y3 = centerY + radiusY * (c * ss + b * cc);

            _operations.Add(new BezierCurve((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3));
        }
    }

    internal class DrawPie : IPDFPageOperation
    {
	    private List<IPDFPageOperation> _operations;

	    public PageOperations Type { get { return PageOperations.Unknown; } }

	    public DrawPie(float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            _operations = new List<IPDFPageOperation>();
            if (radiusX < 0)
                throw new ArgumentOutOfRangeException("radiusX");
            if (radiusY < 0)
                throw new ArgumentOutOfRangeException("radiusY");

            _operations.Add(new DrawArc(centerX, centerY, radiusX, radiusY, startAngle, sweepAngle));
            normalizeAngles(ref startAngle, ref sweepAngle);
            _operations.Add(new LineTo(centerX, centerY));
            _operations.Add(new LineTo(centerX + radiusX * (float)Math.Cos(startAngle), centerY + radiusY * (float)Math.Sin(startAngle)));
            _operations.Add(new CloseSubpath());

        }

	    public void Write(MemoryStream stream)
        {
            for (int i = 0; i < _operations.Count; ++i)
            {
                if (i != 0)
                    stream.WriteByte((byte)'\r');
                _operations[i].Write(stream);
            }
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private void normalizeAngles(ref float startAngle, ref float sweepAngle)
        {
            startAngle *= (float)Math.PI / 180;
            sweepAngle *= (float)Math.PI / 180;
            while (startAngle > 2 * (float)Math.PI)
                startAngle -= 2 * (float)Math.PI;
            while (startAngle < 0)
                startAngle += 2 * (float)Math.PI;
        }
    }

    // General graphics state
    // w, J, j, M, d, ri, i, gs
    internal struct Linewidth : IPDFPageOperation
    {
	    private float _lineWidth;
	    public PageOperations Type { get { return PageOperations.Linewidth; } }

	    public float Width { get { return _lineWidth; } }

	    public Linewidth(float lineWidth)
        {
            _lineWidth = lineWidth;
        }

	    public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(_lineWidth, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'w');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct LineCap : IPDFPageOperation
    {
	    private byte _lineCap;

	    public PageOperations Type { get { return PageOperations.LineCap; } }

        public byte CapStyle { get { return _lineCap; } }

	    public LineCap(LineCapStyle lineCap)
	    {
		    _lineCap = (byte)lineCap;
	    }

	    public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(_lineCap, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'J');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct LineJoin : IPDFPageOperation
    {
	    private byte _lineJoin;

	    public PageOperations Type { get { return PageOperations.LineJoin; } }

        public byte JoinStyle { get { return _lineJoin; } }

	    public LineJoin(LineJoinStyle lineJoin)
	    {
		    _lineJoin = (byte)lineJoin;
	    }

	    public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(_lineJoin, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'j');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct MiterLimit : IPDFPageOperation
    {
	    private float _miterLimit;

	    public PageOperations Type { get { return PageOperations.MiterLimit; } }

        public float Limit { get { return _miterLimit; } }

	    public MiterLimit(float miterLimit)
	    {
		    _miterLimit = miterLimit;
	    }

	    public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(_miterLimit, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'M');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal class LineDash : IPDFPageOperation
    {
	    private DashPattern _dashPattern;

	    public PageOperations Type { get { return PageOperations.LineDash; } }

        public DashPattern DashPattern
        {
            get { return _dashPattern; }
        }

	    public LineDash(DashPattern dashPattern)
	    {
		    _dashPattern = dashPattern;
	    }

	    public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'[');
            float[] pattern = _dashPattern.GetPattern();
            for (int i = 0; i < pattern.Length; ++i)
            {
                StringUtility.WriteToStream(pattern[i], stream);
                if (i != pattern.Length - 1)
                    stream.WriteByte((byte)' ');
            }
            stream.WriteByte((byte)']');
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(_dashPattern.Phase, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'d');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct RenderingIntent : IPDFPageOperation
    {
	    private string _name;

	    public PageOperations Type { get { return PageOperations.RenderingIntent; } }

        public string Name { get { return _name; } }

	    public RenderingIntent(string name)
	    {
		    _name = name;
	    }

	    public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, _name);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'r');
            stream.WriteByte((byte)'i');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct FlatnessTolerance : IPDFPageOperation
    {
	    private float _tolerance;

	    public PageOperations Type { get { return PageOperations.FlatnessTolerance; } }

        public float Tolerance { get { return _tolerance; } }

	    public FlatnessTolerance(float tolerance)
	    {
		    _tolerance = tolerance;
	    }

	    public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(_tolerance, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'i');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct GraphicsState : IPDFPageOperation
    {
	    private string _dictName;

	    public PageOperations Type { get { return PageOperations.GraphicsState; } }

        public string DictName { get { return _dictName; } }

	    public GraphicsState(string dictName)
	    {
		    _dictName = dictName;
	    }

	    public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, _dictName);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'g');
            stream.WriteByte((byte)'s');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    // Special graphics state
    // q, Q, cm
    internal struct SaveGraphicsState : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.SaveGraphicsState; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'q');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct RestoreGraphicsState : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.RestoreGraphicsState; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'Q');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct Transform : IPDFPageOperation
    {
	    private float _a;
	    private float _b;
	    private float _c;
	    private float _d;
	    private float _e;
	    private float _f;

	    public PageOperations Type { get { return PageOperations.Transform; } }

        public float A { get { return _a; } }
        public float B { get { return _b; } }
        public float C { get { return _c; } }
        public float D { get { return _d; } }
        public float E { get { return _e; } }
        public float F { get { return _f; } }

	    public Transform(float a, float b, float c, float d, float e, float f)
	    {
		    _a = a;
		    _b = b;
		    _c = c;
		    _d = d;
		    _e = e;
		    _f = f;
	    }

	    public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(_a, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(_b, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(_c, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(_d, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(_e, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(_f, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'c');
            stream.WriteByte((byte)'m');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    // Path construction
    // m, l, c, v, y, h, re
    internal struct MoveTo : IPDFPageOperation
    {
        public MoveTo(float x, float y)
        {
            m_x = x;
            m_y = y;
        }

        public PageOperations Type { get { return PageOperations.MoveTo; } }

        public float X { get { return m_x; } }

        public float Y { get { return m_y; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_x, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'m');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_x;
        private float m_y;
    }

    internal struct LineTo : IPDFPageOperation
    {
        public LineTo(float x, float y)
        {
            m_x = x;
            m_y = y;
        }

        public PageOperations Type { get { return PageOperations.LineTo; } }

        public float X { get { return m_x; } }

        public float Y { get { return m_y; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_x, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'l');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_x;
        private float m_y;
    }

    internal struct BezierCurve : IPDFPageOperation
    {
        public BezierCurve(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            m_x1 = x1;
            m_x2 = x2;
            m_x3 = x3;
            m_y1 = y1;
            m_y2 = y2;
            m_y3 = y3;
        }

        public PageOperations Type { get { return PageOperations.BezierCurve; } }

        public float X1 { get { return m_x1; } }

        public float Y1 { get { return m_y1; } }

        public float X2 { get { return m_x2; } }

        public float Y2 { get { return m_y2; } }

        public float X3 { get { return m_x3; } }

        public float Y3 { get { return m_y3; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_x1, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y1, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_x2, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y2, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_x3, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y3, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'c');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_x1;
        private float m_y1;
        private float m_x2;
        private float m_y2;
        private float m_x3;
        private float m_y3;
    }

    internal struct BezierCurve2 : IPDFPageOperation
    {
        public BezierCurve2(float x2, float y2, float x3, float y3)
        {
            m_x2 = x2;
            m_x3 = x3;
            m_y2 = y2;
            m_y3 = y3;
        }

        public PageOperations Type { get { return PageOperations.BezierCurve2; } }

        public float X2 { get { return m_x2; } }

        public float Y2 { get { return m_y2; } }

        public float X3 { get { return m_x3; } }

        public float Y3 { get { return m_y3; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_x2, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y2, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_x3, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y3, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'v');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_x2;
        private float m_y2;
        private float m_x3;
        private float m_y3;
    }

    internal struct BezierCurve3 : IPDFPageOperation
    {
        public BezierCurve3(float x1, float y1, float x3, float y3)
        {
            m_x1 = x1;
            m_x3 = x3;
            m_y1 = y1;
            m_y3 = y3;
        }

        public PageOperations Type { get { return PageOperations.BezierCurve3; } }

        public float X1 { get { return m_x1; } }

        public float Y1 { get { return m_y1; } }

        public float X3 { get { return m_x3; } }

        public float Y3 { get { return m_y3; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_x1, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y1, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_x3, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y3, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'y');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_x1;
        private float m_y1;
        private float m_x3;
        private float m_y3;
    }

    internal struct CloseSubpath : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.CloseSubpath; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'h');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct Rectangle : IPDFPageOperation
    {
        public Rectangle(float x, float y, float width, float height)
        {
            m_x = x;
            m_y = y;
            m_width = width;
            m_height = height;
        }

        public PageOperations Type { get { return PageOperations.Rectangle; } }

        public float X { get { return m_x; } }

        public float Y { get { return m_y; } }

        public float Width { get { return m_width; } }

        public float Height { get { return m_height; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_x, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_width, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_height, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'r');
            stream.WriteByte((byte)'e');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_x;
        private float m_y;
        private float m_width;
        private float m_height;
    }

    // Path painting
    // S, s, f, F, f*, B, B*, b, b*, n
    internal struct StrokePath : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.StrokePath; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'S');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct CloseStrokePath : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.CloseStrokePath; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'s');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct FillPathNonZero : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.FillPathNonZero; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'f');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct FillPathEvenOdd : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.FillPathEvenOdd; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'f');
            stream.WriteByte((byte)'*');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct FillStrokePathNonZero : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.FillStrokePathNonZero; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'B');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct FillStrokePathEvenOdd : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.FillStrokePathEvenOdd; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'B');
            stream.WriteByte((byte)'*');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct CloseFillStrokePathNonZero : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.CloseFillStrokePathNonZero; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'b');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct CloseFillStrokePathEvenOdd : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.CloseFillStrokePathEvenOdd; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'b');
            stream.WriteByte((byte)'*');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct EndPath : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.EndPath; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'n');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    // Clipping paths
    // W, W*
    internal struct ClipPathNonZero : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.ClipPathNonZero; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'W');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct ClipPathEvenOdd : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.ClipPathEvenOdd; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'W');
            stream.WriteByte((byte)'*');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    // Text objects
    // BT, ET
    internal struct BeginText : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.BeginText; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'B');
            stream.WriteByte((byte)'T');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal class EndText : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.EndText; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'E');
            stream.WriteByte((byte)'T');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    // Text state
    // Tc, Tw, Tz, TL, Tf, Tr, Ts
    internal struct CharacterSpacing : IPDFPageOperation
    {
        public CharacterSpacing(float charSpace)
        {
            m_charSpace = charSpace;
        }

        public PageOperations Type { get { return PageOperations.CharacterSpacing; } }

        public float CharSpace { get { return m_charSpace; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_charSpace, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'c');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_charSpace;
    }

    internal struct WordSpacing : IPDFPageOperation
    {
        public WordSpacing(float wordSpace)
        {
            m_wordSpace = wordSpace;
        }

        public PageOperations Type { get { return PageOperations.WordSpacing; } }

        public float WordSpace { get { return m_wordSpace; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_wordSpace, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'w');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_wordSpace;
    }

    internal struct HorizontalScaling : IPDFPageOperation
    {
        public HorizontalScaling(float scale)
        {
            m_scale = scale;
        }

        public PageOperations Type { get { return PageOperations.HorizontalScaling; } }

        public float Scale { get { return m_scale; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_scale, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'z');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_scale;
    }

    internal struct TextLeading : IPDFPageOperation
    {
        public TextLeading(float leading)
        {
            m_leading = leading;
        }

        public PageOperations Type { get { return PageOperations.TextLeading; } }

        public float Leading { get { return m_leading; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_leading, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'L');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_leading;
    }

    internal struct TextFont : IPDFPageOperation
    {
        public TextFont(string fontName, float fontSize)
        {
            m_fontName = fontName;
            m_fontSize = fontSize;
        }

        public PageOperations Type { get { return PageOperations.TextFont; } }

        public string FontName { get { return m_fontName; } }

        public float FontSize { get { return m_fontSize; } }

        public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, m_fontName);
            stream.WriteByte((byte)' ');

            StringUtility.WriteToStream(m_fontSize, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'f');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private string m_fontName;
        private float m_fontSize;
    }

    internal struct TextRenderingMode : IPDFPageOperation
    {
        public TextRenderingMode(int render)
        {
            m_render = render;
        }

        public PageOperations Type { get { return PageOperations.TextRenderingMode; } }

        public int Render { get { return m_render; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_render, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'r');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private int m_render;
    }

    internal struct TextRise : IPDFPageOperation
    {
        public TextRise(float rise)
        {
            m_rise = rise;
        }

        public PageOperations Type { get { return PageOperations.TextRise; } }

        public float Rise { get { return m_rise; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_rise, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'s');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_rise;
    }

    // Text positioning
    // Td, TD, Tm, T*
    internal struct MoveTextPos : IPDFPageOperation
    {
        public MoveTextPos(float tx, float ty)
        {
            m_tx = tx;
            m_ty = ty;
        }

        public PageOperations Type { get { return PageOperations.MoveTextPos; } }

        public float TX { get { return m_tx; } }

        public float TY { get { return m_ty; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_tx, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_ty, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'d');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_tx;
        private float m_ty;
    }

    internal struct MoveTextPosWithLeading : IPDFPageOperation
    {
        public MoveTextPosWithLeading(float tx, float ty)
        {
            m_tx = tx;
            m_ty = ty;
        }

        public PageOperations Type { get { return PageOperations.MoveTextPosWithLeading; } }

        public float TX { get { return m_tx; } }

        public float TY { get { return m_ty; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_tx, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_ty, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'D');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_tx;
        private float m_ty;
    }

    internal struct TextMatrix : IPDFPageOperation
    {
        public TextMatrix(float a, float b, float c, float d, float e, float f)
        {
            m_a = a;
            m_b = b;
            m_c = c;
            m_d = d;
            m_e = e;
            m_f = f;
        }

        public PageOperations Type { get { return PageOperations.TextMatrix; } }

        public float A { get { return m_a; } }

        public float B { get { return m_b; } }

        public float C { get { return m_c; } }

        public float D { get { return m_d; } }

        public float E { get { return m_e; } }

        public float F { get { return m_f; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_a, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_b, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_c, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_d, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_e, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_f, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'m');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_a;
        private float m_b;
        private float m_c;
        private float m_d;
        private float m_e;
        private float m_f;
    }

    internal struct MoveTextPosToNextLine : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.MoveTextPosToNextLine; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'*');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    // Text showing
    // Tj, TJ, ', "
    internal struct ShowText : IPDFPageOperation
    {
        public ShowText(PDFString str)
        {
            m_str = str;
        }

        public PageOperations Type { get { return PageOperations.ShowText; } }

        public PDFString String { get { return m_str; } }

        public void Write(MemoryStream stream)
        {
            m_str.Write(new SaveParameters(stream));
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'j');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private PDFString m_str;
    }

    internal struct ShowTextFromNewLine : IPDFPageOperation
    {
        public ShowTextFromNewLine(PDFString str)
        {
            m_str = str;
        }

        public PageOperations Type { get { return PageOperations.ShowTextFromNewLine; } }

        public PDFString String { get { return m_str; } }

        public void Write(MemoryStream stream)
        {
            m_str.Write(new SaveParameters(stream));
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'\'');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private PDFString m_str;
    }

    internal struct ShowTextFromNewLineWithSpacing : IPDFPageOperation
    {
        public ShowTextFromNewLineWithSpacing(float wordSpacing, float characterSpacing, PDFString str)
        {
            m_aw = wordSpacing;
            m_ac = characterSpacing;
            m_str = str;
        }

        public PageOperations Type { get { return PageOperations.ShowTextFromNewLineWithSpacing; } }

        public float WordSpacing { get { return m_aw; } }

        public float CharacterSpacing { get { return m_ac; } }

        public PDFString String { get { return m_str; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_aw, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_ac, stream);
            stream.WriteByte((byte)' ');
            m_str.Write(new SaveParameters(stream));
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'"');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_aw;
        private float m_ac;
        private PDFString m_str;
    }

    internal struct ShowTextStrings : IPDFPageOperation
    {
        public ShowTextStrings(object[] array)
        {
            m_array = array;
        }

        public PageOperations Type { get { return PageOperations.ShowTextStrings; } }

        public object[] Array { get { return m_array; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'[');
            for (int i = 0; i < m_array.Length; ++i)
            {
                if (m_array[i] is PDFString)
                    (m_array[i] as PDFString).Write(new SaveParameters(stream));
                else
                    StringUtility.WriteToStream((float)m_array[i], stream);
            }
            stream.WriteByte((byte)']');
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'T');
            stream.WriteByte((byte)'J');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private object[] m_array;
    }

    // Type 3 fonts
    // d0, d1
    internal struct SetWidthForType3 : IPDFPageOperation
    {
        public SetWidthForType3(float wx, float wy)
        {
            m_wx = wx;
            m_wy = wy;
        }

        public PageOperations Type { get { return PageOperations.SetWidthForType3; } }

        public float WidthX { get { return m_wx; } }

        public float WidthY { get { return m_wy; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_wx, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_wy, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'d');
            stream.WriteByte((byte)'0');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_wx;
        private float m_wy;
    }

    internal struct SetWidthAndBBoxForType3 : IPDFPageOperation
    {
        public SetWidthAndBBoxForType3(float wx, float wy, float llx, float lly, float urx, float ury)
        {
            m_wx = wx;
            m_wy = wy;
            m_llx = llx;
            m_lly = lly;
            m_urx = urx;
            m_ury = ury;
        }

        public PageOperations Type { get { return PageOperations.SetWidthAndBBoxForType3; } }

        public float WX { get { return m_wx; } }

        public float WY { get { return m_wy; } }

        public float LLX { get { return m_llx; } }

        public float LLY { get { return m_lly; } }

        public float URX { get { return m_urx; } }

        public float URY { get { return m_ury; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_wx, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_wy, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_llx, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_lly, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_urx, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_ury, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'d');
            stream.WriteByte((byte)'1');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_wx;
        private float m_wy;
        private float m_llx;
        private float m_lly;
        private float m_urx;
        private float m_ury;
    }

    // Color
    // CS, cs, SC, SCN, sc, scn, G, g, RG, rg, K, k
    internal struct ColorSpaceForStroking : IPDFPageOperation
    {
        public ColorSpaceForStroking(string name)
        {
            m_name = name;
        }

        public PageOperations Type { get { return PageOperations.ColorSpaceForStroking; } }

        public string Name { get { return m_name; } }

        public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, m_name);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'C');
            stream.WriteByte((byte)'S');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private string m_name;
    }

    internal struct ColorSpaceForNonStroking : IPDFPageOperation
    {
        public ColorSpaceForNonStroking(string name)
        {
            m_name = name;
        }

        public PageOperations Type { get { return PageOperations.ColorSpaceForNonStroking; } }

        public string Name { get { return m_name; } }

        public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, m_name);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'c');
            stream.WriteByte((byte)'s');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private string m_name;
    }

    internal struct ColorForStroking : IPDFPageOperation
    {
        public ColorForStroking(float[] color)
        {
            m_color = color;
        }

        public PageOperations Type { get { return PageOperations.ColorForStroking; } }

        public float[] Color { get { return m_color; } }

        public void Write(MemoryStream stream)
        {
            for (int i = 0; i < m_color.Length; ++i)
            {
                StringUtility.WriteToStream(m_color[i], stream);
                stream.WriteByte((byte)' ');
            }
            stream.WriteByte((byte)'S');
            stream.WriteByte((byte)'C');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float[] m_color;
    }

    internal struct ColorForStrokingEx : IPDFPageOperation
    {
        public ColorForStrokingEx(float[] color)
        {
            m_color = color;
            m_name = "";
        }

        public PageOperations Type { get { return PageOperations.ColorForStrokingEx; } }

        public ColorForStrokingEx(float[] color, string name)
        {
            m_color = color;
            m_name = name;
        }

        public float[] Color { get { return m_color; } }

        public string Name { get { return m_name; } }

        public void Write(MemoryStream stream)
        {
            for (int i = 0; i < m_color.Length; ++i)
            {
                StringUtility.WriteToStream(m_color[i], stream);
                stream.WriteByte((byte)' ');
            }

            if (m_name != "")
            {
                PDFName.Write(stream, m_name);
                stream.WriteByte((byte)' ');
            }

            stream.WriteByte((byte)'S');
            stream.WriteByte((byte)'C');
            stream.WriteByte((byte)'N');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float[] m_color;
        private string m_name;
    }

    internal struct ColorForNonStroking : IPDFPageOperation
    {
        public ColorForNonStroking(float[] color)
        {
            m_color = color;
        }

        public PageOperations Type { get { return PageOperations.ColorForNonStroking; } }

        public float[] Color { get { return m_color; } }

        public void Write(MemoryStream stream)
        {
            for (int i = 0; i < m_color.Length; ++i)
            {
                StringUtility.WriteToStream(m_color[i], stream);
                stream.WriteByte((byte)' ');
            }
            stream.WriteByte((byte)'s');
            stream.WriteByte((byte)'c');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float[] m_color;
    }

    internal struct ColorForNonStrokingEx : IPDFPageOperation
    {
        public ColorForNonStrokingEx(float[] color)
        {
            m_color = color;
            m_name = "";
        }

        public PageOperations Type { get { return PageOperations.ColorForNonStrokingEx; } }

        public ColorForNonStrokingEx(float[] color, string name)
        {
            m_color = color;
            m_name = name;
        }

        public float[] Color { get { return m_color; } }

        public string Name { get { return m_name; } }

        public void Write(MemoryStream stream)
        {
            for (int i = 0; i < m_color.Length; ++i)
            {
                StringUtility.WriteToStream(m_color[i], stream);
                stream.WriteByte((byte)' ');
            }

            if (m_name != "")
            {
                PDFName.Write(stream, m_name);
                stream.WriteByte((byte)' ');
            }

            stream.WriteByte((byte)'s');
            stream.WriteByte((byte)'c');
            stream.WriteByte((byte)'n');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float[] m_color;
        private string m_name;
    }

    internal struct GrayColorSpaceForStroking : IPDFPageOperation
    {
        public GrayColorSpaceForStroking(float gray)
        {
            m_gray = gray;
        }

        public PageOperations Type { get { return PageOperations.GrayColorSpaceForStroking; } }

        public float Gray { get { return m_gray; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_gray, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'G');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_gray;
    }

    internal struct GrayColorSpaceForNonStroking : IPDFPageOperation
    {
        public GrayColorSpaceForNonStroking(float gray)
        {
            m_gray = gray;
        }

        public PageOperations Type { get { return PageOperations.GrayColorSpaceForNonStroking; } }

        public float Gray { get { return m_gray; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_gray, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'g');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_gray;
    }

    internal struct RGBColorSpaceForStroking : IPDFPageOperation
    {
        public RGBColorSpaceForStroking(float r, float g, float b)
        {
            m_r = r;
            m_g = g;
            m_b = b;
        }

        public PageOperations Type { get { return PageOperations.RGBColorSpaceForStroking; } }

        public float R { get { return m_r; } }

        public float G { get { return m_g; } }

        public float B { get { return m_b; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_r, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_g, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_b, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'R');
            stream.WriteByte((byte)'G');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_r;
        private float m_g;
        private float m_b;
    }

    internal struct RGBColorSpaceForNonStroking : IPDFPageOperation
    {
        public RGBColorSpaceForNonStroking(float r, float g, float b)
        {
            m_r = r;
            m_g = g;
            m_b = b;
        }

        public PageOperations Type { get { return PageOperations.RGBColorSpaceForNonStroking; } }

        public float R { get { return m_r; } }

        public float G { get { return m_g; } }

        public float B { get { return m_b; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_r, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_g, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_b, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'r');
            stream.WriteByte((byte)'g');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_r;
        private float m_g;
        private float m_b;
    }

    internal struct CMYKColorSpaceForStroking : IPDFPageOperation
    {
        public CMYKColorSpaceForStroking(float c, float m, float y, float k)
        {
            m_c = c;
            m_m = m;
            m_y = y;
            m_k = k;
        }

        public PageOperations Type { get { return PageOperations.CMYKColorSpaceForStroking; } }

        public float C { get { return m_c; } }

        public float M { get { return m_m; } }

        public float Y { get { return m_y; } }

        public float K { get { return m_k; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_c, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_m, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_k, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'K');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_c;
        private float m_m;
        private float m_y;
        private float m_k;
    }

    internal struct CMYKColorSpaceForNonStroking : IPDFPageOperation
    {
        public CMYKColorSpaceForNonStroking(float c, float m, float y, float k)
        {
            m_c = c;
            m_m = m;
            m_y = y;
            m_k = k;
        }

        public PageOperations Type { get { return PageOperations.CMYKColorSpaceForNonStroking; } }

        public float C { get { return m_c; } }

        public float M { get { return m_m; } }

        public float Y { get { return m_y; } }

        public float K { get { return m_k; } }

        public void Write(MemoryStream stream)
        {
            StringUtility.WriteToStream(m_c, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_m, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_y, stream);
            stream.WriteByte((byte)' ');
            StringUtility.WriteToStream(m_k, stream);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'k');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private float m_c;
        private float m_m;
        private float m_y;
        private float m_k;
    }

    //
    internal class DefaultColorSpaceForNotStroking : IPDFPageOperation
    {
        public DefaultColorSpaceForNotStroking(Color color)
        {
            m_color = color;
        }

        public PageOperations Type { get { return PageOperations.Unknown; } }

        public Color Color
        {
            get
            {
                return m_color;
            }
        }

        public void Write(MemoryStream stream)
        {
            string result = m_color.ToString() + ' ';
            switch (m_color.Colorspace.Name)
            {
                case "DeviceCMYK":
                    result += "k";
                    break;
                case "DeviceRGB":
                    result += "rg";
                    break;
                case "DeviceGray":
                    result += "g";
                    break;
                case "ICCBased":
                    result += "scn";
                    break;
            }
            byte[] data = System.Text.Encoding.ASCII.GetBytes(result);
            stream.Write(data, 0, data.Length);
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        Color m_color;
    }

    internal class DefaultColorSpaceForStroking : IPDFPageOperation
    {
        public DefaultColorSpaceForStroking(Color color)
        {
            m_color = color;
        }

        public PageOperations Type { get { return PageOperations.Unknown; } }

        public Color Color
        {
            get
            {
                return m_color;
            }
        }

        public void Write(MemoryStream stream)
        {
            string result = m_color.ToString() + ' ';
            switch (m_color.Colorspace.Name)
            {
                case "DeviceCMYK":
                    result += "K";
                    break;
                case "DeviceRGB":
                    result += "RG";
                    break;
                case "DeviceGray":
                    result += "G";
                    break;
                case "ICCBased":
                    result += "SCN";
                    break;
            }
            byte[] data = System.Text.Encoding.ASCII.GetBytes(result);
            stream.Write(data, 0, data.Length);
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        Color m_color;
    }

    // Shading patterns
    // sh
    internal struct Shading : IPDFPageOperation
    {
        public Shading(string name)
        {
            m_name = name;
        }

        public PageOperations Type { get { return PageOperations.Shading; } }

        public string Name { get { return m_name; } }

        public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, m_name);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'s');
            stream.WriteByte((byte)'h');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private string m_name;
    }

    // Inline images
    // BI, ID, EI
    internal class InlineImage : IPDFPageOperation
    {
        public InlineImage(PDFDictionary dict, MemoryStream stream)
        {
            m_dictionary = dict;
            m_stream = stream;
        }

        public PageOperations Type { get { return PageOperations.InlineImage; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'B');
            stream.WriteByte((byte)'I');
            stream.WriteByte((byte)'\r');

            string[] keys = m_dictionary.GetKeys();
            for (int i = 0; i < keys.Length; ++i)
            {
                stream.WriteByte((byte)'/');
                for (int j = 0; j < keys[i].Length; ++j)
                    stream.WriteByte((byte)keys[i][j]);

                stream.WriteByte((byte)' ');
                IPDFObject obj = m_dictionary[keys[i]];
                obj.Write(new SaveParameters(stream));
                stream.WriteByte((byte)'\r');
            }

            stream.WriteByte((byte)'I');
            stream.WriteByte((byte)'D');
            stream.WriteByte((byte)' ');

            m_stream.WriteTo(stream);

            stream.WriteByte((byte)'\r');
            stream.WriteByte((byte)'E');
            stream.WriteByte((byte)'I');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private MemoryStream m_stream;
        private PDFDictionary m_dictionary;
    }

    // XObjects
    // Do
    internal struct DoXObject : IPDFPageOperation
    {
        public DoXObject(string name)
        {
            m_name = name;
        }

        public PageOperations Type { get { return PageOperations.DoXObject; } }

        public string Name { get { return m_name; } }

        public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, m_name);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'D');
            stream.WriteByte((byte)'o');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private string m_name;
    }

    // Marked content
    // MP, DP, BMC, BDC, EMC
    internal struct MarkedContentPoint : IPDFPageOperation
    {
        public MarkedContentPoint(string tag)
        {
            m_tag = tag;
        }

        public PageOperations Type { get { return PageOperations.MarkedContentPoint; } }

        public string Tag { get { return m_tag; } }

        public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, m_tag);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'M');
            stream.WriteByte((byte)'P');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private string m_tag;
    }

    internal struct MarkedContentPointWithProperties : IPDFPageOperation
    {
        public MarkedContentPointWithProperties(string tag, IPDFObject properties)
        {
            m_tag = tag;
            m_properties = properties;
        }

        public PageOperations Type { get { return PageOperations.MarkedContentPointWithProperties; } }

        public string Tag { get { return m_tag; } }

        public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, m_tag);
            stream.WriteByte((byte)' ');

            m_properties.Write(new SaveParameters(stream));
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'D');
            stream.WriteByte((byte)'P');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private string m_tag;
        private IPDFObject m_properties;
    }

    internal struct BeginMarkedContentSequence : IPDFPageOperation
    {
        public BeginMarkedContentSequence(string tag)
        {
            m_tag = tag;
        }

        public PageOperations Type { get { return PageOperations.BeginMarkedContentSequence; } }

        public string Tag { get { return m_tag; } }

        public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, m_tag);
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'B');
            stream.WriteByte((byte)'M');
            stream.WriteByte((byte)'C');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private string m_tag;
    }

    internal struct BeginMarkedContentSequenceWithProperties : IPDFPageOperation
    {
        public BeginMarkedContentSequenceWithProperties(string tag, IPDFObject properties)
        {
            m_tag = tag;
            m_properties = properties;
        }

        public PageOperations Type { get { return PageOperations.BeginMarkedContentSequenceWithProperties; } }

        public string Tag { get { return m_tag; } }

        public void Write(MemoryStream stream)
        {
            PDFName.Write(stream, m_tag);
            stream.WriteByte((byte)' ');

            m_properties.Write(new SaveParameters(stream));
            stream.WriteByte((byte)' ');
            stream.WriteByte((byte)'B');
            stream.WriteByte((byte)'D');
            stream.WriteByte((byte)'C');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }

        private string m_tag;
        private IPDFObject m_properties;
    }

    internal struct EndMarkedContentSequence : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.EndMarkedContentSequence; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'E');
            stream.WriteByte((byte)'M');
            stream.WriteByte((byte)'C');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    // Compatibility
    // BX, EX
    internal struct BeginCompatibilitySection : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.BeginCompatibilitySection; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'B');
            stream.WriteByte((byte)'X');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }

    internal struct EndCompatibilitySection : IPDFPageOperation
    {
        public PageOperations Type { get { return PageOperations.EndCompatibilitySection; } }

        public void Write(MemoryStream stream)
        {
            stream.WriteByte((byte)'E');
            stream.WriteByte((byte)'X');
        }

        public void WriteBytes(MemoryStream stream)
        {
            PageOperation.WriteBytes(stream, this);
        }
    }
}
