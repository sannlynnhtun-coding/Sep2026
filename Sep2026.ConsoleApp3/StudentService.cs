using Microsoft.Data.SqlClient;
using System.Data;

namespace Sep2026.ConsoleApp3
{
    public class StudentService
    {
        private SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder()
        {
            DataSource = ".",
            InitialCatalog = "Sep2026Db",
            UserID = "sa",
            Password = "sasa@123",
            TrustServerCertificate = true
        };

        // CREATE: add a new student.
        public void RegisterStudent(string no, string name, string classNo)
        {
            SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();
            SqlCommand command = new SqlCommand(
                $"INSERT INTO [dbo].[Students] ([No], [Name], [ClassNo]) VALUES ('{no}', '{name}', '{classNo}')", connection);
            command.ExecuteNonQuery();
            Console.WriteLine("Student registered successfully.");
            connection.Close();
        }

        // READ: display all students.
        public void ShowStudents()
        {
            SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();
            SqlCommand command = new SqlCommand(
                "SELECT [Id], [No], [Name], [ClassNo] FROM [dbo].[Students] ORDER BY [Id]", connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            if (table.Rows.Count == 0)
            {
                Console.WriteLine("No students found.");

            }

            foreach (DataRow item in table.Rows)
            {
                Console.WriteLine($"Id: {item["Id"]} | No: {item["No"]} | Name: {item["Name"]} | ClassNo: {item["ClassNo"]}");
            }
            connection.Close();
        }

        // UPDATE: change a student using their Id.
        public void UpdateStudent(int id, string no, string name, string classNo)
        {
            SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();
            SqlCommand command = new SqlCommand(
                $"UPDATE [dbo].[Students] SET [No] = '{no}', [Name] = '{name}', [ClassNo] = '{classNo}' WHERE [Id] = {id}", connection);
            int rows = command.ExecuteNonQuery();
            if (rows > 0)
            {
                Console.WriteLine("Student updated successfully.");

            }
            else
            {
                Console.WriteLine("Student not found.");
            }
            connection.Close();
        }

        // DELETE: remove a student only when they have no grades.
        public void DeleteStudent(int id)
        {
            SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();
            SqlCommand command = new SqlCommand($@"DELETE FROM [dbo].[Students]
                WHERE [Id] = {id} AND NOT EXISTS
                (SELECT 1 FROM [dbo].[Grades] WHERE [StudentId] = {id})", connection);
            int rows = command.ExecuteNonQuery();
            if (rows > 0)
            {
                Console.WriteLine("Student deleted successfully.");

            }
            else
            {
                Console.WriteLine("Student not found or has grades. Delete the student's grades first.");
            }
            connection.Close();
        }

    }
}
