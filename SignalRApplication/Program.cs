using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using SignalRApplication;
using System.Data;
using System.Security.Claims;

var adminRole = new Role("admin");
var userRole = new Role("user");
var people = new List<Person>()
{
    new Person("admin@gmail.com", "12345", adminRole),
    new Person("user@gmail.com", "54321", userRole)
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/login");
builder.Services.AddAuthorization();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", async context =>
    await SendHtmlAsync(context, "html/login.html"));

app.MapPost("/login", async (string? returnUrl, HttpContext context) =>
{
    var form = context.Request.Form;

    if (!form.ContainsKey("email") || !form.ContainsKey("password"))
        return Results.BadRequest("Email и/или пароль не установлены");
    string email = form["email"];
    string password = form["password"];

    Person? person = people.FirstOrDefault(p => p.Email == email && p.Password == password);

    if (person is null) return Results.Unauthorized();

    var claims = new List<Claim>
    {
        new Claim(ClaimsIdentity.DefaultNameClaimType, person.Email),
        new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role.Name)
    };
    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    await context.SignInAsync(claimsPrincipal);
    return Results.Redirect(returnUrl ?? "/");
});

app.MapGet("/", [Authorize] async (HttpContext context) =>
    await SendHtmlAsync(context, "html/index.html"));

app.MapGet("/admin", [Authorize(Roles = "admin")] async (HttpContext context) =>
    await SendHtmlAsync(context, "html/admin.html"));


app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapHub<ChatHub>("/chat");
app.Run();

async Task SendHtmlAsync(HttpContext context, string path)
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(path);
}

record class Person(string Email, string Password, Role Role);
record class Role(string Name);
