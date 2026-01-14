const PageHeader = ({ title, subtitle, icon, gradient = 'primary' }) => {
  const gradients = {
    primary: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    success: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
    danger: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
    warning: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
    info: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
    light: 'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
  };

  const textColor = gradient === 'light' ? 'text-dark' : 'text-white';

  return (
    <div className="row mb-4">
      <div className="col">
        <div
          className={`p-4 rounded-4 ${textColor} mb-3`}
          style={{ background: gradients[gradient] || gradients.primary }}
        >
          <h1 className="display-5 fw-bold mb-2">
            {icon && <i className={`bi ${icon} me-2`}></i>}
            {title}
          </h1>
          {subtitle && <p className="mb-0 opacity-90">{subtitle}</p>}
        </div>
      </div>
    </div>
  );
};

export default PageHeader;
