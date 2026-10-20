export enum RepairRequestStatus {
  PENDING = 'PENDING',
  APPROVED = 'APPROVED',
  RECEIVED_CHECKING = 'RECEIVED_CHECKING',
  IN_PROGRESS = 'IN_PROGRESS',
  COMPLETED = 'COMPLETED',
  CANCELLED = 'CANCELLED',
}

export type RepairType = 'WARRANTY' | 'REPAIR';
export type ProcessingLocation = 'AT_STORE' | 'AT_MANUFACTURER';

export interface RepairRequest {
  id: string;
  productId: string;
  productName: string;
  productImageUrl?: string;
  serialNumber: string;
  customerId: string;
  customerName: string;
  customerPhone?: string;
  status: RepairRequestStatus;
  problemDescription: string;
  createdAt: string;
  receivedDeviceAt?: string;
  repairType?: RepairType;
  processingLocation?: ProcessingLocation;
  checkingResult?: string;
  solutionDescription?: string;
  totalAmount: number;
  transferredToManufactureAt?: string;
  returnedToStoreAt?: string;
  completedAt?: string;
}
