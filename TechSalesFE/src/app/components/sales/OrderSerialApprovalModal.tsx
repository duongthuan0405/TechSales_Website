import { useState, useEffect, useMemo } from 'react';
import { 
  CheckCircle2, 
  AlertTriangle, 
  Package, 
  PackageSearch,
  ClipboardPaste, 
  Hash, 
  Cpu, 
  Loader2, 
  X, 
  Barcode, 
  Check, 
  AlertCircle 
} from 'lucide-react';
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
import { Textarea } from '../ui/textarea';
import { toast } from 'sonner';
import { Order, OrderStatus } from '../../../models/ui_types/order';
import { useUpdateOrderStatus } from '../../../dataHook/orderDataHook';
import { mockProductUnits } from '../../../data/mockData';

interface OrderSerialApprovalModalProps {
  isOpen: boolean;
  onClose: () => void;
  order: Order | null;
  onSuccess?: () => void;
}

export function OrderSerialApprovalModal({
  isOpen,
  onClose,
  order,
  onSuccess
}: OrderSerialApprovalModalProps) {
  const { mutate: updateStatus, isPending } = useUpdateOrderStatus();

  // Mapping: productId -> string[] (array of serial numbers, length = item.quantity)
  const [serials, setSerials] = useState<Record<string, string[]>>({});
  const [bulkPasteProduct, setBulkPasteProduct] = useState<{ productId: string; productName: string; quantity: number } | null>(null);
  const [bulkText, setBulkText] = useState('');

  // Initialize serials state when modal opens or order changes
  useEffect(() => {
    if (order && order.items) {
      const initial: Record<string, string[]> = {};
      order.items.forEach(item => {
        // If the item already has serial numbers, prefill them; otherwise create empty array of length item.quantity
        if (item.serialNumbers && item.serialNumbers.length === item.quantity) {
          initial[item.productId] = [...item.serialNumbers];
        } else {
          initial[item.productId] = Array.from({ length: item.quantity }, (_, idx) => 
            item.serialNumbers?.[idx] || ''
          );
        }
      });
      setSerials(initial);
    }
  }, [order, isOpen]);

  // Calculate statistics
  const totalRequired = useMemo(() => {
    return order?.items?.reduce((sum, item) => sum + item.quantity, 0) || 0;
  }, [order]);

  const totalFilled = useMemo(() => {
    let count = 0;
    Object.values(serials).forEach(arr => {
      arr.forEach(s => {
        if (s.trim()) count++;
      });
    });
    return count;
  }, [serials]);

  // Check for duplicate serials across the entire order
  const { duplicateSet, allFilled, hasDuplicates } = useMemo(() => {
    const counts: Record<string, number> = {};
    const dupes = new Set<string>();
    let filled = true;

    if (!order?.items) return { duplicateSet: dupes, allFilled: false, hasDuplicates: false };

    order.items.forEach(item => {
      const arr = serials[item.productId] || [];
      if (arr.length !== item.quantity) filled = false;

      arr.forEach(s => {
        const clean = s.trim().toUpperCase();
        if (!clean) {
          filled = false;
        } else {
          counts[clean] = (counts[clean] || 0) + 1;
          if (counts[clean] > 1) {
            dupes.add(clean);
          }
        }
      });
    });

    return {
      duplicateSet: dupes,
      allFilled: filled && totalFilled === totalRequired && totalRequired > 0,
      hasDuplicates: dupes.size > 0
    };
  }, [order, serials, totalFilled, totalRequired]);

  if (!order || !order.items) return null;

  // Handle single serial input change
  const handleSerialChange = (productId: string, index: number, value: string) => {
    setSerials(prev => {
      const currentList = [...(prev[productId] || [])];
      currentList[index] = value.toUpperCase();
      return { ...prev, [productId]: currentList };
    });
  };

  // Auto-pick available serials from warehouse for a single product
  const handleAutoPickProduct = (productId: string, quantity: number, productName?: string) => {
    // 1. Get available units in stock for this product that haven't been selected yet
    const alreadyUsedSerials = new Set<string>();
    Object.values(serials).forEach(arr => arr.forEach(s => {
      if (s.trim()) alreadyUsedSerials.add(s.trim().toUpperCase());
    }));

    const availableInStock = mockProductUnits
      .filter(u => u.productId === productId && u.status === 'AVAILABLE' && !alreadyUsedSerials.has(u.serialNumber.toUpperCase()))
      .map(u => u.serialNumber);

    const generated: string[] = [];
    const prefix = (productName?.split(' ')[0] || productId).toUpperCase().replace(/[^A-Z0-9]/g, '');

    for (let i = 0; i < quantity; i++) {
      if (availableInStock[i]) {
        generated.push(availableInStock[i]);
      } else {
        // Generate mock standard serial
        const randomHex = Math.floor(1000 + Math.random() * 9000);
        const randomChar = String.fromCharCode(65 + Math.floor(Math.random() * 26));
        generated.push(`${prefix}-${randomHex}-${randomChar}${i + 1}`);
      }
    }

    setSerials(prev => ({
      ...prev,
      [productId]: generated
    }));

    toast.success(`Đã tự động lấy ${quantity} số serie cho ${productName || productId}`);
  };

  // Auto-pick for all products in the order
  const handleAutoFillAll = () => {
    const updated: Record<string, string[]> = {};
    const usedSerials = new Set<string>();

    order.items?.forEach(item => {
      const availableInStock = mockProductUnits
        .filter(u => u.productId === item.productId && u.status === 'AVAILABLE' && !usedSerials.has(u.serialNumber.toUpperCase()))
        .map(u => u.serialNumber);

      const generated: string[] = [];
      const prefix = (item.productName?.split(' ')[0] || item.productId).toUpperCase().replace(/[^A-Z0-9]/g, '');

      for (let i = 0; i < item.quantity; i++) {
        if (availableInStock[i]) {
          generated.push(availableInStock[i]);
          usedSerials.add(availableInStock[i].toUpperCase());
        } else {
          const randomHex = Math.floor(1000 + Math.random() * 9000);
          const randomChar = String.fromCharCode(65 + Math.floor(Math.random() * 26));
          const gen = `${prefix}-${randomHex}-${randomChar}${i + 1}`;
          generated.push(gen);
          usedSerials.add(gen);
        }
      }
      updated[item.productId] = generated;
    });

    setSerials(updated);
    toast.success('Đã tự động gán đủ số serie cho toàn bộ thiết bị trong đơn hàng');
  };

  // Apply bulk paste
  const handleApplyBulkPaste = () => {
    if (!bulkPasteProduct) return;
    const { productId, quantity } = bulkPasteProduct;

    // Split text by newlines, commas, or semicolons
    const tokens = bulkText
      .split(/[\n,;]+/)
      .map(s => s.trim().toUpperCase())
      .filter(Boolean);

    if (tokens.length === 0) {
      toast.error('Chưa có mã serie nào được dán vào');
      return;
    }

    const updatedList = Array.from({ length: quantity }, (_, idx) => tokens[idx] || '');

    setSerials(prev => ({
      ...prev,
      [productId]: updatedList
    }));

    if (tokens.length < quantity) {
      toast.warning(`Bạn đã dán ${tokens.length}/${quantity} mã. Vui lòng nhập thêm ${quantity - tokens.length} mã còn thiếu.`);
    } else {
      toast.success(`Đã dán thành công ${Math.min(tokens.length, quantity)} mã serie.`);
    }

    setBulkPasteProduct(null);
    setBulkText('');
  };

  // Submit approval
  const handleSubmit = () => {
    if (!allFilled || hasDuplicates) {
      toast.error('Vui lòng kiểm tra lại: Bắt buộc nhập đầy đủ số serie và không được trùng lặp.');
      return;
    }

    updateStatus(
      { id: order.id, status: OrderStatus.APPROVED, itemSerials: serials },
      {
        onSuccess: () => {
          toast.success(`Đã duyệt đơn #${order.id} và gán thành công ${totalRequired} số serie ProductUnit!`);
          onClose();
          onSuccess?.();
        },
        onError: (err: any) => {
          toast.error(err.message || 'Lỗi khi duyệt đơn hàng');
        }
      }
    );
  };

  const progressPercent = totalRequired > 0 ? Math.round((totalFilled / totalRequired) * 100) : 0;

  return (
    <>
      <Dialog open={isOpen} onOpenChange={(open) => !open && !isPending && onClose()}>
        <DialogContent className="max-w-3xl max-h-[90vh] flex flex-col p-0 overflow-hidden rounded-2xl border-border bg-card shadow-2xl">
          {/* Header */}
          <DialogHeader className="p-6 pb-4 border-b border-border bg-muted/20">
            <div className="flex items-start justify-between gap-4">
              <div className="flex items-center gap-3">
                <div className="h-10 w-10 rounded-xl bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
                  <Barcode className="h-5 w-5" />
                </div>
                <div>
                  <DialogTitle className="text-base font-bold uppercase tracking-tight flex items-center gap-2">
                    Duyệt đơn & Gán số Serie Thiết bị (ProductUnit)
                  </DialogTitle>
                  <DialogDescription className="text-xs text-muted-foreground mt-0.5">
                    Đơn hàng <span className="font-bold text-foreground">#{order.id}</span> • Khách: <span className="font-semibold text-foreground">{order.customerName}</span>
                  </DialogDescription>
                </div>
              </div>

              <Button
                variant="outline"
                size="sm"
                onClick={handleAutoFillAll}
                className="h-8 rounded-lg text-[10px] font-bold uppercase tracking-wider text-primary border-primary/30 hover:bg-primary/10 transition-all flex items-center gap-1.5 shrink-0"
              >
                <PackageSearch className="h-3.5 w-3.5" />
                Tự động lấy tất cả từ kho
              </Button>
            </div>

            {/* Requirement Notice & Progress */}
            <div className="mt-4 pt-4 border-t border-border/60">
              <div className="flex items-center justify-between text-xs mb-2">
                <span className="text-[11px] font-bold text-muted-foreground uppercase tracking-wider flex items-center gap-1.5">
                  <Cpu className="h-3.5 w-3.5 text-primary" />
                  Tiến độ nhập Serie: {totalFilled} / {totalRequired} thiết bị
                </span>
                <span className={`text-[11px] font-extrabold uppercase ${allFilled && !hasDuplicates ? 'text-emerald-500' : 'text-amber-500'}`}>
                  {progressPercent}% {allFilled && !hasDuplicates ? '• ĐÃ ĐỦ ĐIỀU KIỆN DUYỆT' : '• CẦN NHẬP ĐỦ'}
                </span>
              </div>
              <div className="h-2 w-full bg-muted rounded-full overflow-hidden">
                <div 
                  className={`h-full transition-all duration-500 rounded-full ${
                    allFilled && !hasDuplicates ? 'bg-emerald-500' : 'bg-primary'
                  }`}
                  style={{ width: `${progressPercent}%` }}
                />
              </div>

              {hasDuplicates && (
                <div className="mt-2.5 p-2 rounded-lg bg-destructive/10 border border-destructive/20 text-destructive text-[11px] flex items-center gap-2">
                  <AlertCircle className="h-4 w-4 shrink-0" />
                  <span>Phát hiện số serie bị trùng lặp: <strong>{Array.from(duplicateSet).join(', ')}</strong>. Vui lòng chỉnh sửa.</span>
                </div>
              )}
            </div>
          </DialogHeader>

          {/* Scrollable Content: Items List */}
          <div className="flex-1 overflow-y-auto p-6 space-y-6">
            <div className="text-[11px] text-muted-foreground bg-primary/5 p-3 rounded-xl border border-primary/15 flex items-start gap-2.5">
              <AlertTriangle className="h-4 w-4 text-primary shrink-0 mt-0.5" />
              <div>
                <strong>Chính sách quản lý ProductUnit:</strong> Mỗi thiết bị điện tử bán ra cần được định danh bằng số Serial Number duy nhất để phục vụ tra cứu bảo hành, quản lý tồn kho và xác minh chính xác khi khách hàng yêu cầu đổi trả.
              </div>
            </div>

            {order.items?.map((item, itemIdx) => {
              const currentSerials = serials[item.productId] || [];
              const filledInItem = currentSerials.filter(s => s && s.trim()).length;
              const isItemComplete = filledInItem === item.quantity;

              return (
                <div 
                  key={item.productId} 
                  className="rounded-xl border border-border bg-card p-4 shadow-sm space-y-3 hover:border-border/80 transition-all"
                >
                  {/* Row 1: Product Info */}
                  <div className="flex items-center gap-3">
                    <div className="relative shrink-0">
                      <img 
                        src={item.imageUrl || ''} 
                        alt={item.productName} 
                        className="h-12 w-12 rounded-lg object-cover bg-muted grayscale opacity-90"
                      />
                      <span className="absolute -top-1 -right-1 bg-primary text-primary-foreground text-[9px] font-black w-5 h-5 rounded-full flex items-center justify-center">
                        {itemIdx + 1}
                      </span>
                    </div>
                    <div>
                      <h4 className="text-xs font-bold uppercase tracking-tight text-foreground">{item.productName}</h4>
                      <p className="text-[10px] text-muted-foreground">Mã SP: {item.productId} • Đơn giá: ${item.price.toLocaleString()} • Số lượng: <strong className="text-foreground">{item.quantity}</strong></p>
                    </div>
                  </div>

                  {/* Row 2: Status badge + Action buttons */}
                  <div className="flex flex-wrap items-center gap-2 pt-2 border-t border-border/50">
                    <Badge 
                      variant={isItemComplete ? 'success' : 'pending'} 
                      className="text-[9px] font-bold uppercase tracking-wider px-2.5 py-0.5 shrink-0"
                    >
                      {isItemComplete ? (
                        <span className="flex items-center gap-1"><Check className="h-3 w-3" /> Đã đủ {item.quantity}/{item.quantity}</span>
                      ) : (
                        <span>Cần nhập: {filledInItem}/{item.quantity}</span>
                      )}
                    </Badge>

                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => handleAutoPickProduct(item.productId, item.quantity, item.productName)}
                      className="h-7 text-[9px] font-bold uppercase tracking-wider px-2.5 border-border hover:bg-emerald-500/10 hover:text-emerald-600 hover:border-emerald-500/30 transition-colors flex items-center gap-1.5"
                      title="Lấy số serie có sẵn trong kho"
                    >
                      <PackageSearch className="h-3 w-3" />
                      Lấy từ kho
                    </Button>

                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => {
                        setBulkPasteProduct({
                          productId: item.productId,
                          productName: item.productName || item.productId,
                          quantity: item.quantity
                        });
                        setBulkText(currentSerials.join('\n'));
                      }}
                      className="h-7 text-[9px] font-bold uppercase tracking-wider px-2.5 border-border hover:bg-muted transition-colors flex items-center gap-1.5"
                      title="Dán nhanh danh sách serie từ clipboard"
                    >
                      <ClipboardPaste className="h-3 w-3" />
                      Dán nhanh
                    </Button>
                  </div>

                  {/* Serial Number Inputs */}
                  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                    {Array.from({ length: item.quantity }).map((_, unitIdx) => {
                      const value = currentSerials[unitIdx] || '';
                      const isDupe = value && duplicateSet.has(value.trim().toUpperCase());
                      const isValid = value.trim().length > 0 && !isDupe;

                      return (
                        <div 
                          key={unitIdx}
                          className={`p-2.5 rounded-lg border transition-all ${
                            isDupe 
                              ? 'border-destructive bg-destructive/5' 
                              : isValid 
                                ? 'border-emerald-500/30 bg-emerald-500/5' 
                                : 'border-border bg-muted/20'
                          }`}
                        >
                          <div className="flex items-center justify-between mb-1.5">
                            <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground flex items-center gap-1">
                              <Hash className="h-3 w-3 text-primary" /> Thiết bị #{unitIdx + 1}
                            </span>
                            {isValid && (
                              <span className="text-[9px] font-bold text-emerald-600 dark:text-emerald-400 flex items-center gap-0.5">
                                <Check className="h-2.5 w-2.5" /> Hợp lệ
                              </span>
                            )}
                            {isDupe && (
                              <span className="text-[9px] font-bold text-destructive">Trùng lặp</span>
                            )}
                          </div>

                          <div className="relative">
                            <Input
                              value={value}
                              onChange={(e) => handleSerialChange(item.productId, unitIdx, e.target.value)}
                              placeholder={`VD: ${item.productName?.split(' ')[0] || 'SN'}-SN-${1000 + unitIdx}`}
                              className={`h-8 font-mono text-xs uppercase tracking-wider pr-7 ${
                                isDupe ? 'border-destructive focus-visible:ring-destructive' : ''
                              }`}
                            />
                            {value && (
                              <button
                                type="button"
                                onClick={() => handleSerialChange(item.productId, unitIdx, '')}
                                className="absolute right-2 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                              >
                                <X className="h-3 w-3" />
                              </button>
                            )}
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </div>

          {/* Footer Actions */}
          <DialogFooter className="p-4 px-6 border-t border-border bg-muted/10 flex flex-col sm:flex-row items-center justify-between gap-3">
            <div className="text-[11px] text-muted-foreground">
              {allFilled && !hasDuplicates ? (
                <span className="text-emerald-600 dark:text-emerald-400 font-bold flex items-center gap-1.5">
                  <CheckCircle2 className="h-4 w-4" />
                  Đã nhập đủ {totalRequired} số serie hợp lệ. Sẵn sàng duyệt đơn.
                </span>
              ) : (
                <span className="text-amber-500 font-medium flex items-center gap-1.5">
                  <AlertTriangle className="h-4 w-4" />
                  Còn thiếu {totalRequired - totalFilled} số serie hoặc có lỗi trùng lặp.
                </span>
              )}
            </div>

            <div className="flex items-center gap-2 w-full sm:w-auto">
              <Button
                type="button"
                variant="outline"
                onClick={onClose}
                disabled={isPending}
                className="h-9 px-4 rounded-xl text-[10px] font-bold uppercase tracking-wider border-border"
              >
                Hủy bỏ
              </Button>

              <Button
                type="button"
                onClick={handleSubmit}
                disabled={!allFilled || hasDuplicates || isPending}
                className="h-9 px-5 rounded-xl text-[10px] font-bold uppercase tracking-wider bg-emerald-500 hover:bg-emerald-600 text-white disabled:bg-zinc-200 disabled:text-zinc-400 dark:disabled:bg-zinc-800 dark:disabled:text-zinc-600 flex items-center gap-2 shadow-sm"
              >
                {isPending ? (
                  <>
                    <Loader2 className="h-3.5 w-3.5 animate-spin" />
                    Đang lưu...
                  </>
                ) : (
                  <>
                    <CheckCircle2 className="h-4 w-4" />
                    Xác nhận duyệt đơn ({totalRequired} Serie)
                  </>
                )}
              </Button>
            </div>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Bulk Paste Sub-Dialog */}
      {bulkPasteProduct && (
        <Dialog open={!!bulkPasteProduct} onOpenChange={(open) => !open && setBulkPasteProduct(null)}>
          <DialogContent className="max-w-md rounded-2xl border-border bg-card shadow-2xl">
            <DialogHeader>
              <DialogTitle className="text-sm font-bold uppercase tracking-tight flex items-center gap-2">
                <ClipboardPaste className="h-4 w-4 text-primary" />
                Dán nhanh danh sách Serie
              </DialogTitle>
              <DialogDescription className="text-xs text-muted-foreground">
                Sản phẩm: <strong className="text-foreground">{bulkPasteProduct.productName}</strong> (Cần {bulkPasteProduct.quantity} số serie)
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-3 py-2">
              <p className="text-[11px] text-muted-foreground">
                Dán danh sách mã serie từ Excel, văn bản hoặc máy quét barcode. Mỗi mã cách nhau bằng <strong>xuống dòng</strong> hoặc <strong>dấu phẩy</strong>:
              </p>
              <Textarea
                rows={6}
                value={bulkText}
                onChange={(e) => setBulkText(e.target.value)}
                placeholder={"SN-001\nSN-002\nSN-003"}
                className="font-mono text-xs uppercase"
              />
              <p className="text-[10px] text-muted-foreground">
                Số lượng mã nhận diện: {bulkText.split(/[\n,;]+/).filter(s => s.trim()).length} / {bulkPasteProduct.quantity}
              </p>
            </div>

            <DialogFooter className="gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setBulkPasteProduct(null)}
                className="h-8 rounded-lg text-[10px] font-bold uppercase tracking-wider"
              >
                Hủy
              </Button>
              <Button
                size="sm"
                onClick={handleApplyBulkPaste}
                className="h-8 rounded-lg text-[10px] font-bold uppercase tracking-wider bg-primary text-primary-foreground"
              >
                Áp dụng danh sách
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}
    </>
  );
}
