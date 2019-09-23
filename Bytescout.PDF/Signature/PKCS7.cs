using System;
using System.Collections;
//using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1;

namespace Bytescout.PDF
{
    class PKCS7
    {
        private byte[] digestAttr;
        private int version, signerversion;
        private Hashtable digestalgos;
        private ArrayList certs, crls, signCerts;
        private X509Certificate signCert;
        private byte[] digest;
        private IDigest messageDigest;
        private String digestAlgorithm, digestEncryptionAlgorithm;
        private ISigner sig;
        private ICipherParameters privKey;
        private byte[] RSAdata;

        private static readonly Hashtable digestNames = new Hashtable();
        private static readonly Hashtable algorithmNames = new Hashtable();
        private static readonly Hashtable allowedDigests = new Hashtable();

        private const String ID_PKCS7_DATA = "1.2.840.113549.1.7.1";
        private const String ID_PKCS7_SIGNED_DATA = "1.2.840.113549.1.7.2";
        private const String ID_RSA = "1.2.840.113549.1.1.1";
        private const String ID_DSA = "1.2.840.10040.4.1";
        private const String ID_CONTENT_TYPE = "1.2.840.113549.1.9.3";
        private const String ID_MESSAGE_DIGEST = "1.2.840.113549.1.9.4";
        private const String ID_SIGNING_TIME = "1.2.840.113549.1.9.5";
        private const String ID_ADBE_REVOCATION = "1.2.840.113583.1.1.8";

        static PKCS7() {
            digestNames["1.2.840.113549.2.5"] = "MD5";
            digestNames["1.2.840.113549.2.2"] = "MD2";
            digestNames["1.3.14.3.2.26"] = "SHA1";
            digestNames["2.16.840.1.101.3.4.2.4"] = "SHA224";
            digestNames["2.16.840.1.101.3.4.2.1"] = "SHA256";
            digestNames["2.16.840.1.101.3.4.2.2"] = "SHA384";
            digestNames["2.16.840.1.101.3.4.2.3"] = "SHA512";
            digestNames["1.3.36.3.2.2"] = "RIPEMD128";
            digestNames["1.3.36.3.2.1"] = "RIPEMD160";
            digestNames["1.3.36.3.2.3"] = "RIPEMD256";
            digestNames["1.2.840.113549.1.1.4"] = "MD5";
            digestNames["1.2.840.113549.1.1.2"] = "MD2";
            digestNames["1.2.840.113549.1.1.5"] = "SHA1";
            digestNames["1.2.840.113549.1.1.14"] = "SHA224";
            digestNames["1.2.840.113549.1.1.11"] = "SHA256";
            digestNames["1.2.840.113549.1.1.12"] = "SHA384";
            digestNames["1.2.840.113549.1.1.13"] = "SHA512";
            digestNames["1.2.840.113549.2.5"] = "MD5";
            digestNames["1.2.840.113549.2.2"] = "MD2";
            digestNames["1.2.840.10040.4.3"] = "SHA1";
            digestNames["2.16.840.1.101.3.4.3.1"] = "SHA224";
            digestNames["2.16.840.1.101.3.4.3.2"] = "SHA256";
            digestNames["2.16.840.1.101.3.4.3.3"] = "SHA384";
            digestNames["2.16.840.1.101.3.4.3.4"] = "SHA512";
            digestNames["1.3.36.3.3.1.3"] = "RIPEMD128";
            digestNames["1.3.36.3.3.1.2"] = "RIPEMD160";
            digestNames["1.3.36.3.3.1.4"] = "RIPEMD256";
            
            algorithmNames["1.2.840.113549.1.1.1"] = "RSA";
            algorithmNames["1.2.840.10040.4.1"] = "DSA";
            algorithmNames["1.2.840.113549.1.1.2"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.4"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.5"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.14"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.11"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.12"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.13"] = "RSA";
            algorithmNames["1.2.840.10040.4.3"] = "DSA";
            algorithmNames["2.16.840.1.101.3.4.3.1"] = "DSA";
            algorithmNames["2.16.840.1.101.3.4.3.2"] = "DSA";
            algorithmNames["1.3.36.3.3.1.3"] = "RSA";
            algorithmNames["1.3.36.3.3.1.2"] = "RSA";
            algorithmNames["1.3.36.3.3.1.4"] = "RSA";
            
            allowedDigests["MD5"] = "1.2.840.113549.2.5";
            allowedDigests["MD2"] = "1.2.840.113549.2.2";
            allowedDigests["SHA1"] = "1.3.14.3.2.26";
            allowedDigests["SHA224"] = "2.16.840.1.101.3.4.2.4";
            allowedDigests["SHA256"] = "2.16.840.1.101.3.4.2.1";
            allowedDigests["SHA384"] = "2.16.840.1.101.3.4.2.2";
            allowedDigests["SHA512"] = "2.16.840.1.101.3.4.2.3";
            allowedDigests["MD-5"] = "1.2.840.113549.2.5";
            allowedDigests["MD-2"] = "1.2.840.113549.2.2";
            allowedDigests["SHA-1"] = "1.3.14.3.2.26";
            allowedDigests["SHA-224"] = "2.16.840.1.101.3.4.2.4";
            allowedDigests["SHA-256"] = "2.16.840.1.101.3.4.2.1";
            allowedDigests["SHA-384"] = "2.16.840.1.101.3.4.2.2";
            allowedDigests["SHA-512"] = "2.16.840.1.101.3.4.2.3";
            allowedDigests["RIPEMD128"] = "1.3.36.3.2.2";
            allowedDigests["RIPEMD-128"] = "1.3.36.3.2.2";
            allowedDigests["RIPEMD160"] = "1.3.36.3.2.1";
            allowedDigests["RIPEMD-160"] = "1.3.36.3.2.1";
            allowedDigests["RIPEMD256"] = "1.3.36.3.2.3";
            allowedDigests["RIPEMD-256"] = "1.3.36.3.2.3";
        }

