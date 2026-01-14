import { Outlet } from 'react-router-dom';
import Navbar from './Navbar';
import Footer from './Footer';

const Layout = () => {
  return (
    <div className="d-flex flex-column min-vh-100 bg-light">
      <Navbar />
      <div style={{ height: '80px' }}></div>
      <div className="container flex-grow-1 my-4">
        <main role="main" className="pb-3">
          <Outlet />
        </main>
      </div>
      <Footer />
    </div>
  );
};

export default Layout;
