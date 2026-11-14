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

## Spoonacular and local recipes

Local recipe and ingredient CRUD remains at `/api/Recipe` and `/api/Ingredient`. The separate `/api/spoonacular` routes provide upstream recipe search (`?query=pasta&number=1`), recipe details (`/{id}`), ingredients (`/{id}/ingredients`), and pantry matching (`/pantry`). Search counts must be between 1 and 100. These routes require an externally supplied key and use HTTPS with a redacted `x-api-key` header.

For local Development, save the key with .NET User Secrets for `Sep2026.SwanHtetSoe.RecipeAndPantryFinder`; its UserSecretsId identifies the store but contains no credential. Outside Development, configure `Spoonacular__ApiKey` through the environment or a secret provider. The base URL defaults to `https://api.spoonacular.com`. Mock coverage is included in `dotnet run --project tests/SecurityRegression`; that test does not call Spoonacular.

The PersonalityTest context defaults to Windows authentication for local `Sept2026Db`; override it with `ConnectionStrings__PersonalityTestDb` rather than embedding database credentials.
