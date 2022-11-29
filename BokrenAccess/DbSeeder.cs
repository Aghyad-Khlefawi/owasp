using System.Data.SqlClient;
using Bogus;
using Dapper;

namespace BokrenAccess;

public class DbSeeder
{
    private readonly string _connectionString;

    public DbSeeder(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }


    public void Seed()
    {
        using var connection = new SqlConnection(_connectionString);

        CreateDatabase();


        var existing = (int) connection.ExecuteScalar("SELECT COUNT(*) FROM EMPLOYEES");
        if (existing > 0)
            return;

        var data = new Faker<Employee>()
            .RuleFor(e => e.FirstName, e => e.Name.FirstName().Replace("'", ""))
            .RuleFor(e => e.LastName, e => e.Name.LastName().Replace("'", ""))
            .RuleFor(e => e.Role, e => e.Name.JobTitle())
            .Generate(20);


        foreach (var employee in data)
        {
            connection.Execute(
                $"INSERT INTO EMPLOYEES (FirstName,LastName,Role) VALUES ('{employee.FirstName}','{employee.LastName}','{employee.Role}')");
        }
    }

    private void CreateDatabase()
    {
        var connectionString = _connectionString.Replace("employeesDb", "master");
        using var connection = new SqlConnection(connectionString);
        var dbExist = connection.ExecuteScalar("SELECT db_id(N'employeesDb') ");
        if (dbExist == null)
        {
            connection.Execute("create database employeesDb");
            connection.Execute(
                "use employeesDb; create table EMPLOYEES(Id UNIQUEIDENTIFIER not null default(newid()),FirstName NVARCHAR(max) not null,LastName NVARCHAR(max) not null,Role NVARCHAR(max) not null)");
        }
    }
}