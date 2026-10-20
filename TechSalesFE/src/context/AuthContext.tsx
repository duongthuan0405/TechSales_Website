import { createContext, useContext, useState, ReactNode, useEffect } from 'react';
import { AuthUser, UserRole } from '../models/ui_types/user';
import { authService } from '../services/authService';

interface AuthContextType {
  user: AuthUser | null;
  isLoading: boolean;
  setAuthUser: (user: AuthUser | null) => void;
  logout: () => void;
  switchRole: (role: UserRole) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // On init: try to restore session from JWT token
  useEffect(() => {
    const restoreSession = async () => {
      try {
        const currentUser = await authService.getCurrentUser();
        setUser(currentUser);
      } catch {
        // Token invalid or network error — start fresh
        localStorage.removeItem('token');
        localStorage.removeItem('tech_sales_user');
      } finally {
        setIsLoading(false);
      }
    };

    restoreSession();
  }, []);

  const setAuthUser = (newUser: AuthUser | null) => {
    setUser(newUser);
    if (newUser) {
      localStorage.setItem('tech_sales_user', JSON.stringify(newUser));
    } else {
      localStorage.removeItem('tech_sales_user');
    }
  };

  const logout = async () => {
    setUser(null);
    await authService.logout();
  };

  const switchRole = (role: UserRole) => {
    if (user) {
      const updatedUser = { ...user, role };
      setAuthUser(updatedUser);
    }
  };

  return (
    <AuthContext.Provider value={{ user, isLoading, setAuthUser, logout, switchRole }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
