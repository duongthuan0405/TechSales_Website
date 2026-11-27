import { User, UserRole, UserStatus } from '../models/ui_types/user';
import { AuditLog } from '../models/ui_types/auditLog';
import api, { ApiError, apiAssetUrl } from '../api/apiClient';

export interface PagedResponse<T> { items: T[]; pageNumber: number; pageSize: number; totalCount: number; totalPages: number; }
export interface AccountPageParams { pageNumber: number; pageSize: number; keyword?: string; role?: string; }
interface UserRecord { id: string; email: string; status: UserStatus; createdAt: string; roles: UserRole[];
  profile: { fullName: string; phone: string; avatarUrl?: string; dateOfBirth?: string }; }
const mapUser = (row: UserRecord): User => ({ id: row.id, email: row.email, status: row.status,
  createdAt: row.createdAt, ...row.profile, avatarUrl: apiAssetUrl(row.profile.avatarUrl),
  role: row.roles[0] ?? 'Customer', roles: row.roles });
const list = async (kind: 'customers' | 'staff', role?: string, keyword?: string): Promise<User[]> => {
  const result: User[] = [];
  for (let page = 1; ; page++) {
    const roleQuery = role ? `&role=${encodeURIComponent(role)}` : '';
    const keywordQuery = keyword?.trim() ? `&keyword=${encodeURIComponent(keyword.trim())}` : '';
    const data = await api.get<PagedResponse<UserRecord>>(`/api/admin/users/${kind}?pageNumber=${page}&pageSize=100${roleQuery}${keywordQuery}`);
    result.push(...data.items.map(mapUser));
    if (!data.items.length || page * data.pageSize >= data.totalCount) return result;
  }
};

export const userService = {
  getAccountPage: async (kind: 'customers' | 'staff' | 'all', params: AccountPageParams): Promise<PagedResponse<User>> => {
    const query = new URLSearchParams({ pageNumber: String(params.pageNumber), pageSize: String(params.pageSize) });
    if (params.keyword?.trim()) query.set('keyword', params.keyword.trim());
    if (kind !== 'customers' && params.role && params.role !== 'all') query.set('role', params.role);
    const path = kind === 'all' ? '/api/admin/users' : `/api/admin/users/${kind}`;
    const data = await api.get<PagedResponse<UserRecord>>(`${path}?${query}`);
    return { ...data, items: data.items.map(mapUser) };
  },
  getUsers: async (role?: string, keyword?: string): Promise<User[]> => {
    if (role && role !== 'all') return role === 'Customer' ? list('customers', undefined, keyword) : list('staff', role, keyword);
    const session = JSON.parse(localStorage.getItem('tech_sales_user') || 'null');
    const roles: string[] = session?.roles ?? [session?.role];
    const customers = await list('customers', undefined, keyword);
    const staff = roles.some(role => ['Business Admin', 'Technical Admin'].includes(role)) ? await list('staff', undefined, keyword) : [];
    return [...new Map([...customers, ...staff].map(user => [user.id, user])).values()];
  },
  getCustomers: (): Promise<User[]> => list('customers'),
  getStaff: (role?: string): Promise<User[]> => list('staff', role && role !== 'all' ? role : undefined),
  getUserById: async (id: string): Promise<User> => {
    const raw = localStorage.getItem('tech_sales_user');
    if (raw && JSON.parse(raw).id === id) return mapUser(await api.get<UserRecord>('/api/user/me'));
    const user = (await userService.getUsers()).find(item => item.id === id);
    if (!user) throw new ApiError('Không tìm thấy người dùng.', 404);
    return user;
  },
  createUser: async (data: Omit<User, 'id' | 'createdAt'>): Promise<User> =>
    mapUser(await api.post<UserRecord>('/api/admin/users', { email: data.email, password: data.password, roles: data.roles ?? [data.role], fullName: data.fullName.trim(), phone: data.phone.trim() })),
  updateUser: async (id: string, data: Partial<User>): Promise<User> => {
    const current = await userService.getUserById(id);
    const requestedRoles = data.roles ?? (data.role ? [data.role] : undefined);
    const currentRoles = current.roles?.length ? current.roles : [current.role];
    if (requestedRoles && (!requestedRoles.length || requestedRoles.length !== currentRoles.length || requestedRoles.some(role => !currentRoles.includes(role)))) {
      if (!requestedRoles.length) throw new ApiError('Chọn ít nhất một vai trò.', 400);
      const roles = await api.get<{ id: string; name: string }[]>('/api/admin/rbac/roles');
      const roleIds = [...new Set(requestedRoles)].map(name => {
        const role = roles.find(role => role.name === name);
        if (!role) throw new ApiError('Không tìm thấy vai trò.', 400);
        return role.id;
      });
      await api.put(`/api/admin/rbac/users/${id}/roles`, { roleIds });
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
  deactivateUser: async (id: string): Promise<void> => {
    await api.post(`/api/admin/users/${encodeURIComponent(id)}/deactivate`);
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
