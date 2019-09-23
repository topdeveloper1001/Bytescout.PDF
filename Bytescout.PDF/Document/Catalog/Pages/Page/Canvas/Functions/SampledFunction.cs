using System.IO;

namespace Bytescout.PDF
{
	internal class SampledFunction : Function
	{
		private readonly PDFDictionaryStream _stream;
		private readonly MemoryStream _memoryStream;
		private float[] _domain;
		private float[] _range;
		private int[] _size;
		private int _bitsPerSample;
		private int _order;
		private float[] _encode;
		private float[] _decode;

		public override PDFFunctionType FnctionType
		{
			get
			{
				return PDFFunctionType.Sampled;
			}
		}

		public override float[] Domain
		{
			get
			{
				return (float[])_domain.Clone();
			}
			set
			{
				_domain = (float[])value.Clone();
				PDFArray array = new PDFArray();
				for (int i = 0; i < _domain.Length; ++i)
				{
					array.AddItem(new PDFNumber(_domain[i]));
				}
				_stream.Dictionary.AddItem("Domain", array);
			}
		}

		public override float[] Range
		{
			get
			{
				return (float[])_range.Clone();
			}
			set
			{
				_range = (float[])value.Clone();
				PDFArray array = new PDFArray();
				for (int i = 0; i < _range.Length; ++i)
				{
					array.AddItem(new PDFNumber(_range[i]));
				}
				_stream.Dictionary.AddItem("Range", array);
			}
		}

		public override PDFDictionaryStream DictionaryStream
		{
			get
			{
				return _stream;
			}
		}

		public int BitsPerSample
		{
			get
			{
				return _bitsPerSample;
			}
			set
			{
				_bitsPerSample = value;
				_stream.Dictionary.AddItem("BitsPerSample", new PDFNumber(_bitsPerSample));
			}
		}

		public int[] Size
		{
			get
			{
				return (int[])_size.Clone();
			}
			set
			{
				_size = (int[])value.Clone();
				PDFArray array = new PDFArray();
				for (int i = 0; i < _size.Length; ++i)
				{
					array.AddItem(new PDFNumber(_size[i]));
				}
				_stream.Dictionary.AddItem("Size", array);
			}
		}

		public int Order
		{
			get
			{
				return _order;
			}
			set
			{
				_order = value;
				_stream.Dictionary.AddItem("Order", new PDFNumber(_order));
			}
		}

		public float[] Encode
		{
			get
			{
				return (float[])_encode.Clone();
			}
			set
			{
				_encode = (float[])value.Clone();
				PDFArray array = new PDFArray();
				for (int i = 0; i < _encode.Length; ++i)
				{
					array.AddItem(new PDFNumber(_encode[i]));
				}
				_stream.Dictionary.AddItem("Encode", array);
			}
		}

		public float[] Decode
		{
			get
			{
				return (float[])_decode.Clone();
			}
			set
			{
				_decode = (float[])value.Clone();
				PDFArray array = new PDFArray();
				for (int i = 0; i < _decode.Length; ++i)
				{
					array.AddItem(new PDFNumber(_decode[i]));
				}
				_stream.Dictionary.AddItem("Decode", array);
			}
		}

		public MemoryStream Stream
		{
			get
			{
				return _memoryStream;
			}
		}

		public SampledFunction()
		{
			_memoryStream = new MemoryStream();
			_stream = new PDFDictionaryStream(new PDFDictionary(), _memoryStream);

			_domain = new float[2];
			_domain[0] = 0.0f;
			_domain[1] = 1.0f;

			_range = new float[2];
			_range[0] = 0.0f;
			_range[1] = 1.0f;

			_size = new int[1];
			_size[0] = 2;

			_bitsPerSample = 1;

			_order = 1;

			_encode = new float[_size.Length * 2];
			_encode[0] = 0;
			_encode[1] = _size[0] - 1;

			_decode = new float[2];
			_decode[0] = 0.0f;
			_decode[1] = 1.0f;

			initializeDictionary();

			byte b = 1;
			b = (byte)(b << 6);

			_memoryStream.Position = _memoryStream.Length;
			_memoryStream.WriteByte(b);
		}

		public SampledFunction(float[] domain, float[] range, int[] size, int bitsPerSample)
		{
			_memoryStream = new MemoryStream();
			_stream = new PDFDictionaryStream(new PDFDictionary(), _memoryStream);
			PDFDictionary dict = _stream.Dictionary;
			PDFArray array = new PDFArray();

			_domain = (float[])domain.Clone();
			_range = (float[])range.Clone();
			_size = (int[])size.Clone();
			_bitsPerSample = bitsPerSample;

			_encode = new float[_size.Length * 2];
			for (int i = 0; i < _size.Length; ++i)
			{
				_encode[2 * i] = 0;
				_encode[2 * i + 1] = _size[i] - 1;
			}

			_decode = (float[])range.Clone();

			initializeDictionary();
		}

		private void initializeDictionary()
		{
			PDFArray array = new PDFArray();
			PDFDictionary dict = _stream.Dictionary;

			dict.AddItem("FunctionType", new PDFNumber((float)PDFFunctionType.Sampled));
			for (int i = 0; i < _domain.Length; ++i)
			{
				array.AddItem(new PDFNumber(_domain[i]));
			}
			dict.AddItem("Domain", array);
			array = new PDFArray();
			for (int i = 0; i < _range.Length; ++i)
			{
				array.AddItem(new PDFNumber(_range[i]));
			}
			dict.AddItem("Range", array);
			array = new PDFArray();
			for (int i = 0; i < _size.Length; ++i)
			{
				array.AddItem(new PDFNumber(_size[i]));
			}
			dict.AddItem("Size", array);
			dict.AddItem("BitsPerSample", new PDFNumber(_bitsPerSample));
			dict.AddItem("Order", new PDFNumber(_order));
			array = new PDFArray();
			for (int i = 0; i < _encode.Length; ++i)
			{
				array.AddItem(new PDFNumber(_encode[i]));
			}
			dict.AddItem("Encode", array);
			array = new PDFArray();
			for (int i = 0; i < _decode.Length; ++i)
			{
				array.AddItem(new PDFNumber(_decode[i]));
			}
			dict.AddItem("Decode", array);
		}
	}
}