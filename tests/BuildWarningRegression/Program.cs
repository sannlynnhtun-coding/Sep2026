using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sep2026.ConsoleApp5.Database.AppDbContextModels;
using Sep2026.WebApi.Controllers;
using Sep2026.WebApi.Models;

var original = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
try
{
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
    using var defaults = new AppDbContext();
    if (!new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(defaults.Database.GetConnectionString()).IntegratedSecurity)
        throw new Exception("Default connection must use integrated authentication.");

    string database = "BuildWarningRegression_" + Guid.NewGuid().ToString("N");
    string connection = $"Server=.;Database={database};Integrated Security=True;TrustServerCertificate=True;";
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connection);
    using var db = new AppDbContext();
    if (db.Database.GetConnectionString() != connection)
        throw new Exception("Environment configuration was ignored.");
    using var injected = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=injected;Database=Test;Integrated Security=True").Options);
    if (new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(injected.Database.GetConnectionString()).DataSource != "injected")
        throw new Exception("Injected configuration was overwritten.");
    try
    {
        db.Database.EnsureCreated();
        var controller = new StudentController();
        if (controller.CreateStudent(new StudentCreateRequestModel { No = "1", Name = "Test" }) is not OkObjectResult)
            throw new Exception("Creating a student without ClassNo failed.");
        var student = db.Students.Single();
        if (student.ClassNo != null)
            throw new Exception("Optional ClassNo must remain null.");
        if (controller.UpsertStudent(student.Id, new StudentUpsertRequestModel { No = "2", Name = "Updated" }) is not OkObjectResult)
            throw new Exception("Updating a student without ClassNo failed.");
        db.Entry(student).Reload();
        if (student.No != "2" || student.ClassNo != null)
            throw new Exception("Student update did not preserve optional ClassNo.");
    }
    finally
    {
        db.Database.EnsureDeleted();
    }
    Console.WriteLine("PASS: safe connection configuration and nullable ClassNo create/update.");
}
finally
{
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", original);
}
