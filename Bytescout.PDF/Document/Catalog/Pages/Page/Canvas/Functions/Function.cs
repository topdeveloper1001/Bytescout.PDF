namespace Bytescout.PDF
{
    internal enum PDFFunctionType
    {
        Sampled = 0,
        Exponential = 2,
        Stitching = 3,
        PostScript = 4
    }

    internal abstract class Function
    {
        public abstract PDFFunctionType FnctionType
        {
            get;
        }

        public abstract float[] Domain
        {
            get;
            set;
        }

        public abstract float[] Range
        {
            get;
            set;
        }

        public abstract PDFDictionaryStream DictionaryStream
        {
            get;
        }
    }

	internal class PDFExponentialFunction : Function
    {
        public PDFExponentialFunction()
        {
            m_stream = new PDFDictionaryStream();
            PDFDictionary dict = m_stream.Dictionary;
            PDFArray array = new PDFArray();

            m_domain = new float[2];
            m_domain[0] = 0.0f;
            m_domain[1] = 1.0f;

            m_range = new float[0];

            m_c0 = new float[1];
            m_c0[0] = 0.0f;

            m_c1 = new float[1];
            m_c1[0] = 1.0f;

            m_n = 2.0f;

            dict.AddItem("FunctionType", new PDFNumber((float)PDFFunctionType.Exponential));
            for (int i = 0; i < m_domain.Length; ++i)
            {
                array.AddItem(new PDFNumber(m_domain[i]));
            }
            dict.AddItem("Domain", array);
            array = new PDFArray();
            for (int i = 0; i < m_range.Length; ++i)
            {
                array.AddItem(new PDFNumber(m_range[i]));
            }
            if (array.Count != 0)
                dict.AddItem("Range", array);
            array = new PDFArray();
            for (int i = 0; i < m_c0.Length; ++i)
            {
                array.AddItem(new PDFNumber(m_c0[i]));
            }
            dict.AddItem("C0", array);
            array = new PDFArray();
            for (int i = 0; i < m_c1.Length; ++i)
            {
                array.AddItem(new PDFNumber(m_c1[i]));
            }
            dict.AddItem("C1", array);
            dict.AddItem("N", new PDFNumber(m_n));
        }

        public override PDFFunctionType FnctionType
        {
            get
            {
                return PDFFunctionType.Exponential;
            }
        }
        public override float[] Domain
        {
            get
            {
                return (float[])m_domain.Clone();
            }
            set
            {
                m_domain = (float[])value.Clone();
                PDFArray array = new PDFArray();
                for (int i = 0; i < m_domain.Length; ++i)
                {
                    array.AddItem(new PDFNumber(m_domain[i]));
                }
                m_stream.Dictionary.AddItem("Domain", array);
            }
        }

        public override float[] Range
        {
            get
            {
                return (float[])m_range.Clone();
            }
            set
            {
                m_range = (float[])value.Clone();
                PDFArray array = new PDFArray();
                for (int i = 0; i < m_range.Length; ++i)
                {
                    array.AddItem(new PDFNumber(m_range[i]));
                }
                m_stream.Dictionary.AddItem("Range", array);
            }
        }

        public override PDFDictionaryStream DictionaryStream
        {
            get
            {
                return m_stream;
            }
        }

        public float[] C0
        {
            get
            {
                return (float[])m_c0.Clone();
            }
            set
            {
                m_c0 = (float[])value.Clone();
                PDFArray array = new PDFArray();
                for (int i = 0; i < m_c0.Length; ++i)
                {
                    array.AddItem(new PDFNumber(m_c0[i]));
                }
                m_stream.Dictionary.AddItem("C0", array);
            }
        }

        public float[] C1
        {
            get
            {
                return (float[])m_c1.Clone();
            }
            set
            {
                m_c1 = (float[])value.Clone();
                PDFArray array = new PDFArray();
                for (int i = 0; i < m_c1.Length; ++i)
                {
                    array.AddItem(new PDFNumber(m_c1[i]));
                }
                m_stream.Dictionary.AddItem("C1", array);
            }
        }

        public float N
        {
            get
            {
                return m_n;
            }
            set
            {
                m_n = value;
                m_stream.Dictionary.AddItem("N", new PDFNumber(m_n));
            }
        }

        private PDFDictionaryStream m_stream;
        private float[] m_domain;
        private float[] m_range;
        private float[] m_c0;
        private float[] m_c1;
        private float m_n;
    }

    //internal class PDFStitchingFunction : Function
    //{ 

    //}

    //internal class PDFPostScriptFunction : Function
    //{ 

    //}
}
