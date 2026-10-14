namespace RaceDay.Api.DTOs;

public record EnrolDto(int CategoryId);

public record EnrolmentResponseDto(
    int EnrolmentID,
    int ParticipantID,
    int EventID,
    int CategoryID,
    DateTime EnrolmentDate,
    string Status,
    string PaymentStatus,
    int? BibNumber,
    string? Notes
);

public record EnrolmentStatusUpdateDto(string Status);