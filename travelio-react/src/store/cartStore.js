import { create } from 'zustand';
import { persist } from 'zustand/middleware';

const useCartStore = create(
  persist(
    (set, get) => ({
      items: [],
      ivaPercent: 15,
      
      addItem: (item) => {
        const { items } = get();
        const existingIndex = items.findIndex(
          (i) => i.titulo === item.titulo && i.tipo === item.tipo
        );
        
        if (existingIndex >= 0) {
          // Si es un servicio de reserva, no aumentar cantidad
          const isReserva = ['CAR', 'HOTEL', 'FLIGHT', 'RESTAURANT', 'PACKAGE'].includes(item.tipo);
          if (isReserva) return;
          
          const newItems = [...items];
          newItems[existingIndex].cantidad += 1;
          set({ items: newItems });
        } else {
          set({ items: [...items, { ...item, cantidad: 1 }] });
        }
      },
      
      removeItem: (titulo) => {
        set((state) => ({
          items: state.items.filter((item) => item.titulo !== titulo),
        }));
      },
      
      updateQuantity: (titulo, cantidad) => {
        if (cantidad <= 0) {
          get().removeItem(titulo);
          return;
        }
        
        set((state) => ({
          items: state.items.map((item) =>
            item.titulo === titulo ? { ...item, cantidad } : item
          ),
        }));
      },
      
      clearCart: () => {
        set({ items: [] });
      },
      
      getSubtotal: () => {
        return get().items.reduce(
          (acc, item) => acc + item.precioFinal * item.cantidad,
          0
        );
      },
      
      getIva: () => {
        const subtotal = get().getSubtotal();
        return subtotal * (get().ivaPercent / 100);
      },
      
      getTotal: () => {
        return get().getSubtotal() + get().getIva();
      },
      
      getTotalItems: () => {
        return get().items.reduce((acc, item) => acc + item.cantidad, 0);
      },
    }),
    {
      name: 'cart-storage',
    }
  )
);

export default useCartStore;
