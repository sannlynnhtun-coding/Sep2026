using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sep2026.ConsoleApp3;
using Sep2026.ReceipeDb.Models;

string[] keys = ["ConnectionStrings__DefaultConnection", "ConnectionStrings__Sep2026Db"];
var original = keys.Select(Environment.GetEnvironmentVariable).ToArray();
try
{
    foreach (var key in keys) Environment.SetEnvironmentVariable(key, null);
    Check(".");
    foreach (var key in keys)
        Environment.SetEnvironmentVariable(key, "Server=configured;Database=Regression;Integrated Security=True;");
    Check("configured");
    using var injected = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=injected;Database=Regression;Integrated Security=True;").Options);
    if (new SqlConnectionStringBuilder(injected.Database.GetConnectionString()).DataSource != "injected")
        throw new Exception("Injected database options were overridden.");
    Console.WriteLine("PASS: credential-free defaults, environment overrides, and injected configuration.");
}
finally
{
    for (int i = 0; i < keys.Length; i++) Environment.SetEnvironmentVariable(keys[i], original[i]);
}

static void Check(string server)
{
    foreach (object service in new object[] { new StudentService(), new GradeService() })
    {
        var connection = (SqlConnectionStringBuilder)service.GetType()
            .GetField("sqlConnectionStringBuilder", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(service)!;
        AssertConnection(connection, server);
    }
    using var context = new AppDbContext();
    AssertConnection(new SqlConnectionStringBuilder(context.Database.GetConnectionString()), server);
}

static void AssertConnection(SqlConnectionStringBuilder connection, string server)
{
    if (!connection.IntegratedSecurity || connection.Password.Length != 0 || connection.DataSource != server)
        throw new Exception("Unexpected database authentication configuration.");
}
