using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class ClassSchedule
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public Class? Class { get; set; }
    public DateTime ScheduleDate { get; set; }
}

