using System;

namespace Bytescout.PDF
{
	internal enum ResourceType
	{
		ExtGState = 1,
		ColorSpace = 2,
		Pattern = 3,
		Shading = 4,
		XObject = 5,
		Font = 6,
		Properties = 7,
	}

	internal class Resources : ICloneable
	{
		private ReadOnlyCollection<Image> _images;
		private readonly PDFDictionary _dictionary;

		public PDFDictionary Dictionary
		{
			get
			{
				return _dictionary;
			}
		}

		public ReadOnlyCollection<Image> Images
		{ 
			get
			{
				if (_images == null)
				{
                    
					initImages();
				}

				return _images;
			}
		}

		public Resources() : this(true) { }

		public Resources(bool addProcset)
		{
			_dictionary = new PDFDictionary();
			if (addProcset)
				addProcSet();
			initImages();
		}

		public Resources(PDFDictionary dictionary) : this(dictionary, true) { }

		public Resources(PDFDictionary dictionary, bool addProcset)
		{
			if (dictionary == null)
				throw new PDFException();
			_dictionary = dictionary;
			initImages();
			if (addProcset)
				addProcSet();
		}

		public string AddResources(ResourceType type, IPDFObject obj)
		{
			string key = resourceTypeToString(type);
			addDictionaryType(key);

			PDFDictionary res = _dictionary[key] as PDFDictionary;
			string name;

			if (res.Contains(obj, out name))
				return name;

			name = getVacantResourseName(res, type);
			res.AddItem(name, obj);
			if (type == ResourceType.XObject)
			{ 
				PDFDictionaryStream stream = obj as PDFDictionaryStream;
				if (stream != null)
				{
					PDFName imagename = stream.Dictionary["Subtype"] as PDFName;
					if (imagename != null)
					{
						if (imagename.GetValue() == "Image")
						{
							_images.AddItem(new Image(stream));
						}
					}
				}
			}
			return name;
		}

		public IPDFObject GetResource(string name, ResourceType type)
		{
			PDFDictionary dict = _dictionary[resourceTypeToString(type)] as PDFDictionary;
			if (dict != null)
				return dict[name];
			return null;
		}

		private void addDictionaryType(string key)
		{
			if (_dictionary[key] == null)
			{
				PDFDictionary dict = new PDFDictionary();
				_dictionary.AddItem(key, dict);
			}
		}

		private string resourceTypeToString(ResourceType type)
		{
			switch (type)
			{
				case ResourceType.ColorSpace:
					return "ColorSpace";
				case ResourceType.ExtGState:
					return "ExtGState";
				case ResourceType.Font:
					return "Font";
				case ResourceType.Pattern:
					return "Pattern";
				case ResourceType.Properties:
					return "Properties";
				case ResourceType.Shading:
					return "Shading";
				case ResourceType.XObject:
					return "XObject";
			}
			return "";
		}

		private string createPrefix(ResourceType type)
		{
			switch (type)
			{
				case ResourceType.ColorSpace:
					return "CS";
				case ResourceType.ExtGState:
					return "GS";
				case ResourceType.Font:
					return "F";
				case ResourceType.Pattern:
					return "P";
				case ResourceType.Properties:
					return "PT";
				case ResourceType.Shading:
					return "SH";
				case ResourceType.XObject:
					return "X";
			}
			return "";
		}

		private string getVacantResourseName(PDFDictionary dict, ResourceType type)
		{
			string name = createPrefix(type);
			for (int i = 0; i < 10000; ++i)
			{
				if (dict[name + i.ToString()] == null)
					return name + i.ToString();
			}

			return "";
		}
        
		private void addProcSet()
		{
			PDFArray procSet = new PDFArray();
			procSet.AddItem(new PDFName("PDF"));
			procSet.AddItem(new PDFName("Text"));
			procSet.AddItem(new PDFName("ImageB"));
			procSet.AddItem(new PDFName("ImageC"));
			procSet.AddItem(new PDFName("ImageI"));
			_dictionary.AddItem("ProcSet", procSet);
		}

		public object Clone()
		{
			PDFDictionary dict = ResourcesBase.Copy(_dictionary);
			Resources resources = new Resources(dict, false);
			return resources;
		}

		private void initImages ()
		{
			_images = new ReadOnlyCollection<Image>();
			PDFDictionary dictionary = _dictionary["XObject"] as PDFDictionary;
			if (dictionary != null)
			{
				string[] keys = dictionary.GetKeys();

				for (int i = 0; i < keys.Length; ++i)
				{
					PDFDictionaryStream dict = dictionary[keys[i]] as PDFDictionaryStream;
					if (dict != null)
					{
						PDFName name = dict.Dictionary["Subtype"] as PDFName;
						if (name != null)
						{
							if (name.GetValue() == "Image")
								_images.AddItem(new Image(dict));
						}
					}
				}
			}
		}
	}
}