using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonTools.Core.Extensions;

/// <summary>
/// 字符串扩展方法类
/// </summary>
public static class StringExtensions
{
    #region 基础判断
    /// <summary>
    /// 判断字符串是否为空
    /// </summary>
    public static bool IsNullOrEmpty(this string? str) => string.IsNullOrEmpty(str);

    /// <summary>
    /// 判断字符串是否为空或空白
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? str) => string.IsNullOrWhiteSpace(str);
    #endregion

    #region 字符串操作
    /// <summary>
    /// 安全的截取字符串
    /// </summary>
    public static string SafeSubstring(this string? str, int startIndex, int length)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        if (startIndex < 0) startIndex = 0;
        if (length < 0) length = 0;
        if (startIndex >= str!.Length) return string.Empty;
        if (startIndex + length > str.Length) length = str.Length - startIndex;
        return str.Substring(startIndex, length);
    }

    /// <summary>
    /// 反转字符串
    /// </summary>
    public static string Reverse(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        return new string(str.Reverse().ToArray());
    }

    /// <summary>
    /// 截取字符串并添加省略号
    /// </summary>
    public static string Truncate(this string str, int maxLength, string suffix = "...")
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        if (str.Length <= maxLength) return str;
        return str[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// 移除HTML标签
    /// </summary>
    public static string RemoveHtmlTags(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        return Regex.Replace(str, "<[^>]*>", string.Empty);
    }

    /// <summary>
    /// 移除字符串中的特殊字符
    /// </summary>
    public static string RemoveSpecialCharacters(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        return Regex.Replace(str, @"[^0-9a-zA-Z\u4e00-\u9fa5]+", string.Empty);
    }
    #endregion

    #region 命名转换
    /// <summary>
    /// 转换为驼峰命名
    /// </summary>
    public static string ToCamelCase(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        if (str.Length == 1) return str.ToLower();
        return char.ToLowerInvariant(str[0]) + str[1..];
    }

    /// <summary>
    /// 转换为帕斯卡命名
    /// </summary>
    public static string ToPascalCase(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        if (str.Length == 1) return str.ToUpper();
        return char.ToUpperInvariant(str[0]) + str[1..];
    }

    /// <summary>
    /// 转换为下划线命名
    /// </summary>
    public static string ToSnakeCase(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        return Regex.Replace(str, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }

    /// <summary>
    /// 转换为短横线命名
    /// </summary>
    public static string ToKebabCase(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        return Regex.Replace(str, @"([a-z0-9])([A-Z])", "$1-$2").ToLower();
    }
    #endregion

    #region 编码转换
    /// <summary>
    /// 获取字符串的MD5值
    /// </summary>
    public static string ToMD5(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(str);
        var hashBytes = md5.ComputeHash(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    /// <summary>
    /// 转换为Base64字符串
    /// </summary>
    public static string ToBase64(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(str);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 从Base64字符串转换
    /// </summary>
    public static string FromBase64(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        try
        {
            var bytes = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }
    #endregion

    #region 格式化
    /// <summary>
    /// 格式化文件大小
    /// </summary>
    public static string FormatFileSize(this long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
    #endregion

    #region 验证
    /// <summary>
    /// 判断字符串是否是有效的Email地址
    /// </summary>
    public static bool IsValidEmail(this string str)
    {
        if (str.IsNullOrEmpty()) return false;
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return regex.IsMatch(str);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 判断字符串是否是有效的URL
    /// </summary>
    public static bool IsValidUrl(this string str)
    {
        if (str.IsNullOrEmpty()) return false;
        return Uri.TryCreate(str, UriKind.Absolute, out _);
    }
    #endregion

    #region 提取内容
    /// <summary>
    /// 获取字符串中的数字
    /// </summary>
    public static string GetNumbers(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        return new string(str.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// 获取字符串中的字母
    /// </summary>
    public static string GetLetters(this string str)
    {
        if (str.IsNullOrEmpty()) return string.Empty;
        return new string(str.Where(char.IsLetter).ToArray());
    }
    #endregion

    #region 类型转换
    /// <summary>
    /// 将字符串转换为指定类型
    /// </summary>
    public static T? ConvertTo<T>(this string str)
    {
        if (str.IsNullOrEmpty()) return default;
        try
        {
            
            var type = typeof(T);
            if (type == typeof(string)) return (T)(object)str;
            if (type == typeof(int)) return (T)(object)int.Parse(str);
            if (type == typeof(long)) return (T)(object)long.Parse(str);
            if (type == typeof(double)) return (T)(object)double.Parse(str);
            if (type == typeof(decimal)) return (T)(object)decimal.Parse(str);
            if (type == typeof(bool)) return (T)(object)bool.Parse(str);
            if (type == typeof(System.DateTime)) return (T)(object)System.DateTime.Parse(str);
            if (type == typeof(Guid)) return (T)(object)Guid.Parse(str);
            return default;
        }
        catch
        {
            return default;
        }
    }
    #endregion
} 