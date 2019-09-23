using System;
//using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Pkcs;

namespace Bytescout.PDF
{
	internal class Certificate
    {
        private string _certPath;
        private string _password;
        private AsymmetricKeyParameter _akp;
        private X509Certificate[] _chain;

        public AsymmetricKeyParameter Akp
        {
            get { return _akp; }
        }

        public X509Certificate[] Chain
        {
            get { return _chain; }
        }

        //public Certificate(string path)
        //{
        //    _certPath = path;
        //    Load();
        //}

        public Certificate(string path, string password)
        {
            _certPath = path;
            _password = password;
            Load();
        }

        private void Load()
        {
            //First we'll read the certificate file
            Pkcs12Store pkcs12 = new Pkcs12Store(new FileStream(_certPath, FileMode.Open, FileAccess.Read), _password.ToCharArray());

            //then Iterate throught certificate entries to find the private key entry
            IEnumerator keys = pkcs12.Aliases.GetEnumerator();
            string alias = null;
            while (keys.MoveNext())
            {
                alias = (string)keys.Current;
                if (pkcs12.IsKeyEntry(alias))
                    break;
            }

            _akp = pkcs12.GetKey(alias).Key;
            X509CertificateEntry[] ce = pkcs12.GetCertificateChain(alias);
            _chain = new X509Certificate[ce.Length];
            for (int i = 0; i < ce.Length; ++i)
                _chain[i] = ce[i].Certificate;
        }
    }
}
