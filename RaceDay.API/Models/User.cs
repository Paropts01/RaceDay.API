namespace RaceDay.API.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } // "Organiser" or "Participant"
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Organiser? Organiser { get; set; }
        public Participant? Participant { get; set; }
    }
}
