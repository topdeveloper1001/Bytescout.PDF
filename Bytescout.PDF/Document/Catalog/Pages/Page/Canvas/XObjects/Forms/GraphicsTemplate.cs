using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class GraphicsTemplate : Canvas
#else
	/// <summary>
    /// Represents a graphics template.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class GraphicsTemplate : Canvas
#endif
	{
	    private PDFDictionaryStream _dict;

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.GraphicsTemplate class.
        /// </summary>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the template.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the template.</param>
        public GraphicsTemplate(float width, float height) : base (width, height)
        {
            init(null);
        }

        internal GraphicsTemplate(float width, float height, float[] matrix)
            : base(width, height)
        {
            init(matrix);
        }

        internal GraphicsTemplate(MemoryStream stream, Resources resources, float width, float height) : base (stream, resources, width, height)
        {
            init(null);
        }

        internal void WriteForm(MemoryStream stream, Resources resources)
        {
            string name = resources.AddResources(ResourceType.XObject, _dict);
            IPDFPageOperation operation = new DoXObject(name);
            operation.WriteBytes(stream);
        }

        internal static PDFDictionaryStream Copy(PDFDictionaryStream dictStream)
        {
            Stream stream = dictStream.GetStream();
            byte[] buf = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(buf, 0, buf.Length);

            MemoryStream newStream = new MemoryStream();
            newStream.Write(buf, 0, buf.Length);

            PDFDictionary dict = dictStream.Dictionary;
            PDFDictionary newDict = new PDFDictionary();

            string[] keys = { "Type", "Subtype", "FormType", "BBox", "Matrix", "Ref", "Metadata", "LastModified", "Name" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = dict[keys[i]];
                if (obj != null)
                    newDict.AddItem(keys[i], obj.Clone());
            }

            PDFDictionary resources = dict["Resources"] as PDFDictionary;
            if (resources != null)
                newDict.AddItem("Resources", ResourcesBase.Copy(resources));

            PDFDictionary group = dict["Group"] as PDFDictionary;
            if (group != null)
                newDict.AddItem("Group", GroupBase.Copy(group));

            // PieceInfo, OPI, OC - still unknown
            // StructParent, StructParents - do not

            IPDFObject filter = dict["Filter"];
            if (filter != null)
                newDict.AddItem("Filter", filter.Clone());

            IPDFObject decodeParms = dict["DecodeParms"];
            if (decodeParms != null)
                newDict.AddItem("DecodeParms", decodeParms.Clone());

            return new PDFDictionaryStream(newDict, newStream);
        }

        internal PDFDictionaryStream GetDictionaryStream()
        {
            return _dict;
        }

	    private void init(float[] matrix)
        {
            _dict = new PDFDictionaryStream(new PDFDictionary(), Stream);
            _dict.Dictionary.AddItem("Type", new PDFName("XObject"));
            _dict.Dictionary.AddItem("Subtype", new PDFName("Form"));
            PDFArray array = new PDFArray();
            array.AddItem(new PDFNumber(0));
            array.AddItem(new PDFNumber(0));
            array.AddItem(new PDFNumber(Width));
            array.AddItem(new PDFNumber(Height));
            _dict.Dictionary.AddItem("BBox", array);
            _dict.Dictionary.AddItem("Resources", Resources.Dictionary);
            array = new PDFArray();

            if (matrix != null)
            {
                array.AddItem(new PDFNumber(matrix[0]));
                array.AddItem(new PDFNumber(matrix[1]));
                array.AddItem(new PDFNumber(matrix[2]));
                array.AddItem(new PDFNumber(matrix[3]));
                array.AddItem(new PDFNumber(matrix[4]));
                array.AddItem(new PDFNumber(matrix[5]));
                _dict.Dictionary.AddItem("Matrix", array);
            }
        }
    }
}
