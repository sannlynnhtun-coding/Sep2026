using Dapper;
using Microsoft.Data.SqlClient;
using Sep2026.AyeChan.ExpenseTracker.ConsoleApp.Models;

namespace Sep2026.AyeChan.ExpenseTracker.ConsoleApp;

public class ReportService
{
    // Tutorial shortcut: hardcoded connection string (moved to appsettings.json later)
    private readonly string connectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=ExpenseTrackerDb;Integrated Security=True;TrustServerCertificate=True;";

    public void MonthlyReport(int year, int month)
    {
        // Validate input
        if (month < 1 || month > 12)
        {
            Console.WriteLine("Month must be between 1 and 12.");
            return;
        }

        // Date range for the month: [startDate, endDate)
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        using var connection = new SqlConnection(connectionString);

        // Budget vs actual per active category
        string query = @"SELECT c.[CategoryName],
                                ISNULL(b.[Amount], 0) AS [BudgetAmount],
                                ISNULL(e.[SpentAmount], 0) AS [SpentAmount],
                                ISNULL(b.[Amount], 0) - ISNULL(e.[SpentAmount], 0) AS [RemainingAmount]
                         FROM [dbo].[Tbl_Category] c
                         LEFT JOIN [dbo].[Tbl_Budget] b
                                ON b.[CategoryId] = c.[CategoryId]
                               AND b.[BudgetYear] = @Year
                               AND b.[BudgetMonth] = @Month
                               AND b.[DeleteFlag] = 0
                         LEFT JOIN (
                                SELECT [CategoryId], SUM([Amount]) AS [SpentAmount]
                                FROM [dbo].[Tbl_Expense]
                                WHERE [DeleteFlag] = 0
                                  AND [ExpenseDate] >= @StartDate
                                  AND [ExpenseDate] < @EndDate
                                GROUP BY [CategoryId]
                         ) e ON e.[CategoryId] = c.[CategoryId]
                         WHERE c.[DeleteFlag] = 0
                           AND (b.[BudgetId] IS NOT NULL OR e.[SpentAmount] IS NOT NULL)
                         ORDER BY c.[CategoryName]";

        var report = connection.Query<MonthlyReportModel>(query, new
        {
            Year = year,
            Month = month,
            StartDate = startDate,
            EndDate = endDate
        }).ToList();

        // Print header
        Console.WriteLine($"===== Monthly Report: {year}-{month:D2} =====");

        // No data check
        if (report.Count == 0)
        {
            Console.WriteLine("No data found.");
            return;
        }

        string line = new string('-', 64);
        Console.WriteLine($"{"Category",-15}{"Budget",12}{"Spent",12}{"Remaining",12}  {"Status"}");
        Console.WriteLine(line);

        // Print rows
        foreach (var item in report)
        {
            Console.WriteLine($"{item.CategoryName,-15}{item.BudgetAmount,12:N0}{item.SpentAmount,12:N0}{item.RemainingAmount,12:N0}  {item.Status}");
        }

        // Print totals
        Console.WriteLine(line);
        var totalBudget = report.Sum(x => x.BudgetAmount);
        var totalSpent = report.Sum(x => x.SpentAmount);
        var totalRemaining = report.Sum(x => x.RemainingAmount);
        Console.WriteLine($"{"TOTAL",-15}{totalBudget,12:N0}{totalSpent,12:N0}{totalRemaining,12:N0}");
    }
}