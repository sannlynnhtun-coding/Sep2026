using Dapper;
using Microsoft.Data.SqlClient;
using Sep2026.AyeChan.ExpenseTracker.ConsoleApp.Models;

namespace Sep2026.AyeChan.ExpenseTracker.ConsoleApp;

public class DapperService
{
    private readonly string connectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=ExpenseTrackerDb;Integrated Security=True;TrustServerCertificate=True;";

    public void Read()
    {
        // Open connection
        using var connection = new SqlConnection(connectionString);

        // Select active categories only
        string query = @"SELECT [CategoryId], [CategoryName], [DeleteFlag]
                         FROM [dbo].[Tbl_Category]
                         WHERE [DeleteFlag] = 0
                         ORDER BY [CategoryId]";

        var categories = connection.Query<CategoryModel>(query).ToList();

        // Print results
        Console.WriteLine("----- Categories -----");
        foreach (var item in categories)
        {
            Console.WriteLine($"{item.CategoryId}. {item.CategoryName}");
        }
    }

    public void Detail(int id)
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
            Console.WriteLine("No data found.");
            return;
        }

        Console.WriteLine($"Id: {category.CategoryId}, Name: {category.CategoryName}");
    }

    public void Create(string categoryName)
    {
        // Clean input once and reuse it
        var name = categoryName?.Trim() ?? string.Empty;

        // Validate input
        if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
        {
            Console.WriteLine("Category name is required (max 50 characters).");
            return;
        }

        using var connection = new SqlConnection(connectionString);

        // Duplicate Category check
        string duplicateQuery = @"SELECT COUNT(*) FROM [dbo].[Tbl_Category] WHERE [CategoryName] = @CategoryName AND [DeleteFlag] = 0";
        int duplicateCount = connection.ExecuteScalar<int>(duplicateQuery, new { CategoryName = name });

        if (duplicateCount > 0)
        {
            Console.WriteLine("Category name already exists.");
            return;
        }

        // Insert new category
        string query = @"INSERT INTO [dbo].[Tbl_Category] ([CategoryName], [DeleteFlag])
                         VALUES (@CategoryName, 0)";

        int result = connection.Execute(query, new { CategoryName = name });

        string message = result > 0 ? "Saving Successful." : "Saving Failed.";
        Console.WriteLine(message);
    }

    public void Update(int id, string categoryName)
    {
        // Clean input once and reuse it
        var name = categoryName?.Trim() ?? string.Empty;

        // Validate input
        if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
        {
            Console.WriteLine("Category name is required (max 50 characters).");
            return;
        }

        using var connection = new SqlConnection(connectionString);
        // Duplicate Category check 
        // Duplicate check: exclude the category being updated
        string duplicateQuery = @"SELECT COUNT(*)
                          FROM [dbo].[Tbl_Category]
                          WHERE [CategoryName] = @CategoryName
                            AND [DeleteFlag] = 0
                            AND [CategoryId] <> @CategoryId";

        int duplicateCount = connection.ExecuteScalar<int>(duplicateQuery,
            new { CategoryId = id, CategoryName = name });

        if (duplicateCount > 0)
        {
            Console.WriteLine("Category name already exists.");
            return;
        }

        // Update only if the category is still active
        string query = @"UPDATE [dbo].[Tbl_Category]
                         SET [CategoryName] = @CategoryName
                         WHERE [CategoryId] = @CategoryId AND [DeleteFlag] = 0";

        int result = connection.Execute(query, new { CategoryId = id, CategoryName = name });

        string message = result > 0 ? "Updating Successful." : "Updating Failed.";
        Console.WriteLine(message);
    }

    public void Delete(int id)
    {
        using var connection = new SqlConnection(connectionString);

        // Business rule: block delete if the category still has active expenses
        string checkQuery = @"SELECT COUNT(*)
                              FROM [dbo].[Tbl_Expense]
                              WHERE [CategoryId] = @CategoryId AND [DeleteFlag] = 0";

        int expenseCount = connection.ExecuteScalar<int>(checkQuery, new { CategoryId = id });

        if (expenseCount > 0)
        {
            Console.WriteLine($"Cannot delete: this category has {expenseCount} active expense(s).");
            return;
        }

        // Soft delete
        string query = @"UPDATE [dbo].[Tbl_Category]
                         SET [DeleteFlag] = 1
                         WHERE [CategoryId] = @CategoryId AND [DeleteFlag] = 0";

        int result = connection.Execute(query, new { CategoryId = id });

        string message = result > 0 ? "Deleting Successful." : "Deleting Failed.";
        Console.WriteLine(message);
    }
}