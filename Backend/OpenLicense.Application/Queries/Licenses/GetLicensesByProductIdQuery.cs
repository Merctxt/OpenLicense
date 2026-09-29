using MediatR;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Queries.Licenses;

public record GetLicensesByProductIdQuery(Guid UserId, Guid ProductId) : IRequest<IEnumerable<License>>;
public record GetLicenseActivationsQuery(Guid UserId, Guid LicenseId) : IRequest<IEnumerable<Activation>>;
