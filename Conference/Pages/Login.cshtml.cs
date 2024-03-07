using Conference.Models;
using Conference.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Conference.Pages;

public class LoginModel : PageModel
{
    public bool LoginFailed
    {
        get;
        set;
    }
    
    private DatabaseService _databaseService;

    public LoginModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }
    
    public IActionResult OnGet(bool? fail)
    {
        LoginFailed = fail ?? false;
        
        return Page();
    }

    public IActionResult OnPost(string username, string password)
    {
        ConferenceUser? user = _databaseService.GetUser(username);

        if (user == null)
        {
            return Redirect("/login?fail=true");
        }

        if (password != user.Password)
        {
            return Redirect("/login?fail=true");
        }

        return Content("OK");
    }
}