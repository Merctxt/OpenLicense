export default function Terms() {
  return (
    <div style={{ maxWidth: '800px', margin: '0 auto' }}>
      <h1 className="h4 mb-1">Terms of Use</h1>
      <p className="text-body-secondary small mb-3">Last updated: June 18, 2026</p>

      <div className="card">
        <div className="card-body">
          <section className="mb-4">
            <h3 className="h5 border-bottom pb-1 mb-2">1. Acceptance of Terms</h3>
            <p className="mb-0">
              By accessing or using the OpenLicense platform (including the Developer Dashboard, Licensing API, and SDKs), 
              you agree to be bound by these Terms of Use and all applicable laws and regulations. If you do not agree, 
              you are prohibited from using the platform.
            </p>
          </section>

          <section className="mb-4">
            <h3 className="h5 border-bottom pb-1 mb-2">2. Developer Account and API Keys</h3>
            <p className="mb-0">
              You must register for an account to generate API keys, manage products, and issue licenses. 
              You are entirely responsible for maintaining the confidentiality of your credentials and API keys. 
              Any request authenticated via your API keys will be deemed as initiated by you.
            </p>
          </section>

          <section className="mb-4">
            <h3 className="h5 border-bottom pb-1 mb-2">3. Usage Limits and Licensing Rules</h3>
            <p className="mb-0">
              OpenLicense provides tiered resource quotas (e.g., maximum products, maximum licenses, and maximum API keys). 
              You agree not to bypass, attempt to bypass, or exploit any quotas or limitations. 
              The platform may not be used to distribute, authorize, or validate illegal software licenses.
            </p>
          </section>

          <section className="mb-4">
            <h3 className="h5 border-bottom pb-1 mb-2">4. Termination and Restrictions</h3>
            <p className="mb-0">
              We reserve the right to suspend or terminate your account and invalidate your active API keys or licenses 
              at our sole discretion, without notice, if we find you in violation of these terms or engaging in activities 
              detrimental to the stability and integrity of the service.
            </p>
          </section>

          <section>
            <h3 className="h5 border-bottom pb-1 mb-2">5. Limitation of Liability</h3>
            <p className="mb-0">
              THE PLATFORM IS PROVIDED "AS IS" WITHOUT WARRANTIES OF ANY KIND. IN NO EVENT SHALL OPENLICENSE BE LIABLE 
              FOR ANY DAMAGES, LOSS OF DATA, OR BUSINESS INTERRUPTION ARISING OUT OF THE USE OR INABILITY TO USE THE SERVICE, 
              EVEN IF ADVIsED OF THE POSSIBILITY OF SUCH DAMAGES.
            </p>
          </section>
        </div>
      </div>
    </div>
  )
}
