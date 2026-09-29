using MediatR;
using OpenLicense.Application.Commands.Auth;
using OpenLicense.Application.DTOs;
using OpenLicense.Application.Queries.Auth;
using OpenLicense.Application.Services.Interfaces;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Handlers.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, User>
{
    private readonly IAuthService _authService;

    public RegisterUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<User> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(request.Name, request.Email, request.Password);
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password);
    }
}

public class GetMeCommandHandler : IRequestHandler<GetMeQuery, User>
{
    private readonly IAuthService _authService;

    public GetMeCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<User> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        return await _authService.GetMeAsync(request.UserId);
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, User>
{
    private readonly IAuthService _authService;

    public UpdateUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<User> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        return await _authService.UpdateAsync(request.UserId, request.Name, request.Email, request.Password);
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IAuthService _authService;

    public DeleteUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await _authService.DeleteAsync(request.UserId);
    }
}

public class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, CreateApiKeyResponse>
{
    private readonly IAuthService _authService;

    public CreateApiKeyCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<CreateApiKeyResponse> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        return await _authService.CreateApiKeyAsync(request.UserId, new DTOs.CreateApiKeyRequest { Name = request.Name });
    }
}

public class DeleteApiKeyCommandHandler : IRequestHandler<DeleteApiKeyCommand>
{
    private readonly IAuthService _authService;

    public DeleteApiKeyCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task Handle(DeleteApiKeyCommand request, CancellationToken cancellationToken)
    {
        await _authService.DeleteApiKeyAsync(request.UserId, request.ApiKeyId);
    }
}

public class ToggleApiKeyCommandHandler : IRequestHandler<ToggleApiKeyCommand, bool>
{
    private readonly IAuthService _authService;

    public ToggleApiKeyCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(ToggleApiKeyCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ToggleApiKeyAsync(request.UserId, request.ApiKeyId);
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IAuthService _authService;

    public ForgotPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(request.Email);
    }
}

public class VerifyResetTokenCommandHandler : IRequestHandler<VerifyResetTokenCommand, bool>
{
    private readonly IAuthService _authService;

    public VerifyResetTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(VerifyResetTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authService.VerifyResetTokenAsync(request.Email, request.Token);
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
    }
}

public class UpdateReportPreferencesCommandHandler : IRequestHandler<UpdateReportPreferencesCommand, User>
{
    private readonly IAuthService _authService;

    public UpdateReportPreferencesCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<User> Handle(UpdateReportPreferencesCommand request, CancellationToken cancellationToken)
    {
        return await _authService.UpdateReportPreferencesAsync(request.UserId, request.ReportsOptIn);
    }
}
