import { User, UserRole, UserStatus } from '../models/ui_types/user';
import { AuditLog } from '../models/ui_types/auditLog';
import { users, mockAuditLogs } from '../data/mockData';

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

const delay = (ms = 50) => new Promise(resolve => setTimeout(resolve, ms));

export const userService = {
  getUsers: async (): Promise<User[]> => {
    await delay();
    return [...users];
  },

  getCustomers: async (): Promise<User[]> => {
    await delay();
    return users.filter(u => u.role === 'Customer');
  },

  getStaff: async (): Promise<User[]> => {
    await delay();
    return users.filter(u => u.role !== 'Customer');
  },

  getUserById: async (id: string): Promise<User> => {
    await delay();
    const user = users.find(u => u.id === id) || users[0];
    return { ...user };
  },

  createUser: async (userData: Omit<User, 'id' | 'createdAt'>): Promise<User> => {
    await delay();
    const newUser: User = {
      id: `u${Date.now()}`,
      email: userData.email,
      fullName: userData.fullName || userData.email.split('@')[0],
      phone: userData.phone || '',
      role: userData.role || 'Staff',
      status: userData.status || UserStatus.ACTIVE,
      password: userData.password || 'password123',
      createdAt: new Date().toISOString(),
    };
    users.unshift(newUser);

    mockAuditLogs.unshift({
      id: `log-${Date.now()}`,
      createdAt: new Date().toISOString(),
      action: 'CREATE',
      tableName: 'Users',
      primaryKey: newUser.id,
      newValues: JSON.stringify({ email: newUser.email, role: newUser.role }),
    });

    return { ...newUser };
  },

  updateUser: async (id: string, userData: Partial<User>): Promise<User> => {
    await delay();
    const index = users.findIndex(u => u.id === id);
    if (index === -1) {
      throw new Error(`User ${id} not found`);
    }

    users[index] = {
      ...users[index],
      ...userData,
      updatedAt: new Date().toISOString(),
    };

    return { ...users[index] };
  },

  updateProfile: async (data: {
    fullName?: string;
    phone?: string;
    avatarUrl?: string;
    dateOfBirth?: string;
    avatarFile?: File;
  }): Promise<void> => {
    await delay();
    let currentId = 'u4';
    try {
      const rawUser = localStorage.getItem('tech_sales_user');
      if (rawUser) {
        const parsed = JSON.parse(rawUser);
        if (parsed.id) currentId = parsed.id;
      }
    } catch {}

    const user = users.find(u => u.id === currentId);
    if (user) {
      if (data.fullName !== undefined) user.fullName = data.fullName;
      if (data.phone !== undefined) user.phone = data.phone;
      if (data.dateOfBirth !== undefined) user.dateOfBirth = data.dateOfBirth;
      if (data.avatarUrl !== undefined) user.avatarUrl = data.avatarUrl;
      if (data.avatarFile) {
        user.avatarUrl = URL.createObjectURL(data.avatarFile);
      }
      user.updatedAt = new Date().toISOString();

      // Update cached session
      try {
        const rawUser = localStorage.getItem('tech_sales_user');
        if (rawUser) {
          const parsed = JSON.parse(rawUser);
          parsed.name = user.fullName;
          parsed.phone = user.phone;
          parsed.avatarUrl = user.avatarUrl;
          localStorage.setItem('tech_sales_user', JSON.stringify(parsed));
        }
      } catch {}
    }
  },

  toggleUserStatus: async (id: string, currentStatus: UserStatus): Promise<User> => {
    await delay();
    const user = users.find(u => u.id === id);
    if (!user) {
      throw new Error(`User ${id} not found`);
    }

    user.status = currentStatus === UserStatus.BLOCKED ? UserStatus.ACTIVE : UserStatus.BLOCKED;
    user.updatedAt = new Date().toISOString();

    mockAuditLogs.unshift({
      id: `log-${Date.now()}`,
      createdAt: new Date().toISOString(),
      action: 'UPDATE',
      tableName: 'Users',
      primaryKey: user.id,
      affectedColumns: 'status',
      newValues: JSON.stringify({ status: user.status }),
    });

    return { ...user };
  },

  getAuditLogs: async (pageNumber = 1, pageSize = 50): Promise<PagedResponse<AuditLog>> => {
    await delay();
    const totalCount = mockAuditLogs.length;
    const startIndex = (pageNumber - 1) * pageSize;
    const items = mockAuditLogs.slice(startIndex, startIndex + pageSize);

    return {
      items,
      pageNumber,
      pageSize,
      totalCount,
      totalPages: Math.ceil(totalCount / pageSize) || 1,
    };
  },
};
