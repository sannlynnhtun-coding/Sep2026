// See https://aka.ms/new-console-template for more information
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

Console.WriteLine("Hello, World!");

string jsonContent = File.ReadAllText(args.Length == 1 ? args[0] : "C:\\Users\\Sann Lynn Htun\\Downloads\\MinTheinKha.LatHtaukBayDin.json");

QuestionAnswerResponseModel responseModel = JsonSerializer.Deserialize<QuestionAnswerResponseModel>(jsonContent)!;

SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? "Server=.;Database=Sep2026Db;Integrated Security=True;TrustServerCertificate=True;");
using IDbConnection connection = new SqlConnection(connectionStringBuilder.ConnectionString);
connection.Open();

for (int i = 0; i < responseModel.answers.Length; i++)
{
    var item = responseModel.answers[i];
    string query = @"INSERT INTO [dbo].[Project1Answer]
           ([QuestionId]
           ,[AnswerNo]
           ,[AnswerResult])
     VALUES
           (@questionNo
           ,@answerNo
           ,@answerResult)";
    connection.Execute(query, new { item.questionNo, item.answerNo, item.answerResult });
}

connection.Close();

// JSON to C# class conversion
public class QuestionAnswerResponseModel
{
    public QuestionModel[] questions { get; set; } = [];
    public AnswerModel[] answers { get; set; } = [];
    public string[] numberList { get; set; } = [];
}

public class QuestionModel
{
    public int questionNo { get; set; }
    public string questionName { get; set; } = string.Empty;
}

public class AnswerModel
{
    public int questionNo { get; set; }
    public int answerNo { get; set; }
    public string answerResult { get; set; } = string.Empty;
}
