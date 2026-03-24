using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace COTHUYPRO.Services
{
    public class MockAiService : IAiService
    {
        public async Task<string> GenerateLessonContentAsync(string topic)
        {
            // Simulate AI delay
            await Task.Delay(2000);

            return $@"<h3>Giới thiệu về {topic}</h3>
<p>{topic} là một chủ đề vô cùng thú vị và mang tính ứng dụng cao. Sau khi học xong bài này, bạn sẽ nắm vững các nguyên lý cốt lõi của {topic}.</p>
<h4>1. Khái niệm cơ bản</h4>
<p>Trong bài học này, chúng ta sẽ bắt đầu làm quen với các định nghĩa nền tảng. Bạn hãy theo dõi video và phân tích ví dụ dưới đây.</p>
<h4>2. Ứng dụng thực tế</h4>
<p>Để hiểu rõ hơn, cách tốt nhất là liên hệ {topic} với các dự án thực tế. Bạn hãy ghi chú lại những key takeaway quan trọng nhé.</p>
<p><em>Gợi ý: (Nội dung này được sinh tự động bởi AI Trợ giảng, bạn có thể chỉnh sửa lại cho phù hợp).</em></p>";
        }

        public async Task<List<QuestionDto>> GenerateQuizQuestionsAsync(string topic, int count)
        {
            // Simulate AI delay
            await Task.Delay(3000);

            var questions = new List<QuestionDto>();
            for (int i = 1; i <= count; i++)
            {
                questions.Add(new QuestionDto
                {
                    Content = $"Câu hỏi do AI sinh ra dựa trên chủ đề '{topic}' (Số {i})",
                    Options = new List<OptionDto>
                    {
                        new OptionDto { Content = "Đáp án A: Lựa chọn sai", IsCorrect = false },
                        new OptionDto { Content = "Đáp án B: Lựa chọn đúng sinh bởi AI", IsCorrect = true },
                        new OptionDto { Content = "Đáp án C: Lựa chọn sai 2", IsCorrect = false },
                        new OptionDto { Content = "Đáp án D: Lựa chọn sai 3", IsCorrect = false }
                    }
                });
            }
            return questions;
        }
    }
}
