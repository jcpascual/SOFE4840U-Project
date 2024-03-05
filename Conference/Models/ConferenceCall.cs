using System.Collections.Concurrent;

namespace Conference.Models;

public class ConferenceCall
{
    public string Id
    {
        get;
        private set;
    }

    private readonly int _expectedSize;
    private readonly ConcurrentBag<string> _participants = new ConcurrentBag<string>();
    
    public ConferenceCall(string id, int expectedSize)
    {
        Id = id;
        _expectedSize = expectedSize;
    }

    public void AddParticipant(string userId)
    {
        _participants.Add(userId);
    }

    public bool CheckAllReady()
    {
        return _participants.Count == _expectedSize;
    }

    public string GetInitiator()
    {
        string? initiatorId = _participants.ElementAtOrDefault(0);
        
        if (initiatorId == null)
        {
            throw new InvalidOperationException("No users in call");
        }

        return initiatorId;
    }
}