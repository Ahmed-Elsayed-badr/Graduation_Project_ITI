namespace BLL.Chat
{
    
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;     
        public string Content { get; set; } = string.Empty;  
    }

  
    public class GroqRequest
    {
        public string Model { get; set; } = string.Empty;
        public List<ChatMessage> Messages { get; set; } = new();
        public double Temperature { get; set; } = 0.3;
    }

    
    public class GroqResponse
    {
        public List<GroqChoice> Choices { get; set; } = new();
    }

    public class GroqChoice
    {
        public ChatMessage? Message { get; set; }
    }
}