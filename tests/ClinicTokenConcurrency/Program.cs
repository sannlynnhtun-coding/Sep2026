using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Sep2026.ClinicPatientRecord.API.Data;
using Sep2026.ClinicPatientRecord.API.Repositories;

// Run against a SQL Server login allowed to create/drop a temporary test database.
var server = new SqlConnectionStringBuilder(
    Environment.GetEnvironmentVariable("CLINIC_TEST_SQL")
    ?? "Server=.;Integrated Security=True;TrustServerCertificate=True") { InitialCatalog = "master" };
string database = "ClinicTokenTest_" + Guid.NewGuid().ToString("N");
await using var admin = new SqlConnection(server.ConnectionString);
await admin.OpenAsync();
await new SqlCommand($"CREATE DATABASE [{database}]", admin).ExecuteNonQueryAsync();
try
{
    var testConnection = new SqlConnectionStringBuilder(server.ConnectionString) { InitialCatalog = database };
    await using (var connection = new SqlConnection(testConnection.ConnectionString))
    {
        await connection.OpenAsync();
        await new SqlCommand("""
            CREATE TABLE dbo.Tokens (
                TokenID int IDENTITY PRIMARY KEY,
                PatientID int NOT NULL,
                DoctorID int NOT NULL,
                TokenNumber int NOT NULL,
                TokenDate date NOT NULL,
                CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
                UNIQUE(TokenDate, TokenNumber)
            );
            """, connection).ExecuteNonQueryAsync();
    }
    var configuration = new ConfigurationBuilder().AddInMemoryCollection(
        new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = testConnection.ConnectionString }).Build();
    var repository = new TokenRepository(new DatabaseConnection(configuration));
    var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var requests = Enumerable.Range(0, 16).Select(async _ =>
    {
        await start.Task;
        return await repository.GenerateAsync(1, 1);
    }).ToArray();
    start.SetResult();
    var tokens = await Task.WhenAll(requests);
    if (!tokens.Select(x => x.TokenNumber).Order().SequenceEqual(Enumerable.Range(1, 16)))
        throw new Exception("Concurrent requests did not produce consecutive, unique tokens.");

    await using (var connection = new SqlConnection(testConnection.ConnectionString))
    {
        await connection.OpenAsync();
        await new SqlCommand("UPDATE dbo.Tokens SET TokenNumber = 200 WHERE TokenNumber = 16", connection).ExecuteNonQueryAsync();
    }
    try
    {
        await repository.GenerateAsync(1, 1);
        throw new Exception("The daily token limit was not enforced.");
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("200")) { }
    Console.WriteLine("PASS: 16 simultaneous token requests and daily limit.");
}
finally
{
    SqlConnection.ClearAllPools();
    await new SqlCommand($"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]", admin).ExecuteNonQueryAsync();
}
