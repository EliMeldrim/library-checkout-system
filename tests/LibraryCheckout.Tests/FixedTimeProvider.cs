namespace LibraryCheckout.Tests;

/// <summary>A TimeProvider frozen at a specific instant, advanceable by tests.</summary>
public sealed class FixedTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now) => _now = now;

    public override DateTimeOffset GetUtcNow() => _now.ToUniversalTime();

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public void Advance(TimeSpan by) => _now = _now.Add(by);
}
