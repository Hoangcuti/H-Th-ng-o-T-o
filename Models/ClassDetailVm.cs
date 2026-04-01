using System.Collections.Generic;

namespace COTHUYPRO.Models
{
    public class ClassExamResultVm
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public double Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public bool IsPassed { get; set; }
        public int TotalQuestions { get; set; }
    }

    public class ClassAssignmentSubmissionVm
    {
        public int SubmissionId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int? Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }

    public class ClassStudentHistoryVm
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public List<ClassExamResultVm> ExamResults { get; set; } = new();
        public List<ClassAssignmentSubmissionVm> AssignmentSubmissions { get; set; } = new();
    }

    public class ClassDetailVm
    {
        public Class ClassInfo { get; set; } = default!;
        public List<ClassStudentHistoryVm> StudentHistories { get; set; } = new();
        public List<Lesson> Lessons { get; set; } = new();
        public List<Exam> Exams { get; set; } = new();
        public List<Assignment> Assignments { get; set; } = new();
    }
}
