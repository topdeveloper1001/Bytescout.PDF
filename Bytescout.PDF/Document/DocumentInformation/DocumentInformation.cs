using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class DocumentInformation
#else
	/// <summary>
    /// Represents class for a PDF document's metadata.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class DocumentInformation
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
        /// Gets or sets the document’s title.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Title
        {
            get
            {
                PDFString title = _dictionary["Title"] as PDFString;
                if (title != null)
                    return title.GetValue();
                return "";
            }
            set
            {
                string val = "";
                if (value != null)
                    val = value;
                _dictionary.AddItem("Title", new PDFString(val));
            }
        }

        /// <summary>
        /// Gets or sets the name of the person who created the document.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Author
        {
            get
            {
                PDFString author = _dictionary["Author"] as PDFString;
                if (author != null)
                    return author.GetValue();
                return "";
            }
            set
            {
                string val = "";
                if (value != null)
                    val = value;
                _dictionary.AddItem("Author", new PDFString(val));
            }
        }

        /// <summary>
        /// Gets or sets the subject of the document.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Subject
        {
            get
            {
                PDFString subject = _dictionary["Subject"] as PDFString;
                if (subject != null)
                    return subject.GetValue();
                return "";
            }
            set
            {
                string val = "";
                if (value != null)
                    val = value;
                _dictionary.AddItem("Subject", new PDFString(val));
            }
        }

        /// <summary>
        /// Gets or sets the keywords associated with the document.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Keywords
        {
            get
            {
                PDFString keywords = _dictionary["Keywords"] as PDFString;
                if (keywords != null)
                    return keywords.GetValue();
                return "";
            }
            set
            {
                string val = "";
                if (value != null)
                    val = value;
                _dictionary.AddItem("Keywords", new PDFString(val));
            }
        }

        /// <summary>
        /// Gets or sets additional information about the creator of the current PDF document.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Creator
        {
            get
            {
                PDFString creator = _dictionary["Creator"] as PDFString;
                if (creator != null)
                    return creator.GetValue();
                return "";
            }
            set
            {
#if !PDFSDK_EMBEDDED_SOURCES
				//if (RegInfo.CanChangeCreator)
#endif
				{
                    string val = "";
                    if (value != null)
                        val = value;
                    _dictionary.AddItem("Creator", new PDFString(val));
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the application that produced the PDF document.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Producer
        {
            get
            {
                PDFString producer = _dictionary["Producer"] as PDFString;
                if (producer != null)
                    return producer.GetValue();
                return "";
            }
            set
            {
#if !PDFSDK_EMBEDDED_SOURCES
                //if (RegInfo.CanChangeCreator)
#endif
				{
                    string val = "";
                    if (value != null)
                        val = value;
                    _dictionary.AddItem("Producer", new PDFString(val));
                }
            }
        }

        /// <summary>
        /// Gets or sets the date and time the document was created.
        /// </summary>
        /// <value cref="DateTime" href="http://msdn.microsoft.com/en-us/library/system.datetime.aspx"></value>
        public DateTime CreationDate
        {
            get
            {
                PDFString creationDate = _dictionary["CreationDate"] as PDFString;
                if (creationDate != null)
                    return StringUtility.ParseDateTime(creationDate.GetValue());
                return DateTime.MinValue;
            }
            set
            {
                string date = StringUtility.CreatePDFDateTime(value);
                _dictionary.AddItem("CreationDate", new PDFString(System.Text.Encoding.ASCII.GetBytes(date), false));
            }
        }

        /// <summary>
        /// Gets or sets the date and time the document was last modified.
        /// </summary>
        /// <value cref="DateTime" href="http://msdn.microsoft.com/en-us/library/system.datetime.aspx"></value>
        public DateTime ModificationDate
        {
            get
            {
                PDFString modDate = _dictionary["ModDate"] as PDFString;
                if (modDate != null)
                    return StringUtility.ParseDateTime(modDate.GetValue());
                return DateTime.MinValue;
            }
            set
            {
                string date = StringUtility.CreatePDFDateTime(value);
                _dictionary.AddItem("ModDate", new PDFString(System.Text.Encoding.ASCII.GetBytes(date), false));
            }
        }

	    internal DocumentInformation()
	    {
		    _dictionary = new PDFDictionary();
	    }

	    internal DocumentInformation(PDFDictionary dict)
	    {
		    _dictionary = dict;
	    }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        internal void SetCreatorAndProducer(string val)
        {
            _dictionary.AddItem("Producer", new PDFString(val));
            _dictionary.AddItem("Creator", new PDFString(val));
        }
    }
}
