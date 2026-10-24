import { useState } from 'react';
import { formatCurrency } from '../../../utils/format';
import { 
  Dialog, 
  DialogContent, 
  DialogHeader, 
  DialogTitle, 
  DialogDescription,
  DialogFooter
} from '../ui/dialog';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Badge } from '../ui/badge';
import { 
  RotateCcw, 
  Package, 
  Truck, 
  Clock, 
  CheckCircle2, 
  XCircle, 
  MapPin, 
  CreditCard,
  Send,
  Loader2,
  FileSearch,
  ExternalLink
} from 'lucide-react';
import { ReturnRequest, ReturnStatus } from '../../../models/ui_types/returnRequest';
import { useCustomerSubmitReturnTracking } from '../../../dataHook/returnDataHook';
import { toast } from 'sonner';

interface ReturnDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  returnRequest: ReturnRequest | null;
}

export function ReturnDetailModal({
  isOpen,
  onClose,
  returnRequest
}: ReturnDetailModalProps) {
  const { mutate: submitTracking, isPending: isSubmittingTracking } = useCustomerSubmitReturnTracking();

  const [carrier, setCarrier] = useState('');
  const [trackingCode, setTrackingCode] = useState('');
  const [selectedImage, setSelectedImage] = useState<string | null>(null);

  if (!returnRequest) return null;

  const handleSendTracking = (e: React.FormEvent) => {
    e.preventDefault();
    if (!trackingCode.trim()) {
      toast.error('Vui lòng nhập mã vận đơn gửi hàng');
      return;
    }

    submitTracking(
      {
        id: returnRequest.id,
        trackingCode: trackingCode.trim(),
        carrier: carrier.trim() || 'Đơn vị vận chuyển'
      },
      {
        onSuccess: () => {
          toast.success('Cập nhật mã vận đơn thành công!');
          setTrackingCode('');
          setCarrier('');
        },
        onError: (err: any) => {
          toast.error(err.message || 'Cập nhật mã vận đơn thất bại');
        }
      }
    );
  };

  const getStatusStep = (status: ReturnStatus) => {
    switch (status) {
      case ReturnStatus.PENDING_APPROVAL: return 1;
      case ReturnStatus.AWAITING_RETURN: return 2;
      case ReturnStatus.INSPECTING: return 3;
      case ReturnStatus.COMPLETED: return 4;
      case ReturnStatus.REJECTED: return -1;
      default: return 0;
    }
  };

  const steps = [
    { label: 'GỬI YÊU CẦU', icon: Clock, desc: 'Chờ duyệt' },
    { label: 'DUYỆT YÊU CẦU', icon: Truck, desc: 'Gửi hàng về kho' },
    { label: 'KIỂM TRA HÀNG', icon: FileSearch, desc: 'Tự kiểm tra' },
    { label: 'HOÀN TIỀN', icon: CheckCircle2, desc: 'Đạt chuẩn' },
  ];

  const currentStep = getStatusStep(returnRequest.status);

  const getStatusBadge = (status: ReturnStatus) => {
    switch (status) {
      case ReturnStatus.PENDING_APPROVAL:
        return <Badge className="bg-amber-500/10 text-amber-500 border border-amber-500/20 font-bold uppercase tracking-wider text-[10px]">Chờ xét duyệt</Badge>;
      case ReturnStatus.AWAITING_RETURN:
        return <Badge className="bg-blue-500/10 text-blue-500 border border-blue-500/20 font-bold uppercase tracking-wider text-[10px]">Chờ gửi hàng về</Badge>;
      case ReturnStatus.INSPECTING:
        return <Badge className="bg-purple-500/10 text-purple-500 border border-purple-500/20 font-bold uppercase tracking-wider text-[10px]">Đang tự kiểm tra</Badge>;
      case ReturnStatus.COMPLETED:
        return <Badge className="bg-emerald-500/10 text-emerald-500 border border-emerald-500/20 font-bold uppercase tracking-wider text-[10px]">Hoàn tiền thành công</Badge>;
      case ReturnStatus.REJECTED:
        return <Badge className="bg-rose-500/10 text-rose-500 border border-rose-500/20 font-bold uppercase tracking-wider text-[10px]">Từ chối hoàn trả</Badge>;
      default:
        return null;
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto rounded-2xl p-6 bg-card border-border shadow-2xl">
        <DialogHeader className="space-y-1">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div className="p-2 rounded-xl bg-primary/10 text-primary">
                <RotateCcw className="h-5 w-5" />
              </div>
              <div>
                <DialogTitle className="text-xl font-bold tracking-tight uppercase">
                  Tiến Trình Hoàn Trả Hàng
                </DialogTitle>
                <DialogDescription className="text-xs text-muted-foreground">
                  Mã yêu cầu: <span className="font-bold text-foreground">#{returnRequest.id}</span> • Đơn hàng: <span className="font-bold text-foreground">#{returnRequest.orderId}</span>
                </DialogDescription>
              </div>
            </div>
            {getStatusBadge(returnRequest.status)}
          </div>
        </DialogHeader>

        {/* Progress Tracker (unless REJECTED) */}
        {returnRequest.status !== ReturnStatus.REJECTED ? (
          <div className="py-4">
            <div className="relative flex justify-between">
              {/* Background Line */}
              <div className="absolute top-5 left-0 h-1 w-full -translate-y-1/2 bg-muted rounded-full" />
              {/* Active Line */}
              <div 
                className="absolute top-5 left-0 h-1 -translate-y-1/2 bg-primary rounded-full transition-all duration-500" 
                style={{ width: `${Math.max(0, (currentStep - 1) / (steps.length - 1) * 100)}%` }}
              />

              {steps.map((step, idx) => {
                const isCompleted = currentStep > idx;
                const isCurrent = currentStep === idx + 1;
                const Icon = step.icon;

                return (
                  <div key={step.label} className="relative z-10 flex flex-col items-center gap-1.5">
                    <div className={`flex h-10 w-10 items-center justify-center rounded-full border-2 transition-all ${
                      isCompleted || isCurrent
                        ? 'bg-primary border-primary text-primary-foreground'
                        : 'bg-card border-border text-muted-foreground'
                    } ${isCurrent ? 'ring-4 ring-primary/20 scale-105' : ''}`}>
                      <Icon className="h-4 w-4" />
                    </div>
                    <span className={`text-[9px] font-bold uppercase tracking-wider text-center ${
                      isCompleted || isCurrent ? 'text-foreground' : 'text-muted-foreground'
                    }`}>
                      {step.label}
                    </span>
                    <span className="text-[8px] text-muted-foreground text-center hidden sm:block">
                      {step.desc}
                    </span>
                  </div>
                );
              })}
            </div>
          </div>
        ) : (
          <div className="p-4 rounded-xl border border-destructive/20 bg-destructive/5 flex items-start gap-3">
            <XCircle className="h-5 w-5 text-destructive shrink-0 mt-0.5" />
            <div>
              <h5 className="text-xs font-bold uppercase tracking-wider text-destructive">Yêu cầu hoàn trả đã bị từ chối</h5>
              <p className="text-xs text-foreground mt-1 leading-relaxed">
                {returnRequest.rejectionReason || 'Sản phẩm không đáp ứng đủ điều kiện chính sách đổi trả của TechSales.'}
              </p>
            </div>
          </div>
        )}

        {/* Product Details Section */}
        <div className="flex items-center gap-4 p-4 rounded-xl border border-border bg-muted/20">
          <img
            src={returnRequest.productImageUrl || ''}
            alt={returnRequest.productName}
            className="h-16 w-16 rounded-xl object-cover border border-border bg-card shrink-0"
          />
          <div className="flex-1 min-w-0">
            <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Sản phẩm</span>
            <h4 className="font-bold text-sm text-foreground truncate">{returnRequest.productName}</h4>
            <div className="flex items-center gap-3 mt-1 text-xs">
              <span className="text-muted-foreground">SL hoàn trả: <strong className="text-foreground">{returnRequest.quantity}</strong></span>
              <span className="text-muted-foreground">• Tiền hoàn: <strong className="text-primary font-bold">{formatCurrency(returnRequest.totalRefundAmount)}</strong></span>
            </div>
          </div>
        </div>

        {/* Return Reason & Proof Images */}
        <div className="space-y-3">
          <div className="p-4 rounded-xl border border-border bg-card space-y-2">
            <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block">
              Lý do từ khách hàng
            </span>
            <p className="text-xs font-bold text-foreground">{returnRequest.reason}</p>
            <p className="text-xs text-muted-foreground italic leading-relaxed">
              "{returnRequest.description}"
            </p>
          </div>

          {returnRequest.proofImages?.length > 0 && (
            <div className="space-y-1.5">
              <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block">
                Hình ảnh minh chứng ({returnRequest.proofImages.length})
              </span>
              <div className="flex flex-wrap gap-2">
                {returnRequest.proofImages.map((img, i) => (
                  <img
                    key={i}
                    src={img}
                    alt={`Proof ${i}`}
                    onClick={() => setSelectedImage(img)}
                    className="h-16 w-16 rounded-xl object-cover border border-border cursor-pointer hover:opacity-80 transition-opacity"
                    title="Bấm để xem ảnh phóng to"
                  />
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Action Phase 1: Return Instructions when AWAITING_RETURN */}
        {returnRequest.status === ReturnStatus.AWAITING_RETURN && (
          <div className="space-y-4 p-4 rounded-xl border border-blue-500/20 bg-blue-500/5">
            <div className="flex items-start gap-3">
              <MapPin className="h-5 w-5 text-blue-500 shrink-0 mt-0.5" />
              <div>
                <h5 className="text-xs font-bold uppercase tracking-wider text-blue-600 dark:text-blue-400">
                  Địa chỉ gửi hàng về kho
                </h5>
                <p className="text-xs text-foreground font-medium mt-1">
                  {returnRequest.returnAddress || 'Kho TechSales: 123 Tech Avenue, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh (Hotline: 1900-8888)'}
                </p>
                <p className="text-[10px] text-muted-foreground mt-1">
                  {returnRequest.staffNote || 'Vui lòng đóng gói sản phẩm cẩn thận kèm hộp và đầy đủ phụ kiện ban đầu.'}
                </p>
              </div>
            </div>

            {/* Tracking Code Form */}
            <form onSubmit={handleSendTracking} className="pt-2 border-t border-blue-500/10 space-y-3">
              <span className="text-[10px] font-bold uppercase tracking-wider text-foreground block">
                {returnRequest.returnTrackingCode ? 'Cập nhật mã vận đơn gửi hàng' : 'Nhập mã vận đơn sau khi quý khách gửi hàng'}
              </span>
              <div className="flex flex-col sm:flex-row gap-2">
                <Input
                  placeholder="Đơn vị vận chuyển (GHTK, GHN, ViettelPost...)"
                  value={carrier}
                  onChange={(e) => setCarrier(e.target.value)}
                  className="h-9 text-xs rounded-lg flex-1"
                />
                <Input
                  placeholder="Mã vận đơn (vd: GHN8829104VN)"
                  value={trackingCode}
                  onChange={(e) => setTrackingCode(e.target.value)}
                  className="h-9 text-xs rounded-lg flex-1"
                  required
                />
                <Button
                  type="submit"
                  disabled={isSubmittingTracking}
                  className="h-9 px-4 rounded-lg text-xs font-bold uppercase tracking-wider bg-blue-600 hover:bg-blue-700 text-white shrink-0"
                >
                  {isSubmittingTracking ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : <Send className="h-3.5 w-3.5 mr-1.5" />}
                  Lưu vận đơn
                </Button>
              </div>
              {returnRequest.returnTrackingCode && (
                <p className="text-[10px] text-emerald-600 font-medium">
                  ✓ Đã ghi nhận mã vận đơn: <strong>{returnRequest.returnTrackingCode}</strong> ({returnRequest.returnCarrier || 'Vận chuyển'})
                </p>
              )}
            </form>
          </div>
        )}

        {/* Action Phase 2: Inspection Status when INSPECTING */}
        {returnRequest.status === ReturnStatus.INSPECTING && (
          <div className="p-4 rounded-xl border border-purple-500/20 bg-purple-500/5 flex items-start gap-3">
            <FileSearch className="h-5 w-5 text-purple-500 shrink-0 mt-0.5 animate-pulse" />
            <div>
              <h5 className="text-xs font-bold uppercase tracking-wider text-purple-600 dark:text-purple-400">
                Hàng đã về tới kho - Đang tự kiểm tra chất lượng
              </h5>
              <p className="text-xs text-foreground mt-1">
                Kỹ thuật viên TechSales đang trực tiếp kiểm tra tình trạng ngoại quan, linh kiện và lỗi kỹ thuật của sản phẩm. Kết quả sẽ được cập nhật sớm nhất.
              </p>
              {returnRequest.staffNote && (
                <p className="text-[10px] text-muted-foreground mt-1.5 italic">
                  Ghi chú từ kho: "{returnRequest.staffNote}"
                </p>
              )}
            </div>
          </div>
        )}

        {/* Action Phase 3: Completed Refund Info */}
        {returnRequest.status === ReturnStatus.COMPLETED && (
          <div className="p-4 rounded-xl border border-emerald-500/20 bg-emerald-500/5 space-y-3">
            <div className="flex items-start gap-3">
              <CheckCircle2 className="h-5 w-5 text-emerald-500 shrink-0 mt-0.5" />
              <div>
                <h5 className="text-xs font-bold uppercase tracking-wider text-emerald-600 dark:text-emerald-400">
                  Hàng đạt chuẩn - Đã hoàn tiền thành công!
                </h5>
                <p className="text-xs text-foreground mt-1">
                  Sản phẩm đã vượt qua kiểm tra chất lượng tại kho và đáp ứng đầy đủ tiêu chuẩn hoàn tiền.
                </p>
              </div>
            </div>

            {returnRequest.inspection && (
              <div className="text-[11px] p-3 rounded-lg bg-card/60 border border-border space-y-1">
                <span className="text-[9px] font-bold uppercase tracking-wider text-muted-foreground block">
                  Biên bản kiểm tra kỹ thuật:
                </span>
                <p className="text-foreground italic">"{returnRequest.inspection.notes}"</p>
                <p className="text-[9px] text-muted-foreground">Người kiểm tra: {returnRequest.inspection.inspectedBy}</p>
              </div>
            )}

            {returnRequest.refund && (
              <div className="flex flex-wrap items-center justify-between gap-2 pt-2 border-t border-emerald-500/20 text-xs">
                <div>
                  <span className="text-muted-foreground text-[10px] block">Mã giao dịch hoàn tiền</span>
                  <strong className="text-foreground font-mono">{returnRequest.refund.transactionRef || 'REF-TX-000000'}</strong>
                </div>
                <div>
                  <span className="text-muted-foreground text-[10px] block">Phương thức nhận</span>
                  <strong className="text-foreground">{returnRequest.refund.method}</strong>
                </div>
                <div className="text-right">
                  <span className="text-muted-foreground text-[10px] block">Số tiền hoàn</span>
                  <strong className="text-emerald-600 font-bold text-base">{formatCurrency(returnRequest.refund.amount || 0)}</strong>
                </div>
              </div>
            )}
          </div>
        )}

        <DialogFooter className="pt-2">
          <Button
            type="button"
            variant="outline"
            onClick={onClose}
            className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider border-border w-full sm:w-auto"
          >
            Đóng
          </Button>
        </DialogFooter>

        {/* Lightbox for Zooming Images */}
        {selectedImage && (
          <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 p-4"
            onClick={() => setSelectedImage(null)}
          >
            <div className="relative max-w-3xl max-h-[90vh]">
              <img
                src={selectedImage}
                alt="Enlarged proof"
                className="max-h-[85vh] max-w-full rounded-2xl object-contain shadow-2xl"
              />
              <Button
                size="sm"
                variant="secondary"
                className="absolute top-3 right-3 rounded-full h-8 w-8 p-0"
                onClick={() => setSelectedImage(null)}
              >
                ✕
              </Button>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
