using RaceDay.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Organiser
{
    public int OrganiserID { get; set; }

    [Required]
    public int UserID { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(100)]
    public string? OrganisationName { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(200)]
    public string? WebsiteURL { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}