const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '');

export function apiAssetUrl(url?: string | null): string | undefined {
  if (!url) return undefined;
  return url.startsWith('/') ? `${apiBaseUrl}${url}` : url;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export class ApiError extends Error {
  status: number;
  errors?: Record<string, string[]>;

  constructor(message: string, status: number, errors?: Record<string, string[]>) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.errors = errors;
  }
}

let sessionEpoch = 0;
let refreshPromise: Promise<string> | null = null;

export function startSession(token: string) {
  sessionEpoch++;
  localStorage.setItem('token', token);
}

export function clearSession() {
  sessionEpoch++;
  localStorage.removeItem('token');
  localStorage.removeItem('tech_sales_user');
}

export async function refreshAccessToken(): Promise<string> {
  if (refreshPromise) return refreshPromise;
  const epoch = sessionEpoch;
  const previous = localStorage.getItem('token');
  const identity = localStorage.getItem('tech_sales_user');
  const refresh = async () => {
    if (epoch !== sessionEpoch || localStorage.getItem('tech_sales_user') !== identity)
      throw new ApiError('Phiên đăng nhập đã thay đổi. Vui lòng thử lại.', 0);
    const current = localStorage.getItem('token');
    if (current !== previous) {
      if (current) return current; // Another tab refreshed while this tab waited for the lock.
      throw new ApiError('Phiên đăng nhập đã thay đổi. Vui lòng thử lại.', 0);
    }
    let response: Response;
    try {
      response = await fetch(`${apiBaseUrl}/api/auth/refresh`, {
        method: 'POST', credentials: 'include', headers: { 'X-Requested-With': 'XMLHttpRequest' },
      });
    } catch { throw new ApiError('Không kết nối được máy chủ để làm mới phiên đăng nhập.', 0); }
    const payload = await response.json().catch(() => null);
    if (epoch !== sessionEpoch || localStorage.getItem('token') !== previous || localStorage.getItem('tech_sales_user') !== identity)
      throw new ApiError('Phiên đăng nhập đã thay đổi. Vui lòng thử lại.', 0);
    if (!response.ok || payload?.success === false)
      throw new ApiError(payload?.message || 'Không thể làm mới phiên đăng nhập.', response.status, payload?.errors ?? payload?.data);
    const token = payload?.data?.token;
    if (typeof token !== 'string' || !token) throw new ApiError('Phản hồi làm mới phiên không hợp lệ.', 502);
    localStorage.setItem('token', token);
    return token;
  };
  refreshPromise = typeof navigator !== 'undefined' && navigator.locks
    ? navigator.locks.request('techsales-refresh', refresh) : refresh();
  try { return await refreshPromise; }
  finally { refreshPromise = null; }
}

const isPublicAuth = (endpoint: string) => /^\/api\/auth\/(login|register|refresh|forgot-password|reset-password|verify-email|logout)(?:\?|$)/.test(endpoint);

function unauthorized() {
  clearSession();
  window.dispatchEvent(new Event('techsales:unauthorized'));
}

async function apiRequest<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const epoch = sessionEpoch;
  const identity = localStorage.getItem('tech_sales_user');
  for (let attempt = 0; attempt < 2; attempt++) {
    if (epoch !== sessionEpoch || localStorage.getItem('tech_sales_user') !== identity)
      throw new ApiError('Phiên đăng nhập đã thay đổi. Vui lòng thử lại.', 0);
    const headers = new Headers(options.headers);
    headers.set('X-Requested-With', 'XMLHttpRequest');
    const token = localStorage.getItem('token');
    if (token && !token.startsWith('mock-token-')) headers.set('Authorization', `Bearer ${token}`);
    if (options.body && !(options.body instanceof FormData)) headers.set('Content-Type', 'application/json');
    let response: Response;
    try {
      response = await fetch(`${apiBaseUrl}${endpoint}`, { ...options, credentials: 'include', headers });
    } catch {
      throw new ApiError('Không kết nối được máy chủ. Vui lòng kiểm tra backend đang chạy.', 0);
    }
    const payload = await response.json().catch(() => null);
    if (epoch !== sessionEpoch) throw new ApiError('Phiên đăng nhập đã thay đổi. Vui lòng thử lại.', 0);
    if (response.status === 401 && !isPublicAuth(endpoint) && attempt === 0) {
      const current = localStorage.getItem('token');
      if (current && current !== token) continue; // Another request already refreshed this access token.
      try { await refreshAccessToken(); }
      catch (error) {
        if (epoch === sessionEpoch && error instanceof ApiError && [400, 401, 403].includes(error.status)) unauthorized();
        throw error;
      }
      continue;
    }
    if (!response.ok || payload?.success === false) {
      if (response.status === 401 && !isPublicAuth(endpoint)) unauthorized();
      throw new ApiError(payload?.message || `Yêu cầu thất bại (${response.status}).`, response.status, payload?.errors ?? payload?.data);
    }
    return payload?.data as T;
  }
  throw new ApiError('Phiên đăng nhập không hợp lệ.', 401);
}

const requestBody = (body?: unknown) => body instanceof FormData ? body : body === undefined ? undefined : JSON.stringify(body);

export const api = {
  get: <T>(endpoint: string) => apiRequest<T>(endpoint, { method: 'GET' }),
  post: <T>(endpoint: string, body?: unknown) => apiRequest<T>(endpoint, { method: 'POST', body: requestBody(body) }),
  put: <T>(endpoint: string, body?: unknown) => apiRequest<T>(endpoint, { method: 'PUT', body: requestBody(body) }),
  patch: <T>(endpoint: string, body?: unknown) => apiRequest<T>(endpoint, { method: 'PATCH', body: requestBody(body) }),
  delete: <T>(endpoint: string) => apiRequest<T>(endpoint, { method: 'DELETE' }),
};

export default api;
