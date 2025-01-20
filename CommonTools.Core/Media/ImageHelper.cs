using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace CommonTools.Core.Media;

/// <summary>
/// 图片处理帮助类
/// </summary>
public static class ImageHelper
{
    #region 图片信息
    /// <summary>
    /// 获取图片信息
    /// </summary>
    public static async Task<ImageInfo> GetImageInfoAsync(string imagePath)
    {
        using var image = await Image.LoadAsync(imagePath);
        return new ImageInfo
        {
            Width = image.Width,
            Height = image.Height,
            Format = image.Metadata.DecodedImageFormat?.DefaultMimeType ?? "unknown"
        };
    }
    #endregion

    #region 尺寸调整
    /// <summary>
    /// 调整图片大小
    /// </summary>
    /// <param name="sourcePath">源图片路径</param>
    /// <param name="targetPath">目标图片路径</param>
    /// <param name="width">目标宽度</param>
    /// <param name="height">目标高度</param>
    /// <param name="maintainAspectRatio">是否保持宽高比</param>
    public static async Task ResizeImageAsync(string sourcePath, string targetPath, int width, int height, bool maintainAspectRatio = true)
    {
        using var image = await Image.LoadAsync(sourcePath);
        
        if (maintainAspectRatio)
        {
            var ratio = Math.Min((double)width / image.Width, (double)height / image.Height);
            width = (int)(image.Width * ratio);
            height = (int)(image.Height * ratio);
        }

        image.Mutate(x => x.Resize(width, height));
        await image.SaveAsync(targetPath);
    }

    /// <summary>
    /// 生成缩略图
    /// </summary>
    public static async Task CreateThumbnailAsync(string sourcePath, string targetPath, int size)
    {
        using var image = await Image.LoadAsync(sourcePath);
        var ratio = Math.Min((double)size / image.Width, (double)size / image.Height);
        var width = (int)(image.Width * ratio);
        var height = (int)(image.Height * ratio);

        image.Mutate(x => x.Resize(width, height));
        await image.SaveAsync(targetPath);
    }
    #endregion

    #region 图片处理
    /// <summary>
    /// 压缩图片
    /// </summary>
    /// <param name="sourcePath">源图片路径</param>
    /// <param name="targetPath">目标图片路径</param>
    /// <param name="quality">压缩质量(1-100)</param>
    public static async Task CompressImageAsync(string sourcePath, string targetPath, int quality = 75)
    {
        using var image = await Image.LoadAsync(sourcePath);
        var encoder = new JpegEncoder { Quality = quality };
        await image.SaveAsJpegAsync(targetPath, encoder);
    }

    /// <summary>
    /// 旋转图片
    /// </summary>
    public static async Task RotateImageAsync(string sourcePath, string targetPath, int degrees)
    {
        using var image = await Image.LoadAsync(sourcePath);
        image.Mutate(x => x.Rotate(degrees));
        await image.SaveAsync(targetPath);
    }

    /// <summary>
    /// 裁剪图片
    /// </summary>
    public static async Task CropImageAsync(string sourcePath, string targetPath, Rectangle rectangle)
    {
        using var image = await Image.LoadAsync(sourcePath);
        image.Mutate(x => x.Crop(rectangle));
        await image.SaveAsync(targetPath);
    }
    #endregion

    #region 格式转换
    /// <summary>
    /// 转换图片格式
    /// </summary>
    public static async Task ConvertFormatAsync(string sourcePath, string targetPath)
    {
        using var image = await Image.LoadAsync(sourcePath);
        var extension = Path.GetExtension(targetPath).ToLower();
        
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                await image.SaveAsJpegAsync(targetPath);
                break;
            case ".png":
                await image.SaveAsPngAsync(targetPath);
                break;
            default:
                await image.SaveAsync(targetPath);
                break;
        }
    }

    /// <summary>
    /// 转换为PNG格式
    /// </summary>
    public static async Task ConvertToPngAsync(string sourcePath, string targetPath)
    {
        using var image = await Image.LoadAsync(sourcePath);
        await image.SaveAsPngAsync(targetPath);
    }

    /// <summary>
    /// 转换为JPEG格式
    /// </summary>
    public static async Task ConvertToJpegAsync(string sourcePath, string targetPath, int quality = 75)
    {
        using var image = await Image.LoadAsync(sourcePath);
        var encoder = new JpegEncoder { Quality = quality };
        await image.SaveAsJpegAsync(targetPath, encoder);
    }
    #endregion

    #region 水印处理
    /// <summary>
    /// 添加文字水印
    /// </summary>
    public static async Task AddTextWatermarkAsync(string sourcePath, string targetPath, string text, 
        float fontSize = 32, float opacity = 0.5f)
    {
        using var image = await Image.LoadAsync(sourcePath);
        // 注意：这里需要添加字体处理的代码
        // 由于ImageSharp的字体处理较为复杂，这里仅作示例
        // image.Mutate(x => x.DrawText(text, font, color, point));
        await image.SaveAsync(targetPath);
    }

    /// <summary>
    /// 添加图片水印
    /// </summary>
    public static async Task AddImageWatermarkAsync(string sourcePath, string watermarkPath, 
        string targetPath, float opacity = 0.5f)
    {
        using var image = await Image.LoadAsync(sourcePath);
        using var watermark = await Image.LoadAsync(watermarkPath);
        
        // 计算水印位置（右下角）
        var x = image.Width - watermark.Width - 10;
        var y = image.Height - watermark.Height - 10;

        image.Mutate(ctx => ctx
            .DrawImage(watermark, new Point(x, y), opacity));
        
        await image.SaveAsync(targetPath);
    }
    #endregion
}

/// <summary>
/// 图片信息类
/// </summary>
public class ImageInfo
{
    /// <summary>
    /// 宽度
    /// </summary>
    public int Width { get; set; }
    
    /// <summary>
    /// 高度
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// 格式
    /// </summary>
    public string Format { get; set; } = string.Empty;
} 