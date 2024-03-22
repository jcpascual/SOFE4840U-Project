using Conference.Models;
using Conference.Services;
using Microsoft.AspNetCore.SignalR;

namespace Conference.Hubs;

public class IndexHub : Hub
{
    private const string UserInstanceKey = "UserInstance";

    private readonly ILogger<IndexHub> _logger;
    
    private readonly DatabaseService _databaseService;
    private readonly CallCoordinatorService _coordinatorService;
    
    public IndexHub(DatabaseService databaseService, CallCoordinatorService coordinatorService, ILogger<IndexHub> logger)
    {
        _logger = logger;

        _databaseService = databaseService;
        _coordinatorService = coordinatorService;
    }
    
    public override async Task OnConnectedAsync()
    {
        string username = Context.GetHttpContext()!.User.Identity!.Name!;
        
        ConferenceUser user = _databaseService.GetUser(username)!;
        
        if (!_coordinatorService.IsUserConnected(user))
        {
            Context.Items[UserInstanceKey] = user;

            _coordinatorService.SetIndexConnectionId(user, Context.ConnectionId);
            _coordinatorService.SetUserStatus(user, ConferenceUserStatus.Online);
            _coordinatorService.SetUserCallId(user, null);
            
            _logger.LogWarning("User {user} connected", user.Username);
        }
        else
        {
            await Clients.Client(Context.ConnectionId).SendAsync("DisconnectDuplicate");
        }
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        ConferenceUser? user = (ConferenceUser?)Context.Items[UserInstanceKey];

        if (user != null)
        {
            _coordinatorService.SetIndexConnectionId(user, null);
            _coordinatorService.SetUserStatus(user, ConferenceUserStatus.Offline);

            string? callId = _coordinatorService.GetUserCallId(user);

            if (callId != null)
            {
                await Clients.Group("call-" + callId!).SendAsync("CallRequestDenied");
            }
            
            _coordinatorService.SetUserCallId(user, null);
            
            _logger.LogWarning("User {user} disconnected", user.Username);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task InitiateCallRequest(int targetId, bool hasPassword)
    {
        ConferenceUser thisUser = (ConferenceUser)Context.Items[UserInstanceKey]!;

        ConferenceUser? targetUser = _databaseService.GetUser(targetId);

        if (targetUser == null)
        {
            _logger.LogWarning("User {user} initiated a call, but the target doesn't exist", thisUser.Username);
            return;
        }

        if (!_coordinatorService.IsUserAvailable(targetUser))
        {
            _logger.LogWarning("User {user} initiated a call to {target}, but the target isn't available",
                thisUser.Username, targetUser.Username);
            return;
        }

        string? targetConnectionId = _coordinatorService.GetIndexConnectionId(targetUser);

        if (targetConnectionId == null)
        {
            _logger.LogWarning("User {user} initiated a call to {target}, but the target connection ID isn't set",
                thisUser.Username, targetUser.Username);
            
            return;
        }

        ConferenceCall call = _coordinatorService.CreateCall();
        
        _logger.LogWarning("User {initiator} is calling {target} (call {id})", thisUser.Username, targetUser.Username,
            call.Id);
        
        call.SetHasPassword(hasPassword);

        _coordinatorService.SetUserStatus(targetUser, ConferenceUserStatus.InCallRequest);
        _coordinatorService.SetUserStatus(thisUser, ConferenceUserStatus.InCallRequest);
        
        _coordinatorService.SetUserCallId(targetUser, call.Id);
        _coordinatorService.SetUserCallId(thisUser, call.Id);
        
        string groupName = $"call-{call.Id}";

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Groups.AddToGroupAsync(targetConnectionId, groupName);

        await Clients.Client(targetConnectionId).SendAsync("CallRequestReceived", call.Id, thisUser.Username);
    }
    
    public async Task TargetCallAccept()
    {
        ConferenceUser user = (ConferenceUser)Context.Items[UserInstanceKey]!;

        string callId = _coordinatorService.GetUserCallId(user)!;
        
        _logger.LogWarning("User {initiator} accepted call {id}", user.Username, callId);

        await Clients.Group("call-" + callId).SendAsync("CallRequestAccepted", callId);
    }
    
    public async Task TargetCallDeny()
    {
        ConferenceUser user = (ConferenceUser)Context.Items[UserInstanceKey]!;
        
        string callId = _coordinatorService.GetUserCallId(user)!;

        _logger.LogWarning("User {initiator} denied call {id}", user.Username, callId);
        
        _coordinatorService.RemoveCall(callId);

        await Clients.Group("call-" + callId).SendAsync("CallRequestDenied");
    }

    public async Task ResetStateAfterDeny()
    {
        ConferenceUser user = (ConferenceUser)Context.Items[UserInstanceKey]!;
        
        _logger.LogWarning("User {initiator} reset status after deny", user.Username);
        
        string? callId = _coordinatorService.GetUserCallId(user);

        if (callId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "call-" + callId);
        }
        
        _coordinatorService.SetUserStatus(user, ConferenceUserStatus.Online);
    }
}