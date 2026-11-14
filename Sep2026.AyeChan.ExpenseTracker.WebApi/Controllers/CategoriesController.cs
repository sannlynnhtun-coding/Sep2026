using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Sep2026.AyeChan.ExpenseTracker.WebApi.Models;

namespace Sep2026.AyeChan.ExpenseTracker.WebApi.Controllers;

// api/categories
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly string connectionString;

    public CategoriesController(IConfiguration configuration)
    {
        // Read connection string from appsettings.json
        connectionString = configuration.GetConnectionString("DbConnection")!;
    }

    // GET api/categories
    [HttpGet]
    public IActionResult Read()
    {
        using var connection = new SqlConnection(connectionString);

        // Select active categories only
        string query = @"SELECT [CategoryId], [CategoryName], [DeleteFlag]
                         FROM [dbo].[Tbl_Category]
                         WHERE [DeleteFlag] = 0
                         ORDER BY [CategoryId]";

        var categories = connection.Query<CategoryModel>(query).ToList();
        return Ok(categories);
    }

    // GET api/categories/1
    [HttpGet("{id}")]
    public IActionResult Detail(int id)
    {
        using var connection = new SqlConnection(connectionString);

        // Get one active category by id
        string query = @"SELECT [CategoryId], [CategoryName], [DeleteFlag]
                         FROM [dbo].[Tbl_Category]
                         WHERE [CategoryId] = @CategoryId AND [DeleteFlag] = 0";

        var category = connection.QueryFirstOrDefault<CategoryModel>(query, new { CategoryId = id });

        // Null check
        if (category is null)
        {
            return NotFound("No data found.");
        }

        return Ok(category);
    }

    // POST api/categories
    [HttpPost]
    public IActionResult Create(CategoryRequestModel requestModel)
    {
        // Clean input once and reuse it
        var name = requestModel.CategoryName?.Trim() ?? string.Empty;

        // Validate input
        if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
        {
            return BadRequest("Category name is required (max 50 characters).");
        }

        using var connection = new SqlConnection(connectionString);

        // Duplicate check: active categories only
        string duplicateQuery = @"SELECT COUNT(*)
                                  FROM [dbo].[Tbl_Category]
                                  WHERE [CategoryName] = @CategoryName
                                    AND [DeleteFlag] = 0";

        int duplicateCount = connection.ExecuteScalar<int>(duplicateQuery, new { CategoryName = name });
        if (duplicateCount > 0)
        {
            return Conflict("Category name already exists.");
        }

        // Insert new category
        string query = @"INSERT INTO [dbo].[Tbl_Category] ([CategoryName], [DeleteFlag])
                         VALUES (@CategoryName, 0)";

        int result = connection.Execute(query, new { CategoryName = name });

        string message = result > 0 ? "Saving Successful." : "Saving Failed.";
        return result > 0 ? Ok(message) : BadRequest(message);
    }

    // PUT api/categories/1
    [HttpPut("{id}")]
    public IActionResult Update(int id, CategoryRequestModel requestModel)
    {
        // Clean input once and reuse it
        var name = requestModel.CategoryName?.Trim() ?? string.Empty;

        // Validate input
        if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
        {
            return BadRequest("Category name is required (max 50 characters).");
        }

        using var connection = new SqlConnection(connectionString);

        // Duplicate check: exclude the category being updated
        string duplicateQuery = @"SELECT COUNT(*)
                                  FROM [dbo].[Tbl_Category]
                                  WHERE [CategoryName] = @CategoryName
                                    AND [DeleteFlag] = 0
                                    AND [CategoryId] <> @CategoryId";

        int duplicateCount = connection.ExecuteScalar<int>(duplicateQuery, new { CategoryId = id, CategoryName = name });
        if (duplicateCount > 0)
        {
            return Conflict("Category name already exists.");
        }

        // Update only if the category is still active
        string query = @"UPDATE [dbo].[Tbl_Category]
                         SET [CategoryName] = @CategoryName
                         WHERE [CategoryId] = @CategoryId AND [DeleteFlag] = 0";

        int result = connection.Execute(query, new { CategoryId = id, CategoryName = name });

        string message = result > 0 ? "Updating Successful." : "Updating Failed.";
        return result > 0 ? Ok(message) : NotFound(message);
    }

    // DELETE api/categories/1
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        using var connection = new SqlConnection(connectionString);

        // Business rule: block delete if the category still has active expenses
        string checkQuery = @"SELECT COUNT(*)
                              FROM [dbo].[Tbl_Expense]
                              WHERE [CategoryId] = @CategoryId AND [DeleteFlag] = 0";

        int expenseCount = connection.ExecuteScalar<int>(checkQuery, new { CategoryId = id });
        if (expenseCount > 0)
        {
            return Conflict($"Cannot delete: this category has {expenseCount} active expense(s).");
        }

        // Soft delete
        string query = @"UPDATE [dbo].[Tbl_Category]
                         SET [DeleteFlag] = 1
                         WHERE [CategoryId] = @CategoryId AND [DeleteFlag] = 0";

        int result = connection.Execute(query, new { CategoryId = id });

        string message = result > 0 ? "Deleting Successful." : "Deleting Failed.";
        return result > 0 ? Ok(message) : NotFound(message);
    }
}