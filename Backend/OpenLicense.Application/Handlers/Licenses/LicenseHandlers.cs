using MediatR;
using OpenLicense.Application.Commands.Licenses;
using OpenLicense.Application.DTOs;
using OpenLicense.Application.Queries.Licenses;
using OpenLicense.Application.Services.Interfaces;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Handlers.Licenses;

public class CreateLicenseCommandHandler : IRequestHandler<CreateLicenseCommand, License>
{
    private readonly ILicenseService _licenseService;

    public CreateLicenseCommandHandler(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    public async Task<License> Handle(CreateLicenseCommand request, CancellationToken cancellationToken)
    {
        return await _licenseService.CreateLicenseAsync(request.UserId, request.ProductId, new CreateLicenseRequest { ProductId = request.ProductId, Name = request.Name, ExpiresAt = request.ExpiresAt, MaxActivations = request.MaxActivations });
    }
}

public class UpdateLicenseCommandHandler : IRequestHandler<UpdateLicenseCommand, License>
{
    private readonly ILicenseService _licenseService;

    public UpdateLicenseCommandHandler(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    public async Task<License> Handle(UpdateLicenseCommand request, CancellationToken cancellationToken)
    {
        return await _licenseService.UpdateLicenseAsync(request.UserId, request.LicenseId, new UpdateLicenseRequest { LicenseId = request.LicenseId, Name = request.Name, ExpiresAt = request.ExpiresAt, MaxActivations = request.MaxActivations, Status = request.Status });
    }
}

public class DeleteLicenseCommandHandler : IRequestHandler<DeleteLicenseCommand>
{
    private readonly ILicenseService _licenseService;

    public DeleteLicenseCommandHandler(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    public async Task Handle(DeleteLicenseCommand request, CancellationToken cancellationToken)
    {
        await _licenseService.DeleteLicenseAsync(request.UserId, request.LicenseId);
    }
}

public class GetLicensesByProductIdQueryHandler : IRequestHandler<GetLicensesByProductIdQuery, IEnumerable<License>>
{
    private readonly ILicenseService _licenseService;

    public GetLicensesByProductIdQueryHandler(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    public async Task<IEnumerable<License>> Handle(GetLicensesByProductIdQuery request, CancellationToken cancellationToken)
    {
        return await _licenseService.GetLicensesByProductIdAsync(request.UserId, request.ProductId);
    }
}

public class GetLicenseActivationsQueryHandler : IRequestHandler<GetLicenseActivationsQuery, IEnumerable<Activation>>
{
    private readonly ILicenseService _licenseService;

    public GetLicenseActivationsQueryHandler(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    public async Task<IEnumerable<Activation>> Handle(GetLicenseActivationsQuery request, CancellationToken cancellationToken)
    {
        return await _licenseService.GetLicenseActivationsAsync(request.UserId, request.LicenseId);
    }
}

public class ValidateLicenseCommandHandler : IRequestHandler<ValidateLicenseCommand, ValidateLicenseResponse>
{
    private readonly ILicenseService _licenseService;

    public ValidateLicenseCommandHandler(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    public async Task<ValidateLicenseResponse> Handle(ValidateLicenseCommand request, CancellationToken cancellationToken)
    {
        return await _licenseService.ValidateLicenseAsync(request.UserId, new ValidateLicenseRequest { LicenseKey = request.LicenseKey, HardwareId = request.HardwareId });
    }
}

public class DeactivateLicenseCommandHandler : IRequestHandler<DeactivateLicenseCommand>
{
    private readonly ILicenseService _licenseService;

    public DeactivateLicenseCommandHandler(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    public async Task Handle(DeactivateLicenseCommand request, CancellationToken cancellationToken)
    {
        await _licenseService.DeactivateLicenseAsync(request.UserId, request.ProductId, new DeactivateLicenseRequest { LicenseKey = request.LicenseKey, HardwareId = request.HardwareId });
    }
}

public class ToggleActivationCommandHandler : IRequestHandler<ToggleActivationCommand, bool>
{
    private readonly ILicenseService _licenseService;

    public ToggleActivationCommandHandler(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    public async Task<bool> Handle(ToggleActivationCommand request, CancellationToken cancellationToken)
    {
        return await _licenseService.ToggleActivationAsync(request.UserId, request.LicenseId, request.HardwareId);
    }
}
