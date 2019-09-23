using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Path
#else
	/// <summary>
    /// Represents a graphics path, which is a sequence of primitive graphics elements.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class Path
#endif
	{
	    private FillMode _fillmode;
	    private readonly List<IPDFPageOperation> _operations;

	    /// <summary>
        /// Gets or sets the fill mode.
        /// </summary>
        /// <value cref="PDF.FillMode"></value>
        public FillMode FillMode
        {
            get { return _fillmode; }
            set { _fillmode = value; }
        }

        internal IPDFPageOperation[] Operations
        {
            get
            {
                return _operations.ToArray();
            }
        }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.Path class.
	    /// </summary>
	    public Path()
	    { 
		    _fillmode = PDF.FillMode.Alternate;
		    _operations = new List<IPDFPageOperation>();
	    }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.Path class.
	    /// </summary>
	    /// <param name="fillmode">The fill mode to fill a path.</param>
	    public Path(FillMode fillmode)
	    { 
		    _fillmode = fillmode;
		    _operations = new List<IPDFPageOperation>();
	    }

	    /// <summary>
        /// Appends an elliptical arc to the current figure.
        /// </summary>
        /// <param name="centerX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the center of the ellipse.</param>
        /// <param name="centerY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the center of the ellipse.</param>
        /// <param name="radiusX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the ellipse measured along the x-axis.</param>
        /// <param name="radiusY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the ellipse measured along the y-axis.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The start angle of the arc in degrees.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The sweep angle of the arc in degrees.</param>
        public void AddArc(float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            _operations.Add(new DrawArc(centerX, -centerY, radiusX, radiusY, -startAngle, -sweepAngle));
        }

        /// <summary>
        /// Appends an elliptical arc to the current figure.
        /// </summary>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.RectangleF structure that defines the boundaries of the arc.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The start angle of the arc in degrees.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The sweep angle of the arc in degrees.</param>
		[ComVisible(false)]
		public void AddArc(RectangleF rect, float startAngle, float sweepAngle)
        {
            AddArc(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2, startAngle, sweepAngle);
        }

        /// <summary>
        /// Moves the current point to a new position.
        /// </summary>
        /// <param name="x" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the new position.</param>
        /// <param name="y" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the new position.</param>
        public void MoveTo(float x, float y)
        {
            _operations.Add(new MoveTo(x, -y));
        }

        /// <summary>
        /// Moves the current point to a new position.
        /// </summary>
        /// <param name="point" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">A new position.</param>
		[ComVisible(false)]
		public void MoveTo(PointF point)
        {
            _operations.Add(new MoveTo(point.X, -point.Y));
        }

        /// <summary>
        /// Appends a rectangle with rounded corners to the current figure.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the rectangle.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the rectangle.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the rectangle.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the rectangle.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the corner circle.</param>
        public void AddRoundedRectangle(float left, float top, float width, float height, float radius)
        {
            _operations.Add(new DrawRoundRectangle(left, -(top + height), width, height, radius));
        }

        /// <summary>
        /// Appends a rectangle with rounded corners to the current figure.
        /// </summary>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.RectangleF structure that represents the rectangle to append.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the corner circle.</param>
		[ComVisible(false)]
		public void AddRoundedRectangle(RectangleF rect, float radius)
        {
            AddRoundedRectangle(rect.Left, rect.Top, rect.Width, rect.Height, radius);
        }

        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        /// <param name="centerX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the center of the pie shape.</param>
        /// <param name="centerY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the center of the pie shape.</param>
        /// <param name="radiusX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the pie shape measured along the x-axis.</param>
        /// <param name="radiusY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the pie shape measured along the y-axis.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
        public void AddPie(float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            _operations.Add(new DrawPie(centerX, -centerY, radiusX, radiusY, -startAngle, -sweepAngle));
        }

        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.RectangleF structure that represents the bounding rectangle
        /// that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
		[ComVisible(false)]
        public void AddPie(RectangleF rect, float startAngle, float sweepAngle)
        {
            AddPie(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2, startAngle, sweepAngle);
        }

        /// <summary>
        /// Adds a circle to the current path.
        /// </summary>
        /// <param name="centerX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the center of the circle.</param>
        /// <param name="centerY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the center of the circle.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the circle.</param>
        public void AddCircle(float centerX, float centerY, float radius)
        {
            _operations.Add(new DrawArc(centerX, -centerY, radius, radius, 0, -360));
        }

        /// <summary>
        /// Adds a circle to the current path.
        /// </summary>
        /// <param name="center" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The circle center.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the circle.</param>
		[ComVisible(false)]
		public void AddCircle(PointF center, float radius)
        {
            _operations.Add(new DrawArc(center.X, -center.Y, radius, radius, 0, -360));
        }

        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Height of the bounding rectangle that defines the ellipse.</param>
        public void AddEllipse(float left, float top, float width, float height)
        {
            AddArc(left + width / 2, top + height / 2, width / 2, height / 2, 0, 360);
        }

        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.RectangleF that represents the bounding rectangle that defines the ellipse.</param>
		[ComVisible(false)]
        public void AddEllipse(RectangleF rect)
        {
            AddEllipse(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Append a cubic Bézier curve to the current path.
        /// </summary>
        /// <param name="x1" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the first control point for the curve.</param>
        /// <param name="y1" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the first control point for the curve.</param>
        /// <param name="x2" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the second control point for the curve.</param>
        /// <param name="y2" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the second control point for the curve.</param>
        /// <param name="x3" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the endpoint.</param>
        /// <param name="y3" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the endpoint.</param>
        public void AddCurveTo(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            _operations.Add(new BezierCurve(x1, -y1, x2, -y2, x3, -y3));
        }

        /// <summary>
        /// Append a cubic Bézier curve to the current path.
        /// </summary>
        /// <param name="pt1" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The first control point.</param>
        /// <param name="pt2" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The second control point.</param>
        /// <param name="pt3" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The endpoint.</param>
		[ComVisible(false)]
		public void AddCurveTo(PointF pt1, PointF pt2, PointF pt3)
        {
            AddCurveTo(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y);
        }

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the rectangle.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the rectangle.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the rectangle.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the rectangle.</param>
        public void AddRectangle(float left, float top, float width, float height)
        {
            _operations.Add (new Rectangle(left, -(top + height), width, height));
        }

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.Rectangle that represents the rectangle to add.</param>
		[ComVisible(false)]
		public void AddRectangle(RectangleF rect)
        {
            AddRectangle(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Appends a straight line segment from the current position to a specified endpoint.
        /// </summary>
        /// <param name="x" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the endpoint.</param>
        /// <param name="y" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the endpoint.</param>
        public void AddLineTo(float x, float y)
        {
            _operations.Add(new LineTo(x, -y));
        }

        /// <summary>
        /// Appends a straight line segment from the current position to a specified endpoint.
        /// </summary>
        /// <param name="point" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The endpoint.</param>
		[ComVisible(false)]
		public void AddLineTo(PointF point)
        {
            AddLineTo(point.X, point.Y);
        }

        /// <summary>
        /// Closes the current subpath by appending a straight line segment from the current
        /// position to the starting point of the subpath.
        /// </summary>
        public void ClosePath()
        {
            _operations.Add(new CloseSubpath());
        }

        /// <summary>
        /// Empties path of points.
        /// </summary>
        public void Reset()
        {
            _operations.Clear();
        }

        /// <summary>
        /// Append the closed polygon to this path.
        /// </summary>
        /// <param name="points" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The points of the polygon.</param>
        public void AddPolygon(PointF[] points)
        {
            _operations.Add(new MoveTo(points[0].X, -points[0].Y));
            for (int i = 1; i < points.Length; ++i)
            {
                _operations.Add(new LineTo(points[i].X, -points[i].Y));
            }
            ClosePath();
        }
    }
}
