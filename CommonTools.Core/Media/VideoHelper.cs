using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace CommonTools.Core.Media;

/// <summary>
/// 视频处理帮助类
/// </summary>
public static class VideoHelper
{
    #region 信息获取
    /// <summary>
    /// 获取视频信息
    /// </summary>
    public static async Task<VideoInfo> GetVideoInfoAsync(string videoPath)
    {
        var mediaInfo = await FFProbe.AnalyseAsync(videoPath);
        return new VideoInfo
        {
            Duration = mediaInfo.Duration,
            Width = mediaInfo.VideoStreams.FirstOrDefault()?.Width ?? 0,
            Height = mediaInfo.VideoStreams.FirstOrDefault()?.Height ?? 0,
            Bitrate = mediaInfo.VideoStreams.FirstOrDefault()?.BitRate ?? 0,
            Framerate = mediaInfo.VideoStreams.FirstOrDefault()?.FrameRate ?? 0
        };
    }
    #endregion

    #region 视频处理
    /// <summary>
    /// 压缩视频
    /// </summary>
    /// <param name="inputPath">输入文件路径</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <param name="targetSizeMB">目标文件大小(MB)</param>
    public static async Task CompressVideoAsync(string inputPath, string outputPath, int targetSizeMB)
    {
        await FFMpegArguments
            .FromFileInput(inputPath)
            .OutputToFile(outputPath, true, options => options
                .WithVideoBitrate(targetSizeMB * 8192) // 将MB转换为Kbps
                .WithAudioBitrate(128) // 音频比特率128Kbps
                .WithSpeedPreset(Speed.Medium))
            .ProcessAsynchronously();
    }

    /// <summary>
    /// 转换视频格式
    /// </summary>
    public static async Task ConvertFormatAsync(string inputPath, string outputPath)
    {
        await FFMpegArguments
            .FromFileInput(inputPath)
            .OutputToFile(outputPath, true)
            .ProcessAsynchronously();
    }

    /// <summary>
    /// 剪切视频
    /// </summary>
    public static async Task TrimVideoAsync(string inputPath, string outputPath, TimeSpan startTime, TimeSpan duration)
    {
        await FFMpegArguments
            .FromFileInput(inputPath)
            .OutputToFile(outputPath, true, options => options
                .Seek(startTime)
                .WithDuration(duration))
            .ProcessAsynchronously();
    }
    #endregion

    #region 帧处理
    /// <summary>
    /// 提取视频帧
    /// </summary>
    /// <param name="videoPath">视频文件路径</param>
    /// <param name="outputPath">输出目录</param>
    /// <param name="frameRate">每秒提取帧数</param>
    //public static async Task ExtractFramesAsync(string videoPath, string outputPath, int frameRate = 1)
    //{
    //    if (!Directory.Exists(outputPath))
    //    {
    //        Directory.CreateDirectory(outputPath);
    //    }

    //    await FFMpegArguments
    //        .FromFileInput(videoPath)
    //        .OutputToFile(Path.Combine(outputPath, "frame_%d.jpg"), true, options => options
    //            .WithFramerate(frameRate)
    //            .WithVideoFilters(filterOptions => filterOptions
    //                .Select("fps=1")))
    //        .ProcessAsynchronously();
    //}
    #endregion
}

/// <summary>
/// 视频信息类
/// </summary>
public class VideoInfo
{
    /// <summary>
    /// 时长
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// 宽度
    /// </summary>
    public int Width { get; set; }
    
    /// <summary>
    /// 高度
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// 比特率
    /// </summary>
    public double Bitrate { get; set; }
    
    /// <summary>
    /// 帧率
    /// </summary>
    public double Framerate { get; set; }
} 