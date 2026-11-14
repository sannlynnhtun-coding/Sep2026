using Microsoft.EntityFrameworkCore;
using Sep2026.ZodiacHoroscope.Database.AppDbContextModels;

var original = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
try
{
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
    using var defaults = new AppDbContext();
    if (!defaults.Database.GetConnectionString()!.Contains("Integrated Security=True"))
        throw new Exception("Default connection must not use a committed password.");
    const string configured = "Server=configured;Database=Regression;Integrated Security=True";
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", configured);
    using var environment = new AppDbContext();
    if (new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(environment.Database.GetConnectionString()).DataSource != "configured")
        throw new Exception("Environment connection was ignored.");
    var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=injected;Database=Regression;Integrated Security=True").Options;
    using var injected = new AppDbContext(options);
    if (new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(injected.Database.GetConnectionString()).DataSource != "injected")
        throw new Exception("Injected configuration was overridden.");
}
finally
{
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", original);
}
Console.WriteLine("Regression checks passed.");
