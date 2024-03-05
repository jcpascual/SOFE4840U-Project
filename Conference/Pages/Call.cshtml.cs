using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Conference.Pages;

public class CallModel : PageModel
{
    public string CallId
    {
        get;
        set;
    } = "";

    public IActionResult OnGet(string callId)
    {
        CallId = callId;
        
        return Page();
    }
}