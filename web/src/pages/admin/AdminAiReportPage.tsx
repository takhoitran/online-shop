import { useState } from 'react';
import { getRestockReport } from '../../api/ai';
import { extractErrorMessage } from '../../api/client';
import type { RestockReport } from '../../types';

export function AdminAiReportPage() {
  const [report, setReport] = useState<RestockReport | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleGenerate() {
    setLoading(true);
    setError(null);
    try {
      setReport(await getRestockReport());
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div>
      <h1>AI Report — Restock suggestions</h1>
      <p className="product-stock">
        AI analyzes current stock and sales from confirmed orders to suggest which products to restock or clear out.
      </p>

      <button className="btn" onClick={handleGenerate} disabled={loading}>
        {loading ? 'Analyzing…' : 'Generate new report'}
      </button>

      {error && <div className="alert error">{error}</div>}

      {report && (
        <div style={{ marginTop: 20 }}>
          <div className="summary-card">
            <p>{report.summary}</p>
          </div>

          {report.suggestions.length > 0 && (
            <table className="order-items" style={{ marginTop: 16 }}>
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Suggestion</th>
                  <th>Reason</th>
                </tr>
              </thead>
              <tbody>
                {report.suggestions.map((s, i) => (
                  <tr key={i}>
                    <td>{s.productName}</td>
                    <td>{s.action}</td>
                    <td className="product-stock">{s.reason}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </div>
  );
}
