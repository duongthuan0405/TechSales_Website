import { products, mockProductUnits } from '../data/mockData';
import { RepairRequest, RepairRequestStatus, RepairType, ProcessingLocation } from '../models/ui_types/repairRequest';

const delay = (ms = 50) => new Promise(resolve => setTimeout(resolve, ms));

const repairRequests: RepairRequest[] = [
  {
    id: 'REP-001', productId: 'p8', productName: 'Sony WH-1000XM5',
    productImageUrl: products.find(p => p.id === 'p8')?.imageUrl,
    serialNumber: 'SN-SONY-XM5-1049', customerId: 'u4', customerName: 'Khách hàng Mẫu (Nguyễn Văn An)',
    customerPhone: '+84 901 234 567', status: RepairRequestStatus.PENDING,
    problemDescription: 'Tai nghe bên trái không phát âm thanh, đã thử sạc và khởi động lại.',
    createdAt: new Date(Date.now() - 86400000).toISOString(), totalAmount: 0,
  },
];

const copy = (item: RepairRequest) => ({ ...item });

export const repairService = {
  async getRepairRequests(customerId?: string) {
    await delay();
    return repairRequests.filter(item => !customerId || item.customerId === customerId).map(copy);
  },
  async createRepairRequest(input: { productId: string; serialNumber: string; customerId: string; customerName: string; problemDescription: string }) {
    await delay();
    const serial = input.serialNumber.trim().toUpperCase();
    const unit = mockProductUnits.find(item => item.productId === input.productId && item.serialNumber.toUpperCase() === serial && item.status === 'SOLD');
    if (!unit) throw new Error('Serial không thuộc thiết bị đã bán bởi TechSales.');
    if (repairRequests.some(item => item.serialNumber === serial && ![RepairRequestStatus.COMPLETED, RepairRequestStatus.CANCELLED].includes(item.status))) {
      throw new Error('Thiết bị đang có một yêu cầu sửa chữa chưa hoàn tất.');
    }
    const product = products.find(item => item.id === input.productId);
    const request: RepairRequest = {
      id: `REP-${String(Math.floor(100 + Math.random() * 900))}`,
      productId: input.productId, productName: product?.name || unit.productName || 'Sản phẩm', productImageUrl: product?.imageUrl,
      serialNumber: serial, customerId: input.customerId, customerName: input.customerName,
      status: RepairRequestStatus.PENDING, problemDescription: input.problemDescription,
      createdAt: new Date().toISOString(), totalAmount: 0,
    };
    repairRequests.unshift(request);
    return copy(request);
  },
  async updateRepairRequest(id: string, update: Partial<Pick<RepairRequest, 'status' | 'processingLocation' | 'repairType' | 'checkingResult' | 'solutionDescription' | 'totalAmount'>>) {
    await delay();
    const request = repairRequests.find(item => item.id === id);
    if (!request) throw new Error('Không tìm thấy yêu cầu sửa chữa.');
    Object.assign(request, update);
    const now = new Date().toISOString();
    if (update.status === RepairRequestStatus.RECEIVED_CHECKING) request.receivedDeviceAt = now;
    if (update.status === RepairRequestStatus.IN_PROGRESS && update.processingLocation === 'AT_MANUFACTURER') request.transferredToManufactureAt = now;
    if (update.status === RepairRequestStatus.COMPLETED) request.completedAt = now;
    return copy(request);
  },
};
