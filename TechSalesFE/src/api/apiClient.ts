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

async function apiRequest<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  const token = localStorage.getItem('token');
  if (token && !token.startsWith('mock-token-')) headers.set('Authorization', `Bearer ${token}`);
  if (options.body && !(options.body instanceof FormData)) headers.set('Content-Type', 'application/json');
  let response: Response;
  try {
    response = await fetch(`${apiBaseUrl}${endpoint}`, { ...options, headers });
  } catch {
    throw new ApiError('Không kết nối được máy chủ. Vui lòng kiểm tra backend đang chạy.', 0);
  }
  const payload = await response.json().catch(() => null);
  if (!response.ok || payload?.success === false) {
    if (response.status === 401 && !endpoint.startsWith('/api/auth/')) {
      localStorage.removeItem('token');
      localStorage.removeItem('tech_sales_user');
      window.dispatchEvent(new Event('techsales:unauthorized'));
    }
    throw new ApiError(payload?.message || `Yêu cầu thất bại (${response.status}).`, response.status, payload?.data);
  }
  return payload?.data as T;
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
