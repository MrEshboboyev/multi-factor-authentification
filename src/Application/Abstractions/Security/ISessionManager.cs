namespace Application.Abstractions.Security;

public interface ISessionManager
{
    string CreateSession(string userId, string deviceId);
    bool IsValidSession(string sessionId);
    void InvalidateSession(string sessionId);
    void InvalidateAllUserSessions(string userId);
}
