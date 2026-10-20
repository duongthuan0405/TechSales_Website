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
import { Textarea } from '../ui/textarea';
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from '../ui/select';
import { 
  RotateCcw, 
  Upload, 
  X, 
  AlertCircle, 
  Image as ImageIcon, 
  Camera, 
  Loader2,
  Sparkles,
  CheckCircle2
} from 'lucide-react';
import { toast } from 'sonner';
import { useCreateReturnRequest } from '../../../dataHook/returnDataHook';
import { OrderStatus } from '../../../models/ui_types/order';

interface CreateReturnModalProps {
  isOpen: boolean;
  onClose: () => void;
  orderId: string;
  orderStatus?: OrderStatus;
  item: {
    productId: string;
    productName: string;
    imageUrl?: string;
    price: number;
    quantity: number;
    serialNumbers?: string[];
  } | null;
}

const COMMON_REASONS = [
  'Sản phẩm bị lỗi kỹ thuật / không hoạt động',
  'Hàng bị bể vỡ / trầy xước do vận chuyển',
  'Giao sai màu sắc, mẫu mã hoặc kích thước',
  'Sản phẩm thiếu phụ kiện / quà tặng kèm',
  'Sản phẩm khác biệt so với mô tả',
  'Lý do khác (chi tiết bên dưới)'
];

