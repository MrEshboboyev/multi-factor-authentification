namespace Application.Abstractions.Security;

public interface IMfaRateLimiter
{
    bool IsRateLimited(string userId);
    void RecordFailedAttempt(string userId);
    void ResetAttempts(string userId);
}
