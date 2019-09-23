using System;
using System.IO;

namespace Bytescout.PDF
{
    internal class RC4
    {
	    private const int ARC4_BUFFER_SIZE = 256;
	    private byte _idx1 = 0;
	    private byte _idx2 = 0;
	    private readonly byte[] _state = new byte[ARC4_BUFFER_SIZE];
	    private byte[] _key;

	    public RC4(byte[] key, int length)
        {
            _key = new byte[length];
            Array.Copy(key, _key, length);
        }

        public void SetKey(byte[] key)
        {
            _key = key;
        }

        public byte[] CryptBuffer(byte[] input, int offset, int length)
        {
            initialize();
            byte[] output = new byte[length];
            for (int i = 0; i < length; i++)
            {
                _idx1 = (byte)((_idx1 + 1) % 256);
                _idx2 = (byte)((_idx2 + _state[_idx1]) % 256);

                byte tmp = _state[_idx1];
                _state[_idx1] = _state[_idx2];
                _state[_idx2] = tmp;

                byte t = (byte)((_state[_idx1] + _state[_idx2]) % 256);
                byte K = _state[t];

                output[i] = (byte)(input[offset + i] ^ K);
            }

            return output;
        }

        public void Crypt(byte[] input, int offset, int length, Stream output)
        {
            initialize();
            for (int i = 0; i < length; i++)
            {
                _idx1 = (byte)((_idx1 + 1) % 256);
                _idx2 = (byte)((_idx2 + _state[_idx1]) % 256);

                byte tmp = _state[_idx1];
                _state[_idx1] = _state[_idx2];
                _state[_idx2] = tmp;

                byte t = (byte)((_state[_idx1] + _state[_idx2]) % 256);
                byte K = _state[t];

                output.WriteByte((byte)(input[offset + i] ^ K));
            }
        }

        public void Crypt(Stream input, int length, Stream output)
        {
            initialize();
            for (int i = 0; i < length; i++)
            {
                _idx1 = (byte)((_idx1 + 1) % 256);
                _idx2 = (byte)((_idx2 + _state[_idx1]) % 256);

                byte tmp = _state[_idx1];
                _state[_idx1] = _state[_idx2];
                _state[_idx2] = tmp;

                byte t = (byte)((_state[_idx1] + _state[_idx2]) % 256);
                byte K = _state[t];

                int b = input.ReadByte();
                output.WriteByte((byte)(b ^ K));
            }
        }

        private void initialize()
        {
            for (int i = 0; i < ARC4_BUFFER_SIZE; i++)
                _state[i] = (byte)i;

            byte[] tmp_array = new byte[ARC4_BUFFER_SIZE];
            for (int i = 0; i < ARC4_BUFFER_SIZE; i++)
                tmp_array[i] = _key[i % _key.Length];

            int j = 0;
            for (int i = 0; i < ARC4_BUFFER_SIZE; i++)
            {
                j = (j + _state[i] + tmp_array[i]) % ARC4_BUFFER_SIZE;
                byte tmp = _state[i];
                _state[i] = _state[j];
                _state[j] = tmp;
            }

            _idx1 = 0;
            _idx2 = 0;
        }
    }
}
