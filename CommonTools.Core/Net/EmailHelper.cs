using System.Net.Mail;
using System.Net;
using System.Text;

namespace CommonTools.Core.Net;

/// <summary>
/// 邮件发送帮助类
/// </summary>
public static class EmailHelper
{
    #region 配置
    /// <summary>
    /// 邮件服务器配置
    /// </summary>
    public class SmtpConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }

    private static SmtpConfig? _defaultConfig;

    /// <summary>
    /// 设置默认邮件服务器配置
    /// </summary>
    public static void SetDefaultConfig(SmtpConfig config)
    {
        _defaultConfig = config;
    }
    #endregion

    #region 邮件发送
    /// <summary>
    /// 发送邮件
    /// </summary>
    public static async Task SendEmailAsync(string to, string subject, string body, 
        SmtpConfig? config = null, bool isHtml = false)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置邮件服务器配置");

        using var message = new MailMessage
        {
            From = new MailAddress(config.Username),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        message.To.Add(to);

        using var client = new SmtpClient(config.Host, config.Port)
        {
            EnableSsl = config.EnableSsl,
            Credentials = new NetworkCredential(config.Username, config.Password)
        };

        await client.SendMailAsync(message);
    }

    /// <summary>
    /// 发送带附件的邮件
    /// </summary>
    public static async Task SendEmailWithAttachmentAsync(string to, string subject, string body,
        string[] attachmentPaths, SmtpConfig? config = null, bool isHtml = false)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置邮件服务器配置");

        using var message = new MailMessage
        {
            From = new MailAddress(config.Username),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        message.To.Add(to);

        foreach (var path in attachmentPaths)
        {
            message.Attachments.Add(new Attachment(path));
        }

        using var client = new SmtpClient(config.Host, config.Port)
        {
            EnableSsl = config.EnableSsl,
            Credentials = new NetworkCredential(config.Username, config.Password)
        };

        await client.SendMailAsync(message);
    }

    /// <summary>
    /// 群发邮件
    /// </summary>
    public static async Task SendBulkEmailAsync(string[] toAddresses, string subject, string body,
        SmtpConfig? config = null, bool isHtml = false)
    {
        config ??= _defaultConfig ?? throw new InvalidOperationException("未设置邮件服务器配置");

        using var message = new MailMessage
        {
            From = new MailAddress(config.Username),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        foreach (var address in toAddresses)
        {
            message.Bcc.Add(address); // 使用密送避免收件人看到其他收件人
        }

        using var client = new SmtpClient(config.Host, config.Port)
        {
            EnableSsl = config.EnableSsl,
            Credentials = new NetworkCredential(config.Username, config.Password)
        };

        await client.SendMailAsync(message);
    }
    #endregion

    #region 邮件模板
    /// <summary>
    /// 使用模板发送邮件
    /// </summary>
    public static async Task SendTemplateEmailAsync(string to, string subject, string templatePath,
        Dictionary<string, string> parameters, SmtpConfig? config = null)
    {
        var template = await File.ReadAllTextAsync(templatePath);
        var body = ReplaceTemplateParameters(template, parameters);
        await SendEmailAsync(to, subject, body, config, true);
    }

    /// <summary>
    /// 替换模板参数
    /// </summary>
    private static string ReplaceTemplateParameters(string template, Dictionary<string, string> parameters)
    {
        foreach (var param in parameters)
        {
            template = template.Replace($"{{{param.Key}}}", param.Value);
        }
        return template;
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 验证邮箱格式
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 创建HTML格式的邮件内容
    /// </summary>
    public static string CreateHtmlBody(string content, string? css = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        if (!string.IsNullOrEmpty(css))
        {
            sb.AppendLine("<style>");
            sb.AppendLine(css);
            sb.AppendLine("</style>");
        }
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine(content);
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }
    #endregion
} 