namespace SharedLife.Utilities;

public static class TimeHelper
{
    private static readonly TimeSpan NepalOffset = TimeSpan.FromHours(5) + TimeSpan.FromMinutes(45);

    /// <summary>
    /// Returns the current date and time in Nepal Standard Time (UTC+5:45).
    /// </summary>
    public static DateTime Now => DateTime.UtcNow.Add(NepalOffset);
}
