using System.Text;

namespace CommonTools.Core.IO;

/// <summary>
/// 文件操作帮助类
/// </summary>
public static class FileHelper
{
    #region 基础操作
    /// <summary>
    /// 安全地创建目录
    /// </summary>
    public static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// 安全地删除文件
    /// </summary>
    public static void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// 复制文件，如果目标存在则覆盖
    /// </summary>
    public static void CopyFile(string sourcePath, string targetPath, bool overwrite = true)
    {
        var targetDir = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(targetDir))
        {
            CreateDirectoryIfNotExists(targetDir);
        }
        File.Copy(sourcePath, targetPath, overwrite);
    }

    /// <summary>
    /// 移动文件，如果目标存在则覆盖
    /// </summary>
    public static void MoveFile(string sourcePath, string targetPath, bool overwrite = true)
    {
        if (overwrite && File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }
        var targetDir = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(targetDir))
        {
            CreateDirectoryIfNotExists(targetDir);
        }
        File.Move(sourcePath, targetPath);
    }
    #endregion

    #region 目录操作
    /// <summary>
    /// 复制目录
    /// </summary>
    /// <param name="sourceDir">源目录</param>
    /// <param name="targetDir">目标目录</param>
    /// <param name="recursive">是否递归复制子目录</param>
    public static void CopyDirectory(string sourceDir, string targetDir, bool recursive = true)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"源目录不存在: {sourceDir}");
        }

        CreateDirectoryIfNotExists(targetDir);
        
        // 复制文件
        foreach (var file in dir.GetFiles())
        {
            var targetPath = Path.Combine(targetDir, file.Name);
            file.CopyTo(targetPath, true);
        }

        // 递归复制子目录
        if (recursive)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                var newTargetDir = Path.Combine(targetDir, subDir.Name);
                CopyDirectory(subDir.FullName, newTargetDir, true);
            }
        }
    }

    /// <summary>
    /// 获取目录大小
    /// </summary>
    public static long GetDirectorySize(string path)
    {
        var dir = new DirectoryInfo(path);
        return GetDirectorySize(dir);
    }

    private static long GetDirectorySize(DirectoryInfo dir)
    {
        long size = 0;
        
        // 获取文件大小
        var files = dir.GetFiles();
        foreach (var file in files)
        {
            size += file.Length;
        }

        // 递归获取子目录大小
        var subDirs = dir.GetDirectories();
        foreach (var subDir in subDirs)
        {
            size += GetDirectorySize(subDir);
        }

        return size;
    }

    /// <summary>
    /// 清空目录（但保留目录本身）
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="recursive">是否递归删除子目录</param>
    public static void ClearDirectory(string path, bool recursive = true)
    {
        var dir = new DirectoryInfo(path);
        if (!dir.Exists) return;

        foreach (var file in dir.GetFiles())
        {
            file.Delete();
        }

        if (recursive)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                subDir.Delete(true);
            }
        }
    }
    #endregion

    #region 文件信息
    /// <summary>
    /// 获取文件大小的友好显示
    /// </summary>
    public static string GetFileSizeDisplay(long bytes)
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

    /// <summary>
    /// 获取文件MD5值
    /// </summary>
    public static string GetFileMD5(string filePath)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// 获取文件扩展名（不含点号）
    /// </summary>
    public static string GetFileExtension(string filePath)
    {
        return Path.GetExtension(filePath).TrimStart('.');
    }

    /// <summary>
    /// 获取文件的MIME类型
    /// </summary>
    public static string GetMimeType(string filePath)
    {
        var extension = GetFileExtension(filePath).ToLower();
        return extension switch
        {
            "txt" => "text/plain",
            "pdf" => "application/pdf",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xls" => "application/vnd.ms-excel",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "png" => "image/png",
            "jpg" => "image/jpeg",
            "jpeg" => "image/jpeg",
            "gif" => "image/gif",
            "csv" => "text/csv",
            "xml" => "application/xml",
            "json" => "application/json",
            "zip" => "application/zip",
            "rar" => "application/x-rar-compressed",
            "7z" => "application/x-7z-compressed",
            "mp3" => "audio/mpeg",
            "mp4" => "video/mp4",
            "avi" => "video/x-msvideo",
            _ => "application/octet-stream"
        };
    }
    #endregion

    #region 文本操作
    /// <summary>
    /// 读取文本文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="encoding">编码方式，默认UTF8</param>
    public static async Task<string> ReadTextAsync(string filePath, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return await File.ReadAllTextAsync(filePath, encoding);
    }

    /// <summary>
    /// 写入文本文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="content">文件内容</param>
    /// <param name="encoding">编码方式，默认UTF8</param>
    /// <param name="append">是否追加模式</param>
    public static async Task WriteTextAsync(string filePath, string content, Encoding? encoding = null, bool append = false)
    {
        encoding ??= Encoding.UTF8;
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            CreateDirectoryIfNotExists(dir);
        }

        if (append)
        {
            await File.AppendAllTextAsync(filePath, content, encoding);
        }
        else
        {
            await File.WriteAllTextAsync(filePath, content, encoding);
        }
    }
    #endregion
} 