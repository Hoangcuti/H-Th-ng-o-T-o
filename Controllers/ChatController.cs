using COTHUYPRO.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http.Json;

namespace COTHUYPRO.Controllers;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<ChatTurn>? History { get; set; }
}

public class ChatTurn
{
    public string Role { get; set; } = "user"; // "user" hoáº·c "model"
    public string Content { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
}

public class ChatController : Controller
{
    private readonly IConfiguration _config;
    private readonly TrainingContext _db;

    // Inject configuration vÃ  DbContext Ä‘á»ƒ chatbot láº¥y danh má»¥c khÃ³a há»c ná»™i bá»™
    public ChatController(IConfiguration config, TrainingContext db)
    {
        _config = config;
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        string apiKey = _config["Gemini:ApiKey"]?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Json(new { response = "Thiáº¿u khÃ³a Gemini API. Vui lÃ²ng cáº¥u hÃ¬nh `Gemini:ApiKey` trong appsettings." });
        }

        // Danh má»¥c khÃ³a há»c + há»c phÃ­ (cÃ¹ng cÃ´ng thá»©c hiá»ƒn thá»‹ á»Ÿ Views/Courses)
        var catalog = _db.Courses
            .Select(c => new
            {
                c.CourseName,
                Category = c.Category != null ? c.Category.Name : "KhÃ¡c",
                Level = c.Level != null ? c.Level.Name : "KhÃ´ng rÃµ trÃ¬nh Ä‘á»™",
                Price = 1_200_000 + c.Id * 150_000,
                Link = $"/Courses/Details/{c.Id}"
            })
            .ToList();

        var catalogText = string.Join("\n", catalog.Select(c =>
            $"- {c.CourseName} ({c.Category}, {c.Level}) â€“ khoáº£ng {c.Price:N0} Ä‘ â€“ Link: {c.Link}"));

        // Prompt Ä‘Ã³ng Ä‘Æ°a vÃ o systemInstruction Ä‘á»ƒ model luÃ´n bÃ¡m
        var prompt = $"Danh má»¥c khÃ³a há»c ná»™i bá»™ vÃ  há»c phÃ­ tham kháº£o:\n{catalogText}\n\n" +
                     "HÆ°á»›ng dáº«n tráº£ lá»i:\n" +
                     "- Chá»‰ dÃ¹ng danh sÃ¡ch trÃªn; khÃ´ng bá»‹a ná»™i dung ngoÃ i.\n" +
                     "- Náº¿u ngÆ°á»i dÃ¹ng há»i lÃ m web/website/app: Æ°u tiÃªn Software Development / .NET Backend.\n" +
                     "- Náº¿u ngÆ°á»i dÃ¹ng há»i \"ngÃ nh nÃ o lÃ m vá» web\" hoáº·c tÆ°Æ¡ng tá»±: tráº£ lá»i ngáº¯n 1-2 cÃ¢u nÃªu Software Development / .NET Backend, mÃ´ táº£ ngáº¯n (2 Ã½), há»c phÃ­ tham kháº£o, vÃ  ghi dÃ²ng riÃªng dáº¡ng 'ÄÄƒng kÃ½: <link>'.\n" +
                     "- Náº¿u ngÆ°á»i dÃ¹ng nÃ³i mÃ¬nh má»›i, sá»£ nÃ¢ng cao: Æ°u tiÃªn cÃ¡c khÃ³a Level 'CÆ¡ báº£n' hoáº·c giÃ¡ tháº¥p nháº¥t; gá»£i Ã½ 1-2 khÃ³a dá»… báº¯t Ä‘áº§u.\n" +
                     "- LuÃ´n kÃ¨m há»c phÃ­ (giÃ¡ tham kháº£o) vÃ  link Ä‘Äƒng kÃ½ (Link á»Ÿ danh sÃ¡ch trÃªn), tráº£ lá»i ngáº¯n gá»n tiáº¿ng Viá»‡t, dÃ¹ng bullet náº¿u thÃ­ch há»£p.\n" +
                     "- Káº¿t thÃºc báº±ng cÃ¢u há»i ngáº¯n Ä‘á»ƒ tiáº¿p tá»¥c tÆ° váº¥n (vÃ­ dá»¥: 'Báº¡n muá»‘n Ä‘Äƒng kÃ½ ngay hay nháº­n lá»™ trÃ¬nh chi tiáº¿t?').\n" +
                     "- Náº¿u cÃ¢u há»i khÃ´ng liÃªn quan Ä‘Ã o táº¡o, lá»‹ch, há»c phÃ­: lá»‹ch sá»± tá»« chá»‘i vÃ  má»i quay láº¡i chá»§ Ä‘á» khÃ³a há»c.";

