using System.Data.SqlClient;
using Bogus;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

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
        Username = "user1",
        Password = "user1",
        Role = "guest"
    }
};

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(builder.Environment.ContentRootPath)
});

app.UseCors("Any");

app.MapPost("/login", ([FromBody] LoginRequest request) =>
{
    var user = users.FirstOrDefault(e => e.Username == request.Username && e.Password == request.Password);
    if (user != null)
    {
        return Results.Ok(new {username = user.Username, role = user.Role});
    }

    return Results.BadRequest("Invalid credentials.");
});

app.MapGet("/employees", async (context) =>
{
    if (!context.Request.Headers.TryGetValue("user-role", out var value))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    var connection = new SqlConnection(builder.Configuration.GetConnectionString("DbConnection"));

    if (context.Request.Query.TryGetValue("role", out var role))
        await context.Response.WriteAsJsonAsync(
            connection.Query<Employee>($"SELECT * FROM EMPLOYEES WHERE Role ='{role}'"));
    else
        await context.Response.WriteAsJsonAsync(connection.Query<Employee>("SELECT * FROM EMPLOYEES "));
});

app.MapGet("/titles", async (context) =>
{
    if (!context.Request.Headers.TryGetValue("user-role", out var value))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    var connection = new SqlConnection(builder.Configuration.GetConnectionString("DbConnection"));

    await context.Response.WriteAsJsonAsync(connection.Query<string>($"SELECT DISTINCT Role FROM EMPLOYEES "));
});


app.MapGet("/admin", async (context) =>
{
    if (!context.Request.Headers.TryGetValue("user-role", out var value) || value != "admin")
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    await context.Response.WriteAsJsonAsync(users);
});
app.Run();


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

//http://localhost:61898/home/Global%20Directives%20Liaison' update employees set lastname %3D 'test' where firstname %3D 'Jayden' --