using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;
using iText.Kernel.Font;

namespace CommonTools.Core.Office;

/// <summary>
/// PDF文件处理帮助类
/// </summary>
public static class PdfHelper
{
    /// <summary>
    /// 创建PDF文档
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="content">文档内容</param>
    public static void CreatePdf(string filePath, string content)
    {
        using var writer = new PdfWriter(filePath);
        using var pdf = new PdfDocument(writer);
        var document = new Document(pdf);

        foreach (var paragraph in content.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
        {
            if (!string.IsNullOrWhiteSpace(paragraph))
            {
                document.Add(new Paragraph(paragraph));
            }
        }
    }

    /// <summary>
    /// 读取PDF文档内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文档内容</returns>
    public static string ReadPdf(string filePath)
    {
        using var reader = new PdfReader(filePath);
        using var pdf = new PdfDocument(reader);
        var text = new StringBuilder();

        for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
        {
            var page = pdf.GetPage(i);
            var strategy = new SimpleTextExtractionStrategy();
            var content = PdfTextExtractor.GetTextFromPage(page, strategy);
            text.AppendLine(content);
        }

        return text.ToString();
    }

    /// <summary>
    /// 合并PDF文件
    /// </summary>
    /// <param name="inputFiles">输入文件列表</param>
    /// <param name="outputFile">输出文件路径</param>
    public static void MergePdf(string[] inputFiles, string outputFile)
    {
        using var writer = new PdfWriter(outputFile);
        using var mergedPdf = new PdfDocument(writer);

        foreach (var inputFile in inputFiles)
        {
            using var reader = new PdfReader(inputFile);
            using var pdf = new PdfDocument(reader);
            pdf.CopyPagesTo(1, pdf.GetNumberOfPages(), mergedPdf);
        }
    }

    /// <summary>
    /// 分割PDF文件
    /// </summary>
    /// <param name="inputFile">输入文件</param>
    /// <param name="outputDirectory">输出目录</param>
    /// <param name="pagesPerFile">每个文件的页数</param>
    public static void SplitPdf(string inputFile, string outputDirectory, int pagesPerFile)
    {
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        using var reader = new PdfReader(inputFile);
        using var sourcePdf = new PdfDocument(reader);
        var totalPages = sourcePdf.GetNumberOfPages();
        var fileCount = (int)Math.Ceiling((double)totalPages / pagesPerFile);

        for (int i = 0; i < fileCount; i++)
        {
            var outputPath = Path.Combine(outputDirectory, $"split_{i + 1}.pdf");
            using var writer = new PdfWriter(outputPath);
            using var targetPdf = new PdfDocument(writer);

            var startPage = i * pagesPerFile + 1;
            var endPage = Math.Min((i + 1) * pagesPerFile, totalPages);
            sourcePdf.CopyPagesTo(startPage, endPage, targetPdf);
        }
    }

    /// <summary>
    /// 添加水印
    /// </summary>
    public static void AddWatermark(string inputFile, string outputFile, string watermarkText)
    {
        using var reader = new PdfReader(inputFile);
        using var writer = new PdfWriter(outputFile);
        using var pdf = new PdfDocument(reader, writer);

        for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
        {
            var page = pdf.GetPage(i);
            var canvas = new PdfCanvas(page);
            var pageSize = page.GetPageSize();

            canvas.SaveState()
                .SetFillColorGray(0.5f)
                .BeginText()
                .SetFontAndSize(PdfFontFactory.CreateFont(), 60)
                .SetTextMatrix(30, 30)
                .ShowText(watermarkText)
                .EndText()
                .RestoreState();
        }
    }
} 