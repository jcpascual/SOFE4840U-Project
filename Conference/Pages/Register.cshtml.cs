using Conference.Models;
using Conference.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Conference.Pages;

public class RegisterModel : PageModel
{
    public enum RegisterResult
    {
        None,
        UsernameTaken,
        Success
    }

    public RegisterResult RegistrationResult
    {
        get;
        set;
    }
    
    private DatabaseService _databaseService;

    public RegisterModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }
    
    public IActionResult OnGet(RegisterResult? result)
    {
        RegistrationResult = result ?? RegisterResult.None;
        
        return Page();
    }

    public IActionResult OnPost(string username, string password)
    {
        ConferenceUser? potentialUser = _databaseService.GetUser(username);

        if (potentialUser != null)
        {
            return Redirect("/register?result=" + RegisterResult.UsernameTaken);
        }
        
        _databaseService.InsertUser(username, password);
        
        return Redirect("/register?result=" + RegisterResult.Success);
    }
}