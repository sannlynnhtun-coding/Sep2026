// See https://aka.ms/new-console-template for more information
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

Console.WriteLine("Hello, World!");

SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? "Server=.;Database=Sep2026Db;Integrated Security=True;TrustServerCertificate=True;");

//SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
//connection.Open();
//connection.Close();

//using (IDbConnection db = new SqlConnection(sqlConnectionStringBuilder.ConnectionString))
//{
//    var lst = db.Query<StudentDto>("Select * from Students").ToList();

//    foreach (var item in lst)
//    {
//        Console.WriteLine($"Id: {item.Id} | No: {item.No} | Name: {item.Name} | ClassNo: {item.ClassNo}");
//    }
//}

using IDbConnection db = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
db.Open();
var lst = db.Query<StudentDto>("Select * from Students").ToList();

foreach (var item in lst)
{
    Console.WriteLine($"Id: {item.Id} | No: {item.No} | Name: {item.Name} | ClassNo: {item.ClassNo}");
}


using IDbConnection db2 = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
db2.Open();
int result = db2.Execute("DELETE FROM [dbo].[Students] WHERE [Id] = 1");

// sql injection attack

public class StudentDto
{
    public int Id { get; set; }
    public string No { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ClassNo { get; set; } = string.Empty;
}