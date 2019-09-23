using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace Bytescout.PDF
{
    class Signing
    {
        private Stream _document;
        private int[] _byteRange;
        private Certificate _cert;

        //private PKCS7 pkcs;
        //protected String hashAlgorithm;

        public Signing(Stream document, int[] byteRange, Certificate cert)
        {
            _document = document;
            _byteRange = byteRange;
            _cert = cert;
        }

        public void WriteDigest()
        {
            PKCS7 pkcs = new PKCS7(_cert.Akp, _cert.Chain, null, "SHA1", true);

            for (int i = 0; i < _byteRange.Length; i += 2)
            {
                byte[] buf = new byte[_byteRange[i + 1]];
                _document.Position = _byteRange[i];
                _document.Read(buf, 0, _byteRange[i + 1]);
                pkcs.Update(buf, 0, buf.Length);
            }

            byte[] bsig = pkcs.GetEncodedPKCS7(null, DateTime.Now, null);
            //byte[] sbuf = new byte[bsig.Length/* +64*/];
            //Array.Copy(bsig, 0, sbuf, 0, bsig.Length);
            // Записываем дайджест
            //PDFString str = new PDFString(sbuf, true);
            PDFString str = new PDFString(bsig, true);
            SaveParameters sp = new SaveParameters(_document);
            sp.Stream.Position = _byteRange[1];
            str.Write(sp);
        }

    }
}
