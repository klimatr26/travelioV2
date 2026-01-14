import { Routes, Route } from 'react-router-dom';

// Layout
import Layout from './components/layout/Layout';

// Pages
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import Carrito from './pages/Carrito';
import Checkout from './pages/Checkout';
import Profile from './pages/Profile';
import Admin from './pages/Admin';
import Modulos from './pages/Modulos';

// Service Pages
import Hoteles from './pages/Hoteles';
import Vuelos from './pages/Vuelos';
import Autos from './pages/Autos';
import Restaurantes from './pages/Restaurantes';
import Paquetes from './pages/Paquetes';

function App() {
  return (
    <Routes>
      {/* Admin - Sin Layout */}
      <Route path="/admin" element={<Admin />} />
      
      {/* Rutas con Layout */}
      <Route element={<Layout />}>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/carrito" element={<Carrito />} />
        <Route path="/checkout" element={<Checkout />} />
        <Route path="/profile" element={<Profile />} />
        <Route path="/modulos" element={<Modulos />} />
        
        {/* Servicios */}
        <Route path="/hoteles" element={<Hoteles />} />
        <Route path="/vuelos" element={<Vuelos />} />
        <Route path="/autos" element={<Autos />} />
        <Route path="/restaurantes" element={<Restaurantes />} />
        <Route path="/paquetes" element={<Paquetes />} />
      </Route>
    </Routes>
  );
}

export default App;
