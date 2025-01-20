using System.IO.Compression;

namespace CommonTools.Core.IO;

/// <summary>
/// ZIP压缩文件帮助类
/// </summary>
public static class ZipHelper
{
    #region 压缩操作
    /// <summary>
    /// 创建ZIP压缩文件
    /// </summary>
    /// <param name="sourceDirectory">源目录</param>
    /// <param name="zipFilePath">ZIP文件路径</param>
    /// <param name="includeBaseDirectory">是否包含基目录</param>
    public static void CreateZip(string sourceDirectory, string zipFilePath, bool includeBaseDirectory = false)
    {
        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }

        ZipFile.CreateFromDirectory(
            sourceDirectory,
            zipFilePath,
            CompressionLevel.Optimal,
            includeBaseDirectory);
    }

    /// <summary>
    /// 添加文件到ZIP
    /// </summary>
    public static void AddToZip(string zipFilePath, string fileToAdd, string entryName)
    {
        using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update);
        archive.CreateEntryFromFile(fileToAdd, entryName);
    }

    /// <summary>
    /// 添加多个文件到ZIP
    /// </summary>
    public static void AddFilesToZip(string zipFilePath, Dictionary<string, string> files)
    {
        using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update);
        foreach (var file in files)
        {
            archive.CreateEntryFromFile(file.Key, file.Value);
        }
    }
    #endregion

    #region 解压操作
    /// <summary>
    /// 解压ZIP文件
    /// </summary>
    /// <param name="zipFilePath">ZIP文件路径</param>
    /// <param name="extractPath">解压目录</param>
    public static void ExtractZip(string zipFilePath, string extractPath)
    {
        if (!File.Exists(zipFilePath))
        {
            throw new FileNotFoundException("ZIP文件不存在", zipFilePath);
        }

        if (!Directory.Exists(extractPath))
        {
            Directory.CreateDirectory(extractPath);
        }

        ZipFile.ExtractToDirectory(zipFilePath, extractPath, true);
    }

    /// <summary>
    /// 从ZIP中提取单个文件
    /// </summary>
    public static void ExtractFileFromZip(string zipFilePath, string fileName, string extractPath)
    {
        using var archive = ZipFile.OpenRead(zipFilePath);
        var entry = archive.GetEntry(fileName);
        if (entry == null)
        {
            throw new FileNotFoundException($"在ZIP文件中未找到 {fileName}");
        }
        
        entry.ExtractToFile(Path.Combine(extractPath, fileName), true);
    }

    /// <summary>
    /// 从ZIP中提取指定的文件
    /// </summary>
    public static void ExtractFilesFromZip(string zipFilePath, string extractPath, string[] fileNames)
    {
        using var archive = ZipFile.OpenRead(zipFilePath);
        foreach (var fileName in fileNames)
        {
            var entry = archive.GetEntry(fileName);
            if (entry != null)
            {
                entry.ExtractToFile(Path.Combine(extractPath, fileName), true);
            }
        }
    }
    #endregion

    #region 信息查询
    /// <summary>
    /// 获取ZIP文件内容列表
    /// </summary>
    public static List<string> GetZipContents(string zipFilePath)
    {
        using var archive = ZipFile.OpenRead(zipFilePath);
        return archive.Entries.Select(entry => entry.FullName).ToList();
    }

    /// <summary>
    /// 检查ZIP文件是否包含指定文件
    /// </summary>
    public static bool ContainsFile(string zipFilePath, string fileName)
    {
        using var archive = ZipFile.OpenRead(zipFilePath);
        return archive.GetEntry(fileName) != null;
    }

    /// <summary>
    /// 获取ZIP文件中的文件大小
    /// </summary>
    public static long GetCompressedFileSize(string zipFilePath, string fileName)
    {
        using var archive = ZipFile.OpenRead(zipFilePath);
        var entry = archive.GetEntry(fileName);
        return entry?.CompressedLength ?? 0;
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 合并多个ZIP文件
    /// </summary>
    public static void MergeZipFiles(string[] sourceFiles, string targetFile)
    {
        if (File.Exists(targetFile))
        {
            File.Delete(targetFile);
        }

        using var targetArchive = ZipFile.Open(targetFile, ZipArchiveMode.Create);
        foreach (var sourceFile in sourceFiles)
        {
            using var sourceArchive = ZipFile.OpenRead(sourceFile);
            foreach (var entry in sourceArchive.Entries)
            {
                using var sourceStream = entry.Open();
                var targetEntry = targetArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                using var targetStream = targetEntry.Open();
                sourceStream.CopyTo(targetStream);
            }
        }
    }

    /// <summary>
    /// 验证ZIP文件完整性
    /// </summary>
    public static bool ValidateZipFile(string zipFilePath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            foreach (var entry in archive.Entries)
            {
                using var stream = entry.Open();
                using var reader = new BinaryReader(stream);
                while (reader.ReadBytes(4096).Length > 0) { }
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