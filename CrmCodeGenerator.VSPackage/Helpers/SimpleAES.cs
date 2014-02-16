using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    // http://stackoverflow.com/questions/165808/simple-2-way-encryption-for-c-sharp
    public class SimpleAES : IDisposable
    {
        private static byte[] key = { 132, 160, 120, 162, 053, 113, 166, 142, 164, 121, 113, 130, 102, 111, 161, 056, 043, 134, 171, 040, 105, 145, 136, 141, 104, 107, 153, 211, 241, 170, 115, 209 };
        private static byte[] vector = { 133, 147, 041, 111, 057, 3, 133, 171, 231, 134, 114, 112, 072, 32, 101, 110 };
        private ICryptoTransform encryptor, decryptor;
        private UTF8Encoding encoder;

        public SimpleAES()
        {
            RijndaelManaged rm = new RijndaelManaged();
            encryptor = rm.CreateEncryptor(key, vector);
            decryptor = rm.CreateDecryptor(key, vector);
            encoder = new UTF8Encoding();
        }

        public string Encrypt(string unencrypted)
        {
            return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
        }

        public string Decrypt(string encrypted)
        {
            return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }

        //public string EncryptToUrl(string unencrypted)
        //{
        //    return System.Web.HttpUtility.UrlEncode(Encrypt(unencrypted));
        //}

        //public string DecryptFromUrl(string encrypted)
        //{
        //    return Decrypt(System.Web.HttpUtility.UrlDecode(encrypted));
        //}

        public byte[] Encrypt(byte[] buffer)
        {
            return Transform(buffer, encryptor);
        }

        public byte[] Decrypt(byte[] buffer)
        {
            return Transform(buffer, decryptor);
        }

        protected byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            MemoryStream stream = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }
        public void Dispose()
        {
            encryptor.Dispose();
            decryptor.Dispose();
        }
    }
}
