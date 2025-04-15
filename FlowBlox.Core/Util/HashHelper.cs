using System;
using System.Security.Cryptography;

namespace FlowBlox.Core.Util
{
    public static class HashHelper
    {
        public static string ComputeSHA256Hash(byte[] content)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(content);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
