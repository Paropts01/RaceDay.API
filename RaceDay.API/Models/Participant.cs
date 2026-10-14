using RaceDay.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Participant
{
    public int ParticipantID { get; set; }

    [Required]
    public int UserID { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(500)]
    public string? ProfilePictureURL { get; set; }

    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }

    [MaxLength(500)]
    public string? MedicalConditions { get; set; }

    public ICollection<EventEnrolment> EventEnrolments { get; set; } = new List<EventEnrolment>();
}
