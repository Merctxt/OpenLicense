import PasswordValidation from '../../../../shared/components/PasswordValidation/PasswordValidation'

export default function AccountProfileCard({ editing, name, email, password, setName, setEmail, setPassword, pwAllPassed, handleUpdateProfile, user, setEditing }) {
  return (
    <div className="card mb-3">
      <div className="card-header d-flex justify-content-between align-items-center">
        <span className="fw-semibold">Profile</span>
        {!editing && (
          <button className="btn btn-outline-secondary btn-sm" onClick={() => setEditing(true)}>Edit</button>
        )}
      </div>
      <div className="card-body">
        {editing ? (
          <form onSubmit={handleUpdateProfile}>
            <div className="mb-3">
              <label className="form-label">Name</label>
              <input className="form-control" value={name} onChange={(e) => setName(e.target.value)} required />
            </div>
            <div className="mb-3">
              <label className="form-label">Email</label>
              <input className="form-control" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
            </div>
            <div className="mb-3">
              <label className="form-label">New Password <small className="text-body-secondary">(leave blank to keep current)</small></label>
              <input className="form-control" type="password" value={password} onChange={(e) => setPassword(e.target.value)} minLength={8} />
              <PasswordValidation password={password} showLabel={false} />
            </div>
            <div className="d-flex gap-2">
              <button type="submit" className="btn btn-primary" disabled={!pwAllPassed}>Save</button>
              <button type="button" className="btn btn-secondary" onClick={() => { setEditing(false); setName(user.name); setEmail(user.email); setPassword('') }}>Cancel</button>
            </div>
          </form>
        ) : (
          <div>
            <div className="row mb-2">
              <div className="col-sm-3 fw-semibold text-body-secondary small">Name</div>
              <div className="col-sm-9">{user.name}</div>
            </div>
            <div className="row mb-2">
              <div className="col-sm-3 fw-semibold text-body-secondary small">Email</div>
              <div className="col-sm-9">{user.email}</div>
            </div>
            <div className="row">
              <div className="col-sm-3 fw-semibold text-body-secondary small">Member since</div>
              <div className="col-sm-9">{new Date(user.createdAt).toLocaleDateString()}</div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
