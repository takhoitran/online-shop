import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div className="empty-state">
      <p>The page you requested could not be found.</p>
      <Link to="/" className="btn">
        Back to home
      </Link>
    </div>
  );
}
