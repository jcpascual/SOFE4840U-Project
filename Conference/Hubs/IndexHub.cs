using Conference.Models;
using Conference.Services;
using Microsoft.AspNetCore.SignalR;

namespace Conference.Hubs;

public class IndexHub : Hub
{
    private const string UserInstanceKey = "UserInstance";
    
    private readonly DatabaseService _databaseService;
    private readonly CallCoordinatorService _coordinatorService;
    
    public IndexHub(DatabaseService databaseService, CallCoordinatorService coordinatorService)
    {
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
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}