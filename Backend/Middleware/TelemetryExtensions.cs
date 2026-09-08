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
            {
                Console.WriteLine("[OTEL] Disabled - OTEL_EXPORTER_OTLP_ENDPOINT is not set");
                return builder;
            }

            var headerDictionary = ParseHeaders(rawHeaders);

            Console.WriteLine($"[OTEL] Initialized:");
            Console.WriteLine($"  Endpoint: {endpoint}");
            Console.WriteLine($"  Headers: {headerDictionary?.Count ?? 0} header(s)");
            if (headerDictionary != null)
            {
                foreach (var h in headerDictionary)
                {
                    var masked = h.Value.Length > 10 ? h.Value[..7] + "..." : h.Value;
                    Console.WriteLine($"    {h.Key} = {masked}");
                }
            }

            var resource = ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: "1.0.0");

            var traceEndpoint = NormalizeSignalEndpoint(endpoint, "v1/traces");
            var metricsEndpoint = NormalizeSignalEndpoint(endpoint, "v1/metrics");
            var logsEndpoint = NormalizeSignalEndpoint(endpoint, "v1/logs");

            Action<OtlpExporterOptions> configureTraceExporter = o =>
            {
                o.Endpoint = traceEndpoint;
                o.Protocol = OtlpExportProtocol.HttpProtobuf;
                if (headerDictionary != null)
                {
                    o.Headers = string.Join(";", headerDictionary.Select(h => $"{h.Key}={h.Value}"));
                }
            };

            Action<OtlpExporterOptions> configureMetricsExporter = o =>
            {
                o.Endpoint = metricsEndpoint;
                o.Protocol = OtlpExportProtocol.HttpProtobuf;
                if (headerDictionary != null)
                {
                    o.Headers = string.Join(";", headerDictionary.Select(h => $"{h.Key}={h.Value}"));
                }
            };

            // ── Traces ──────────────────────────────────────────────────
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(configureTraceExporter)
                )

            // ── Metrics ─────────────────────────────────────────────────
                .WithMetrics(metrics => metrics
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(configureMetricsExporter)
                );

            // ── Logs ────────────────────────────────────────────────────
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.SetResourceBuilder(resource);
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.AddOtlpExporter(o =>
                {
                    o.Endpoint = logsEndpoint;
                    o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    if (headerDictionary != null)
                    {
                        o.Headers = string.Join(";", headerDictionary.Select(h => $"{h.Key}={h.Value}"));
                    }
                });
            });

            return builder;
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
