const Loading = ({ message = 'Cargando...' }) => {
  return (
    <div className="loading-container">
      <div className="text-center">
        <div className="spinner-border text-primary mb-3" role="status" style={{ width: '3rem', height: '3rem' }}>
          <span className="visually-hidden">{message}</span>
        </div>
        <p className="text-muted">{message}</p>
      </div>
    </div>
  );
};

export default Loading;
