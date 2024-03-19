using Conference.Models;
using Conference.Services;
using Microsoft.AspNetCore.SignalR;

namespace Conference.Hubs;

public class CallHub : Hub
{
    private const string CallInstanceKey = "CallInstance";
    
    private CallCoordinatorService _callCoordinator;
    private ILogger<CallHub> _logger;
    
    public CallHub(CallCoordinatorService callCoordinator, ILogger<CallHub> logger)
    {
        _callCoordinator = callCoordinator;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        string callId = Context.GetHttpContext()!.Request.Query["callId"]!;
        
        ConferenceCall? call = _callCoordinator.GetCall(callId);

        if (call != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, callId);

            Context.Items[CallInstanceKey] = call;
        
            call.AddParticipant(Context.ConnectionId);

            if (call.CheckAllReady())
            {
                await Clients.Group(callId).SendAsync("StartSimplePeerConnection", call.GetInitiator());
            }   
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        ConferenceCall? call = (ConferenceCall?)Context.Items[CallInstanceKey];

        if (call != null)
        {
            _callCoordinator.RemoveCall(call.Id);
        
            await Clients.GroupExcept(call.Id, Context.ConnectionId).SendAsync("PeerDisconnected");   
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendSimplePeerSignal(string data)
    {
        ConferenceCall? call = (ConferenceCall?)Context.Items[CallInstanceKey];

        if (call != null)
        {
            _logger.LogWarning("Received simple-peer signal from {userId} on call {callId}", Context.ConnectionId, call.Id);
        
            await Clients.GroupExcept(call.Id, Context.ConnectionId).SendAsync("ReceiveSimplePeerSignal", data);   
        }
    }
}