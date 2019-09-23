
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class MovieActivation
#else
	/// <summary>
    /// Represents the class specifying whether and how to play the movie when the movie annotation is activated.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class MovieActivation
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
        /// Gets or sets the initial speed at which to play the movie.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Speed
        {
            get
            {
                PDFNumber rate = _dictionary["Rate"] as PDFNumber;
                if (rate == null)
                    return 1.0f;
                return (float)(rate.GetValue());
            }
            set
            {
                _dictionary.AddItem("Rate", new PDFNumber(value));
            }
        }

        /// <summary>
        /// Gets or set the initial sound volume at which to play the movie, in the range -1.0 to 1.0.
        /// Higher values denote greater volume; negative values mute the sound.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Volume
        {
            get
            {
                PDFNumber volume = _dictionary["Volume"] as PDFNumber;
                if (volume == null)
                    return 1;
                return (float)volume.GetValue();
            }
            set
            {
                if (value < -1 || value > 1)
                    throw new PDFVolumeException();
                _dictionary.AddItem("Volume", new PDFNumber(value));
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether to display a movie controller bar while playing the movie.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool ShowControls
        {
            get
            {
                PDFBoolean showControls = _dictionary["ShowControls"] as PDFBoolean;
                if (showControls == null)
                    return false;
                return showControls.GetValue();
            }
            set
            {
                _dictionary.AddItem("ShowControls", new PDFBoolean(value));
            }
        }

        /// <summary>
        /// Gets or sets the play mode for playing the movie.
        /// </summary>
        /// <value cref="MoviePlayMode"></value>
        public MoviePlayMode PlayMode
        {
            get { return TypeConverter.PDFNameToMovieMode(_dictionary["Mode"] as PDFName); }
            set { _dictionary.AddItem("Mode", TypeConverter.MovieModeToPDFName(value)); }
        }

        /// <summary>
        /// Gets or sets the value indicating whether to play the movie synchronously or asynchronously.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool Synchronous
        {
            get
            {
                PDFBoolean synchronous = _dictionary["Synchronous"] as PDFBoolean;
                if (synchronous == null)
                    return false;
                return synchronous.GetValue();
            }
            set
            {
                _dictionary.AddItem("Synchronous", new PDFBoolean(value));
            }
        }

        internal float FWPositionHorizontal
        {
            get
            {
                PDFArray array = _dictionary["FWPosition"] as PDFArray;
                if (array == null)
                {
                    return 1;
                }
                if (array.Count != 2)
                {
                    array = new PDFArray();
                    _dictionary.AddItem("FWPosition", array);
                    array.AddItem(new PDFNumber(1));
                    array.AddItem(new PDFNumber(1));
                    return 1;
                }
                PDFNumber number = array[0] as PDFNumber;
                if (number == null)
                    return 1;
                return (float)number.GetValue();
            }
            set
            {
                PDFArray fwPosition = _dictionary["FWPosition"] as PDFArray;
                if (fwPosition == null)
                {
                    fwPosition = new PDFArray();
                    _dictionary.AddItem("FWPosition", fwPosition);
                    fwPosition.AddItem(new PDFNumber(value));
                    fwPosition.AddItem(new PDFNumber(1));
                    return;
                }
                fwPosition.RemoveItem(0);
                fwPosition.Insert(0, new PDFNumber(value));
            }
        }

        internal float FWPositionVertical
        {
            get
            {
                PDFArray array = _dictionary["FWPosition"] as PDFArray;
                if (array == null)
                {
                    return 1;
                }
                if (array.Count != 2)
                {
                    array = new PDFArray();
                    _dictionary.AddItem("FWPosition", array);
                    array.AddItem(new PDFNumber(1));
                    array.AddItem(new PDFNumber(1));
                    return 1;
                }
                PDFNumber number = array[1] as PDFNumber;
                if (number == null)
                    return 1;
                return (float)number.GetValue();
            }
            set
            {
                PDFArray fwPosition = _dictionary["FWPosition"] as PDFArray;
                if (fwPosition == null)
                {
                    fwPosition = new PDFArray();
                    _dictionary.AddItem("FWPosition", fwPosition);
                    fwPosition.AddItem(new PDFNumber(value));
                    fwPosition.AddItem(new PDFNumber(1));
                    return;
                }
                fwPosition.RemoveItem(1);
                fwPosition.AddItem(new PDFNumber(value));
            }
        }

        internal float FWScaleNumenator
        {
            get
            {
                PDFArray array = _dictionary["FWScale"] as PDFArray;
                if (array == null)
                {
                    return 1;
                }
                if (array.Count != 2)
                {
                    array = new PDFArray();
                    _dictionary.AddItem("FWScale", array);
                    array.AddItem(new PDFNumber(1));
                    array.AddItem(new PDFNumber(1));
                    return 1;
                }
                PDFNumber number = array[0] as PDFNumber;
                if (number == null)
                    return 1;
                return (float)number.GetValue();
            }
            set
            {
                PDFArray fwScale = _dictionary["FWScale"] as PDFArray;
                if (fwScale == null)
                {
                    fwScale = new PDFArray();
                    _dictionary.AddItem("FWScale", fwScale);
                    fwScale.AddItem(new PDFNumber(value));
                    fwScale.AddItem(new PDFNumber(1));
                    return;
                }
                fwScale.RemoveItem(0);
                fwScale.Insert(0, new PDFNumber(value));
            }
        }

        internal float FWScaleDenominator
        {
            get
            {
                PDFArray array = _dictionary["FWScale"] as PDFArray;
                if (array == null)
                {
                    return 1;
                }
                if (array.Count != 2)
                {
                    array = new PDFArray();
                    _dictionary.AddItem("FWScale", array);
                    array.AddItem(new PDFNumber(1));
                    array.AddItem(new PDFNumber(1));
                    return 1;
                }
                PDFNumber number = array[1] as PDFNumber;
                if (number == null)
                    return 1;
                return (float)number.GetValue();
            }
            set
            {
                PDFArray fwScale = _dictionary["FWScale"] as PDFArray;
                if (fwScale == null)
                {
                    fwScale = new PDFArray();
                    _dictionary.AddItem("FWScale", fwScale);
                    fwScale.AddItem(new PDFNumber(value));
                    fwScale.AddItem(new PDFNumber(1));
                    return;
                }
                fwScale.RemoveItem(1);
                fwScale.AddItem(new PDFNumber(value));
            }
        }

        //TODO
        internal IPDFObject Start
        {
            get
            {
                return _dictionary["Start"];
            }
            set
            {
                _dictionary.AddItem("Start", value);
            }
        }

        //TODO
        internal IPDFObject Duration
        {
            get
            {
                return _dictionary["Duration"];
            }
            set
            {
                _dictionary.AddItem("Duration", value);
            }
        }

	    internal MovieActivation()
	    {
		    _dictionary = new PDFDictionary();
	    }

	    internal MovieActivation(PDFDictionary dict)
	    {
		    _dictionary = dict;
	    }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }
    }
}
