using Microsoft.Data.SqlClient;
using Sep2026.ClinicPatientRecord.API.Data;
using Sep2026.ClinicPatientRecord.API.Models;

namespace Sep2026.ClinicPatientRecord.API.Repositories
{
    public class DoctorRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public DoctorRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Get all doctors
        public async Task<List<Doctor>> GetAllAsync()
        {
            var doctors = new List<Doctor>();

            await using SqlConnection connection =
                _databaseConnection.GetConnection();

            await connection.OpenAsync();

            string query = @"
                SELECT DoctorID, DoctorName, Specialization
                FROM Doctors
                ORDER BY DoctorID ASC";

            await using SqlCommand command =
                new SqlCommand(query, connection);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var doctor = new Doctor
                {
                    DoctorID = reader.GetInt32(
                        reader.GetOrdinal("DoctorID")),

                    DoctorName = reader.GetString(
                        reader.GetOrdinal("DoctorName")),

                    Specialization = reader.GetString(
                        reader.GetOrdinal("Specialization"))
                };

                doctors.Add(doctor);
            }

            return doctors;
        }
    }
}