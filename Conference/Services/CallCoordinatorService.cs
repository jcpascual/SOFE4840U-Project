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

    public string? GetIndexConnectionId(ConferenceUser user)
    {
        return GetUserIndexState(user).ConnectionId;
    }

    public void SetIndexConnectionId(ConferenceUser user, string? connectionId)
    {
        GetUserIndexState(user).ConnectionId = connectionId;
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

    public string? GetUserCallId(ConferenceUser user)
    {
        return GetUserIndexState(user).CallId;
    }

    public void SetUserCallId(ConferenceUser user, string? callId)
    {
        GetUserIndexState(user).CallId = callId;
    }

    public ConferenceCall CreateCall()
    {
        string GenerateRandomId()
        {
            const string validChars = "ABCDEFHIJKLMNOPQRSTUVWXYZabcdefhijklmnopqrstuvwxyz0123456789";
            const int maxLength = 10;

            StringBuilder builder = new StringBuilder();
        
            Random random = new Random();

            for (int i = 0; i < maxLength; i++)
            {
                builder.Append(validChars[random.Next(validChars.Length)]);
            }

            return builder.ToString();
        }

        string callId;

        do
        {
            callId = GenerateRandomId();
        } while (_calls.ContainsKey(callId));
        
        _logger.LogWarning("New call: {callId}", callId);
        
        ConferenceCall call = new ConferenceCall(callId, 2);
        
        _calls[callId] = call;

        return call;
    }

    public ConferenceCall? GetCall(string callId)
    {
        if (_calls.TryGetValue(callId, out ConferenceCall? foundCall))
        {
            return foundCall;
        }

        return null;
    }

    public void RemoveCall(string callId)
    {
        _calls.TryRemove(callId, out _);
    }
}