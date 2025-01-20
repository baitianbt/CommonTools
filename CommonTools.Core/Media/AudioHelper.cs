using NAudio.Wave;
using NAudio.MediaFoundation;

namespace CommonTools.Core.Media;

/// <summary>
/// 音频处理帮助类
/// </summary>
public static class AudioHelper
{
    #region 信息获取
    /// <summary>
    /// 获取音频文件信息
    /// </summary>
    public static AudioInfo GetAudioInfo(string audioPath)
    {
        using var reader = new AudioFileReader(audioPath);
        return new AudioInfo
        {
            Duration = reader.TotalTime,
            SampleRate = reader.WaveFormat.SampleRate,
            Channels = reader.WaveFormat.Channels,
            BitsPerSample = reader.WaveFormat.BitsPerSample
        };
    }
    #endregion

    #region 格式转换
    /// <summary>
    /// 转换音频格式
    /// </summary>
    /// <param name="inputPath">输入文件路径</param>
    /// <param name="outputPath">输出文件路径</param>
    public static void ConvertFormat(string inputPath, string outputPath)
    {
        using var reader = new AudioFileReader(inputPath);
        MediaFoundationEncoder.EncodeToMp3(reader, outputPath);
    }
    #endregion

    #region 音频处理
    /// <summary>
    /// 剪切音频
    /// </summary>
    /// <param name="inputPath">输入文件路径</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="duration">持续时间</param>
    public static void TrimAudio(string inputPath, string outputPath, TimeSpan startTime, TimeSpan duration)
    {
        using var reader = new AudioFileReader(inputPath);
        using var writer = File.Create(outputPath);
        
        reader.CurrentTime = startTime;
        var bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000.0;
        var startPos = (int)(startTime.TotalMilliseconds * bytesPerMillisecond);
        var endPos = (int)((startTime + duration).TotalMilliseconds * bytesPerMillisecond);
        
        var buffer = new byte[1024];
        while (reader.Position < endPos)
        {
            var bytesRead = reader.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) break;
            writer.Write(buffer, 0, bytesRead);
        }
    }

    /// <summary>
    /// 合并音频文件
    /// </summary>
    public static void ConcatenateAudio(string[] inputFiles, string outputPath)
    {
        using var writer = new WaveFileWriter(outputPath, new AudioFileReader(inputFiles[0]).WaveFormat);
        foreach (var inputFile in inputFiles)
        {
            using var reader = new AudioFileReader(inputFile);
            var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                writer.Write(buffer, 0, read);
            }
        }
    }
    #endregion
}

/// <summary>
/// 音频信息类
/// </summary>
public class AudioInfo
{
    /// <summary>
    /// 时长
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// 采样率
    /// </summary>
    public int SampleRate { get; set; }
    
    /// <summary>
    /// 声道数
    /// </summary>
    public int Channels { get; set; }
    
    /// <summary>
    /// 采样位数
    /// </summary>
    public int BitsPerSample { get; set; }
} 