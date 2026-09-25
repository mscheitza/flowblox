using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.Components.Modifier
{
    public enum CryptographicHashAlgorithm
    {
        [Display(Name = "CryptographicHashAlgorithm_SHA256", ResourceType = typeof(FlowBloxTexts))]
        SHA256,
        [Display(Name = "CryptographicHashAlgorithm_SHA384", ResourceType = typeof(FlowBloxTexts))]
        SHA384,
        [Display(Name = "CryptographicHashAlgorithm_SHA512", ResourceType = typeof(FlowBloxTexts))]
        SHA512,
        [Display(Name = "CryptographicHashAlgorithm_SHA1", ResourceType = typeof(FlowBloxTexts))]
        SHA1,
        [Display(Name = "CryptographicHashAlgorithm_MD5", ResourceType = typeof(FlowBloxTexts))]
        MD5
    }

    public enum CryptographicOutputEncoding
    {
        [Display(Name = "CryptographicOutputEncoding_HexLowerCase", ResourceType = typeof(FlowBloxTexts))]
        HexLowerCase,
        [Display(Name = "CryptographicOutputEncoding_HexUpperCase", ResourceType = typeof(FlowBloxTexts))]
        HexUpperCase,
        [Display(Name = "CryptographicOutputEncoding_Base64", ResourceType = typeof(FlowBloxTexts))]
        Base64
    }

    public enum CryptographicKeyEncoding
    {
        [Display(Name = "CryptographicKeyEncoding_Text", ResourceType = typeof(FlowBloxTexts))]
        Text,
        [Display(Name = "CryptographicKeyEncoding_Base64", ResourceType = typeof(FlowBloxTexts))]
        Base64,
        [Display(Name = "CryptographicKeyEncoding_Hex", ResourceType = typeof(FlowBloxTexts))]
        Hex
    }

    internal static class CryptographicModifierSupport
    {
        public static HashAlgorithm CreateHash(CryptographicHashAlgorithm algorithm) => algorithm switch
        {
            CryptographicHashAlgorithm.SHA256 => SHA256.Create(),
            CryptographicHashAlgorithm.SHA384 => SHA384.Create(),
            CryptographicHashAlgorithm.SHA512 => SHA512.Create(),
            CryptographicHashAlgorithm.SHA1 => SHA1.Create(),
            CryptographicHashAlgorithm.MD5 => MD5.Create(),
            _ => throw new NotSupportedException($"Unsupported hash algorithm '{algorithm}'.")
        };

        public static HMAC CreateHmac(CryptographicHashAlgorithm algorithm, byte[] key) => algorithm switch
        {
            CryptographicHashAlgorithm.SHA256 => new HMACSHA256(key),
            CryptographicHashAlgorithm.SHA384 => new HMACSHA384(key),
            CryptographicHashAlgorithm.SHA512 => new HMACSHA512(key),
            CryptographicHashAlgorithm.SHA1 => new HMACSHA1(key),
            CryptographicHashAlgorithm.MD5 => new HMACMD5(key),
            _ => throw new NotSupportedException($"Unsupported HMAC algorithm '{algorithm}'.")
        };

        public static byte[] DecodeKey(string value, CryptographicKeyEncoding encoding) => encoding switch
        {
            CryptographicKeyEncoding.Text => Encoding.UTF8.GetBytes(value ?? string.Empty),
            CryptographicKeyEncoding.Base64 => Convert.FromBase64String(value ?? string.Empty),
            CryptographicKeyEncoding.Hex => Convert.FromHexString(value ?? string.Empty),
            _ => throw new NotSupportedException($"Unsupported key encoding '{encoding}'.")
        };

        public static string Encode(byte[] value, CryptographicOutputEncoding encoding) => encoding switch
        {
            CryptographicOutputEncoding.HexLowerCase => Convert.ToHexString(value).ToLowerInvariant(),
            CryptographicOutputEncoding.HexUpperCase => Convert.ToHexString(value),
            CryptographicOutputEncoding.Base64 => Convert.ToBase64String(value),
            _ => throw new NotSupportedException($"Unsupported output encoding '{encoding}'.")
        };
    }
}
