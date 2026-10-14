namespace RaceDay.API.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? CategoryType { get; set; } // "Age", "Distance", "Other"
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public decimal? Distance { get; set; }
        public decimal? EntryFee { get; set; }
        public int? Capacity { get; set; }

        public Event Event { get; set; }
        public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
    }
}
