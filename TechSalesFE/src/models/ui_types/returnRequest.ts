export enum ReturnStatus {
  PENDING_APPROVAL = 'PENDING_APPROVAL',   // Khách vừa gửi yêu cầu, chờ nhân viên duyệt
  AWAITING_RETURN = 'AWAITING_RETURN',     // Nhân viên đã duyệt, yêu cầu khách gửi hàng về kho
  INSPECTING = 'INSPECTING',               // Hàng đã về tới kho, nhân viên đang tự kiểm tra
  COMPLETED = 'COMPLETED',                 // Hàng đạt chuẩn, đã hoàn tiền thành công
  REJECTED = 'REJECTED'                    // Từ chối (ở bước duyệt hoặc kiểm tra không đạt)
}

export interface InspectionResult {
  passed: boolean;
  notes: string;
  inspectedBy?: string;
  inspectedAt?: string;
}

export interface RefundInfo {
  amount: number;
  method: string;
  transactionRef?: string;
  refundedAt?: string;
  accountNumber?: string;
  accountHolder?: string;
  bankName?: string;
}

export interface ReturnRequest {
  id: string;                      // vd: 'RET-001'
  orderId: string;                 // vd: 'ORD-004'
  userId: string;
  customerName: string;
  customerPhone?: string;
  productId: string;
  productName: string;
  productImageUrl?: string;
  price: number;
  quantity: number;
  totalRefundAmount: number;
  reason: string;                  // Lý do chọn từ danh mục
  description: string;             // Mô tả chi tiết lỗi/vấn đề từ khách
  proofImages: string[];           // Danh sách ảnh chụp minh chứng
  status: ReturnStatus;
  
  // Thông tin gửi hàng hoàn về
  returnCarrier?: string;          // Đơn vị vận chuyển (GHTK, GHN, ViettelPost,...)
  returnTrackingCode?: string;     // Mã vận đơn gửi hàng về
  returnAddress?: string;          // Địa chỉ kho tiếp nhận hàng hoàn
  
  // Quá trình xử lý của nhân viên
  staffNote?: string;              // Phản hồi / hướng dẫn từ nhân viên
  rejectionReason?: string;        // Lý do từ chối nếu có
  approvedAt?: string;             // Thời gian duyệt yêu cầu
  receivedAt?: string;             // Thời gian hàng về tới kho
  
  // Xác thực ProductUnit
  serialNumber?: string;           // Số serial của thiết bị hoàn trả
  orderSerialNumbers?: string[];   // Toàn bộ số serial của sản phẩm trong đơn gốc để đối chiếu
  unitDecisions?: ReturnUnitDecision[];
  
  // Tự kiểm tra & Hoàn tiền
  inspection?: InspectionResult;
  refund?: RefundInfo;
  
  createdAt: string;
  updatedAt?: string;
}

export interface CreateReturnDto {
  orderId: string;
  productId: string;
  quantity: number;
  reason: string;
  description: string;
  proofImages: string[];
  serialNumber?: string;
  bankName?: string;
  accountNumber?: string;
  accountHolder?: string;
}

export interface ReturnUnitDecision {
  serialNumber: string;
  isApproved: boolean;
  reason?: string;
}

export interface ApproveReturnParams {
  commonReason?: string;
  units: ReturnUnitDecision[];
}
