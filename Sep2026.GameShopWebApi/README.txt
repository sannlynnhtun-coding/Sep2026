Database setup

Use Windows integrated authentication locally, or set ConnectionStrings__GameShopDb outside source control.

dotnet ef dbcontext scaffold "Name=ConnectionStrings:GameShopDb" Microsoft.EntityFrameworkCore.SqlServer --output-dir GameShopAppDbContext --context GameShopAppDbContext --no-onconfiguring --force
