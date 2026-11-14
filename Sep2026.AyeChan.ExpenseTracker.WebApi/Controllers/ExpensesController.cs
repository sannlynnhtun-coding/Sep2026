using Microsoft.AspNetCore.Mvc;
using Sep2026.AyeChan.ExpenseTracker.Database.AppDbContextModels;
using Sep2026.AyeChan.ExpenseTracker.WebApi.Models;

namespace Sep2026.AyeChan.ExpenseTracker.WebApi.Controllers;

// api/expenses
[Route("api/[controller]")]
[ApiController]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext db;

    public ExpensesController(AppDbContext db)
    {
        // AppDbContext is provided by dependency injection (registered in Program.cs)
        this.db = db;
    }

    // GET api/expenses
    // GET api/expenses?year=2026&month=11
    // GET api/expenses?categoryId=2
    // GET api/expenses?year=2026&month=11&categoryId=2
    [HttpGet]
    public IActionResult Read(int? year, int? month, int? categoryId)
    {
        // Active expenses only (SQL is not executed yet)
        var query = db.TblExpenses.Where(x => x.DeleteFlag == false);

        // Optional month filter
        if (year.HasValue || month.HasValue)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return BadRequest("Provide both year and month.");
            }

            if (year < 2000 || year > 2100 || month < 1 || month > 12)
            {
                return BadRequest("Invalid year or month.");
            }

            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate = startDate.AddMonths(1);
            query = query.Where(x => x.ExpenseDate >= startDate && x.ExpenseDate < endDate);
        }

        // Optional category filter
        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        // Project to response model, newest first (SQL runs at ToList)
        var expenses = query
            .OrderByDescending(x => x.ExpenseDate)
            .Select(x => new ExpenseResponseModel
            {
                ExpenseId = x.ExpenseId,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.CategoryName,
                Amount = x.Amount,
                ExpenseDate = x.ExpenseDate,
                Note = x.Note
            })
            .ToList();

        return Ok(expenses);
    }

    // GET api/expenses/3
    [HttpGet("{id}")]
    public IActionResult Detail(int id)
    {
        // Get one active expense by id
        var expense = db.TblExpenses
            .Where(x => x.ExpenseId == id && x.DeleteFlag == false)
            .Select(x => new ExpenseResponseModel
            {
                ExpenseId = x.ExpenseId,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.CategoryName,
                Amount = x.Amount,
                ExpenseDate = x.ExpenseDate,
                Note = x.Note
            })
            .FirstOrDefault();

        // Null check
        if (expense is null)
        {
            return NotFound("No data found.");
        }

        return Ok(expense);
    }



    // POST api/expenses
    [HttpPost]
    public IActionResult Create(ExpenseRequestModel requestModel)
    {
        // Check business rules
        string? error = Validate(requestModel);
        if (error is not null)
        {
            return BadRequest(error);
        }

        // Build new entity
        var expense = new TblExpense
        {
            CategoryId = requestModel.CategoryId,
            Amount = requestModel.Amount,
            ExpenseDate = requestModel.ExpenseDate,
            Note = string.IsNullOrWhiteSpace(requestModel.Note) ? null : requestModel.Note.Trim(),
            DeleteFlag = false
        };

        // Insert
        db.TblExpenses.Add(expense);
        int result = db.SaveChanges();

        string message = result > 0 ? "Saving Successful." : "Saving Failed.";
        return result > 0 ? Ok(message) : BadRequest(message);
    }

    // PUT api/expenses/3
    [HttpPut("{id}")]
    public IActionResult Update(int id, ExpenseRequestModel requestModel)
    {
        // Find the active expense
        var expense = db.TblExpenses.FirstOrDefault(x => x.ExpenseId == id && x.DeleteFlag == false);
        if (expense is null)
        {
            return NotFound("No data found.");
        }

        // Check business rules
        string? error = Validate(requestModel);
        if (error is not null)
        {
            return BadRequest(error);
        }

        // Change properties (EF Core tracks these changes)
        expense.CategoryId = requestModel.CategoryId;
        expense.Amount = requestModel.Amount;
        expense.ExpenseDate = requestModel.ExpenseDate;
        expense.Note = string.IsNullOrWhiteSpace(requestModel.Note) ? null : requestModel.Note.Trim();

        // Save only the changed columns (0 if nothing changed)
        int result = db.SaveChanges();

        string message = result > 0 ? "Updating Successful." : "No changes were made.";
        return Ok(message);
    }

    // DELETE api/expenses/3
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        // Find the active expense
        var expense = db.TblExpenses.FirstOrDefault(x => x.ExpenseId == id && x.DeleteFlag == false);
        if (expense is null)
        {
            return NotFound("No data found.");
        }

        // Soft delete
        expense.DeleteFlag = true;
        int result = db.SaveChanges();

        string message = result > 0 ? "Deleting Successful." : "Deleting Failed.";
        return result > 0 ? Ok(message) : BadRequest(message);
    }

    // Shared business rules for Create and Update. Returns an error message, or null if valid.
    private string? Validate(ExpenseRequestModel requestModel)
    {
        // Rule 1: Amount must be greater than 0
        if (requestModel.Amount <= 0)
        {
            return "Amount must be greater than 0.";
        }

        // Rule 2: ExpenseDate is required (missing in JSON becomes 0001-01-01)
        if (requestModel.ExpenseDate == default)
        {
            return "Expense date is required.";
        }

        // Rule 3: ExpenseDate cannot be in the future
        if (requestModel.ExpenseDate.Date > DateTime.Today)
        {
            return "Expense date cannot be in the future.";
        }

        // Rule 4: Note max 200 characters
        if (requestModel.Note is not null && requestModel.Note.Trim().Length > 200)
        {
            return "Note must be 200 characters or less.";
        }

        // Rule 5: Category must exist and be active (checked last because it hits the database)
        bool categoryExists = db.TblCategories.Any(x => x.CategoryId == requestModel.CategoryId && x.DeleteFlag == false);
        if (!categoryExists)
        {
            return "Category not found.";
        }

        return null;
    }
}