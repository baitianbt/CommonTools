namespace CommonTools.Core.DateTime;

/// <summary>
/// 日期时间帮助类
/// </summary>
public static class DateTimeHelper
{
    #region 日期获取
    /// <summary>
    /// 获取某一天的开始时间
    /// </summary>
    public static System.DateTime GetStartOfDay(System.DateTime date)
        => new(date.Year, date.Month, date.Day, 0, 0, 0, 0);

    /// <summary>
    /// 获取某一天的结束时间
    /// </summary>
    public static System.DateTime GetEndOfDay(System.DateTime date)
        => new(date.Year, date.Month, date.Day, 23, 59, 59, 999);

    /// <summary>
    /// 获取本周第一天
    /// </summary>
    public static System.DateTime GetStartOfWeek(System.DateTime date, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
    {
        var diff = date.DayOfWeek - firstDayOfWeek;
        if (diff < 0) diff += 7;
        return date.AddDays(-diff).Date;
    }

    /// <summary>
    /// 获取本月第一天
    /// </summary>
    public static System.DateTime GetStartOfMonth(System.DateTime date)
        => new(date.Year, date.Month, 1);
    #endregion

    #region 时间戳转换
    /// <summary>
    /// 获取时间戳（Unix时间戳，单位：秒）
    /// </summary>
    public static long GetTimeStamp(System.DateTime? dateTime = null)
    {
        var date = dateTime ?? System.DateTime.UtcNow;
        return ((DateTimeOffset)date).ToUnixTimeSeconds();
    }

    /// <summary>
    /// 将时间戳转换为DateTime
    /// </summary>
    public static System.DateTime FromTimeStamp(long timestamp)
        => DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;

    /// <summary>
    /// 获取毫秒级时间戳
    /// </summary>
    public static long GetMilliTimeStamp(System.DateTime? dateTime = null)
    {
        var date = dateTime ?? System.DateTime.UtcNow;
        return ((DateTimeOffset)date).ToUnixTimeMilliseconds();
    }
    #endregion

    #region 时间计算
    /// <summary>
    /// 计算两个日期之间的工作日天数
    /// </summary>
    public static int GetWorkDays(System.DateTime startDate, System.DateTime endDate)
    {
        var days = 0;
        var current = startDate.Date;
        while (current <= endDate.Date)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                days++;
            }
            current = current.AddDays(1);
        }
        return days;
    }

    /// <summary>
    /// 获取两个日期之间的天数
    /// </summary>
    public static int GetDaysBetween(System.DateTime startDate, System.DateTime endDate)
        => (int)(endDate.Date - startDate.Date).TotalDays;

    /// <summary>
    /// 获取年龄
    /// </summary>
    public static int GetAge(System.DateTime birthDate)
    {
        var today = System.DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
    #endregion

    #region 格式化
    /// <summary>
    /// 格式化为友好的时间显示
    /// </summary>
    public static string ToFriendlyDisplay(System.DateTime dateTime)
    {
        var span = System.DateTime.Now - dateTime;
        if (span.TotalDays > 365)
            return $"{(int)(span.TotalDays / 365)}年前";
        if (span.TotalDays > 30)
            return $"{(int)(span.TotalDays / 30)}个月前";
        if (span.TotalDays > 7)
            return $"{(int)(span.TotalDays / 7)}周前";
        if (span.TotalDays >= 1)
            return $"{(int)span.TotalDays}天前";
        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours}小时前";
        if (span.TotalMinutes >= 1)
            return $"{(int)span.TotalMinutes}分钟前";
        return "刚刚";
    }

    /// <summary>
    /// 转换为中文日期格式
    /// </summary>
    public static string ToChineseDate(System.DateTime dateTime)
    {
        string[] numbers = { "〇", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        var result = string.Empty;
        var year = dateTime.Year.ToString();
        foreach (var c in year)
        {
            result += numbers[c - '0'];
        }
        result += "年";
        result += numbers[dateTime.Month / 10] + numbers[dateTime.Month % 10] + "月";
        result += numbers[dateTime.Day / 10] + numbers[dateTime.Day % 10] + "日";
        return result;
    }
    #endregion
} 