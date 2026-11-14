using Microsoft.Data.SqlClient;
using Sep2026.ClinicPatientRecord.API.Data;
using Sep2026.ClinicPatientRecord.API.Models;
using System.Data;

namespace Sep2026.ClinicPatientRecord.API.Repositories
{
    public class TokenRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public TokenRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Generate and save the next daily token
        public async Task<Token> GenerateAsync(
            int patientId,
            int doctorId)
        {
            await using SqlConnection connection =
                _databaseConnection.GetConnection();

            await connection.OpenAsync();

            await using SqlTransaction transaction =
                (SqlTransaction)await connection.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                // Get today's date
                DateTime today = DateTime.Today;

                // Find the next available token number
                string query = @"
                    SELECT ISNULL(MAX(TokenNumber), 0) + 1
                    FROM dbo.Tokens WITH (UPDLOCK, HOLDLOCK)
                    WHERE TokenDate = @TokenDate;
                ";

                int nextNumber;

                await using (SqlCommand command =
                    new SqlCommand(query, connection, transaction))
                {
                    command.Parameters.Add(
                        "@TokenDate", SqlDbType.Date
                    ).Value = today;

                    nextNumber = Convert.ToInt32(
                        await command.ExecuteScalarAsync()
                    );
                }

                // Check daily token limit
                if (nextNumber > 200)
                {
                    throw new InvalidOperationException(
                        "Daily token limit of 200 has been reached."
                    );
                }

                // Insert the new token
                string insertQuery = @"
                    INSERT INTO dbo.Tokens
                    (
                        PatientID,
                        DoctorID,
                        TokenNumber,
                        TokenDate
                    )
                    OUTPUT
                        INSERTED.TokenID,
                        INSERTED.CreatedAt
                    VALUES
                    (
                        @PatientID,
                        @DoctorID,
                        @TokenNumber,
                        @TokenDate
                    );
                ";

                Token token = new Token
                {
                    PatientID = patientId,
                    DoctorID = doctorId,
                    TokenNumber = nextNumber,
                    TokenDate = today
                };

                await using (SqlCommand insertCommand =
                    new SqlCommand(insertQuery, connection, transaction))
                {
                    insertCommand.Parameters.Add(
                        "@PatientID", SqlDbType.Int
                    ).Value = patientId;

                    insertCommand.Parameters.Add(
                        "@DoctorID", SqlDbType.Int
                    ).Value = doctorId;

                    insertCommand.Parameters.Add(
                        "@TokenNumber", SqlDbType.Int
                    ).Value = nextNumber;

                    insertCommand.Parameters.Add(
                        "@TokenDate", SqlDbType.Date
                    ).Value = today;

                    await using SqlDataReader reader =
                        await insertCommand.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        token.TokenID = reader.GetInt32(0);
                        token.CreatedAt = reader.GetDateTime(1);
                    }
                }

                // Commit transaction
                await transaction.CommitAsync();

                return token;
            }
            catch
            {
                // Roll back if anything fails
                await transaction.RollbackAsync();

                throw;
            }
        }
    }
}
