using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using BokrenAccess;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureServices((context, services) =>
{
    new DbSeeder(context.Configuration.GetConnectionString("DbConnection")).Seed();
});

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
app.UseCors("Any");


// Setup static file serving to download employees csv
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(builder.Environment.ContentRootPath)
});


// Login process
app.MapPost("/login", ([FromBody] LoginRequest request) =>
{
    var user = User.Users.FirstOrDefault(e => e.Username == request.Username && e.PasswordHash == Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(request.Password))));
    if (user != null)
    {
        return Results.Ok(new {username = user.Username, role = user.Role});
    }

    return Results.BadRequest("Invalid credentials.");
});


// Employee listing
app.MapGet("/employees", async (context) =>
{
    if (!context.Request.Headers.TryGetValue("user-role", out var value))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    await using var connection = new SqlConnection(builder.Configuration.GetConnectionString("DbConnection"));

    if (context.Request.Query.TryGetValue("role", out var role))
        await context.Response.WriteAsJsonAsync(
            connection.Query<Employee>($"SELECT * FROM EMPLOYEES WHERE Role ='{role}'"));
    else
        await context.Response.WriteAsJsonAsync(connection.Query<Employee>("SELECT * FROM EMPLOYEES "));
});

// Employee titles dropdown
app.MapGet("/titles", async (context) =>
{
    if (!context.Request.Headers.TryGetValue("user-role", out var value))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    await using var connection = new SqlConnection(builder.Configuration.GetConnectionString("DbConnection"));

    await context.Response.WriteAsJsonAsync(connection.Query<string>($"SELECT DISTINCT Role FROM EMPLOYEES "));
});

// Users listing
app.MapGet("/admin", async (context) =>
{   
    if (!context.Request.Headers.TryGetValue("user-role", out var value) || value != "admin")
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    await context.Response.WriteAsJsonAsync(User.Users);
});

app.Run();



//http://localhost:61898/home/Global%20Directives%20Liaison' update employees set lastname %3D 'test' where firstname %3D 'Jayden' --