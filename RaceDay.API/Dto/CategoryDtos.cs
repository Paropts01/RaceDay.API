namespace RaceDay.API.DTOs;

public record CategoryCreateDto(string Name, string? Description, string? CategoryType, int? MinAge, int? MaxAge, decimal? Distance, decimal? EntryFee, int? Capacity);
public record CategoryUpdateDto(string Name, string? Description, string? CategoryType, int? MinAge, int? MaxAge, decimal? Distance, decimal? EntryFee, int? Capacity);
public record CategoryResponseDto(int CategoryID, int EventID, string Name, string? Description, string? CategoryType, int? MinAge, int? MaxAge, decimal? Distance, decimal? EntryFee, int? Capacity);
