using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class SoundAction : Action
#else
	/// <summary>
    /// Represents the sound action.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class SoundAction : Action
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private Sound _sound;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.Sound; } }

	    /// <summary>
	    /// Gets the sound.
	    /// </summary>
	    /// <value cref="PDF.Sound"></value>
	    public Sound Sound
	    {
		    get
		    {
			    if (_sound == null)
				    loadSound();
			    return _sound;
		    }
	    }

	    /// <summary>
	    /// Gets or sets the volume at which to play the sound, in the range -1.0 to 1.0.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float Volume
	    {
		    get
		    {
			    PDFNumber vol = _dictionary["Volume"] as PDFNumber;
			    if (vol == null)
				    return 1;
			    return (float)vol.GetValue();
		    }
		    set
		    {
			    if (value < -1 || value > 1)
				    throw new PDFVolumeException();

			    _dictionary.AddItem("Volume", new PDFNumber(value));
		    }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether to play the sound synchronously or asynchronously.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Synchronous
	    {
		    get
		    {
			    PDFBoolean sync = _dictionary["Synchronous"] as PDFBoolean;
			    if (sync == null)
				    return false;
			    return sync.GetValue();
		    }
		    set
		    {
			    _dictionary.AddItem("Synchronous", new PDFBoolean(value));
		    }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether to repeat the sound indefinitely.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Repeat
	    {
		    get
		    {
			    PDFBoolean repeat = _dictionary["Repeat"] as PDFBoolean;
			    if (repeat == null)
				    return false;
			    return repeat.GetValue();
		    }
		    set
		    {
			    _dictionary.AddItem("Repeat", new PDFBoolean(value));
		    }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether to mix this sound with any other sound already playing.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Mix
	    {
		    get
		    {
			    PDFBoolean mix = _dictionary["Mix"] as PDFBoolean;
			    if (mix == null)
				    return false;
			    return mix.GetValue();
		    }
		    set
		    {
			    _dictionary.AddItem("Mix", new PDFBoolean(value));
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.SoundAction.
        /// </summary>
        /// <param name="sound">Sound to be played.</param>
        public SoundAction(Sound sound)
            : base(null)
        {
            if (sound == null)
                throw new ArgumentNullException();

            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("Sound"));
            _dictionary.AddItem("Sound", sound.GetDictionaryStream());
            _sound = sound;
        }

        internal SoundAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("Sound"));

            IPDFObject sound = _dictionary["Sound"];
            if (sound != null)
                dict.AddItem("Sound", sound);

            string[] keys = { "Volume", "Synchronous", "Repeat", "Mix" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = _dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            SoundAction action = new SoundAction(dict, owner);

            IPDFObject next = _dictionary["Next"];
            if (next != null)
            {
                for (int i = 0; i < Next.Count; ++i)
                    action.Next.Add(Next[i]);
            }

            return action;
        }

        internal override PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        private void loadSound()
        {
            PDFDictionaryStream sound = _dictionary["Sound"] as PDFDictionaryStream;
            if (sound != null)
                _sound = new Sound(sound);
        }
    }
}
