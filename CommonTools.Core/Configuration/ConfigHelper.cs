using System.Text.Json;
using System.Text.Json.Nodes;

namespace CommonTools.Core.Configuration;

/// <summary>
/// 配置文件帮助类
/// </summary>
public static class ConfigHelper
{
    #region 配置
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static readonly string DefaultConfigDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
    #endregion

    #region 基础操作
    /// <summary>
    /// 确保配置目录存在
    /// </summary>
    public static void EnsureConfigDirectory(string? directory = null)
    {
        var configDir = directory ?? DefaultConfigDirectory;
        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }
    }

    /// <summary>
    /// 获取配置文件完整路径
    /// </summary>
    public static string GetConfigPath(string fileName, string? directory = null)
    {
        var configDir = directory ?? DefaultConfigDirectory;
        return Path.Combine(configDir, fileName);
    }

    /// <summary>
    /// 检查配置文件是否存在
    /// </summary>
    public static bool ConfigExists(string fileName, string? directory = null)
    {
        var filePath = GetConfigPath(fileName, directory);
        return File.Exists(filePath);
    }
    #endregion

    #region JSON配置
    /// <summary>
    /// 保存JSON配置
    /// </summary>
    public static async Task SaveJsonConfigAsync<T>(string fileName, T config, string? directory = null)
    {
        var filePath = GetConfigPath(fileName, directory);
        EnsureConfigDirectory(directory);
        var json = JsonSerializer.Serialize(config, DefaultOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// 加载JSON配置
    /// </summary>
    public static async Task<T?> LoadJsonConfigAsync<T>(string fileName, string? directory = null)
    {
        var filePath = GetConfigPath(fileName, directory);
        if (!File.Exists(filePath)) return default;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// 更新JSON配置的特定字段
    /// </summary>
    public static async Task UpdateJsonConfigAsync(string fileName, string jsonPath, JsonNode value, string? directory = null)
    {
        var filePath = GetConfigPath(fileName, directory);
        if (!File.Exists(filePath)) return;

        var jsonObject = JsonNode.Parse(await File.ReadAllTextAsync(filePath));
        if (jsonObject == null) return;

        var pathParts = jsonPath.Split('.');
        var current = jsonObject;

        for (int i = 0; i < pathParts.Length - 1; i++)
        {
            current = current[pathParts[i]];
            if (current == null) return;
        }

        current[pathParts[^1]] = value;
        await File.WriteAllTextAsync(filePath, jsonObject.ToJsonString(DefaultOptions));
    }
    #endregion

    #region 环境配置
    /// <summary>
    /// 加载不同环境的配置
    /// </summary>
    public static async Task<T?> LoadEnvironmentConfigAsync<T>(string baseFileName, string environment, string? directory = null)
    {
        var envFileName = Path.GetFileNameWithoutExtension(baseFileName) + 
                         $".{environment}" + 
                         Path.GetExtension(baseFileName);

        // 先加载基础配置
        var baseConfig = await LoadJsonConfigAsync<T>(baseFileName, directory);
        
        // 如果有环境特定配置，则覆盖基础配置
        var envConfig = await LoadJsonConfigAsync<T>(envFileName, directory);
        
        if (baseConfig == null) return envConfig;
        if (envConfig == null) return baseConfig;

        // 合并配置
        var baseJson = JsonSerializer.SerializeToNode(baseConfig);
        var envJson = JsonSerializer.SerializeToNode(envConfig);
        MergeJsonNodes(baseJson, envJson);

        return baseJson.Deserialize<T>(DefaultOptions);
    }
    #endregion

    #region 配置监控
    private static readonly Dictionary<string, FileSystemWatcher> ConfigWatchers = new();

    /// <summary>
    /// 监控配置文件变化
    /// </summary>
    public static void WatchConfig(string fileName, Action<string> onChange, string? directory = null)
    {
        var filePath = GetConfigPath(fileName, directory);
        var configDir = Path.GetDirectoryName(filePath) ?? DefaultConfigDirectory;
        var configFile = Path.GetFileName(filePath);

        if (ConfigWatchers.ContainsKey(filePath))
        {
            ConfigWatchers[filePath].Dispose();
        }

        var watcher = new FileSystemWatcher(configDir, configFile)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime
        };

        watcher.Changed += (sender, e) => onChange(e.FullPath);
        watcher.EnableRaisingEvents = true;
        ConfigWatchers[filePath] = watcher;
    }

    /// <summary>
    /// 停止监控配置文件
    /// </summary>
    public static void StopWatching(string fileName, string? directory = null)
    {
        var filePath = GetConfigPath(fileName, directory);
        if (ConfigWatchers.TryGetValue(filePath, out var watcher))
        {
            watcher.Dispose();
            ConfigWatchers.Remove(filePath);
        }
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 合并JSON节点
    /// </summary>
    private static void MergeJsonNodes(JsonNode? target, JsonNode? source)
    {
        if (target == null || source == null) return;

        if (target is JsonObject targetObj && source is JsonObject sourceObj)
        {
            foreach (var property in sourceObj)
            {
                if (!targetObj.ContainsKey(property.Key))
                {
                    targetObj[property.Key] = property.Value?.DeepClone();
                }
                else if (property.Value != null)
                {
                    MergeJsonNodes(targetObj[property.Key], property.Value);
                }
            }
        }
        else if (source != null)
        {
            target = source.DeepClone();
        }
    }

    /// <summary>
    /// 备份配置文件
    /// </summary>
    public static async Task BackupConfigAsync(string fileName, string? directory = null)
    {
        var filePath = GetConfigPath(fileName, directory);
        if (!File.Exists(filePath)) return;

        var backupDir = Path.Combine(Path.GetDirectoryName(filePath) ?? DefaultConfigDirectory, "Backups");
        Directory.CreateDirectory(backupDir);

        var timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupPath = Path.Combine(backupDir, 
            $"{Path.GetFileNameWithoutExtension(fileName)}.{timestamp}{Path.GetExtension(fileName)}");

        File.Copy(filePath, backupPath);
    }

    /// <summary>
    /// 还原配置文件
    /// </summary>
    public static async Task RestoreConfigAsync(string fileName, string backupFileName, string? directory = null)
    {
        var configPath = GetConfigPath(fileName, directory);
        var backupDir = Path.Combine(Path.GetDirectoryName(configPath) ?? DefaultConfigDirectory, "Backups");
        var backupPath = Path.Combine(backupDir, backupFileName);

        if (!File.Exists(backupPath)) 
            throw new FileNotFoundException("备份文件不存在", backupPath);

        // 先备份当前配置
        await BackupConfigAsync(fileName, directory);

        // 还原备份
        File.Copy(backupPath, configPath, true);
    }
    #endregion
} 