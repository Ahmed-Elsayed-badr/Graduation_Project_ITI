namespace Graduation_Project_ITI.Models.Admin
{
    public class AdminUserViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // حسابات لا يجوز إيقافها من الواجهة
        public bool IsAdministrator { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}