using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Result
{
    public int ResultID { get; set; }

    [Required]
    public int EnrolmentID { get; set; }
    public EventEnrolment Enrolment { get; set; } = null!;

    public TimeSpan? FinishTime { get; set; }
    public TimeSpan? NetTime { get; set; }
    public int? FinishingPosition { get; set; }
    public int? OverallPlacement { get; set; }
    public int? CategoryPlacement { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Not Started"; // Not Started | In Progress | Finished | DNF | DQ

    public bool IsPublished { get; set; } = false;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
