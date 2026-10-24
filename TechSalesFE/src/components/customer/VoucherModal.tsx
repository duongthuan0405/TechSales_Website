import { Modal } from '../ui/modal';
import { Button } from '../ui/button';
import { useGetAvailableVouchers } from '../../dataHook/voucherDataHook';
import { Voucher, VoucherType } from '../../models/ui_types/voucher';
import { Loader2, Ticket, Clock, CheckCircle2 } from 'lucide-react';
import { Badge } from '../ui/badge';

interface VoucherModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSelect: (voucher: Voucher) => void;
  currentTotal: number;
}

export function VoucherModal({ isOpen, onClose, onSelect, currentTotal }: VoucherModalProps) {
  const { data: vouchers = [], isLoading } = useGetAvailableVouchers();

  return (
    <Modal open={isOpen} onOpenChange={onClose} title="SELECT VOUCHER">
      <div className="space-y-6 pt-4 max-h-[60vh] overflow-auto pr-1 scrollbar-none">
        {isLoading ? (
          <div className="flex h-40 items-center justify-center">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
          </div>
        ) : vouchers.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-10 text-center bg-muted/20 rounded-2xl border-2 border-dashed border-border">
            <Ticket className="h-10 w-10 text-muted/30 mb-4" />
            <p className="text-sm font-medium text-muted-foreground uppercase tracking-widest">No active vouchers</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-4">
            {vouchers.map((voucher) => {
              const isDisabled = voucher.minOrderAmount ? currentTotal < voucher.minOrderAmount : false;
              
              return (
                <div 
                  key={voucher.id}
                  className={`relative flex flex-col gap-4 p-5 rounded-2xl border-2 transition-all ${
                    isDisabled 
                      ? 'border-border/40 opacity-60' 
                      : 'border-border hover:border-primary cursor-pointer bg-card hover:shadow-lg'
                  }`}
                  onClick={() => !isDisabled && onSelect(voucher)}
                >
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
                      <Ticket className="h-6 w-6" />
                    </div>
                    <div className="flex-1 space-y-1">
                      <div className="flex items-center gap-3">
                        <span className="font-bold text-sm uppercase tracking-tighter">{voucher.code}</span>
                        <Badge variant="outline" className="text-[8px] font-bold tracking-widest border-primary/20 uppercase px-2 py-0">
                          {voucher.type === VoucherType.PERCENTAGE ? `${voucher.value}% OFF` : `$${voucher.value} OFF`}
                        </Badge>
                      </div>

                    </div>
                  </div>

                  <div className="flex items-center justify-between pt-4 border-t border-border/50">
                    <div className="flex items-center gap-1.5 text-[9px] font-normal text-muted-foreground uppercase tracking-widest">
                      <Clock className="h-3 w-3" />
                      Exp: {voucher.endDate ? new Date(voucher.endDate).toLocaleDateString() : 'Never'}
                    </div>
                    {isDisabled ? (
                      <span className="text-[9px] font-medium text-destructive uppercase tracking-widest">
                        Min spend: ${voucher.minOrderAmount}
                      </span>
                    ) : (
                      <div className="flex items-center gap-1 text-[9px] font-bold text-primary uppercase tracking-widest">
                        Available
                        <CheckCircle2 className="h-3 w-3" />
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
      <div className="pt-6">
        <Button variant="outline" className="w-full h-11 rounded-xl font-bold uppercase tracking-widest text-[10px]" onClick={onClose}>
          Close
        </Button>
      </div>
    </Modal>
  );
}
