using System.Net.Http.Json;

namespace BLL.Chat
{
    public class GroqClient
    {
        private readonly HttpClient _http;
        private readonly GroqOptions _options;

        public GroqClient(HttpClient http, GroqOptions options)
        {
            _http = http;
            _options = options;
        }

        public async Task<string> SendAsync(List<ChatMessage> messages)
        {
            var request = new GroqRequest
            {
                Model = _options.Model,
                Messages = messages
            };

            var response = await _http.PostAsJsonAsync("chat/completions", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq API error {(int)response.StatusCode}: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<GroqResponse>();

            return result?.Choices.FirstOrDefault()?.Message?.Content
                   ?? "معلش، ماقدرتش أجهز رد دلوقتي.";
        }
    }
}