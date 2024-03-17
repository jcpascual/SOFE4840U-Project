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

        Context.Items[UserInstanceKey] = user;
        
        _coordinatorService.UpdateUserAvailableStatus(user, true);
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        ConferenceUser user = (ConferenceUser)Context.Items[UserInstanceKey]!;
        
        _coordinatorService.UpdateUserAvailableStatus(user, false);
        
        await base.OnDisconnectedAsync(exception);
    }
}