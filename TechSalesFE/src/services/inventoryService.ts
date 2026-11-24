import api from '../api/apiClient';

export type UnitStatus = 'IN_STOCK' | 'RESERVED' | 'SOLD' | 'DAMAGED';
export interface InventoryUnit { productId: string; serialNumber: string; status: UnitStatus; }
export interface UnitQuery { productId?: string; keyword?: string; status?: string; pageNumber: number; pageSize: number; }
export interface StockImportResult { receivedQuantity: number; status: string; products: { productId: string; receivedQuantity: number; quantity: number }[]; }
export const inventoryService = {
  importExcel: (file: File): Promise<StockImportResult> => {
    const body = new FormData();
    body.append('file', file);
    return api.post('/api/inventory/import', body);
  },
  getUnits: (params: UnitQuery): Promise<{ items: InventoryUnit[]; totalCount: number }> => {
    const query = new URLSearchParams();
    for (const [key, value] of Object.entries(params)) {
      if (value !== undefined && value !== '') query.set(key, String(value));
    }
    return api.get(`/api/inventory/units?${query}`);
  },
  receive: (productId: string, serialNumbers: string[]) => api.post(`/api/inventory/${encodeURIComponent(productId)}/receive`, { serialNumbers }),
  markDamaged: (unit: InventoryUnit) => api.patch(`/api/inventory/${encodeURIComponent(unit.productId)}/units/status`, { serialNumber: unit.serialNumber, status: 'DAMAGED' }),
  restoreToStock: (unit: InventoryUnit) => api.patch(`/api/inventory/${encodeURIComponent(unit.productId)}/units/status`, { serialNumber: unit.serialNumber, status: 'IN_STOCK' }),
};
