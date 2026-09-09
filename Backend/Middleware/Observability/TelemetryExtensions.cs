using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenLicenseApi.Middleware.Observability
{
    public static class TelemetryExtensions
    {
        public static IHostApplicationBuilder AddOpenTelemetry(
            this IHostApplicationBuilder builder,
            string serviceName = "OpenLicenseApi")
        {
            var endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                Console.WriteLine("[OTEL] Disabled - OTEL_EXPORTER_OTLP_ENDPOINT is not set");
                return builder;
            }

            var headers = ParseHeaders(builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"]);

            var resource = ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: "1.0.0");

            var traceEndpoint = NormalizeSignalEndpoint(endpoint, "v1/traces");
            var metricsEndpoint = NormalizeSignalEndpoint(endpoint, "v1/metrics");
            var logsEndpoint = NormalizeSignalEndpoint(endpoint, "v1/logs");

            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options => ConfigureOtlpOptions(options, traceEndpoint, headers))
                )
                .WithMetrics(metrics => metrics
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(options => ConfigureOtlpOptions(options, metricsEndpoint, headers))
                );

            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.SetResourceBuilder(resource);
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.AddOtlpExporter(options => ConfigureOtlpOptions(options, logsEndpoint, headers));
            });

            return builder;
        }

        private static void ConfigureOtlpOptions(OtlpExporterOptions options, Uri endpoint, Dictionary<string, string>? headers)
        {
            options.Endpoint = endpoint;
            options.Protocol = OtlpExportProtocol.HttpProtobuf;

            if (headers is not null && headers.Count > 0)
            {
                options.Headers = string.Join(";", headers.Select(h => $"{h.Key}={h.Value}"));
            }
        }

        private static Uri NormalizeSignalEndpoint(string endpoint, string signalPath)
        {
            var normalizedEndpoint = endpoint.Trim();
            if (string.IsNullOrWhiteSpace(normalizedEndpoint))
                throw new InvalidOperationException("OTEL_EXPORTER_OTLP_ENDPOINT cannot be empty.");

            var trimmedBase = normalizedEndpoint.TrimEnd('/');
            var normalizedSignal = signalPath.Trim('/');

            if (trimmedBase.EndsWith($"/{normalizedSignal}", StringComparison.OrdinalIgnoreCase))
                return new Uri(trimmedBase, UriKind.Absolute);

            return new Uri($"{trimmedBase}/{normalizedSignal}", UriKind.Absolute);
        }

        private static Dictionary<string, string>? ParseHeaders(string? rawHeaders)
        {
            if (string.IsNullOrWhiteSpace(rawHeaders))
                return null;

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var parts = rawHeaders.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                var separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex <= 0)
                    continue;

                var key = trimmed[..separatorIndex].Trim();
                var value = trimmed[(separatorIndex + 1)..].Trim();

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    result[key] = value;
                }
            }

            return result;
        }
    }
}
