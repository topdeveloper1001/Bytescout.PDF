using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ThreeDData
#else
	/// <summary>
    /// Represents a 3D data (.u3d) in PDF document.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class ThreeDData
#endif
	{
	    private readonly PDFDictionaryStream _stream;

		//TODO:
		//private PDF3DViews m_views;

	    /// <summary>
        /// Creates a new 3D data initialized from the specified existing file.
        /// </summary>
        /// <param name="filename" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The path to the data.</param>
        public ThreeDData(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            _stream = new PDFDictionaryStream();
            initData(fs, _stream);
            fs.Close();
        }

        /// <summary>
        /// Creates a new 3D data initialized from specified stream.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">A stream that contains the data.</param>
        public ThreeDData(Stream stream)
        {
            _stream = new PDFDictionaryStream();
            initData(stream, _stream);
        }

        /// <summary>
        /// Saves the contents of this Bytescout.PDF.ThreeDData to the specified file.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the file to which to save the data.</param>
        public void Save(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Create);
            save(stream);
        }

        /// <summary>
        /// Saves the contents of this Bytescout.PDF.ThreeDData to the specified stream.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">The stream to which to save the data.</param>
        public void Save(Stream stream)
        {
            save(stream);
        }

        internal PDFDictionaryStream GetDictionary()
        {
            return _stream;
        }

        private void save(Stream stream)
        {
            _stream.Decode();
            _stream.GetStream().WriteTo(stream);
        }

        private void initData(Stream fs, PDFDictionaryStream stream)
        {
            stream.Dictionary.AddItem("Type", new PDFName("3D"));
            stream.Dictionary.AddItem("Subtype", new PDFName("U3D"));
            stream.GetStream().SetLength(fs.Length);
            fs.Read(stream.GetStream().GetBuffer(), 0, (int)fs.Length);
        }

        internal static IPDFObject Copy(IPDFObject obj)
        {
            IPDFObject result;
            if (obj is PDFDictionary)
            {
                result = new PDFDictionary();
                (result as PDFDictionary).AddItem("Type", new PDFName("3DRef"));
                IPDFObject threedd = (obj as PDFDictionary)["3DD"];
                (result as PDFDictionary).AddItem("3DD", threedd);
                return result;
            }

            return obj;
        }
    }
}
