import { AuthUser, UserRole, UserStatus } from '../models/ui_types/user';
import { users } from '../data/mockData';

export interface LoginParams {
  email: string;
  password?: string;
}

export interface RegisterParams {
  fullName: string;
  email: string;
  phone: string;
  password?: string;
}

const delay = (ms = 80) => new Promise(resolve => setTimeout(resolve, ms));

export const authService = {
  login: async ({ email, password }: LoginParams): Promise<AuthUser> => {
    await delay();
    const cleanEmail = email.trim().toLowerCase();
    
    // Look up in RAM mock users
    let found = users.find(u => u.email.toLowerCase() === cleanEmail);

    if (!found) {
      // Dynamic fallback based on email pattern or create new demo user
      let role: UserRole = 'Customer';
      if (cleanEmail.includes('admin') || cleanEmail.includes('tech')) {
        role = 'Technical Admin';
      } else if (cleanEmail.includes('business') || cleanEmail.includes('manager')) {
        role = 'Business Admin';
      } else if (cleanEmail.includes('staff') || cleanEmail.includes('sales')) {
        role = 'Staff';
      }

      found = {
        id: `u-${Date.now()}`,
        email: email.trim(),
        status: UserStatus.ACTIVE,
        role,
        fullName: email.split('@')[0],
        phone: '+84 901 234 567',
        password: password || 'password',
        createdAt: new Date().toISOString(),
      };
      users.push(found);
    }

    if (found.status === UserStatus.BLOCKED) {
      throw new Error('This account has been locked. Please contact support.');
    }

    const authUser: AuthUser = {
      id: found.id,
      name: found.fullName || found.email.split('@')[0],
      email: found.email,
      role: found.role,
      phone: found.phone,
      avatarUrl: found.avatarUrl,
    };

    localStorage.setItem('token', `mock-token-${found.id}`);
    localStorage.setItem('tech_sales_user', JSON.stringify(authUser));

    return authUser;
  },

  register: async (params: RegisterParams): Promise<AuthUser> => {
    await delay();
    const cleanEmail = params.email.trim().toLowerCase();
    const existing = users.find(u => u.email.toLowerCase() === cleanEmail);
    if (existing) {
      throw new Error('An account with this email already exists.');
    }

    const newUser = {
      id: `u-${Date.now()}`,
      email: params.email.trim(),
      status: UserStatus.ACTIVE,
      role: 'Customer' as UserRole,
      fullName: params.fullName,
      phone: params.phone,
      password: params.password || 'password',
      createdAt: new Date().toISOString(),
    };
    users.push(newUser);

    const authUser: AuthUser = {
      id: newUser.id,
      name: newUser.fullName,
      email: newUser.email,
      role: newUser.role,
      phone: newUser.phone,
    };

    localStorage.setItem('token', `mock-token-${newUser.id}`);
    localStorage.setItem('tech_sales_user', JSON.stringify(authUser));

    return authUser;
  },

  resetPassword: async (_email: string): Promise<void> => {
    await delay();
  },

  confirmResetPassword: async (
    password: string,
    _confirmPassword: string,
    _token: string,
    email: string = ''
  ): Promise<void> => {
    await delay();
    const user = users.find(u => u.email.toLowerCase() === email.toLowerCase());
    if (user) {
      user.password = password;
    }
  },

  verifyEmail: async (_token: string, _email: string = ''): Promise<void> => {
    await delay();
  },

  changePassword: async (
    _oldPassword: string,
    newPassword: string,
    _confirmPassword: string
  ): Promise<void> => {
    await delay();
    const current = await authService.getCurrentUser();
    if (current) {
      const user = users.find(u => u.id === current.id);
      if (user) {
        user.password = newPassword;
      }
    }
  },

  getCurrentUser: async (): Promise<AuthUser | null> => {
    const raw = localStorage.getItem('tech_sales_user');
    if (!raw) return null;
    try {
      const parsed = JSON.parse(raw) as AuthUser;
      // Sync with RAM user in case profile was updated
      const inRam = users.find(u => u.id === parsed.id || u.email.toLowerCase() === parsed.email.toLowerCase());
      if (inRam) {
        return {
          id: inRam.id,
          name: inRam.fullName || inRam.email.split('@')[0],
          email: inRam.email,
          role: inRam.role,
          phone: inRam.phone,
          avatarUrl: inRam.avatarUrl,
        };
      }
      return parsed;
    } catch {
      localStorage.removeItem('tech_sales_user');
      localStorage.removeItem('token');
      return null;
    }
  },

  logout: async (): Promise<void> => {
    await delay(30);
    localStorage.removeItem('token');
    localStorage.removeItem('tech_sales_user');
  },
};
