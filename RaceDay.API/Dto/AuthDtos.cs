namespace RaceDay.API.DTOs;

public record RegisterDto(string Username, string Email, string Password, string FirstName, string LastName, DateTime DateOfBirth, string? PhoneNumber, string Role);
public record LoginDto(string UsernameOrEmail, string Password);
public record UserResponseDto(int UserID, string Username, string Email, string FirstName, string LastName, string Role);