ADO.NET 
Dapper (Micro ORM)
EFCore (ORM)
	- Model First
	- Code First
	- Database First - split project 


dotnet ef dbcontext scaffold "Server=.;Database=Sep2026Db;Integrated Security=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir AppDbContextModels --context AppDbContext --no-onconfiguring --force
Database samples use Windows integrated authentication by default. Set `ConnectionStrings__DefaultConnection` for ConsoleApp3, ConsoleApp4, and ProjectIdea1, or `ConnectionStrings__Sep2026Db` for RecipeAndPantryFinder, to supply an external connection string. The recipe API also requires `Spoonacular__ApiKey`; keep this in the environment or a secret store, not tracked configuration. Previously published credentials must be rotated by their owners.