const SAMPLE_DEFECT_IMAGES = [
  { label: 'Ảnh mẫu: Lỗi màn hình sọc', url: 'https://images.unsplash.com/photo-1545127398-14699f92334b?w=500' },
  { label: 'Ảnh mẫu: Nứt vỡ vỏ máy', url: 'https://images.unsplash.com/photo-1592286927505-c0d1981d9f09?w=500' },
  { label: 'Ảnh mẫu: Phụ kiện hỏng', url: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500' }
];

export function CreateReturnModal({
  isOpen,
  onClose,
  orderId,
  orderStatus,
  item
}: CreateReturnModalProps) {
  const { mutate: createReturn, isPending } = useCreateReturnRequest();

  const [quantity, setQuantity] = useState<number>(1);
  const [reason, setReason] = useState<string>(COMMON_REASONS[0]);
  const [description, setDescription] = useState<string>('');
  const [proofImages, setProofImages] = useState<string[]>([]);
  const [bankName, setBankName] = useState<string>('Vietcombank');
  const [accountNumber, setAccountNumber] = useState<string>('');
  const [accountHolder, setAccountHolder] = useState<string>('');
  const [serialNumber, setSerialNumber] = useState('');
  const [serialChecked, setSerialChecked] = useState(false);
  const [serialValid, setSerialValid] = useState(false);

  const verifySerial = () => {
    const value = serialNumber.trim().toUpperCase();
    if (!value) { toast.error('Vui lòng nhập serial của thiết bị cần hoàn trả'); return; }
    const valid = (item.serialNumbers || []).some(serial => serial.trim().toUpperCase() === value);
    setSerialValid(valid);
    setSerialChecked(true);
    if (valid) toast.success('Đã xác nhận thiết bị thuộc đơn hàng của TechSales');
    else toast.error('Không tìm thấy serial trong lịch sử bán hàng của TechSales');
  };

  if (!item) return null;

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files) return;

    Array.from(files).forEach((file) => {
      if (!file.type.startsWith('image/')) {
        toast.error('Vui lòng chỉ chọn tệp hình ảnh');
        return;
      }
      const reader = new FileReader();
      reader.onload = (uploadEvent) => {
        const result = uploadEvent.target?.result as string;
        if (result) {
          setProofImages((prev) => [...prev, result]);
        }
      };
      reader.readAsDataURL(file);
    });
    e.target.value = '';
  };

  const handleAddSampleImage = (url: string) => {
    if (proofImages.includes(url)) {
      toast.info('Ảnh này đã được thêm vào');
      return;
    }
    setProofImages((prev) => [...prev, url]);
  };

  const handleRemoveImage = (indexToRemove: number) => {
    setProofImages((prev) => prev.filter((_, idx) => idx !== indexToRemove));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (orderStatus && orderStatus !== OrderStatus.DELIVERED) {
      toast.error('Chỉ những đơn hàng ở trạng thái đã giao thành công mới được yêu cầu hoàn trả hàng.');
      return;
    }

    if (!serialChecked) {
      toast.error('Vui lòng xác thực serial trước khi tiếp tục');
      return;
    }

    const normalizedSerial = serialNumber.trim().toUpperCase();
    if (!serialValid) {
      createReturn({ orderId, productId: item.productId, quantity: 1, serialNumber: normalizedSerial, reason: 'Nhập sai serial number hoặc thiết bị này chưa được bán', description: 'Serial không được tìm thấy trong lịch sử bán hàng của TechSales.', proofImages: [] }, {
        onSuccess: () => { toast.info('Đã ghi nhận serial không hợp lệ để cửa hàng kiểm tra'); onClose(); },
        onError: (err: any) => toast.error(err.message || 'Không thể ghi nhận serial')
      });
      return;
    }

    if (!description.trim()) {
      toast.error('Vui lòng mô tả cụ thể tình trạng lỗi sản phẩm');
      return;
    }

    if (proofImages.length === 0) {
      toast.error('Vui lòng cung cấp ít nhất 1 ảnh chụp minh chứng sản phẩm lỗi');
      return;
    }

    createReturn(
      {
        orderId,
        productId: item.productId,
        quantity,
        reason,
        description,
        proofImages,
        serialNumber: normalizedSerial,
        bankName,
        accountNumber,
        accountHolder
      },
      {
        onSuccess: () => {
          toast.success('Gửi yêu cầu hoàn trả thành công! Đang chờ nhân viên xét duyệt.');
          // Reset form
          setDescription('');
          setProofImages([]);
          setAccountNumber('');
          setAccountHolder('');
          onClose();
        },
        onError: (err: any) => {
          toast.error(err.message || 'Gửi yêu cầu hoàn trả thất bại');
        }
      }
    );
  };

  const totalRefund = item.price * quantity;

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && !isPending && onClose()}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto rounded-2xl p-6 bg-card border-border shadow-2xl">
        <DialogHeader className="space-y-1">
          <div className="flex items-center gap-2">
            <div className="p-2 rounded-xl bg-primary/10 text-primary">
              <RotateCcw className="h-5 w-5" />
            </div>
            <DialogTitle className="text-xl font-bold tracking-tight uppercase">
              Yêu Cầu Hoàn Trả Hàng
            </DialogTitle>
          </div>
          <DialogDescription className="text-xs text-muted-foreground">
            Đơn hàng #{orderId} • Chọn sản phẩm cần đổi trả, cung cấp lý do và ảnh minh chứng.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-6 pt-2">
          <div className="space-y-3 rounded-xl border border-primary/20 bg-primary/5 p-4">
            <div>
              <label className="text-xs font-bold uppercase tracking-wider text-foreground">Bước 1: Nhập serial thiết bị</label>
              <p className="mt-1 text-[11px] text-muted-foreground">Kiểm tra thiết bị có nằm trong lịch sử bán hàng của TechSales trước khi nhập lý do.</p>
            </div>
            <div className="flex gap-2">
              <Input value={serialNumber} onChange={(e) => { setSerialNumber(e.target.value); setSerialChecked(false); }} placeholder="Ví dụ: SN-ABC-123" className="h-10 rounded-lg font-mono text-xs" />
              <Button type="button" onClick={verifySerial} className="h-10 rounded-lg">Kiểm tra</Button>
            </div>
            {serialChecked && <div className={`rounded-lg border p-3 text-xs ${serialValid ? 'border-emerald-500/30 bg-emerald-500/10 text-emerald-700' : 'border-amber-500/30 bg-amber-500/10 text-amber-700'}`}>
              {serialValid ? 'Serial hợp lệ. Tiếp tục nhập lý do và quyết định hoàn trả.' : 'Serial không hợp lệ. Lý do readonly: Nhập sai serial number hoặc thiết bị này chưa được bán.'}
            </div>}
          </div>
          {serialChecked && serialValid && <>
          {/* Selected Item Card */}
          <div className="flex items-center gap-4 p-4 rounded-xl border border-border bg-muted/30">
            <img
              src={item.imageUrl || ''}
              alt={item.productName}
              className="h-16 w-16 rounded-xl object-cover border border-border bg-card shrink-0"
            />
            <div className="flex-1 min-w-0">
              <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Sản phẩm hoàn trả</p>
              <h4 className="font-bold text-sm text-foreground truncate">{item.productName}</h4>
              <div className="flex items-center gap-3 mt-1 text-xs">
                <span className="font-bold text-primary">{formatCurrency(item.price)}</span>
                <span className="text-muted-foreground">• Đã mua: {item.quantity} sản phẩm</span>
              </div>
            </div>
            <div className="w-24 shrink-0">
              <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-1">
                SL Trả
              </label>
              <Select
                value={quantity.toString()}
                onValueChange={(val) => setQuantity(Number(val))}
              >
                <SelectTrigger className="h-9 rounded-lg">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {Array.from({ length: item.quantity }, (_, i) => i + 1).map((q) => (
                    <SelectItem key={q} value={q.toString()}>
                      {q}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Reason Selection */}
          <div className="space-y-2">
            <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
              Lý do hoàn trả <span className="text-destructive">*</span>
            </label>
            <Select value={reason} onValueChange={setReason}>
              <SelectTrigger className="h-11 rounded-xl">
                <SelectValue placeholder="Chọn lý do hoàn trả" />
              </SelectTrigger>
              <SelectContent>
                {COMMON_REASONS.map((r) => (
                  <SelectItem key={r} value={r}>
                    {r}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Detailed Description */}
          <div className="space-y-2">
            <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
              Mô tả chi tiết tình trạng lỗi <span className="text-destructive">*</span>
            </label>
            <Textarea
              placeholder="Vui lòng mô tả cụ thể tình trạng sản phẩm khi nhận, lỗi phát sinh, dấu hiệu bất thường..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
              className="rounded-xl border-border resize-none text-xs"
              required
            />
          </div>

          {/* Proof Images Upload */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground flex items-center gap-1.5">
                <Camera className="h-3.5 w-3.5 text-primary" />
                Ảnh chụp minh chứng <span className="text-destructive">*</span>
              </label>
              <span className="text-[10px] text-muted-foreground font-medium">
                {proofImages.length} ảnh đã chọn (tối thiểu 1 ảnh)
              </span>
            </div>

            {/* Upload Box & Image Previews */}
            <div className="grid grid-cols-3 sm:grid-cols-4 gap-3">
              {proofImages.map((img, idx) => (
                <div key={idx} className="relative group aspect-square rounded-xl overflow-hidden border border-border bg-card">
                  <img src={img} alt={`Proof ${idx}`} className="w-full h-full object-cover" />
                  <button
                    type="button"
                    onClick={() => handleRemoveImage(idx)}
                    className="absolute top-1.5 right-1.5 p-1 rounded-full bg-black/70 text-white hover:bg-destructive transition-colors shadow-sm"
                    title="Xóa ảnh này"
                  >
                    <X className="h-3 w-3" />
                  </button>
                </div>
              ))}

              <label className="flex flex-col items-center justify-center aspect-square rounded-xl border-2 border-dashed border-border hover:border-primary/50 hover:bg-primary/5 cursor-pointer transition-all p-3 text-center">
                <Upload className="h-5 w-5 text-muted-foreground group-hover:text-primary mb-1" />
                <span className="text-[10px] font-bold text-muted-foreground uppercase tracking-tight">Tải ảnh lên</span>
                <span className="text-[8px] text-muted-foreground opacity-60">PNG, JPG, WebP</span>
                <input
                  type="file"
                  accept="image/*"
                  multiple
                  onChange={handleFileUpload}
                  className="hidden"
                />
              </label>
            </div>

            {/* Fast sample photos selector */}
            <div className="pt-1">
              <div className="flex items-center gap-1 text-[10px] font-bold uppercase tracking-widest text-muted-foreground mb-1.5">
                <Sparkles className="h-3 w-3 text-amber-500" />
                <span>Hoặc chọn nhanh ảnh mẫu thử nghiệm:</span>
              </div>
              <div className="flex flex-wrap gap-2">
                {SAMPLE_DEFECT_IMAGES.map((sample, i) => (
                  <button
                    key={i}
                    type="button"
                    onClick={() => handleAddSampleImage(sample.url)}
                    className="text-[10px] px-2.5 py-1 rounded-lg border border-border bg-card hover:bg-muted font-medium transition-colors text-muted-foreground hover:text-foreground flex items-center gap-1.5"
                  >
                    <ImageIcon className="h-3 w-3" />
                    {sample.label}
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Refund Bank Info */}
          <div className="p-4 rounded-xl border border-border bg-card space-y-3">
            <h5 className="text-xs font-bold uppercase tracking-wider text-foreground">
              Thông tin nhận hoàn tiền
            </h5>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
              <div>
                <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-1">
                  Ngân hàng / Ví
                </label>
                <Select value={bankName} onValueChange={setBankName}>
                  <SelectTrigger className="h-9 rounded-lg text-xs">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Vietcombank">Vietcombank</SelectItem>
                    <SelectItem value="Techcombank">Techcombank</SelectItem>
                    <SelectItem value="MB Bank">MB Bank</SelectItem>
                    <SelectItem value="ACB">ACB</SelectItem>
                    <SelectItem value="VPBank">VPBank</SelectItem>
                    <SelectItem value="Momo">Ví MoMo</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-1">
                  Số tài khoản
                </label>
                <Input
                  placeholder="0123456789"
                  value={accountNumber}
                  onChange={(e) => setAccountNumber(e.target.value)}
                  className="h-9 rounded-lg text-xs"
                />
              </div>
              <div>
                <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-1">
                  Chủ tài khoản
                </label>
                <Input
                  placeholder="NGUYEN VAN A"
                  value={accountHolder}
                  onChange={(e) => setAccountHolder(e.target.value.toUpperCase())}
                  className="h-9 rounded-lg text-xs"
                />
              </div>
            </div>
          </div>

          {/* Refund Valuation Banner */}
          <div className="flex items-center justify-between p-4 rounded-xl bg-primary/5 border border-primary/20">
            <div>
              <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block">
                Tổng tiền dự kiến hoàn lại
              </span>
              <span className="text-2xl font-black text-primary tracking-tight">
                ${totalRefund.toLocaleString()}
              </span>
            </div>
            <div className="text-right text-[10px] text-muted-foreground space-y-0.5 max-w-[200px]">
              <div className="flex items-center justify-end gap-1 text-emerald-600 font-bold">
                <CheckCircle2 className="h-3 w-3" /> Miễn phí đổi trả
              </div>
              <p>Hoàn tiền 100% giá trị sản phẩm sau khi nhân viên kiểm tra đạt chuẩn.</p>
            </div>
          </div>
          </>}

          <DialogFooter className="gap-2 sm:gap-0 pt-2">
            <Button
              type="button"
              variant="outline"
              onClick={onClose}
              disabled={isPending}
              className="h-11 rounded-xl text-xs font-bold uppercase tracking-widest border-border"
            >
              Hủy bỏ
            </Button>
            <Button
              type="submit"
              disabled={isPending || !serialChecked}
              className="h-11 rounded-xl text-xs font-bold uppercase tracking-widest bg-primary text-primary-foreground hover:bg-primary/90 flex items-center gap-2"
            >
              {isPending ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Đang gửi yêu cầu...
                </>
              ) : (
                <>
                  <RotateCcw className="h-4 w-4" />
                  Xác nhận gửi yêu cầu
                </>
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
