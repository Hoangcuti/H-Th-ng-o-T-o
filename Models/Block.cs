using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models
{
    public class Block
    {
        public int Id { get; set; }
        
        [MaxLength(20)]
        public string BlockName { get; set; } = string.Empty; // Block 1, Block 2
        
        public int SemesterId { get; set; }
        public Semester? Semester { get; set; }
        
        public bool IsCurrent { get; set; } = false;

        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
