import { createContext, useContext, useState, ReactNode, useEffect } from 'react';
import { AuthUser, UserRole } from '../../models/ui_types/user';
import { authService } from '../../services/authService';
import { useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router';

interface AuthContextType {
  user: AuthUser | null;
  isLoading: boolean;
  setAuthUser: (user: AuthUser | null) => void;
  logout: () => void;
  switchRole: (role: UserRole) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [user, setUser] = useState<AuthUser | null>(() => {
    try {
      if (!localStorage.getItem('token')) return null;
      const saved = JSON.parse(localStorage.getItem('tech_sales_user') || 'null');
      return saved?.id && ['Customer', 'Staff', 'Business Admin', 'Technical Admin'].includes(saved.role) ? saved : null;
    } catch { return null; }
  });
  const [isLoading, setIsLoading] = useState(false);

  // Restore session by verifying token with the Backend on page init/refresh
  useEffect(() => {
    const handleUnauthorized = () => {
      setUser(null);
      setIsLoading(false);
      queryClient.clear();
      navigate('/auth/login', { replace: true });
    };
    window.addEventListener('techsales:unauthorized', handleUnauthorized);
    return () => window.removeEventListener('techsales:unauthorized', handleUnauthorized);
  }, [queryClient, navigate]);

  useEffect(() => {
    let active = true;
    const restoreSession = async () => {
      try {
        const currentUser = await authService.getCurrentUser();
        if (active) setUser(previous => currentUser && previous && currentUser.roles?.includes(previous.role)
          ? { ...currentUser, role: previous.role } : currentUser);
      } catch {
        // API client clears an invalid session on 401; network/server errors retain the token.
      } finally {
        if (active) setIsLoading(false);
      }
    };

    restoreSession();
    return () => { active = false; };
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
    queryClient.clear();
    await authService.logout();
  };

  const switchRole = (role: UserRole) => {
    if (user?.roles?.includes(role)) {
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
