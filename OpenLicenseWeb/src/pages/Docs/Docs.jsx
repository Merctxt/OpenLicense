import useDocs from './useDocs'

export default function Docs() {
  const { scalarUrl, apiUrl } = useDocs()

  return (
    <div>
      <h1 className="h4 mb-3">API Documentation</h1>

      <div className="card mb-3">
        <div className="card-header fw-semibold">Getting Started</div>
        <div className="card-body">
          <p className="mb-3">
            OpenLicense provides a RESTful API for managing software licenses.
            Use the API to validate licenses from your application.
          </p>

          <h5 className="h6">Base URL</h5>
          <div className="font-mono small bg-body-tertiary border rounded p-2 mb-3">{apiUrl}</div>

          <h5 className="h6">Authentication</h5>
          <p>The API supports two authentication methods:</p>

          <div className="bg-body-tertiary border rounded p-3 mb-3">
            <h6 className="mb-1">1. JWT Bearer Token</h6>
            <p className="text-body-secondary small mb-2">Used for dashboard and management operations.</p>
            <pre className="font-mono small bg-dark text-light rounded p-2 mb-0"><code>Authorization: Bearer &lt;your-jwt-token&gt;</code></pre>
          </div>

          <div className="bg-body-tertiary border rounded p-3">
            <h6 className="mb-1">2. API Key</h6>
            <p className="text-body-secondary small mb-2">Used for client-side integration (e.g., license validation). Create keys in your <a href="/account">Account settings</a>.</p>
            <pre className="font-mono small bg-dark text-light rounded p-2 mb-0"><code>X-Api-Key: &lt;your-api-key&gt;</code></pre>
          </div>
        </div>
      </div>

      <div className="card mb-3">
        <div className="card-header fw-semibold">Quick Reference</div>
        <div className="card-body p-0">
          <div className="table-responsive">
            <table className="table table-sm align-middle mb-0">
              <thead>
                <tr>
                  <th>Endpoint</th>
                  <th>Method</th>
                  <th>Auth</th>
                  <th>Description</th>
                </tr>
              </thead>
              <tbody>
                <tr><td>/api/licenses</td><td>GET</td><td>Bearer / ApiKey</td><td>List licenses for a product</td></tr>
                <tr><td>/api/licenses</td><td>POST</td><td>Bearer / ApiKey</td><td>Create a license</td></tr>
                <tr><td>/api/licenses/validate</td><td>POST</td><td>ApiKey</td><td>Validate a license key</td></tr>
                <tr><td>/api/licenses/deactivate</td><td>POST</td><td>Bearer / ApiKey</td><td>Deactivate a license</td></tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-header fw-semibold">How can I get the hardware ID?</div>
        <div className="card-body">
          <p className="mb-2">
            The hardware ID is a unique identifier used to lock a license to a specific client, installation, or device.
            While commonly referred to as a "hardware ID", it does <strong>not</strong> strictly have to be a hardware hash.
            You can use any unique and stable identifier of the client or user environment (e.g., an installation UUID, a MAC address, or a hashed combination of system characteristics).
          </p>

          <div className="alert alert-info py-2 mb-3">
            <strong>Implementation Note:</strong> It is the client application's responsibility to keep the collection logic consistent.
            Ensure that your code generates the exact same identifier across subsequent executions on the same environment to prevent activation verification failures.
          </div>

          <p className="mb-2">If you choose to generate it from hardware properties, here are common methods depending on the platform:</p>

          <ul className="mb-3 ps-3">
            <li><strong>Windows:</strong> Use the <code>wmic</code> command to get the UUID: <code>wmic csproduct get uuid</code>.</li>
            <li><strong>macOS:</strong> Use the <code>ioreg</code> command: <code>ioreg -rd1 -c IOPlatformExpertDevice | grep IOPlatformUUID</code>.</li>
            <li><strong>Linux:</strong> Use the <code>cat /etc/machine-id</code> command or read from <code>/sys/class/dmi/id/product_uuid</code>.</li>
          </ul>

          <p className="mb-2">Example:</p>

          <pre className="font-mono small bg-dark text-light rounded p-3 mb-0" style={{ whiteSpace: 'pre-wrap' }}><code>{`using System;</code></pre>
using System.Management;
using System.Security.Cryptography;
using System.Text;

public static class HardwareInfo
{
    public static string GetHardwareId()
    {
        string processorId = GetWmiProperty("Win32_Processor", "ProcessorId");
        string motherBoard = GetWmiProperty("Win32_BaseBoard", "SerialNumber");
        string diskSerial  = GetWmiProperty("Win32_DiskDrive", "SerialNumber");

        string rawId = $\"{processorId}_{motherBoard}_{diskSerial}\";
        return GenerateHash(rawId);
    }

    private static string GetWmiProperty(string wmiClass, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($\"SELECT {{property}} FROM {{wmiClass}}\");
            foreach (var obj in searcher.Get())
            {
                var value = obj[property]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(value)) return value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($\"[WMI Error] {{wmiClass}}.{{property}}: {{ex.Message}}\");
        }
        return "UNKNOWN";
    }

    private static string GenerateHash(string input)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }
}`}</code></pre>

          <p className="mt-3 mb-0">You can also use third-party libraries in your application to generate a consistent hardware ID across platforms.</p>
        </div>
      </div>

      <div className="card mb-3">
        <div className="card-header fw-semibold">Integration Example</div>
        <div className="card-body">
          <h5 className="h6 mb-2">Validating a License (cURL)</h5>
          <pre className="font-mono small bg-dark text-light rounded p-3 mb-3" style={{ whiteSpace: 'pre-wrap' }}><code>{`curl -X POST ${apiUrl}/api/licenses/validate \\
  -H "X-Api-Key: your-api-key" \\
  -H "Content-Type: application/json" \\
  -d '{"licenseKey": "XXXX-XXXX-XXXX-XXXX", "hardwareId": "unique-device-id"}'`}</code></pre>

          <h5 className="h6 mb-2">Response</h5>
          <pre className="font-mono small bg-dark text-light rounded p-3 mb-0" style={{ whiteSpace: 'pre-wrap' }}><code>{`{
  "isValid": true,
  "message": "License is valid",
  "currentActivations": 1,
  "maxActivations": 3,
  "expiresAt": "2026-12-31T23:59:59Z"
}`}</code></pre>
        </div>
      </div>

      <div className="card">
        <div className="card-header fw-semibold">Full API Reference</div>
        <div className="card-body">
          <p className="mb-2">For the complete interactive API documentation with all endpoints, schemas, and request examples:</p>
          <a href={scalarUrl} target="_blank" rel="noopener noreferrer" className="btn btn-primary">
            Open Scalar API Docs &rarr;
          </a>
        </div>
      </div>
    </div>
  )
}
