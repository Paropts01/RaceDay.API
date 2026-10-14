namespace RaceDay.API.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; } // "Run", "Walk", "Cycle"
        public string? BannerImageURL { get; set; }
        public int? MaxParticipants { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Organiser Organiser { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
    }
}
