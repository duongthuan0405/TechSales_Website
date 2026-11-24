import { unsupportedFeature } from './unsupportedFeature';
import { ReturnRequest, CreateReturnDto, ApproveReturnParams } from '../models/ui_types/returnRequest';


export const returnService = {
  // ── Lấy danh sách yêu cầu hoàn trả ────────────────────────
  getReturnRequests: async (userId?: string): Promise<ReturnRequest[]> => { return unsupportedFeature("hoàn trả"); },

  // ── Lấy chi tiết 1 yêu cầu ────────────────────────────────
  getReturnById: async (id: string): Promise<ReturnRequest> => { return unsupportedFeature("hoàn trả"); },

  // ── Lấy danh sách hoàn trả theo đơn hàng ──────────────────
  getReturnsByOrderId: async (orderId: string): Promise<ReturnRequest[]> => { return unsupportedFeature("hoàn trả"); },

  // ── Khách hàng tạo yêu cầu hoàn trả 1 món hàng ────────────
  createReturnRequest: async (dto: CreateReturnDto): Promise<ReturnRequest> => { return unsupportedFeature("hoàn trả"); },

  // ── Bước 1 (Nhân viên): Duyệt yêu cầu & yêu cầu gửi hàng về
  approveReturnRequest: async (id: string, params: ApproveReturnParams): Promise<ReturnRequest> => { return unsupportedFeature("hoàn trả"); },

  // ── Từ chối yêu cầu hoàn trả ──────────────────────────────
  rejectReturnRequest: async (id: string, reason: string): Promise<ReturnRequest> => { return unsupportedFeature("hoàn trả"); },

  // ── Khách hàng cập nhật mã vận đơn khi gửi hàng về ────────
  customerSubmitReturnTracking: async (
    id: string,
    trackingCode: string,
    carrier?: string
  ): Promise<ReturnRequest> => { return unsupportedFeature("hoàn trả"); },

  // ── Bước 2 (Nhân viên): Hàng về tới kho, xác nhận bắt đầu tự kiểm tra
  confirmItemReceived: async (id: string, staffNote?: string): Promise<ReturnRequest> => { return unsupportedFeature("hoàn trả"); },

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
  ): Promise<ReturnRequest> => { return unsupportedFeature("hoàn trả"); },
};
