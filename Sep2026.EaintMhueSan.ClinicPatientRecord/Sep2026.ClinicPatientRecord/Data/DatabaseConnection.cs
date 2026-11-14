using Microsoft.Data.SqlClient;

namespace Sep2026.ClinicPatientRecord.API.Data
{
    public class DatabaseConnection
    {
        private readonly IConfiguration _configuration;

        public DatabaseConnection(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection GetConnection()
        {
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Database connection string not found.");

            return new SqlConnection(connectionString);
        }
    }
}