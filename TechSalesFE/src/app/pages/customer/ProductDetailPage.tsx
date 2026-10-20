import { useState, useEffect } from 'react';
import { formatCurrency } from '../../../utils/format';
import { useParams, useNavigate } from 'react-router';
import { 
  Star, 
  ShoppingCart, 
  ChevronLeft, 
  ShieldCheck, 
  Truck, 
  Plus, 
  Minus,
  Loader2,
  CheckCircle2
} from 'lucide-react';
import { Button } from '../../components/ui/button';
import { Badge } from '../../components/ui/badge';
import { Card, CardContent } from '../../components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../../components/ui/tabs';
import { useGetProduct } from '../../../dataHook/productDataHook';
import { useGetProductReviews } from '../../../dataHook/reviewDataHook';
import { useAddToCart } from '../../../dataHook/cartDataHook';
import { ProductStatus } from '../../../models/ui_types/product';
import { toast } from 'sonner';
import { Separator } from '../../components/ui/separator';

export function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [quantity, setQuantity] = useState(1);
  const { data: product, isLoading: isProductLoading, isError } = useGetProduct(id || '');
  const { data: reviewData, isLoading: isReviewsLoading } = useGetProductReviews(id || '');
  const reviews = reviewData?.reviews || [];
  const { mutate: addToCart, isPending: isAdding } = useAddToCart();

  useEffect(() => {
    if (isError) {
      toast.error('Không tìm thấy sản phẩm');
      navigate('/customer/products');
    }
  }, [isError, navigate]);

  const handleAddToCart = () => {
    if (!product) return;
    addToCart(product.id, {
      onSuccess: () => toast.success('Đã thêm vào giỏ hàng'),
      onError: () => toast.error('Thêm vào giỏ hàng thất bại')
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
        Quay lại danh sách
      </Button>

      <div className="grid grid-cols-1 gap-10 lg:grid-cols-2 items-start">
        {/* Product Images */}
        <div className="space-y-4">
          <div className="aspect-square overflow-hidden rounded-2xl border border-border bg-card p-10">
            <img 
              src={product.imageUrl || ''} 
              alt={product.name} 
              className="h-full w-full object-contain transition-transform duration-500 hover:scale-105"
            />
          </div>
        </div>

        {/* Product Info */}
        <div className="space-y-6">
          <div className="space-y-2">
            <Badge className="bg-primary text-primary-foreground px-2.5 py-0.5 rounded-md font-bold text-[8px] uppercase tracking-wider border-none">{product.categoryName || 'Chung'}</Badge>
            <h1 className="text-3xl font-bold tracking-tight text-foreground uppercase">{product.name}</h1>
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-1">
                <Star className="h-3.5 w-3.5 fill-yellow-400 text-yellow-400" />
                <span className="text-sm font-bold text-foreground">{product.rating || 0}</span>
              </div>
              <span className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">{reviews.length} đánh giá xác thực</span>
            </div>
          </div>

          <div className="flex items-baseline gap-2">
            <span className="text-4xl font-bold text-foreground tracking-tight">{formatCurrency(product.price)}</span>
            <span className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">VND</span>
            {product.status === ProductStatus.DISCONTINUED && (
              <Badge variant="danger" className="ml-4 px-3 py-1 text-[10px] font-black uppercase tracking-widest animate-pulse">Ngừng kinh doanh</Badge>
            )}
          </div>

          <p className="text-base leading-relaxed text-muted-foreground font-medium">
            {product.description}
          </p>

          <div className="space-y-6 pt-4 border-t border-border">
            <div className="flex items-center gap-8">
              <div className="flex flex-col gap-1.5">
                <span className="text-[9px] font-bold uppercase tracking-widest text-muted-foreground ml-0.5">Số lượng</span>
                <div className="flex items-center gap-1 bg-muted/50 p-1 rounded-xl border border-border/50">
                  <button 
                    onClick={() => setQuantity(Math.max(1, quantity - 1))}
                    disabled={product.status === ProductStatus.DISCONTINUED}
                    className="flex h-9 w-9 items-center justify-center rounded-lg bg-card shadow-sm border border-border transition-colors hover:bg-muted disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <Minus className="h-3.5 w-3.5" />
                  </button>
                  <span className="w-10 text-center font-bold text-base">{quantity}</span>
                  <button 
                    onClick={() => setQuantity(quantity + 1)}
                    disabled={product.status === ProductStatus.DISCONTINUED}
                    className="flex h-9 w-9 items-center justify-center rounded-lg bg-card shadow-sm border border-border transition-colors hover:bg-muted disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <Plus className="h-3.5 w-3.5" />
                  </button>
                </div>
              </div>
              <div className="pt-5">
                <span className="text-[10px] font-medium text-muted-foreground uppercase tracking-widest">
                  Kho: <span className="text-foreground font-bold">{product.stock} sản phẩm</span>
                </span>
              </div>
            </div>

            <div className="flex gap-3">
              <Button 
                size="lg" 
                className="h-12 flex-[2] rounded-xl text-xs font-bold bg-primary text-primary-foreground uppercase tracking-widest hover:opacity-90 transition-all"
                onClick={handleAddToCart}
                disabled={isAdding || product.stock === 0 || product.status === ProductStatus.DISCONTINUED}
              >
                {isAdding ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <ShoppingCart className="mr-2 h-4 w-4" />}
                {product.status === ProductStatus.DISCONTINUED ? 'Không khả dụng' : 'Thêm vào giỏ hàng'}
              </Button>
              <Button 
                size="lg" 
                variant="outline" 
                className="h-12 flex-1 rounded-xl text-xs font-bold border-border uppercase tracking-widest hover:bg-muted transition-all"
                onClick={() => {
                  navigate('/customer/checkout', {
                    state: {
                      items: [{
                        productId: product.id,
                        quantity,
                        productName: product.name,
                        price: product.price,
                        imageUrl: product.imageUrl
                      }],
                      fromCart: false
                    }
                  });
                }}
                disabled={product.stock === 0 || product.status === ProductStatus.DISCONTINUED}
              >
                Mua ngay
              </Button>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="flex items-center gap-3 p-3 rounded-xl bg-muted/30 border border-border/50">
              <Truck className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-[9px] font-bold uppercase tracking-widest text-muted-foreground">Vận chuyển</p>
                <p className="font-bold text-xs uppercase tracking-tight">Giao hàng nhanh</p>
              </div>
            </div>
            <div className="flex items-center gap-3 p-3 rounded-xl bg-muted/30 border border-border/50">
              <ShieldCheck className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-[9px] font-bold uppercase tracking-widest text-muted-foreground">Bảo hành</p>
                <p className="font-bold text-xs uppercase tracking-tight">12 Tháng</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <Tabs defaultValue="reviews" className="w-full mt-10">
        <TabsList className="h-12 w-full justify-start gap-8 border-b border-border bg-transparent p-0">
          <TabsTrigger 
            value="reviews" 
            className="h-12 rounded-none border-b-2 border-transparent px-1 font-bold uppercase tracking-widest text-[10px] text-muted-foreground data-[state=active]:border-primary data-[state=active]:text-foreground transition-all"
          >
            Đánh giá ({reviews.length})
          </TabsTrigger>
          <TabsTrigger 
            value="description" 
            className="h-12 rounded-none border-b-2 border-transparent px-1 font-bold uppercase tracking-widest text-[10px] text-muted-foreground data-[state=active]:border-primary data-[state=active]:text-foreground transition-all"
          >
            Chi tiết
          </TabsTrigger>
        </TabsList>
        
        <TabsContent value="reviews" className="py-8">
          <div className="flex flex-col gap-10 lg:flex-row">
            <div className="w-full lg:w-64">
              <div className="p-6 rounded-2xl bg-muted/30 text-center space-y-1 border border-border/50">
                <div className="text-5xl font-bold text-foreground tracking-tighter">{product.rating || 0}</div>
                <div className="flex justify-center gap-1">
                  {[1,2,3,4,5].map(i => (
                    <Star key={i} className={`h-3.5 w-3.5 ${i <= (product.rating || 0) ? 'fill-yellow-400 text-yellow-400' : 'text-muted'}`} />
                  ))}
                </div>
                <p className="text-[9px] font-bold uppercase tracking-widest text-muted-foreground">Điểm trung bình</p>
              </div>
            </div>

            <div className="flex-1 space-y-6">
              <h3 className="text-xl font-bold uppercase tracking-tight text-foreground">Đánh giá gần đây</h3>
              {isReviewsLoading ? (
                <div className="flex justify-center py-10"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div>
              ) : reviews.length === 0 ? (
                <div className="py-12 text-center text-muted-foreground font-bold uppercase tracking-widest bg-muted/20 rounded-2xl border border-dashed border-border">
                  Chưa có đánh giá nào
                </div>
              ) : (
                <div className="space-y-4">
                  {reviews.map((rev) => (
                    <div key={rev.id} className="p-6 rounded-2xl bg-card border border-border transition-shadow hover:shadow-sm">
                      <div className="space-y-4">
                        <div className="flex items-center justify-between">
                          <div className="flex items-center gap-3">
                            <div className="h-10 w-10 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold text-sm">
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
                          <span className="text-[9px] font-medium text-muted-foreground uppercase tracking-widest">
                            {new Date(rev.createdAt).toLocaleDateString()}
                          </span>
                        </div>
                        <p className="text-sm text-muted-foreground leading-relaxed font-medium italic">
                          "{rev.comment}"
                        </p>

                        {/* Multiple Responses */}
                        {rev.responses && rev.responses.length > 0 ? (
                          <div className="mt-4 space-y-3">
                            {rev.responses.map((resp) => (
                              <div key={resp.id} className="p-4 rounded-xl bg-muted/40 border-l-2 border-primary space-y-1">
                                <div className="flex justify-between items-center">
                                  <p className="text-[9px] font-bold uppercase tracking-widest text-primary">
                                    {resp.userName || 'Phản hồi từ cửa hàng'}
                                  </p>
                                  <span className="text-[8px] text-muted-foreground uppercase font-medium">
                                    {new Date(resp.createdAt).toLocaleDateString()}
                                  </span>
                                </div>
                                <p className="text-xs text-foreground font-medium leading-relaxed italic">"{resp.content}"</p>
                              </div>
                            ))}
                          </div>
                        ) : rev.reply ? (
                          <div className="mt-4 p-4 rounded-xl bg-muted/40 border-l-2 border-primary space-y-1">
                            <p className="text-[9px] font-bold uppercase tracking-widest text-primary">Phản hồi từ cửa hàng</p>
                            <p className="text-xs text-foreground font-medium leading-relaxed italic">"{rev.reply}"</p>
                          </div>
                        ) : null}
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
            <h3 className="text-xl font-bold uppercase tracking-tight text-foreground">Thông số & Đặc điểm</h3>
            <p className="text-base text-muted-foreground leading-relaxed font-medium">
              {product.description}. Tối ưu cho hiệu năng cao và độ bền vượt trội.
            </p>
            <Separator className="bg-border" />
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
               {[
                 'Độ bền tiêu chuẩn công nghiệp',
                 'Nguồn gốc xuất xứ rõ ràng',
                 'Tản nhiệt tối ưu',
                 'Tương thích hoàn hảo'
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
    </div>
  );
}