        /**
        * Gets the digest name for a certain id
        * @param oid    an id (for instance "1.2.840.113549.2.5")
        * @return   a digest name (for instance "MD5")
        * @since    2.1.6
        */
        public static String GetDigest(String oid)
        {
            String ret = (String)digestNames[oid];
            if (ret == null)
                return oid;
            else
                return ret;
        }

        /**
        * Gets the algorithm name for a certain id.
        * @param oid    an id (for instance "1.2.840.113549.1.1.1")
        * @return   an algorithm name (for instance "RSA")
        * @since    2.1.6
        */
        public static String GetAlgorithm(String oid)
        {
            String ret = (String)algorithmNames[oid];
            if (ret == null)
                return oid;
            else
                return ret;
        }

        /**
        * Generates a signature.
        * @param privKey the private key
        * @param certChain the certificate chain
        * @param crlList the certificate revocation list
        * @param hashAlgorithm the hash algorithm
        * @param provider the provider or <code>null</code> for the default provider
        * @param hasRSAdata <CODE>true</CODE> if the sub-filter is adbe.pkcs7.sha1
        * @throws SecurityException on error
        * @throws InvalidKeyException on error
        * @throws NoSuchProviderException on error
        * @throws NoSuchAlgorithmException on error
        */    
        public PKCS7(ICipherParameters privKey, X509Certificate[] certChain, object[] crlList,
                        String hashAlgorithm, bool hasRSAdata) {
            this.privKey = privKey;
            
            digestAlgorithm = (String)allowedDigests[hashAlgorithm.ToUpper(CultureInfo.InvariantCulture)];
            if (digestAlgorithm == null)
                throw new ArgumentException("Unknown Hash Algorithm "+hashAlgorithm);
            
            version = signerversion = 1;
            certs = new ArrayList();
            crls = new ArrayList();
            digestalgos = new Hashtable();
            digestalgos[digestAlgorithm] = null;
            
            //
            // Copy in the certificates and crls used to sign the private key.
            //
            signCert = certChain[0];
            for (int i = 0;i < certChain.Length;i++) {
                certs.Add(certChain[i]);
            }
            
//            if (crlList != null) {
//                for (int i = 0;i < crlList.length;i++) {
//                    crls.Add(crlList[i]);
//                }
//            }
            
            if (privKey != null) {
                //
                // Now we have private key, find out what the digestEncryptionAlgorithm is.
                //
                if (privKey is RsaKeyParameters)
                    digestEncryptionAlgorithm = ID_RSA;
                else if (privKey is DsaKeyParameters)
                    digestEncryptionAlgorithm = ID_DSA;
                else
                    throw new ArgumentException("Unknown Key Algorithm "+privKey.ToString());

            }
            if (hasRSAdata) {
                RSAdata = new byte[0];
                messageDigest = GetHashClass();
            }

            if (privKey != null) {
                sig = SignerUtilities.GetSigner(GetDigestAlgorithm());
                sig.Init(true, privKey);
            }
        }

