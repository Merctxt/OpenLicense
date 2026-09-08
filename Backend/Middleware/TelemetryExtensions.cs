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
            var headers  = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"];

            if (string.IsNullOrWhiteSpace(endpoint))
                return builder;

            var resource = ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: "1.0.0");

            // ── Traces ──────────────────────────────────────────────────
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        // Ignora health check para não poluir os traces
                        o.Filter = ctx =>
                            !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri($"{endpoint.TrimEnd('/')}/v1/traces");
                        o.Protocol  = OtlpExportProtocol.HttpProtobuf;
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
                        o.Endpoint = new Uri($"{endpoint.TrimEnd('/')}/v1/metrics");
                        o.Protocol  = OtlpExportProtocol.HttpProtobuf;
                        if (!string.IsNullOrWhiteSpace(headers))
                            o.Headers = headers;
                    })
                );

            // ── Logs ────────────────────────────────────────────────────
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.SetResourceBuilder(resource);
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes           = true;
                logging.AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri($"{endpoint.TrimEnd('/')}/v1/logs");
                    o.Protocol  = OtlpExportProtocol.HttpProtobuf;
                    if (!string.IsNullOrWhiteSpace(headers))
                        o.Headers = headers;
                });
            });

            return builder;
        }
    }
}

