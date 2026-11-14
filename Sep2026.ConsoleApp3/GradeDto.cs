using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sep2026.ConsoleApp3
{
    public class GradeDto
    {
        public int GradeId { get; set; }
        public int StudentId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public int Mark { get; set; }
    }
}
