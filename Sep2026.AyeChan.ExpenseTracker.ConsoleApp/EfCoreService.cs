using Microsoft.EntityFrameworkCore;
using Sep2026.AyeChan.ExpenseTracker.Database.AppDbContextModels;

namespace Sep2026.AyeChan.ExpenseTracker.ConsoleApp;

public class EfCoreService
{
    public void Read()
    {
        using var db = new AppDbContext();

        // Active expenses with category name, newest first
        var expenses = db.TblExpenses
            .Include(x => x.Category)
            .Where(x => x.DeleteFlag == false)
            .OrderByDescending(x => x.ExpenseDate)
            .ToList();

        // Print results
        Console.WriteLine("----- Expenses -----");
        foreach (var item in expenses)
        {
            Console.WriteLine($"{item.ExpenseId}. {item.ExpenseDate:yyyy-MM-dd} | {item.Category.CategoryName} | {item.Amount:N0} MMK | {item.Note}");
        }
    }

    public void Detail(int id)
    {
        using var db = new AppDbContext();

        // Get one active expense by id
        var expense = db.TblExpenses
            .Include(x => x.Category)
            .FirstOrDefault(x => x.ExpenseId == id && x.DeleteFlag == false);

        // Null check
        if (expense is null)
        {
            Console.WriteLine("No data found.");
            return;
        }

        Console.WriteLine($"Id: {expense.ExpenseId}");
        Console.WriteLine($"Category: {expense.Category.CategoryName}");
        Console.WriteLine($"Amount: {expense.Amount:N0} MMK");
        Console.WriteLine($"Date: {expense.ExpenseDate:yyyy-MM-dd}");
        Console.WriteLine($"Note: {expense.Note ?? "-"}");
    }

    public void Create(int categoryId, decimal amount, DateTime expenseDate, string? note)
    {
        using var db = new AppDbContext();

        // Check business rules
        string? error = Validate(db, categoryId, amount, expenseDate, note);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        // Build new entity
        var expense = new TblExpense
        {
            CategoryId = categoryId,
            Amount = amount,
            ExpenseDate = expenseDate,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            DeleteFlag = false
        };

        // Insert
        db.TblExpenses.Add(expense);
        int result = db.SaveChanges();

        string message = result > 0 ? "Saving Successful." : "Saving Failed.";
        Console.WriteLine(message);
    }

    public void Update(int id, int categoryId, decimal amount, DateTime expenseDate, string? note)
    {
        using var db = new AppDbContext();

        // Find the active expense
        var expense = db.TblExpenses.FirstOrDefault(x => x.ExpenseId == id && x.DeleteFlag == false);
        if (expense is null)
        {
            Console.WriteLine("No data found.");
            return;
        }

        // Check business rules
        string? error = Validate(db, categoryId, amount, expenseDate, note);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        // Change properties (EF Core tracks these changes)
        expense.CategoryId = categoryId;
        expense.Amount = amount;
        expense.ExpenseDate = expenseDate;
        expense.Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        // Save only the changed columns
        int result = db.SaveChanges();

        string message = result > 0 ? "Updating Successful." : "Updating Failed.";
        Console.WriteLine(message);
    }

    public void Delete(int id)
    {
        using var db = new AppDbContext();

        // Find the active expense
        var expense = db.TblExpenses.FirstOrDefault(x => x.ExpenseId == id && x.DeleteFlag == false);
        if (expense is null)
        {
            Console.WriteLine("No data found.");
            return;
        }

        // Soft delete
        expense.DeleteFlag = true;
        int result = db.SaveChanges();

        string message = result > 0 ? "Deleting Successful." : "Deleting Failed.";
        Console.WriteLine(message);
    }

    public void ReadByMonth(int year, int month)
    {
        using var db = new AppDbContext();

        // Active expenses in the given month, newest first
        var expenses = db.TblExpenses
            .Include(x => x.Category)
            .Where(x => x.DeleteFlag == false
                     && x.ExpenseDate.Year == year
                     && x.ExpenseDate.Month == month)
            .OrderByDescending(x => x.ExpenseDate)
            .ToList();

        Console.WriteLine($"----- Expenses for {year}-{month:D2} -----");

        // No data check
        if (expenses.Count == 0)
        {
            Console.WriteLine("No data found.");
            return;
        }

        // Print results
        foreach (var item in expenses)
        {
            Console.WriteLine($"{item.ExpenseId}. {item.ExpenseDate:yyyy-MM-dd} | {item.Category.CategoryName} | {item.Amount:N0} MMK | {item.Note}");
        }

        // Monthly total
        var total = expenses.Sum(x => x.Amount);
        Console.WriteLine($"Total: {total:N0} MMK");
    }

    // Shared business rules for Create and Update. Returns an error message, or null if valid.
    private static string? Validate(AppDbContext db, int categoryId, decimal amount, DateTime expenseDate, string? note)
    {
        // Rule 1: Amount must be greater than 0
        if (amount <= 0)
        {
            return "Amount must be greater than 0.";
        }

        // Rule 2: ExpenseDate cannot be in the future
        if (expenseDate.Date > DateTime.Today)
        {
            return "Expense date cannot be in the future.";
        }

        // Rule 3: Note max 200 characters
        if (note is not null && note.Trim().Length > 200)
        {
            return "Note must be 200 characters or less.";
        }

        // Rule 4: Category must exist and be active (checked last because it hits the database)
        bool categoryExists = db.TblCategories.Any(x => x.CategoryId == categoryId && x.DeleteFlag == false);
        if (!categoryExists)
        {
            return "Category not found.";
        }

        return null;
    }
}