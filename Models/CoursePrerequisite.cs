using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class CoursePrerequisite
{
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int PrerequisiteId { get; set; }
    public Course? Prerequisite { get; set; }
}

// ===== Class participation =====