        /**
        * Get the algorithm used to calculate the message digest
        * @return the algorithm used to calculate the message digest
        */
        public String GetDigestAlgorithm()
        {
            String dea = GetAlgorithm(digestEncryptionAlgorithm);
            if (dea == null)
                dea = digestEncryptionAlgorithm;

            return GetHashAlgorithm() + "with" + dea;
        }

        /**
       * Returns the algorithm.
       * @return the digest algorithm
       */
        public String GetHashAlgorithm()
        {
            return GetDigest(digestAlgorithm);
        }

        internal IDigest GetHashClass()
        {
            return DigestUtilities.GetDigest(GetHashAlgorithm());
        }

        /**
        * Update the digest with the specified bytes. This method is used both for signing and verifying
        * @param buf the data buffer
        * @param off the offset in the data buffer
        * @param len the data length
        * @throws SignatureException on error
        */
        public void Update(byte[] buf, int off, int len) 
		{
            if (RSAdata != null || digestAttr != null)
                messageDigest.BlockUpdate(buf, off, len);
            else
                sig.BlockUpdate(buf, off, len);
        }

        /**
        * Get the X.509 certificate actually used to sign the digest.
        * @return the X.509 certificate actually used to sign the digest
        */
        public X509Certificate SigningCertificate
        {
            get
            {
                return signCert;
            }
        }

        /**
        * Get the "issuer" from the TBSCertificate bytes that are passed in
        * @param enc a TBSCertificate in a byte array
        * @return a DERObject
        */
        private static Asn1Object GetIssuer(byte[] enc)
        {
            Asn1InputStream inp = new Asn1InputStream(new MemoryStream(enc));
            Asn1Sequence seq = (Asn1Sequence)inp.ReadObject();
            return (Asn1Object)seq[seq[0] is DerTaggedObject ? 3 : 2];
        }

        /**
        * Gets the bytes for the PKCS7SignedData object. Optionally the authenticatedAttributes
        * in the signerInfo can also be set, OR a time-stamp-authority client
        * may be provided.
        * @param secondDigest the digest in the authenticatedAttributes
        * @param signingTime the signing time in the authenticatedAttributes
        * @param tsaClient TSAClient - null or an optional time stamp authority client
        * @return byte[] the bytes for the PKCS7SignedData object
        * @since   2.1.6
        */
        public byte[] GetEncodedPKCS7(byte[] secondDigest, DateTime signingTime, /*ITSAClient tsaClient,*/ byte[] ocsp)
        {
            //if (externalDigest != null)
            //{
            //    digest = externalDigest;
            //    if (RSAdata != null)
            //        RSAdata = externalRSAdata;
            //}
            //else if (externalRSAdata != null && RSAdata != null)
            //{
            //    RSAdata = externalRSAdata;
            //    sig.BlockUpdate(RSAdata, 0, RSAdata.Length);
            //    digest = sig.GenerateSignature();
            //}
            //else
            {
                if (RSAdata != null)
                {
                    RSAdata = new byte[messageDigest.GetDigestSize()];
                    messageDigest.DoFinal(RSAdata, 0);
                    sig.BlockUpdate(RSAdata, 0, RSAdata.Length);
                }
                digest = sig.GenerateSignature();
            }

            // Create the set of Hash algorithms
            Asn1EncodableVector digestAlgorithms = new Asn1EncodableVector();
            foreach (string dal in digestalgos.Keys)
            {
                Asn1EncodableVector algos = new Asn1EncodableVector();
                algos.Add(new DerObjectIdentifier(dal));
                algos.Add(DerNull.Instance);
                digestAlgorithms.Add(new DerSequence(algos));
            }

            // Create the contentInfo.
            Asn1EncodableVector v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(ID_PKCS7_DATA));
            if (RSAdata != null)
                v.Add(new DerTaggedObject(0, new DerOctetString(RSAdata)));
            DerSequence contentinfo = new DerSequence(v);

