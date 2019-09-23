using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class AnnotationCollections : IEnumerable<Annotation>
#else
	/// <summary>
    /// Represents a collection of annotations.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    [DebuggerDisplay("Count = {Count}")]
    public class AnnotationCollections : IEnumerable<Annotation>
#endif
	{
	    private readonly PDFArray _array;
	    private readonly List<Annotation> _annotations = new List<Annotation>();
	    private Page _page;
	    private readonly IDocumentEssential _owner;

	    /// <summary>
	    /// Gets the element at the specified index.
	    /// </summary>
	    /// <param name="index">The zero-based index of the element to get.</param>
	    /// <returns cref="Annotation">The Bytescout.PDF.Annotation with specified index.</returns>
	    public Annotation this[int index]
	    {
		    get
		    {
			    return _annotations[index];
		    }
	    }

		/// <summary>
		/// Gets named annotation (e.g. input field) by its name.
		/// </summary>
		/// <param name="name">Name.</param>
		public Annotation this[string name]
		{
			get
			{
				foreach (Annotation annotation in _annotations)
				{
					Field field = annotation as Field;
					if (field != null && field.Name == name)
						return field;
				}

				return null;
			}
		}

	    /// <summary>
	    /// Gets the number of the elements in the collection.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Count
	    {
		    get { return _annotations.Count; }
	    }

	    internal AnnotationCollections(IDocumentEssential owner, Page page)
        {
            _array = new PDFArray();
            _owner = owner;
            _page = page;
        }

        internal AnnotationCollections(PDFArray arr, IDocumentEssential owner, Page page)
        {
            _array = arr;
            _owner = owner;
            _page = page;
            parseAnnotations();
        }

	    /// <summary>
        /// Adds the specified annotation to the collection.
        /// </summary>
        /// <param name="annotation">Annotation to be added.</param>
        public void Add(Annotation annotation)
        {
            if (annotation == null)
                throw new ArgumentNullException();

            Annotation copy = annotation.Clone(_owner, _page);
            _annotations.Add(copy);
            _array.AddItem(copy.Dictionary);

            if (annotation is MarkupAnnotation)
            {
                PopupAnnotation popup = (annotation as MarkupAnnotation).Popup;
                if (popup != null)
                {
                    Annotation popupClone = popup.Clone(_owner, _page);
                    (popupClone as PopupAnnotation).SetParent(copy);

                    copy.Dictionary.AddItem("Popup", popupClone.Dictionary);
                    _array.AddItem(popupClone.Dictionary);
                }
            }
            else if (annotation is Field)
            {
                if (_owner != null)
                    _owner.AddField(copy as Field);
            }
        }

        /// <summary>
        /// Removes the annotation with a specified index from the collection.
        /// </summary>
        /// <param name="index" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The zero-based index of the annotation to be removed.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            Annotation annot = _annotations[index];
            _annotations.RemoveAt(index);

            remove(annot.Dictionary);
            if (annot is MarkupAnnotation)
            {
                PopupAnnotation popup = (annot as MarkupAnnotation).Popup;
                if (popup != null)
                    remove(popup.Dictionary);

                PDFDictionary currentDict = annot.Dictionary;
                for (int i = 0; i < _array.Count; )
                {
                    PDFDictionary dict = _array[i] as PDFDictionary;
                    PDFDictionary irt = dict["IRT"] as PDFDictionary;
                    if (irt != null && irt == currentDict)
                    {
                        remove(dict);
                        PDFDictionary popupDict = dict["Popup"] as PDFDictionary;
                        if (popupDict != null)
                            remove(popupDict);

                        currentDict = dict;
                        i = 0;
                    }
                    else
                        ++i;
                }
            }

            if (annot is Field && _owner != null)
                _owner.RemoveField(annot as Field);
        }

        /// <summary>
        /// Clears this collection.
        /// </summary>
        public void Clear()
        {
            int count = Count;
            for (int i = 0; i < count; ++i)
                Remove(0);
        }

        internal PDFArray GetArray()
        {
            return _array;
        }

        internal AnnotationCollections Clone(IDocumentEssential owner, Page page)
        {
            AnnotationCollections annots = new AnnotationCollections(owner, _page);
            annots._page = page;

            for (int i = 0; i < _annotations.Count; ++i)
                annots.Add(_annotations[i]);

            return annots;
        }

        private void remove(PDFDictionary annotDict)
        {
            for (int i = 0; i < _array.Count; ++i)
            {
                if (_array[i] == annotDict)
                {
                    _array.RemoveItem(i);
                    return;
                }
            }
        }

        private void parseAnnotations()
        {
            for (int i = 0; i < _array.Count; ++i)
            {
                PDFDictionary annotDict = _array[i] as PDFDictionary;
                if (annotDict != null)
                {
                    if (annotDict["IRT"] == null)
                    {
                        PDFName subtype = annotDict["Subtype"] as PDFName;
                        if (subtype != null)
                        {
                            Annotation annot = null;
                            switch (subtype.GetValue())
                            {
                                case "Text":
                                    annot = new TextAnnotation(annotDict, _owner);
                                    break;
                                case "Link":
                                    annot = new LinkAnnotation(annotDict, _owner);
                                    break;
                                case "FreeText":
                                    annot = new FreeTextAnnotation(annotDict, _owner);
                                    break;
                                case "Line":
                                    annot = new LineAnnotation(annotDict, _owner);
                                    break;
                                case "Square":
                                    annot = new SquareAnnotation(annotDict, _owner);
                                    break;
                                case "Circle":
                                    annot = new CircleAnnotation(annotDict, _owner);
                                    break;
                                case "Polygon":
                                    annot = new PolygonAnnotation(annotDict, _owner);
                                    break;
                                case "PolyLine":
                                    annot = new PolylineAnnotation(annotDict, _owner);
                                    break;
                                case "Highlight":
                                    annot = new HighlightAnnotation(annotDict, _owner);
                                    break;
                                case "Underline":
                                    annot = new UnderlineAnnotation(annotDict, _owner);
                                    break;
                                case "Squiggly":
                                    annot = new SquigglyAnnotation(annotDict, _owner);
                                    break;
                                case "StrikeOut":
                                    annot = new StrikeOutAnnotation(annotDict, _owner);
                                    break;
                                case "Stamp":
                                    annot = new RubberStampAnnotation(annotDict, _owner);
                                    break;
                                case "Caret":
                                    annot = new CaretAnnotation(annotDict, _owner);
                                    break;
                                case "Ink":
                                    annot = new InkAnnotation(annotDict, _owner);
                                    break;
                                case "FileAttachment":
                                    annot = new FileAttachmentAnnotation(annotDict, _owner);
                                    break;
                                case "Sound":
                                    annot = new SoundAnnotation(annotDict, _owner);
                                    break;
                                case "Movie":
                                    annot = new MovieAnnotation(annotDict, _owner);
                                    break;
                                case "Screen":
                                    annot = new ScreenAnnotation(annotDict, _owner);
                                    break;
                                case "3D":
                                    annot = new ThreeDAnnotation(annotDict, _owner);
                                    break;
                                case "Widget":
                                    PDFName ft = annotDict["FT"] as PDFName;
                                    if (ft == null)
                                    {
                                        PDFDictionary parent = annotDict["Parent"] as PDFDictionary;
                                        if (parent != null)
                                            ft = parent["FT"] as PDFName;
                                    }

                                    if (ft != null)
                                    {
                                        switch(ft.GetValue())
                                        {
                                            case "Tx":
                                                annot = new EditBox(annotDict, _owner);
                                                break;
                                            case "Ch":
                                                uint flag = getFlag(annotDict);
                                                if ((flag >> 17) % 2 != 0)
                                                    annot = new ComboBox(annotDict, _owner);
                                                else
                                                    annot = new ListBox(annotDict, _owner);
                                                break;
                                            case "Btn":
                                                flag = getFlag(annotDict);
                                                if ((flag >> 16) % 2 != 0)
                                                    annot = new PushButton(annotDict, _owner);
                                                else if ((flag >> 15) % 2 != 0)
                                                    annot = new RadioButton(annotDict, _owner);
                                                else
                                                    annot = new CheckBox(annotDict, _owner);
                                                break;
                                        }
                                    }
                                    break;
                            }

                            if (annot != null)
                            {
                                annot.SetPage(_page, false);
                                _annotations.Add(annot);
                            }
                        }
                    }
                }
            }
        }

        private uint getFlag(PDFDictionary dict)
        {
            PDFNumber ff = dict["Ff"] as PDFNumber;
            if (ff == null)
            {
                PDFDictionary parent = dict["Parent"] as PDFDictionary;
                if (parent != null)
                    ff = parent["Ff"] as PDFNumber;
            }

            uint flag = 0;
            if (ff != null)
                flag = (uint)ff.GetValue();

            return flag;
        }

		[ComVisible(false)]
	    public IEnumerator<Annotation> GetEnumerator()
	    {
		    return _annotations.GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }
    }
}
