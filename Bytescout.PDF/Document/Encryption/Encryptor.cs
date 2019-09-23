using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Bytescout.PDF
{
    internal enum CryptFilter { Identity, V2, AESV2, AESV3 }

    internal enum DataType { String, Stream }

    internal class Encryptor
    {
	    private static readonly byte[] _paddingString = 
	    {0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41,
		    0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
		    0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80,
		    0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A};
        
	    private byte[] _docID;
	    private bool _isDocIDEmpty;
	    private int _version = 0;
	    private int _length = 0;
	    private int _revision = 0;
	    private int _permission;
	    private byte[] _ownerHash;
	    private byte[] _userHash;
	    private bool _encryptMetadata = true;
	    private CryptFilter _stmF = CryptFilter.V2;
	    private CryptFilter _strF = CryptFilter.V2;

	    private string _userPassword = "";
	    private string _ownerPassword = "";

	    private byte[] _encryptionKey;
	    private byte[] _curObjectKey;
	    private int _curObject = -1;

	    //for AES-256 bit
	    private byte[] _oe;
	    private byte[] _ue;
	    private byte[] _perms;

	    private RC4 _rc4;
	    private AES _aes;

	    public string UserPassword
        {
            get { return _userPassword; }
            set { _userPassword = value; }
        }

        public string OwnerPassword
        {
            get { return _ownerPassword; }
            set { _ownerPassword = value; }
        }

        public int Permissions
        {
            get { return _permission; }
            set { _permission = value; }
        }

        public int Revision
        {
            get { return _revision; }
            set { _revision = value; }
        }

        public int KeyLength
        {
            get { return _length; }
            set { _length = value; }
        }

        public int Version
        {
            get { return _version; }
            set { _version = value; }
        }

        public bool EncryptMetadata
        {
            get { return _encryptMetadata; }
            set { _encryptMetadata = value; }
        }

	    public Encryptor()
	    {
	    }

	    public Encryptor(PDFDictionary dict, PDFArray idArray)
	    {
		    if (dict == null)
			    throw new PDFException();

		    readFilter(dict);
		    readAlgorithmVersion(dict);
		    readRevision(dict);
		    readKeyLength(dict);
		    readOwnerHash(dict);
		    readUserHash(dict);
		    readPermissions(dict);
		    if (_revision >= 4)
		    {
			    readEncryptMetadata(dict);
			    readCryptFilters(dict);
		    }
		    if (_revision == 5)
		    {
			    readUE(dict);
			    readOE(dict);
			    readPerms(dict);
		    }
                
		    readID(idArray);
	    }

	    public void SetDocumentID(byte[] id)
        {
            _docID = new byte[16];
            Array.Copy(id, _docID, _docID.Length);
            _isDocIDEmpty = false;
        }

        public byte[] GetDocumentID()
        {
            return _docID;
        }

        public EncryptionAlgorithm GetEncryptionLevel()
        {
            if (_version == 1 || _length == 40)
                return EncryptionAlgorithm.RC4_40bit;
            if (_version == 4 && (_stmF == CryptFilter.AESV2 || _strF == CryptFilter.AESV2))
                return EncryptionAlgorithm.AES_128bit;
            if (_version == 5)
                return EncryptionAlgorithm.AES_256bit;
            return EncryptionAlgorithm.RC4_128bit;
        }

        public byte[] GetUserHash()
        {
            return _userHash;
        }

        public byte[] GetOwnerHash()
        {
            return _ownerHash;
        }

        public byte[] GetOE()
        {
            return _oe;
        }

        public byte[] GetUE()
        {
            return _ue;
        }

        public byte[] GetPerms()
        {
            return _perms;
        }

        public bool AuthenticatePassword(string password)
        {
            if (!authenticateOwnerPassword(password))
            {
                if (authenticateUserPassword(password))
                {
                    _ownerPassword = "";
                    _userPassword = password;
                    return true;
                }
                return false;
            }
            
            _ownerPassword = password;
            if(_revision < 5)
                _userPassword = calculateUserPasswordFromOwnerPassword(password);
            return true;
        }

        public void Decrypt(byte[] inputData, int offset, int length, Stream output, DataType dataType)
        {
            switch (getFilter(dataType))
            {
                case CryptFilter.Identity:
                    output.Write(inputData, offset, length);
                    return;
                case CryptFilter.V2:
                    RC4 rc4 = getRC4(_curObjectKey, _curObjectKey.Length);
                    rc4.Crypt(inputData, offset, length, output);
                    return;
                case CryptFilter.AESV3:
                case CryptFilter.AESV2:
                    AES aes = getAES(_curObjectKey);
                    aes.Decrypt(inputData, offset, length, output);
                    return;
            }
        }

        public void Decrypt(Stream input, int length, Stream output, DataType dataType)
        {
            switch (getFilter(dataType))
            {
                case CryptFilter.Identity:
                    for (int i = 0; i < length; ++i)
                        output.WriteByte((byte)input.ReadByte());
                    return;
                case CryptFilter.V2:
                    RC4 rc4 = getRC4(_curObjectKey, _curObjectKey.Length);
                    rc4.Crypt(input, length, output);
                    return;
                case CryptFilter.AESV2:
                case CryptFilter.AESV3:
                    AES aes = getAES(_curObjectKey);
                    aes.Decrypt(input, length, output);
                    return;
            }
        }

        public void Encrypt(byte[] inputData, int offset, int length, Stream output, DataType dataType)
        {
            switch (getFilter(dataType))
            {
                case CryptFilter.Identity:
                    output.Write(inputData, offset, length);
                    return;
                case CryptFilter.V2:
                    RC4 rc4 = getRC4(_curObjectKey, _curObjectKey.Length);
                    rc4.Crypt(inputData, offset, length, output);
                    return;
                case CryptFilter.AESV3:
                case CryptFilter.AESV2:
                    AES aes = getAES(_curObjectKey);
                    aes.Encrypt(inputData, offset, length, output);
                    return;
            }
        }

        public void RecalculateEncryptionKey()
        {
            if (_revision < 5)
                _encryptionKey = computeEncryptionKey(_userPassword, _length);
            else
                _encryptionKey = computeEncryptionKey_a(_ownerPassword, _userPassword);
        }

        public void Reset()
        {
            if (_revision < 5)
            {
                _ownerHash = calculateOwnerHash(_ownerPassword, _userPassword, false);
                _userHash = calculateUserHash(_userPassword);
                _encryptionKey = computeEncryptionKey(_userPassword, _length);
                if (_revision == 4)
                {
                    _stmF = CryptFilter.AESV2;
                    _strF = CryptFilter.AESV2;
                }
            }
            else
            {
                _userHash = computeUserHash(_userPassword);
                _ownerHash = computeOwnerHash(_ownerPassword, _userHash);
                _encryptionKey = new byte[32];
                Random rnd = new Random();
                rnd.NextBytes(_encryptionKey);
                _ue = computeUE(_encryptionKey, _userPassword);
                _oe = computeOE(_encryptionKey, _ownerPassword, _userHash);
                _perms = computePerms();
                _stmF = CryptFilter.AESV3;
                _strF = CryptFilter.AESV3;
            }
        }

        // General encryption algorithm 3.1, step 1 - 3
        public void ResetObjectReference(int objNo, int genNo, DataType dataType)
        {
            if (_revision < 5)
            {
                if (_curObject == objNo)
                    return;
                _curObject = objNo;
                if (_curObjectKey == null)
                    _curObjectKey = new byte[Math.Min(_encryptionKey.Length + 5, 16)];

                int n = _encryptionKey.Length;
                byte[] step2Bytes = new byte[n + 5];
                Array.Copy(_encryptionKey, 0, step2Bytes, 0, n);

                step2Bytes[n] = (byte)(objNo & 0xff);
                step2Bytes[n + 1] = (byte)(objNo >> 8 & 0xff);
                step2Bytes[n + 2] = (byte)(objNo >> 16 & 0xff);
                step2Bytes[n + 3] = (byte)(genNo & 0xff);
                step2Bytes[n + 4] = (byte)(genNo >> 8 & 0xff);

                CryptFilter filter;
                if (dataType == DataType.Stream)
                    filter = _stmF;
                else
                    filter = _strF;

                if (filter == CryptFilter.AESV2)
                {
                    int len = step2Bytes.Length;
                    byte[] tmp = new byte[step2Bytes.Length + 4];
                    Array.Copy(step2Bytes, tmp, step2Bytes.Length);
                    tmp[len] = 0x73;
                    tmp[len + 1] = 0x41;
                    tmp[len + 2] = 0x6C;
                    tmp[len + 3] = 0x54;

                    step2Bytes = tmp;
                }

                MD5 md5 = MD5.Create();
                md5.TransformFinalBlock(step2Bytes, 0, step2Bytes.Length);
                byte[] step3Bytes = md5.Hash;

                Array.Copy(step3Bytes, 0, _curObjectKey, 0, _curObjectKey.Length);
            }
            else
            {
                _curObjectKey = _encryptionKey;
            }
        }

        private bool authenticateOwnerPassword(string ownerPassword)
        {
            if (_revision < 5)
                return authenticateOwnerPassword_3_7(ownerPassword);
            return authenticateOwnerPassword_3_12(ownerPassword);
        }

        private bool authenticateUserPassword(string userPassword)
        {
            if (_revision < 5)
                return authenticateUserPassword_3_6(userPassword);
            return authenticateUserPassword_3_11(userPassword);
        }

        private byte[] padPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return (byte[])_paddingString.Clone();

            byte[] paddedPassword = new byte[32];
            int passwordLength = Math.Min(password.Length, 32);
            byte[] bytePassword = System.Text.Encoding.Default.GetBytes(password);

            Array.Copy(bytePassword, paddedPassword, passwordLength);
            Array.Copy(_paddingString, 0, paddedPassword, passwordLength, 32 - passwordLength);

            return paddedPassword;
        }

        private byte[] truncatePassword(string password)
        {
            byte[] result = System.Text.Encoding.UTF8.GetBytes(password);
            if (result.Length < 128)
                return result;

            byte[] tmp = new byte[127];
            Array.Copy(result, tmp, tmp.Length);
            return tmp;
        }

        // Encryption key algorithm 3.2 for computing an encryption key given a password string.
        private byte[] computeEncryptionKey(string password, int keyLength)
        {
            // Step 1:  pad the password
            byte[] paddedPassword = padPassword(password);

            // Step 2: initialize the MD5 hash function
            byte[] tmp = new byte[32];
            MD5 md5 = MD5.Create();
            md5.TransformBlock(paddedPassword, 0, paddedPassword.Length, tmp, 0);

            // Step 3: Pass the value of the encryption dictionary's O entry
            md5.TransformBlock(_ownerHash, 0, _ownerHash.Length, tmp, 0);

            // Step 4: treat P as an unsigned 4-byte integer
            byte[] tmp_flg = new byte[4];
            tmp_flg[0] = (byte)_permission;
            tmp_flg[1] = (byte)(_permission >> 8);
            tmp_flg[2] = (byte)(_permission >> 16);
            tmp_flg[3] = (byte)(_permission >> 24);
            
            if (!_isDocIDEmpty || (_revision >= 4 && !_encryptMetadata))
                md5.TransformBlock(tmp_flg, 0, 4, tmp, 0);
            else
                md5.TransformFinalBlock(tmp_flg, 0, 4);

            // Step 5: Pass in the first element of the file's file identifies array
            if (!_isDocIDEmpty)
            {
                if (_revision >= 4 && !_encryptMetadata)
                    md5.TransformBlock(_docID, 0, _docID.Length, tmp, 0);
                else
                    md5.TransformFinalBlock(_docID, 0, _docID.Length);
            }

            // Step 6: If document metadata is not being encrypted, pass 4 bytes with
            // the value of 0xFFFFFFFF to the MD5 hash
            if (_revision >= 4 && !_encryptMetadata)
            {
                tmp_flg[0] = 0xFF;
                tmp_flg[1] = 0xFF;
                tmp_flg[2] = 0xFF;
                tmp_flg[3] = 0xFF;
                md5.TransformFinalBlock(tmp_flg, 0, tmp_flg.Length);
            }

            // Step 7: Finish Hash.
            paddedPassword = (byte[])md5.Hash.Clone();
            md5 = MD5.Create();

            // Step 8: Do the following 50 times: take the output from the previous
            // MD5 hash and pass it as ainput into a new MD5 hash;
            // only for R = 3
            if (_revision >= 3)
            {
                for (int i = 0; i < 50; i++)
                    paddedPassword = md5.ComputeHash(paddedPassword, 0, _length / 8);
            }
            
            // Step 9: Set the encryption key to the first n bytes of the output from
            // the MD5 hash
            byte[] output = null;
            int n = 5;
            if (_revision == 2)
                output = new byte[n];
            else if (_revision >= 3)
            {
                n = _length / 8;
                output = new byte[n];
            }
            Array.Copy(paddedPassword, 0, output, 0, n);
            return output;
        }

        // Encryption key algorithm 3.2a for computing an encryption key given a password string.
        private byte[] computeEncryptionKey_a(string ownerPassword, string userPassword)
        {
            if (authenticateOwnerPassword_3_12(ownerPassword))
            {
                byte[] pass = truncatePassword(ownerPassword);

                byte[] buf = new byte[pass.Length + 56];
                Array.Copy(pass, buf, pass.Length);
                Array.Copy(_ownerHash, 40, buf, pass.Length, 8);
                Array.Copy(_userHash, 0, buf, pass.Length + 8, _userHash.Length);

                SHA256 sha = SHA256.Create();
                sha.TransformFinalBlock(buf, 0, buf.Length);
                byte[] hash = sha.Hash;

                RijndaelManaged aes = new RijndaelManaged();
                aes.Key = hash;
                aes.IV = new byte[16];
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                
                ICryptoTransform transform = aes.CreateDecryptor();
                byte[] result = transform.TransformFinalBlock(_oe, 0, _oe.Length);
                return result;
            }
            else
            {
                byte[] pass = truncatePassword(userPassword);

                byte[] buf = new byte[pass.Length + 8];
                Array.Copy(pass, buf, pass.Length);
                Array.Copy(_userHash, 40, buf, pass.Length, 8);

                SHA256 sha = SHA256.Create();
                sha.TransformFinalBlock(buf, 0, buf.Length);
                byte[] hash = sha.Hash;

                RijndaelManaged aes = new RijndaelManaged();
                aes.Key = hash;
                aes.IV = new byte[16];
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                ICryptoTransform transform = aes.CreateDecryptor();
                byte[] result = transform.TransformFinalBlock(_ue, 0, _ue.Length);
                return result;
            }
        }

        // Computing Owner password value, Algorithm 3.3.
        private byte[] calculateOwnerHash(string ownerPassword, string userPassword, bool isAuthentication)
        {
            // Step 1:  padd the owner password, use the userPassword if empty.
            if ("".Equals(ownerPassword) && !"".Equals(userPassword))
                ownerPassword = userPassword;
            byte[] paddedOwnerPassword = padPassword(ownerPassword);

            // Step 2: Initialize the MD5 hash function and pass in step 2.
            MD5 md5 = MD5.Create();
            paddedOwnerPassword = md5.ComputeHash(paddedOwnerPassword);

            // Step 3: Do the following 50 times: take the output from the previous
            // MD5 hash and pass it as input into a new MD5 hash;
            // only for R = 3
            if (_revision >= 3)
            {
                for (int i = 0; i < 50; ++i)
                    paddedOwnerPassword = md5.ComputeHash(paddedOwnerPassword, 0, _length / 8);
            }

            // Step 4: Create an RC4 encryption key using the first n bytes of the
            // final MD5 hash, where n is always 5 for revision 2 and the value
            // of the encryption dictionary's Length entry for revision 3.
            int dataSize = 5;
            if (_revision >= 3)
                dataSize = _length / 8;

            byte[] encryptionKey = new byte[dataSize];
            Array.Copy(paddedOwnerPassword, encryptionKey, dataSize);

            // Key is needed by algorithm 3.7, Authenticating owner password
            if (isAuthentication)
                return encryptionKey;

            // Step 5: Pad or truncate the user password string
            byte[] paddedUserPassword = padPassword(userPassword);

            // Step 6: Encrypt the result of step 4, using the RC4 encryption
            // function with the encryption key obtained in step 4
            RC4 rc4 = new RC4(encryptionKey, dataSize);
            byte[] finalData = rc4.CryptBuffer(paddedUserPassword, 0, paddedUserPassword.Length);

            // Step 7: Do the following 19 times: Take the output from the previous
            // invocation of the RC4 function and pass it as input to a new
            // invocation of the function; use an encryption key generated by taking
            // each byte of the encryption key in step 4 and performing an XOR
            // operation between that byte and the single-byte value of the
            // iteration counter
            if (_revision >= 3)
            {
                byte[] indexedKey = new byte[encryptionKey.Length];
                for (int i = 1; i <= 19; i++)
                {
                    for (int j = 0; j < encryptionKey.Length; j++)
                        indexedKey[j] = (byte)(encryptionKey[j] ^ i);

                    rc4 = new RC4(indexedKey, indexedKey.Length);
                    finalData = rc4.CryptBuffer(finalData, 0, finalData.Length);
                }
            }
            return finalData;
        }

        // Computing the encryption dictionary’s U (user password)
        private byte[] calculateUserHash(String userPassword)
        {
            // Step 1: Create an encryption key based on the user password String,
            // as described in Algorithm 3.2
            byte[] encryptionKey = computeEncryptionKey(userPassword, _length);

            // Algorithm 3.4 steps, 2 - 3
            if (_revision == 2)
            {
                // Step 2: Encrypt the 32-byte padding string show in step 1, using
                // an RC4 encryption function with the encryption key from the
                // preceding step
                byte[] paddedUserPassword = (byte[])_paddingString.Clone();
                byte[] finalData = null;
                RC4 rc4 = new RC4(encryptionKey, encryptionKey.Length);
                finalData = rc4.CryptBuffer(paddedUserPassword, 0, paddedUserPassword.Length);

                // Step 3: return the result of step 2 as the value of the U entry
                return finalData;
            }
            // algorithm 3.5 steps, 2 - 6
            else
            {
                // Step 2: Initialize the MD5 hash function and pass the 32-byte
                // padding string shown in step 1 of Algorithm 3.2 as input to
                // this function
                byte[] tmp = new byte[32];
                byte[] paddedUserPassword = (byte[])_paddingString.Clone();
                MD5 md5 = MD5.Create();

                if (!_isDocIDEmpty)
                    md5.TransformBlock(paddedUserPassword, 0, paddedUserPassword.Length, tmp, 0);
                else
                    md5.TransformFinalBlock(paddedUserPassword, 0, paddedUserPassword.Length);

                // Step 3: Pass the first element of the files identify array to the
                // hash function and finish the hash.
                if (!_isDocIDEmpty)
                    md5.TransformFinalBlock(_docID, 0, _docID.Length);
                byte[] encryptData = (byte[])md5.Hash.Clone();

                // Step 4: Encrypt the 16 byte result of the hash, using an RC4
                // encryption function with the encryption key from step 1
                RC4 rc4 = new RC4(encryptionKey, encryptionKey.Length);
                encryptData = rc4.CryptBuffer(encryptData, 0, encryptData.Length);

                // Step 5: Do the following 19 times: Take the output from the previous
                // invocation of the RC4 function and pass it as input to a new
                // invocation of the function; use an encryption key generated by taking
                // each byte of the encryption key in step 4 and performing an XOR
                // operation between that byte and the single-byte value of the
                // iteration counter

                byte[] indexedKey = new byte[encryptionKey.Length];
                for (int i = 1; i <= 19; i++)
                {
                    for (int j = 0; j < encryptionKey.Length; j++)
                        indexedKey[j] = (byte)(encryptionKey[j] ^ (byte)i);
                    rc4 = new RC4(indexedKey, indexedKey.Length);
                    encryptData = rc4.CryptBuffer(encryptData, 0, encryptData.Length);
                }

                // Step 6: Append 16 bytes of arbitrary padding to the output from
                // the final invocation of the RC4 function and return the 32-byte
                // result as the value of the U entry.
                byte[] finalData = new byte[32];
                Array.Copy(encryptData, finalData, 16);
                Array.Copy(_paddingString, 0, finalData, 16, 16);
                return finalData;
            }
        }

        // Authenticating the user password,  algorithm 3.6
        private bool authenticateUserPassword_3_6(string userPassword)
        {
            // Step 1: Perform all but the last step of Algorithm 3.4(Revision 2) or
            // Algorithm 3.5 (Revision 3) using the supplied password string.
            byte[] userHash = calculateUserHash(userPassword);

            byte[] trunkUserHash;
            if (_revision == 2)
                trunkUserHash = new byte[32];
            else
                trunkUserHash = new byte[16];
            Array.Copy(userHash, trunkUserHash, trunkUserHash.Length);

            // Step 2: If the result of step 1 is equal o the value of the
            // encryption dictionary's U entry, the password supplied is the correct
            // user password.
            
            for (int i = 0; i < trunkUserHash.Length; i++)
            {
                if (trunkUserHash[i] != _userHash[i])
                    return false;
            }
            return true;
        }

        // Authenticating the owner password,  algorithm 3.7
        private bool authenticateOwnerPassword_3_7(string ownerPassword)
        {
            return authenticateUserPassword_3_6(calculateUserPasswordFromOwnerPassword(ownerPassword));
        }

        // algorithm 3.7, step 1-2
        private string calculateUserPasswordFromOwnerPassword(string ownerPassword)
        {
            // Step 1: Computer an encryption key from the supplied password string,
            // as described in steps 1 to 4 of algorithm 3.3.
            byte[] encryptionKey = calculateOwnerHash(ownerPassword, "", true);

            // Step 2: start decryption of O
            byte[] ownerHash = null;
            if (_revision == 2)
            {
                // Step 2 (R == 2):  decrypt the value of the encryption dictionary
                // O entry, using an RC4 encryption function with the encryption
                // key computed in step 1.
                RC4 rc4 = new RC4(encryptionKey, encryptionKey.Length);
                ownerHash = rc4.CryptBuffer(_ownerHash, 0, _ownerHash.Length);
            }
            else
            {
                // Step 2 (R == 3): Do the following 19 times: Take the output from the previous
                // invocation of the RC4 function and pass it as input to a new
                // invocation of the function; use an encryption key generated by taking
                // each byte of the encryption key in step 4 and performing an XOR
                // operation between that byte and the single-byte value of the
                // iteration counter
                byte[] indexedKey = new byte[encryptionKey.Length];
                ownerHash = (byte[])_ownerHash.Clone();

                for (int i = 19; i >= 0; i--)
                {
                    for (int j = 0; j < indexedKey.Length; j++)
                        indexedKey[j] = (byte)(encryptionKey[j] ^ (byte)i);

                    RC4 rc4 = new RC4(indexedKey, indexedKey.Length);
                    ownerHash = rc4.CryptBuffer(ownerHash, 0, ownerHash.Length);
                }
            }

            
            int index = findIndex(ownerHash);
            string localUserPassword = System.Text.Encoding.Default.GetString(ownerHash, 0, index);
            return localUserPassword;
        }

        // Algorithm 3.8 step 1
        private byte[] computeUserHash(string userPassword)
        {
            Random rnd = new Random();
            byte[] validationSalt = new byte[8];
            byte[] keySalt = new byte[8];
            rnd.NextBytes(validationSalt);
            rnd.NextBytes(keySalt);

            byte[] password = truncatePassword(userPassword);
            byte[] tmp = new byte[password.Length + 8];
            Array.Copy(password, tmp, password.Length);
            Array.Copy(validationSalt, 0, tmp, password.Length, validationSalt.Length);

            SHA256 sha = SHA256.Create();
            sha.TransformFinalBlock(tmp, 0, tmp.Length);
            byte[] hash = sha.Hash;

            byte[] userHash = new byte[48];
            Array.Copy(hash, userHash, hash.Length);
            Array.Copy(validationSalt, 0, userHash, hash.Length, validationSalt.Length);
            Array.Copy(keySalt, 0, userHash, hash.Length + validationSalt.Length, keySalt.Length);

            return userHash;
        }

        // Algorithm 3.8 step 2
        private byte[] computeUE(byte[] key, string userPassword)
        {
            byte[] password = truncatePassword(userPassword);
            byte[] tmp = new byte[password.Length + 8];
            Array.Copy(password, tmp, password.Length);
            Array.Copy(_userHash, 40, tmp, password.Length, 8);
            
            SHA256 sha = SHA256.Create();
            sha.TransformFinalBlock(tmp, 0, tmp.Length);
            byte[] aesKey = sha.Hash;

            RijndaelManaged aes = new RijndaelManaged();
            aes.Key = aesKey;
            aes.IV = new byte[16];
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            ICryptoTransform transform = aes.CreateEncryptor();
            return transform.TransformFinalBlock(key, 0, key.Length);
        }

        // Algorithm 3.9 step 1
        private byte[] computeOwnerHash(string ownerPassword, byte[] userHash)
        {
            Random rnd = new Random();
            byte[] validationSalt = new byte[8];
            byte[] keySalt = new byte[8];
            rnd.NextBytes(validationSalt);
            rnd.NextBytes(keySalt);

            byte[] password = truncatePassword(ownerPassword);
            byte[] tmp = new byte[password.Length + 8 + userHash.Length];
            Array.Copy(password, tmp, password.Length);
            Array.Copy(validationSalt, 0, tmp, password.Length, validationSalt.Length);
            Array.Copy(userHash, 0, tmp, password.Length + 8, userHash.Length);

            SHA256 sha = SHA256.Create();
            sha.TransformFinalBlock(tmp, 0, tmp.Length);
            byte[] hash = sha.Hash;

            byte[] ownerHash = new byte[48];
            Array.Copy(hash, ownerHash, hash.Length);
            Array.Copy(validationSalt, 0, ownerHash, hash.Length, validationSalt.Length);
            Array.Copy(keySalt, 0, ownerHash, hash.Length + validationSalt.Length, keySalt.Length);

            return ownerHash;
        }

        // Algorithm 3.9 step 2
        private byte[] computeOE(byte[] key, string ownerPassword, byte[] userHash)
        {
            byte[] password = truncatePassword(ownerPassword);
            byte[] tmp = new byte[password.Length + 8 + userHash.Length];
            Array.Copy(password, tmp, password.Length);
            Array.Copy(_ownerHash, 40, tmp, password.Length, 8);
            Array.Copy(userHash, 0, tmp, password.Length + 8, userHash.Length);

            SHA256 sha = SHA256.Create();
            sha.TransformFinalBlock(tmp, 0, tmp.Length);
            byte[] aesKey = sha.Hash;

            RijndaelManaged aes = new RijndaelManaged();
            aes.Key = aesKey;
            aes.IV = new byte[16];
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            ICryptoTransform transform = aes.CreateEncryptor();
            return transform.TransformFinalBlock(key, 0, key.Length);
        }

        // Algorithm 3.10 Computing the encryption dictionary’s Perms (permissions) value
        private byte[] computePerms()
        {
            byte[] result = new byte[16];

            uint value = (uint)_permission;
            int m = 256 * 256 * 256;
            for (int i = 3; i >= 0; --i)
            {
                result[i] = (byte)(value / m);
                value = (uint)(value - result[i] * m);
                m = m / 256;
            }

            result[4] = 1;
            result[5] = 1;
            result[6] = 1;
            result[7] = 1;

            if (_encryptMetadata)
                result[8] = (byte)'T';
            else
                result[8] = (byte)'F';
            result[9] = (byte)'a';
            result[10] = (byte)'d';
            result[11] = (byte)'b';

            Random rnd = new Random();
            for (int i = 12; i < 16; ++i)
                result[i] = (byte)rnd.Next(255);

            RijndaelManaged aes = new RijndaelManaged();
            aes.Key = _encryptionKey;
            aes.IV = new byte[16];
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            ICryptoTransform transform = aes.CreateEncryptor();
            return transform.TransformFinalBlock(result, 0, result.Length);
        }

        // Authenticating the User Password, algorithm 3.11
        private bool authenticateUserPassword_3_11(string userPassword)
        {
            byte[] password = truncatePassword(userPassword);

            byte[] buf = new byte[password.Length + 8];
            Array.Copy(password, buf, password.Length);
            Array.Copy(_userHash, 32, buf, password.Length, 8);

            SHA256 sha = SHA256.Create();
            sha.TransformFinalBlock(buf, 0, buf.Length);
            byte[] hash = sha.Hash;
            for (int i = 0; i < hash.Length; ++i)
            {
                if (hash[i] != _userHash[i])
                    return false;
            }

            return true;
        }

        // Authenticating the Owner Password, algorithm 3.12
        private bool authenticateOwnerPassword_3_12(string ownerPassword)
        {
            byte[] password = truncatePassword(ownerPassword);

            byte[] buf = new byte[password.Length + 56];
            Array.Copy(password, buf, password.Length);
            Array.Copy(_ownerHash, 32, buf, password.Length, 8);
            Array.Copy(_userHash, 0, buf, password.Length + 8, _userHash.Length);

            SHA256 sha = SHA256.Create();
            sha.TransformFinalBlock(buf, 0, buf.Length);
            byte[] hash = sha.Hash;
            for (int i = 0; i < hash.Length; ++i)
            {
                if (hash[i] != _ownerHash[i])
                    return false;
            }

            return true;
        }

        private int findIndex(byte[] hash)
        {
            for (int i = 32; i > 0; --i)
            {
                byte[] val1 = new byte[i];
                Array.Copy(hash, 32 - i, val1, 0, val1.Length);
                byte[] val2 = new byte[i];
                Array.Copy(_paddingString, 0, val2, 0, val2.Length);
                if (equal(val1, val2))
                    return 32 - i;
            }
            return 0;
        }

        private bool equal(byte[] val1, byte[] val2)
        {
            if (val1.Length != val2.Length)
                return false;
            for (int i = 0; i < val1.Length; ++i)
            {
                if (val1[i] != val2[i])
                    return false;
            }

            return true;
        }

        private void readFilter(PDFDictionary dictionary)
        {
            PDFName filter = dictionary["Filter"] as PDFName;
            if (filter == null)
                throw new PDFException();

            if(filter.GetValue() != "Standard")
                throw new PDFUnsupportEncryptorException();
        }

        private void readAlgorithmVersion(PDFDictionary dictionary)
        {
            PDFNumber version = dictionary["V"] as PDFNumber;
            if (version != null)
            {
                int value = (int)version.GetValue();
                if (value >= 0 && value <= 5)
                    _version = value;
                else
                    throw new PDFUnsupportEncryptorException();
                return;
            }
            _revision = 0;
        }

        private void readKeyLength(PDFDictionary dictionary)
        {
            PDFNumber lenght = dictionary["Length"] as PDFNumber;
            if (lenght != null)
            {
                int value = (int)lenght.GetValue();
                if ((value >= 40 && value <= 128 && value % 8 == 0) || (value == 256 && _revision == 5))
                    _length = value;
                else
                    throw new InvalidDocumentException();
                return;
            }
            _length = 40;
        }

        private void readRevision(PDFDictionary dictionary)
        {
            PDFNumber revision = dictionary["R"] as PDFNumber;
            if (revision != null)
            {
                int value = (int)revision.GetValue();
                if (value >= 2 && value <= 5)
                    _revision = value;

                if (_revision > 5)
                    throw new PDFUnsupportEncryptorException();
                return;
            }

            throw new PDFException();
        }

        private void readOwnerHash(PDFDictionary dictionary)
        {
            _ownerHash = getPasswordHash("O", dictionary);
        }

        private void readUserHash(PDFDictionary dictionary)
        {
            _userHash = getPasswordHash("U", dictionary);
        }

        private byte[] getPasswordHash(string key, PDFDictionary dictionary)
        {
            PDFString hash = dictionary[key] as PDFString;
            if (hash == null)
                throw new PDFException();

            byte[] result = hash.GetBytes();
            if (_revision < 5)
            {
                if (result.Length != 32)
                    throw new PDFException();
            }
            else
            {
                if(result.Length != 48)
                    throw new PDFException();
            }
            return result;
        }

        private void readPermissions(PDFDictionary dictionary)
        {
            PDFNumber perms = dictionary["P"] as PDFNumber;
            if (perms == null)
                throw new PDFException();
            _permission = (int)perms.GetValue();
        }

        private void readEncryptMetadata(PDFDictionary dictionary)
        {
            PDFBoolean encryptMetedata = dictionary["EncryptMetadata"] as PDFBoolean;
            if (encryptMetedata == null)
                _encryptMetadata = true;
            else
                _encryptMetadata = encryptMetedata.GetValue();
        }

        private void readCryptFilters(PDFDictionary dictionary)
        {
            PDFName stmf = dictionary["StmF"] as PDFName;
            PDFName strf = dictionary["StrF"] as PDFName;

            if (stmf == null)
                _stmF = CryptFilter.Identity;
            else
                readCFM(dictionary["CF"] as PDFDictionary, stmf.GetValue(), out _stmF);

            if (strf == null)
                _strF = CryptFilter.Identity;
            else
                readCFM(dictionary["CF"] as PDFDictionary, strf.GetValue(), out _strF);
        }

        private void readCFM(PDFDictionary CF, string key, out CryptFilter filter)
        {
            filter = CryptFilter.Identity;
            if (CF == null)
                return;

            PDFDictionary StdCF = CF[key] as PDFDictionary;
            if (StdCF != null)
            {
                PDFName cfm = StdCF["CFM"] as PDFName;
                if (cfm == null)
                    return;

                switch (cfm.GetValue())
                {
                    case "V2":
                        filter = CryptFilter.V2;
                        break;
                    case "AESV2":
                        filter = CryptFilter.AESV2;
                        break;
                    case "AESV3":
                        filter = CryptFilter.AESV3;
                        break;
                    default:
                        filter = CryptFilter.Identity;
                        break;
                }
            }
        }

        private void readID(PDFArray arr)
        {
            if (arr == null)
            {
                _docID = new byte[16];
                _isDocIDEmpty = true;
                return;
            }

            PDFString stringItem = arr[0] as PDFString;
            if (stringItem == null)
            {
                _docID = new byte[16];
                _isDocIDEmpty = true;
                return;
            }

            byte[] id = stringItem.GetBytes();
            _docID = id;
            _isDocIDEmpty = false;
        }

        private RC4 getRC4(byte[] key, int keyLength)
        {
            if (_rc4 == null)
            {
                _rc4 = new RC4(key, keyLength);
                return _rc4;
            }

            _rc4.SetKey(key);
            return _rc4;
        }

        private AES getAES(byte[] key)
        {
            if (_aes == null)
            {
                _aes = new AES(key);
                return _aes;
            }

            _aes.SetKey(key);
            return _aes;
        }

        private CryptFilter getFilter(DataType dataType)
        {
            if (dataType == DataType.Stream)
                return _stmF;
            return _strF;
        }

        private void readOE(PDFDictionary dict)
        {
            PDFString oe = dict["OE"] as PDFString;
            if (oe == null || oe.GetBytes().Length != 32)
                throw new PDFException();
            _oe = oe.GetBytes();
        }

        private void readUE(PDFDictionary dict)
        {
            PDFString ue = dict["UE"] as PDFString;
            if (ue == null || ue.GetBytes().Length != 32)
                throw new PDFException();
            _ue = ue.GetBytes();
        }

        private void readPerms(PDFDictionary dict)
        {
            PDFString perms = dict["Perms"] as PDFString;
            if (perms == null || perms.GetBytes().Length != 16)
                throw new PDFException();
            _perms = perms.GetBytes();
        }
    }
}
