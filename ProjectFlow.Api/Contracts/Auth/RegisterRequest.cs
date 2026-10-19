namespace ProjectFlow.Api.Contracts.Auth;

public record RegisterRequest(
    string UserName,
    string Email,
    string Password,
    string DisplayName);