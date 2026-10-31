using Microsoft.Data.SqlClient;
using System.Data;

namespace Sep2026.ConsoleApp3
{
    public class GradeService
    {
        private SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder()
        {
            DataSource = ".",
            InitialCatalog = "Sep2026Db",
            UserID = "sa",
            Password = "sasa@123",
            TrustServerCertificate = true
        };

        // CREATE: find the student, then add their grade.
        public void RegisterGrade(string studentNo, string subject, int mark)
        {
            SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();

            SqlCommand studentCommand = new SqlCommand(
                $"SELECT [Id], [Name] FROM [dbo].[Students] WHERE [No] = '{studentNo}'", connection);

            SqlDataAdapter studentAdapter = new SqlDataAdapter(studentCommand);
            DataTable studentTable = new DataTable();
            studentAdapter.Fill(studentTable);

            if (studentTable.Rows.Count == 0)
            {
                Console.WriteLine($"Student with no {studentNo} was not found.");
                connection.Close();
                return;

            }

            DataRow student = studentTable.Rows[0];
            int studentId = Convert.ToInt32(student["Id"]);
            string studentName = Convert.ToString(student["Name"]) ?? studentNo;

            string query = $@"INSERT INTO [dbo].[Grades] ([StudentId], [Subject], [Mark])
                                  VALUES ({studentId}, '{subject}', {mark})";
            SqlCommand command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();

            Console.WriteLine($"Grade registered successfully for student {studentName} in subject {subject} with mark {mark}.");
            connection.Close();
        }

        // READ: display grades with student details.
        public void ShowGrades()
        {
            SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();

            const string query = @"SELECT g.[GradeId], s.[No], s.[Name], g.[Subject], g.[Mark]
                                  FROM [dbo].[Grades] g
                                  INNER JOIN [dbo].[Students] s ON s.[Id] = g.[StudentId]";
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            if (table.Rows.Count == 0) Console.WriteLine("No grades found.");
            foreach (DataRow item in table.Rows)
            {
                int id = Convert.ToInt32(item["GradeId"]);

                Console.WriteLine($"Grade Id: {item["GradeId"]} | Student No: {item["No"]} | Name: {item["Name"]} | Subject: {item["Subject"]} | Mark: {item["Mark"]}");

            }
            connection.Close();
        }

        // UPDATE: change a grade using its GradeId.
        public void UpdateGrade(int gradeId, string studentNo, string subject, int mark)
        {
            SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();
            SqlCommand studentCommand = new SqlCommand(
                $"SELECT [Id] FROM [dbo].[Students] WHERE [No] = '{studentNo}'", connection);
            SqlDataAdapter studentAdapter = new SqlDataAdapter(studentCommand);
            DataTable studentTable = new DataTable();
            studentAdapter.Fill(studentTable);

            if (studentTable.Rows.Count == 0)
            {
                Console.WriteLine("Student not found.");
                connection.Close();
                return;

            }

            int studentId = Convert.ToInt32(studentTable.Rows[0]["Id"]);

            SqlCommand command = new SqlCommand($@"UPDATE [dbo].[Grades]
                SET [StudentId] = {studentId}, [Subject] = '{subject}', [Mark] = {mark}
                WHERE [GradeId] = {gradeId}", connection);
            int rows = command.ExecuteNonQuery();
            if (rows > 0)
            {
                Console.WriteLine("Grade updated successfully.");
            }
            else
            {
                Console.WriteLine("Grade not found.");
            }
            connection.Close();
        }

        // DELETE: remove a grade using its GradeId.
        public void DeleteGrade(int gradeId)
        {
            SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);

            connection.Open();
            SqlCommand command = new SqlCommand($"DELETE FROM [dbo].[Grades] WHERE [GradeId] = {gradeId}", connection);
            int rows = command.ExecuteNonQuery();
            if (rows > 0)
            {
                Console.WriteLine("Grade deleted successfully.");

            }
            else
            {
                Console.WriteLine("Grade not found.");
            }
            connection.Close();
        }

    }
}
