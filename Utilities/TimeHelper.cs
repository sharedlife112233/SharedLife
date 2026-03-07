namespace SharedLife.Utilities;

public static class TimeHelper
{
    private static readonly TimeSpan NepalOffset = TimeSpan.FromHours(5) + TimeSpan.FromMinutes(45);

    /// <summary>
    /// Returns the current date and time in Nepal Standard Time (UTC+5:45).
    /// Uses DateTimeKind.Unspecified so JSON serialization does not add "Z" suffix,
    /// which would cause JavaScript to misinterpret the value as UTC.
    /// </summary>
    public static DateTime Now => DateTime.SpecifyKind(DateTime.UtcNow.Add(NepalOffset), DateTimeKind.Unspecified);
}
