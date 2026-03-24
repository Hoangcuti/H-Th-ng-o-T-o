using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class AnswerDetail
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public ExamAttempt? Attempt { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
    public int SelectedOptionId { get; set; }
    public QuestionOption? SelectedOption { get; set; }
}

// ===== Certificates =====
