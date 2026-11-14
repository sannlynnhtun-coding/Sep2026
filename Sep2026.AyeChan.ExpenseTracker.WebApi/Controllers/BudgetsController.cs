using Microsoft.AspNetCore.Mvc;
using Sep2026.AyeChan.ExpenseTracker.Database.AppDbContextModels;
using Sep2026.AyeChan.ExpenseTracker.WebApi.Models;

namespace Sep2026.AyeChan.ExpenseTracker.WebApi.Controllers;

// api/budgets
[Route("api/[controller]")]
[ApiController]
public class BudgetsController : ControllerBase
{
    private readonly AppDbContext db;

    public BudgetsController(AppDbContext db)
    {
        // AppDbContext is provided by dependency injection (registered in Program.cs)
        this.db = db;
    }

    // GET api/budgets
    // GET api/budgets?year=2026&month=11
    [HttpGet]
    public IActionResult Read(int? year, int? month)
    {
        // Active budgets only (SQL is not executed yet)
        var query = db.TblBudgets.Where(x => x.DeleteFlag == false);

        // Optional month filter
        if (year.HasValue || month.HasValue)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return BadRequest("Provide both year and month.");
            }

            query = query.Where(x => x.BudgetYear == year.Value && x.BudgetMonth == month.Value);
        }

        // Project to response model (SQL runs at ToList)
        var budgets = query
            .OrderByDescending(x => x.BudgetYear)
            .ThenByDescending(x => x.BudgetMonth)
            .ThenBy(x => x.Category.CategoryName)
            .Select(x => new BudgetResponseModel
            {
                BudgetId = x.BudgetId,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.CategoryName,
                BudgetYear = x.BudgetYear,
                BudgetMonth = x.BudgetMonth,
                Amount = x.Amount
            })
            .ToList();

        return Ok(budgets);
    }

    // GET api/budgets/1
    [HttpGet("{id}")]
    public IActionResult Detail(int id)
    {
        // Get one active budget by id
        var budget = db.TblBudgets
            .Where(x => x.BudgetId == id && x.DeleteFlag == false)
            .Select(x => new BudgetResponseModel
            {
                BudgetId = x.BudgetId,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.CategoryName,
                BudgetYear = x.BudgetYear,
                BudgetMonth = x.BudgetMonth,
                Amount = x.Amount
            })
            .FirstOrDefault();

        // Null check
        if (budget is null)
        {
            return NotFound("No data found.");
        }

        return Ok(budget);
    }

    // POST api/budgets
    [HttpPost]
    public IActionResult Create(BudgetRequestModel requestModel)
    {
        // Check input rules (400)
        string? error = Validate(requestModel);
        if (error is not null)
        {
            return BadRequest(error);
        }

        // Business rule: only one active budget per category per month (409)
        if (HasActiveBudget(requestModel.CategoryId, requestModel.BudgetYear, requestModel.BudgetMonth))
        {
            return Conflict("An active budget already exists for this category and month.");
        }

        // Build new entity
        var budget = new TblBudget
        {
            CategoryId = requestModel.CategoryId,
            BudgetYear = requestModel.BudgetYear,
            BudgetMonth = requestModel.BudgetMonth,
            Amount = requestModel.Amount,
            DeleteFlag = false
        };

        // Insert
        db.TblBudgets.Add(budget);
        int result = db.SaveChanges();

        string message = result > 0 ? "Saving Successful." : "Saving Failed.";
        return result > 0 ? Ok(message) : BadRequest(message);
    }

    // PUT api/budgets/1
    [HttpPut("{id}")]
    public IActionResult Update(int id, BudgetRequestModel requestModel)
    {
        // Find the active budget
        var budget = db.TblBudgets.FirstOrDefault(x => x.BudgetId == id && x.DeleteFlag == false);
        if (budget is null)
        {
            return NotFound("No data found.");
        }

        // Check input rules (400)
        string? error = Validate(requestModel);
        if (error is not null)
        {
            return BadRequest(error);
        }

        // Business rule: exclude this budget itself from the duplicate check (409)
        if (HasActiveBudget(requestModel.CategoryId, requestModel.BudgetYear, requestModel.BudgetMonth, id))
        {
            return Conflict("An active budget already exists for this category and month.");
        }

        // Change properties (EF Core tracks these changes)
        budget.CategoryId = requestModel.CategoryId;
        budget.BudgetYear = requestModel.BudgetYear;
        budget.BudgetMonth = requestModel.BudgetMonth;
        budget.Amount = requestModel.Amount;

        // Save only the changed columns (0 if nothing changed)
        int result = db.SaveChanges();

        string message = result > 0 ? "Updating Successful." : "No changes were made.";
        return Ok(message);
    }

    // DELETE api/budgets/1
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        // Find the active budget
        var budget = db.TblBudgets.FirstOrDefault(x => x.BudgetId == id && x.DeleteFlag == false);
        if (budget is null)
        {
            return NotFound("No data found.");
        }

        // Soft delete
        budget.DeleteFlag = true;
        int result = db.SaveChanges();

        string message = result > 0 ? "Deleting Successful." : "Deleting Failed.";
        return result > 0 ? Ok(message) : BadRequest(message);
    }

    // Input rules for Create and Update. Returns an error message, or null if valid.
    private string? Validate(BudgetRequestModel requestModel)
    {
        // Rule 1: Amount must be greater than 0
        if (requestModel.Amount <= 0)
        {
            return "Amount must be greater than 0.";
        }

        // Rule 2: Year and month must be valid (missing values in JSON become 0)
        if (requestModel.BudgetYear < 2000 || requestModel.BudgetYear > 2100)
        {
            return "Budget year must be between 2000 and 2100.";
        }

        if (requestModel.BudgetMonth < 1 || requestModel.BudgetMonth > 12)
        {
            return "Budget month must be between 1 and 12.";
        }

        // Rule 3: Category must exist and be active (checked last because it hits the database)
        bool categoryExists = db.TblCategories.Any(x => x.CategoryId == requestModel.CategoryId && x.DeleteFlag == false);
        if (!categoryExists)
        {
            return "Category not found.";
        }

        return null;
    }

    // True if another active budget exists for the same category and month
    private bool HasActiveBudget(int categoryId, int year, int month, int? excludeBudgetId = null)
    {
        return db.TblBudgets.Any(x => x.CategoryId == categoryId
                                   && x.BudgetYear == year
                                   && x.BudgetMonth == month
                                   && x.DeleteFlag == false
                                   && (excludeBudgetId == null || x.BudgetId != excludeBudgetId));
    }
}