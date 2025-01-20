using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CommonTools.Core.Office;

/// <summary>
/// Word文档处理帮助类
/// </summary>
public static class WordHelper
{
    /// <summary>
    /// 创建Word文档
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="content">文档内容</param>
    public static void CreateDocument(string filePath, string content)
    {
        using var document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        foreach (var paragraph in content.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
        {
            if (string.IsNullOrWhiteSpace(paragraph)) continue;
            
            body.AppendChild(new Paragraph(
                new Run(
                    new Text(paragraph))));
        }
    }

    /// <summary>
    /// 读取Word文档内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文档内容</returns>
    public static string ReadDocument(string filePath)
    {
        using var document = WordprocessingDocument.Open(filePath, false);
        var body = document.MainDocumentPart?.Document.Body;
        if (body == null) return string.Empty;

        return string.Join(Environment.NewLine,
            body.Descendants<Paragraph>()
                .Select(p => p.InnerText));
    }

    /// <summary>
    /// 向Word文档追加内容
    /// </summary>
    public static void AppendText(string filePath, string content)
    {
        using var document = WordprocessingDocument.Open(filePath, true);
        var body = document.MainDocumentPart?.Document.Body;
        if (body == null) return;

        body.AppendChild(new Paragraph(
            new Run(
                new Text(content))));
    }

    /// <summary>
    /// 替换Word文档中的文本
    /// </summary>
    public static void ReplaceText(string filePath, string oldText, string newText)
    {
        using var document = WordprocessingDocument.Open(filePath, true);
        var body = document.MainDocumentPart?.Document.Body;
        if (body == null) return;

        foreach (var text in body.Descendants<Text>())
        {
            if (text.Text.Contains(oldText))
            {
                text.Text = text.Text.Replace(oldText, newText);
            }
        }
    }
} 