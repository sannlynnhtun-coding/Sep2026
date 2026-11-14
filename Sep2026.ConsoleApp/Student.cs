using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sep2026.ConsoleApp
{
    public class Student
    {
        //public Student()
        //{
        //    Name = "Default Name";
        //}

        public Student(string name, int age)
        {
            Name = name;
            Age = age;
        }

        //public int Age; // fields
        //public string Name;

        public int Age { get; set; } // properties
        public string Name { get; set; }
        public string? FatherName { get; private set; }
        public decimal Fees { get; set; }

        private decimal fees2; // field
        public decimal Fees2   // property
        {
            get { return fees2; }
            set
            {
                Console.WriteLine("Old value: " + fees2);
                fees2 = value;
                Console.WriteLine("New value: " + fees2);
            }
        }

        public int Test() // method
        {
            return 0;
        }

        public void SetFatherName(string fatherName)
        {
            FatherName = fatherName;
        }
    }
}
