import { useState } from 'react';
import { 
  Dialog, 
  DialogContent, 
  DialogHeader, 
  DialogTitle, 
  DialogFooter,
  DialogDescription
} from '../ui/dialog';
import { Button } from '../ui/button';
import { Textarea } from '../ui/textarea';
import { Star, Loader2 } from 'lucide-react';
import { useSubmitReview } from '../../../dataHook/reviewDataHook';
import { toast } from 'sonner';

interface ReviewModalProps {
  isOpen: boolean;
  onClose: () => void;
  orderId: string;
  productId: string;
  productName: string;
}

export function ReviewModal({ 
  isOpen, 
  onClose, 
  orderId,
  productId, 
  productName
}: ReviewModalProps) {
  const [rating, setRating] = useState(5);
  const [hoverRating, setHoverRating] = useState(0);
  const [comment, setComment] = useState('');
  const { mutate: submitReview, isPending } = useSubmitReview();

  const handleSubmit = () => {
    if (!comment.trim()) {
      toast.error('Vui lòng nhập nhận xét');
      return;
    }

    submitReview({
      orderId,
      productId,
      ratingStars: rating,
      reviewComment: comment
    }, {
      onSuccess: () => {
        toast.success('Gửi đánh giá thành công!');
        onClose();
        setComment('');
        setRating(5);
      },
      onError: () => {
        toast.error('Gửi đánh giá thất bại');
      }
    });
  };

  const getRatingLabel = (score: number) => {
    switch (score) {
      case 5: return 'Tuyệt vời';
      case 4: return 'Rất tốt';
      case 3: return 'Bình thường';
      case 2: return 'Tạm được';
      default: return 'Chưa tốt';
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[450px] rounded-2xl p-6 border-none shadow-xl">
        <DialogHeader className="space-y-1.5">
          <DialogTitle className="text-xl font-bold uppercase tracking-tight">Đánh giá sản phẩm</DialogTitle>
          <DialogDescription className="text-slate-500 text-sm">
            Chia sẻ cảm nhận của bạn về <span className="text-slate-950 dark:text-white font-bold">{productName}</span>
          </DialogDescription>
        </DialogHeader>

        <div className="py-6 space-y-6">
          <div className="flex flex-col items-center gap-3 py-6 bg-slate-50 dark:bg-slate-900 rounded-xl">
            <span className="text-[10px] font-bold uppercase tracking-widest text-slate-400">Số sao đánh giá</span>
            <div className="flex gap-2">
              {[1, 2, 3, 4, 5].map((star) => (
                <button
                  key={star}
                  type="button"
                  onClick={() => setRating(star)}
                  onMouseEnter={() => setHoverRating(star)}
                  onMouseLeave={() => setHoverRating(0)}
                  className="transition-transform active:scale-90"
                >
                  <Star 
                    className={`h-7 w-7 transition-colors ${
                      (hoverRating || rating) >= star 
                        ? 'fill-slate-950 text-slate-950 dark:fill-white dark:text-white' 
                        : 'text-slate-200 dark:text-slate-800'
                    }`} 
                  />
                </button>
              ))}
            </div>
            <span className="text-[10px] font-bold uppercase tracking-widest text-slate-950 dark:text-white">
              {getRatingLabel(rating)}
            </span>
          </div>

          <div className="space-y-2">
            <label className="text-[10px] font-bold uppercase tracking-widest text-slate-400 ml-1">Nhận xét chi tiết</label>
            <Textarea 
              placeholder="Bạn cảm nhận thế nào về chất lượng sản phẩm..." 
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              className="min-h-[120px] rounded-xl border-slate-200 focus:ring-slate-950 text-sm"
            />
          </div>
        </div>

        <DialogFooter className="gap-2 sm:gap-2">
          <Button variant="ghost" onClick={onClose} className="rounded-xl h-11 flex-1 font-bold text-xs uppercase tracking-widest">
            Hủy
          </Button>
          <Button 
            onClick={handleSubmit} 
            disabled={isPending}
            className="rounded-xl h-11 flex-1 bg-slate-950 hover:bg-black text-white font-bold text-xs uppercase tracking-widest"
          >
            {isPending ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
            Gửi đánh giá
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
