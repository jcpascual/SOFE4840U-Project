using System.Collections.Concurrent;
using System.Text;
using Conference.Hubs;
using Conference.Models;
using Microsoft.AspNetCore.SignalR;

namespace Conference.Services;

public class CallCoordinatorService
{
    private ILogger<CallCoordinatorService> _logger;

    private ConcurrentDictionary<string, ConferenceCall> _calls = new ConcurrentDictionary<string, ConferenceCall>();

    public CallCoordinatorService(ILogger<CallCoordinatorService> logger)
    {
        _logger = logger;
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