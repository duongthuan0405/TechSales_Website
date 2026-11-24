import { ApiError } from '../api/apiClient';

export function unsupportedFeature(feature: string): never {
  throw new ApiError(`Chức năng ${feature} chưa có API hỗ trợ.`, 501);
}
