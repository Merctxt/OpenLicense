using MediatR;
using OpenLicense.Application.DTOs;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Commands.Auth;

public record RegisterUserCommand(string Name, string Email, string Password) : IRequest<User>;
public record LoginCommand(string Email, string Password) : IRequest<string>;
public record UpdateUserCommand(Guid UserId, string? Name, string? Email, string? Password) : IRequest<User>;
public record DeleteUserCommand(Guid UserId) : IRequest;
public record CreateApiKeyCommand(Guid UserId, string Name) : IRequest<CreateApiKeyResponse>;
public record DeleteApiKeyCommand(Guid UserId, Guid ApiKeyId) : IRequest;
public record ToggleApiKeyCommand(Guid UserId, Guid ApiKeyId) : IRequest<bool>;
public record ForgotPasswordCommand(string Email) : IRequest;
public record VerifyResetTokenCommand(string Email, string Token) : IRequest<bool>;
public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest;
public record UpdateReportPreferencesCommand(Guid UserId, bool ReportsOptIn) : IRequest<User>;
