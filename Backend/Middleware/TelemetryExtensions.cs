using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;

namespace OpenLicenseApi.Middleware
{
    public static class TelemetryExtensions
    {
        public static IHostApplicationBuilder AddOpenTelemetry(
            this IHostApplicationBuilder builder,
            string serviceName = "OpenLicenseApi")
        {
            var endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            var rawHeaders = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"];

            if (string.IsNullOrWhiteSpace(endpoint))
                return builder;

            var headers = rawHeaders?.Replace("Authorization:", "Authorization=");

            var resource = ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: "1.0.0");

            var baseUri = new Uri(endpoint);

            // ── Traces ──────────────────────────────────────────────────
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = baseUri;
                        o.Protocol = OtlpExportProtocol.HttpProtobuf;
                        if (!string.IsNullOrWhiteSpace(headers))
                            o.Headers = headers;
                    })
                )

            // ── Metrics ─────────────────────────────────────────────────
                .WithMetrics(metrics => metrics
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = baseUri;
                        o.Protocol = OtlpExportProtocol.HttpProtobuf;
                        if (!string.IsNullOrWhiteSpace(headers))
                            o.Headers = headers;
                    })
                );

            // ── Logs ────────────────────────────────────────────────────
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.SetResourceBuilder(resource);
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.AddOtlpExporter(o =>
                {
                    o.Endpoint = baseUri;
                    o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    if (!string.IsNullOrWhiteSpace(headers))
                        o.Headers = headers;
                });
            });

            return builder;
        }
    }
}