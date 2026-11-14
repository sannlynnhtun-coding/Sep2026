using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Filters;

public sealed class DatabaseConflictFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } })
        {
            context.Result = new ConflictObjectResult("A record with the same unique value already exists.");
            context.ExceptionHandled = true;
        }
    }
}
