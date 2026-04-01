using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models
{
    public class AcademicYear
    {
        public int Id { get; set; }
        public int YearNumber { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Semester> Semesters { get; set; } = new List<Semester>();
    }
}
