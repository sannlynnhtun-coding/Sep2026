// See https://aka.ms/new-console-template for more information
using Sep2026.ConsoleApp;

Console.WriteLine("Hello, World!");

// F5 -- Fn + F5
// break point

Console.Write("Please enter Numbe 1: ");
string number1 = Console.ReadLine()!;

Console.Write("Please enter Numbe 2: ");
string number2 = Console.ReadLine()!;

int num1 = Convert.ToInt32(number1);
int num2 = Convert.ToInt32(number2);

int result = Add(num1, num2);

Console.WriteLine("Result is " + result);
Console.WriteLine($"Result is {result}");

//`${code}`

//int numeber;
//double number2;
//decimal number3;
//string name;
//char value;
//bool result;
//DateTime dt; // 01-01-0001 12:00 AM


int a = 0;

string message = a == 0 ? "Success" : "Fail";

if (a == 1)
{

}
else if (a == 2)
{

}
else if (a == 3)
{

}
else if (a == 4)
{

}
else
{

}

EnumTransferType enumTransferType = EnumTransferType.OwnTransfer;

switch (enumTransferType)
{
    case EnumTransferType.OwnTransfer:

        break;
    case EnumTransferType.OtherTransfer:

        break;
    case EnumTransferType.OtherBankTransfer:

        break;
    case EnumTransferType.None:
    default:

        break;
}

for (int i = 0; i < 10; i++)
{
    if (i == 4)
    {
        continue;
    }
    Console.WriteLine(i);
}

int[] numbers = new int[10];
List<int> numbers2 = new List<int>();

//dynamic student = new { name = "mg mg"};
//student.Hehe();

Student student1 = new Student("Ma Ma", 20); // create object, create a instance
//student.Test();
//student1.Age = 18;
//student1.Name = "Mg Mg";
student1.Fees = 1000;
student1.Fees = 2000;
student1.Fees2 = 1000;
student1.Fees2 = 2000;
//student1.FatherName = "U Mg Mg";
student1.SetFatherName("U Mg Mg");

Student student2 = new Student("Mg Mg", 18);

SannLynnHtun sannLynnHtun = new KaungZawHein();

string status = "Pending";
EnumStatusType statusType = EnumStatusType.Pending;

status = statusType.ToString();

int statusCode = (int)statusType;

File.WriteAllText("status.txt", statusCode.ToString());

KaungZawHein kaungZawHein = new KaungZawHein();

try
{
    int[] myNumbers = { 1, 2, 3 };
    Console.WriteLine(myNumbers[10]);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine(e.ToString());
}

Console.ReadLine();

static int Add(int num1, int num2)
{
    return num1 + num2;

    /// code
}

enum EnumStatusType
{
    None,
    Pending,
    Success,
    Fail
}

enum EnumTransferType
{
    None,
    OwnTransfer,
    OtherTransfer,
    OtherBankTransfer
}

public abstract class SannLynnHtun
{
    public abstract void Eat();
    public void Sleep()
    {
        Console.WriteLine("Zzz");
    }
}

// Ctrl + .
public sealed class KaungZawHein : SannLynnHtun
{
    public override void Eat()
    {
    
    }

    public void Run()
    {

    }
}

//public class SannLynnHtun2 : KaungZawHein
//{

//}

public interface IStudent
{
    void Eat();
    void Sleep();
}

public class KaungMyat : IStudent
{
    public void Eat()
    {

    }

    public void Sleep()
    {

    }
}