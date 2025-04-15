using System;
using System.Security.Cryptography;
using System.Text;

namespace FlowBlox.Core.Util
{
    internal class Encryption
    {
        private static readonly string AesIV256 = @"lC7AXp35Cc780#07";
        private static readonly string AesKey256 = @"ld#kC7tZAUX3bpPü#35C#>c7!8z#07D";

        public static string Encrypt(string text)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(AesIV256);
            aes.Key = Encoding.UTF8.GetBytes(AesKey256);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Convert string to byte array
            byte[] src = Encoding.Unicode.GetBytes(text);

            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                // Convert byte array to Base64 strings
                return Convert.ToBase64String(dest);
            }
        }

        public static string Decrypt(string text)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(AesIV256);
            aes.Key = Encoding.UTF8.GetBytes(AesKey256);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] src = Convert.FromBase64String(text);

            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                return Encoding.Unicode.GetString(dest);
            }
        }
    }
}
