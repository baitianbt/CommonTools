using System.Text;
using System.Text.RegularExpressions;

namespace CommonTools.Core.Configuration;

/// <summary>
/// INI文件处理帮助类
/// </summary>
public static class IniHelper
{
    #region 数据结构
    /// <summary>
    /// INI配置项
    /// </summary>
    public class IniSection : Dictionary<string, string> { }

    /// <summary>
    /// INI配置集合
    /// </summary>
    public class IniData : Dictionary<string, IniSection> { }
    #endregion

    #region 读取操作
    /// <summary>
    /// 读取INI文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public static IniData ReadIniFile(string filePath)
    {
        var iniData = new IniData();
        if (!File.Exists(filePath)) return iniData;

        IniSection? currentSection = null;
        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                continue;

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                var sectionName = trimmedLine[1..^1].Trim();
                currentSection = new IniSection();
                iniData[sectionName] = currentSection;
            }
            else if (currentSection != null)
            {
                var keyValue = trimmedLine.Split(new[] { '=' }, 2);
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();
                    currentSection[key] = value;
                }
            }
        }

        return iniData;
    }

    /// <summary>
    /// 获取INI文件中的值
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="section">节名</param>
    /// <param name="key">键名</param>
    /// <param name="defaultValue">默认值</param>
    public static string GetValue(string filePath, string section, string key, string defaultValue = "")
    {
        var iniData = ReadIniFile(filePath);
        return iniData.TryGetValue(section, out var sectionData) && sectionData.TryGetValue(key, out var value)
            ? value
            : defaultValue;
    }

    /// <summary>
    /// 获取指定节的所有键值对
    /// </summary>
    public static Dictionary<string, string> GetSection(string filePath, string section)
    {
        var iniData = ReadIniFile(filePath);
        return iniData.TryGetValue(section, out var sectionData)
            ? new Dictionary<string, string>(sectionData)
            : new Dictionary<string, string>();
    }

    /// <summary>
    /// 获取所有节名
    /// </summary>
    public static List<string> GetSections(string filePath)
    {
        var iniData = ReadIniFile(filePath);
        return new List<string>(iniData.Keys);
    }
    #endregion

    #region 写入操作
    /// <summary>
    /// 写入INI文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="iniData">INI数据</param>
    public static void WriteIniFile(string filePath, IniData iniData)
    {
        var content = new StringBuilder();
        foreach (var section in iniData)
        {
            content.AppendLine($"[{section.Key}]");
            foreach (var keyValue in section.Value)
            {
                content.AppendLine($"{keyValue.Key}={keyValue.Value}");
            }
            content.AppendLine();
        }

        File.WriteAllText(filePath, content.ToString());
    }

    /// <summary>
    /// 设置INI文件中的值
    /// </summary>
    public static void SetValue(string filePath, string section, string key, string value)
    {
        var iniData = File.Exists(filePath) ? ReadIniFile(filePath) : new IniData();

        if (!iniData.TryGetValue(section, out var sectionData))
        {
            sectionData = new IniSection();
            iniData[section] = sectionData;
        }

        sectionData[key] = value;
        WriteIniFile(filePath, iniData);
    }

    /// <summary>
    /// 删除指定的键
    /// </summary>
    public static bool DeleteKey(string filePath, string section, string key)
    {
        var iniData = ReadIniFile(filePath);
        if (iniData.TryGetValue(section, out var sectionData))
        {
            if (sectionData.Remove(key))
            {
                WriteIniFile(filePath, iniData);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 删除指定的节
    /// </summary>
    public static bool DeleteSection(string filePath, string section)
    {
        var iniData = ReadIniFile(filePath);
        if (iniData.Remove(section))
        {
            WriteIniFile(filePath, iniData);
            return true;
        }
        return false;
    }
    #endregion

    #region 类型转换
    /// <summary>
    /// 获取整数值
    /// </summary>
    public static int GetInt(string filePath, string section, string key, int defaultValue = 0)
    {
        var value = GetValue(filePath, section, key);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// 获取布尔值
    /// </summary>
    public static bool GetBool(string filePath, string section, string key, bool defaultValue = false)
    {
        var value = GetValue(filePath, section, key);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// 获取浮点数值
    /// </summary>
    public static double GetDouble(string filePath, string section, string key, double defaultValue = 0.0)
    {
        var value = GetValue(filePath, section, key);
        return double.TryParse(value, out var result) ? result : defaultValue;
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 合并两个INI文件
    /// </summary>
    public static void MergeIniFiles(string sourceFile, string targetFile)
    {
        var sourceData = ReadIniFile(sourceFile);
        var targetData = File.Exists(targetFile) ? ReadIniFile(targetFile) : new IniData();

        foreach (var section in sourceData)
        {
            if (!targetData.TryGetValue(section.Key, out var targetSection))
            {
                targetSection = new IniSection();
                targetData[section.Key] = targetSection;
            }

            foreach (var keyValue in section.Value)
            {
                targetSection[keyValue.Key] = keyValue.Value;
            }
        }

        WriteIniFile(targetFile, targetData);
    }

    /// <summary>
    /// 验证INI文件格式是否正确
    /// </summary>
    public static bool ValidateIniFormat(string filePath)
    {
        if (!File.Exists(filePath)) return false;

        try
        {
            var lines = File.ReadAllLines(filePath);
            var sectionPattern = new Regex(@"^\[[\w\-\s]+\]$");
            var keyValuePattern = new Regex(@"^[\w\-\s]+=[^=]*$");
            var hasSection = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                    continue;

                if (trimmedLine.StartsWith("["))
                {
                    if (!sectionPattern.IsMatch(trimmedLine))
                        return false;
                    hasSection = true;
                }
                else if (trimmedLine.Contains('='))
                {
                    if (!hasSection || !keyValuePattern.IsMatch(trimmedLine))
                        return false;
                }
            }

            return hasSection;
        }
        catch
        {
            return false;
        }
    }
    #endregion
} 