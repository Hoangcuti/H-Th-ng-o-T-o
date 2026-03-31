using COTHUYPRO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace COTHUYPRO.Controllers;

[Authorize(Roles = "Student,Admin")]
public class StudentQuizController : Controller
{
    private readonly TrainingContext _context;

    public StudentQuizController(TrainingContext context)
    {
        _context = context;
    }

    // Trang chuẩn bị (Screenshot 2)
    public async Task<IActionResult> Start(int id)
    {
        var exam = await _context.Exams.Include(e => e.Course).FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null) return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        // Kiểm tra xem có đang làm dở không
        var unfinished = await _context.ExamAttempts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.ExamId == id && a.Status == "Started");

        ViewBag.HasUnfinished = unfinished != null;
        ViewBag.TotalQuestions = await _context.Questions.CountAsync(q => q.ExamId == id);

        return View(exam);
    }

    // Trang làm tập trung (Screenshot 3)
    public async Task<IActionResult> Take(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null) return NotFound();

        var attempt = await _context.ExamAttempts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.ExamId == id && a.Status == "Started");

        if (attempt == null)
        {
            attempt = new ExamAttempt
            {
                UserId = userId,
                ExamId = id,
                Status = "Started",
                RemainingSeconds = exam.DurationMinutes * 60,
                StartedAt = DateTime.Now
            };
            _context.ExamAttempts.Add(attempt);
            await _context.SaveChangesAsync();
        }

        var questions = await _context.Questions
            .Include(q => q.Options)
            .Where(q => q.ExamId == id)
            .ToListAsync();

        ViewBag.Attempt = attempt;
        return View(questions);
    }

    // AJAX: Lưu tiến độ (Lưu đáp án đã chọn & thời gian còn lại)
    [HttpPost]
    public async Task<IActionResult> SaveProgress(int attemptId, string answersJson, int remaining)
    {
        var attempt = await _context.ExamAttempts.FindAsync(attemptId);
        if (attempt == null || attempt.Status != "Started") return BadRequest();

        attempt.AnswersJson = answersJson;
        attempt.RemainingSeconds = remaining;
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    // Submit bài: Tính điểm & Trình bày kết quả (Screenshot 4)
    [HttpPost]
    public async Task<IActionResult> Submit(int attemptId, string finalAnswersJson)
    {
        var attempt = await _context.ExamAttempts
            .Include(a => a.Exam)
            .FirstOrDefaultAsync(a => a.Id == attemptId);
        if (attempt == null || attempt.Status != "Started") return BadRequest();

        var questions = await _context.Questions
            .Include(q => q.Options)
            .Where(q => q.ExamId == attempt.ExamId)
            .ToListAsync();

        // Parse answers
        var userAnswers = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(finalAnswersJson))
        {
            userAnswers = JsonSerializer.Deserialize<Dictionary<string, object>>(finalAnswersJson) ?? new();
        }

        int correctCount = 0;
        foreach (var q in questions)
        {
            if (userAnswers.TryGetValue(q.Id.ToString(), out var selectedIdObj))
            {
                var selectedIdStr = selectedIdObj?.ToString();
                if (int.TryParse(selectedIdStr, out var selectedId))
                {
                    var opt = q.Options.FirstOrDefault(o => o.Id == selectedId);
                    if (opt?.IsCorrect == true) correctCount++;
                }
            }
        }

        double score = (double)correctCount / questions.Count * 10.0;
        attempt.Status = "Completed";
        attempt.CompletedAt = DateTime.Now;
        attempt.AnswersJson = finalAnswersJson;

        var result = new ExamResult
        {
            AttemptId = attemptId,
            Score = Math.Round(score, 1),
            CorrectCount = correctCount,
            TotalQuestions = questions.Count
        };
        _context.ExamResults.Add(result);

        // AUTO-COMPLETE LESSON IF 100% CORRECT
        if (correctCount == questions.Count && questions.Count > 0 && attempt.Exam!.LessonId != null)
        {
            var userId = attempt.UserId;
            var lessonId = attempt.Exam.LessonId.Value;
            var progress = await _context.LearningProgresses
                .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);
            
            if (progress == null)
            {
                _context.LearningProgresses.Add(new LearningProgress 
                { 
                    UserId = userId, 
                    LessonId = lessonId, 
                    Status = "Completed" 
                });
            }
            else
            {
                progress.Status = "Completed";
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Result", new { id = result.Id });
    }

    public async Task<IActionResult> Result(int id)
    {
        var result = await _context.ExamResults
            .Include(r => r.Attempt).ThenInclude(a => a!.Exam)
            .FirstOrDefaultAsync(r => r.Id == id);
        return View(result);
    }

    // Review đáp án (Screenshot 5)
    public async Task<IActionResult> Review(int id)
    {
        var result = await _context.ExamResults
            .Include(r => r.Attempt).ThenInclude(a => a!.Exam)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (result == null) return NotFound();

        var questions = await _context.Questions
            .Include(q => q.Options)
            .Where(q => q.ExamId == result.Attempt!.ExamId)
            .ToListAsync();

        ViewBag.UserAnswers = JsonSerializer.Deserialize<Dictionary<string, object>>(result.Attempt!.AnswersJson ?? "{}");
        return View(questions);
    }
}
