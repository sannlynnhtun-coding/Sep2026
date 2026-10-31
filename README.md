ADO.NET 
Dapper (Micro ORM)
EFCore (ORM)
	- Model First
	- Code First
	- Database First - split project 


dotnet ef dbcontext scaffold "Server=.;Database=Sep2026Db;User ID=sa;Password=sasa@123;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir AppDbContextModels --context AppDbContext --no-onconfiguring --force