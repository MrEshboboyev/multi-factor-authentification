using Application.Abstractions.Security;
using System.Collections.Concurrent;

namespace Infrastructure.Authentication;

public class SessionManager : ISessionManager
{
    private static readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
    private static readonly ConcurrentDictionary<string, List<string>> _userSessions = new();

    public string CreateSession(string userId, string deviceId)
    {
        var sessionId = Guid.NewGuid().ToString();
        var sessionInfo = new SessionInfo
        {
            UserId = userId,
            DeviceId = deviceId,
            CreatedAt = DateTime.UtcNow,
            LastAccessed = DateTime.UtcNow
        };

        _sessions[sessionId] = sessionInfo;
        
        // Track sessions by user
        _userSessions.AddOrUpdate(userId, 
            new List<string> { sessionId }, 
            (key, existing) => { existing.Add(sessionId); return existing; });

        return sessionId;
    }

    public bool IsValidSession(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return false;

        // Check if session is expired (24 hours)
        if (DateTime.UtcNow > session.CreatedAt.AddHours(24))
        {
            InvalidateSession(sessionId);
            return false;
        }

        // Update last accessed time
        session.LastAccessed = DateTime.UtcNow;
        return true;
    }

    public void InvalidateSession(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            // Remove from user sessions list
            if (_userSessions.TryGetValue(session.UserId, out var userSessions))
            {
                userSessions.Remove(sessionId);
            }
        }
    }

    public void InvalidateAllUserSessions(string userId)
    {
        if (_userSessions.TryGetValue(userId, out var sessionIds))
        {
            foreach (var sessionId in sessionIds.ToList())
            {
                InvalidateSession(sessionId);
            }
        }
    }

    private class SessionInfo
    {
        public string UserId { get; set; }
        public string DeviceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
    }
}