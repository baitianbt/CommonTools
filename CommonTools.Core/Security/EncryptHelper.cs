using System.Security.Cryptography;
using System.Text;

namespace CommonTools.Core.Security;

/// <summary>
/// 加密解密帮助类
/// </summary>
public static class EncryptHelper
{
    #region Hash计算
    /// <summary>
    /// 计算MD5值
    /// </summary>
    public static string ComputeMD5(string input)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    /// <summary>
    /// 计算SHA256值
    /// </summary>
    public static string ComputeSHA256(string input)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    /// <summary>
    /// 计算文件的MD5值
    /// </summary>
    public static async Task<string> ComputeFileMD5Async(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = await md5.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
    #endregion

    #region AES加密
    /// <summary>
    /// AES加密
    /// </summary>
    public static string AesEncrypt(string input, string key)
    {
        using var aes = Aes.Create();
        aes.Key = GetValidKey(key, aes.KeySize / 8);
        aes.IV = new byte[16];  // 使用全零IV，实际应用中建议使用随机IV

        using var encryptor = aes.CreateEncryptor();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// AES解密
    /// </summary>
    public static string AesDecrypt(string encryptedText, string key)
    {
        using var aes = Aes.Create();
        aes.Key = GetValidKey(key, aes.KeySize / 8);
        aes.IV = new byte[16];  // 使用全零IV，需要与加密时相同

        using var decryptor = aes.CreateDecryptor();
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
    #endregion

    #region RSA加密
    /// <summary>
    /// 生成RSA密钥对
    /// </summary>
    public static (string publicKey, string privateKey) GenerateRSAKeys()
    {
        using var rsa = RSA.Create();
        return (
            Convert.ToBase64String(rsa.ExportRSAPublicKey()),
            Convert.ToBase64String(rsa.ExportRSAPrivateKey())
        );
    }

    /// <summary>
    /// RSA加密
    /// </summary>
    public static string RsaEncrypt(string input, string publicKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
        
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var encryptedBytes = rsa.Encrypt(inputBytes, RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// RSA解密
    /// </summary>
    public static string RsaDecrypt(string encryptedText, string privateKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
        
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 获取有效的密钥（如果密钥长度不够，进行填充；如果过长，进行截断）
    /// </summary>
    private static byte[] GetValidKey(string key, int requiredLength)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        if (keyBytes.Length == requiredLength)
            return keyBytes;

        var validKey = new byte[requiredLength];
        if (keyBytes.Length > requiredLength)
        {
            Array.Copy(keyBytes, validKey, requiredLength);
        }
        else
        {
            Array.Copy(keyBytes, validKey, keyBytes.Length);
            // 填充剩余部分
            for (int i = keyBytes.Length; i < requiredLength; i++)
            {
                validKey[i] = 0;
            }
        }
        return validKey;
    }

    /// <summary>
    /// 生成随机密钥
    /// </summary>
    public static string GenerateRandomKey(int length = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
    #endregion
} 