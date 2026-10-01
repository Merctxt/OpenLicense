export default function AccountDangerZone({ handleDeleteAccount }) {
  return (
    <div className="border border-danger rounded p-4">
      <h3 className="h5 text-danger">Danger Zone</h3>
      <p className="text-body-secondary small mb-3">Once you delete your account, there is no going back.</p>
      <button className="btn btn-danger" onClick={handleDeleteAccount}>Delete Account</button>
    </div>
  )
}
