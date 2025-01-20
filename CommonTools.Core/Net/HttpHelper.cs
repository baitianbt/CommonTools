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
    #endregion

    #region GET请求
    /// <summary>
    /// 发送GET请求
    /// </summary>
    public static async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddHeaders(request, headers);
        
        using var response = await DefaultClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// 发送GET请求并反序列化结果
    /// </summary>
    public static async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
    {
        var json = await GetAsync(url, headers);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    public static async Task DownloadFileAsync(string url, string savePath)
    {
        using var response = await DefaultClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create(savePath);
        await stream.CopyToAsync(fileStream);
    }
    #endregion

    #region POST请求
    /// <summary>
    /// 发送POST请求
    /// </summary>
    public static async Task<string> PostAsync(string url, object? data = null, 
        Dictionary<string, string>? headers = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        AddHeaders(request, headers);
        
        if (data != null)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var response = await DefaultClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// 发送POST请求并反序列化结果
    /// </summary>
    public static async Task<T?> PostAsync<T>(string url, object? data = null, 
        Dictionary<string, string>? headers = null)
    {
        var json = await PostAsync(url, data, headers);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    /// <summary>
    /// 发送表单数据
    /// </summary>
    public static async Task<string> PostFormAsync(string url, Dictionary<string, string> formData, 
        Dictionary<string, string>? headers = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        AddHeaders(request, headers);
        
        request.Content = new FormUrlEncodedContent(formData);

        using var response = await DefaultClient.SendAsync(request);
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
        Dictionary<string, string>? headers = null)
    {
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

        using var response = await DefaultClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    #endregion

    #region 工具方法
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
            _ => "application/octet-stream"
        };
        return new MediaTypeHeaderValue(mimeType);
    }
    #endregion
} 