import { Link } from 'react-router-dom';

const ServiceCard = ({
  image,
  fallbackImage,
  title,
  subtitle,
  location,
  badges = [],
  price,
  originalPrice,
  priceUnit,
  discountPercent,
  providerName,
  linkTo,
  linkText = 'Ver más',
  cardStyle = 'primary',
  children,
}) => {
  const hasDiscount = originalPrice && originalPrice > price;

  const getButtonClass = () => {
    switch (cardStyle) {
      case 'success': return 'btn-success';
      case 'warning': return 'btn-warning';
      case 'danger': return 'btn-danger';
      case 'info': return 'btn-info';
      default: return 'btn-primary';
    }
  };

  return (
    <div className="card h-100 shadow-sm service-card">
      <div className="position-relative">
        <img
          src={image || fallbackImage}
          className="card-img-top"
          alt={title}
          style={{ height: '200px', objectFit: 'cover' }}
          onError={(e) => {
            e.target.src = fallbackImage;
          }}
        />
        
        {hasDiscount && (
          <span className="position-absolute top-0 end-0 m-2 badge bg-danger fs-6">
            <i className="bi bi-fire"></i> -{discountPercent || Math.round((1 - price / originalPrice) * 100)}%
          </span>
        )}
        
        {providerName && (
          <span className={`position-absolute bottom-0 start-0 m-2 badge bg-${cardStyle}`}>
            {providerName}
          </span>
        )}
      </div>

      <div className="card-body">
        {subtitle && <h6 className="text-muted mb-1 small">{subtitle}</h6>}
        <h5 className="card-title fw-bold mb-1">{title}</h5>
        
        {location && (
          <p className="text-muted small mb-2">
            <i className="bi bi-geo-alt"></i> {location}
          </p>
        )}

        {badges.length > 0 && (
          <div className="d-flex flex-wrap gap-1 mb-3">
            {badges.map((badge, index) => (
              <span key={index} className={`badge ${badge.className || 'bg-light text-dark border'}`}>
                {badge.icon && <i className={`bi ${badge.icon} me-1`}></i>}
                {badge.text}
              </span>
            ))}
          </div>
        )}

        {children}

        <div className="d-flex justify-content-between align-items-end">
          <div>
            {hasDiscount && (
              <small className="text-muted text-decoration-line-through">
                ${originalPrice?.toFixed(2)}
              </small>
            )}
            <div className={`fw-bold text-${cardStyle} fs-4`}>
              ${price?.toFixed(2)} {priceUnit && <small className="text-muted fw-normal fs-6">{priceUnit}</small>}
            </div>
          </div>

          {linkTo && (
            <Link to={linkTo} className={`btn ${getButtonClass()}`}>
              {linkText} →
            </Link>
          )}
        </div>
      </div>
    </div>
  );
};

export default ServiceCard;
