using System.Collections.Concurrent;

namespace CommonTools.Core.Cache;

/// <summary>
/// 缓存帮助类
/// </summary>
public static class CacheHelper
{
    private static readonly ConcurrentDictionary<string, CacheItem> Cache = new();

    private class CacheItem
    {
        public object? Value { get; set; }
        public System.DateTime? ExpirationTime { get; set; }
    }

    /// <summary>
    /// 设置缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="expirationMinutes">过期时间（分钟）</param>
    public static void Set(string key, object value, int expirationMinutes = 30)
    {
        var item = new CacheItem
        {
            Value = value,
            ExpirationTime = System.DateTime.Now.AddMinutes(expirationMinutes)
        };
        Cache.AddOrUpdate(key, item, (_, _) => item);
    }

    /// <summary>
    /// 获取缓存
    /// </summary>
    public static T? Get<T>(string key)
    {
        if (!Cache.TryGetValue(key, out var item)) return default;
        if (item.ExpirationTime < System.DateTime.Now)
        {
            Cache.TryRemove(key, out _);
            return default;
        }
        return (T?)item.Value;
    }

    /// <summary>
    /// 移除缓存
    /// </summary>
    public static void Remove(string key)
    {
        Cache.TryRemove(key, out _);
    }

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    public static void Clear()
    {
        Cache.Clear();
    }
} 