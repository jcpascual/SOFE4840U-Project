using System.Security.Claims;
using Conference.Models;
using Conference.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

    private ILogger<LoginModel> _logger;
    private DatabaseService _databaseService;

    public LoginModel(ILogger<LoginModel> logger, DatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
    }
    
    public IActionResult OnGet(bool? fail)
    {
        LoginFailed = fail ?? false;
        
        return Page();
    }

    public async Task<IActionResult> OnPost(string username, string password)
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
        
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username)
        };

        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            IssuedUtc = DateTimeOffset.UtcNow
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);

        _logger.LogInformation("User {name} logged in at {Time}", user.Username, DateTime.UtcNow);

        return Content("OK");
    }
}