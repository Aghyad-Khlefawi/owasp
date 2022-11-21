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
        Username = "guest",
        Password = "guest",
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
    var data = connection.Query<Employee>("SELECT * FROM EMPLOYEES ");
    await context.Response.WriteAsJsonAsync(data);
});

app.MapGet("/employees/{id}", async (context) =>
{
    if (!context.Request.Headers.TryGetValue("user-role", out var value))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    var connection = new SqlConnection(builder.Configuration.GetConnectionString("DbConnection"));
    var id = context.Request.Query["id"].ToString();
    await context.Response.WriteAsJsonAsync(connection.QueryFirst<Employee>($"SELECT * FROM EMPLOYEES WHERE ID ={id}"));
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