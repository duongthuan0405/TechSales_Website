import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Star, MessageSquare, ShieldAlert, EyeOff, Eye, Loader2, Search, AlertTriangle } from 'lucide-react';
import { useGetAllReviews, useModerateReview, useReplyToReview } from '../../../dataHook/reviewDataHook';
import { ReviewStatus } from '../../../models/ui_types/review';
import { toast } from 'sonner';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "../../components/ui/alert-dialog";

export function ReviewManagementPage() {
  const { data: reviews = [], isLoading } = useGetAllReviews();
  const { mutate: moderateReview } = useModerateReview();
  const { mutate: replyToReview } = useReplyToReview();
  
  const [searchTerm, setSearchTerm] = useState('');
  const [replyingTo, setReplyingTo] = useState<string | null>(null);
  const [replyText, setReplyText] = useState('');

  // Moderation Confirmation State
  const [confirmingMod, setConfirmingMod] = useState<{ id: string, status: ReviewStatus } | null>(null);

  const filteredReviews = reviews.filter(rev => 
    rev.userName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    rev.comment.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (rev.productName || '').toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleModerate = () => {
    if (!confirmingMod) return;
    
    moderateReview({ id: confirmingMod.id, status: confirmingMod.status }, {
      onSuccess: () => {
        toast.success(`Review protocol updated to ${confirmingMod.status}`);
        setConfirmingMod(null);
      },
      onError: () => {
        toast.error('Failed to update review status');
        setConfirmingMod(null);
      }
    });
  };

  const handleReply = (id: string) => {
    if (!replyText.trim()) return;
    replyToReview({ id, reply: replyText }, {
      onSuccess: () => {
        toast.success('Reply sent');
        setReplyingTo(null);
        setReplyText('');
      },
    });
  };

  return (
    <div className="space-y-6 pb-12">
      <div>
        <h1 className="text-3xl font-bold tracking-tight uppercase">Review Moderation</h1>
        <p className="text-sm text-muted-foreground font-bold uppercase tracking-widest opacity-60">Customer Feedback Protocol</p>
      </div>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="Search by user, product, or content..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="pl-10 h-12 rounded-xl border-border bg-card"
        />
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-muted-foreground uppercase tracking-widest text-[10px] font-bold">Retrieving Feedback Logs...</p>
          </div>
        </div>
      ) : (
        <div className="space-y-4">
          {filteredReviews.map((review) => (
            <Card key={review.id} className={`border-border shadow-sm rounded-2xl overflow-hidden transition-all ${review.status === ReviewStatus.HIDDEN ? 'opacity-60 bg-muted/20' : 'bg-card'}`}>
              <CardContent className="p-6">
                <div className="flex flex-col gap-6 md:flex-row">
                  <div className="w-full md:w-48 shrink-0 flex flex-col gap-3">
                    <div className="flex items-center gap-2">
                       <div className="h-8 w-8 rounded-full bg-primary flex items-center justify-center text-primary-foreground font-bold text-xs uppercase">
                         {review.userName.charAt(0)}
                       </div>
                       <div>
                         <p className="text-xs font-bold uppercase tracking-tight">{review.userName}</p>
                         <p className="text-[9px] text-muted-foreground font-medium uppercase">{new Date(review.createdAt).toLocaleDateString()}</p>
                       </div>
                    </div>
                    <div className="flex items-center gap-0.5">
                      {[1,2,3,4,5].map(i => (
                        <Star key={i} className={`h-3 w-3 ${i <= review.rating ? 'fill-yellow-400 text-yellow-400' : 'text-muted'}`} />
                      ))}
                    </div>
                    <Badge variant={review.status === ReviewStatus.VISIBLE ? 'success' : 'pending'} className="text-[8px] font-black uppercase tracking-widest w-fit">
                      {review.status}
                    </Badge>
                  </div>

                  <div className="flex-1 space-y-4">
                    <div className="space-y-1">
                       <p className="text-[9px] font-bold uppercase tracking-widest text-muted-foreground">Product Feedback</p>
                       <h4 className="font-bold text-sm uppercase tracking-tight">{review.productName || 'Unknown Product'}</h4>
                    </div>
                    
                    <div className="p-4 rounded-xl bg-muted/30 border border-border/50 italic text-sm text-foreground">
                      "{review.comment}"
                    </div>

                    {/* Multiple Responses */}
                    {review.responses && review.responses.length > 0 ? (
                      <div className="space-y-3">
                        {review.responses.map((resp) => (
                          <div key={resp.id} className="p-4 rounded-xl bg-primary/5 border border-primary/10 space-y-1">
                            <div className="flex items-center justify-between">
                              <div className="flex items-center gap-2">
                                <MessageSquare className="h-3 w-3 text-primary" />
                                <p className="text-[9px] font-bold uppercase tracking-widest text-primary">
                                  {resp.userName || 'Staff Response'}
                                </p>
                              </div>
                              <span className="text-[8px] text-muted-foreground font-medium">
                                {new Date(resp.createdAt).toLocaleDateString()}
                              </span>
                            </div>
                            <p className="text-sm text-foreground font-medium">{resp.content}</p>
                          </div>
                        ))}
                      </div>
                    ) : review.reply ? (
                      /* Legacy single reply fallback */
                      <div className="p-4 rounded-xl bg-primary/5 border border-primary/10 space-y-2">
                        <div className="flex items-center gap-2">
                          <MessageSquare className="h-3 w-3 text-primary" />
                          <p className="text-[9px] font-bold uppercase tracking-widest text-primary">Staff Response</p>
                        </div>
                        <p className="text-sm text-foreground font-medium">{review.reply}</p>
                      </div>
                    ) : null}

                    {replyingTo === review.id ? (
                      <div className="space-y-3 pt-2">
                        <Input 
                          placeholder="Type your protocol response..." 
                          className="h-10 rounded-lg text-xs"
                          value={replyText}
                          onChange={(e) => setReplyText(e.target.value)}
                        />
                        <div className="flex gap-2">
                          <Button size="sm" className="h-8 px-4 rounded-lg text-[9px] font-bold uppercase tracking-widest" onClick={() => handleReply(review.id)}>
                            Send Reply
                          </Button>
                          <Button variant="ghost" size="sm" className="h-8 px-4 rounded-lg text-[9px] font-bold uppercase tracking-widest" onClick={() => setReplyingTo(null)}>
                            Cancel
                          </Button>
                        </div>
                      </div>
                    ) : (
                      <div className="flex gap-2 pt-2">
                        <Button 
                          variant="outline" 
                          size="sm" 
                          className="h-8 px-4 rounded-lg text-[9px] font-bold uppercase tracking-widest border-border"
                          onClick={() => setReplyingTo(review.id)}
                        >
                          <MessageSquare className="h-3.5 w-3.5 mr-2" />
                          {(review.responses && review.responses.length > 0) || review.reply ? 'Reply Again' : 'Reply'}
                        </Button>
                        <Button 
                          variant="ghost" 
                          size="sm" 
                          className={`h-8 px-4 rounded-lg text-[9px] font-bold uppercase tracking-widest ${review.status === ReviewStatus.VISIBLE ? 'text-muted-foreground' : 'text-primary'}`}
                          onClick={() => setConfirmingMod({ id: review.id, status: review.status === ReviewStatus.VISIBLE ? ReviewStatus.HIDDEN : ReviewStatus.VISIBLE })}
                        >
                          {review.status === ReviewStatus.VISIBLE ? (
                            <><EyeOff className="h-3.5 w-3.5 mr-2" /> Hide</>
                          ) : (
                            <><Eye className="h-3.5 w-3.5 mr-2" /> Show</>
                          )}
                        </Button>
                        <Button variant="ghost" size="sm" className="h-8 px-4 rounded-lg text-[9px] font-bold uppercase tracking-widest text-destructive hover:bg-destructive/10">
                          <ShieldAlert className="h-3.5 w-3.5 mr-2" />
                          Report
                        </Button>
                      </div>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Moderation Confirmation Dialog */}
      <AlertDialog open={!!confirmingMod} onOpenChange={(open) => !open && setConfirmingMod(null)}>
        <AlertDialogContent className="rounded-2xl border-border bg-card shadow-2xl">
          <AlertDialogHeader>
            <div className="flex items-center gap-3 mb-2">
               <div className="p-2 rounded-xl bg-primary/10 text-primary">
                 <AlertTriangle className="h-5 w-5" />
               </div>
               <AlertDialogTitle className="text-sm font-bold uppercase tracking-tight">
                 {confirmingMod?.status === ReviewStatus.HIDDEN ? 'Hide Feedback?' : 'Publish Feedback?'}
               </AlertDialogTitle>
            </div>
            <AlertDialogDescription className="text-xs font-medium text-muted-foreground leading-relaxed italic">
              {confirmingMod?.status === ReviewStatus.HIDDEN 
                ? "Are you sure you want to hide this review from the public protocol? It will no longer be visible on the product detail page."
                : "Confirm that this review is appropriate and should be made visible to all users."
              }
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="mt-6">
            <AlertDialogCancel className="h-10 rounded-xl text-[10px] font-bold uppercase tracking-widest border-border">Cancel</AlertDialogCancel>
            <AlertDialogAction 
              onClick={handleModerate}
              className="h-10 rounded-xl text-[10px] font-bold uppercase tracking-widest text-white bg-primary hover:bg-primary/90"
            >
              Update Visibility
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
