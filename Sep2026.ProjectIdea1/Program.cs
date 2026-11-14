// See https://aka.ms/new-console-template for more information
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

Console.WriteLine("Hello, World!");

string jsonContent = File.ReadAllText("C:\\Users\\Sann Lynn Htun\\Downloads\\MinTheinKha.LatHtaukBayDin.json");

QuestionAnswerResponseModel responseModel = JsonSerializer.Deserialize<QuestionAnswerResponseModel>(jsonContent)!;

SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
{
    DataSource = ".",
    InitialCatalog = "Sep2026Db",
    UserID = "sa",
    Password = "sasa@123",
    TrustServerCertificate = true
};
IDbConnection connection = new SqlConnection(connectionStringBuilder.ConnectionString);
connection.Open();

//for (int i = 0; i < responseModel.questions.Length; i++)
//{
//    var item = responseModel.questions[i];
//    string query = $@"
//INSERT INTO [dbo].[Project1Question]
//           ([QuestionId]
//           ,[QuestionNo]
//           ,[QuestionName])
//     VALUES
//           ({item.questionNo}
//           ,{item.questionNo}
//           ,N'{item.questionName}')";
//    connection.Execute(query);
//}

for (int i = 0; i < responseModel.answers.Length; i++)
{
    var item = responseModel.answers[i];
    string query = $@"INSERT INTO [dbo].[Project1Answer]
           ([QuestionId]
           ,[AnswerNo]
           ,[AnswerResult])
     VALUES
           ('{item.questionNo}'
           ,'{item.answerNo}'
           ,N'{item.answerResult}')";
    connection.Execute(query);
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
