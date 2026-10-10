using Sep2026.ConsoleApp3;

Start:

Console.WriteLine("Student Management");

Console.WriteLine("1. Student Registration");
Console.WriteLine("2. Grade Registration");
Console.WriteLine("3. Exit");

Console.Write("Please select an option : ");
string option = Console.ReadLine();

switch (option)
{
    case "1":
        Console.Write("Please enter student no: ");
        string no = Console.ReadLine();

        Console.Write("Please enter student name: ");
        string name = Console.ReadLine();

        Console.Write("Please enter student class no: ");
        string classNo = Console.ReadLine();

        StudentService studentService = new StudentService();
        studentService.RegisterStudent(no, name, classNo);

        studentService.ShowStudents();
        break;
    case "2":
        Console.Write("Please enter student no: ");
        string studentNo = Console.ReadLine();

        Console.Write("Please enter subject: ");
        string subject = Console.ReadLine();

        Console.Write("Please enter mark: ");
        int mark = int.Parse(Console.ReadLine());

        GradeService gradeService = new GradeService();
        gradeService.RegisterGrade(studentNo, subject, mark);
        break;
    case "3":
        goto End;
    default:
        break;
}

goto Start;
Console.ReadLine();
End:
Console.WriteLine("Exiting app...");