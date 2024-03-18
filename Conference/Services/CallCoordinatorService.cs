using System.Collections.Concurrent;
using System.Text;
using Conference.Hubs;
using Conference.Models;
using Microsoft.AspNetCore.SignalR;

namespace Conference.Services;

public class CallCoordinatorService
{
    private ILogger<CallCoordinatorService> _logger;

    private ConcurrentDictionary<int, ConferenceUserIndexState> _indexStates =
        new ConcurrentDictionary<int, ConferenceUserIndexState>();
    private ConcurrentDictionary<string, ConferenceCall> _calls = new ConcurrentDictionary<string, ConferenceCall>();

    public CallCoordinatorService(ILogger<CallCoordinatorService> logger)
    {
        _logger = logger;
    }
    
    private ConferenceUserIndexState GetUserIndexState(ConferenceUser user)
    {
        if (_indexStates.TryGetValue(user.Id, out ConferenceUserIndexState? state))
        {
            return state;
        }

        state = new ConferenceUserIndexState();

        _indexStates[user.Id] = state;

        return state;
    }
    
    public ConferenceUserStatus GetUserStatus(ConferenceUser user)
    {
        return GetUserIndexState(user).Status;
    }

    public bool IsUserAvailable(ConferenceUser user)
    {
        return GetUserStatus(user) == ConferenceUserStatus.Online;
    }

    public bool IsUserConnected(ConferenceUser user)
    {
        return GetUserStatus(user) != ConferenceUserStatus.Offline;
    }
    
    public void SetUserStatus(ConferenceUser user, ConferenceUserStatus status)
    {
        GetUserIndexState(user).Status = status;
    }

    public ConferenceCall GetOrCreateCall(string callId)
    {
        if (_calls.TryGetValue(callId, out ConferenceCall? foundCall))
        {
            return foundCall;
        }
        
        _logger.LogWarning("New call: {callId}", callId);
        
        ConferenceCall call = new ConferenceCall(callId, 2);
        
        _calls[callId] = call;

        return call;
    }

    public void RemoveCall(string callId)
    {
        _calls.TryRemove(callId, out _);
    }
}