using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DePeuter.Shared.Advanced
{
    public static class Encryption
    {
        [Obsolete("Use RijndaelEncrypt instead.")]
        public static byte[] AesEncrypt(byte[] data, string key)
        {
            using(var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using(var ms = new MemoryStream())
                {
                    using(var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    return ms.ToArray();
                }
            }
        }
        [Obsolete("Use RijndaelDecrypt instead.")]
        public static byte[] AesDecrypt(byte[] data, string key)
        {
            using(var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using(var ms = new MemoryStream())
                {
                    using(var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    return ms.ToArray();
                }
            }
        }

        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;
        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string RijndaelEncrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using(var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using(var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using(var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using(var memoryStream = new MemoryStream())
                        {
                            using(var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string RijndaelDecrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using(var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using(var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using(var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using(var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using(var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }
        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using(var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;

//namespace DePeuter.Shared.Advanced
//{
//    public static class Encryption
//    {
//        private static readonly byte[] IV = { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76, 0x76, 0x65, 0x64 };

//        private static byte[] GetKey(string key, int keySize)
//        {
//            var maxBytes = keySize/8;
//            var data = Encoding.ASCII.GetBytes(key);
//            var result = new byte[maxBytes];
//            data.CopyTo(result, 0);
//            return result;
//        }

//        private static AesCryptoServiceProvider NewAesCryptoServiceProvider()
//        {
//            return new AesCryptoServiceProvider()
//            {
//                BlockSize = 128,
//                KeySize = 256,
//                IV = IV,
//                Padding = PaddingMode.PKCS7,
//                Mode = CipherMode.CBC
//            };
//        }
//        public static byte[] AesEncrypt(byte[] data, string key)
//        {
//            //using(var aesProvider = NewAesCryptoServiceProvider())
//            //{
//            //    aesProvider.Key = GetKey(key, aesProvider.KeySize);

//            //    var cryptoTransform = aesProvider.CreateEncryptor(aesProvider.Key, aesProvider.IV);
//            //    return cryptoTransform.TransformFinalBlock(data, 0, data.Length);
//            //}

//            using(var aes = Aes.Create())
//            {
//                var pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
//                aes.Key = pdb.GetBytes(32);
//                aes.IV = pdb.GetBytes(16);
//                aes.Padding = PaddingMode.Zeros;
//                using(var ms = new MemoryStream())
//                {
//                    using(var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
//                    {
//                        cs.Write(data, 0, data.Length);
//                        cs.FlushFinalBlock();
//                        return ms.ToArray();
//                    }
//                }
//            }
//        }

//        public static byte[] AesDecrypt(byte[] data, string key)
//        {
//            //using(var aesProvider = NewAesCryptoServiceProvider())
//            //{
//            //    aesProvider.Key = GetKey(key, aesProvider.KeySize);

//            //    var cryptoTransform = aesProvider.CreateDecryptor(aesProvider.Key, aesProvider.IV);
//            //    return cryptoTransform.TransformFinalBlock(data, 0, data.Length);

//            //}

//            using(var aes = Aes.Create())
//            {
//                var pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
//                aes.Key = pdb.GetBytes(32);
//                aes.IV = pdb.GetBytes(16);
//                aes.Padding = PaddingMode.Zeros;
//                using(var ms = new MemoryStream())
//                {
//                    using(var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
//                    {
//                        cs.Write(data, 0, data.Length);
//                        cs.FlushFinalBlock();
//                        return ms.ToArray();
//                    }
//                }
//            }
//        }
//    }
//}
