using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Canvas
#else
	/// <summary>
    /// Represents class for a canvas (a two-dimensional region on which all painting occurs).
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class Canvas
#endif
	{
		private readonly MemoryStream _stream;
		private readonly Resources _resources;
		private StateCanvas _curentState;
		private List<StateCanvas> _listSaveState;
		private RectangleF _rect;

		internal event ChangeGroupEventHandler ChangeGroup;

	    /// <summary>
        /// Gets the width of the canvas.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Width
        {
            get
            {
                return _rect.Right - _rect.Left;
            }
        }

        /// <summary>
        /// Gets the height of the canvas.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Height
        {
            get
            {
                return _rect.Bottom - _rect.Top;
            }
        }

        /// <summary>
        /// Gets or sets the rendering intent.
        /// </summary>
        /// <value cref="RenderingIntentType"></value>
        public RenderingIntentType RenderingIntent
        {
            get
            {
                return _curentState.RenderingIntent;
            }
            set
            {
                _curentState.RenderingIntent = value;
                addOperation(new RenderingIntent(TypeConverter.PDFRenderingIntentToString(_curentState.RenderingIntent)));
            }
        }

        /// <summary>
        /// Gets or sets the blend mode.
        /// </summary>
        /// <value cref="PDF.BlendMode"></value>
        public BlendMode BlendMode
        {
            get
            {
                return _curentState.BlendMode;
            }
            set
            {
                _curentState.BlendMode = value;
                PDFDictionary dict = new PDFDictionary();
                dict.AddItem("BM", new PDFName(TypeConverter.PDFBlendModeToString(_curentState.BlendMode)));
                string name = _resources.AddResources(ResourceType.ExtGState, dict);
                addOperation(new GraphicsState(name));
            }
        }

        internal Resources Resources
        {
            get
            {
                return _resources;
            }
        }

        internal MemoryStream Stream
        {
            get
            {
                return _stream;
            }
        }


        private Brush Brush
        {
            get
            {
                return _curentState.Brush;
            }

            set
            {
                _curentState.Brush.WriteChanges(value, _stream, _resources);
                _curentState.Brush = value.Clone() as Brush;
            }
        }

        private Pen Pen
        {
            get
            {
                return _curentState.Pen;
            }

            set
            {
                _curentState.Pen.WriteChanges(value, _stream, _resources);
                _curentState.Pen = value.Clone() as Pen;
            }
        }

	    internal Canvas(MemoryStream stream, Resources resources, RectangleF rect)
	    {
		    _stream = stream;
		    _resources = resources;
		    _curentState = new StateCanvas();
		    _listSaveState = new List<StateCanvas>();
		    addOperation(new Transform(1, 0, 0, 1, rect.Left, rect.Top + rect.Height));
		    _rect = rect;
	    }

	    internal Canvas(float width, float height)
	    {
		    _stream = new MemoryStream();
		    _resources = new Resources();
		    _curentState = new StateCanvas();
		    _listSaveState = new List<StateCanvas>();
		    addOperation(new Transform(1, 0, 0, 1, 0, height));
		    _rect = new RectangleF(0, 0, width, height);
	    }

	    internal Canvas(MemoryStream stream, Resources resources, float width, float height)
	    {
		    _stream = stream;
		    _resources = resources;
		    _curentState = new StateCanvas();
		    _listSaveState = new List<StateCanvas>();
		    addOperation(new Transform(1, 0, 0, 1, 0, height));
		    _rect = new RectangleF(0, 0, width, height);
	    }

	    internal void DrawMetafile(Metafile metafile, float left, float top)
        {
            DrawTemplate(metafile.GraphicsTemplate, left, top);
        }

        internal void DrawMetafile(Metafile metafile, float left, float top, float width, float height)
        {
            DrawTemplate(metafile.GraphicsTemplate, left, top, width, height);
        }

		[ComVisible(false)]
        internal void DrawMetafile(Metafile metafile, RectangleF rect)
        {
            DrawTemplate(metafile.GraphicsTemplate, rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Draws the specified table at the specified location.
        /// </summary>
        /// <param name="table">The table to draw.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn table.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn table.</param>
        public void DrawTable(Table table, float left, float top)
        {
            TableColumnCollection columns = table.Columns;
            TableRowCollection rows = table.Rows;

            float width = 0.0f;
            float height = table.HeaderHeight;
            for (int i = 0; i < columns.Count; ++i)
                width += columns[i].Width;
            for (int i = 0; i < rows.Count; ++i)
                height += rows[i].Height;
            float widthCurent = 0.0f;
            float heightCurent = table.HeaderHeight;

            DeviceColor borderColor = null;

            if (table.BorderColor != null)
                borderColor = table.BorderColor;
            
            widthCurent = 0.0f;
            for (int i = 0; i < columns.Count; ++i)
            {
                SolidBrush brush = null;
                if (columns[i].BackgroundColor != null)
                {
                    brush = new SolidBrush(columns[i].BackgroundColor);
                }
                else
                {
                    if (table.BackgroundColor != null)
                        brush = new SolidBrush(table.BackgroundColor);
                }
                RectangleF rect = new RectangleF(left + widthCurent, top, columns[i].Width, table.HeaderHeight);
                if (brush != null)
                    DrawRectangle(brush, rect);

				rect.Inflate(-2, 0); // add padding
                DrawString(columns[i].Text, columns[i].Font, new SolidBrush(columns[i].TextColor), rect, columns[i].TextFormat);

                widthCurent += columns[i].Width;
            }

            heightCurent = table.HeaderHeight;
            for (int i = 0; i < rows.Count; ++i)
            {
                widthCurent = 0.0f;
                for (int j = 0; j < columns.Count; ++j)
                {
                    TableElement element = rows[i][columns[j].ColumnName];
                    RectangleF rect = new RectangleF(left + widthCurent, top + heightCurent, columns[j].Width, rows[i].Height);
                    SolidBrush brush = null;
                    if (element.BackgroundColor != null)
                    {
                        brush = new SolidBrush(element.BackgroundColor);
                    }
                    else
                    {
                        if (columns[j].BackgroundColor != null)
                        {
                            brush = new SolidBrush(columns[j].BackgroundColor);
                        }
                        else
                        {
                            if (rows[i].BackgroundColor != null)
                            {
                                brush = new SolidBrush(rows[i].BackgroundColor);
                            }
                            else
                            {
                                if (table.BackgroundColor != null)
                                    brush = new SolidBrush(table.BackgroundColor);
                            }
                        }
                    }
                    if (brush != null)
                        DrawRectangle(brush, rect);

					rect.Inflate(-2, 0); // add padding
                    DrawString(element.Text, element.Font, new SolidBrush(element.TextColor), rect, element.TextFormat);

                    widthCurent += columns[j].Width;
                }
                heightCurent += rows[i].Height;
            }

            if (borderColor != null)
            {
                SolidPen pen = new SolidPen(borderColor, table.BorderWidth);
                if (table.BorderStyle == TableBorderStyle.Dashed)
                    pen.DashPattern = new DashPattern(new float[1] { 3 }, 0);

                DrawRectangle(pen, new RectangleF(left, top, width, height));
                DrawLine(pen, left, top + table.HeaderHeight, left + width, top + table.HeaderHeight);

                widthCurent = 0.0f;
                for (int i = 0; i < columns.Count - 1; ++i)
                {
                    widthCurent += columns[i].Width;
                    DrawLine(pen, left + widthCurent, top, left + widthCurent, top + height);
                }
                heightCurent = table.HeaderHeight;
                for (int i = 0; i < rows.Count - 1; ++i)
                {
                    heightCurent += rows[i].Height;
                    DrawLine(pen, left, top + heightCurent, left + width, top + heightCurent);
                }
            }
        }

        /// <summary>
        /// Draws the specified image, using its original size, at the specified location.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn image.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn image.</param>
        public void DrawImage(Image image, float left, float top)
        {
            DrawImage(image, left, top, image.Width, image.Height);
        }

        /// <summary>
        /// Draws the specified image at the specified location with specified size.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn image.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn image.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the drawn image.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the drawn image.</param>
        public void DrawImage(Image image, float left, float top, float width, float height)
        {
            addOperation(new SaveGraphicsState());
            TranslateTransform(left, top + height);
            ScaleTransform(width, height);
            image.WriteParameters(_stream, _resources);
            if (image.IsSMask)
            {
	            if (ChangeGroup != null) 
					ChangeGroup(this, new ChangeGroupEventArgs(new PDFName("DeviceRGB")));
            }
            addOperation(new RestoreGraphicsState());
        }

        /// <summary>
        /// Draws a template using its original size, at the specified location.
        /// </summary>
        /// <param name="template">The template to draw.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn template.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn template.</param>
        public void DrawTemplate(GraphicsTemplate template, float left, float top)
        {
            addOperation(new SaveGraphicsState());
            TranslateTransform(left, top + template.Height);
            SetClip(0, -template.Height, template.Width, template.Height);
            template.WriteForm(_stream, _resources);
            addOperation(new RestoreGraphicsState());
        }

        /// <summary>
        /// Draws a template at the specified location and size.
        /// </summary>
        /// <param name="template">The template to draw.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn template.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn template.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the drawn template.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the drawn template.</param>
        public void DrawTemplate(GraphicsTemplate template, float left, float top, float width, float height)
        {
            addOperation(new SaveGraphicsState());
            TranslateTransform(left, top + height);
            ScaleTransform(width / template.Width, height / template.Height);
            SetClip(0, -height, template.Width, template.Height);
            template.WriteForm(_stream, _resources);
            addOperation(new RestoreGraphicsState());
        }

        /// <summary>
        /// Saves the current graphics state.
        /// </summary>
        public void SaveGraphicsState()
        {
            _listSaveState.Add(_curentState.Clone() as StateCanvas);
            addOperation(new SaveGraphicsState());
        }

        /// <summary>
        /// Restores the last saved graphics state.
        /// </summary>
        public void RestoreGraphicsState()
        {
            if (_listSaveState.Count == 0)
                return;
            _curentState = _listSaveState[_listSaveState.Count - 1];
            _listSaveState.Remove(_curentState);
            addOperation(new RestoreGraphicsState());
        }

        /// <summary>
        /// Skews the x axis by an angle alpha and the y axis by an angle beta.
        /// </summary>
        /// <param name="alpha" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The alpha angle in degrees.</param>
        /// <param name="beta" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The beta angle in degrees.</param>
        public void SkewTransform(float alpha, float beta)
        {
            alpha *= -(float)Math.PI / 180;
            beta *= -(float)Math.PI / 180;
            addOperation(new Transform(1, (float)Math.Tan(alpha), (float)Math.Tan(beta), 1, 0, 0));
        }

        /// <summary>
        /// Applies the specified rotation to the transformation matrix.
        /// </summary>
        /// <param name="angle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The angle of rotation in degrees.</param>
        public void RotateTransform(float angle)
        {
            angle *= -(float)Math.PI / 180;
            addOperation(new Transform((float)Math.Cos(angle), (float)Math.Sin(angle), -(float)Math.Sin(angle), (float)Math.Cos(angle), 0, 0));
        }

        /// <summary>
        /// Applies the specified scaling operation to the transformation matrix.
        /// </summary>
        /// <param name="scaleX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Scale factor in the x direction.</param>
        /// <param name="scaleY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Scale factor in the y direction.</param>
        public void ScaleTransform(float scaleX, float scaleY)
        {
            addOperation(new Transform(scaleX, 0, 0, scaleY, 0, 0));
        }

        /// <summary>
        /// Changes the origin of the coordinate system by prepending the specified translation to the transformation matrix.
        /// </summary>
        /// <param name="x" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the translation.</param>
        /// <param name="y" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the translation.</param>
        public void TranslateTransform(float x, float y)
        {
            addOperation(new Transform(1, 0, 0, 1, x, -y));
        }

		/// <summary>
		/// Measures the specified string when drawn with the specified Font.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font">Font that defines the format of the string.</param>
		/// <returns>Returns the size of the string in document points (1/72").</returns>
		public SizeF MeasureString(string text, PDF.Font font)
		{
            /*float width = font.GetTextWidth(text);
			float height = font.GetTextHeight();

			return new SizeF(width, height);*/

		    StringFormat stringFormat = new StringFormat();
		    TextString textString = new TextString(text, font, new SizeF(float.MaxValue, float.MaxValue), stringFormat);

		    float width = 0;
		    foreach (string line in textString.Text)
		        width = Math.Max(width, font.GetTextWidth(line));

		    float fontHeight = font.GetTextHeight();
		    PointF[] points = textString.GetCoordinate();

		    return new SizeF(width, points[points.Length - 1].Y + fontHeight * stringFormat.Leading);
        }

        /// <summary>
        /// Measures the specified string limited to the specified width.
        /// </summary>
        /// <param name="text">String to measure.</param>
        /// <param name="font">Font that defines the format of the string.</param>
        /// <param name="maxWidth">Maximum text width. The text will be automatically wrapped at this width.</param>
        /// <returns>Returns the height of the string in document points (1/72").</returns>
        public float MeasureString(string text, PDF.Font font, float maxWidth)
		{
			return MeasureString(text, font, maxWidth, new StringFormat());
		}

		/// <summary>
		/// Measures the specified string limited to the specified width.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font">Font that defines the format of the string.</param>
		/// <param name="maxWidth">Maximum text width. The text will be automatically wrapped at this width.</param>
		/// <param name="stringFormat">The text string format.</param>
		/// <returns>Returns the height of the string in document points (1/72").</returns>
		public float MeasureString(string text, PDF.Font font, float maxWidth, StringFormat stringFormat)
		{
			TextString textString = new TextString(text, font, new SizeF(maxWidth, float.MaxValue), stringFormat);

			float fontHeight = font.GetTextHeight();
			PointF[] points = textString.GetCoordinate();

			return points[points.Length - 1].Y + fontHeight * stringFormat.Leading;
		}

	    /// <summary>
	    /// Draws the specified text string at the specified location with the specified Brush and font objects.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">String to draw.</param>
	    /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
	    /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
	    /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn text.</param>
	    /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn text.</param>
	    public void DrawString(string text, Font font, Brush brush, float left, float top)
	    {
		    this.Brush = brush;
            
		    drawString(text, font, TextRenderingModes.Fill, left, top, PDF.StringFormat.DefaultFormat);
	    }

	    /// <summary>
        /// Draws the specified text string at the specified location with the specified pen and font objects.
        /// </summary>
        /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">String to draw.</param>
        /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the text outline.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn text.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn text.</param>
        public void DrawString(string text, Font font, Pen pen, float left, float top)
        {
            this.Pen = pen;

            drawString(text, font, TextRenderingModes.Stroke, left, top, StringFormat.DefaultFormat);
        }

	    /// <summary>
	    /// Draws the specified text string at the specified location with the specified pen, Brush and font objects.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
	    /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
	    /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
		/// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the text outline.</param>
	    /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn text.</param>
	    /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn text.</param>
	    public void DrawString(string text, Font font, Brush brush, Pen pen, float left, float top)
        {
            this.Pen = pen;
            this.Brush = brush;
            
            drawString(text, font, TextRenderingModes.FillAndStroke, left, top, PDF.StringFormat.DefaultFormat);
        }

	    /// <summary>
	    /// Draws the specified text string at the specified location and size with the specified Brush and font objects.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
	    /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
	    /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
	    /// <param name="layoutRectangle" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that specifies the bounds of the drawn text.</param>
	    [ComVisible(false)]
	    public void DrawString(string text, Font font, Brush brush, RectangleF layoutRectangle)
	    {
		    this.Brush = brush;

		    drawString(text, font, TextRenderingModes.Fill, layoutRectangle.Left, layoutRectangle.Top, layoutRectangle.Width, layoutRectangle.Height, PDF.StringFormat.DefaultFormat);
	    }

	    /// <summary>
        /// Draws the specified text string at the specified location and size with the specified pen and font objects.
        /// </summary>
        /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
        /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
		/// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the text outline.</param>
        /// <param name="layoutRectangle" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that specifies the bounds of the drawn text.</param>
		[ComVisible(false)]
        public void DrawString(string text, Font font, Pen pen, RectangleF layoutRectangle)
        {
            this.Pen = pen;

            drawString(text, font, TextRenderingModes.Stroke, layoutRectangle.Left, layoutRectangle.Top, layoutRectangle.Width, layoutRectangle.Height, PDF.StringFormat.DefaultFormat);
        }

	    /// <summary>
	    /// Draws the specified text string at the specified location and size with the specified pen, Brush and font objects.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
	    /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
	    /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
		/// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the text outline.</param>
	    /// <param name="layoutRectangle" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that specifies the bounds of the drawn text.</param>
	    [ComVisible(false)]
        public void DrawString(string text, Font font, Brush brush, Pen pen, RectangleF layoutRectangle)
        {
            this.Pen = pen;
            this.Brush = brush;

            drawString(text, font, TextRenderingModes.FillAndStroke, layoutRectangle.Left, layoutRectangle.Top, layoutRectangle.Width, layoutRectangle.Height, PDF.StringFormat.DefaultFormat);
        }

	    /// <summary>
	    /// Draws the specified text string at the specified location with the specified Brush and font objects.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
	    /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
	    /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
	    /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn text.</param>
	    /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn text.</param>
	    /// <param name="sf">The text string format.</param>
	    public void DrawString(string text, Font font, Brush brush, float left, float top, StringFormat sf)
	    {
		    this.Brush = brush;

		    drawString(text, font, TextRenderingModes.Fill, left, top, sf);
	    }

	    /// <summary>
        /// Draws the specified text string at the specified location with the specified pen and font objects.
        /// </summary>
        /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
        /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
		/// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the text outline.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn text.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn text.</param>
        /// <param name="sf">The text string format.</param>
        public void DrawString(string text, Font font, Pen pen, float left, float top, StringFormat sf)
        {
            this.Pen = pen;

            drawString(text, font, TextRenderingModes.Stroke, left, top, sf);
        }

	    /// <summary>
	    /// Draws the specified text string at the specified location with the specified pen, Brush and font objects.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
	    /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
	    /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
		/// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the text outline.</param>
	    /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the drawn text.</param>
	    /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the drawn text.</param>
	    /// <param name="sf">The text string format.</param>
	    public void DrawString(string text, Font font, Brush brush, Pen pen, float left, float top, StringFormat sf)
        {
            this.Pen = pen;
            this.Brush = brush;

            drawString(text, font, TextRenderingModes.FillAndStroke, left, top, sf);
        }

	    /// <summary>
	    /// Draws the specified text string at the specified location and size with the specified Brush and font objects.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
	    /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
	    /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
	    /// <param name="layoutRectangle" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that specifies the bounds of the drawn text.</param>
	    /// <param name="sf">The text string format.</param>
	    [ComVisible(false)]
	    public void DrawString(string text, Font font, Brush brush, RectangleF layoutRectangle, StringFormat sf)
	    {
		    this.Brush = brush;

		    drawString(text, font, TextRenderingModes.Fill, layoutRectangle.Left, layoutRectangle.Top, layoutRectangle.Width, layoutRectangle.Height, sf);
	    }

	    /// <summary>
        /// Draws the specified text string at the specified location and size with the specified pen and font objects.
        /// </summary>
        /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
        /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
		/// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the text outline.</param>
        /// <param name="layoutRectangle" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that specifies the bounds of the drawn text.</param>
        /// <param name="sf">The text string format.</param>
		[ComVisible(false)]
		public void DrawString(string text, Font font, Pen pen, RectangleF layoutRectangle, StringFormat sf)
        {
            this.Pen = pen;

            drawString(text, font, TextRenderingModes.Stroke, layoutRectangle.Left, layoutRectangle.Top, layoutRectangle.Width, layoutRectangle.Height, sf);
        }

	    /// <summary>
	    /// Draws the specified text string at the specified location and size with the specified pen, Brush and font objects.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
	    /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
	    /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
		/// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the text outline.</param>
	    /// <param name="layoutRectangle" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that specifies the bounds of the drawn text.</param>
	    /// <param name="sf">The text string format.</param>
	    [ComVisible(false)]
		public void DrawString(string text, Font font, Brush brush, Pen pen, RectangleF layoutRectangle, StringFormat sf)
        {
            this.Pen = pen;
            this.Brush = brush;

            drawString(text, font, TextRenderingModes.FillAndStroke, layoutRectangle.Left, layoutRectangle.Top, layoutRectangle.Width, layoutRectangle.Height, sf);
        }

	    /// <summary>
	    /// Draws the specified text string at the specified location and size with the specified Brush and font objects.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
	    /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
	    /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
		/// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the target rectangle to draw the text.</param>
		/// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the target rectangle to draw the text.</param>
	    /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the target rectangle to draw the text.</param>
		/// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the target rectangle to draw the text.</param>
	    /// <param name="sf" >The text string format.</param>
	    public void DrawString(string text, Font font, Brush brush, float left, float top, float width, float height, StringFormat sf)
		{
			this.Brush = brush;

			drawString(text, font, TextRenderingModes.Fill, left, top, width, height, sf);
		}

		/// <summary>
		/// Draws the specified text string at the specified location and size with the specified pen and font objects.
		/// </summary>
		/// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
		/// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
		/// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the text outline.</param>
		/// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the target rectangle to draw the text.</param>
		/// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the target rectangle to draw the text.</param>
		/// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the target rectangle to draw the text.</param>
		/// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the target rectangle to draw the text.</param>
		/// <param name="sf">The text string format.</param>
		public void DrawString(string text, Font font, Pen pen, float left, float top, float width, float height, StringFormat sf)
		{
			this.Pen = pen;

			drawString(text, font, TextRenderingModes.Stroke, left, top, width, height, sf);
		}

	    /// <summary>
	    /// Draws the specified text string at the specified location and size with the specified pen, Brush and font objects.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The string to draw.</param>
	    /// <param name="font">Bytescout.PDF.Font that defines the text format of the string.</param>
	    /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
		/// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the text outline.</param>
		/// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the target rectangle to draw the text.</param>
		/// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the target rectangle to draw the text.</param>
		/// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the target rectangle to draw the text.</param>
		/// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the target rectangle to draw the text.</param>
	    /// <param name="sf">The text string format.</param>
	    public void DrawString(string text, Font font, Brush brush, Pen pen, float left, float top, float width, float height, StringFormat sf)
	    {
			this.Pen = pen;
			this.Brush = brush;

			drawString(text, font, TextRenderingModes.FillAndStroke, left, top, width, height, sf);
	    }

        /// <summary>
        /// Draws a line connecting the two points specified by the coordinate pairs.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the line.</param>
        /// <param name="x1" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the first point.</param>
        /// <param name="y1" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the first point.</param>
        /// <param name="x2" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the second point.</param>
        /// <param name="y2" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the second point.</param>
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            this.Pen = pen;
            moveTo(x1, y1);
            drawLineTo(x2, y2);
            stroke();
        }

        /// <summary>
        /// Draws a line connecting two System.Drawing.PointF structures.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the line.</param>
        /// <param name="pt1" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">System.Drawing.PointF structure that represents the first point to connect.</param>
        /// <param name="pt2" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">System.Drawing.PointF structure that represents the second point to connect.</param>
		[ComVisible(false)]
		public void DrawLine(Pen pen, PointF pt1, PointF pt2)
        {
            DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the rectangle.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the rectangle to draw.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the rectangle to draw.</param>
        public void DrawRectangle(Pen pen, float left, float top, float width, float height)
        {
            this.Pen = pen;
            drawRectangle(left, top, width, height);
            stroke();
        }

        /// <summary>
        /// Draws a rectangle specified by a System.Drawing.Rectangle structure.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the rectangle.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.Rectangle structure that represents the rectangle to draw.</param>
		[ComVisible(false)]
		public void DrawRectangle(Pen pen, RectangleF rect)
        {
            this.Pen = pen;
            drawRectangle(rect);
            stroke();
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the rectangle to draw.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the rectangle to draw.</param>
        public void DrawRectangle(Brush brush, float left, float top, float width, float height)
        {
            this.Brush = brush;
            drawRectangle(left, top, width, height);
            fill();
        }

        /// <summary>
        /// Draws a rectangle specified by a System.Drawing.Rectangle structure.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.Rectangle structure that represents the rectangle to draw.</param>
		[ComVisible(false)]
		public void DrawRectangle(Brush brush, RectangleF rect)
        {
            this.Brush = brush;
            drawRectangle(rect);
            fill();
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the rectangle.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the rectangle to draw.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the rectangle to draw.</param>
        public void DrawRectangle(Pen pen, Brush brush, float left, float top, float width, float height)
        {
            this.Pen = pen;
            this.Brush = brush;
            drawRectangle(left, top, width, height);
            fillAndStroke();
        }

        /// <summary>
        /// Draws a rectangle specified by a System.Drawing.Rectangle structure.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the rectangle.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.Rectangle structure that represents the rectangle to draw.</param>
		[ComVisible(false)]
		public void DrawRectangle(Pen pen, Brush brush, RectangleF rect)
        {
            this.Pen = pen;
            this.Brush = brush;
            drawRectangle(rect);
            fillAndStroke();
        }

        /// <summary>
        /// Draws a cubic Bézier curve.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the curve.</param>
        /// <param name="x1" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the starting point of the curve.</param>
        /// <param name="y1" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the starting point of the curve.</param>
        /// <param name="x2" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the first control point of the curve.</param>
        /// <param name="y2" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the first control point of the curve.</param>
        /// <param name="x3" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the second control point of the curve.</param>
        /// <param name="y3" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the second control point of the curve.</param>
        /// <param name="x4" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the ending point of the curve.</param>
        /// <param name="y4" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the ending point of the curve.</param>
        public void DrawCurve(Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            this.Pen = pen;
            moveTo(x1, y1);
            drawCurveTo(x2, y2, x3, y3, x4, y4);
            stroke();
        }

        /// <summary>
        /// Draws a cubic Bézier curve.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the curve.</param>
        /// <param name="pt1" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The starting point of the curve.</param>
        /// <param name="pt2" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The first control point for the curve.</param>
        /// <param name="pt3" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The second control point for the curve.</param>
        /// <param name="pt4" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The ending point of the curve.</param>
		[ComVisible(false)]
		public void DrawCurve(Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
        {
            this.Pen = pen;
            moveTo(pt1.X, pt1.Y);
            drawCurveTo(pt2, pt3, pt4);
            stroke();
        }

        /// <summary>
        /// Draws an ellipse defined by a bounding rectangle specified by a pair of coordinates, a height, and a width.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the ellipse.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Height of the bounding rectangle that defines the ellipse.</param>
        public void DrawEllipse(Pen pen, float left, float top, float width, float height)
        {
            this.Pen = pen;
            drawEllipse(left, top, width, height);
            stroke();
        }

        /// <summary>
        /// Draws an ellipse defined by a bounding System.Drawing.RectangleF.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the ellipse.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that defines the boundaries of the ellipse.</param>
		[ComVisible(false)]
		public void DrawEllipse(Pen pen, RectangleF rect)
        {
            this.Pen = pen;
            drawEllipse(rect);
            stroke();
        }

        /// <summary>
        /// Draws an ellipse defined by a bounding rectangle specified by a pair of coordinates, a height, and a width.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Height of the bounding rectangle that defines the ellipse.</param>
        public void DrawEllipse(Brush brush, float left, float top, float width, float height)
        {
            this.Brush = brush;
            drawEllipse(left, top, width, height);
            fill();
        }

        /// <summary>
        /// Draws an ellipse defined by a bounding System.Drawing.RectangleF.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that defines the boundaries of the ellipse.</param>
		[ComVisible(false)]
		public void DrawEllipse(Brush brush, RectangleF rect)
        {
            this.Brush = brush;
            drawEllipse(rect);
            fill();
        }

        /// <summary>
        /// Draws an ellipse defined by a bounding rectangle specified by a pair of coordinates, a height, and a width.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the ellipse.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Height of the bounding rectangle that defines the ellipse.</param>
        public void DrawEllipse(Pen pen, Brush brush, float left, float top, float width, float height)
        {
            this.Pen = pen;
            this.Brush = brush;
            drawEllipse(left, top, width, height);
            fillAndStroke();
        }

        /// <summary>
        /// Draws an ellipse defined by a bounding System.Drawing.RectangleF.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the ellipse.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that defines the boundaries of the ellipse.</param>
		[ComVisible(false)]
		public void DrawEllipse(Pen pen, Brush brush, RectangleF rect)
        {
            this.Pen = pen;
            this.Brush = brush;
            drawEllipse(rect);
            fillAndStroke();
        }

        /// <summary>
        /// Draws a circle with specified center coordinates and radius.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the circle.</param>
        /// <param name="centerX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the center of the circle.</param>
        /// <param name="centerY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the center of the circle.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the circle.</param>
        public void DrawCircle(Pen pen, float centerX, float centerY, float radius)
        {
            this.Pen = pen;
            drawCircle(centerX, centerY, radius);
            stroke();
        }

        /// <summary>
        /// Draws a circle with specified center coordinates and radius.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the circle.</param>
        /// <param name="center" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The circle center.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the circle.</param>
		[ComVisible(false)]
        public void DrawCircle(Pen pen, PointF center, float radius)
        {
            this.Pen = pen;
            drawCircle(center, radius);
            stroke();
        }

        /// <summary>
        /// Draws a circle with specified center coordinates and radius.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="centerX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the center of the circle.</param>
        /// <param name="centerY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the center of the circle.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the circle.</param>
        public void DrawCircle(Brush brush, float centerX, float centerY, float radius)
        {
            this.Brush = brush;
            drawCircle(centerX, centerY, radius);
            fill();
        }

        /// <summary>
        /// Draws a circle with specified center coordinates and radius.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="center" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The circle center.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the circle.</param>
		[ComVisible(false)]
        public void DrawCircle(Brush brush, PointF center, float radius)
        {
            this.Brush = brush;
            drawCircle(center, radius);
            fill();
        }

        /// <summary>
        /// Draws a circle with specified center coordinates and radius.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the circle.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="centerX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the center of the circle.</param>
        /// <param name="centerY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the center of the circle.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the circle.</param>
        public void DrawCircle(Pen pen, Brush brush, float centerX, float centerY, float radius)
        {
            this.Pen = pen;
            this.Brush = brush;
            drawCircle(centerX, centerY, radius);
            fillAndStroke();
        }

        /// <summary>
        /// Draws a circle with specified center coordinates and radius.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the circle.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="center" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The circle center.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the circle.</param>
		[ComVisible(false)]
        public void DrawCircle(Pen pen, Brush brush, PointF center, float radius)
        {
            this.Pen = pen;
            this.Brush = brush;
            drawCircle(center, radius);
            fillAndStroke();
        }

        /// <summary>
        /// Draws a pie shape.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the pie.</param>
        /// <param name="centerX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the center of the ellipse.</param>
        /// <param name="centerY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the center of the ellipse.</param>
        /// <param name="radiusX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the ellipse measured along the x-axis.</param>
        /// <param name="radiusY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the ellipse measured along the y-axis.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
        public void DrawPie(Pen pen, float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            this.Pen = pen;
            drawPie(centerX, centerY, radiusX, radiusY, startAngle, sweepAngle);
            stroke();
        }

        /// <summary>
        /// Draws a pie shape defined by an ellipse specified by a System.Drawing.RectangleF structure and two radial lines.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the pie.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that represents the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
		[ComVisible(false)]
		public void DrawPie(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
        {
            this.Pen = pen;
            drawPie(rect, new SizeF(startAngle, sweepAngle));
            stroke();
        }

        /// <summary>
        /// Draws a pie shape.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="centerX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the center of the ellipse.</param>
        /// <param name="centerY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the center of the ellipse.</param>
        /// <param name="radiusX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the ellipse measured along the x-axis.</param>
        /// <param name="radiusY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the ellipse measured along the y-axis.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
        public void DrawPie(Brush brush, float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            this.Brush = brush;
            drawPie(centerX, centerY, radiusX, radiusY, startAngle, sweepAngle);
            fill();
        }

        /// <summary>
        /// Draws a pie shape defined by an ellipse specified by a System.Drawing.RectangleF structure and two radial lines.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that represents the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
		[ComVisible(false)]
		public void DrawPie(Brush brush, RectangleF rect, float startAngle, float sweepAngle)
        {
            this.Brush = brush;
            drawPie(rect, new SizeF(startAngle, sweepAngle));
            fill();
        }

        /// <summary>
        /// Draws a pie shape.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the pie.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="centerX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the center of the ellipse.</param>
        /// <param name="centerY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the center of the ellipse.</param>
        /// <param name="radiusX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the ellipse measured along the x-axis.</param>
        /// <param name="radiusY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the ellipse measured along the y-axis.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
        public void DrawPie(Pen pen, Brush brush, float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            this.Pen = pen;
            this.Brush = brush;
            drawPie(centerX, centerY, radiusX, radiusY, startAngle, sweepAngle);
            fillAndStroke();
        }

        /// <summary>
        /// Draws a pie shape defined by an ellipse specified by a System.Drawing.RectangleF structure and two radial lines.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the pie.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that represents the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
		[ComVisible(false)]
		public void DrawPie(Pen pen, Brush brush, RectangleF rect, float startAngle, float sweepAngle)
        {
            this.Pen = pen;
            this.Brush = brush;
            drawPie(rect, new SizeF(startAngle, sweepAngle));
            fillAndStroke();
        }

        /// <summary>
        /// Draws an arc representing a portion of an ellipse.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the arc.</param>
        /// <param name="centerX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the center of the ellipse.</param>
        /// <param name="centerY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the center of the ellipse.</param>
        /// <param name="radiusX" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the ellipse measured along the x-axis.</param>
        /// <param name="radiusY" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the ellipse measured along the y-axis.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle in degrees measured clockwise from the x-axis to the starting point of the arc.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle in degrees measured clockwise from the startAngle parameter to the ending point of the arc.</param>
        public void DrawArc(Pen pen, float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            this.Pen = pen;
            DrawArc(centerX, centerY, radiusX, radiusY, startAngle, sweepAngle);
            stroke();
        }

        /// <summary>
        /// Draws an arc representing a portion of an ellipse specified by a System.Drawing.RectangleF structure.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the arc.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">System.Drawing.RectangleF structure that defines the boundaries of the ellipse.</param>
        /// <param name="startAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle in degrees measured clockwise from the x-axis to the starting point of the arc.</param>
        /// <param name="sweepAngle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">Angle in degrees measured clockwise from the startAngle parameter to the ending point of the arc.</param>
		[ComVisible(false)]
		public void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
        {
            this.Pen = pen;
            drawArc(rect, new SizeF(startAngle, sweepAngle));
            stroke();
        }

        /// <summary>
        /// Draws a rectangle structure with rounded corners specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the rectangle.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the rectangle to draw.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the rectangle to draw.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the corner circle.</param>
        public void DrawRoundedRectangle(Pen pen, float left, float top, float width, float height, float radius)
        {
            this.Pen = pen;
            drawRoundedRectangle(left, top, width, height, radius);
            stroke();
        }

        /// <summary>
        /// Draws a rectangle structure with rounded corners specified by a System.Drawing.RectangleF structure.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the rectangle.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.Rectangle structure that represents the rectangle to draw.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the corner circle.</param>
		[ComVisible(false)]
		public void DrawRoundedRectangle(Pen pen, RectangleF rect, float radius)
        {
            this.Pen = pen;
            drawRoundedRectangle(rect, radius);
            stroke();
        }

        /// <summary>
        /// Draws a rectangle structure with rounded corners specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the rectangle to draw.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the rectangle to draw.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the corner circle.</param>
        public void DrawRoundedRectangle(Brush brush, float left, float top, float width, float height, float radius)
        {
            this.Brush = brush;
            drawRoundedRectangle(left, top, width, height, radius);
            fill();
        }

        /// <summary>
        /// Draws a rectangle structure with rounded corners specified by a System.Drawing.RectangleF structure.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.Rectangle structure that represents the rectangle to draw.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the corner circle.</param>
		[ComVisible(false)]
		public void DrawRoundedRectangle(Brush brush, RectangleF rect, float radius)
        {
            this.Brush = brush;
            drawRoundedRectangle(rect, radius);
            fill();
        }

        /// <summary>
        /// Draws a rectangle structure with rounded corners specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the rectangle.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the rectangle to draw.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the rectangle to draw.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the corner circle.</param>
        public void DrawRoundedRectangle(Pen pen, Brush brush, float left, float top, float width, float height, float radius)
        {
            this.Pen = pen;
            this.Brush = brush;
            drawRoundedRectangle(left, top, width, height, radius);
            fillAndStroke();
        }

        /// <summary>
        /// Draws a rectangle structure with rounded corners specified by a System.Drawing.RectangleF structure.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the rectangle.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">A System.Drawing.Rectangle structure that represents the rectangle to draw.</param>
        /// <param name="radius" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The radius of the corner circle.</param>
		[ComVisible(false)]
		public void DrawRoundedRectangle(Pen pen, Brush brush, RectangleF rect, float radius)
        {
            this.Pen = pen;
            this.Brush = brush;
            drawRoundedRectangle(rect, radius);
            fillAndStroke();
        }

        /// <summary>
        /// Draws a Bytescout.PDF.Path.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the path.</param>
        /// <param name="path">Bytescout.PDF.Path to draw.</param>
        public void DrawPath(Pen pen, Path path)
        {
            this.Pen = pen;
            IPDFPageOperation[] operations = path.Operations;
            for (int i = 0; i < operations.Length; ++i)
                addOperation(operations[i]);
            stroke();
        }

        /// <summary>
        /// Draws a Bytescout.PDF.Path.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="path">Bytescout.PDF.Path to draw.</param>
        public void DrawPath(Brush brush, Path path)
        { 
            this.Brush = brush;
            IPDFPageOperation[] operations = path.Operations;
            for (int i = 0; i < operations.Length; ++i)
                addOperation(operations[i]);
            if (path.FillMode == FillMode.Alternate)
                alternateFill();
            else if (path.FillMode == FillMode.Winding)
                fill();
        }

        /// <summary>
        /// Draws a Bytescout.PDF.Path.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the path.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="path">Bytescout.PDF.Path to draw.</param>
        public void DrawPath(Pen pen, Brush brush, Path path)
        {
            this.Pen = pen;
            this.Brush = brush;
            IPDFPageOperation[] operations = path.Operations;
            for (int i = 0; i < operations.Length; ++i)
                addOperation(operations[i]);
            if (path.FillMode == FillMode.Alternate)
                alternateFillAndStroke();
            else if (path.FillMode == FillMode.Winding)
                fillAndStroke();
        }

        /// <summary>
        /// Modifies the current clipping path by intersecting it with the current path.
        /// </summary>
        /// <param name="path">Clip path.</param>
        public void SetClip(Path path)
        {
            IPDFPageOperation[] operations = path.Operations;
            for (int i = 0; i < operations.Length; ++i)
                addOperation(operations[i]);
            if (path.FillMode == FillMode.Alternate)
                alternateClip();
            else if (path.FillMode == FillMode.Winding)
                clip();
            endPath();
        }

        /// <summary>
        /// Draws a polygon defined by an array of System.Drawing.PointF structures.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the polygon.</param>
        /// <param name="points">Array of points that represent the vertices of the polygon.</param>
        public void DrawPolygon(Pen pen, PointsArray points)
        {
            Pen = pen;
            drawPolygon(points.ToArray(), points.Count);
            stroke();
        }
		
		/// <summary>
        /// Draws a polygon defined by an array of System.Drawing.PointF structures.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the polygon.</param>
        /// <param name="points" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">Array of System.Drawing.PointF structures that represent the vertices of the polygon.</param>
        [ComVisible(false)]
        public void DrawPolygon(Pen pen, PointF[] points)
        {
            Pen = pen;
            drawPolygon(points, points.Length);
            stroke();
        }

        /// <summary>
        /// Draws a polygon defined by an array of System.Drawing.PointF structures.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="points">Array of points that represent the vertices of the polygon.</param>
        public void DrawPolygon(Brush brush, PointsArray points)
        {
            Brush = brush;
            drawPolygon(points.ToArray(), points.Count);
            fill();
        }
		
		/// <summary>
        /// Draws a polygon defined by an array of System.Drawing.PointF structures.
        /// </summary>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="points" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">Array of System.Drawing.PointF structures that represent the vertices of the polygon.</param>
		[ComVisible(false)]
        public void DrawPolygon(Brush brush, PointF[] points)
        {
            Brush = brush;
            drawPolygon(points, points.Length);
            fill();
        }

        /// <summary>
        /// Draws a polygon defined by an array of System.Drawing.PointF structures.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the polygon.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="points">Array of points that represent the vertices of the polygon.</param>
        public void DrawPolygon(Pen pen, Brush brush, PointsArray points)
        {
            Pen = pen;
            Brush = brush;
            drawPolygon(points.ToArray(), points.Count);
            fillAndStroke();
        }
		
		/// <summary>
        /// Draws a polygon defined by an array of System.Drawing.PointF structures.
        /// </summary>
        /// <param name="pen">Bytescout.PDF.Pen that determines the color, width, and style of the polygon.</param>
        /// <param name="brush">Bytescout.PDF.Brush that determines the characteristics of the fill.</param>
        /// <param name="points" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">Array of System.Drawing.PointF structures that represent the vertices of the polygon.</param>
		[ComVisible(false)]
        public void DrawPolygon(Pen pen, Brush brush, PointF[] points)
        {
            Pen = pen;
            Brush = brush;
            drawPolygon(points, points.Length);
            fillAndStroke();
        }

        /// <summary>
        /// Modifies the current clipping path by intersecting it with the current path.
        /// </summary>
        /// <param name="rect" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Clip rectangle.</param>
		[ComVisible(false)]
        public void SetClip(RectangleF rect)
        {
            SetClip(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Initializes the creation of content group.
        /// </summary>
        /// <param name="layer">The layer.</param>
        public void BeginMarkedContent(Layer layer)
        {
            string str = _resources.AddResources(ResourceType.Properties, layer.GetDictionary());
            addOperation(new BeginMarkedContentSequenceWithProperties("OC", new PDFName(str)));
        }

        /// <summary>
        /// Finalizes an optional content group.
        /// </summary>
        public void EndMarkedContent()
        {
            addOperation(new EndMarkedContentSequence());
        }

        internal void Transform(float a, float b, float c, float d, float e, float f)
        {
            addOperation(new Transform(a, b, c, d, e, f));
        }

		/// <summary>
		/// Modifies the current clipping path by intersecting it with the current path.
		/// </summary>
	    public void SetClip(float left, float top, float width, float height)
        {
            drawRectangle(left, top, width, height);
            clip();
            endPath();
        }

        internal void SetSize(RectangleF rect)
        {
            _rect = rect;
        }

        internal void BeforeLicense()
        {
            _stream.Position = _stream.Length;
            for (int i = 0; i <= _listSaveState.Count; ++i)
            {
                _stream.WriteByte((byte)'\r');
                _stream.WriteByte((byte)'Q');
            }

            _listSaveState.Clear();
            _curentState = new StateCanvas();

            _curentState = new StateCanvas();
            _listSaveState = new List<StateCanvas>();
            addOperation(new Transform(1, 0, 0, 1, _rect.Left, _rect.Top + _rect.Height));
        }

        internal void BeginStringEdit()
        {
            addOperation(new RestoreGraphicsState());
            addOperation(new BeginMarkedContentSequence("Tx"));
            SaveGraphicsState();
        }

        internal void EndStringEdit()
        {
            RestoreGraphicsState();
            addOperation(new EndMarkedContentSequence());
            addOperation(new SaveGraphicsState());
        }

        private void drawPolygon(float[] pointsX, float[] pointsY, int count)
        {
            if (count > 1)
            {
                moveTo(pointsX[0], pointsY[0]);
                for (int i = 1; i < count; ++i)
                {
                    drawLineTo(pointsX[i], pointsY[i]);
                }
                closePath();
            }
        }

        private void drawPolygon(PointF[] points, int count)
        {
            if (count > 1)
            {
                moveTo(points[0].X, points[0].Y);
                for (int i = 1; i < count; ++i)
                {
                    drawLineTo(points[i].X, points[i].Y);
                }
                closePath();
            }
        }

        private StringFormat StringFormat
        {
            get
            {
                return _curentState.StringFormat;
            }
            set
            {
                _curentState.StringFormat.WriteChanges(value, _stream, _resources);
                _curentState.StringFormat = value.Clone() as StringFormat;
            }
        }

        private void moveTo(float x, float y)
        {
            addOperation(new MoveTo(x, -y));
        }

        private void drawLineTo(float x, float y)
        {
            addOperation(new LineTo(x, -y));
        }

        private void drawRectangle(float left, float top, float width, float height)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException("width");
            if (height < 0)
                throw new ArgumentOutOfRangeException("height");
            addOperation(new Rectangle(left, -(top + height), width, height));
        }

        private void drawRectangle(RectangleF rectangle)
        {
            drawRectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
        }

        private void drawCurveTo(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            addOperation(new BezierCurve(x1, -y1, x2, -y2, x3, -y3));
        }

        private void drawCurveTo(PointF point1, PointF point2, PointF point3)
        {
            drawCurveTo(point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y);
        }

        private void drawEllipse(float left, float top, float width, float height)
        {
            DrawArc(left + width / 2, top + height / 2, width / 2, height / 2, 0, 360);
        }

        private void drawEllipse(PointF center, float radiusX, float radiusY)
        {
            DrawArc(center.X, center.Y, radiusX, radiusY, 0, 360);
        }

        private void drawEllipse(RectangleF rectangle)
        {
            drawEllipse(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
        }

        private void drawCircle(float centerX, float centerY, float radius)
        {
            DrawArc(centerX, centerY, radius, radius, 0, 360);
        }

        private void drawCircle(PointF point, float radius)
        {
            drawCircle(point.X, point.Y, radius);
        }

        private void drawPie(float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            addOperation(new DrawPie(centerX, -centerY, radiusX, radiusY, -startAngle, -sweepAngle));
        }

        private void drawPie(RectangleF rectangle, SizeF angle)
        {
            drawPie(rectangle.Left + rectangle.Width / 2, rectangle.Top + rectangle.Height / 2, rectangle.Width / 2, rectangle.Height / 2, angle.Width, angle.Height);
        }

        private void DrawArc(float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle)
        {
            addOperation(new DrawArc(centerX, -centerY, radiusX, radiusY, -startAngle, -sweepAngle));
        }

        private void drawArc(RectangleF rectangle, SizeF angle)
        {
            DrawArc(rectangle.Left + rectangle.Width / 2, rectangle.Top + rectangle.Height / 2, rectangle.Width / 2, rectangle.Height / 2, angle.Width, angle.Height);
        }

        private void drawRoundedRectangle(float left, float top, float width, float height, float radius)
        {
            addOperation(new DrawRoundRectangle(left, -(top + height), width, height, radius));
        }

        private void drawRoundedRectangle(RectangleF rectangle, float radius)
        { 
            drawRoundedRectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, radius);
        }


        private void stroke()
        {
            addOperation(new StrokePath());
        }

        private void fill()
        {
            addOperation(new FillPathNonZero());
        }

        private void fillAndStroke()
        {
            addOperation(new FillStrokePathNonZero());
        }

        private void alternateFill()
        {
            addOperation(new FillPathEvenOdd());
        }

        private void alternateFillAndStroke()
        {
            addOperation(new FillStrokePathEvenOdd());
        }

        private void closeFillAndStroke()
        {
            addOperation(new CloseFillStrokePathNonZero());
        }

        private void closeAlternateFillAndStroke()
        {
            addOperation(new CloseFillStrokePathEvenOdd());
        }

        private void closePath()
        {
            addOperation(new CloseSubpath());
        }

        private void endPath()
        {
            addOperation(new EndPath());
        }

        private void clip()
        {
            addOperation(new ClipPathNonZero());
        }

        private void alternateClip()
        {
            addOperation(new ClipPathEvenOdd());
        }

        internal void addOperation (IPDFPageOperation operation)
        {
            operation.WriteBytes(_stream);
        }

        private float beginDSFontSetup(Font font, TextRenderingModes rm, float left, float top)
        {
            float alpha = 0;
            if (font.BaseFont is TrueTypeFont)
            {
                if ((!(font.BaseFont as TrueTypeFont).RealBold && font.Bold) || (!(font.BaseFont as TrueTypeFont).RealItalic && font.Italic))
                    SaveGraphicsState();
            }
            if (font.BaseFont is TrueTypeFont)
            {
                if (!(font.BaseFont as TrueTypeFont).RealItalic && font.Italic)
                {
                    alpha = 15;
                    TranslateTransform(left, top);
                    SkewTransform(0, -alpha);
                    TranslateTransform(-left, -top);
                }
            }
            return alpha;
        }

        private void endDSFontSetup(Font font)
        {
            if (font.BaseFont is TrueTypeFont)
            {
                if ((!(font.BaseFont as TrueTypeFont).RealBold && font.Bold) || (!(font.BaseFont as TrueTypeFont).RealItalic && font.Italic))
                {
                    RestoreGraphicsState();
                }
            }
        }

        private void drawText(string text, Font font, TextRenderingModes rm, float left, float top, float alpha, TextString textString, StringFormat sf)
        {
            //DrawText
            addOperation(new BeginText());

            if (font.BaseFont is TrueTypeFont)
            {
                if (!(font.BaseFont as TrueTypeFont).RealBold && font.Bold)
                {
                    addOperation(new Linewidth(font.Size * 0.025f));
                    if (rm != TextRenderingModes.Stroke)
                        rm = TextRenderingModes.FillAndStroke;
                }
            }

            font.WriteParameters(_stream, _resources);
            addOperation(new TextRenderingMode((int)rm));
            this.StringFormat = sf;
            float heightText = font.GetTextHeight();
            addOperation(new TextLeading(heightText * this.StringFormat.Leading));
            
            font.BaseFont.AddStringToEncoding(text);
            
            string[] lines = textString.Text;

            PointF[] points = textString.GetCoordinate();
            top += points[0].Y;
            addOperation(new MoveTextPos(left, -top));
            float curentLeft = 0;
            float prevLeft = 0;
            
            for (int i = 0; i < textString.Count; ++i)
            {
                addOperation(new MoveTextPosToNextLine());
                curentLeft = points[i].X;
                if (alpha != 0)
                    addOperation(new MoveTextPos((i + 1) * font.GetTextHeight() * (float)Math.Tan(alpha * Math.PI / 180), 0));
                if (curentLeft - prevLeft != 0)
                    addOperation(new MoveTextPos(curentLeft - prevLeft, 0));

                object[] arrayWords = textString.GetArrayWords(i);
                addOperation(new ShowTextStrings(arrayWords));

                prevLeft = points[i].X;
            }

            addOperation(new EndText());

            endDSFontSetup(font);

            //DrawLines
            if (rm == TextRenderingModes.Stroke)
                Pen.WriteForNotStroking(_stream, _resources);

            for (int i = 0; i < textString.Count; ++i)
            {
                drawLinesString(lines[i], font, top + points[i].Y - points[0].Y + heightText, left + points[i].X, StringFormat);
            }
        }

        private void drawString(string text, Font font, TextRenderingModes rm, float left, float top, StringFormat sf)
        {
            if (string.IsNullOrEmpty(text))
                return;

            float alpha = beginDSFontSetup(font, rm, left, top);

            TextString textString = new TextString(text, font, sf);
            drawText(text, font, rm, left, top, alpha, textString, sf);
        }

        private void drawString(string text, Font font, TextRenderingModes rm, float left, float top, float width, float height, StringFormat sf)
        {
            if (string.IsNullOrEmpty(text))
                return;

            float alpha = beginDSFontSetup(font, rm, left, top);

            SaveGraphicsState();
            Path path = new Path(FillMode.Winding);
            path.AddRectangle(left, top, width, height);
            SetClip(path);

            TextString textString = new TextString(text, font, new SizeF(width, height), sf);
            drawText(text, font, rm, left, top, alpha, textString, sf);

            RestoreGraphicsState();

            endDSFontSetup(font);
        }

        private void drawLinesString(string text, Font font, float top, float left, StringFormat sf)
        {
            int countSpace = TextString.GetSpaceCount(text);
            float width = (font.GetTextWidth(text) + sf.CharacterSpacing * (text.Length - 1) + sf.WordSpacing * countSpace) * sf.Scaling / 100;
            if (font.Underline)
            {
                drawRectangle(left, top + font.GetTextHeight() / 2 - font.Size * 0.15f - sf.Rise, width, font.Size * 0.05f);
                fill();
            }
            if (font.Strikeout)
            {
                drawRectangle(left, top - font.Size * 0.15f - sf.Rise, width, font.Size * 0.05f);
                fill();
            }
        }
    }
}
