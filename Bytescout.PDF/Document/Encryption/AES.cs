using System;
using System.Security.Cryptography;
using System.IO;

namespace Bytescout.PDF
{
    internal class AES
    {
	    private readonly RijndaelManaged _rijndael;

	    public AES(byte[] key)
        {
            _rijndael = new RijndaelManaged();
            _rijndael.Key = key;
        }

        public void Decrypt(byte[] input, int offset, int length, Stream output)
        {
            int blockSize = _rijndael.BlockSize / 8;
            if(length < blockSize)
                return;
            byte[] tmp = new byte[blockSize];
            Array.Copy(input, offset, tmp, 0, blockSize);
            _rijndael.IV = tmp;

            ICryptoTransform cryptoTransform = _rijndael.CreateDecryptor();
            int count = length - blockSize;
            if (count % blockSize == 0)
                count = count / blockSize;
            else
                count = count / blockSize + 1;

            byte[] buf = new byte[blockSize];
            int off = offset + blockSize;

            try
            {
                for (int i = 0; i < count - 1; ++i)
                {
                    cryptoTransform.TransformBlock(input, off, blockSize, buf, 0);
                    off += blockSize;
                    if (i != 0)
                        output.Write(buf, 0, blockSize);
                }

                buf = cryptoTransform.TransformFinalBlock(input, off, offset + length - off);
                output.Write(buf, 0, buf.Length);
            }
            catch { }
            cryptoTransform.Dispose();
        }

        public void Decrypt(Stream input, int length, Stream output)
        {
            int blockSize = _rijndael.BlockSize / 8;
            if (length < blockSize)
                return;
            byte[] tmp = new byte[blockSize];
            input.Read(tmp, 0, blockSize);
            _rijndael.IV = tmp;

            ICryptoTransform cryptoTransform = _rijndael.CreateDecryptor();
            int count = length - blockSize;
            if (count % blockSize == 0)
                count = count / blockSize;
            else
                count = count / blockSize + 1;
            byte[] buf = new byte[blockSize];
            

            try
            {
                for (int i = 0; i < count - 1; ++i)
                {
                    input.Read(tmp, 0, blockSize);
                    cryptoTransform.TransformBlock(tmp, 0, blockSize, buf, 0);
                    if (i != 0)
                        output.Write(buf, 0, blockSize);
                }

                int l = length % blockSize;
                if (l == 0)
                    l = blockSize;
                input.Read(tmp, 0, l);
                buf = cryptoTransform.TransformFinalBlock(tmp, 0, l);
                output.Write(buf, 0, buf.Length);
            }
            catch { }
            cryptoTransform.Dispose();
        }

        public void Encrypt(byte[] inputData, int offset, int length, Stream output)
        {
            _rijndael.GenerateIV();
            ICryptoTransform cryptoTransform = _rijndael.CreateEncryptor();

            byte[] iv = _rijndael.IV;
            int bufSize = iv.Length;
            byte[] buf = new byte[bufSize];
            int count = length / bufSize;
            if (length % bufSize == 0)
                --count;
            int off = offset;

            for (int i = 0; i < count; ++i)
            {
                cryptoTransform.TransformBlock(inputData, off, bufSize, buf, 0);
                off += bufSize;
                output.Write(iv, 0, bufSize);

                byte[] tmp = iv;
                iv = buf;
                buf = tmp;
            }

            int l = length % bufSize;
            if (l == 0 && length != 0)
                l = bufSize;
            buf = cryptoTransform.TransformFinalBlock(inputData, off, l);
            output.Write(iv, 0, bufSize);
            output.Write(buf, 0, buf.Length);

            cryptoTransform.Dispose();
        }

        public void SetKey(byte[] key)
        {
            _rijndael.Key = key;
        }
    }
}
