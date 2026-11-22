import { AuthUser, UserRole } from '../models/ui_types/user';
import api, { apiAssetUrl } from '../api/apiClient';

export interface LoginParams { email: string; password?: string; }
export interface RegisterParams { fullName: string; email: string; phone: string; password?: string; confirmPassword?: string; }
interface MeResponse {
  id: string; email: string; roles: UserRole[];
  profile: { fullName: string; phone: string; avatarUrl?: string };
}
const supportedRoles: UserRole[] = ['Business Admin', 'Technical Admin', 'Staff', 'Customer'];
const mapUser = (me: MeResponse): AuthUser => ({
  id: me.id, email: me.email, name: me.profile.fullName || me.email,
  role: supportedRoles.find(role => me.roles.includes(role)) || 'Customer',
  roles: me.roles, phone: me.profile.phone, avatarUrl: apiAssetUrl(me.profile.avatarUrl),
});

export const authService = {
  login: async ({ email, password }: LoginParams): Promise<AuthUser> => {
    const result = await api.post<{ token: string }>('/api/auth/login', { email: email.trim(), password });
    localStorage.setItem('token', result.token);
    try {
      const user = mapUser(await api.get<MeResponse>('/api/user/me'));
      localStorage.setItem('tech_sales_user', JSON.stringify(user));
      return user;
    } catch (error) {
      localStorage.removeItem('token');
      localStorage.removeItem('tech_sales_user');
      throw error;
    }
  },
  register: async (params: RegisterParams): Promise<void> => {
    await api.post('/api/auth/register', { email: params.email, password: params.password,
      confirmPassword: params.confirmPassword ?? params.password });
  },
  resetPassword: async (email: string): Promise<void> => {
    await api.post('/api/auth/forgot-password', { email });
  },
  confirmResetPassword: async (newPassword: string, confirmPassword: string, token: string, email = ''): Promise<void> => {
    await api.put('/api/auth/reset-password', { email, token, newPassword, confirmPassword });
  },
  verifyEmail: async (token: string, email = ''): Promise<void> => {
    await api.post('/api/auth/verify-email', { token, email });
  },
  changePassword: async (currentPassword: string, newPassword: string, confirmPassword: string): Promise<void> => {
    await api.put('/api/auth/change-password', { currentPassword, newPassword, confirmPassword });
  },
  getCurrentUser: async (): Promise<AuthUser | null> => {
    const token = localStorage.getItem('token');
    if (!token || token.startsWith('mock-token-')) {
      localStorage.removeItem('token');
      localStorage.removeItem('tech_sales_user');
      return null;
    }
    return mapUser(await api.get<MeResponse>('/api/user/me'));
  },
  logout: async (): Promise<void> => {
    try {
      if (localStorage.getItem('token')) await api.post('/api/auth/logout');
    } finally {
      localStorage.removeItem('token');
      localStorage.removeItem('tech_sales_user');
    }
  },
};