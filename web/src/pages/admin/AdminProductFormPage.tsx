import { useEffect, useRef, useState, type FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  addProductGalleryImage,
  createProduct,
  getCategories,
  getProductById,
  removeProductGalleryImage,
  updateProduct,
  uploadProductImage,
} from '../../api/catalog';
import { extractErrorMessage } from '../../api/client';
import type { Category, ProductImage } from '../../types';

export function AdminProductFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEdit = Boolean(id);
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [categories, setCategories] = useState<Category[]>([]);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [nameEn, setNameEn] = useState('');
  const [nameVi, setNameVi] = useState('');
  const [descriptionEn, setDescriptionEn] = useState('');
  const [descriptionVi, setDescriptionVi] = useState('');
  const [price, setPrice] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [imageUrl, setImageUrl] = useState<string | null>(null);
  const [images, setImages] = useState<ProductImage[]>([]);
  const [loading, setLoading] = useState(isEdit);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [uploadingGalleryImage, setUploadingGalleryImage] = useState(false);
  const [removingImageId, setRemovingImageId] = useState<string | null>(null);
  const galleryInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    getCategories().then(setCategories);
  }, []);

  useEffect(() => {
    if (!id) return;
    getProductById(id)
      .then((p) => {
        setName(p.name);
        setDescription(p.description ?? '');
        setNameEn(p.nameEn ?? '');
        setNameVi(p.nameVi ?? '');
        setDescriptionEn(p.descriptionEn ?? '');
        setDescriptionVi(p.descriptionVi ?? '');
        setPrice(String(p.price));
        setCategoryId(p.categoryId);
        setImageUrl(p.imageUrl);
        setImages(p.images);
      })
      .finally(() => setLoading(false));
  }, [id]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      const input = {
        name,
        description: description || null,
        price: Number(price),
        categoryId,
        nameEn: nameEn || null,
        nameVi: nameVi || null,
        descriptionEn: descriptionEn || null,
        descriptionVi: descriptionVi || null,
      };
      if (isEdit && id) {
        await updateProduct(id, input);
        navigate('/admin/products');
      } else {
        const newId = await createProduct(input);
        navigate(`/admin/products/${newId}`, { replace: true });
      }
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  async function handleImageChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file || !id) return;
    setUploadingImage(true);
    setError(null);
    try {
      const response = await uploadProductImage(id, file);
      setImageUrl(response.data);
    } catch (err) {
      setError(extractErrorMessage(err, 'Image upload failed.'));
    } finally {
      setUploadingImage(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  }

  async function handleGalleryImageChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file || !id) return;
    setUploadingGalleryImage(true);
    setError(null);
    try {
      await addProductGalleryImage(id, file);
      const refreshed = await getProductById(id);
      setImages(refreshed.images);
    } catch (err) {
      setError(extractErrorMessage(err, 'Gallery image upload failed.'));
    } finally {
      setUploadingGalleryImage(false);
      if (galleryInputRef.current) galleryInputRef.current.value = '';
    }
  }

  async function handleRemoveGalleryImage(imageId: string) {
    if (!id) return;
    setRemovingImageId(imageId);
    setError(null);
    try {
      await removeProductGalleryImage(id, imageId);
      setImages((prev) => prev.filter((img) => img.id !== imageId));
    } catch (err) {
      setError(extractErrorMessage(err, 'Could not remove this image.'));
    } finally {
      setRemovingImageId(null);
    }
  }

  if (loading) return <div className="loading-state">Loading…</div>;

  return (
    <div style={{ maxWidth: 560 }}>
      <h1>{isEdit ? 'Edit product' : 'Add new product'}</h1>
      {error && <div className="alert error">{error}</div>}

      {isEdit && (
        <div className="field">
          <label>Product image</label>
          <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
            <div className="admin-table-thumb" style={{ width: 72, height: 72 }}>
              {imageUrl ? <img src={imageUrl} alt="" /> : 'No image'}
            </div>
            <div>
              <input ref={fileInputRef} type="file" accept="image/png,image/jpeg,image/webp" onChange={handleImageChange} disabled={uploadingImage} />
              {uploadingImage && <div className="product-stock">Uploading…</div>}
            </div>
          </div>
        </div>
      )}

      {isEdit && (
        <div className="field">
          <label>Gallery images (extra photos shown on the product page)</label>
          <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap', marginBottom: 10 }}>
            {images.map((img) => (
              <div key={img.id} style={{ position: 'relative' }}>
                <div className="admin-table-thumb" style={{ width: 72, height: 72 }}>
                  <img src={img.url} alt="" />
                </div>
                <button
                  type="button"
                  className="btn danger small"
                  disabled={removingImageId === img.id}
                  onClick={() => handleRemoveGalleryImage(img.id)}
                  style={{ position: 'absolute', top: -8, right: -8, padding: '2px 6px', lineHeight: 1 }}
                  title="Remove image"
                >
                  ×
                </button>
              </div>
            ))}
            {images.length === 0 && <span className="product-stock">No gallery images yet.</span>}
          </div>
          <input ref={galleryInputRef} type="file" accept="image/png,image/jpeg,image/webp" onChange={handleGalleryImageChange} disabled={uploadingGalleryImage} />
          {uploadingGalleryImage && <div className="product-stock">Uploading…</div>}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="field">
          <label htmlFor="name">Product name</label>
          <input id="name" value={name} onChange={(e) => setName(e.target.value)} required maxLength={200} />
        </div>
        <div className="field">
          <label htmlFor="description">Description</label>
          <textarea id="description" value={description} onChange={(e) => setDescription(e.target.value)} rows={4} maxLength={2000} />
        </div>
        <div className="field">
          <label htmlFor="nameEn">English name</label>
          <input id="nameEn" value={nameEn} onChange={(e) => setNameEn(e.target.value)} maxLength={200} placeholder="Optional" />
        </div>
        <div className="field">
          <label htmlFor="descriptionEn">English description</label>
          <textarea id="descriptionEn" value={descriptionEn} onChange={(e) => setDescriptionEn(e.target.value)} rows={4} maxLength={2000} placeholder="Optional" />
        </div>
        <div className="field">
          <label htmlFor="nameVi">Vietnamese name</label>
          <input id="nameVi" value={nameVi} onChange={(e) => setNameVi(e.target.value)} maxLength={200} placeholder="Optional" />
        </div>
        <div className="field">
          <label htmlFor="descriptionVi">Vietnamese description</label>
          <textarea id="descriptionVi" value={descriptionVi} onChange={(e) => setDescriptionVi(e.target.value)} rows={4} maxLength={2000} placeholder="Optional" />
        </div>
        <div className="field">
          <label htmlFor="price">Price (VND)</label>
          {/* min must itself be a multiple of step, or the browser rejects otherwise-valid
              round prices like 123000 (nearest "valid" steps from min=1 would be 122001/123001) */}
          <input id="price" type="number" min="1000" step="1000" value={price} onChange={(e) => setPrice(e.target.value)} required />
        </div>
        <div className="field">
          <label htmlFor="categoryId">Category</label>
          <select id="categoryId" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} required>
            <option value="" disabled>
              — Select a category —
            </option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>
        <button className="btn" type="submit" disabled={submitting}>
          {submitting ? 'Saving…' : isEdit ? 'Save changes' : 'Create product'}
        </button>
      </form>
    </div>
  );
}
