// 1. Force the console to input and output UTF-8 text
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// 2. Output Burmese Unicode text
Console.WriteLine("မင်္ဂလာပါ (Hello in Burmese)");

// 3. Keep console open to verify
Console.ReadLine();