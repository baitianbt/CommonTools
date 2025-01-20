using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonTools.Core.Json;

/// <summary>
/// JSON帮助类
/// </summary>
public static class JsonHelper
{
    #region 配置
    /// <summary>
    /// 默认JSON序列化选项
    /// </summary>
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    #endregion

    #region 序列化
    /// <summary>
    /// 将对象序列化为JSON字符串
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <param name="options">序列化选项</param>
    public static string Serialize<T>(T obj, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options ?? DefaultOptions);
    }

    /// <summary>
    /// 将对象序列化为格式化的JSON字符串
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    public static string SerializePretty<T>(T obj)
    {
        var options = new JsonSerializerOptions(DefaultOptions) { WriteIndented = true };
        return Serialize(obj, options);
    }

    /// <summary>
    /// 将对象序列化为压缩的JSON字符串
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    public static string SerializeCompressed<T>(T obj)
    {
        var options = new JsonSerializerOptions(DefaultOptions) { WriteIndented = false };
        return Serialize(obj, options);
    }
    #endregion

    #region 反序列化
    /// <summary>
    /// 将JSON字符串反序列化为对象
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <param name="options">反序列化选项</param>
    public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 尝试将JSON字符串反序列化为对象
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <param name="result">反序列化结果</param>
    public static bool TryDeserialize<T>(string json, out T? result)
    {
        try
        {
            result = Deserialize<T>(json);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
    #endregion

    #region 文件操作
    /// <summary>
    /// 将对象序列化为JSON并保存到文件
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="options">序列化选项</param>
    public static async Task SaveToFileAsync<T>(T obj, string filePath, JsonSerializerOptions? options = null)
    {
        var json = Serialize(obj, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// 从JSON文件中读取并反序列化为对象
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="options">反序列化选项</param>
    public static async Task<T?> LoadFromFileAsync<T>(string filePath, JsonSerializerOptions? options = null)
    {
        if (!File.Exists(filePath)) return default;
        var json = await File.ReadAllTextAsync(filePath);
        return Deserialize<T>(json, options);
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 验证JSON字符串是否有效
    /// </summary>
    public static bool IsValidJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 深度克隆对象（通过JSON序列化实现）
    /// </summary>
    public static T? DeepClone<T>(T obj)
    {
        if (obj == null) return default;
        var json = Serialize(obj);
        return Deserialize<T>(json);
    }

    /// <summary>
    /// 将对象转换为其他类型（通过JSON序列化实现）
    /// </summary>
    public static TTarget? ConvertTo<TSource, TTarget>(TSource source)
    {
        if (source == null) return default;
        var json = Serialize(source);
        return Deserialize<TTarget>(json);
    }
    #endregion
} 