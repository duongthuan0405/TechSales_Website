import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';
import { 
  Star, 
  ChevronLeft, 
  ShieldCheck, 
  Truck, 
  Loader2,
  CheckCircle2,
  MessageSquare,
  EyeOff,
  Eye,
  ShieldAlert,
  AlertTriangle
} from 'lucide-react';
import { Button } from '../../components/ui/button';
import { Badge } from '../../components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../../components/ui/tabs';
import { useGetProduct } from '../../../dataHook/productDataHook';
import { useGetProductReviews, useModerateReview, useReplyToReview } from '../../../dataHook/reviewDataHook';
import { ReviewStatus } from '../../../models/ui_types/review';
import { toast } from 'sonner';
import { Separator } from '../../components/ui/separator';
import { Input } from '../../components/ui/input';
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

export function StaffProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data: product, isLoading: isProductLoading, isError } = useGetProduct(id || '');
  const { data: reviewData, isLoading: isReviewsLoading } = useGetProductReviews(id || '');
  const reviews = reviewData?.reviews || [];
  const { mutate: moderateReview } = useModerateReview();
  const { mutate: replyToReview } = useReplyToReview();

  const [replyingTo, setReplyingTo] = useState<string | null>(null);
  const [replyText, setReplyText] = useState('');
  const [confirmingMod, setConfirmingMod] = useState<{ id: string, status: ReviewStatus } | null>(null);

  useEffect(() => {
    if (isError) {
      toast.error('Product not found in operational database');
      navigate('/sales/products');
    }
  }, [isError, navigate]);

  const handleModerate = () => {
    if (!confirmingMod) return;
    
    moderateReview({ id: confirmingMod.id, status: confirmingMod.status }, {
      onSuccess: () => {
        toast.success(`Review visibility updated`);
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
        toast.success('Reply protocol synchronized');
        setReplyingTo(null);
        setReplyText('');
      },
    });
  };

  if (isProductLoading || !product) {
    return (
      <div className="flex h-[70vh] items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-6xl space-y-8 pb-16 px-4 md:px-0">
      <Button 
        variant="ghost" 
        className="mb-2 pl-0 hover:bg-transparent font-bold text-muted-foreground hover:text-foreground transition-colors text-[10px] tracking-widest uppercase" 
        onClick={() => navigate(-1)}
      >
        <ChevronLeft className="mr-1 h-4 w-4" />
        Return to Catalog
      </Button>

      <div className="grid grid-cols-1 gap-10 lg:grid-cols-2 items-start">
        {/* Product Images */}
        <div className="space-y-4">
          <div className="aspect-square overflow-hidden rounded-2xl border border-border bg-card p-10">
            <img 
              src={product.imageUrl || ''} 
              alt={product.name} 
              className="h-full w-full object-contain grayscale opacity-90 transition-all duration-500 hover:grayscale-0 hover:opacity-100"
            />
          </div>
        </div>

        {/* Product Info */}
        <div className="space-y-6">
          <div className="space-y-2">
            <Badge className="bg-primary text-primary-foreground px-2.5 py-0.5 rounded-md font-bold text-[8px] uppercase tracking-wider border-none">{product.categoryName || 'General'}</Badge>
            <h1 className="text-3xl font-bold tracking-tight text-foreground uppercase">{product.name}</h1>
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-1">
                <Star className="h-3.5 w-3.5 fill-yellow-400 text-yellow-400" />
                <span className="text-sm font-bold text-foreground">{product.rating || 0}</span>
              </div>
              <span className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">{reviews.length} Logged Reviews</span>
            </div>
          </div>

          <div className="flex items-baseline gap-2">
            <span className="text-4xl font-bold text-foreground tracking-tight">{formatCurrency(product.price)}</span>
            <span className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">VND</span>
          </div>

          <p className="text-base leading-relaxed text-muted-foreground font-medium">
            {product.description}
          </p>

          <div className="space-y-6 pt-4 border-t border-border">
            <div className="flex items-center justify-between p-4 rounded-xl bg-primary/5 border border-primary/20">
              <div className="flex items-center gap-3">
                 <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center">
                    <CheckCircle2 className="h-5 w-5 text-primary" />
                 </div>
                 <div>
                    <p className="text-[10px] font-bold uppercase tracking-widest text-primary">Catalog Status</p>
                    <p className="text-xs font-black uppercase tracking-tight">Active for Fulfillment</p>
                 </div>
              </div>
              <Badge variant="outline" className="text-[9px] font-bold uppercase tracking-widest px-3 border-primary/30 text-primary">
                {product.stock} Units In Reserve
              </Badge>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="flex items-center gap-3 p-3 rounded-xl bg-muted/30 border border-border/50">
              <Truck className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-[9px] font-bold uppercase tracking-widest text-muted-foreground">Logistics Tier</p>
                <p className="font-bold text-xs uppercase tracking-tight">Priority Shipping</p>
              </div>
            </div>
            <div className="flex items-center gap-3 p-3 rounded-xl bg-muted/30 border border-border/50">
              <ShieldCheck className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-[9px] font-bold uppercase tracking-widest text-muted-foreground">Quality Control</p>
                <p className="font-bold text-xs uppercase tracking-tight">Standard Warranty</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <Tabs defaultValue="moderation" className="w-full mt-10">
        <TabsList className="h-12 w-full justify-start gap-8 border-b border-border bg-transparent p-0">
          <TabsTrigger 
            value="moderation" 
            className="h-12 rounded-none border-b-2 border-transparent px-1 font-bold uppercase tracking-widest text-[10px] text-muted-foreground data-[state=active]:border-primary data-[state=active]:text-foreground transition-all"
          >
            Review Moderation ({reviews.length})
          </TabsTrigger>
          <TabsTrigger 
            value="description" 
            className="h-12 rounded-none border-b-2 border-transparent px-1 font-bold uppercase tracking-widest text-[10px] text-muted-foreground data-[state=active]:border-primary data-[state=active]:text-foreground transition-all"
          >
            Protocol Details
          </TabsTrigger>
        </TabsList>
        
        <TabsContent value="moderation" className="py-8">
          <div className="flex flex-col gap-10 lg:flex-row">
            <div className="w-full lg:w-64">
              <div className="p-6 rounded-2xl bg-muted/30 text-center space-y-1 border border-border/50">
                <div className="text-5xl font-bold text-foreground tracking-tighter">{product.rating || 0}</div>
                <div className="flex justify-center gap-1">
                  {[1,2,3,4,5].map(i => (
                    <Star key={i} className={`h-3.5 w-3.5 ${i <= (product.rating || 0) ? 'fill-yellow-400 text-yellow-400' : 'text-muted'}`} />
                  ))}
                </div>
                <p className="text-[9px] font-bold uppercase tracking-widest text-muted-foreground">Aggregate Sentiment</p>
              </div>
            </div>

            <div className="flex-1 space-y-6">
              <h3 className="text-xl font-bold uppercase tracking-tight text-foreground">Customer Sentiment Feed</h3>
              {isReviewsLoading ? (
                <div className="flex justify-center py-10"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div>
              ) : reviews.length === 0 ? (
                <div className="py-12 text-center text-muted-foreground font-bold uppercase tracking-widest bg-muted/20 rounded-2xl border border-dashed border-border">
                  No sentiment logs recorded
                </div>
              ) : (
                <div className="space-y-6">
                  {reviews.map((rev) => (
                    <div key={rev.id} className={`p-6 rounded-2xl border transition-all ${rev.status === ReviewStatus.HIDDEN ? 'opacity-60 bg-muted/20 border-dashed' : 'bg-card border-border shadow-sm'}`}>
                      <div className="space-y-4">
                        <div className="flex items-center justify-between">
                          <div className="flex items-center gap-3">
                            <div className="h-10 w-10 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold text-sm uppercase">
                              {rev.userName.charAt(0)}
                            </div>
                            <div>
                              <p className="font-bold text-sm uppercase tracking-tight">{rev.userName}</p>
                              <div className="flex gap-0.5">
                                {[1,2,3,4,5].map(j => (
                                  <Star key={j} className={`h-2.5 w-2.5 ${j <= rev.rating ? 'fill-yellow-400 text-yellow-400' : 'text-muted'}`} />
                                ))}
                              </div>
                            </div>
                          </div>
                          <div className="flex items-center gap-3">
                             <Badge variant={rev.status === ReviewStatus.VISIBLE ? 'success' : 'pending'} className="text-[8px] font-black uppercase tracking-widest">
                                {rev.status}
                             </Badge>
                             <span className="text-[9px] font-medium text-muted-foreground uppercase tracking-widest">
                                {new Date(rev.createdAt).toLocaleDateString()}
                             </span>
                          </div>
                        </div>
                        <p className="text-sm text-foreground leading-relaxed font-medium italic p-4 rounded-xl bg-muted/30 border border-border/50">
                          "{rev.comment}"
                        </p>

                        {/* Responses */}
                        {rev.responses && rev.responses.length > 0 && (
                          <div className="space-y-3 pl-4 border-l-2 border-primary/20 mt-4">
                            {rev.responses.map((resp) => (
                              <div key={resp.id} className="p-4 rounded-xl bg-primary/5 border border-primary/10 space-y-1">
                                <div className="flex justify-between items-center">
                                  <p className="text-[9px] font-bold uppercase tracking-widest text-primary">
                                    {resp.userName || 'Official Protocol Response'}
                                  </p>
                                  <span className="text-[8px] text-muted-foreground uppercase font-medium">
                                    {new Date(resp.createdAt).toLocaleDateString()}
                                  </span>
                                </div>
                                <p className="text-xs text-foreground font-medium leading-relaxed italic">"{resp.content}"</p>
                              </div>
                            ))}
                          </div>
                        )}

                        {/* Moderation Actions */}
                        {replyingTo === rev.id ? (
                          <div className="space-y-3 pt-2">
                            <Input 
                              placeholder="Type your official response..." 
                              className="h-10 rounded-lg text-xs"
                              value={replyText}
                              onChange={(e) => setReplyText(e.target.value)}
                            />
                            <div className="flex gap-2">
                              <Button size="sm" className="h-8 px-4 rounded-lg text-[9px] font-bold uppercase tracking-widest" onClick={() => handleReply(rev.id)}>
                                Send Reply
                              </Button>
                              <Button variant="ghost" size="sm" className="h-8 px-4 rounded-lg text-[9px] font-bold uppercase tracking-widest" onClick={() => setReplyingTo(null)}>
                                Cancel
                              </Button>
                            </div>
                          </div>
                        ) : (
                          <div className="flex gap-2 pt-2 border-t border-border mt-4 pt-4">
                            <Button 
                              variant="outline" 
                              size="sm" 
                              className="h-8 px-4 rounded-lg text-[9px] font-bold uppercase tracking-widest border-border"
                              onClick={() => setReplyingTo(rev.id)}
                            >
                              <MessageSquare className="h-3.5 w-3.5 mr-2" />
                              {rev.responses?.length || 0 > 0 ? 'Reply Again' : 'Reply'}
                            </Button>
                            <Button 
                              variant="ghost" 
                              size="sm" 
                              className={`h-8 px-4 rounded-lg text-[9px] font-bold uppercase tracking-widest ${rev.status === ReviewStatus.VISIBLE ? 'text-muted-foreground' : 'text-primary'}`}
                              onClick={() => setConfirmingMod({ id: rev.id, status: rev.status === ReviewStatus.VISIBLE ? ReviewStatus.HIDDEN : ReviewStatus.VISIBLE })}
                            >
                              {rev.status === ReviewStatus.VISIBLE ? (
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
                  ))}
                </div>
              )}
            </div>
          </div>
        </TabsContent>

        <TabsContent value="description" className="py-8">
          <div className="max-w-3xl space-y-6">
            <h3 className="text-xl font-bold uppercase tracking-tight text-foreground">System Specifications</h3>
            <p className="text-base text-muted-foreground leading-relaxed font-medium">
              {product.description}. This product is currently active in the marketplace protocol and monitored for fulfillment accuracy.
            </p>
            <Separator className="bg-border" />
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
               {[
                 'Verified Industrial Durability',
                 'Sustainable Logistics Sourcing',
                 'Thermal Efficiency Optimized',
                 'Ecosystem Integration Ready'
               ].map((feature, idx) => (
                 <div key={idx} className="flex items-center gap-3">
                    <div className="h-10 w-10 rounded-xl bg-muted/50 flex items-center justify-center">
                       <CheckCircle2 className="h-5 w-5 text-primary" />
                    </div>
                    <span className="font-bold text-xs text-foreground uppercase tracking-tight">{feature}</span>
                 </div>
               ))}
            </div>
          </div>
        </TabsContent>
      </Tabs>

      {/* Moderation Confirmation Dialog */}
      <AlertDialog open={!!confirmingMod} onOpenChange={(open) => !open && setConfirmingMod(null)}>
        <AlertDialogContent className="rounded-2xl border-border bg-card shadow-2xl">
          <AlertDialogHeader>
            <div className="flex items-center gap-3 mb-2">
               <div className="p-2 rounded-xl bg-primary/10 text-primary">
                 <AlertTriangle className="h-5 w-5" />
               </div>
               <AlertDialogTitle className="text-sm font-bold uppercase tracking-tight">
                 {confirmingMod?.status === ReviewStatus.HIDDEN ? 'Suspend Visibility?' : 'Restore Visibility?'}
               </AlertDialogTitle>
            </div>
            <AlertDialogDescription className="text-xs font-medium text-muted-foreground leading-relaxed italic">
              {confirmingMod?.status === ReviewStatus.HIDDEN 
                ? "Are you sure you want to suspend this feedback from the public feed? This action will be logged in the moderation audit trail."
                : "Confirm restoration of this feedback to the public sentiment feed."
              }
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="mt-6">
            <AlertDialogCancel className="h-10 rounded-xl text-[10px] font-bold uppercase tracking-widest border-border">Cancel</AlertDialogCancel>
            <AlertDialogAction 
              onClick={handleModerate}
              className="h-10 rounded-xl text-[10px] font-bold uppercase tracking-widest text-white bg-primary hover:bg-primary/90"
            >
              Update Status
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
