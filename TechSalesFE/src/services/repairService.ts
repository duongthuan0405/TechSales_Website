import { unsupportedFeature } from './unsupportedFeature';

import { RepairRequest } from '../models/ui_types/repairRequest';

export const repairService = {
  async getRepairRequests(customerId?: string): Promise<RepairRequest[]> { return unsupportedFeature("sửa chữa"); },
  async createRepairRequest(input: { productId: string; serialNumber: string; customerId: string; customerName: string; problemDescription: string }): Promise<RepairRequest> { return unsupportedFeature("sửa chữa"); },
  async updateRepairRequest(id: string, update: Partial<Pick<RepairRequest, 'status' | 'processingLocation' | 'repairType' | 'checkingResult' | 'solutionDescription' | 'totalAmount'>>): Promise<RepairRequest> { return unsupportedFeature("sửa chữa"); },
};
