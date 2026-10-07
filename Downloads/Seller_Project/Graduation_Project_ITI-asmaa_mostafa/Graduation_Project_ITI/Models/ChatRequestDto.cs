using BLL.Chat;

namespace Graduation_Project_ITI.Models
{
    public class ChatRequestDto
    {
        public List<ChatMessage> Messages { get; set; } = new();
    }
}