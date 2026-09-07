export default function AppBackground({ children }) {
  return (
    <div style={{ minHeight: '100vh' }}>
      <div
        className="position-fixed top-0 start-0 w-100 h-100"
        style={{
          backgroundImage: 'url(/dashboard.svg)',
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          opacity: 0.15,
          zIndex: -1,
        }}
      ></div>
      {children}
    </div>
  )
}
