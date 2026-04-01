using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COTHUYPRO.Models
{
    public class Semester
    {
        public int Id { get; set; }
        
        [MaxLength(20)]
        public string SemesterName { get; set; } = string.Empty; // Spring, Summer, Fall
        
        public int YearId { get; set; }
        public AcademicYear? Year { get; set; }
        
        public bool IsCurrent { get; set; } = false;

        public ICollection<Block> Blocks { get; set; } = new List<Block>();
    }
}