            // Get all the certificates
            //
            v = new Asn1EncodableVector();
            foreach (X509Certificate xcert in certs)
            {
                Asn1InputStream tempstream = new Asn1InputStream(new MemoryStream(xcert.GetEncoded()));
                v.Add(tempstream.ReadObject());
            }

            DerSet dercertificates = new DerSet(v);

            // Create signerinfo structure.
            //
            Asn1EncodableVector signerinfo = new Asn1EncodableVector();

            // Add the signerInfo version
            //
            signerinfo.Add(new DerInteger(signerversion));

            v = new Asn1EncodableVector();
            v.Add(GetIssuer(signCert.GetTbsCertificate()));
            v.Add(new DerInteger(signCert.SerialNumber));
            signerinfo.Add(new DerSequence(v));

            // Add the digestAlgorithm
            v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(digestAlgorithm));
            v.Add(DerNull.Instance);
            signerinfo.Add(new DerSequence(v));

            //// add the authenticated attribute if present
            //if (secondDigest != null /*&& signingTime != null*/)
            //{
            //    signerinfo.Add(new DerTaggedObject(false, 0, GetAuthenticatedAttributeSet(secondDigest, signingTime, ocsp)));
            //}

            // Add the digestEncryptionAlgorithm
            v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(digestEncryptionAlgorithm));
            v.Add(DerNull.Instance);
            signerinfo.Add(new DerSequence(v));

            // Add the digest
            signerinfo.Add(new DerOctetString(digest));

            //// When requested, go get and add the timestamp. May throw an exception.
            //// Added by Martin Brunecky, 07/12/2007 folowing Aiken Sam, 2006-11-15
            //// Sam found Adobe expects time-stamped SHA1-1 of the encrypted digest
            //if (tsaClient != null)
            //{
            //    byte[] tsImprint = new System.Security.Cryptography.SHA1CryptoServiceProvider().ComputeHash(digest);
            //    byte[] tsToken = tsaClient.GetTimeStampToken(this, tsImprint);
            //    if (tsToken != null)
            //    {
            //        Asn1EncodableVector unauthAttributes = BuildUnauthenticatedAttributes(tsToken);
            //        if (unauthAttributes != null)
            //        {
            //            signerinfo.Add(new DerTaggedObject(false, 1, new DerSet(unauthAttributes)));
            //        }
            //    }
            //}

            // Finally build the body out of all the components above
            Asn1EncodableVector body = new Asn1EncodableVector();
            body.Add(new DerInteger(version));
            body.Add(new DerSet(digestAlgorithms));
            body.Add(contentinfo);
            body.Add(new DerTaggedObject(false, 0, dercertificates));

