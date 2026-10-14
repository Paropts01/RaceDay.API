namespace RaceDay.API.Models
{
    public class Participant
    {
        public int ParticipantID { get; set; }
        public int UserID { get; set; }
        public string? ProfilePictureURL { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? MedicalConditions { get; set; }

        public User User { get; set; }
        public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
    }
}
