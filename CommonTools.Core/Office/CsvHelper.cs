using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace CommonTools.Core.Office;

/// <summary>
/// CSV文件处理帮助类
/// </summary>
public static class CsvFileHelper
{
    #region 配置
    /// <summary>
    /// 默认CSV配置
    /// </summary>
    private static readonly CsvConfiguration DefaultConfig = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        Delimiter = ",",
        Encoding = System.Text.Encoding.UTF8
    };
    #endregion

    #region 读取操作
    /// <summary>
    /// 读取CSV文件
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="config">CSV配置</param>
    /// <returns>数据列表</returns>
    public static List<T> ReadCsv<T>(string filePath, CsvConfiguration? config = null)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config ?? DefaultConfig);
        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// 读取CSV文件为字符串数组
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="hasHeader">是否包含表头</param>
    /// <returns>数据列表</returns>
    public static List<string[]> ReadCsvAsArray(string filePath, bool hasHeader = true)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeader,
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        
        var records = new List<string[]>();
        while (csv.Read())
        {
            var record = new string[csv.Parser.Count];
            for (int i = 0; i < csv.Parser.Count; i++)
            {
                record[i] = csv.GetField(i) ?? string.Empty;
            }
            records.Add(record);
        }
        
        return records;
    }

    /// <summary>
    /// 读取CSV文件的指定列
    /// </summary>
    public static List<string> ReadColumn(string filePath, int columnIndex, bool hasHeader = true)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeader,
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        
        var values = new List<string>();
        while (csv.Read())
        {
            if (columnIndex < csv.Parser.Count)
            {
                values.Add(csv.GetField(columnIndex) ?? string.Empty);
            }
        }
        
        return values;
    }
    #endregion

    #region 写入操作
    /// <summary>
    /// 写入CSV文件
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">数据列表</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="config">CSV配置</param>
    public static void WriteCsv<T>(IEnumerable<T> data, string filePath, CsvConfiguration? config = null)
    {
        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, config ?? DefaultConfig);
        csv.WriteRecords(data);
    }

    /// <summary>
    /// 追加数据到CSV文件
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">数据列表</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="config">CSV配置</param>
    public static void AppendToCsv<T>(IEnumerable<T> data, string filePath, CsvConfiguration? config = null)
    {
        var configuration = config ?? DefaultConfig;
        var fileExists = File.Exists(filePath);
        
        using var stream = File.Open(filePath, FileMode.Append);
        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, configuration);

        if (fileExists)
        {
            configuration.HasHeaderRecord = false;
        }

        csv.WriteRecords(data);
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 合并CSV文件
    /// </summary>
    public static void MergeCsvFiles(string[] sourceFiles, string targetFile, bool includeHeader = true)
    {
        var isFirst = true;
        foreach (var sourceFile in sourceFiles)
        {
            var records = ReadCsvAsArray(sourceFile, includeHeader);
            if (!isFirst && includeHeader && records.Any())
            {
                records.RemoveAt(0); // 移除后续文件的表头
            }
            
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = isFirst && includeHeader
            };
            
            using var writer = new StreamWriter(targetFile, append: !isFirst);
            using var csv = new CsvWriter(writer, config);
            foreach (var record in records)
            {
                foreach (var field in record)
                {
                    csv.WriteField(field);
                }
                csv.NextRecord();
            }
            
            isFirst = false;
        }
    }

    /// <summary>
    /// 验证CSV文件格式
    /// </summary>
    public static bool ValidateCsvFormat(string filePath, int expectedColumns = -1)
    {
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, DefaultConfig);
            
            var isFirstRow = true;
            var columnCount = -1;
            
            while (csv.Read())
            {
                if (isFirstRow)
                {
                    columnCount = csv.Parser.Count;
                    isFirstRow = false;
                    
                    if (expectedColumns > 0 && columnCount != expectedColumns)
                    {
                        return false;
                    }
                }
                else if (csv.Parser.Count != columnCount)
                {
                    return false;
                }
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}