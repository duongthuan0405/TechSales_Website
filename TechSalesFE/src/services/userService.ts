import { User, UserRole, UserStatus } from '../models/ui_types/user';
import { AuditLog } from '../models/ui_types/auditLog';
import api, { ApiError, apiAssetUrl } from '../api/apiClient';

export interface PagedResponse<T> { items: T[]; pageNumber: number; pageSize: number; totalCount: number; totalPages: number; }
interface UserRecord { id: string; email: string; status: UserStatus; createdAt: string; roles: UserRole[];
  profile: { fullName: string; phone: string; avatarUrl?: string; dateOfBirth?: string }; }
const mapUser = (row: UserRecord): User => ({ id: row.id, email: row.email, status: row.status,
  createdAt: row.createdAt, ...row.profile, avatarUrl: apiAssetUrl(row.profile.avatarUrl),
  role: row.roles[0] ?? 'Customer', roles: row.roles });
const list = async (kind: 'customers' | 'staff'): Promise<User[]> => {
  const result: User[] = [];
  for (let page = 1; ; page++) {
    const data = await api.get<PagedResponse<UserRecord>>(`/api/admin/users/${kind}?pageNumber=${page}&pageSize=100`);
    result.push(...data.items.map(mapUser));
    if (!data.items.length || page * data.pageSize >= data.totalCount) return result;
  }
};

export const userService = {
  getUsers: async (): Promise<User[]> => {
    const session = JSON.parse(localStorage.getItem('tech_sales_user') || 'null');
    const roles: string[] = session?.roles ?? [session?.role];
    const customers = await list('customers');
    const staff = roles.some(role => ['Business Admin', 'Technical Admin'].includes(role)) ? await list('staff') : [];
    return [...new Map([...customers, ...staff].map(user => [user.id, user])).values()];
  },
  getCustomers: (): Promise<User[]> => list('customers'),
  getStaff: (): Promise<User[]> => list('staff'),
  getUserById: async (id: string): Promise<User> => {
    const raw = localStorage.getItem('tech_sales_user');
    if (raw && JSON.parse(raw).id === id) return mapUser(await api.get<UserRecord>('/api/user/me'));
    const user = (await userService.getUsers()).find(item => item.id === id);
    if (!user) throw new ApiError('Không tìm thấy người dùng.', 404);
    return user;
  },
  createUser: async (data: Omit<User, 'id' | 'createdAt'>): Promise<User> =>
    mapUser(await api.post<UserRecord>('/api/admin/users', { email: data.email, password: data.password, roles: [data.role] })),
  updateUser: async (id: string, data: Partial<User>): Promise<User> => {
    const current = await userService.getUserById(id);
    if (data.role && !current.roles?.includes(data.role)) {
      const roles = await api.get<{ id: string; name: string }[]>('/api/admin/rbac/roles');
      const role = roles.find(role => role.name === data.role);
      if (!role) throw new ApiError('Không tìm thấy vai trò.', 400);
      await api.put(`/api/admin/rbac/users/${id}/roles`, { roleIds: [role.id] });
    }
    if (data.email !== undefined && data.email !== current.email) await api.put(`/api/admin/users/${id}`, { email: data.email });
    return userService.getUserById(id);
  },
  updateProfile: async (data: { fullName?: string; phone?: string; avatarUrl?: string; dateOfBirth?: string; avatarFile?: File }): Promise<void> => {
    const body = new FormData();
    if (data.fullName !== undefined) body.set('FullName', data.fullName);
    if (data.phone !== undefined) body.set('Phone', data.phone);
    if (data.dateOfBirth) body.set('DateOfBirth', data.dateOfBirth.slice(0, 10));
    if (data.avatarFile) body.set('AvatarFile', data.avatarFile);
    else if (data.avatarUrl !== undefined) body.set('AvatarUrl', data.avatarUrl);
    await api.put('/api/user-profile', body);
  },
  toggleUserStatus: async (id: string, status: UserStatus): Promise<User> => {
    await api.post(`/api/admin/users/${id}/${status === UserStatus.BLOCKED ? 'unlock' : 'lock'}`,
      status === UserStatus.BLOCKED ? undefined : { until: null });
    return userService.getUserById(id);
  },
  getAuditLogs: async (_page = 1, _size = 50): Promise<PagedResponse<AuditLog>> => {
    throw new ApiError('Chưa có API nhật ký hệ thống.', 501);
  },
};
