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
import { useSubmitReview } from '../../dataHook/reviewDataHook';
import { toast } from 'sonner';

interface ReviewModalProps {
  isOpen: boolean;
  onClose: () => void;
  productId: string;
  productName: string;
  userId: string;
  userName: string;
}

export function ReviewModal({ 
  isOpen, 
  onClose, 
  productId, 
  productName, 
  userId, 
  userName 
}: ReviewModalProps) {
  const [rating, setRating] = useState(5);
  const [hoverRating, setHoverRating] = useState(0);
  const [comment, setComment] = useState('');
  const { mutate: submitReview, isPending } = useSubmitReview();

  const handleSubmit = () => {
    if (!comment.trim()) {
      toast.error('Please write a comment');
      return;
    }

    submitReview({
      productId,
      userId,
      userName,
      rating,
      comment
    }, {
      onSuccess: () => {
        toast.success('Review submitted successfully!');
        onClose();
        setComment('');
        setRating(5);
      },
      onError: () => {
        toast.error('Failed to submit review');
      }
    });
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[450px] rounded-2xl p-6 border-none shadow-xl">
        <DialogHeader className="space-y-1.5">
          <DialogTitle className="text-xl font-bold uppercase tracking-tight">Review Product</DialogTitle>
          <DialogDescription className="text-slate-500 text-sm">
            Share your thoughts on <span className="text-slate-950 dark:text-white font-bold">{productName}</span>
          </DialogDescription>
        </DialogHeader>

        <div className="py-6 space-y-6">
          <div className="flex flex-col items-center gap-3 py-6 bg-slate-50 dark:bg-slate-900 rounded-xl">
            <span className="text-[10px] font-bold uppercase tracking-widest text-slate-400">Rating</span>
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
              {rating === 5 ? 'Excellent' : rating === 4 ? 'Great' : rating === 3 ? 'Good' : rating === 2 ? 'Fair' : 'Poor'}
            </span>
          </div>

          <div className="space-y-2">
            <label className="text-[10px] font-bold uppercase tracking-widest text-slate-400 ml-1">Comment</label>
            <Textarea 
              placeholder="What's your experience?" 
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              className="min-h-[120px] rounded-xl border-slate-200 focus:ring-slate-950 text-sm"
            />
          </div>
        </div>

        <DialogFooter className="gap-2 sm:gap-2">
          <Button variant="ghost" onClick={onClose} className="rounded-xl h-11 flex-1 font-bold text-xs uppercase tracking-widest">
            Cancel
          </Button>
          <Button 
            onClick={handleSubmit} 
            disabled={isPending}
            className="rounded-xl h-11 flex-1 bg-slate-950 hover:bg-black text-white font-bold text-xs uppercase tracking-widest"
          >
            {isPending ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
            Submit
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
