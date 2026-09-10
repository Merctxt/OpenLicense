export default function Background({ variant = 'app', children }) {
  const isAuth = variant === 'auth'

  return (
    <div className={`${isAuth ? 'd-flex align-items-center justify-content-center min-vh-100' : ''}`}>
      <div
        className="position-fixed top-0 start-0 w-100 h-100"
        style={{
          backgroundImage: `url(/${isAuth ? 'background' : 'dashboard'}.svg)`,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          opacity: isAuth ? 0.7 : 0.15,
          zIndex: -1,
        }}
      ></div>
      {children}
    </div>
  )
}
