using System.Threading.Tasks;
using System.Collections.Generic;

namespace COTHUYPRO.Services
{
    public interface IAiService
    {
        Task<string> GenerateLessonContentAsync(string topic);
        Task<List<QuestionDto>> GenerateQuizQuestionsAsync(string topic, int count);
    }

    public class QuestionDto 
    {
        public string Content { get; set; } = string.Empty;
        public List<OptionDto> Options { get; set; } = new();
    }

    public class OptionDto
    {
        public string Content { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
