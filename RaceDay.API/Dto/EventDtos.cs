namespace RaceDay.Api.DTOs;

public record EventCreateDto(
    string Name,
    string? Description,
    DateTime EventDate,
    string Location,
    decimal Distance,
    string EventType,
    string? BannerImageURL,
    int? MaxParticipants,
    DateTime? RegistrationDeadline
);

public record EventUpdateDto(
    string Name,
    string? Description,
    DateTime EventDate,
    string Location,
    decimal Distance,
    string EventType,
    string? BannerImageURL,
    int? MaxParticipants,
    DateTime? RegistrationDeadline
);

public record EventResponseDto(
    int EventID,
    int OrganiserID,
    string Name,
    string? Description,
    DateTime EventDate,
    string Location,
    decimal Distance,
    string EventType,
    string? BannerImageURL,
    int? MaxParticipants,
    DateTime? RegistrationDeadline,
    bool IsActive
);
