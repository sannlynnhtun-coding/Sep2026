using Sep2026.ConsoleApp3;

// Create the services once, then use them from the menu.
StudentService studentService = new StudentService();
GradeService gradeService = new GradeService();
bool running = true;

while (running)
{
    Console.WriteLine("Student Management");
    Console.WriteLine("1. Create student");
    Console.WriteLine("2. Create grade");
    Console.WriteLine("3. Exit");
    Console.WriteLine("4. List students");
    Console.WriteLine("5. Update student");
    Console.WriteLine("6. Delete student");
    Console.WriteLine("7. List grades");
    Console.WriteLine("8. Update grade");
    Console.WriteLine("9. Delete grade");
    Console.Write("Choose an option: ");
    string? option = Console.ReadLine();

    if (option == null)
    {
        break;
    }

    // Each case calls one service method.
    switch (option)
    {
        case "1":
        {
            string no = ReadText("Student no: ");
            string name = ReadText("Student name: ");
            string classNo = ReadText("Class no: ");
            studentService.RegisterStudent(no, name, classNo);
            break;
        }
        case "2":
        {
            string studentNo = ReadText("Student no: ");
            string subject = ReadText("Subject: ");
            int mark = ReadNumber("Mark: ");
            gradeService.RegisterGrade(studentNo, subject, mark);
            break;
        }
        case "3":
        {
            running = false;
            break;
        }
        case "4":
        {
            studentService.ShowStudents();
            break;
        }
        case "5":
        {
            int id = ReadNumber("Student Id: ");
            string no = ReadText("New student no: ");
            string name = ReadText("New student name: ");
            string classNo = ReadText("New class no: ");
            studentService.UpdateStudent(id, no, name, classNo);
            break;
        }
        case "6":
        {
            int id = ReadNumber("Student Id to delete: ");
            studentService.DeleteStudent(id);
            break;
        }
        case "7":
        {
            gradeService.ShowGrades();
            break;
        }
        case "8":
        {
            int gradeId = ReadNumber("Grade Id: ");
            string studentNo = ReadText("Student no: ");
            string subject = ReadText("New subject: ");
            int mark = ReadNumber("New mark: ");
            gradeService.UpdateGrade(gradeId, studentNo, subject, mark);
            break;
        }
        case "9":
        {
            int gradeId = ReadNumber("Grade Id to delete: ");
            gradeService.DeleteGrade(gradeId);
            break;
        }
        default:
        {
            Console.WriteLine("Please choose an option from 1 to 9.");
            break;
        }
    }

    Console.WriteLine();
}

Console.WriteLine("Exiting app...");

// Keep asking until the user enters text.
static string ReadText(string message)
{
    while (true)
    {
        Console.Write(message);
        string? input = Console.ReadLine();
        if (input == null)
        {
            throw new EndOfStreamException();
        }
        if (!string.IsNullOrWhiteSpace(input))
        {
            return input.Trim();
        }
        Console.WriteLine("Please enter a value.");
    }
}

// TryParse checks the number without crashing on text such as "abc".
static int ReadNumber(string message)
{
    while (true)
    {
        string input = ReadText(message);
        int number;
        bool isNumber = int.TryParse(input, out number);
        if (isNumber)
        {
            return number;
        }
        Console.WriteLine("Please enter a whole number.");
    }
}
