using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class Exam
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
}

