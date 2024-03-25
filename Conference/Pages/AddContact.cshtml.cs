using Conference.Models;
using Conference.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Conference.Pages;

[Authorize]
public class AddContactModel : PageModel
{
    public enum AddContactResult
    {
        None,
        NotFound,
        AlreadyExists,
        Success
    }
    
    public AddContactResult ContactResult
    {
        get;
        set;
    }
    
    private DatabaseService _databaseService;

    public AddContactModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }
    
    public IActionResult OnGet(AddContactResult? result)
    {
        ContactResult = result ?? AddContactResult.None;
        
        return Page();
    }

    public IActionResult OnPost(string username)
    {
        ConferenceUser? potentialUser = _databaseService.GetUser(username);

        if (potentialUser == null)
        {
            return Redirect("/add-contact?result=" + AddContactResult.NotFound);
        }

        ConferenceUser thisUser = _databaseService.GetUser(HttpContext.User!.Identity!.Name!)!;

        List<ConferenceContact> contacts = _databaseService.GetContactsForUser(thisUser);

        if (contacts.Exists(c => c.TargetId == potentialUser.Id))
        {
            return Redirect("/add-contact?result=" + AddContactResult.AlreadyExists);
        }
        
        _databaseService.InsertContact(thisUser, potentialUser);
        
        return Redirect("/add-contact?result=" + AddContactResult.Success);
    }
}