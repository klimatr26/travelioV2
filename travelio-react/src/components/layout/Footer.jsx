const Footer = () => {
  return (
    <footer className="bg-white border-top py-4 mt-auto">
      <div className="container text-center">
        <div className="mb-2">
          <i className="bi bi-airplane-fill text-primary"></i>
        </div>
        <p className="text-muted small mb-0">
          © {new Date().getFullYear()} – <strong>Travelio</strong>
        </p>
      </div>
    </footer>
  );
};

export default Footer;
