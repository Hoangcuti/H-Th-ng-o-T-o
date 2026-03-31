using System;
using System.Collections.Generic;

namespace COTHUYPRO.Models;

public class StudentDetailsVm
{
    public User User { get; set; } = null!;
    public List<CourseProgressVm> CourseProgresses { get; set; } = new();
    public List<StudentQuizResultVm> QuizResults { get; set; } = new();
}

public class CourseProgressVm
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int ProgressPercent => TotalLessons > 0 ? (CompletedLessons * 100 / TotalLessons) : 0;
}

public class StudentQuizResultVm
{
    public string ExamTitle { get; set; } = string.Empty;
    public double Score { get; set; }
    public double MaxScore { get; set; } = 10.0;
    public DateTime AttemptDate { get; set; }
    public bool IsPassed { get; set; }
}
