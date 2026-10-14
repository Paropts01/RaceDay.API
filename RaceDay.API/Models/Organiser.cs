namespace RaceDay.API.Models
{
    public class Organiser
    {
        public int OrganiserID { get; set; }
        public int UserID { get; set; }
        public string? OrganisationName { get; set; }
        public string? Bio { get; set; }
        public string? WebsiteURL { get; set; }

        public User User { get; set; }
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
