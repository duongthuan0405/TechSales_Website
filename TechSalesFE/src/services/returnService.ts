import { ReturnRequest, ReturnStatus, CreateReturnDto, ApproveReturnParams } from '../models/ui_types/returnRequest';
import { OrderStatus } from '../models/ui_types/order';
import { returnRequests, orders, products } from '../data/mockData';

const delay = (ms = 50) => new Promise(resolve => setTimeout(resolve, ms));

export const returnService = {
  // ── Lấy danh sách yêu cầu hoàn trả ────────────────────────
  getReturnRequests: async (userId?: string): Promise<ReturnRequest[]> => {
    await delay();
    if (userId) {
      return returnRequests.filter(r => r.userId === userId);
    }
    return [...returnRequests];
  },

  // ── Lấy chi tiết 1 yêu cầu ────────────────────────────────
  getReturnById: async (id: string): Promise<ReturnRequest> => {
    await delay();
    const ret = returnRequests.find(r => r.id === id);
    if (!ret) {
      throw new Error(`Return request ${id} not found`);
    }
    return { ...ret };
  },

  // ── Lấy danh sách hoàn trả theo đơn hàng ──────────────────
  getReturnsByOrderId: async (orderId: string): Promise<ReturnRequest[]> => {
    await delay();
    const order = orders.find(o => o.id === orderId);
    if (!order || order.status !== OrderStatus.DELIVERED) {
      return [];
    }
    return returnRequests.filter(r => r.orderId === orderId);
  },

  // ── Khách hàng tạo yêu cầu hoàn trả 1 món hàng ────────────
  createReturnRequest: async (dto: CreateReturnDto): Promise<ReturnRequest> => {
    await delay();
    const order = orders.find(o => o.id === dto.orderId);
    if (!order) {
      throw new Error(`Không tìm thấy đơn hàng #${dto.orderId}`);
    }

    if (order.status !== OrderStatus.DELIVERED) {
      throw new Error('Chỉ những đơn hàng ở trạng thái đã giao thành công mới được phép yêu cầu hoàn trả hàng.');
    }

    const item = order.items?.find(i => i.productId === dto.productId);
    if (!item) {
      throw new Error(`Không tìm thấy sản phẩm trong đơn hàng`);
    }

    const returnId = `RET-${Math.floor(100 + Math.random() * 900)}`;
    const totalRefundAmount = item.price * dto.quantity;
    const expectedSerials = (item.serialNumbers || []).map(serial => serial.trim().toUpperCase());
    const serialIsValid = !!dto.serialNumber && expectedSerials.includes(dto.serialNumber.trim().toUpperCase());
    const invalidSerialReason = 'Nhập sai serial number hoặc thiết bị này chưa được bán';

    // Lấy thông tin user hiện tại
    let customerName = order.customerName || 'Demo Customer';
    let customerPhone = '+84 901 234 567';
    let userId = order.userId || 'u4';
    try {
      const rawUser = localStorage.getItem('tech_sales_user');
      if (rawUser) {
        const parsed = JSON.parse(rawUser);
        if (parsed.name) customerName = parsed.name;
        if (parsed.phone) customerPhone = parsed.phone;
        if (parsed.id) userId = parsed.id;
      }
    } catch {}

    const newReturn: ReturnRequest = {
      id: returnId,
      orderId: dto.orderId,
      userId,
      customerName,
      customerPhone,
      productId: dto.productId,
      productName: item.productName || 'Product',
      productImageUrl: item.imageUrl,
      price: item.price,
      quantity: dto.quantity,
      totalRefundAmount: serialIsValid ? totalRefundAmount : 0,
      reason: serialIsValid ? dto.reason : invalidSerialReason,
      description: dto.description,
      proofImages: dto.proofImages || [],
      serialNumber: dto.serialNumber,
      orderSerialNumbers: item.serialNumbers || [],
      status: serialIsValid ? ReturnStatus.PENDING_APPROVAL : ReturnStatus.REJECTED,
      rejectionReason: serialIsValid ? undefined : invalidSerialReason,
      refund: {
        amount: totalRefundAmount,
        method: dto.bankName ? `Ngân hàng (${dto.bankName})` : 'Chuyển khoản Ngân hàng',
        bankName: dto.bankName,
        accountNumber: dto.accountNumber,
        accountHolder: dto.accountHolder,
      },
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    returnRequests.unshift(newReturn);
    return newReturn;
  },

  // ── Bước 1 (Nhân viên): Duyệt yêu cầu & yêu cầu gửi hàng về
  approveReturnRequest: async (id: string, params: ApproveReturnParams): Promise<ReturnRequest> => {
    await delay();
    const ret = returnRequests.find(r => r.id === id);
    if (!ret) {
      throw new Error(`Không tìm thấy yêu cầu hoàn trả ${id}`);
    }

    if (!params.units?.length) {
      ret.status = ReturnStatus.AWAITING_RETURN;
      ret.staffNote = params.commonReason || 'Đã chấp nhận yêu cầu. Vui lòng chờ khách gửi máy về kho để kiểm tra serial.';
      ret.returnAddress = 'Kho TechSales: 123 Tech Avenue, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh (Hotline: 1900-8888)';
      ret.updatedAt = new Date().toISOString();
      return { ...ret };
    }
    const expected = (ret.orderSerialNumbers || []).map(serial => serial.trim().toUpperCase());
    if (params.units.length !== ret.quantity || params.units.some(unit => !unit.serialNumber.trim())) {
      throw new Error('Vui lòng nhập và xử lý serial cho tất cả thiết bị.');
    }
    const seen = new Set<string>();
    for (const unit of params.units) {
      const serial = unit.serialNumber.trim().toUpperCase();
      if (seen.has(serial)) throw new Error(`Serial bị trùng: ${unit.serialNumber}`);
      seen.add(serial);
      if (!expected.includes(serial)) throw new Error(`Serial ${unit.serialNumber} không thuộc đơn hàng.`);
      if (!unit.isApproved && !unit.reason?.trim()) throw new Error(`Vui lòng nhập lý do từ chối cho serial ${unit.serialNumber}.`);
    }
    const approvedUnits = params.units.filter(unit => unit.isApproved);
    ret.unitDecisions = params.units;
    ret.serialNumber = approvedUnits[0]?.serialNumber;
    ret.status = approvedUnits.length ? ReturnStatus.INSPECTING : ReturnStatus.REJECTED;
    ret.approvedAt = new Date().toISOString();
    ret.staffNote = params.commonReason || 'Yêu cầu hoàn trả đã được duyệt. Quý khách vui lòng gửi sản phẩm cùng đầy đủ phụ kiện về kho TechSales.';
    if (!approvedUnits.length) ret.rejectionReason = params.commonReason || params.units.map(unit => unit.reason).filter(Boolean).join('; ');
    ret.returnAddress = 'Kho TechSales: 123 Tech Avenue, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh (Hotline: 1900-8888)';
    ret.updatedAt = new Date().toISOString();

    return { ...ret };
  },

  // ── Từ chối yêu cầu hoàn trả ──────────────────────────────
  rejectReturnRequest: async (id: string, reason: string): Promise<ReturnRequest> => {
    await delay();
    const ret = returnRequests.find(r => r.id === id);
    if (!ret) {
      throw new Error(`Không tìm thấy yêu cầu hoàn trả ${id}`);
    }

    ret.status = ReturnStatus.REJECTED;
    ret.rejectionReason = reason;
    ret.updatedAt = new Date().toISOString();

    return { ...ret };
  },

  // ── Khách hàng cập nhật mã vận đơn khi gửi hàng về ────────
  customerSubmitReturnTracking: async (
    id: string,
    trackingCode: string,
    carrier?: string
  ): Promise<ReturnRequest> => {
    await delay();
    const ret = returnRequests.find(r => r.id === id);
    if (!ret) {
      throw new Error(`Không tìm thấy yêu cầu hoàn trả ${id}`);
    }

    ret.returnTrackingCode = trackingCode;
    ret.returnCarrier = carrier || 'Đơn vị vận chuyển';
    ret.updatedAt = new Date().toISOString();

    return { ...ret };
  },

  // ── Bước 2 (Nhân viên): Hàng về tới kho, xác nhận bắt đầu tự kiểm tra
  confirmItemReceived: async (id: string, staffNote?: string): Promise<ReturnRequest> => {
    await delay();
    const ret = returnRequests.find(r => r.id === id);
    if (!ret) {
      throw new Error(`Không tìm thấy yêu cầu hoàn trả ${id}`);
    }

    ret.status = ReturnStatus.INSPECTING;
    ret.receivedAt = new Date().toISOString();
    ret.staffNote = staffNote || 'Hàng đã về tới kho TechSales. Nhân viên đang tự kiểm tra sản phẩm.';
    ret.updatedAt = new Date().toISOString();

    return { ...ret };
  },

  // ── Bước 3 & 4 (Nhân viên): Tự kiểm tra, duyệt hàng đạt chuẩn & hoàn tiền
  submitInspectionAndRefund: async (
    id: string,
    params: {
      passed: boolean;
      notes: string;
      inspectedBy?: string;
      refundMethod?: string;
      units?: { serialNumber: string; isApproved: boolean; reason?: string }[];
    }
  ): Promise<ReturnRequest> => {
    await delay();
    const ret = returnRequests.find(r => r.id === id);
    if (!ret) {
      throw new Error(`Không tìm thấy yêu cầu hoàn trả ${id}`);
    }

    const now = new Date().toISOString();
    const units = params.units || [];
    const expected = (ret.orderSerialNumbers || []).map(serial => serial.trim().toUpperCase());
    if (units.length < 1 || units.length > ret.quantity || units.some(unit => !unit.serialNumber.trim())) throw new Error(`Số unit thực nhận phải từ 1 đến ${ret.quantity}.`);
    const seen = new Set<string>();
    for (const unit of units) {
      const serial = unit.serialNumber.trim().toUpperCase();
      if (seen.has(serial) || !expected.includes(serial)) throw new Error(`Serial ${unit.serialNumber} bị trùng hoặc không thuộc đơn hàng.`);
      if (!unit.isApproved && !unit.reason?.trim()) throw new Error(`Vui lòng nhập lý do riêng cho serial ${unit.serialNumber}.`);
      seen.add(serial);
    }
    ret.unitDecisions = units;
    const approvedCount = units.filter(unit => unit.isApproved).length;
    ret.totalRefundAmount = ret.price * approvedCount;
    ret.inspection = {
      passed: params.passed,
      notes: params.notes,
      inspectedBy: params.inspectedBy || 'KTV TechSales',
      inspectedAt: now,
    };

    if (params.passed && approvedCount > 0) {
      // Hàng đạt chuẩn -> Duyệt hoàn tiền
      ret.status = ReturnStatus.COMPLETED;
      const txRef = `REF-TX-${Math.floor(100000 + Math.random() * 900000)}`;
      ret.refund = {
        amount: ret.totalRefundAmount,
        method: params.refundMethod || ret.refund?.method || 'Chuyển khoản Ngân hàng',
        transactionRef: txRef,
        refundedAt: now,
        bankName: ret.refund?.bankName,
        accountNumber: ret.refund?.accountNumber,
        accountHolder: ret.refund?.accountHolder,
      };
      ret.staffNote = `Sản phẩm đạt chuẩn hoàn tiền. Đã hoàn tiền thành công (${txRef}).`;

      // Cập nhật thông tin thanh toán của đơn hàng gốc (nếu có)
      const order = orders.find(o => o.id === ret.orderId);
      if (order && order.payments) {
        order.payments.push({
          id: `pay-ref-${Date.now()}`,
          paymentMethodName: ret.refund.method,
          status: 'SUCCESS',
          amount: -ret.totalRefundAmount,
          transactionRef: txRef,
        });
      }
    } else {
      // Hàng không đạt chuẩn -> Từ chối hoàn tiền
      ret.status = ReturnStatus.REJECTED;
      ret.rejectionReason = `Kiểm tra không đạt chuẩn: ${params.notes}`;
      ret.staffNote = `Sản phẩm không đạt chuẩn hoàn tiền: ${params.notes}`;
    }

    ret.updatedAt = now;
    return { ...ret };
  },
};
