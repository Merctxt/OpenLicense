export default function AuthBackground({ children }) {
  return (
    <div
      className="d-flex align-items-center justify-content-center min-vh-100"
      style={{
        backgroundImage: 'url(/background.svg)',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        backgroundRepeat: 'no-repeat',
      }}
    >
      <div className="position-absolute top-0 start-0 w-100 h-100 bg-dark" style={{ opacity: 0.3, zIndex: 0 }}></div>
      {children}
    </div>
  )
}
