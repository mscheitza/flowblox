using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using FlowBlox.Core.Util;

namespace FlowBlox.Core.Authentication
{
    public class FlowBloxSecureStorageManager
    {
        private static readonly string BaseKey = "FlowBlox.ProtectedData";

        public static string SetProtectedData(string key, string decryptedDataString)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            // Convert the JSON string to a byte array
            byte[] dataBytes = Encoding.UTF8.GetBytes(decryptedDataString);

            // Combine base key with provided key for entropy
            string entropyKey = $"{BaseKey}.{key}";

            // Encrypt the data
            byte[] encryptedData = ProtectedData.Protect(
                dataBytes,
                Encoding.UTF8.GetBytes(entropyKey),
                DataProtectionScope.CurrentUser
            );

            // Convert the encrypted byte array to a Base64 string
            string encryptedDataString = Convert.ToBase64String(encryptedData);

            // Return the encrypted data
            return encryptedDataString;
        }

        public static string GetProtectedData(string key, string encryptedDataString)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            // Combine base key with provided key for entropy
            string entropyKey = $"{BaseKey}.{key}";

            if (string.IsNullOrEmpty(encryptedDataString))
            {
                return default;
            }

            // Convert the Base64 string back to a byte array
            byte[] encryptedData = Convert.FromBase64String(encryptedDataString);

            // Decrypt the byte array
            byte[] dataBytes = ProtectedData.Unprotect(
                encryptedData,
                Encoding.UTF8.GetBytes(entropyKey),
                DataProtectionScope.CurrentUser
            );

            // Convert the byte array back to a JSON string
            string decryptedData = Encoding.UTF8.GetString(dataBytes);

            // Return decrypted data
            return decryptedData;
        }
    }
}
