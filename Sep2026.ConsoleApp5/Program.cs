using Sep2026.ConsoleApp5.Database.AppDbContextModels;

AppDbContext db = new AppDbContext();
var lst = db.Students.ToList();

foreach (var item in lst)
{
    Console.WriteLine($"Id: {item.Id} | No: {item.No} | Name: {item.Name}");
}

db.Students.Add(new Student()
{
    No = "S0001",
    Name = "John Doe",
    ClassNo = "C001"
});
db.Students.Add(new Student()
{
    No = "S0002",
    Name = "Jane Smith",
    ClassNo = "C002"
});
int result = db.SaveChanges();

var student = db.Students.FirstOrDefault(x => x.Id == 1);
if (student is null)
{
    Console.WriteLine("Student not found.");
}
else
{
    student.Name = "Updated Name";
    db.SaveChanges();
}

var deleteStudent = db.Students.FirstOrDefault(x => x.Id == 2);
if (deleteStudent is null)
{
    Console.WriteLine("Student not found.");
}
else
{
    db.Students.Remove(deleteStudent);
    db.SaveChanges();
}