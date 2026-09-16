import { useEffect, useState, type FormEvent } from 'react';
import { createCategory, deleteCategory, getCategories, updateCategory } from '../../api/catalog';
import { extractErrorMessage } from '../../api/client';
import type { Category } from '../../types';

export function AdminCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [nameEn, setNameEn] = useState('');
  const [nameVi, setNameVi] = useState('');
  const [descriptionEn, setDescriptionEn] = useState('');
  const [descriptionVi, setDescriptionVi] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editName, setEditName] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [editNameEn, setEditNameEn] = useState('');
  const [editNameVi, setEditNameVi] = useState('');
  const [editDescriptionEn, setEditDescriptionEn] = useState('');
  const [editDescriptionVi, setEditDescriptionVi] = useState('');
  const [rowBusyId, setRowBusyId] = useState<string | null>(null);

  function load() {
    getCategories()
      .then(setCategories)
      .finally(() => setLoading(false));
  }

  useEffect(load, []);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await createCategory({
        name,
        description: description || null,
        nameEn: nameEn || null,
        nameVi: nameVi || null,
        descriptionEn: descriptionEn || null,
        descriptionVi: descriptionVi || null,
      });
      setName('');
      setDescription('');
      setNameEn('');
      setNameVi('');
      setDescriptionEn('');
      setDescriptionVi('');
      load();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  function startEdit(c: Category) {
    setError(null);
    setEditingId(c.id);
    setEditName(c.name);
    setEditDescription(c.description || '');
    setEditNameEn(c.nameEn || '');
    setEditNameVi(c.nameVi || '');
    setEditDescriptionEn(c.descriptionEn || '');
    setEditDescriptionVi(c.descriptionVi || '');
  }

  function cancelEdit() {
    setEditingId(null);
  }

  async function saveEdit(id: string) {
    setError(null);
    setRowBusyId(id);
    try {
      await updateCategory(id, {
        name: editName,
        description: editDescription || null,
        nameEn: editNameEn || null,
        nameVi: editNameVi || null,
        descriptionEn: editDescriptionEn || null,
        descriptionVi: editDescriptionVi || null,
      });
      setEditingId(null);
      load();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setRowBusyId(null);
    }
  }

  async function handleDelete(c: Category) {
    if (!window.confirm(`Delete category "${c.name}"?`)) return;
    setError(null);
    setRowBusyId(c.id);
    try {
      await deleteCategory(c.id);
      load();
    } catch (err) {
      setError(extractErrorMessage(err, 'Could not delete this category.'));
    } finally {
      setRowBusyId(null);
    }
  }

  return (
    <div>
      <h1>Categories</h1>

      <form onSubmit={handleSubmit} style={{ display: 'flex', gap: 10, alignItems: 'flex-start', marginBottom: 24, flexWrap: 'wrap' }}>
        <input placeholder="New category name" value={name} onChange={(e) => setName(e.target.value)} required style={{ padding: '9px 12px', border: '1px solid var(--color-border)', borderRadius: 8, minWidth: 200 }} />
        <input placeholder="Description (optional)" value={description} onChange={(e) => setDescription(e.target.value)} style={{ padding: '9px 12px', border: '1px solid var(--color-border)', borderRadius: 8, minWidth: 240 }} />
        <input placeholder="English name" value={nameEn} onChange={(e) => setNameEn(e.target.value)} style={{ padding: '9px 12px', border: '1px solid var(--color-border)', borderRadius: 8, minWidth: 200 }} />
        <input placeholder="Vietnamese name" value={nameVi} onChange={(e) => setNameVi(e.target.value)} style={{ padding: '9px 12px', border: '1px solid var(--color-border)', borderRadius: 8, minWidth: 200 }} />
        <input placeholder="English description" value={descriptionEn} onChange={(e) => setDescriptionEn(e.target.value)} style={{ padding: '9px 12px', border: '1px solid var(--color-border)', borderRadius: 8, minWidth: 240 }} />
        <input placeholder="Vietnamese description" value={descriptionVi} onChange={(e) => setDescriptionVi(e.target.value)} style={{ padding: '9px 12px', border: '1px solid var(--color-border)', borderRadius: 8, minWidth: 240 }} />
        <button className="btn" type="submit" disabled={submitting}>
          {submitting ? 'Adding…' : '+ Add category'}
        </button>
      </form>

      {error && <div className="alert error">{error}</div>}

      {loading ? (
        <div className="loading-state">Loading…</div>
      ) : (
        <table className="admin-table">
          <thead>
            <tr>
              <th>Category name</th>
              <th>Description</th>
              <th>English</th>
              <th>Vietnamese</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {categories.map((c) => (
              <tr key={c.id}>
                {editingId === c.id ? (
                  <>
                    <td>
                      <input value={editName} onChange={(e) => setEditName(e.target.value)} style={{ padding: '6px 8px', border: '1px solid var(--color-border)', borderRadius: 6, width: '100%' }} />
                    </td>
                    <td>
                      <input value={editDescription} onChange={(e) => setEditDescription(e.target.value)} style={{ padding: '6px 8px', border: '1px solid var(--color-border)', borderRadius: 6, width: '100%' }} />
                    </td>
                    <td>
                      <input value={editNameEn} onChange={(e) => setEditNameEn(e.target.value)} placeholder="Name" style={{ padding: '6px 8px', border: '1px solid var(--color-border)', borderRadius: 6, width: '100%', marginBottom: 6 }} />
                      <input value={editDescriptionEn} onChange={(e) => setEditDescriptionEn(e.target.value)} placeholder="Description" style={{ padding: '6px 8px', border: '1px solid var(--color-border)', borderRadius: 6, width: '100%' }} />
                    </td>
                    <td>
                      <input value={editNameVi} onChange={(e) => setEditNameVi(e.target.value)} placeholder="Name" style={{ padding: '6px 8px', border: '1px solid var(--color-border)', borderRadius: 6, width: '100%', marginBottom: 6 }} />
                      <input value={editDescriptionVi} onChange={(e) => setEditDescriptionVi(e.target.value)} placeholder="Description" style={{ padding: '6px 8px', border: '1px solid var(--color-border)', borderRadius: 6, width: '100%' }} />
                    </td>
                    <td style={{ whiteSpace: 'nowrap' }}>
                      <button className="btn small" disabled={rowBusyId === c.id} onClick={() => saveEdit(c.id)} style={{ marginRight: 6 }}>
                        Save
                      </button>
                      <button className="btn secondary small" onClick={cancelEdit}>
                        Cancel
                      </button>
                    </td>
                  </>
                ) : (
                  <>
                    <td>{c.name}</td>
                    <td className="product-stock">{c.description || '—'}</td>
                    <td className="product-stock">
                      <strong>{c.nameEn || '—'}</strong>
                      {c.descriptionEn && <div>{c.descriptionEn}</div>}
                    </td>
                    <td className="product-stock">
                      <strong>{c.nameVi || '—'}</strong>
                      {c.descriptionVi && <div>{c.descriptionVi}</div>}
                    </td>
                    <td style={{ whiteSpace: 'nowrap' }}>
                      <button className="btn secondary small" onClick={() => startEdit(c)} style={{ marginRight: 6 }}>
                        Edit
                      </button>
                      <button className="btn danger small" disabled={rowBusyId === c.id} onClick={() => handleDelete(c)}>
                        Delete
                      </button>
                    </td>
                  </>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
