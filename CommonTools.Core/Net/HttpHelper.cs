using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CommonTools.Core.Net;

/// <summary>
/// HTTP请求帮助类
/// </summary>
public static class HttpHelper
{
    #region 配置
    private static readonly HttpClient DefaultClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// HTTP请求配置
    /// </summary>
    public class HttpConfig
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public bool AllowAutoRedirect { get; set; } = true;
        public IWebProxy? Proxy { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
    #endregion

    #region GET请求
    /// <summary>
    /// 发送GET请求
    /// </summary>
    public static async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null, HttpConfig? config = null)
    {
        using var client = CreateHttpClient(config);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddHeaders(request, headers);
        
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// 发送GET请求并反序列化结果
    /// </summary>
    public static async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, HttpConfig? config = null)
    {
        var json = await GetAsync(url, headers, config);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    public static async Task DownloadFileAsync(string url, string savePath, IProgress<double>? progress = null, HttpConfig? config = null)
    {
        using var client = CreateHttpClient(config);
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var totalBytesRead = 0L;

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create(savePath);
        
        var buffer = new byte[8192];
        var isMoreToRead = true;

        do
        {
            var bytesRead = await contentStream.ReadAsync(buffer);
            if (bytesRead == 0)
            {
                isMoreToRead = false;
                continue;
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalBytesRead += bytesRead;

            if (totalBytes > 0 && progress != null)
            {
                var progressPercentage = (double)totalBytesRead / totalBytes * 100;
                progress.Report(progressPercentage);
            }
        }
        while (isMoreToRead);
    }
    #endregion

    #region POST请求
    /// <summary>
    /// 发送POST请求
    /// </summary>
    public static async Task<string> PostAsync(string url, object? data = null, 
        Dictionary<string, string>? headers = null, HttpConfig? config = null)
    {
        using var client = CreateHttpClient(config);
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        AddHeaders(request, headers);
        
        if (data != null)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// 发送POST请求并反序列化结果
    /// </summary>
    public static async Task<T?> PostAsync<T>(string url, object? data = null, 
        Dictionary<string, string>? headers = null, HttpConfig? config = null)
    {
        var json = await PostAsync(url, data, headers, config);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    /// <summary>
    /// 发送表单数据
    /// </summary>
    public static async Task<string> PostFormAsync(string url, Dictionary<string, string> formData, 
        Dictionary<string, string>? headers = null, HttpConfig? config = null)
    {
        using var client = CreateHttpClient(config);
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        AddHeaders(request, headers);
        
        request.Content = new FormUrlEncodedContent(formData);

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    #endregion

    #region 文件上传
    /// <summary>
    /// 上传文件
    /// </summary>
    public static async Task<string> UploadFileAsync(string url, string filePath, 
        string fileFieldName = "file", Dictionary<string, string>? formData = null,
        Dictionary<string, string>? headers = null, HttpConfig? config = null)
    {
        using var client = CreateHttpClient(config);
        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(filePath);
        var fileName = Path.GetFileName(filePath);
        var fileContent = new StreamContent(fileStream);
        
        fileContent.Headers.ContentType = GetContentType(fileName);
        form.Add(fileContent, fileFieldName, fileName);

        if (formData != null)
        {
            foreach (var item in formData)
            {
                form.Add(new StringContent(item.Value), item.Key);
            }
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
        AddHeaders(request, headers);

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// 上传多个文件
    /// </summary>
    public static async Task<string> UploadFilesAsync(string url, Dictionary<string, string> files,
        Dictionary<string, string>? formData = null, Dictionary<string, string>? headers = null,
        HttpConfig? config = null)
    {
        using var client = CreateHttpClient(config);
        using var form = new MultipartFormDataContent();

        foreach (var file in files)
        {
            using var fileStream = File.OpenRead(file.Value);
            var fileName = Path.GetFileName(file.Value);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = GetContentType(fileName);
            form.Add(fileContent, file.Key, fileName);
        }

        if (formData != null)
        {
            foreach (var item in formData)
            {
                form.Add(new StringContent(item.Value), item.Key);
            }
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
        AddHeaders(request, headers);

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 创建HttpClient实例
    /// </summary>
    private static HttpClient CreateHttpClient(HttpConfig? config)
    {
        if (config == null) return DefaultClient;

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = config.AllowAutoRedirect,
            Proxy = config.Proxy
        };

        var client = new HttpClient(handler)
        {
            Timeout = config.Timeout
        };

        if (config.Headers != null)
        {
            foreach (var header in config.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        return client;
    }

    /// <summary>
    /// 添加请求头
    /// </summary>
    private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers == null) return;
        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }
    }

    /// <summary>
    /// 获取文件的Content-Type
    /// </summary>
    private static MediaTypeHeaderValue GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        var mimeType = extension switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
        return new MediaTypeHeaderValue(mimeType);
    }

    /// <summary>
    /// 构建查询字符串
    /// </summary>
    public static string BuildQueryString(Dictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0) return string.Empty;
        
        var query = new StringBuilder("?");
        foreach (var param in parameters)
        {
            if (query.Length > 1) query.Append('&');
            query.Append(Uri.EscapeDataString(param.Key));
            query.Append('=');
            query.Append(Uri.EscapeDataString(param.Value));
        }
        return query.ToString();
    }
    #endregion
}