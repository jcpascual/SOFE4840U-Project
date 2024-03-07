using Conference.Hubs;
using Conference.Services;
using MySqlConnector;
using MySqlConnector.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

services.AddMySqlDataSource(builder.Configuration.GetConnectionString("Default")!);

services.AddSingleton<CallCoordinatorService>();
services.AddSingleton<DatabaseService>();

services.AddRazorPages();
services.AddSignalR();

var app = builder.Build();

MySqlConnectorLogManager.Provider =
    new MicrosoftExtensionsLoggingLoggerProvider(app.Services.GetRequiredService<ILoggerFactory>());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapHub<CallHub>("/hubs/call");

app.Run();