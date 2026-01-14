import { create } from 'zustand';
import { persist } from 'zustand/middleware';

const useAuthStore = create(
  persist(
    (set, get) => ({
      user: null,
      isLoggedIn: false,
      isAdmin: false,
      
      login: (userData) => {
        set({
          user: userData,
          isLoggedIn: true,
          isAdmin: userData.isAdmin || false,
        });
      },
      
      logout: () => {
        set({
          user: null,
          isLoggedIn: false,
          isAdmin: false,
        });
        localStorage.removeItem('authToken');
      },
      
      updateUser: (userData) => {
        set((state) => ({
          user: { ...state.user, ...userData },
        }));
      },
    }),
    {
      name: 'auth-storage',
    }
  )
);

export default useAuthStore;
