namespace RaceDay.API.Models
{
    public class EventEnrolment
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public int EventID { get; set; }
        public int CategoryID { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Refunded
        public int? BibNumber { get; set; }
        public DateTime? StartTime { get; set; }
        public string? Notes { get; set; }

        public Participant Participant { get; set; }
        public Event Event { get; set; }
        public Category Category { get; set; }
        public Result? Result { get; set; }
    }
}
