namespace LanLinker.Core;

public abstract class AppSettings
{
    public static readonly TimeSpan PeerTimeout = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan AnnouncementIntervalTime = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan CleanupDueTime = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan CleanupPeriodTime = TimeSpan.FromSeconds(10);
}