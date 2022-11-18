using Bogus;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(e =>
{
    e.AddPolicy("Any", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

List<User> users = new List<User>
{
    new()
    {
        Username = "admin",
        Password = "admin",
        Role = "admin"
    },
    new()
    {
        Username = "guest",
        Password = "guest",
        Role = "guest"
    }
};

List<Data> data = new Faker<Data>()
    .RuleFor(e => e.FirstName, e => e.Name.FirstName())
    .RuleFor(e => e.LastName, e => e.Name.LastName())
    .RuleFor(e => e.Role, e => e.Name.JobTitle())
    .Generate(20);

app.UseCors("Any");

app.MapPost("/login", ([FromBody] LoginRequest request) =>
{
    var user = users.FirstOrDefault(e => e.Username == request.Username && e.Password == request.Password);
    if (user != null)
    {
        return Results.Ok(new {username = user.Username,role=user.Role});
    }

    return Results.BadRequest("Invalid credentials.");
});

app.MapGet("/data", async (context) =>
{
    if (!context.Request.Headers.TryGetValue("user-role", out var value))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    await context.Response.WriteAsJsonAsync(data);
});

app.MapGet("/admin", async (context) =>
{
    if (!context.Request.Headers.TryGetValue("user-role", out var value))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    await context.Response.WriteAsJsonAsync(users);
});
app.Run();


class Data
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; }
}

class User
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
}

class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}