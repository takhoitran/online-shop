import { useState } from 'react';
import { generateTelegramLinkCode } from '../api/auth';
import { extractErrorMessage } from '../api/client';
import { formatDateTime } from '../utils/format';

export function TelegramLinkPage() {
  const [code, setCode] = useState<string | null>(null);
  const [expiresAtUtc, setExpiresAtUtc] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleGenerate() {
    setLoading(true);
    setError(null);
    try {
      const result = await generateTelegramLinkCode();
      setCode(result.code);
      setExpiresAtUtc(result.expiresAtUtc);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="form-card">
      <h1>Link your Telegram account</h1>
      <p>
        Link your account to shop and get order notifications right on Telegram. Get a code below, then open the
        store's bot on Telegram and send it the code.
      </p>

      {error && <div className="alert error">{error}</div>}

      <button className="btn" onClick={handleGenerate} disabled={loading} style={{ width: '100%' }}>
        {loading ? 'Generating code…' : 'Get link code'}
      </button>

      {code && (
        <div className="empty-state" style={{ marginTop: '1.5rem' }}>
          <p>Your link code:</p>
          <p style={{ fontSize: '2rem', fontWeight: 700, letterSpacing: '0.2em' }}>{code}</p>
          {expiresAtUtc && <p className="product-stock">Expires at: {formatDateTime(expiresAtUtc)}</p>}
          <p>Send this code to the store's Telegram bot to finish linking.</p>
        </div>
      )}
    </div>
  );
}
