const SearchForm = ({ children, onSubmit, className = '' }) => {
  const handleSubmit = (e) => {
    e.preventDefault();
    if (onSubmit) {
      const formData = new FormData(e.target);
      const data = Object.fromEntries(formData.entries());
      onSubmit(data);
    }
  };

  return (
    <div className={`card shadow-sm mb-4 ${className}`}>
      <div className="card-body">
        <form onSubmit={handleSubmit}>
          <div className="row g-3">
            {children}
          </div>
        </form>
      </div>
    </div>
  );
};

export default SearchForm;
