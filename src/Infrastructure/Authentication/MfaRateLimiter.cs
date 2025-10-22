using Application.Abstractions.Security;
using System.Collections.Concurrent;

namespace Infrastructure.Authentication;

public class MfaRateLimiter : IMfaRateLimiter
{
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitInfo = new();
    private const int MaxAttempts = 5;
    private const int LockoutMinutes = 30;

    public bool IsRateLimited(string userId)
    {
        if (_rateLimitInfo.TryGetValue(userId, out var info))
        {
            return info.Attempts >= MaxAttempts && 
                   DateTime.UtcNow < info.LastAttempt.AddMinutes(LockoutMinutes);
        }
        return false;
    }

    public void RecordFailedAttempt(string userId)
    {
        _rateLimitInfo.AddOrUpdate(userId, 
            new RateLimitInfo { Attempts = 1, LastAttempt = DateTime.UtcNow },
            (key, existing) => new RateLimitInfo 
            { 
                Attempts = existing.Attempts + 1, 
                LastAttempt = DateTime.UtcNow 
            });
    }

    public void ResetAttempts(string userId)
    {
        _rateLimitInfo.TryRemove(userId, out _);
    }

    private class RateLimitInfo
    {
        public int Attempts { get; set; }
        public DateTime LastAttempt { get; set; }
    }
}
