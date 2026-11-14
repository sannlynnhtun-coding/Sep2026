The sample database context defaults to Windows authentication against the local `Sep2026Db` database. Set `ConnectionStrings__DefaultConnection` to override the connection without storing credentials in source. Explicitly injected context options take precedence.

Run `dotnet run --project tests/BuildWarningRegression` from the repository root to check configuration and nullable student ClassNo handling. This requires a local SQL Server identity allowed to create and drop a uniquely named temporary test database.
