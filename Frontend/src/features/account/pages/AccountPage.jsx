import { useCallback } from 'react'
import { AccountProfileCard } from '../components/AccountProfileCard'
import { AccountApiKeysCard } from '../components/AccountApiKeysCard'
import { AccountApiKeyModal } from '../components/AccountApiKeyModal'
import { AccountReportsCard } from '../components/AccountReportsCard'
import { AccountLimitsCard } from '../components/AccountLimitsCard'
import { AccountDangerZone } from '../components/AccountDangerZone'
import useAccount from './useAccount'

export default function AccountPage() {
  const {
    user, apiKeyModal, setApiKeyModal,
    newKeyName, setNewKeyName, createdKey, setCreatedKey,
    editing, setEditing, submitting,
    name, setName, email, setEmail, password, setPassword, pwAllPassed,
    handleUpdateProfile, handleDeleteAccount,
    handleCreateApiKey, handleDeleteApiKey, handleToggleApiKey, handleToggleReports,
  } = useAccount()

  const handleModalClose = useCallback(() => setApiKeyModal(false), [setApiKeyModal])

  if (!user) return null

  return (
    <div>
      <h1 className="h4 mb-3">Account</h1>

      <AccountProfileCard
        editing={editing} setEditing={setEditing}
        name={name} setName={setName}
        email={email} setEmail={setEmail}
        password={password} setPassword={setPassword}
        pwAllPassed={pwAllPassed}
        handleUpdateProfile={handleUpdateProfile}
        user={user}
      />

      <AccountApiKeysCard
        user={user}
        submitting={submitting}
        setShowApiKeyModal={() => { setApiKeyModal(true); setNewKeyName(''); setCreatedKey('') }}
        handleDeleteApiKey={handleDeleteApiKey}
        handleToggleApiKey={handleToggleApiKey}
      />

      <AccountApiKeyModal
        isOpen={apiKeyModal}
        createdKey={createdKey} setCreatedKey={setCreatedKey}
        newKeyName={newKeyName} setNewKeyName={setNewKeyName}
        submitting={submitting}
        handleCreateApiKey={handleCreateApiKey}
        handleModalClose={handleModalClose}
      />

      <AccountReportsCard
        user={user}
        submitting={submitting}
        handleToggleReports={handleToggleReports}
      />

      <AccountLimitsCard user={user} />

      <AccountDangerZone handleDeleteAccount={handleDeleteAccount} />
    </div>
  )
}
