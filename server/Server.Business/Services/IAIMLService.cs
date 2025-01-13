using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Service.Business.Services
{
    public interface IAIMLService
    {
        Task<List<string>> GetGross(string inputText);

    }
    public class AIMLService : IAIMLService
    {
        private readonly HttpClient _httpClient;
        List<string> offensiveWords = new List<string>
        {
            "lồn",
            "bím",
            "buồi",
            "chim",
            "dái",
            "cu",
            "đụ",
            "cặc",
            "chó",
            "khốn nạn",
            "ngu",
            "điên",
            "mất dạy",
            "súc vật",
            "vô học",
            "mẹ kiếp",
            "con mẹ mày",
            "địt",
            "bố láo",
            "láo toét",
            "thằng chó",
            "đồ con lợn",
            "cái loại mất dạy",
            "đụ mẹ mày",
            "đéo biết",
            "éo",
            "địt mẹ",
            "đéo hiểu",
            "địt bà",
            "đồ quỷ",
            "mặt lồn",
            "mặt cặc",
            "mặt ngu",
            "ngu như chó",
            "chết mẹ",
            "bố mày",
            "con đĩ",
            "mẹ mày",
            "cha mày",
            "ông nội mày",
            "đồ chó má",
            "loại chó",
            "khốn kiếp",
            "láo xược",
            "cái đồ súc sinh",
            "ngu si đần độn",
            "loại rác rưởi",
            "vãi lồn",
            "vãi cặc",
            "phịch",
            "bựa",
            "sàm lồn"
        };

        public AIMLService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AIML");
        }
        public async Task<List<string>> GetGross(string inputText)
        {
            const int maxLength = 255;
            var prompt = $@"Hãy liệt kê các từ thô tục có trong đoạn văn sau, mỗi từ cách nhau bằng dấu ','; nếu không có thì trả về dấu '.': {inputText}";
            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 300,
                temperature = 0.1,
                stream = false
            };
            List<string> result = new List<string>();
            HashSet<string> foundWords = new HashSet<string>(offensiveWords
                .Where(x => inputText.Contains(x, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Trim())
            );
            result.AddRange(foundWords);

            var jsonContent = JsonSerializer.Serialize(request);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("chat/completions", httpContent);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    var readResponse = await response.Content.ReadAsStringAsync();
                    var responseString = JsonSerializer.Deserialize<ChatCompletionResponse>(readResponse);
                    if (responseString != null)
                    {
                        var content = responseString.Choices.FirstOrDefault()?.Message?.Content.Trim();
                        if (content != ".")
                        {
                            var contentList = content.Split(',')
                            .Select(x => x.Trim())
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .ToList();

                            result.AddRange(contentList);
                            result = result.Distinct().ToList();
                            return result;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            return result;
        }

        public string FormatPrompt(string question, string correctAnswer, string answerText)
        {
            const int maxLength = 255;

            string prompt = $"Câu hỏi: {question} Đáp án: {correctAnswer} Trả lời: {answerText} Yêu cầu: Câu trả lời có đúng không?(Trả lời 'true' nếu đúng hoặc 'false' nếu sai)";

            if (prompt.Length > maxLength)
            {
                int excessLength = prompt.Length - maxLength;

                if (correctAnswer.Length > 0)
                {
                    int reduceCorrectAnswer = Math.Min(correctAnswer.Length, excessLength);
                    correctAnswer = correctAnswer.Substring(0, correctAnswer.Length - reduceCorrectAnswer);
                    excessLength -= reduceCorrectAnswer;
                }

                if (excessLength > 0 && answerText.Length > 0)
                {
                    int reduceAnswerText = Math.Min(answerText.Length, excessLength);
                    answerText = answerText.Substring(0, answerText.Length - reduceAnswerText);
                }

                // Reformat the prompt
                prompt = $"Câu hỏi: {question} Đáp án: {correctAnswer} Trả lời: {answerText} Yêu cầu: Câu trả lời có đúng không?(Trả lời 'true' nếu đúng hoặc 'false' nếu sai)";
            }

            return prompt;
        }
    }

    public class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("logprobs")]
        public object Logprobs { get; set; }

        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("refusal")]
        public object Refusal { get; set; }
    }
}
