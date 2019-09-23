using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Security
#else
	/// <summary>
    /// Represents the options to control the PDF document security options.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class Security
#endif
	{
	    private string _userPassword = "";
	    private string _ownerPassword = "";
	    private EncryptionAlgorithm _encryptionAlgorithm = EncryptionAlgorithm.None;
	    private bool _allowPrintDocument = true;
	    private PrintQuality _printQuality = PrintQuality.HightResolution;
	    private bool _allowModifyDocument = true;
	    private bool _allowFillForms = true;
	    private bool _allowModifyAnnotations = true;
	    private bool _allowContentExtraction = true;
	    private bool _allowAccessibilitySupport = true;
	    private bool _allowAssemlyDocument = true;

	    /// <summary>
        /// Gets or sets the password for view protection of the PDF document.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string UserPassword
        {
            get { return _userPassword; }
            set { _userPassword = value; }
        }

        /// <summary>
        /// Gets or sets the password for edit protection of the PDF document.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string OwnerPassword
        {
            get { return _ownerPassword; }
            set { _ownerPassword = value; }
        }

        /// <summary>
        /// Gets or sets the encryption algorithm.
        /// </summary>
        /// <value cref="PDF.EncryptionAlgorithm"></value>
        public EncryptionAlgorithm EncryptionAlgorithm
        {
            get { return _encryptionAlgorithm; }
            set { _encryptionAlgorithm = value; }
        }

        /// <summary>
        /// Allows or prohibits printing a PDF document.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool AllowPrintDocument
        {
            get { return _allowPrintDocument; }
            set { _allowPrintDocument = value; }
        }

        /// <summary>
        /// Gets or sets print quality.
        /// <remarks>This property has effect when a 128 or 256 bit encryption key is used and print document is allowed.</remarks>
        /// </summary>
        /// <value cref="PDF.PrintQuality"></value>
        public PrintQuality PrintQuality
        {
            get { return _printQuality; }
            set { _printQuality = value; }
        }

        /// <summary>
        /// Allows or prohibits modifying a PDF document.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool AllowModifyDocument
        {
            get { return _allowModifyDocument; }
            set { _allowModifyDocument = value; }
        }

        /// <summary>
        /// Allows or prohibits filling in interactive form fields (including signature fields) in a PDF document.
        /// <remarks>This property has effect when a 128 or 256 bit encryption key is used and modifying annotations is not allowed.</remarks>
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool AllowFillForms
        {
            get { return _allowFillForms; }
            set { _allowFillForms = value; }
        }

        /// <summary>
        /// Allows or prohibits interacting with text annotations and forms in a PDF document.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool AllowModifyAnnotations
        {
            get { return _allowModifyAnnotations; }
            set { _allowModifyAnnotations = value; }
        }

        /// <summary>
        /// Allows or prohibits copying content from a PDF document.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool AllowContentExtraction
        {
            get { return _allowContentExtraction; }
            set { _allowContentExtraction = value; }
        }

        /// <summary>
        /// Allows or prohibits content extraction for accessibility.
        /// <remarks>This property has effect when a 128 or 256 bit encryption key is used.</remarks>
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool AllowAccessibilitySupport
        {
            get { return _allowAccessibilitySupport; }
            set { _allowAccessibilitySupport = value; }
        }

        /// <summary>
        /// Allows or prohibits assembling the document.
        /// <remarks>This property has effect when a 128 or 256 bit encryption key is used and modify document is not allowed.</remarks>
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool AllowAssemlyDocument
        {
            get { return _allowAssemlyDocument; }
            set { _allowAssemlyDocument = value; }
        }

	    internal Security()
	    {
	    }

	    internal void SetPermissions(int perms)
        {
            // See TABLE 3.20 for more information
            _allowPrintDocument = (perms >> 2) % 2 != 0;
            _allowModifyDocument = (perms >> 3) % 2 != 0;
            _allowContentExtraction = (perms >> 4) % 2 != 0;
            _allowModifyAnnotations = (perms >> 5) % 2 != 0;

            if (_encryptionAlgorithm == EncryptionAlgorithm.RC4_40bit)
            {
                _allowFillForms = true;
                _allowAccessibilitySupport = true;
                _allowAssemlyDocument = true;
                _printQuality = PrintQuality.HightResolution;
            }
            else
            {
                _allowFillForms = (perms >> 8) % 2 != 0;
                _allowAccessibilitySupport = (perms >> 9) % 2 != 0;
                _allowAssemlyDocument = (perms >> 10) % 2 != 0;
                if ((perms >> 11) % 2 != 0)
                    _printQuality = PrintQuality.HightResolution;
                else
                    _printQuality = PrintQuality.LowResolution;
            }
        }

        internal int GetPermissions()
        {
            // See TABLE 3.20 for more information
            uint result = 0xFFFFF0C0;
            if(_allowPrintDocument)
                result = result | 4;
            if(_allowModifyDocument)
                result = result | 8;
            if (_allowContentExtraction)
                result = result | 16;
            if (_allowModifyAnnotations)
                result = result | 32;

            if (_encryptionAlgorithm == EncryptionAlgorithm.RC4_40bit)
            {
                result = result | 0xF00;
            }
            else
            {
                if (_allowFillForms)
                    result = result | 256;
                if (_allowAccessibilitySupport)
                    result = result | 512;
                if (_allowAssemlyDocument)
                    result = result | 1024;
                if (_allowPrintDocument && _printQuality == PrintQuality.HightResolution)
                    result = result | 2048;
            }

            return (int)result;
        }
    }
}
