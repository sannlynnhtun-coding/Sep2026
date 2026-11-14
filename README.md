ADO.NET 
Dapper (Micro ORM)
EFCore (ORM)
	- Model First
	- Code First
	- Database First - split project 


dotnet ef dbcontext scaffold "Server=.;Database=Sep2026Db;Integrated Security=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir AppDbContextModels --context AppDbContext --no-onconfiguring --force
Database samples use Windows integrated authentication by default. Set `ConnectionStrings__DefaultConnection` for ConsoleApp3, ConsoleApp4, and ProjectIdea1, or `ConnectionStrings__Sep2026Db` for RecipeAndPantryFinder, to supply an external connection string. The recipe API also requires `Spoonacular__ApiKey`; keep this in the environment or a secret store, not tracked configuration. Previously published credentials must be rotated by their owners.

The Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi also requires `ConnectionStrings__DefaultConnection`. Supply it through the environment or a secret store with a least-privileged SQL login; do not put credentials in tracked configuration.

## Local training security boundary

These unauthenticated APIs are local training samples. Bind them only to localhost, use disposable training data and a least-privileged SQL login, and do not expose their ports through a public host, tunnel, reverse proxy, or container port mapping. Authentication and authorization are required before any public deployment. Development exception pages and the clinic database diagnostic endpoint are for local development only.

SQL values must be bound as parameters rather than interpolated into command text. The JSON answer importer accepts an optional input-file argument for testing: `dotnet run --project Sep2026.ProjectIdea1 -- path/to/answers.json`.

Run `dotnet run --project tests/SecurityRegression` to check SQL injection payloads against a newly created temporary SQL database and recipe credential/error handling against a mock HTTP handler. The SQL identity must be allowed to create/drop test databases; the test removes its database afterward. Run `dotnet list Sep2026.slnx package --vulnerable --include-transitive` for the root solution's dependency audit. The additional ReaderClub, Battleship, and Clinic projects should also be audited when changed because they have separate project/solution files.

Recipe requests send the external API key in the redacted `x-api-key` header, require an HTTPS upstream, and do not follow redirects. See the [Spoonacular authentication documentation](https://spoonacular.com/food-api/docs#Authentication). The SQL client patch also updates the vulnerable transitive cryptography dependency; see [Microsoft's advisory](https://github.com/advisories/GHSA-2fmv-rv2c-59x7).
