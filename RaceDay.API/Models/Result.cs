namespace RaceDay.API.Models
{
    public class Result
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public TimeSpan? NetTime { get; set; }
        public int? FinishingPosition { get; set; }
        public int? OverallPlacement { get; set; }
        public int? CategoryPlacement { get; set; }
        public string Status { get; set; } = "Not Started"; // Not Started, In Progress, Finished, DNF, DQ
        public bool IsPublished { get; set; } = false;
        public string? Notes { get; set; }

        public EventEnrolment Enrolment { get; set; }
    }
}
