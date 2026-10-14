namespace RaceDay.API.DTOs;

public record ResultCreateDto(int EnrolmentId, TimeSpan? FinishTime, TimeSpan? NetTime, int? FinishingPosition, int? OverallPlacement, int? CategoryPlacement, string Status, string? Notes);
public record ResultUpdateDto(TimeSpan? FinishTime, TimeSpan? NetTime, int? FinishingPosition, int? OverallPlacement, int? CategoryPlacement, string Status, string? Notes);
public record ResultResponseDto(int ResultID, int EnrolmentID, TimeSpan? FinishTime, TimeSpan? NetTime, int? FinishingPosition, int? OverallPlacement, int? CategoryPlacement, string Status, bool IsPublished, string? Notes);
