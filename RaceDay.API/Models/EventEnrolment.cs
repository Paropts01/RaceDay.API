using RaceDay.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class EventEnrolment
{
    [Key]
    public int EnrolmentID { get; set; }

    [Required]
    public int ParticipantID { get; set; }
    public Participant Participant { get; set; } = null!;

    [Required]
    public int EventID { get; set; }
    public Event Event { get; set; } = null!;

    [Required]
    public int CategoryID { get; set; }
    public Category Category { get; set; } = null!;

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending | Confirmed | Cancelled | Completed

    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending"; // Pending | Paid | Refunded

    public int? BibNumber { get; set; }
    public DateTime? StartTime { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public Result? Result { get; set; }
}
