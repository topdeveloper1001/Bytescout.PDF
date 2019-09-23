using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Sound
#else
	/// <summary>
    /// Represents a sound embedded into the PDF document.
    /// <remarks>The AIFF, AIFF-C, RIFF (.wav), and snd (.au) file formats are supported.</remarks>
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class Sound
#endif
	{
	    private readonly PDFDictionaryStream _stream;

	    /// <summary>
	    ///  Gets the sampling rate, in samples per second (in Hz).
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float SamplingRate
	    {
		    get
		    {
			    PDFNumber number = _stream.Dictionary["R"] as PDFNumber;
			    if (number == null)
				    return 8000;
			    return (float)number.GetValue();
		    }
		    internal set
		    {
			    _stream.Dictionary.AddItem("R", new PDFNumber(value));
		    }
	    }

	    /// <summary>
	    /// Gets the number of sound channels.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int Channels
	    {
		    get
		    {
			    PDFNumber number = _stream.Dictionary["C"] as PDFNumber;
			    if (number == null)
				    return 1;
			    return (int)number.GetValue();
		    }
		    internal set
		    {
			    _stream.Dictionary.AddItem("C", new PDFNumber(value));
		    }
	    }

	    /// <summary>
	    /// Gets the number of bits per sample value per channel.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int BitsPerSample
	    {
		    get
		    {
			    PDFNumber number = _stream.Dictionary["B"] as PDFNumber;
			    if (number == null)
				    return 8;
			    return (int)number.GetValue();
		    }
		    internal set
		    {
			    _stream.Dictionary.AddItem("B", new PDFNumber(value));
		    }
	    }

	    /// <summary>
	    /// Gets the encoding format for the sample data.
	    /// </summary>
	    /// <value cref="SoundEncoding"></value>
	    public SoundEncoding Encoding
	    {
		    get { return TypeConverter.PDFNameToPDFSoundEncoding(_stream.Dictionary["E"] as PDFName); }
	    }

	    /// <summary>
        /// Creates a new sound initialized with the data from the specified existing file.
        /// </summary>
        /// <param name="filename" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The path to the sound.</param>
        public Sound(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            _stream = new PDFDictionaryStream();
            initSound(fs, _stream);
            fs.Close();
        }

        /// <summary>
        /// Creates a new sound initialized with the data from the specified stream.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">A stream that contains the data.</param>
        public Sound(Stream stream)
        {
            _stream = new PDFDictionaryStream();
            stream.Position = 0;
            initSound(stream, _stream);
        }

        internal Sound(PDFDictionaryStream stream)
        {
            _stream = stream;
        }

	    internal PDFDictionaryStream GetDictionaryStream()
        {
            return _stream;
        }

        private void initSound(Stream streamSound, PDFDictionaryStream streamOut)
        {
            byte[] word = new byte[4];
            streamSound.Read(word, 0, word.Length);
            string nameMagic = PDF.Encoding.GetString(word);
            if (nameMagic == "RIFF")
            {
                initWAVE(streamSound, streamOut);
            }
            else if (nameMagic == ".snd")
            {
                initAU(streamSound, streamOut);
            }
            else if (nameMagic == "FORM")
            {
                initAIF(streamSound, streamOut);
            }
            else
                throw new PDFUnsupportedSoundFormatException();
        }

        private void initAIF(Stream streamSound, PDFDictionaryStream streamOut)
        {
            byte[] word = new byte[4];
            streamSound.Read(word, 0, word.Length);
            invert(word);
            int sizeForm = BitConverter.ToInt32(word, 0);
            streamSound.Read(word, 0, word.Length);
            if (PDF.Encoding.GetString(word) == "AIFF")
            {
                initAIFF(streamSound, streamOut);
            }
            else if (PDF.Encoding.GetString(word) == "AIFC")
            {
                initAIFC(streamSound, streamOut);
            }
            else
            {
                throw new PDFUnsupportedSoundFormatException();
            }
        }

        private void initAIFF(Stream streamSound, PDFDictionaryStream streamOut)
        {
            byte[] word = new byte[4];
	        streamOut.Dictionary.AddItem("E", new PDFName("Signed"));
            streamSound.Read(word, 0, word.Length);
            if (PDF.Encoding.GetString(word) == "COMM")
            {
                streamSound.Position += 4;
                byte[] buf = new byte[2];
                streamSound.Read(buf, 0, buf.Length);
                invert(buf);
                streamOut.Dictionary.AddItem("C", new PDFNumber(BitConverter.ToInt16(buf, 0)));
                streamSound.Position += 4;
                streamSound.Read(buf, 0, buf.Length);
                invert(buf);
                int bitPerDepth = BitConverter.ToInt16(buf, 0);
                streamOut.Dictionary.AddItem("B", new PDFNumber(bitPerDepth));
                buf = new byte[10];
                streamSound.Read(buf, 0, buf.Length);
                streamOut.Dictionary.AddItem("R", new PDFNumber((float)BinaryUtility.ConvertFromIeeeExtended(buf)));
                streamSound.Read(word, 0, word.Length);
                if (PDF.Encoding.GetString(word) == "SSND")
                {
                    streamSound.Read(word, 0, word.Length);
                    invert(word);
                    int size = BitConverter.ToInt32(word, 0);
                    streamSound.Read(word, 0, word.Length);
                    invert(word);
                    int offset = BitConverter.ToInt32(word, 0);
                    streamSound.Read(word, 0, word.Length);
                    invert(word);
                    int blockSize = BitConverter.ToInt32(word, 0);
                    streamSound.Position += offset;
                    byte[] data = new byte[size];
                    streamSound.Read(data, 0, data.Length);
                    streamOut.GetStream().Write(data, 0, data.Length);
                }
                else
                {
                    throw new PDFUnsupportedSoundFormatException();
                }
            }
            else
            {
                throw new PDFUnsupportedSoundFormatException();
            }
        }

        private void initAIFC(Stream streamSound, PDFDictionaryStream streamOut)
        {
            byte[] word = new byte[4];
	        streamSound.Read(word, 0, word.Length);
            if (PDF.Encoding.GetString(word) == "COMM")
            {
                streamSound.Read(word, 0, word.Length);
                invert(word);
                long nextPosition = BitConverter.ToInt32(word, 0) + streamSound.Position;
                byte[] buf = new byte[2];
                streamSound.Read(buf, 0, buf.Length);
                invert(buf);
                int channel = BitConverter.ToInt16(buf, 0);
                streamOut.Dictionary.AddItem("C", new PDFNumber(channel));
                streamSound.Position += 4;
                streamSound.Read(buf, 0, buf.Length);
                invert(buf);
                int bitPerDepth = BitConverter.ToInt16(buf, 0);
                streamOut.Dictionary.AddItem("B", new PDFNumber(bitPerDepth));
                buf = new byte[10];
                streamSound.Read(buf, 0, buf.Length);
                streamOut.Dictionary.AddItem("R", new PDFNumber((float)BinaryUtility.ConvertFromIeeeExtended(buf)));
                streamSound.Read(word, 0, word.Length);
                if (PDF.Encoding.GetString(word) == "NONE" && channel >= 2)
                    streamOut.Dictionary.AddItem("E", new PDFName("Signed"));
                if (PDF.Encoding.GetString(word) == "alaw")
                    streamOut.Dictionary.AddItem("E", new PDFName("ALaw"));
                if (PDF.Encoding.GetString(word) == "ulaw")
                    streamOut.Dictionary.AddItem("E", new PDFName("muLaw"));
                streamSound.Position = nextPosition;
                streamSound.Read(word, 0, word.Length);
                if (PDF.Encoding.GetString(word) == "SSND")
                {
                    streamSound.Read(word, 0, word.Length);
                    invert(word);
                    int size = BitConverter.ToInt32(word, 0);
                    streamSound.Read(word, 0, word.Length);
                    invert(word);
                    int offset = BitConverter.ToInt32(word, 0);
                    streamSound.Read(word, 0, word.Length);
                    invert(word);
                    int blockSize = BitConverter.ToInt32(word, 0);
                    streamSound.Position += offset;
                    byte[] data = new byte[size];
                    streamSound.Read(data, 0, data.Length);
                    streamOut.GetStream().Write(data, 0, data.Length);
                }
                else
                {
                    throw new PDFUnsupportedSoundFormatException();
                }
            }
            else
            {
                throw new PDFUnsupportedSoundFormatException();
            }
        }

        private void initAU(Stream streamSound, PDFDictionaryStream streamOut)
        {
            byte[] word = new byte[4];
            streamSound.Position = 12;
            streamSound.Read(word, 0, word.Length);
            int compression = BitConverter.ToInt32(word, 0);
            if (compression == 0)
                streamOut.Dictionary.AddItem("E", new PDFName("Signed"));
            else if (compression == 1)
                streamOut.Dictionary.AddItem("E", new PDFName("muLaw"));
            else if (compression == 27)
                streamOut.Dictionary.AddItem("E", new PDFName("ALaw"));
            streamSound.Read(word, 0, word.Length);
            streamOut.Dictionary.AddItem("R", new PDFNumber(BitConverter.ToInt32(word, 0)));
            streamSound.Read(word, 0, word.Length);
            int channel = BitConverter.ToInt32(word, 0);
            streamOut.Dictionary.AddItem("B", new PDFNumber(channel));
            streamOut.GetStream().SetLength(streamSound.Length);
            streamSound.Read(streamOut.GetStream().GetBuffer(), 0, streamOut.GetStream().GetBuffer().Length);
        }

        private void initWAVE(Stream streamSound, PDFDictionaryStream streamOut)
        {
            byte[] word = new byte[4];
            int bitPerDepth = 8;
            streamSound.Position = 12;
            int load = 0;
	        while (load < 2 && streamSound.Position < streamSound.Length)
            {
                streamSound.Read(word, 0, word.Length);
                string chunkName = PDF.Encoding.GetString(word);
	            long position;

	            if (chunkName == "fmt ")
                {
                    streamSound.Read(word, 0, word.Length);
                    position = streamSound.Position + BitConverter.ToInt32(word, 0);
                    streamSound.Read(word, 0, word.Length);
                    int compression = BitConverter.ToInt16(word, 0);
                    int channel = BitConverter.ToInt16(word, 2);
                    streamOut.Dictionary.AddItem("C", new PDFNumber(channel));
                    streamSound.Read(word, 0, word.Length);
                    streamOut.Dictionary.AddItem("R", new PDFNumber(BitConverter.ToInt32(word, 0)));
                    streamSound.Read(word, 0, word.Length);
                    streamSound.Read(word, 0, word.Length);
                    bitPerDepth = BitConverter.ToInt16(word, 2);
                    streamOut.Dictionary.AddItem("B", new PDFNumber(bitPerDepth));
                    if (compression == 6)
                        streamOut.Dictionary.AddItem("E", new PDFName("ALaw"));
                    if (compression == 7)
                        streamOut.Dictionary.AddItem("E", new PDFName("muLaw"));
                    if (compression == 1 && channel >= 2)
                        streamOut.Dictionary.AddItem("E", new PDFName("Signed"));
                    streamSound.Position = position;
                    ++load;
                }
                else if (chunkName == "data")
                {
                    streamSound.Read(word, 0, word.Length);
                    position = streamSound.Position + BitConverter.ToInt32(word, 0);
                    byte[] data = new byte[BitConverter.ToInt32(word, 0)];
                    streamSound.Read(data, 0, data.Length);
                    if (bitPerDepth > 8)
                    {
                        int countByte = bitPerDepth / 8;
                        for (int i = 0; i < data.Length; i += countByte)
                        {
                            for (int j = 0; j < countByte; ++j)
                            {
                                streamOut.GetStream().WriteByte(data[i + countByte - 1 - j]);
                            }
                        }
                    }
                    else
                        streamOut.GetStream().Write(data, 0, data.Length);
                    ++load;
                    streamSound.Position = position;
                }
            }
            if (load != 2)
                throw new PDFUnsupportedSoundFormatException();
        }

        private void invert(byte[] buf)
        {
            for (int i = 0; i < buf.Length / 2; ++i)
            {
                byte a = buf[i];
                buf[i] = buf[buf.Length - i - 1];
                buf[buf.Length - i - 1] = a;
            }
        }
    }
}
