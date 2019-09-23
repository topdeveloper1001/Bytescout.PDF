namespace Bytescout.PDF
{
    internal class AcroForm
    {
	    private readonly PDFDictionary _dictionary;
	    private Resources _resources;

	    internal Resources Resources
	    {
		    get
		    {
			    if (_resources == null)
				    loadResources();
			    return _resources;
		    }
	    }

	    internal PDFString DefaultAttribute
	    {
		    get { return _dictionary["DA"] as PDFString; }
	    }

	    internal AcroForm()
        {
            _dictionary = new PDFDictionary();
        }

        internal AcroForm(PDFDictionary dict)
        {
            _dictionary = dict;
        }

	    internal void AddField(Field field)
        {
            if (_dictionary["Fields"] as PDFArray == null)
                _dictionary.AddItem("Fields", new PDFArray());

            if (field is RadioButton)
            {
                addRadioButton(field);
            }
            else
            {
                ((PDFArray) _dictionary["Fields"]).AddItem(field.Dictionary);
            }
        }

        internal void RemoveField(PDFDictionary dict)
        {
            PDFDictionary parent = dict["Parent"] as PDFDictionary;
            if (parent != null)
            {
                PDFArray kids = parent["Kids"] as PDFArray;
                if (kids != null)
                {
                    for (int i = 0; i < kids.Count; ++i)
                    {
                        if (kids[i] == dict)
                        {
                            kids.RemoveItem(i);
                            break;
                        }
                    }

                    if (kids.Count == 0)
                        RemoveField(parent);
                    return;
                }
            }

            PDFArray fields = _dictionary["Fields"] as PDFArray;
            for (int i = 0; i < fields.Count; ++i)
            {
                if (fields[i] == dict)
                {
                    fields.RemoveItem(i);
                    return;
                }
            }
        }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        private void loadResources()
        {
            PDFDictionary resources = _dictionary["DR"] as PDFDictionary;
            if (resources != null)
            {
                _resources = new Resources(resources, false);
            }
            else
            {
                _resources = new Resources(false);
                _dictionary.AddItem("DR", _resources.Dictionary);
            }
        }

        private void addRadioButton(Field field)
        {
            PDFDictionary dict = findRadioButtonGroup(field.Name);
            if (dict == null)
                dict = createRadioButtonGroup(field.Name);

            PDFArray kids = dict["Kids"] as PDFArray;
            if (kids == null)
            {
                kids = new PDFArray();
                dict.AddItem("Kids", kids);
            }

            if (field is RadioButton && (field as RadioButton).Checked == true)
            {
                dict.AddItem("V", new PDFName((field as RadioButton).ExportValue));
                dict.AddItem("DV", new PDFName((field as RadioButton).ExportValue));
            }

            kids.AddItem(field.Dictionary);
            field.Dictionary.AddItem("Parent", dict);
            field.Dictionary.RemoveItem("T");
        }

        private PDFDictionary createRadioButtonGroup(string name)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("FT", new PDFName("Btn"));
            dict.AddItem("Ff", new PDFNumber(49152));
            dict.AddItem("V", new PDFName("Off"));
            dict.AddItem("DV", new PDFName("Off"));
            dict.AddItem("Kids", new PDFArray());
            dict.AddItem("T", new PDFString(name));

            (_dictionary["Fields"] as PDFArray).AddItem(dict);
            return dict;
        }

        private PDFDictionary findRadioButtonGroup(string name)
        {
            PDFArray fields = _dictionary["Fields"] as PDFArray;
            for (int i = 0; i < fields.Count; ++i)
            {
                PDFDictionary dict = fields[i] as PDFDictionary;
                if (dict != null)
                {
                    PDFName ft = dict["FT"] as PDFName;
                    PDFNumber ff = dict["Ff"] as PDFNumber;
                    PDFString t = dict["T"] as PDFString;
                    if (ft != null && ff != null && t != null)
                    {
                        if (ft.GetValue() == "Btn" && t.GetValue() == name &&
                            ((uint)ff.GetValue() >> 15) % 2 != 0)
                            return dict;
                    }
                }
            }
            return null;
        }

        private bool containsField(string fieldName, PDFArray fields)
        {
            if (fields == null)
                return false;

            for (int i = 0; i < fields.Count; ++i)
            {
                PDFDictionary dict = fields[i] as PDFDictionary;
                if (dict != null)
                {
                    PDFString name = dict["T"] as PDFString;
                    if (name != null && name.GetValue() == fieldName)
                        return true;

                    PDFArray kids = dict["Kids"] as PDFArray;
                    if (kids != null)
                        return containsField(fieldName, kids);
                }
            }

            return false;
        }
    }
}
