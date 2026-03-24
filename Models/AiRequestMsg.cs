namespace COTHUYPRO.Models
{
    public class AiRequestMsg
    {
        public string Topic { get; set; } = string.Empty;
    }

    public class AiExamRequestMsg
    {
        public string Topic { get; set; } = string.Empty;
        public int ExamId { get; set; }
        public int Count { get; set; } = 5;
    }
}