//                if (crls.Count > 0) {
//                    v = new Asn1EncodableVector();
//                    for (Iterator i = crls.Iterator();i.HasNext();) {
//                        Asn1InputStream t = new Asn1InputStream(new ByteArrayInputStream((((X509CRL)i.Next()).GetEncoded())));
//                        v.Add(t.ReadObject());
//                    }
//                    DERSet dercrls = new DERSet(v);
//                    body.Add(new DERTaggedObject(false, 1, dercrls));
//                }

            // Only allow one signerInfo
            body.Add(new DerSet(new DerSequence(signerinfo)));

            // Now we have the body, wrap it in it's PKCS7Signed shell
            // and return it
            //
            Asn1EncodableVector whole = new Asn1EncodableVector();
            whole.Add(new DerObjectIdentifier(ID_PKCS7_SIGNED_DATA));
            whole.Add(new DerTaggedObject(0, new DerSequence(body)));

            MemoryStream bOut = new MemoryStream();

            Asn1OutputStream dout = new Asn1OutputStream(bOut);
            dout.WriteObject(new DerSequence(whole));
            dout.Close();

            return bOut.ToArray();
        }

        /**
        * Get the "subject" from the TBSCertificate bytes that are passed in
        * @param enc A TBSCertificate in a byte array
        * @return a DERObject
        */
        private static Asn1Object GetSubject(byte[] enc)
        {
            Asn1InputStream inp = new Asn1InputStream(new MemoryStream(enc));
            Asn1Sequence seq = (Asn1Sequence)inp.ReadObject();
            return (Asn1Object)seq[seq[0] is DerTaggedObject ? 5 : 4];
        }

        /**
        * Get the subject fields from an X509 Certificate
        * @param cert an X509Certificate
        * @return an X509Name
        */
        public static X509Name GetSubjectFields(X509Certificate cert)
        {
            return new X509Name((Asn1Sequence)GetSubject(cert.GetTbsCertificate()));
        }

        public Hashtable values = new Hashtable();

        public String GetField(String name)
        {
            ArrayList vs = (ArrayList)values[name];
            return vs == null ? null : (String)vs[0];
        }
    }


    /**
    * a class that holds an X509 name
    */
	internal class X509Name
    {
        /**
        * country code - StringType(SIZE(2))
        */
        public static DerObjectIdentifier C = new DerObjectIdentifier("2.5.4.6");

        /**
        * organization - StringType(SIZE(1..64))
        */
        public static DerObjectIdentifier O = new DerObjectIdentifier("2.5.4.10");

        /**
        * organizational unit name - StringType(SIZE(1..64))
        */
        public static DerObjectIdentifier OU = new DerObjectIdentifier("2.5.4.11");

        /**
        * Title
        */
        public static DerObjectIdentifier T = new DerObjectIdentifier("2.5.4.12");

        /**
        * common name - StringType(SIZE(1..64))
        */
        public static DerObjectIdentifier CN = new DerObjectIdentifier("2.5.4.3");

        /**
        * device serial number name - StringType(SIZE(1..64))
        */
        public static DerObjectIdentifier SN = new DerObjectIdentifier("2.5.4.5");

        /**
        * locality name - StringType(SIZE(1..64))
        */
        public static DerObjectIdentifier L = new DerObjectIdentifier("2.5.4.7");

        /**
        * state, or province name - StringType(SIZE(1..64))
        */
        public static DerObjectIdentifier ST = new DerObjectIdentifier("2.5.4.8");

        /** Naming attribute of type X520name */
        public static DerObjectIdentifier SURNAME = new DerObjectIdentifier("2.5.4.4");
        /** Naming attribute of type X520name */
        public static DerObjectIdentifier GIVENNAME = new DerObjectIdentifier("2.5.4.42");
        /** Naming attribute of type X520name */
        public static DerObjectIdentifier INITIALS = new DerObjectIdentifier("2.5.4.43");
        /** Naming attribute of type X520name */
        public static DerObjectIdentifier GENERATION = new DerObjectIdentifier("2.5.4.44");
        /** Naming attribute of type X520name */
        public static DerObjectIdentifier UNIQUE_IDENTIFIER = new DerObjectIdentifier("2.5.4.45");

        /**
        * Email address (RSA PKCS#9 extension) - IA5String.
        * Note: if you're trying to be ultra orthodox, don't use this! It shouldn't be in here.
        */
        public static DerObjectIdentifier EmailAddress = new DerObjectIdentifier("1.2.840.113549.1.9.1");

        /**
        * email address in Verisign certificates
        */
        public static DerObjectIdentifier E = EmailAddress;

        /** object identifier */
        public static DerObjectIdentifier DC = new DerObjectIdentifier("0.9.2342.19200300.100.1.25");

        /** LDAP User id. */
        public static DerObjectIdentifier UID = new DerObjectIdentifier("0.9.2342.19200300.100.1.1");

        /** A Hashtable with default symbols */
        public static Hashtable DefaultSymbols = new Hashtable();

        static X509Name()
        {
            DefaultSymbols[C] = "C";
            DefaultSymbols[O] = "O";
            DefaultSymbols[T] = "T";
            DefaultSymbols[OU] = "OU";
            DefaultSymbols[CN] = "CN";
            DefaultSymbols[L] = "L";
            DefaultSymbols[ST] = "ST";
            DefaultSymbols[SN] = "SN";
            DefaultSymbols[EmailAddress] = "E";
            DefaultSymbols[DC] = "DC";
            DefaultSymbols[UID] = "UID";
            DefaultSymbols[SURNAME] = "SURNAME";
            DefaultSymbols[GIVENNAME] = "GIVENNAME";
            DefaultSymbols[INITIALS] = "INITIALS";
            DefaultSymbols[GENERATION] = "GENERATION";
        }
        /** A Hashtable with values */
        public Hashtable values = new Hashtable();

        /**
        * Constructs an X509 name
        * @param seq an Asn1 Sequence
        */
        public X509Name(Asn1Sequence seq)
        {
            IEnumerator e = seq.GetEnumerator();

            while (e.MoveNext())
            {
                Asn1Set sett = (Asn1Set)e.Current;

                for (int i = 0; i < sett.Count; i++)
                {
                    Asn1Sequence s = (Asn1Sequence)sett[i];
                    String id = (String)DefaultSymbols[s[0]];
                    if (id == null)
                        continue;
                    ArrayList vs = (ArrayList)values[id];
                    if (vs == null)
                    {
                        vs = new ArrayList();
                        values[id] = vs;
                    }
                    vs.Add(((DerStringBase)s[1]).GetString());
                }
            }
        }
        /**
        * Constructs an X509 name
        * @param dirName a directory name
        */
        public X509Name(String dirName)
        {
            X509NameTokenizer nTok = new X509NameTokenizer(dirName);

            while (nTok.HasMoreTokens())
            {
                String token = nTok.NextToken();
                int index = token.IndexOf('=');

                if (index == -1)
                {
                    throw new ArgumentException("badly formated directory string");
                }

                String id = token.Substring(0, index).ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                String value = token.Substring(index + 1);
                ArrayList vs = (ArrayList)values[id];
                if (vs == null)
                {
                    vs = new ArrayList();
                    values[id] = vs;
                }
                vs.Add(value);
            }

        }

        public String GetField(String name)
        {
            ArrayList vs = (ArrayList)values[name];
            return vs == null ? null : (String)vs[0];
        }

        /**
        * gets a field array from the values Hashmap
        * @param name
        * @return an ArrayList
        */
        public ArrayList GetFieldArray(String name)
        {
            ArrayList vs = (ArrayList)values[name];
            return vs == null ? null : vs;
        }

        /**
        * getter for values
        * @return a Hashtable with the fields of the X509 name
        */
        public Hashtable GetFields()
        {
            return values;
        }

        /**
        * @see java.lang.Object#toString()
        */
        public override String ToString()
        {
            return values.ToString();
        }
    }

    /**
    * class for breaking up an X500 Name into it's component tokens, ala
    * java.util.StringTokenizer. We need this class as some of the
    * lightweight Java environment don't support classes like
    * StringTokenizer.
    */
	internal class X509NameTokenizer
    {
        private String oid;
        private int index;
        private StringBuilder buf = new StringBuilder();

        public X509NameTokenizer(
        String oid)
        {
            this.oid = oid;
            this.index = -1;
        }

        public bool HasMoreTokens()
        {
            return (index != oid.Length);
        }

        public String NextToken()
        {
            if (index == oid.Length)
            {
                return null;
            }

            int end = index + 1;
            bool quoted = false;
            bool escaped = false;

            buf.Length = 0;

            while (end != oid.Length)
            {
                char c = oid[end];

                if (c == '"')
                {
                    if (!escaped)
                    {
                        quoted = !quoted;
                    }
                    else
                    {
                        buf.Append(c);
                    }
                    escaped = false;
                }
                else
                {
                    if (escaped || quoted)
                    {
                        buf.Append(c);
                        escaped = false;
                    }
                    else if (c == '\\')
                    {
                        escaped = true;
                    }
                    else if (c == ',')
                    {
                        break;
                    }
                    else
                    {
                        buf.Append(c);
                    }
                }
                end++;
            }

            index = end;
            return buf.ToString().Trim();
        }
    }

}
