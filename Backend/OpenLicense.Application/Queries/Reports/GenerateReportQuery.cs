using MediatR;

namespace OpenLicense.Application.Queries.Reports;

public record GenerateReportQuery(Guid UserId, int ExpiringDaysWarning) : IRequest<byte[]>;
