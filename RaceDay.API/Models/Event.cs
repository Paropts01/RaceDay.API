using RaceDay.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Event
{
    public int EventID { get; set; }

    [Required]
    public int OrganiserID { get; set; }
    public Organiser Organiser { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = null!;

    [Required]
    public decimal Distance { get; set; }

    [Required, MaxLength(20)]
    public string EventType { get; set; } = null!; // Run | Walk | Cycle

    [MaxLength(500)]
    public string? BannerImageURL { get; set; }

    public int? MaxParticipants { get; set; }
    public DateTime? RegistrationDeadline { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<EventEnrolment> EventEnrolments { get; set; } = new List<EventEnrolment>();
}
