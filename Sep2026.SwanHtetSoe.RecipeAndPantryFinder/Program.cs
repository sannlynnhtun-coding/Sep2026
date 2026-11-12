using Microsoft.EntityFrameworkCore;
using Sep2026.RecipeAndPantryFinder.Services;
using Sep2026.ReceipeDb.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Sep2026Db")));

builder.Services.AddHttpClient<IRecipeService, RecipeService>();

builder.Services.AddScoped<IPantryService, PantryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
