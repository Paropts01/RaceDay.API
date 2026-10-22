namespace RaceDay.MVC.Models;

public record UserResponseDto(
    int UserID, string Username, string Email,
    string FirstName, string LastName, string Role);

public record EventResponseDto(
    int EventID, int OrganiserID, string Name, string? Description,
    DateTime EventDate, string Location, decimal Distance,
    string EventType, string? BannerImageURL, int? MaxParticipants,
    DateTime? RegistrationDeadline, bool IsActive);

public record CategoryResponseDto(
    int CategoryID, int EventID, string Name, string? Description,
    string? CategoryType, int? MinAge, int? MaxAge,
    decimal? Distance, decimal? EntryFee, int? Capacity);

public record EnrolmentResponseDto(
    int EnrolmentID, int ParticipantID, int EventID, int CategoryID,
    DateTime EnrolmentDate, string Status, string PaymentStatus,
    int? BibNumber, string? Notes);

public record ProfileResponseDto(
    int UserID, string Username, string Email, string FirstName,
    string LastName, DateTime DateOfBirth, string? PhoneNumber,
    string Role, OrganiserProfileDto? Organiser,
    ParticipantProfileDto? Participant);

public record OrganiserProfileDto(
    int OrganiserID, string? OrganisationName,
    string? Bio, string? WebsiteURL);

public record ParticipantProfileDto(
    int ParticipantID, string? ProfilePictureURL,
    string? EmergencyContactName, string? EmergencyContactPhone,
    string? MedicalConditions);

public record MyEnrolmentDto(
    int EnrolmentID, string Event, DateTime EventDate,
    string Category, string Status, string PaymentStatus,
    int? BibNumber);

public record EventEnrolmentListItemDto(
    int EnrolmentID, string ParticipantName, string Email,
    string Category, string Status, string PaymentStatus,
    int? BibNumber, DateTime EnrolmentDate);

public record MyResultDto(
    int ResultID, string Event, DateTime EventDate, string Category,
    TimeSpan? FinishTime, TimeSpan? NetTime, int? FinishingPosition,
    int? OverallPlacement, int? CategoryPlacement, string Status);

public record ResultListItemDto(
    int ResultID, string ParticipantName, TimeSpan? FinishTime,
    TimeSpan? NetTime, int? FinishingPosition, int? OverallPlacement,
    int? CategoryPlacement, string Status, bool IsPublished);

public record UploadResponse(string Url);

public record EventCreateDto(
    string Name, string? Description, DateTime EventDate,
    string Location, decimal Distance, string EventType,
    string? BannerImageURL, int? MaxParticipants,
    DateTime? RegistrationDeadline);

public record CategoryCreateDto(
    string Name, string? Description, string? CategoryType,
    int? MinAge, int? MaxAge, decimal? Distance,
    decimal? EntryFee, int? Capacity);

public record ResultCreateDto(
    int EnrolmentId, TimeSpan? FinishTime, TimeSpan? NetTime,
    int? FinishingPosition, int? OverallPlacement,
    int? CategoryPlacement, string Status, string? Notes);