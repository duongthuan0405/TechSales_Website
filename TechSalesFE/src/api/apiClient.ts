/**
 * Mock API Client cho TechSales FE (Demo In-RAM).
 * Lưu trữ và xử lý trực tiếp trên RAM thay vì gọi Backend.
 */

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

// In-RAM mock request handler
async function mockApiRequest<T>(_endpoint: string, _options: RequestInit = {}): Promise<T> {
  return {} as T;
}

export const api = {
  get: <T>(endpoint: string) => mockApiRequest<T>(endpoint, { method: 'GET' }),
  post: <T>(endpoint: string, body?: unknown) => mockApiRequest<T>(endpoint, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(endpoint: string, body?: unknown) => mockApiRequest<T>(endpoint, { method: 'PUT', body: JSON.stringify(body) }),
  patch: <T>(endpoint: string, body?: unknown) => mockApiRequest<T>(endpoint, { method: 'PATCH', body: JSON.stringify(body) }),
  delete: <T>(endpoint: string) => mockApiRequest<T>(endpoint, { method: 'DELETE' }),
};

export default api;
