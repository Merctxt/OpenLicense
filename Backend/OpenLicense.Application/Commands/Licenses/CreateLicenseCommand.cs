using MediatR;
using OpenLicense.Application.DTOs;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Commands.Licenses;

public record CreateLicenseCommand(Guid UserId, Guid ProductId, string Name, DateTime? ExpiresAt, int MaxActivations) : IRequest<License>;
public record UpdateLicenseCommand(Guid UserId, Guid LicenseId, string? Name, DateTime? ExpiresAt, int? MaxActivations, bool? Status) : IRequest<License>;
public record DeleteLicenseCommand(Guid UserId, Guid LicenseId) : IRequest;
public record ValidateLicenseCommand(Guid UserId, string LicenseKey, string HardwareId) : IRequest<ValidateLicenseResponse>;
public record DeactivateLicenseCommand(Guid UserId, Guid? ProductId, string LicenseKey, string HardwareId) : IRequest;
public record ToggleActivationCommand(Guid UserId, Guid LicenseId, string HardwareId) : IRequest<bool>;
