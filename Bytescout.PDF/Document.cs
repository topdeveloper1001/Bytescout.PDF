using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Document : IDisposable
#else
	/// <summary>
	/// Represents a PDF document.
	/// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public sealed class Document : IDisposable
#endif
	{
        internal RegInfo RegInfo { get; set; } = new RegInfo();

        private DocumentInformation _docInfo;
		private Catalog _catalog;
		private Security _security;
		private Compression _compression = Compression.Flate;
		private FileStream _fileStream; // keep the file stream to avoid disposing

		private string _signature;
        private Certificate _certificate;

#if !PDFSDK_EMBEDDED_SOURCES

		///<summary>
		/// Registration name.
		///</summary>
		public string RegistrationName
		{
			get { return RegInfo.RegistrationName; }
			set { RegInfo.RegistrationName = value; }
		}

		///<summary>
		/// Registration key.
		///</summary>
		public string RegistrationKey
		{
			get { return RegInfo.RegistrationKey; }
			set { RegInfo.RegistrationKey = value; }
		}

#endif

		/// <summary>
		/// Offers access to the document information object where the document's title, subject, keywords, etc. can be set.
		/// </summary>
		/// <value cref="Bytescout.PDF.DocumentInformation"></value>
		public DocumentInformation DocumentInfo
		{
			get { return _docInfo; }
		}

		/// <summary>
		/// Offers access to security features of the PDF document such as encryption and digital signatures.
		/// </summary>
		/// <value cref="Bytescout.PDF.Security"></value>
		public Security Security
		{
			get { return _security; }
		}

		/// <summary>
		/// Gets the document pages collection.
		/// </summary>
		/// <value cref="Bytescout.PDF.PageCollection"></value>
		public PageCollection Pages
		{
			get { return _catalog.Pages; }
		}

		/// <summary>
		/// Gets or sets the document compression to use when saving the document.
		/// </summary>
		/// <value cref="Bytescout.PDF.Compression"></value>
		public Compression Compression
		{
			get { return _compression; }
			set { _compression = value; }
		}

		/// <summary>
		/// Offers access to viewer preferences of this document.
		/// </summary>
		/// <value cref="Bytescout.PDF.ViewerPreferences"></value>
		public ViewerPreferences ViewerPreferences
		{
			get { return _catalog.ViewerPreferences; }
		}

		/// <summary>
		/// Gets the root collection of the outline items.
		/// </summary>
		/// <value cref="Bytescout.PDF.OutlinesCollection"></value>
		public OutlinesCollection Outlines
		{
			get { return _catalog.Outlines; }
		}

		/// <summary>
		/// Gets the collection of watermarks added to every page of the document.
		/// </summary>
		public WatermarkCollection Watermarks { get; private set; }

		/// <summary>
		/// Gets the root collection of the page labels items.
		/// </summary>
		/// <value cref="Bytescout.PDF.PageLabelsCollection"></value>
		public PageLabelsCollection PageLabels
		{
			get { return _catalog.PageLabels; }
		}

		/// <summary>
		/// Gets or sets the page layout to be used when the document is opened by a PDF viewer application.
		/// </summary>
		/// <value cref="Bytescout.PDF.PageLayout"></value>
		public PageLayout PageLayout
		{
			get { return _catalog.PageLayout; }
			set { _catalog.PageLayout = value; }
		}

		/// <summary>
		/// Gets or sets how the document should be displayed when opened by a PDF viewer application.
		/// </summary>
		/// <value cref="Bytescout.PDF.PageMode"></value>
		public PageMode PageMode
		{
			get { return _catalog.PageMode; }
			set { _catalog.PageMode = value; }
		}

		/// <summary>
		/// Gets or sets the action to be performed when the document is opened.
		/// </summary>
		/// <value cref="Bytescout.PDF.Action"></value>
		public Action OnOpenDocument
		{
			get { return _catalog.OnOpenDocument; }
			set { _catalog.OnOpenDocument = value; }
		}

		/// <summary>
		/// Gets or sets the action to be performed after printing a document.
		/// </summary>
		/// <value cref="Bytescout.PDF.JavaScriptAction"></value>
		public JavaScriptAction OnAfterPrinting
		{
			get { return _catalog.OnAfterPrinting; }
			set { _catalog.OnAfterPrinting = value; }
		}

		/// <summary>
		/// Gets or sets the action to be performed after saving a document.
		/// </summary>
		/// <value cref="Bytescout.PDF.JavaScriptAction"></value>
		public JavaScriptAction OnAfterSaving
		{
			get { return _catalog.OnAfterSaving; }
			set { _catalog.OnAfterSaving = value; }
		}

		/// <summary>
		/// Gets or sets the action to be performed before closing a document.
		/// </summary>
		/// <value cref="Bytescout.PDF.JavaScriptAction"></value>
		public JavaScriptAction OnBeforeClosing
		{
			get { return _catalog.OnBeforeClosing; }
			set { _catalog.OnBeforeClosing = value; }
		}

		/// <summary>
		/// Gets or sets the action to be performed before printing a document.
		/// </summary>
		/// <value cref="Bytescout.PDF.JavaScriptAction"></value>
		public JavaScriptAction OnBeforePrinting
		{
			get { return _catalog.OnBeforePrinting; }
			set { _catalog.OnBeforePrinting = value; }
		}

		/// <summary>
		/// Gets or set the action to be performed before saving a document.
		/// </summary>
		/// <value cref="Bytescout.PDF.JavaScriptAction"></value>
		public JavaScriptAction OnBeforeSaving
		{
			get { return _catalog.OnBeforeSaving; }
			set { _catalog.OnBeforeSaving = value; }
		}

		/// <summary>
		/// Gets the document’s optional content properties.
		/// </summary>
		/// <value cref="PDF.OptionalContents"></value>
		public OptionalContents OptionalContents
		{
			get { return _catalog.OptionalContents; }
		}

#if !NETCOREAPP2_0
        /// <summary>
        /// Set of helping methods for use from COM/ActiveX. 
        /// </summary>
        public ComHelpers ComHelpers { get; private set; }
#endif

		internal XRef Xref { get; set; }

		/// <summary>
		/// Creates a new PDF document.
		/// </summary>
		public Document()
		{
#if !NETCOREAPP2_0
			ComHelpers = new ComHelpers(this);
#endif
			Watermarks = new WatermarkCollection();

			createSecurity(null);
			createInfo(null);
			createCatalog(null);

			DocumentInfo.SetCreatorAndProducer(getCreator());
			DocumentInfo.CreationDate = DateTime.Now;
		}

		/// <summary>
		/// Creates a new PDF document and loads data from specified existing PDF file.
		/// </summary>
		/// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the existing PDF file.</param>
		/// <param name="password" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The user or owner password that unlocks the existing PDF file data.</param>
		public Document(string fileName, string password)
		{
#if !NETCOREAPP2_0
			ComHelpers = new ComHelpers(this);
#endif
			Watermarks = new WatermarkCollection();

			try
			{
				_fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				DocumentParser parser = new DocumentParser(_fileStream);
				open(parser, password);
			}
			catch (PDFException)
			{
				if (_fileStream != null)
				{
					_fileStream.Dispose();
					_fileStream = null;
				}
				throw;
			}
		}

		/// <summary>
		/// Creates a new PDF document initialized with the data from the specified existing PDF file.
		/// </summary>
		/// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the existing PDF file.</param>
		public Document(string fileName)
			: this(fileName, "")
		{
		}

		/// <summary>
		/// Creates a new PDF document initialized with the data from a specified stream with PDF data.
		/// </summary>
		/// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The stream to read PDF data from.</param>
		/// <param name="password" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The user or owner password that unlocks the PDF data read from the stream.</param>
		public Document(Stream stream, string password)
		{
#if !NETCOREAPP2_0
			ComHelpers = new ComHelpers(this);
#endif
			Watermarks = new WatermarkCollection();

			DocumentParser parser = new DocumentParser(stream);
			open(parser, password);
		}

		/// <summary>
		/// Creates a new PDF document initialized with the data from a specified stream with PDF data.
		/// </summary>
		/// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The stream to read PDF data from.</param>
		public Document(Stream stream)
			: this(stream, "")
		{
		}

		/// <summary>
		/// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
		/// </summary>
		~Document()
		{
			Dispose(false);
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				GC.SuppressFinalize(this);
				//Because the object was explicitly disposed, there will be no need to 
				//run the finalizer.  Suppressing it reduces pressure on the GC
				//The managed reference to an IDisposable is disposed only if the 

				if (_fileStream != null)
				{
					_fileStream.Dispose();
					_fileStream = null;
				}
			}

//			if (_SomeNativeHandle != IntPtr.Zero)
//			{
//				NativeMethods.CloseHandle(_SomeNativeHandle);
//				_SomeNativeHandle = IntPtr.Zero;
//			}
		}

		/// <summary>
		/// Loads document from specified file.
		/// </summary>
		/// <param name="fileName">Name of file to load.</param>
		public void Load(string fileName)
		{
			Load(fileName, "");
		}

		/// <summary>
		/// Loads document from specified file.
		/// </summary>
		/// <param name="fileName">Name of file to load.</param>
		/// <param name="password">The user or owner password that unlocks the existing PDF file data.</param>
		public void Load(string fileName, string password)
		{
			try
			{
				_fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				DocumentParser parser = new DocumentParser(_fileStream);
				open(parser, password);
			}
			catch (PDFException)
			{
				if (_fileStream != null)
				{
					_fileStream.Dispose();
					_fileStream = null;
				}
				throw;
			}
		}

		/// <summary>
		/// Loads document from specified stream.
		/// </summary>
		/// <param name="stream">The stream to read PDF data from.</param>
		[ComVisible(false)]
		public void Load(Stream stream)
		{
			Load(stream, "");
		}
		
		/// <summary>
		/// Loads document from specified stream.
		/// </summary>
		/// <param name="stream">The stream to read PDF data from.</param>
		/// <param name="password">The user or owner password that unlocks the existing PDF file data.</param>
		[ComVisible(false)]
		public void Load(Stream stream, string password)
		{
			if (_fileStream != null)
			{
				_fileStream.Dispose();
				_fileStream = null;
			}

			DocumentParser parser = new DocumentParser(stream);
			open(parser, password);
		}

		/// <summary>
		/// Saves the document to the specified file.
		/// </summary>
		/// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the file to which to save the document.</param>
		public void Save(string fileName)
		{
			AddWarnigs(false);
			ApplyWatermarks();
			DocumentWriter writer = createWriter();

            RegInfo.AddProcessingDelayForWorkstationLicense(120000);

            writer.Write(fileName);
		}

