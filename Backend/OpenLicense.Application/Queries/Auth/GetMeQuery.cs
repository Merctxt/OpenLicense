using MediatR;
using OpenLicense.Application.DTOs;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Queries.Auth;

public record GetMeQuery(Guid UserId) : IRequest<User>;
