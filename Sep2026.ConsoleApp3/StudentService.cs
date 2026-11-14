using Microsoft.Data.SqlClient;
using System.Data;

namespace Sep2026.ConsoleApp3
{
    public class StudentService
    {
        private SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=.;Database=Sep2026Db;Integrated Security=True;TrustServerCertificate=True;");

        // CREATE: add a new student.
        public void RegisterStudent(string no, string name, string classNo)
        {
            using SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();
            using SqlCommand command = new SqlCommand(
                "INSERT INTO [dbo].[Students] ([No], [Name], [ClassNo]) VALUES (@no, @name, @classNo)", connection);
            command.Parameters.Add("@no", SqlDbType.NVarChar, -1).Value = no;
            command.Parameters.Add("@name", SqlDbType.NVarChar, -1).Value = name;
            command.Parameters.Add("@classNo", SqlDbType.NVarChar, -1).Value = classNo;
            command.ExecuteNonQuery();
            Console.WriteLine("Student registered successfully.");
            connection.Close();
        }

        // READ: display all students.
        public void ShowStudents()
        {
            using SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();
            using SqlCommand command = new SqlCommand(
                "SELECT [Id], [No], [Name], [ClassNo] FROM [dbo].[Students] ORDER BY [Id]", connection);
            using SqlDataAdapter adapter = new SqlDataAdapter(command);
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

            List<StudentDto> students = new List<StudentDto>();

            foreach (DataRow item in table.Rows)
            {
                StudentDto student = new StudentDto
                {
                    Id = Convert.ToInt32(item["Id"]),
                    No = Convert.ToString(item["No"]) ?? string.Empty,
                    Name = Convert.ToString(item["Name"]) ?? string.Empty,
                    ClassNo = Convert.ToString(item["ClassNo"]) ?? string.Empty
                };
                students.Add(student);
            }
        }

        // UPDATE: change a student using their Id.
        public void UpdateStudent(int id, string no, string name, string classNo)
        {
            using SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();
            using SqlCommand command = new SqlCommand(
                "UPDATE [dbo].[Students] SET [No] = @no, [Name] = @name, [ClassNo] = @classNo WHERE [Id] = @id", connection);
            command.Parameters.Add("@no", SqlDbType.NVarChar, -1).Value = no;
            command.Parameters.Add("@name", SqlDbType.NVarChar, -1).Value = name;
            command.Parameters.Add("@classNo", SqlDbType.NVarChar, -1).Value = classNo;
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
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
            using SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();
            using SqlCommand command = new SqlCommand(@"DELETE FROM [dbo].[Students]
                WHERE [Id] = @id AND NOT EXISTS
                (SELECT 1 FROM [dbo].[Grades] WHERE [StudentId] = @id)", connection);
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
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