#if DEBUG
        /// <summary>
        /// Saves the document to the specified file.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the file to which to save the document.</param>
        /// <param name="forced">if set to <c>true</c> [forced].</param>
        public void Save(string fileName, bool forced)
        {
            AddWarnigs(false);
	        ApplyWatermarks();
            DocumentWriter writer = createWriter();
            //writer.Forced = forced;

            RegInfo.AddProcessingDelayForWorkstationLicense(120000);

            writer.Write(fileName);
        }
#endif

        /// <summary>
		/// Saves the document to the specified stream.
		/// </summary>
		/// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The stream to save the document to.</param>
		[ComVisible(false)]
		public void Save(Stream stream)
		{
			AddWarnigs(true);
			ApplyWatermarks();
			DocumentWriter writer = createWriter();

            RegInfo.AddProcessingDelayForWorkstationLicense(120000);

            writer.Write(stream);
			stream.Flush();
		}

		/// <summary>
		/// Saves range of pages to the specified file.
		/// </summary>
		/// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the file to which to save the document.</param>
		/// <param name="startIndex" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based start index of pages.</param>
		/// <param name="count" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The number of pages.</param>
		public void SaveRange(string fileName, int startIndex, int count)
		{
			using (Document doc = getRange(startIndex, count))
				doc.Save(fileName);
		}

		/// <summary>
		/// Saves the specified range of pages to the specified stream.
		/// </summary>
		/// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The stream to save the document to.</param>
		/// <param name="startIndex" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based start index of pages.</param>
		/// <param name="count" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The number of pages.</param>
		[ComVisible(false)]
		public void SaveRange(Stream stream, int startIndex, int count)
		{
			using (Document doc = getRange(startIndex, count))
				doc.Save(stream);
		}

        /// <summary>
        /// Gets the bytes of the document.
        /// </summary>
        /// <returns>The bytes of the document</returns>
        public byte[] GetDocumentBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Save(ms);
                return ms.ToArray();
            }
        }

		/// <summary>
		/// Appends the contents of specified PDF document to the current document.
		/// </summary>
		/// <param name="document">The document to append.</param>
		public void Append(Document document)
		{
			if (document == null)
				throw new ArgumentNullException();

			int count = document.Pages.Count;
			for (int i = 0; i < count; ++i)
				this.Pages.Add(document.Pages[i]);
		}

		/// <summary>
		/// Saves the document as text to the specified file.
		/// </summary>
		/// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the file to which to save.</param>
		public void SaveText(string fileName)
		{
			using (StreamWriter streamWriter = new StreamWriter(fileName))
				saveAsText(streamWriter);
		}

		/// <summary>
		/// Saves the document as text to the specified stream.
		/// </summary>
		/// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The stream to save to.</param>
		public void SaveText(Stream stream)
		{
			StreamWriter streamWriter = new StreamWriter(stream);
			saveAsText(streamWriter);
			streamWriter.Flush();
		}

		private void saveAsText(StreamWriter streamWriter)
		{
			int count = Pages.Count;
			for (int i = 0; i < count; ++i)
			{
				streamWriter.WriteLine("--------------------- Page {0} ---------------------", i + 1);
				streamWriter.WriteLine();
				streamWriter.WriteLine(Pages[i].GetText());
				streamWriter.WriteLine();
			}
		}

        /// <summary>
        /// Flatten by setting the read-only flag
        /// </summary>
        private void FlattenDocument2()
        {
            checkDestinations();

            if (_catalog.AcroForm != null)
            {
                PDFDictionary dict = _catalog.AcroForm.GetDictionary();
                PDFArray fields = dict["Fields"] as PDFArray;
                if (fields != null)
                {
                    for (int i = 0; i < fields.Count; i++)
                    {
                        PDFDictionary f_dict = fields[i] as PDFDictionary;
                        if (f_dict != null)
                        {
                            PDFName annot = f_dict["Type"] as PDFName;
                            if (annot != null && annot.GetValue() == "Annot")
                            {
                                PDFName subtype = f_dict["Subtype"] as PDFName;
                                if (subtype != null && subtype.GetValue() == "Widget")
                                {
                                    PDFNumber Ff = f_dict["Ff"] as PDFNumber;
                                    int flag = 0;
                                    if (Ff != null)
                                    {
                                        f_dict.RemoveItem("Ff");
                                        flag = (int)Ff.GetValue();
                                    }
                                    f_dict.AddItem("Ff", new PDFNumber(flag | 1));
                                }
                            }
                        }
                    }
                }
            }
            //for (int i = 0; i < _catalog.Pages.Count; i++)
            //{
            //    Page page = _catalog.Pages[i];
            //    if (page.Annotations != null)
            //    {
            //        for (int j = page.Annotations.Count - 1; j >= 0; j--)
            //        {
            //            if (page.Annotations[j] is Field)
            //            {
            //                Field editBox = page.Annotations[j] as Field;
            //                PDFNumber Ff = editBox.Dictionary["Ff"] as PDFNumber;
            //                int flag = 0;
            //                if (Ff != null)
            //                {
            //                    editBox.Dictionary.RemoveItem("Ff");
            //                    flag = (int)Ff.GetValue();
            //                }
            //                editBox.Dictionary.AddItem("Ff", new PDFNumber(flag | 1));
            //            }
            //        }
            //    }
            //}
        }

        private void EditBoxToText(EditBox editBox, Page page)
        {
            string text = editBox.Text;
            Font font = editBox.Font;
            System.Drawing.RectangleF rectf = new System.Drawing.RectangleF(editBox.Left, editBox.Top,
                                                                editBox.Width, editBox.Height);

            //page.Canvas.DrawRectangle(new SolidBrush(editBox.BackgroundColor), rectf);
            StringFormat stringFormat = new StringFormat();
            switch (editBox.TextAlign)
            {
                case TextAlign.Left:
                    stringFormat.HorizontalAlign = HorizontalAlign.Left;
                    break;
                case TextAlign.Center:
                    stringFormat.HorizontalAlign = HorizontalAlign.Center;
                    break;
                case TextAlign.Right:
                    stringFormat.HorizontalAlign = HorizontalAlign.Right;
                    break;
            }
            stringFormat.VerticalAlign = VerticalAlign.Bottom;
            
            PDFDictionary ap_dict = editBox.Dictionary["AP"] as PDFDictionary;
            if (ap_dict != null)
            {
                PDFDictionaryStream n_dict = ap_dict["N"] as PDFDictionaryStream;
                if (n_dict != null)
                {
                    PDFDictionary r_dict = n_dict.Dictionary["Resources"] as PDFDictionary;
                    Resources resources;
                    if (r_dict == null)
                    {
                        resources = new Resources();
                        n_dict.Dictionary.AddItem("Resources", resources.Dictionary);
                    }
                    else
                    {
                        resources = new Resources(r_dict);
                    }

                    System.Collections.Generic.List<CoordinateText> listStrings = new System.Collections.Generic.List<CoordinateText>();
                    TextExtractor.fillListStrings(listStrings, new float[] { 1, 0, 0, 1, 0, 0 }, ap_dict["N"], resources);
                    text = TextExtractor.convertText(listStrings);

                    stringFormat.HorizontalAlign = HorizontalAlign.Left;
                    for (int l = 0; l < listStrings.Count; l++)
                    {
	                    Font newFont = null;
	                    try
	                    {
		                    newFont = new Font(listStrings[l].FontBase.Name, listStrings[0].FontSize);
	                    }
	                    catch
	                    {
							// try standard font instead 
		                    foreach (StandardFonts stdFont in Enum.GetValues(typeof(StandardFonts)))
		                    {
			                    if (string.Compare(stdFont.ToString(), listStrings[l].FontBase.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
			                    {
				                    newFont = new Font(stdFont, listStrings[0].FontSize);
				                    break;
			                    }
		                    }

							if (newFont == null)
								newFont = new Font(StandardFonts.Helvetica, listStrings[0].FontSize);
						}
						System.Drawing.RectangleF l_rectf = new System.Drawing.RectangleF(rectf.X + listStrings[l].Coordinate.X, rectf.Y,
							rectf.Width, rectf.Height - listStrings[l].Coordinate.Y);
                        page.Canvas.DrawString(listStrings[l].Text.ToString(), newFont, new SolidBrush(), l_rectf, stringFormat);
                    }
                }
            }
            else
                page.Canvas.DrawString(text, font, new SolidBrush(), rectf, stringFormat);
        }

        private void CheckBoxToText(CheckBox checkBox, Page page)
        {
            PDFDictionary parent_dict = checkBox.Dictionary["Parent"] as PDFDictionary;

            if (checkBox.Checked)
            {
                PDFName check = checkBox.Dictionary["V"] as PDFName;
                PDFDictionary ap_dict = checkBox.Dictionary["AP"] as PDFDictionary;
                if (ap_dict != null)
                {
                    PDFDictionary n_dict = ap_dict["N"] as PDFDictionary;
                    if (n_dict != null)
                    {
                        PDFDictionaryStream yes_dict = n_dict[check.GetValue()] as PDFDictionaryStream;
                        if (yes_dict != null)
                        {
                            PDFDictionary r_dict = yes_dict.Dictionary["Resources"] as PDFDictionary;
                            Resources resources;
                            if (r_dict == null)
                            {
                                resources = new Resources();
                                yes_dict.Dictionary.AddItem("Resources", resources.Dictionary);
                            }
                            else
                            {
                                resources = new Resources(r_dict);
                            }

                            System.Collections.Generic.List<CoordinateText> listStrings = new System.Collections.Generic.List<CoordinateText>();
                            TextExtractor.fillListStrings(listStrings, new float[] { 1, 0, 0, 1, 0, 0 }, yes_dict, resources);
                            if (listStrings.Count > 0)
                            {
                                string text = TextExtractor.convertText(listStrings);
                                System.Drawing.RectangleF rectf = new System.Drawing.RectangleF(checkBox.Left + listStrings[0].Coordinate.X,
                                                                        checkBox.Top + listStrings[0].Coordinate.Y, checkBox.Width, checkBox.Height);

                                Font font = newFont(listStrings[0].FontBase, listStrings[0].FontSize);
                                StringFormat stringFormat = new StringFormat();
                                page.Canvas.DrawString(text, font, new SolidBrush(), rectf, stringFormat);
                            }
                        }
                    }
                }
            }
            else if (parent_dict != null)
            {
                PDFName check = parent_dict["V"] as PDFName;
                PDFDictionary ap_dict = checkBox.Dictionary["AP"] as PDFDictionary;
                if (check != null && ap_dict != null)
                {
                    PDFDictionary n_dict = ap_dict["N"] as PDFDictionary;
                    if (n_dict != null)
                    {
                        PDFDictionaryStream yes_dict = n_dict[check.GetValue()] as PDFDictionaryStream;
                        if (yes_dict != null)
                        {
                            PDFDictionary r_dict = yes_dict.Dictionary["Resources"] as PDFDictionary;
                            Resources resources;
                            if (r_dict == null)
                            {
                                resources = new Resources();
                                yes_dict.Dictionary.AddItem("Resources", resources.Dictionary);
                            }
                            else
                            {
                                resources = new Resources(r_dict);
                            }

                            System.Collections.Generic.List<CoordinateText> listStrings = new System.Collections.Generic.List<CoordinateText>();
                            TextExtractor.fillListStrings(listStrings, new float[] { 1, 0, 0, 1, 0, 0 }, yes_dict, resources);
                            if (listStrings.Count > 0)
                            {
                                string text = TextExtractor.convertText(listStrings);
                                System.Drawing.RectangleF rectf = new System.Drawing.RectangleF(checkBox.Left + listStrings[0].Coordinate.X,
                                                                        checkBox.Top + listStrings[0].Coordinate.Y, checkBox.Width, checkBox.Height);
                                
                                Font font = newFont(listStrings[0].FontBase, listStrings[0].FontSize);
                                StringFormat stringFormat = new StringFormat();
                                page.Canvas.DrawString(text, font, new SolidBrush(), rectf, stringFormat);
                            }
                        }
                    }
                }
            }
        }

        private void PushButtonToText(PushButton pushButton, Page page)
        {
            string text = pushButton.Caption;
            Font font = pushButton.Font;
            System.Drawing.RectangleF rectf = new System.Drawing.RectangleF(pushButton.Left, pushButton.Top,
                                                                pushButton.Width, pushButton.Height);

            StringFormat stringFormat = new StringFormat();
            stringFormat.HorizontalAlign = HorizontalAlign.Center;
            stringFormat.VerticalAlign = VerticalAlign.Bottom;

            PDFDictionary ap_dict = pushButton.Dictionary["AP"] as PDFDictionary;
            if (ap_dict != null)
            {
                PDFDictionaryStream n_dict = ap_dict["N"] as PDFDictionaryStream;
                if (n_dict != null)
                {
                    PDFDictionary r_dict = n_dict.Dictionary["Resources"] as PDFDictionary;
                    Resources resources;
                    if (r_dict == null)
                    {
                        resources = new Resources();
                        n_dict.Dictionary.AddItem("Resources", resources.Dictionary);
                    }
                    else
                    {
                        resources = new Resources(r_dict);
                    }

                    System.Collections.Generic.List<CoordinateText> listStrings = new System.Collections.Generic.List<CoordinateText>();
                    TextExtractor.fillListStrings(listStrings, new float[] { 1, 0, 0, 1, 0, 0 }, ap_dict["N"], resources);
                    text = TextExtractor.convertText(listStrings);

                    stringFormat.HorizontalAlign = HorizontalAlign.Left;
                    for (int l = 0; l < listStrings.Count; l++)
                    {
                        font = new Font(listStrings[l].FontBase.Name, listStrings[0].FontSize);
                        System.Drawing.RectangleF l_rectf = new System.Drawing.RectangleF(rectf.X + listStrings[l].Coordinate.X, rectf.Y,
                                                                 rectf.Width, rectf.Height - listStrings[l].Coordinate.Y);
                        //System.Drawing.RectangleF l_rectf = new System.Drawing.RectangleF(rectf.X + listStrings[l].Coordinate.X, rectf.Y- listStrings[l].Coordinate.Y,
                        //                                         rectf.Width, rectf.Height);
                        page.Canvas.DrawString(listStrings[l].Text.ToString(), font, new SolidBrush(), l_rectf, stringFormat);
                    }
                }
            }
            else
                page.Canvas.DrawString(text, font, new SolidBrush(), rectf, stringFormat);
        }

        private Font newFont(FontBase fontBase, float size)
        {
            Font font = new Font(fontBase.Name, size, fontBase.RealBold, fontBase.RealItalic, false, false);
            if (font.Name != fontBase.Name)
            {
                foreach (String s in Enum.GetNames(typeof(StandardFonts)))
                {
                    if (s == fontBase.Name)
                    {
                        StandardFonts standardFonts = (StandardFonts)Enum.Parse(typeof(StandardFonts), s);
                        font = new Font(standardFonts, size, false, false);
                        break;
                    }
                }
            }
            return font;
        }

		/// <summary>
		/// Flattens document (replaces all input controls with static text) making PDF forms uneditable.
		/// </summary>
        public void FlattenDocument()
        {
            checkDestinations();

            for (int i = 0; i < _catalog.Pages.Count; i++)
            {
                Page page = _catalog.Pages[i];
                if (page.Annotations != null)
                {
                    for (int j = page.Annotations.Count - 1; j >= 0; j--)
                    {
                        if (page.Annotations[j] is EditBox)
                        {
                            EditBoxToText(page.Annotations[j] as EditBox, page);

                            page.Annotations.Remove(j);
                        }
                        else if (page.Annotations[j] is CheckBox)
                        {
                            CheckBoxToText(page.Annotations[j] as CheckBox, page);

                            page.Annotations.Remove(j);
                        }
                        else if (page.Annotations[j] is PushButton)
                        {
                            PushButtonToText(page.Annotations[j] as PushButton, page);

                            page.Annotations.Remove(j);
                        }
                    }
                }
            }
            if (_catalog.AcroForm != null)
            {
                PDFDictionary dict = _catalog.AcroForm.GetDictionary();
                PDFArray fields = dict["Fields"] as PDFArray;
                if (fields != null)
                {
                    for (int i = fields.Count - 1; i >= 0; i--)
                    {
                        PDFDictionary f_dict = fields[i] as PDFDictionary;
                        if (f_dict != null)
                        {
                            PDFName annot = f_dict["Type"] as PDFName;
                            if (annot != null && annot.GetValue() == "Annot")
                            {
                                PDFName subtype = f_dict["Subtype"] as PDFName;
                                if (subtype != null && subtype.GetValue() == "Widget")
                                {
                                    PDFName ft = f_dict["FT"] as PDFName;
                                    if (ft != null && (ft.GetValue() == "Tx" || ft.GetValue() == "Btn"))  // EditBox || Button
                                    {
                                        ((PDFArray)dict["Fields"]).RemoveItem(i);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // задает permissions с помощью цифровой подписи
        //public void SetPermissions(string certificatePath, string password)
        //{
        //    _certificate = new Certificate(certificatePath, password);
        //    //Certificate p_certificate = new Certificate(certificatePath, password);
        //    _signature = "permissions"; // нужно уникальное значение
        //    Permissions permissions = new Permissions(_certificate);
        //    //Permissions permissions = new Permissions(p_certificate);
        //    //Sig sig = new Sig(_certificate);
        //    //Signature signature = new Signature(_signature, permissions.GetDictionary());

        //    PDFDictionary perms = new PDFDictionary();
        //    perms.AddItem("UR3", permissions.GetDictionary());
        //    _catalog.GetDictionary().AddItem("Perms", perms);
        //}

		/// <summary>
		/// Signs document with digital signature.
		/// </summary>
		/// <param name="certificatePath"></param>
		/// <param name="password"></param>
        public void Sign(string certificatePath, string password)
        {
            _certificate = new Certificate(certificatePath, password);
            _signature = "signature"; // нужно уникальное значение
            Sig sig = new Sig(_certificate);
            Signature signature = new Signature(_signature, sig.GetDictionary());
            Pages[0].Annotations.Add(signature);
        }

		/// <summary>
		/// Signs document with digital signature.
		/// </summary>
		/// <param name="certificatePath">Digital certificate file name.</param>
		/// <param name="password">Certificate password.</param>
		/// <param name="rect">Rectangular area on the first document page that will display signature information.</param>
		/// <param name="reason">Signing reason.</param>
		/// <param name="contact">Contact information.</param>
		/// <param name="location">Signing location.</param>
		[ComVisible(false)]
        public void Sign(string certificatePath, string password, System.Drawing.RectangleF rect, string reason, string contact, string location)
        {
            _certificate = new Certificate(certificatePath, password);
            _signature = "signature"; // нужно уникальное значение
            Sig sig = new Sig(_certificate, reason, contact, location);
            Signature signature = new Signature(rect.Left, rect.Top, rect.Width, rect.Height, _signature, sig.GetDictionary(), reason, contact, location);
            Pages[0].Annotations.Add(signature);
        }

		/// <summary>
		/// Signs document with digital signature.
		/// </summary>
		/// <param name="certificatePath">Digital certificate file name.</param>
		/// <param name="password">Certificate password.</param>
		/// <param name="left">Left coordinate of rectangular area on the first document page that will display signature information.</param>
		/// <param name="top">Top coordinate of rectangular area on the first document page that will display signature information.</param>
		/// <param name="width">Width of rectangular area on the first document page that will display signature information.</param>
		/// <param name="height">Height of rectangular area on the first document page that will display signature information.</param>
		/// <param name="reason">Signing reason.</param>
		/// <param name="contact">Contact information.</param>
		/// <param name="location">Signing location.</param>
		public void Sign(string certificatePath, string password, float left, float top, float width, float height, string reason, string contact, string location)
		{
			_certificate = new Certificate(certificatePath, password);
			_signature = "signature"; // нужно уникальное значение
			Sig sig = new Sig(_certificate, reason, contact, location);
			Signature signature = new Signature(left, top, width, height, _signature, sig.GetDictionary(), reason, contact, location);
			Pages[0].Annotations.Add(signature);
		}

        private int findSigObj(XRef xref)
        {
            for (int i = 1; i < xref.Entries.Count; ++i)
            {
                if (xref.Entries[i].Object is PDFDictionary)
                {
                    PDFDictionary dict = (PDFDictionary)xref.Entries[i].Object;
                    PDFName ft = (PDFName)dict["FT"];
                    //PDFString name = (PDFString)dict["T"];
                    //if (ft != null && ft.GetValue() == "Sig" && name != null && name.GetValue() == _signature)
                    if (ft != null && ft.GetValue() == "Sig")
                    {
                        PDFString name = (PDFString)dict["T"];
                        if (name != null && name.GetValue() == _signature)
                        {
                            PDFDictionary v = (PDFDictionary)dict["V"];
                            if (v != null)
                                return v.ObjNo;
                        }
                    }
                }
            }

            // Возвращает объект подпись для permissions
            //PDFDictionary catalogDict = _catalog.GetDictionary();
            //PDFDictionary perms = (PDFDictionary)_catalog.GetDictionary()["Perms"];
            //if (perms != null)
            //{
            //    PDFDictionary trans = (PDFDictionary)perms["UR3"];
            //    if (trans != null /*&& trans["Type"] == "Sig"*/)
            //    {
            //        return trans.ObjNo;
            //    }
            //}
            return -1;
        }

        private DocumentWriter createWriter()
		{
			FontsManager.Release();
			setCorrectPageLinks();
			_catalog.CheckPageLabels(Pages.Count);

			XRef xref = new XRef();
			xref.Entries.Add(new Entry(0, 65535));
			_docInfo.GetDictionary().Collect(xref);
			_catalog.GetDictionary().Collect(xref);

			xref.SetInfo(_docInfo.GetDictionary());
			xref.SetCatalog(_catalog.GetDictionary());
			xref.AddEncryption(_security);

			if (_fileStream != null)
			{
				_fileStream.Dispose();
				_fileStream = null;
			}

			DocumentWriter writer = new DocumentWriter(xref, _compression);
            writer.signature = findSigObj(xref);
            writer._certificate = _certificate;
            return writer;
		}

        private void open(DocumentParser parser, string password)
        {
            parser.Parse(password);
            XRef xref = parser.GetXref();
            createInfo(xref.GetInfo());
            createCatalog(xref.GetCatalog());
            createSecurity(xref.Encryptor);
            Xref = xref;
        }

		private void createInfo(PDFDictionary dict)
		{
			if (dict != null)
				_docInfo = new DocumentInformation(dict);
			else
				_docInfo = new DocumentInformation();
		}

		private void createCatalog(PDFDictionary dict)
		{
			if (dict != null)
				_catalog = new Catalog(dict);
			else
				_catalog = new Catalog();
		}

		private string getCreator()
		{
			string result = "";
			object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (AssemblyProductAttribute), false);
			if (attributes.Length > 0)
				result += ((AssemblyProductAttribute) attributes[0]).Product;

			result += ' ';
			string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			version = version.Substring(0, version.LastIndexOf('.'));

			return result + version;
		}

		private void createSecurity(Encryptor encryptor)
		{
			_security = new Security();
			if (encryptor != null)
			{
				_security.UserPassword = encryptor.UserPassword;
				_security.OwnerPassword = encryptor.OwnerPassword;
				_security.EncryptionAlgorithm = encryptor.GetEncryptionLevel();
				_security.SetPermissions(encryptor.Permissions);
			}
		}

		private void AddWarnigs(bool generateInMemory)
		{
			string message = "";

			/*if (generateInMemory && !RegInfo.CanSaveToStream ||
			    !generateInMemory && !RegInfo.CanSaveToFile)
			{
				message = "license error";
			}*/

#if !PDFSDK_EMBEDDED_SOURCES
			if (!RegInfo.IsRegistered)
			{
				message = RegInfo.GetDemoWarningString();
			}
#endif

			if (String.IsNullOrEmpty(message))
				return;

            Font fnt = new Font(StandardFonts.Helvetica, 12);
            Brush br = new SolidBrush(new ColorRGB(0, 0, 255));
            float y;
            int count = Pages.Count;
            for (int i = 0; i < count; ++i)
            {
                Page page = Pages[i];
                page.Canvas.BeforeLicense();
                page.Canvas.SaveGraphicsState();
                if (page.RotationAngle == RotationAngle.Rotate0 || page.RotationAngle == RotationAngle.Rotate180)
                    y = page.Height - 2.5f * fnt.GetTextHeight();
                else
                    y = page.Width - 2.5f * fnt.GetTextHeight();

                switch (page.RotationAngle)
                {
                    case RotationAngle.Rotate90:
                        page.Canvas.RotateTransform(-90);
                        page.Canvas.TranslateTransform(-page.Height, 0);
                        break;
                    case RotationAngle.Rotate180:
                        page.Canvas.RotateTransform(180);
                        page.Canvas.TranslateTransform(-page.Width, -page.Height);
                        break;
                    case RotationAngle.Rotate270:
                        page.Canvas.RotateTransform(-270);
                        page.Canvas.TranslateTransform(0, -page.Width);
                        break;
                }

                page.Canvas.DrawString(message, fnt, br, 5, y);
            }
        }

		private void ApplyWatermarks()
		{
			for (int i = 0; i < Pages.Count; ++i)
			{
				Page page = Pages[i];
				ApplyWatermarks(page, Watermarks);
				ApplyWatermarks(page, page.Watermarks);
			}
		}

		private void ApplyWatermarks(Page page, WatermarkCollection watermarks)
		{
			foreach (Watermark watermark in watermarks)
			{
				if (watermark is TextWatermark)
				{
					TextWatermark textWatermark = (TextWatermark) watermark;

					Canvas canvas = page.Canvas;
					
					Font font;
					float angle;
					PointF drawPoint;
				    int rows = 1, columns = 1;
				    float xStep = 0, yStep = 0;

					if (textWatermark.WatermarkLocation == TextWatermarkLocation.Custom)
					{
						font = textWatermark.Font;
						angle = textWatermark.Angle;
						drawPoint = new PointF(textWatermark.Left, textWatermark.Top);
					}
                    else if (textWatermark.WatermarkLocation == TextWatermarkLocation.Tiled)
					{
					    font = textWatermark.Font;
                        angle = textWatermark.Angle;
					    CalculateWatermarkTiling(page, textWatermark.Text, textWatermark.Font, textWatermark.Angle, out rows, out columns, out drawPoint, 
                            out xStep, out yStep);
					}
					else
					{
						float fontSize;
						CalculateWatermarkSizeAndPosition(page, textWatermark.Text, textWatermark.Font, textWatermark.WatermarkLocation,
							out fontSize, out drawPoint, out angle);
						font = new Font(textWatermark.Font.Name, fontSize, textWatermark.Font.Bold, textWatermark.Font.Italic,
							textWatermark.Font.Underline, textWatermark.Font.Strikeout);
					}

				    for (int row = 0; row < rows; row++)
				    {
				        for (int column = 0; column < columns; column++)
				        {
				            canvas.SaveGraphicsState();

				            if (Math.Abs(drawPoint.X) > float.Epsilon || Math.Abs(drawPoint.Y) > float.Epsilon)
				                canvas.TranslateTransform(drawPoint.X + xStep * column, drawPoint.Y + yStep * row);
				            if (Math.Abs(angle) > float.Epsilon)
				                canvas.RotateTransform(angle);

				            StringFormat sf = new StringFormat();
				            sf.HorizontalAlign = HorizontalAlign.Center;
				            sf.VerticalAlign = VerticalAlign.Center;

				            if (textWatermark.Pen != null)
				                canvas.DrawString(textWatermark.Text, font, textWatermark.Brush, textWatermark.Pen, 0, 0, sf);
				            else
				                canvas.DrawString(textWatermark.Text, font, textWatermark.Brush, 0, 0, sf);

				            canvas.RestoreGraphicsState();
                        }
				    }
				}
			}
		}

		private void CalculateWatermarkSizeAndPosition(Page page, string text, Font font, TextWatermarkLocation location, 
			out float fontSize, out PointF drawPoint, out float angle)
		{
			const int margin = 10;

			float width = page.Width - margin * 2;
			float height = page.Height - margin * 2;

			switch (location)
			{
				case TextWatermarkLocation.Custom:
				case TextWatermarkLocation.Tiled:
					throw new ArgumentException();
				case TextWatermarkLocation.Top:
				case TextWatermarkLocation.Bottom:
				case TextWatermarkLocation.Center:
					angle = 0;
					break;
				case TextWatermarkLocation.VerticalFromTopToBottom:
					angle = 90;
					break;
				case TextWatermarkLocation.VerticalFromBottomToTop:
					angle = -90;
					break;
				case TextWatermarkLocation.DiagonalFromTopLeftToBottomRight:
					angle = (float) (Math.Atan2(height, width) / (Math.PI / 180));
					break;
				case TextWatermarkLocation.DiagonalFromBottomLeftToTopRight:
					angle = -(float) (Math.Atan2(height, width) / (Math.PI / 180));
					break;
				default:
					throw new ArgumentOutOfRangeException("location", location, null);
			}
			
			float deltaWidth = width * 0.03f; // correcting width at 3% to fit the rotated text

			RectangleF maxRect = new RectangleF(margin + deltaWidth / 2, margin, width - deltaWidth, height);
			
			Font tempFont = new Font(font.Name, 9, font.Bold, font.Italic, font.Underline, font.Strikeout);
			SizeF textSize = page.Canvas.MeasureString(text, tempFont);

			if (textSize.Width > textSize.Height)
				fontSize = 9 * maxRect.Width / textSize.Width;
			else
				fontSize = 9 * maxRect.Height / textSize.Height;

			if (fontSize < 1)
				fontSize = 1;

			fontSize = Math.Min(fontSize, Math.Min(maxRect.Height, maxRect.Width));

			Font newFont = new Font(font.Name, fontSize, font.Bold, font.Italic, font.Underline, font.Strikeout);
			textSize = page.Canvas.MeasureString(text, newFont);

			RectangleF textRect = new RectangleF(
				maxRect.Left + maxRect.Width / 2 - textSize.Width / 2, 
				maxRect.Top + maxRect.Height / 2 - textSize.Height / 2, 
				textSize.Width, 
				textSize.Height);

			if (location == TextWatermarkLocation.Top)
			{
				textRect.X = maxRect.Left;
				textRect.Y = maxRect.Top;
			}
			else if (location == TextWatermarkLocation.Bottom)
			{
				textRect.X = maxRect.Left;
				textRect.Y = maxRect.Bottom - textRect.Height;
			}

			PointF[] points =
			{
				new PointF(textRect.Left, textRect.Top),
				new PointF(textRect.Right, textRect.Top),
				new PointF(textRect.Right, textRect.Bottom),
				new PointF(textRect.Left, textRect.Bottom)
			};

			if (Math.Abs(angle) > 0)
			{
				Matrix matrix = new Matrix();
				matrix.RotateAt(angle, new PointF(maxRect.X + maxRect.Width / 2, maxRect.Y + maxRect.Height / 2));
				matrix.TransformPoints(points);
			}

			drawPoint = points[0];
		}

        private void CalculateWatermarkTiling(Page page, string text, Font font, float angle, out int rows, out int columns, 
            out PointF firstDrawPoint, out float xStep, out float yStep)
        {
            const int padding = 10;

            //float maxWidth = page.Width - padding * 2;
            //float maxHeight = page.Height - padding * 2;

            SizeF textSize = page.Canvas.MeasureString(text, font);
            RectangleF textRect = new RectangleF(0, 0, textSize.Width, textSize.Height);

            PointF[] points =
            {
                new PointF(textRect.Left, textRect.Top),
                new PointF(textRect.Right, textRect.Top),
                new PointF(textRect.Right, textRect.Bottom),
                new PointF(textRect.Left, textRect.Bottom)
            };

            if (Math.Abs(angle) > 0)
            {
                Matrix matrix = new Matrix();
                matrix.RotateAt(angle, new PointF(textRect.X + textRect.Width / 2, textRect.Y + textRect.Height / 2));
                matrix.TransformPoints(points);
                using (GraphicsPath path = new GraphicsPath(points, new byte[] { (byte) PathPointType.Start, (byte) PathPointType.Line, (byte) PathPointType.Line, (byte) PathPointType.Line }))
                    textRect = path.GetBounds();
            }

            firstDrawPoint = new PointF(padding + points[0].X, padding + points[0].Y);
            firstDrawPoint.Y += textRect.Height / 2 - (Math.Abs(angle) > 0 ? textSize.Height / 2 : 0);

            columns = Math.Max(1, (int) ((page.Width - padding) / (textRect.Width + padding)));
            rows = Math.Max(1, (int) ((page.Height - padding) / (textRect.Height + padding)));

            // align tiles 
            float xDelta = (page.Width - padding) - (textRect.Width + padding) * columns;
            float xOffset = xDelta / (columns + 1);
            xStep = xOffset + textRect.Width + padding;
            float yDelta = (page.Height - padding) - (textRect.Height + padding) * rows;
            float yOffset = yDelta / (rows + 1);
            yStep = yOffset + textRect.Height + padding;

            firstDrawPoint.X += xOffset;
            firstDrawPoint.Y += yOffset;
        }

        private void setCorrectPageLinks()
        {
            checkDestinations();
            checkOutlines();
        }

        private void checkDestinations()
        {
            for (int i = 0; i < Pages.Count; ++i)
            {
                if (Pages[i].GetDictionary()["Annots"] != null)
                {
                    AnnotationCollections annots = Pages[i].Annotations;
                    for (int j = 0; j < annots.Count; ++j)
                        checkAnnotation(annots[j]);
                }
                checkAction(Pages[i].OnClosed);
                checkAction(Pages[i].OnOpened);
            }
        }

        private void checkAnnotation(Annotation annot)
        {
            if (annot is LinkAnnotation)
            {
                correctDestination((annot as LinkAnnotation).Destination);
                checkAction((annot as LinkAnnotation).Action);
            }
            else if (annot is Field)
            {
                Field field = (annot as Field);
                checkAction(field.OnActivated);
                checkAction(field.OnBeforeFormatting);
                checkAction(field.OnChange);
                checkAction(field.OnKeyPressed);
                checkAction(field.OnLoseFocus);
                checkAction(field.OnMouseDown);
                checkAction(field.OnMouseEnter);
                checkAction(field.OnMouseExit);
                checkAction(field.OnMouseUp);
                checkAction(field.OnOtherFieldChanged);
                checkAction(field.OnPageClose);
                checkAction(field.OnPageInvisible);
                checkAction(field.OnPageOpen);
                checkAction(field.OnPageVisible);
                checkAction(field.OnReceiveFocus);
            }
        }

        private void checkAction(Action action)
        {
            if (action == null)
                return;

            if (action is GoToAction)
            {
                Destination dest = (action as GoToAction).Destination;
                correctDestination(dest);
            }

            if (action.GetDictionary()["Next"] != null)
            {
                for (int i = 0; i < action.Next.Count; ++i)
                    checkAction(action.Next[i]);
            }
        }

        private void correctDestination(Destination dest)
        {
            if (dest != null)
            {
                if (dest.Page == null)
                {
                    PDFArray arr = dest.GetArray();
                    arr.RemoveItem(0);
                    arr.Insert(0, new PDFNull());
                }
                else
                {
                    if (dest.Page.GetDictionary()["Parent"] != Pages.GetDictionary())
                    {
                        PDFArray arr = dest.GetArray();
                        arr.RemoveItem(0);
                        Page page = Pages.FindPageByID(dest.Page.PageID);
                        if (page == null)
                            arr.Insert(0, new PDFNull());
                        else
                            arr.Insert(0, page.GetDictionary());
                    }
                    else
                    {
                        if (Pages.GetPage(dest.Page.GetDictionary()) == null)
                        {
                            PDFArray arr = dest.GetArray();
                            arr.RemoveItem(0);
                            arr.Insert(0, new PDFNull());
                        }
                    }
                }
            }
        }

        private void checkOutlines()
        {
            if (_catalog.GetDictionary()["Outlines"] != null)
            {
                for (int i = 0; i < Outlines.Count; ++i)
                    checkOutline(Outlines[i]);
            }
        }

        private void checkOutline(Outline outline)
        {
            correctDestination(outline.Destination);
            checkAction(outline.Action);

            for (int i = 0; i < outline.Kids.Count; ++i)
                checkOutline(outline.Kids[i]);
        }

        private Document getRange(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex >= Pages.Count)
                throw new ArgumentOutOfRangeException("startIndex");
            if (length < 0 || startIndex + length >= Pages.Count)
                throw new ArgumentOutOfRangeException("length");

            Document doc = new Document();
            for (int i = startIndex; i - startIndex < length; ++i)
                doc.Pages.Add(Pages[i]);

            doc.DocumentInfo.GetDictionary().Clear();
            doc.DocumentInfo.GetDictionary().AddRange(DocumentInfo.GetDictionary());

            return doc;
        }
    }
}
