import { Order } from '../../../models/ui_types/order';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '../ui/dialog';
interface OrderSerialApprovalModalProps {
  isOpen: boolean;
  onClose: () => void;
  order: Order | null;
  onSuccess?: () => void;
}
export function OrderSerialApprovalModal({ isOpen, onClose }: OrderSerialApprovalModalProps) {
  return <Dialog open={isOpen} onOpenChange={open => { if (!open) onClose(); }}>
    <DialogContent><DialogHeader>
      <DialogTitle>Duyệt đơn hàng</DialogTitle>
      <DialogDescription>Chức năng duyệt đơn hàng chưa có API hỗ trợ.</DialogDescription>
    </DialogHeader></DialogContent>
  </Dialog>;
}
