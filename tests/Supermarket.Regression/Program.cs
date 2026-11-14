using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;
using Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Controllers;
using Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Filters;

string database = "SupermarketRegression_" + Guid.NewGuid().ToString("N");
string connection = $"Server=.;Database={database};Integrated Security=True;TrustServerCertificate=True;";
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection).Options;
using var db = new AppDbContext(options);
var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Logging.ClearProviders();
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connection));
builder.Services.AddControllers(o => o.Filters.Add<DatabaseConflictFilter>()).AddApplicationPart(typeof(SaleController).Assembly);
builder.Services.AddProblemDetails();
await using var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
try
{
    await db.Database.EnsureCreatedAsync();
    await app.StartAsync();
    using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()), Timeout = TimeSpan.FromSeconds(15) };
    await Send(HttpMethod.Post, "/api/Category", new { categoryName = "Food" }, HttpStatusCode.OK);
    int categoryId = await db.Categories.Select(x => x.CategoryId).SingleAsync();
    await Send(HttpMethod.Post, "/api/Category", new { categoryName = "Food" }, HttpStatusCode.Conflict);
    await Send(HttpMethod.Post, "/api/Category", new { categoryName = " " }, HttpStatusCode.BadRequest);
    await Send(HttpMethod.Post, "/api/Category", new { categoryName = new string('x', 101) }, HttpStatusCode.BadRequest);
    await Send(HttpMethod.Post, "/api/Employee", new { firstName = "Employee", phone = "555" }, HttpStatusCode.OK);
    int employeeId = await db.Employees.Select(x => x.EmployeeId).SingleAsync();
    await Send(HttpMethod.Post, "/api/Customer", new { firstName = "Customer", phone = "111" }, HttpStatusCode.OK);
    int customerId = await db.Customers.Select(x => x.CustomerId).SingleAsync();
    Assert(await db.Customers.AsNoTracking().Select(x => x.Phone).SingleAsync() == "111", "Customer phone was not created.");
    await Send(HttpMethod.Put, $"/api/Customer/{customerId}", new { firstName = "Customer", phone = "222" }, HttpStatusCode.OK);
    Assert(await db.Customers.AsNoTracking().Select(x => x.Phone).SingleAsync() == "222", "Customer phone was not updated.");

    object ProductBody(int parent, string barcode = "ABC", decimal price = 2) => new
    {
        barcode, productName = "Rice", categoryId = parent, purchasePrice = price, sellingPrice = price, quantityInStock = 5
    };
    await Send(HttpMethod.Post, "/api/Product", ProductBody(int.MaxValue), HttpStatusCode.NotFound);
    await Send(HttpMethod.Post, "/api/Product", ProductBody(categoryId, price: -1), HttpStatusCode.BadRequest);
    await Send(HttpMethod.Post, "/api/Product", ProductBody(categoryId), HttpStatusCode.OK);
    int productId = await db.Products.Select(x => x.ProductId).SingleAsync();
    await Send(HttpMethod.Post, "/api/Product", ProductBody(categoryId), HttpStatusCode.Conflict);
    await Send(HttpMethod.Put, $"/api/Product/{productId}", ProductBody(int.MaxValue, "CHANGED"), HttpStatusCode.NotFound);
    Assert(await db.Products.AsNoTracking().Select(x => x.Barcode).SingleAsync() == "ABC", "Invalid parent changed product data.");

    object SaleBody(string saleNumber, int employee, int? customer, decimal total = 2) => new
    {
        saleNumber, employeeId = employee, customerId = customer, subtotal = 2, discountAmount = 0, taxAmount = 0, totalAmount = total
    };
    await Send(HttpMethod.Post, "/api/Sale", SaleBody("BAD-E", int.MaxValue, customerId), HttpStatusCode.NotFound);
    await Send(HttpMethod.Post, "/api/Sale", SaleBody("BAD-C", employeeId, int.MaxValue), HttpStatusCode.NotFound);
    await Send(HttpMethod.Post, "/api/Sale", SaleBody("NEGATIVE", employeeId, customerId, -1), HttpStatusCode.BadRequest);
    await Send(HttpMethod.Post, "/api/Sale", SaleBody("SALE-1", employeeId, customerId), HttpStatusCode.OK);
    await Send(HttpMethod.Post, "/api/Sale", SaleBody("SALE-1", employeeId, customerId), HttpStatusCode.Conflict);
    long saleId = await db.Sales.Select(x => x.SaleId).SingleAsync();
    db.SaleItems.Add(new SaleItem { SaleId = saleId, ProductId = productId, Quantity = 1, UnitPrice = 2 });
    await db.SaveChangesAsync();

    await Send(HttpMethod.Delete, $"/api/Category/{categoryId}", null, HttpStatusCode.OK);
    await Send(HttpMethod.Get, $"/api/Category/{categoryId}", null, HttpStatusCode.NotFound);
    Assert(await db.Categories.AsNoTracking().Where(x => x.CategoryId == categoryId).Select(x => x.DeleteFlag).SingleAsync(), "Category was physically deleted.");
    await Send(HttpMethod.Post, "/api/Product", ProductBody(categoryId, "NEW"), HttpStatusCode.NotFound);
    await Send(HttpMethod.Delete, $"/api/Customer/{customerId}", null, HttpStatusCode.OK);
    await Send(HttpMethod.Post, "/api/Sale", SaleBody("DELETED-C", employeeId, customerId), HttpStatusCode.NotFound);
    await Send(HttpMethod.Delete, $"/api/Employee/{employeeId}", null, HttpStatusCode.OK);
    await Send(HttpMethod.Post, "/api/Sale", SaleBody("DELETED-E", employeeId, null), HttpStatusCode.NotFound);
    await Send(HttpMethod.Delete, $"/api/Product/{productId}", null, HttpStatusCode.OK);
    Assert(await db.Products.AsNoTracking().Where(x => x.ProductId == productId).Select(x => x.DeleteFlag).SingleAsync(), "Product was physically deleted.");
    Assert(await db.Employees.AsNoTracking().Where(x => x.EmployeeId == employeeId).Select(x => x.DeleteFlag).SingleAsync(), "Employee was physically deleted.");
    Assert(await db.Customers.AsNoTracking().Where(x => x.CustomerId == customerId).Select(x => x.DeleteFlag).SingleAsync(), "Customer was physically deleted.");
    Assert(await db.Sales.CountAsync() == 1 && await db.SaleItems.CountAsync() == 1, "Sale history was lost.");
    await Send(HttpMethod.Get, $"/api/Sale/{saleId}", null, HttpStatusCode.OK);

    // Filtered unique indexes permit reuse after soft deletion, but protect concurrent active writes.
    await Send(HttpMethod.Post, "/api/Category", new { categoryName = "Food" }, HttpStatusCode.OK);
    int activeCategory = await db.Categories.Where(x => !x.DeleteFlag).Select(x => x.CategoryId).SingleAsync();
    await Send(HttpMethod.Post, "/api/Product", ProductBody(activeCategory), HttpStatusCode.OK);
    var race = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => Send(HttpMethod.Post, "/api/Category", new { categoryName = "Race" })));
    Assert(race.Count(x => x == HttpStatusCode.OK) == 1 && race.Count(x => x == HttpStatusCode.Conflict) == 1, "Concurrent duplicate writes were not handled.");
    int raceId = await db.Categories.Where(x => x.CategoryName == "Race").Select(x => x.CategoryId).SingleAsync();
    await Send(HttpMethod.Put, $"/api/Category/{raceId}", new { categoryName = "Food" }, HttpStatusCode.Conflict);
    Console.WriteLine("PASS: supermarket HTTP validation, active parents, phone persistence, soft-delete history, and unique-key conflicts.");

    async Task<HttpStatusCode> Send(HttpMethod method, string path, object? payload, HttpStatusCode? expected = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (payload != null) request.Content = JsonContent.Create(payload);
        using var response = await client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();
        Assert(expected == null || response.StatusCode == expected, $"{method} {path}: expected {expected}, got {response.StatusCode}.");
        Assert(!body.Contains("SqlException") && !body.Contains("DbUpdateException") && !body.Contains("UX_"), "SQL details leaked.");
        return response.StatusCode;
    }
}
finally
{
    await app.StopAsync();
    await db.Database.EnsureDeletedAsync();
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