        // Gá»™p lá»‹ch sá»­ há»™i thoáº¡i (tá»‘i Ä‘a 10 lÆ°á»£t gáº§n nháº¥t) + message má»›i
        var turns = new List<object>();

        var history = request.History ?? new List<ChatTurn>();
        foreach (var h in history.TakeLast(10))
        {
            var role = h.Role == "model" ? "model" : "user";
            turns.Add(new
            {
                role,
                parts = new[] { new { text = h.Content } }
            });
        }
        // thÃªm cÃ¢u há»i hiá»‡n táº¡i
        turns.Add(new
        {
            role = "user",
            parts = new[] { new { text = request.Message } }
        });

        using var client = new HttpClient();

        // Endpoint Gemini 2.5 Flash
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        var body = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = prompt } }
            },
            contents = turns.ToArray(),
            generationConfig = new
            {
                temperature = 0.6,
                topP = 0.9,
                maxOutputTokens = 400,
                candidateCount = 1
            }
        };

        string FormatRetryDelay(string retryDelay)
        {
            if (string.IsNullOrWhiteSpace(retryDelay)) return "vÃ i chá»¥c giÃ¢y";
            var trimmed = retryDelay.Trim();
            if (trimmed.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed[..^1];
            }
            if (double.TryParse(trimmed, out var sec))
            {
                if (sec < 90) return $"{Math.Round(sec)} giÃ¢y";
                var minutes = Math.Round(sec / 60, 1);
                return $"{minutes} phÃºt";
            }
            return retryDelay;
        }

        try
        {
            var response = await client.PostAsJsonAsync(url, body);
            var resultText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 429)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(resultText);
                        if (doc.RootElement.TryGetProperty("error", out var err))
                        {
                            string retry = null;
                            if (err.TryGetProperty("details", out var details) && details.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var d in details.EnumerateArray())
                                {
                                    if (d.TryGetProperty("@type", out var t) && t.GetString()?.Contains("RetryInfo") == true &&
                                        d.TryGetProperty("retryDelay", out var rd))
                                    {
                                        retry = rd.GetString();
                                        break;
                                    }
                                }
                            }
                            var retryText = FormatRetryDelay(retry);
                            var msg = err.TryGetProperty("message", out var m) ? m.GetString() : "Vượt hạn mức miễn phí.";
                            return Json(new { response = $"Hệ thống vượt hạn mức (429). {msg} Vui lòng chờ khoảng {retryText} rồi thử lại, hoặc dùng API key có hạn mức cao hơn." });
                        }
                    }
                    catch
                    {
                        // ignore and fall back
                    }

                    return Json(new { response = "Hệ thống vượt hạn mức (429). Vui lòng chờ ít phút rồi thử lại hoặc dùng API key trả phí." });
                }

                return Json(new { response = $"Lỗi từ Google (HTTP {(int)response.StatusCode}): {resultText}" });
            }try
            {
                using var doc = JsonDocument.Parse(resultText);

                if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0 &&
                    parts[0].TryGetProperty("text", out var textNode))
                {
                    var botReply = textNode.GetString();
                    if (!string.IsNullOrWhiteSpace(botReply))
                    {
                        return Json(new { response = botReply });
                    }
                }

                // Khi bá»‹ cháº·n an toÃ n (khÃ´ng cÃ³ candidates) nhÆ°ng cÃ³ promptFeedback
                if (doc.RootElement.TryGetProperty("promptFeedback", out var feedback))
                {
                    var reason = feedback.TryGetProperty("blockReason", out var br) ? br.GetString() : "Ná»™i dung bá»‹ cháº·n.";
                    return Json(new { response = $"Xin lá»—i, ná»™i dung nÃ y bá»‹ cháº·n bá»Ÿi bá»™ lá»c: {reason}. Báº¡n thá»­ diá»…n Ä‘áº¡t khÃ¡c giÃºp mÃ¬nh nhÃ©." });
                }

                return Json(new { response = "Há»‡ thá»‘ng chÆ°a nháº­n Ä‘Æ°á»£c cÃ¢u tráº£ lá»i há»£p lá»‡. Báº¡n thá»­ láº¡i hoáº·c há»i cÃ¢u khÃ¡c nhÃ©." });
            }
            catch (Exception parseEx)
            {
                return Json(new { response = $"Lá»—i phÃ¢n tÃ­ch pháº£n há»“i: {parseEx.Message}" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { response = "Lá»—i káº¿t ná»‘i: " + ex.Message });
        }
    }
}
