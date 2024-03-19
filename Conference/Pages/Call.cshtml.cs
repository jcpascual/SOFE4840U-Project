using Conference.Models;
using Conference.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Conference.Pages;

[Authorize]
public class CallModel : PageModel
{
    private readonly CallCoordinatorService _coordinatorService;
    
    public string CallId
    {
        get;
        set;
    } = "";

    public CallModel(CallCoordinatorService coordinatorService)
    {
        _coordinatorService = coordinatorService;
    }

    public IActionResult OnGet(string callId)
    {
        CallId = callId;

        ConferenceCall? call = _coordinatorService.GetCall(callId);

        if (call == null)
        {
            return NotFound();
        }
        
        return Page();
    }
}