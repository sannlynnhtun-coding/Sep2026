using Microsoft.Data.SqlClient;
using Sep2026.ClinicPatientRecord.API.Data;
using Sep2026.ClinicPatientRecord.API.Models;
using System.Data;

namespace Sep2026.ClinicPatientRecord.API.Repositories
{
    public class PatientRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public PatientRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // 1. Get All Patients
        public async Task<List<Patient>> GetAllAsync()
        {
            var patients = new List<Patient>();

            await using SqlConnection connection =
                _databaseConnection.GetConnection();

            await connection.OpenAsync();

            string query = "SELECT * FROM Patients ORDER BY PatientID DESC";

            await using SqlCommand command =
                new SqlCommand(query, connection);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                patients.Add(new Patient
                {
                    PatientID = reader.GetInt32("PatientID"),
                    FullName = reader.GetString("FullName"),
                    Age = reader.GetInt32("Age"),
                    Gender = reader.GetString("Gender"),
                    Phone = reader.IsDBNull("Phone")
                        ? null : reader.GetString("Phone"),
                    Address = reader.IsDBNull("Address")
                        ? null : reader.GetString("Address"),
                    RegistrationDate =
                        reader.GetDateTime("RegistrationDate")
                });
            }

            return patients;
        }

        // 2. Get Patient By ID
        public async Task<Patient?> GetByIdAsync(int id)
        {
            await using SqlConnection connection =
                _databaseConnection.GetConnection();

            await connection.OpenAsync();

            string query =
                "SELECT * FROM Patients WHERE PatientID = @PatientID";

            await using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add("@PatientID", SqlDbType.Int).Value = id;

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Patient
            {
                PatientID = reader.GetInt32("PatientID"),
                FullName = reader.GetString("FullName"),
                Age = reader.GetInt32("Age"),
                Gender = reader.GetString("Gender"),
                Phone = reader.IsDBNull("Phone")
                    ? null : reader.GetString("Phone"),
                Address = reader.IsDBNull("Address")
                    ? null : reader.GetString("Address"),
                RegistrationDate =
                    reader.GetDateTime("RegistrationDate")
            };
        }

        // 3. Create Patient
        public async Task<int> CreateAsync(Patient patient)
        {
            await using SqlConnection connection =
                _databaseConnection.GetConnection();

            await connection.OpenAsync();

            string query = @"
                INSERT INTO Patients
                (FullName, Age, Gender, Phone, Address)
                OUTPUT INSERTED.PatientID
                VALUES
                (@FullName, @Age, @Gender, @Phone, @Address)";

            await using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@FullName", patient.FullName);
            command.Parameters.AddWithValue("@Age", patient.Age);
            command.Parameters.AddWithValue("@Gender", patient.Gender);

            command.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value =
                (object?)patient.Phone ?? DBNull.Value;

            command.Parameters.Add("@Address", SqlDbType.NVarChar, 200).Value =
                (object?)patient.Address ?? DBNull.Value;

            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        // 4. Update Patient
        public async Task<bool> UpdateAsync(int id, Patient patient)
        {
            await using SqlConnection connection =
                _databaseConnection.GetConnection();

            await connection.OpenAsync();

            string query = @"
                UPDATE Patients
                SET FullName = @FullName,
                    Age = @Age,
                    Gender = @Gender,
                    Phone = @Phone,
                    Address = @Address
                WHERE PatientID = @PatientID";

            await using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@PatientID", id);
            command.Parameters.AddWithValue("@FullName", patient.FullName);
            command.Parameters.AddWithValue("@Age", patient.Age);
            command.Parameters.AddWithValue("@Gender", patient.Gender);

            command.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value =
                (object?)patient.Phone ?? DBNull.Value;

            command.Parameters.Add("@Address", SqlDbType.NVarChar, 200).Value =
                (object?)patient.Address ?? DBNull.Value;

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        // 5. Delete Patient
        public async Task<bool> DeleteAsync(int id)
        {
            await using SqlConnection connection =
                _databaseConnection.GetConnection();

            await connection.OpenAsync();

            string query =
                "DELETE FROM Patients WHERE PatientID = @PatientID";

            await using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add("@PatientID", SqlDbType.Int).Value = id;

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
    }
}