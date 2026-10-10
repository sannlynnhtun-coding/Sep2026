using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sep2026.ConsoleApp3
{
    public class GradeService
    {
        public static List<GradeDto> Grades = new List<GradeDto>();

        public void RegisterGrade(string studentNo, string subject, int mark)
        {
            int maxId = Grades.Count == 0 ? 0 : Grades.Max(x => x.GradeId);

            StudentDto item = StudentService.Students.Where(x => x.No == studentNo).FirstOrDefault();

            GradeDto grade = new GradeDto
            {
                GradeId = maxId + 1,
                StudentId = item.Id,
                Subject = subject,
                Mark = mark
            };
            Grades.Add(grade);

            Console.WriteLine($"Grade registered successfully for student {item.Name} in subject {subject} with mark {mark}.");
        }
    }
}
