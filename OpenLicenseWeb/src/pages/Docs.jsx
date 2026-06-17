import './Docs.css'

export default function Docs() {
  const scalarUrl = import.meta.env.VITE_SCALAR_URL || '/scalar/v1'
  const apiUrl = import.meta.env.VITE_API_URL || ''

  return (
    <div className="docs-page">
      <h1>API Documentation</h1>

      <div className="panel">
        <div className="panel-heading">Getting Started</div>
        <div className="panel-body">
          <p className="docs-intro">
            OpenLicense provides a RESTful API for managing software licenses. 
            Use the API to validate licenses from your application.
          </p>

          <h4>Base URL</h4>
          <div className="code-box" style={{ display: 'block', marginTop: 4, marginBottom: 16 }}>{apiUrl}</div>

          <h4>Authentication</h4>
          <p>The API supports two authentication methods:</p>

          <div className="auth-method">
            <h4>1. JWT Bearer Token</h4>
            <p>Used for dashboard and management operations.</p>
            <div className="code-block">Authorization: Bearer &lt;your-jwt-token&gt;</div>
          </div>

          <div className="auth-method">
            <h4>2. API Key</h4>
            <p>Used for client-side integration (e.g., license validation). Create keys in your <a href="/account">Account settings</a>.</p>
            <div className="code-block">X-Api-Key: &lt;your-api-key&gt;</div>
          </div>
        </div>
      </div>

      <div className="panel">
        <div className="panel-heading">Quick Reference</div>
        <div className="panel-body">
          <table className="table">
            <thead>
              <tr>
                <th>Endpoint</th>
                <th>Method</th>
                <th>Auth</th>
                <th>Description</th>
              </tr>
            </thead>
            <tbody>
              <tr><td>/api/products/all</td><td>GET</td><td>Bearer / ApiKey</td><td>List all products</td></tr>
              <tr><td>/api/products/create</td><td>POST</td><td>Bearer / ApiKey</td><td>Create a product</td></tr>
              <tr><td>/api/licenses</td><td>GET</td><td>Bearer / ApiKey</td><td>List licenses for a product</td></tr>
              <tr><td>/api/licenses</td><td>POST</td><td>Bearer / ApiKey</td><td>Create a license</td></tr>
              <tr><td>/api/licenses/validate</td><td>POST</td><td>ApiKey</td><td>Validate a license key</td></tr>
              <tr><td>/api/licenses/deactivate</td><td>POST</td><td>Bearer / ApiKey</td><td>Deactivate a license</td></tr>
            </tbody>
          </table>
        </div>
      </div>

      <div className="panel">
        <div className="panel-heading">How i can get the hardware ID?</div>
        <div className="panel-body">
          <p className="panel-text">The hardware ID is a unique identifier for the device where the software is installed. It can be generated using various methods depending on the platform:</p>

          <ul className="panel-text list">
            <li><strong>Windows:</strong> Use the <code>wmic</code> command to get the UUID: <code>wmic csproduct get uuid</code>.</li>
            <li><strong>macOS:</strong> Use the <code>ioreg</code> command: <code>ioreg -rd1 -c IOPlatformExpertDevice | grep IOPlatformUUID</code>.</li>
            <li><strong>Linux:</strong> Use the <code>cat /etc/machine-id</code> command or read from <code>/sys/class/dmi/id/product_uuid</code>.</li>
          </ul>

          <p className="panel-text">Example:</p>

          <div className="code-block">{`using System;
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

        string rawId = $"{processorId}_{motherBoard}_{diskSerial}";
        return GenerateHash(rawId);
    }

    private static string GetWmiProperty(string wmiClass, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}");
            foreach (var obj in searcher.Get())
            {
                var value = obj[property]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(value)) return value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WMI Error] {wmiClass}.{property}: {ex.Message}");
        }
        return "UNKNOWN";
    }

    private static string GenerateHash(string input)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower(); // Retorna o hash em string hexadecimal
    }
}


`}</div>

          <p className="panel-text">You can also use third-party libraries in your application to generate a consistent hardware ID across platforms.</p>
        </div>
      </div>

      <div className="panel">
        <div className="panel-heading">Integration Example</div>
        <div className="panel-body">
          <h4>Validating a License (cURL)</h4>
          <div className="code-block">{`curl -X POST ${apiUrl}/api/licenses/validate \\
  -H "X-Api-Key: your-api-key" \\
  -H "Content-Type: application/json" \\
  -d '{"licenseKey": "XXXX-XXXX-XXXX-XXXX", "hardwareId": "unique-device-id"}'`}</div>

          <h4 style={{ marginTop: 16 }}>Response</h4>
          <div className="code-block">{`{
  "isValid": true,
  "message": "License is valid",
  "currentActivations": 1,
  "maxActivations": 3,
  "expiresAt": "2026-12-31T23:59:59Z"
}`}</div>
        </div>
      </div>

      <div className="panel">
        <div className="panel-heading">Full API Reference</div>
        <div className="panel-body">
          <p>For the complete interactive API documentation with all endpoints, schemas, and request examples:</p>
          <a href={scalarUrl} target="_blank" rel="noopener noreferrer" className="btn btn-primary" style={{ marginTop: 8, display: 'inline-block' }}>
            Open Scalar API Docs &rarr;
          </a>
        </div>
      </div>
    </div>
  )
}
