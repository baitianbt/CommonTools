using System.Net;
using System.Text;

namespace CommonTools.Core.Net;

/// <summary>
/// FTP操作帮助类
/// </summary>
public static class FtpHelper
{
    #region 配置
    /// <summary>
    /// FTP服务器配置
    /// </summary>
    public class FtpConfig
    {
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = false;
        public int Timeout { get; set; } = 30000;
    }

    private static FtpConfig? _defaultConfig;

    /// <summary>
    /// 设置默认FTP配置
    /// </summary>
    public static void SetDefaultConfig(FtpConfig config)
    {
        _defaultConfig = config;
    }
    #endregion

    #region 文件上传
    /// <summary>
    /// 上传文件
    /// </summary>
    public static async Task UploadFileAsync(string localPath, string remotePath, FtpConfig? config = null)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置FTP配置");
        var request = CreateFtpRequest(config.Host + remotePath, config, WebRequestMethods.Ftp.UploadFile);

        using var fileStream = File.OpenRead(localPath);
        using var ftpStream = await request.GetRequestStreamAsync();
        await fileStream.CopyToAsync(ftpStream);
    }

    /// <summary>
    /// 上传目录
    /// </summary>
    public static async Task UploadDirectoryAsync(string localPath, string remotePath, FtpConfig? config = null)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置FTP配置");

        foreach (var file in Directory.GetFiles(localPath, "*.*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(localPath, file);
            var remoteFilePath = Path.Combine(remotePath, relativePath).Replace("\\", "/");
            
            // 确保远程目录存在
            var remoteDir = Path.GetDirectoryName(remoteFilePath)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(remoteDir))
            {
                await CreateDirectoryAsync(remoteDir, config);
            }

            await UploadFileAsync(file, remoteFilePath, config);
        }
    }
    #endregion

    #region 文件下载
    /// <summary>
    /// 下载文件
    /// </summary>
    public static async Task DownloadFileAsync(string remotePath, string localPath, FtpConfig? config = null)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置FTP配置");
        var request = CreateFtpRequest(config.Host + remotePath, config, WebRequestMethods.Ftp.DownloadFile);

        using var response = (FtpWebResponse)await request.GetResponseAsync();
        using var ftpStream = response.GetResponseStream();
        using var fileStream = File.Create(localPath);
        await ftpStream.CopyToAsync(fileStream);
    }

    /// <summary>
    /// 下载目录
    /// </summary>
    public static async Task DownloadDirectoryAsync(string remotePath, string localPath, FtpConfig? config = null)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置FTP配置");
        var files = await ListDirectoryAsync(remotePath, config);

        foreach (var file in files)
        {
            var remoteFilePath = Path.Combine(remotePath, file).Replace("\\", "/");
            var localFilePath = Path.Combine(localPath, file);
            
            var localDir = Path.GetDirectoryName(localFilePath);
            if (!string.IsNullOrEmpty(localDir))
            {
                Directory.CreateDirectory(localDir);
            }

            await DownloadFileAsync(remoteFilePath, localFilePath, config);
        }
    }
    #endregion

    #region 目录操作
    /// <summary>
    /// 创建目录
    /// </summary>
    public static async Task CreateDirectoryAsync(string remotePath, FtpConfig? config = null)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置FTP配置");
        var request = CreateFtpRequest(config.Host + remotePath, config, WebRequestMethods.Ftp.MakeDirectory);

        try
        {
            using var response = (FtpWebResponse)await request.GetResponseAsync();
        }
        catch (WebException ex)
        {
            // 如果目录已存在，忽略错误
            if (((FtpWebResponse)ex.Response).StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// 列出目录内容
    /// </summary>
    public static async Task<string[]> ListDirectoryAsync(string remotePath, FtpConfig? config = null)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置FTP配置");
        var request = CreateFtpRequest(config.Host + remotePath, config, WebRequestMethods.Ftp.ListDirectory);

        using var response = (FtpWebResponse)await request.GetResponseAsync();
        using var streamReader = new StreamReader(response.GetResponseStream());
        var result = await streamReader.ReadToEndAsync();
        return result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }
    #endregion

    #region 文件操作
    /// <summary>
    /// 删除文件
    /// </summary>
    public static async Task DeleteFileAsync(string remotePath, FtpConfig? config = null)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置FTP配置");
        var request = CreateFtpRequest(config.Host + remotePath, config, WebRequestMethods.Ftp.DeleteFile);
        using var response = (FtpWebResponse)await request.GetResponseAsync();
    }

    /// <summary>
    /// 重命名文件
    /// </summary>
    public static async Task RenameFileAsync(string remotePath, string newName, FtpConfig? config = null)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置FTP配置");
        var request = CreateFtpRequest(config.Host + remotePath, config, WebRequestMethods.Ftp.Rename);
        request.RenameTo = newName;
        using var response = (FtpWebResponse)await request.GetResponseAsync();
    }

    /// <summary>
    /// 获取文件大小
    /// </summary>
    public static async Task<long> GetFileSizeAsync(string remotePath, FtpConfig? config = null)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置FTP配置");
        var request = CreateFtpRequest(config.Host + remotePath, config, WebRequestMethods.Ftp.GetFileSize);
        using var response = (FtpWebResponse)await request.GetResponseAsync();
        return response.ContentLength;
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 创建FTP请求
    /// </summary>
    private static FtpWebRequest CreateFtpRequest(string url, FtpConfig config, string method)
    {
        var request = (FtpWebRequest)WebRequest.Create(url);
        request.Method = method;
        request.Credentials = new NetworkCredential(config.Username, config.Password);
        request.EnableSsl = config.EnableSsl;
        request.Timeout = config.Timeout;
        request.UsePassive = true;
        request.KeepAlive = false;
        return request;
    }

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    public static async Task<bool> FileExistsAsync(string remotePath, FtpConfig? config = null)
    {
        try
        {
            await GetFileSizeAsync(remotePath, config);
            return true;
        }
        catch (WebException)
        {
            return false;
        }
    }
    #endregion
}