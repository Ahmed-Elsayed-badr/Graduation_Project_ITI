using BLL.Chat;
using Graduation_Project_ITI.Models;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project_ITI.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatService _chat;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ChatService chat, ILogger<ChatController> logger)
        {
            _chat = chat;
            _logger = logger;
        }

        // POST /Chat/Send
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ChatRequestDto request)
        {
            if (request?.Messages == null || request.Messages.Count == 0)
                return BadRequest();

            try
            {
                var reply = await _chat.AskAsync(request.Messages);
                return Json(new { reply });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chatbot error");
                return Json(new { reply = "معلش، حصلت مشكلة. جرّب تاني بعد شوية." });
            }
        }
    }
}