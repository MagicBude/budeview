namespace BudeView.Core;

public static class CacheBudgetPolicy
{
    public const long MinimumBytes = 256L * 1024L * 1024L;
    public const long MaximumBytes = 512L * 1024L * 1024L;

    public static long CalculateDefaultBytes()
    {
        var available = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        if (available <= 0)
        {
            return MinimumBytes;
        }

        return Math.Clamp(
            available / 32L,
            MinimumBytes,
            MaximumBytes);
    }
}
