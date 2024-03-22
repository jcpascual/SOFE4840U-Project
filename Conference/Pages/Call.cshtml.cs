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

    public ConferenceCall Call
    {
        get;
        set;
    }

    public CallModel(CallCoordinatorService coordinatorService)
    {
        _coordinatorService = coordinatorService;
    }

    public IActionResult OnGet(string callId)
    {
        ConferenceCall? call = _coordinatorService.GetCall(callId);

        if (call == null)
        {
            return NotFound();
        }

        Call = call;
        
        return Page();
    }
}