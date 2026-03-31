using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COTHUYPRO.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace COTHUYPRO.Controllers;

[Authorize]
public class RankingController : Controller
{
    private readonly TrainingContext _context;

    public RankingController(TrainingContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Chá»‰ lấy danh sách học viên
        var students = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role!.Name == "Student"))
            .ToListAsync();

        var rankingData = new List<RankingVm>();

        foreach (var s in students)
        {
            // Quiz: Lấy điểm cao nhất của mỗi bài thi (đã duyệt)
            var quizPoints = 0.0;
            var results = await _context.ExamResults
                .Include(er => er.Attempt)
                .Where(er => er.Attempt != null && er.Attempt.UserId == s.Id)
                .ToListAsync();

            if (results.Any()) {
                quizPoints = results.GroupBy(er => er.Attempt!.ExamId)
                                   .Select(g => g.Max(x => x.Score))
                                   .Sum();
            }

            // Assignment: Tổng điểm bài tập
            var assignmentPoints = await _context.AssignmentSubmissions
                .Where(asub => asub.StudentId == s.Id)
                .SumAsync(asub => asub.Score ?? 0);

            rankingData.Add(new RankingVm
            {
                StudentId = s.Id,
                FullName = s.FullName,
                StudentCode = s.StudentCode ?? $"STU{s.Id:D4}",
                QuizScore = (decimal)quizPoints,
                AssignmentScore = (decimal)assignmentPoints,
                TotalScore = (decimal)(quizPoints + assignmentPoints)
            });
        }

        rankingData = rankingData.OrderByDescending(r => r.TotalScore).ToList();

        // Gán hạng
        for (int i = 0; i < rankingData.Count; i++)
        {
            rankingData[i].Rank = i + 1;
        }

        return View(rankingData);
    }
}

public class RankingVm
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public decimal QuizScore { get; set; }
    public decimal AssignmentScore { get; set; }
    public decimal TotalScore { get; set; }
    public int Rank { get; set; }
}
