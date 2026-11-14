using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Sep2026.ClinicPatientRecord.API.Data;

namespace Sep2026.ClinicPatientRecord.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        private readonly DatabaseConnection _databaseConnection;

        public DatabaseTestController(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                await using SqlConnection connection =
                    _databaseConnection.GetConnection();

                await connection.OpenAsync();

                await using SqlCommand command =
                    new SqlCommand("SELECT DB_NAME()", connection);

                string? databaseName =
                    await command.ExecuteScalarAsync() as string;

                return Ok(new
                {
                    Success = true,
                    Message = "Database connection successful!",
                    Database = databaseName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Database connection failed.",
                    Error = ex.Message
                });
            }
        }
    }
}