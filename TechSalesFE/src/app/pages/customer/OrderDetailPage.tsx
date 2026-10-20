import { useState } from 'react';
import { formatCurrency } from '../../../utils/format';
import { useParams, useNavigate } from 'react-router';
import { 
  ChevronLeft, 
  Package, 
  Truck, 
  CheckCircle2, 
  Clock, 
  MapPin, 
  CreditCard,
  AlertCircle,
  Loader2,
  Star,
  RotateCcw,
  Edit3,
  Lock
} from 'lucide-react';
import { useGetOrder, useCancelOrder, useRepayOrder, useUpdatePaymentMethod } from '../../../dataHook/orderDataHook';
import { useGetPaymentMethods } from '../../../dataHook/paymentDataHook';
import { useGetReturnsByOrderId } from '../../../dataHook/returnDataHook';
import { OrderStatus } from '../../../models/ui_types/order';
import { ReturnRequest, ReturnStatus } from '../../../models/ui_types/returnRequest';
import { toast } from 'sonner';
import { useAuth } from '../../context/AuthContext';
import { ReviewModal } from '../../components/customer/ReviewModal';
import { CreateReturnModal } from '../../components/customer/CreateReturnModal';
import { ReturnDetailModal } from '../../components/customer/ReturnDetailModal';
import { Button } from '../../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Badge } from '../../components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter
} from '../../components/ui/dialog';

export function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { data: order, isLoading } = useGetOrder(id || '');
  const { data: orderReturns = [] } = useGetReturnsByOrderId(id || '');
  const { data: paymentMethods = [] } = useGetPaymentMethods();
  const { mutate: cancelOrder, isPending: isCancelling } = useCancelOrder();
  const { mutate: repayOrder, isPending: isRepaying } = useRepayOrder();
  const { mutate: updatePaymentMethod, isPending: isUpdatingPayment } = useUpdatePaymentMethod();
  
  const [reviewItem, setReviewItem] = useState<{ id: string, name: string } | null>(null);
  const [returnItem, setReturnItem] = useState<{ productId: string, productName: string, imageUrl?: string, price: number, quantity: number, serialNumbers?: string[] } | null>(null);
  const [viewReturnRequest, setViewReturnRequest] = useState<ReturnRequest | null>(null);
  const [isChangePaymentOpen, setIsChangePaymentOpen] = useState(false);
  const [selectedPaymentMethodId, setSelectedPaymentMethodId] = useState<string>('');

  const getPaymentStatusInfo = (payments?: any[]) => {
    if (!payments || payments.length === 0) {
      return { label: 'CHỜ THANH TOÁN', color: 'bg-amber-500/10 text-amber-500 hover:bg-amber-500/20' };
    }
    
    const hasSuccess = payments.some(p => p.status === 'SUCCESS');
    if (hasSuccess) {
      return { label: 'ĐÃ THANH TOÁN', color: 'bg-emerald-500/10 text-emerald-500 hover:bg-emerald-500/20' };
    }

    const sorted = [...payments].sort((a, b) => new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime());
    const latest = sorted[0];

    if (latest.status === 'FAILED') {
      return { label: 'THANH TOÁN THẤT BÀI', color: 'bg-rose-500/10 text-rose-500 hover:bg-rose-500/20' };
    }
    if (latest.status === 'CANCELLED') {
      return { label: 'ĐÃ HỦY', color: 'bg-neutral-500/10 text-neutral-500 hover:bg-neutral-500/20' };
    }
    return { label: 'CHỜ THANH TOÁN', color: 'bg-amber-500/10 text-amber-500 hover:bg-amber-500/20' };
  };

  const handleCancelOrder = () => {
    if (confirm('Bạn có chắc chắn muốn hủy đơn hàng này không?')) {
      cancelOrder(id || '', {
        onSuccess: () => toast.success('Đã hủy đơn hàng'),
        onError: () => toast.error('Hủy đơn hàng thất bại')
      });
    }
  };

  const getStatusStep = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.PENDING: return 1;
      case OrderStatus.APPROVED: return 2;
      case OrderStatus.SHIPPING: return 3;
      case OrderStatus.DELIVERED: return 4;
      default: return 0;
    }
  };

  const steps = [
    { label: 'CHỜ XỬ LÝ', icon: Clock },
    { label: 'ĐÃ DUYỆT', icon: CheckCircle2 },
    { label: 'ĐANG GIAO', icon: Truck },
    { label: 'ĐÃ GIAO', icon: Package },
  ];

  const currentStep = order ? getStatusStep(order.status) : 0;

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!order) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center mx-auto max-w-md">
        <AlertCircle className="mb-4 h-10 w-10 text-muted" />
        <h2 className="text-xl font-bold uppercase tracking-tight">Không tìm thấy đơn hàng</h2>
        <Button onClick={() => navigate('/customer/orders')} className="mt-4 h-11 px-6 rounded-xl bg-primary text-primary-foreground font-medium uppercase tracking-widest text-xs">Quay lại đơn hàng</Button>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-5xl space-y-8 pb-16 px-4 md:px-0">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)} className="h-10 w-10 rounded-xl hover:bg-muted/50">
          <ChevronLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight uppercase">Chi tiết đơn hàng</h1>
          <p className="text-[10px] font-medium text-muted-foreground tracking-widest uppercase">Mã: #{order.id} • {new Date(order.createdAt).toLocaleDateString()}</p>
        </div>
      </div>

      {/* Tracking Timeline */}
      <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
        <CardContent className="p-8">
          <div className="relative flex justify-between">
            {/* Background Line */}
            <div className="absolute top-1/2 left-0 h-1 w-full -translate-y-1/2 bg-muted rounded-full" />
            
            {/* Progress Line */}
            <div 
              className="absolute top-1/2 left-0 h-1 -translate-y-1/2 bg-primary rounded-full transition-all duration-700" 
              style={{ width: `${Math.max(0, (currentStep - 1) / (steps.length - 1) * 100)}%` }}
            />

            {steps.map((step, idx) => {
              const isCompleted = currentStep > idx;
              const isCurrent = currentStep === idx + 1;
              const Icon = step.icon;
              
              return (
                <div key={step.label} className="relative z-10 flex flex-col items-center gap-2">
                  <div className={`flex h-10 w-10 items-center justify-center rounded-full border-2 transition-all duration-500 ${
                    isCompleted || isCurrent 
                      ? 'bg-primary border-primary text-primary-foreground' 
                      : 'bg-card border-border text-muted-foreground'
                  } ${isCurrent ? 'ring-4 ring-muted' : ''}`}>
                    <Icon className="h-5 w-5" />
                  </div>
                  <span className={`text-[10px] font-bold uppercase tracking-widest ${isCompleted || isCurrent ? 'text-foreground' : 'text-muted-foreground'}`}>
                    {step.label}
                  </span>
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-8">
          <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
            <CardHeader className="p-6 pb-0">
              <CardTitle className="text-sm font-bold uppercase tracking-widest text-muted-foreground">Danh sách sản phẩm</CardTitle>
            </CardHeader>
            <CardContent className="p-6">
              <Table>
                <TableHeader>
                  <TableRow className="border-border hover:bg-transparent">
                    <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Sản phẩm</TableHead>
                    <TableHead className="text-center font-bold uppercase tracking-widest text-[9px] text-muted-foreground">SL</TableHead>
                    <TableHead className="text-right font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Thành tiền</TableHead>
                    <TableHead className="text-right font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Hành động</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {order.items?.map((item) => (
                    <TableRow key={item.productId} className="border-border">
                      <TableCell className="py-4">
                        <div className="flex items-center gap-3">
                          <img 
                            src={item.imageUrl || ''} 
                            alt={item.productName} 
                            className="h-10 w-10 rounded-lg object-cover bg-muted grayscale opacity-80" 
                          />
                          <span className="font-medium text-foreground text-xs">{item.productName}</span>
                        </div>
                      </TableCell>
                      <TableCell className="text-center font-normal text-muted-foreground text-xs">{item.quantity}</TableCell>
                      <TableCell className="text-right font-bold text-foreground text-sm">{formatCurrency(item.price * item.quantity)}</TableCell>
                      <TableCell className="text-right">
                        {order.status === OrderStatus.DELIVERED ? (
                          <div className="flex items-center justify-end gap-2">
                            {(() => {
                              const itemReturn = orderReturns.find(r => r.productId === item.productId);
                              if (itemReturn) {
                                return (
                                  <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => setViewReturnRequest(itemReturn)}
                                    className="h-8 rounded-lg px-2.5 text-[8px] font-bold uppercase tracking-wider border-border hover:border-primary transition-all flex items-center gap-1.5"
                                  >
                                    <RotateCcw className="h-3 w-3 text-primary" />
                                    {itemReturn.status === ReturnStatus.PENDING_APPROVAL && (
                                      <span className="text-amber-500">Chờ duyệt</span>
                                    )}
                                    {itemReturn.status === ReturnStatus.AWAITING_RETURN && (
                                      <span className="text-blue-500 font-bold">Gửi hàng về</span>
                                    )}
                                    {itemReturn.status === ReturnStatus.INSPECTING && (
                                      <span className="text-purple-500 font-bold">Đang kiểm tra</span>
                                    )}
                                    {itemReturn.status === ReturnStatus.COMPLETED && (
                                      <span className="text-emerald-500 font-bold">Đã hoàn tiền</span>
                                    )}
                                    {itemReturn.status === ReturnStatus.REJECTED && (
                                      <span className="text-destructive font-bold">Từ chối trả</span>
                                    )}
                                  </Button>
                                );
                              }

                              return (
                                <Button 
                                  variant="outline" 
                                  size="sm" 
                                  className="rounded-lg font-bold uppercase tracking-widest text-[8px] h-8 border-border text-primary hover:bg-primary/10 transition-all flex items-center gap-1"
                                  onClick={() => setReturnItem({
                                    productId: item.productId,
                                    productName: item.productName ?? '',
                                    imageUrl: item.imageUrl,
                                    price: item.price,
                                    quantity: item.quantity,
                                    serialNumbers: item.serialNumbers
                                  })}
                                >
                                  <RotateCcw className="h-3 w-3" />
                                  Hoàn trả
                                </Button>
                              );
                            })()}

                            <Button 
                              variant="outline" 
                              size="sm" 
                              className="rounded-lg font-bold uppercase tracking-widest text-[8px] h-8 border-border hover:bg-primary hover:text-primary-foreground transition-all"
                              onClick={() => setReviewItem({ id: item.productId, name: item.productName ?? "" })}
                            >
                              Đánh giá
                            </Button>
                          </div>
                        ) : (
                          <span className="text-[10px] text-muted-foreground/60 italic block text-right">
                            {order.status === OrderStatus.CANCELLED ? 'Đã hủy đơn' : 'Hoàn trả khi đã giao'}
                          </span>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>

          <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
            <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
              <CardHeader className="p-6 pb-2 flex flex-row items-center gap-2">
                <MapPin className="h-4 w-4 text-muted-foreground" />
                <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Địa chỉ nhận hàng</CardTitle>
              </CardHeader>
              <CardContent className="p-6 pt-0">
                <p className="text-xs text-foreground font-medium leading-relaxed italic">
                  {order.shippingAddressSnapshot}
                </p>
              </CardContent>
            </Card>
            <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
              <CardHeader className="p-6 pb-2 flex flex-row items-center gap-2">
                <CreditCard className="h-4 w-4 text-muted-foreground" />
                <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Thông tin thanh toán</CardTitle>
              </CardHeader>
              <CardContent className="p-6 pt-0 space-y-3">
                <div className="flex flex-wrap items-center gap-2">
                  <div className="flex flex-col gap-1">
                    <span className="text-[8px] font-bold text-muted-foreground uppercase tracking-widest">Trạng thái đơn hàng</span>
                    <Badge className="bg-primary/10 text-primary uppercase font-bold text-[8px] tracking-widest px-2 py-0.5 rounded-md w-fit hover:bg-primary/20 transition-all">{order.status}</Badge>
                  </div>
                  <div className="flex flex-col gap-1">
                    <span className="text-[8px] font-bold text-muted-foreground uppercase tracking-widest">Trạng thái thanh toán</span>
                    <Badge className={`uppercase font-bold text-[8px] tracking-widest px-2 py-0.5 rounded-md w-fit transition-all ${getPaymentStatusInfo(order.payments).color}`}>
                      {getPaymentStatusInfo(order.payments).label}
                    </Badge>
                  </div>
                </div>
                
                <div className="flex items-center justify-between pt-1">
                  <div>
                    <span className="text-[8px] font-bold text-muted-foreground uppercase tracking-widest block">Phương thức thanh toán</span>
                    <p className="text-xs font-bold text-foreground mt-0.5">{order.paymentMethodName || 'Không xác định'}</p>
                  </div>
                  {order.status === OrderStatus.PENDING ? (
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => {
                        const currentPm = paymentMethods.find(p => p.name === order.paymentMethodName);
                        setSelectedPaymentMethodId(order.paymentMethodId || currentPm?.id || paymentMethods[0]?.id || '');
                        setIsChangePaymentOpen(true);
                      }}
                      className="h-8 px-2.5 rounded-lg text-[9px] font-bold uppercase tracking-wider border-border hover:border-primary text-primary hover:bg-primary/10 transition-all flex items-center gap-1.5"
                    >
                      <Edit3 className="h-3 w-3" />
                      Đổi phương thức
                    </Button>
                  ) : (
                    <span className="text-[9px] text-muted-foreground flex items-center gap-1 font-medium italic">
                      <Lock className="h-3 w-3" /> Đã duyệt (Cố định)
                    </span>
                  )}
                </div>

                {order.status !== OrderStatus.PENDING && (
                  <p className="text-[10px] text-muted-foreground/80 italic">
                    * Phương thức thanh toán chỉ có thể thay đổi khi đơn hàng chưa được duyệt.
                  </p>
                )}

                {order.status === OrderStatus.PENDING && 
                 order.paymentMethodName && 
                 !['cod', 'cash', 'cash on delivery', 'tiền mặt', 'thanh toán khi nhận hàng'].includes(order.paymentMethodName.toLowerCase().trim()) && 
                 !order.payments?.some(p => p.status === 'SUCCESS') && (
                  <Button 
                    onClick={() => {
                      repayOrder({ id: order.id }, {
                        onSuccess: (checkoutUrl) => {
                          if (checkoutUrl) {
                            toast.success('Đang chuyển hướng đến cổng thanh toán...');
                            window.location.href = checkoutUrl;
                          } else {
                            toast.error('Không thể lấy liên kết thanh toán');
                          }
                        },
                        onError: (err: any) => {
                          toast.error(err.message || 'Không thể khởi tạo thanh toán lại');
                        }
                      });
                    }}
                    disabled={isRepaying}
                    className="w-full mt-2 h-10 rounded-xl font-bold uppercase tracking-widest text-[9px] bg-gradient-to-r from-violet-600 to-indigo-600 hover:from-violet-700 hover:to-indigo-700 text-white shadow-md hover:shadow-lg active:scale-[0.98] transition-all flex items-center justify-center gap-2"
                  >
                    {isRepaying ? (
                      <>
                        <Loader2 className="h-3 w-3 animate-spin text-white" />
                        Đang tạo liên kết...
                      </>
                    ) : (
                      <>
                        <CreditCard className="h-3.5 w-3.5 text-white animate-pulse" />
                        Thanh toán ngay
                      </>
                    )}
                  </Button>
                )}
              </CardContent>
            </Card>
          </div>

          {/* Return Requests Card (only for DELIVERED orders that have returns) */}
          {order.status === OrderStatus.DELIVERED && orderReturns.length > 0 && (
            <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden mt-6">
              <CardHeader className="p-6 pb-3 border-b border-border/50 flex flex-row items-center justify-between">
                <div className="flex items-center gap-2">
                  <RotateCcw className="h-4 w-4 text-primary" />
                  <CardTitle className="text-xs font-bold uppercase tracking-widest text-foreground">
                    Yêu Cầu Hoàn Trả Hàng ({orderReturns.length})
                  </CardTitle>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-[10px] text-muted-foreground uppercase tracking-widest font-medium hidden sm:inline">Tiến trình xử lý</span>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => navigate('/customer/returns')}
                    className="h-7 px-2 text-[10px] font-bold uppercase tracking-wider text-primary hover:bg-primary/10 flex items-center gap-1"
                  >
                    Tất cả yêu cầu →
                  </Button>
                </div>
              </CardHeader>
              <CardContent className="p-6 space-y-3">
                {orderReturns.map((ret) => (
                  <div 
                    key={ret.id} 
                    className="p-4 rounded-xl border border-border bg-muted/20 flex flex-col sm:flex-row sm:items-center justify-between gap-4 hover:border-primary/40 transition-all cursor-pointer"
                    onClick={() => setViewReturnRequest(ret)}
                  >
                    <div className="flex items-center gap-3">
                      <img
                        src={ret.productImageUrl || ''}
                        alt={ret.productName}
                        className="h-12 w-12 rounded-lg object-cover border border-border shrink-0"
                      />
                      <div>
                        <div className="flex items-center gap-2">
                          <span className="text-[10px] font-mono font-bold text-muted-foreground">#{ret.id}</span>
                          <span className="text-xs font-bold text-foreground">{ret.productName}</span>
                        </div>
                        <p className="text-[11px] text-muted-foreground mt-0.5">
                          Lý do: <span className="text-foreground font-medium">{ret.reason}</span> • SL: {ret.quantity}
                        </p>
                      </div>
                    </div>

                    <div className="flex items-center justify-between sm:justify-end gap-3 shrink-0">
                      <div className="text-right">
                        <span className="text-[10px] text-muted-foreground uppercase tracking-wider block">Tiền hoàn</span>
                        <span className="text-sm font-black text-primary">{formatCurrency(ret.totalRefundAmount)}</span>
                      </div>
                      
                      <div className="flex items-center gap-2">
                        {ret.status === ReturnStatus.PENDING_APPROVAL && (
                          <Badge className="bg-amber-500/10 text-amber-500 border border-amber-500/20 text-[9px] font-bold uppercase">
                            Chờ duyệt
                          </Badge>
                        )}
                        {ret.status === ReturnStatus.AWAITING_RETURN && (
                          <Badge className="bg-blue-500/10 text-blue-500 border border-blue-500/20 text-[9px] font-bold uppercase">
                            Gửi hàng về
                          </Badge>
                        )}
                        {ret.status === ReturnStatus.INSPECTING && (
                          <Badge className="bg-purple-500/10 text-purple-500 border border-purple-500/20 text-[9px] font-bold uppercase">
                            Đang kiểm tra
                          </Badge>
                        )}
                        {ret.status === ReturnStatus.COMPLETED && (
                          <Badge className="bg-emerald-500/10 text-emerald-500 border border-emerald-500/20 text-[9px] font-bold uppercase">
                            Đã hoàn tiền
                          </Badge>
                        )}
                        {ret.status === ReturnStatus.REJECTED && (
                          <Badge className="bg-rose-500/10 text-rose-500 border border-rose-500/20 text-[9px] font-bold uppercase">
                            Từ chối
                          </Badge>
                        )}
                        <Button
                          variant="ghost"
                          size="sm"
                          className="h-8 px-2 text-[10px] font-bold uppercase tracking-wider text-primary hover:bg-primary/10"
                        >
                          Chi tiết
                        </Button>
                      </div>
                    </div>
                  </div>
                ))}
              </CardContent>
            </Card>
          )}
        </div>

        <div className="space-y-6">
          <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
            <CardHeader className="p-6 pb-0">
              <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground text-center">Tóm tắt đơn hàng</CardTitle>
            </CardHeader>
            <CardContent className="p-6 space-y-3">
              <div className="flex justify-between text-xs font-medium">
                <span className="text-muted-foreground uppercase tracking-widest">Tạm tính</span>
                <span className="text-foreground">{formatCurrency(order.totalProductAmount || 0)}</span>
              </div>
              <div className="flex justify-between text-xs font-medium">
                <span className="text-muted-foreground uppercase tracking-widest">Vận chuyển</span>
                <span className="text-foreground">{order.shippingFee === 0 ? 'MIỄN PHÍ' : formatCurrency(order.shippingFee || 0)}</span>
              </div>
              <div className="flex justify-between text-xs font-medium">
                <span className="text-muted-foreground uppercase tracking-widest">Giảm giá</span>
                <span className="text-foreground">-{formatCurrency(order.discountAmount || 0)}</span>
              </div>
              <div className="border-t border-border pt-4 flex flex-col items-center gap-0.5">
                <span className="text-[8px] font-bold text-muted-foreground uppercase tracking-widest">Tổng thanh toán</span>
                <span className="text-3xl font-bold text-foreground tracking-tight">{formatCurrency(order.totalAmount || 0)}</span>
              </div>
            </CardContent>
          </Card>
          
          <div className="space-y-3">
            <Button variant="outline" className="w-full h-11 rounded-xl font-bold uppercase tracking-widest text-[10px] border-border" onClick={() => window.print()}>
              In hóa đơn
            </Button>
            
            {order.status === OrderStatus.PENDING && (
              <Button 
                variant="outline" 
                className="w-full h-11 rounded-xl font-bold uppercase tracking-widest text-[10px] text-destructive border-destructive/20 hover:bg-destructive hover:text-destructive-foreground transition-all"
                onClick={handleCancelOrder}
                disabled={isCancelling}
              >
                Hủy đơn hàng
              </Button>
            )}
          </div>
        </div>
      </div>

      <ReviewModal 
        isOpen={!!reviewItem}
        onClose={() => setReviewItem(null)}
        orderId={order.id}
        productId={reviewItem?.id || ''}
        productName={reviewItem?.name || ''}
      />

      <CreateReturnModal 
        isOpen={!!returnItem}
        onClose={() => setReturnItem(null)}
        orderId={order.id}
        orderStatus={order.status}
        item={returnItem}
      />

      <ReturnDetailModal
        isOpen={!!viewReturnRequest}
        onClose={() => setViewReturnRequest(null)}
        returnRequest={viewReturnRequest}
      />

      {/* Modal Thay đổi Phương thức Thanh toán */}
      <Dialog open={isChangePaymentOpen} onOpenChange={setIsChangePaymentOpen}>
        <DialogContent className="max-w-md rounded-2xl bg-card border-border shadow-2xl p-6">
          <DialogHeader className="space-y-1">
            <div className="flex items-center gap-2">
              <div className="p-2 rounded-xl bg-primary/10 text-primary">
                <CreditCard className="h-5 w-5" />
              </div>
              <DialogTitle className="text-sm font-bold uppercase tracking-tight">
                Đổi phương thức thanh toán
              </DialogTitle>
            </div>
            <DialogDescription className="text-xs text-muted-foreground leading-relaxed">
              Đơn hàng <strong>#{order.id}</strong> đang ở trạng thái <strong className="text-amber-500">CHỜ XỬ LÝ (CHƯA DUYỆT)</strong>. Bạn có thể thay đổi phương thức thanh toán trước khi đơn được nhân viên duyệt.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-2 py-3">
            {paymentMethods.map((method) => {
              const isSelected = selectedPaymentMethodId === method.id;
              return (
                <div
                  key={method.id}
                  onClick={() => setSelectedPaymentMethodId(method.id)}
                  className={`p-3 rounded-xl border-2 cursor-pointer transition-all flex items-start gap-3 ${
                    isSelected
                      ? 'border-primary bg-primary/5 shadow-sm'
                      : 'border-border/60 hover:border-border hover:bg-muted/30'
                  }`}
                >
                  <div className={`mt-0.5 h-4 w-4 rounded-full border-2 flex items-center justify-center shrink-0 ${
                    isSelected ? 'border-primary bg-primary text-primary-foreground' : 'border-muted-foreground/40'
                  }`}>
                    {isSelected && <div className="h-1.5 w-1.5 rounded-full bg-white" />}
                  </div>
                  <div className="space-y-0.5 flex-1">
                    <p className={`text-xs font-bold ${isSelected ? 'text-primary' : 'text-foreground'}`}>
                      {method.name}
                    </p>
                    {method.description && (
                      <p className="text-[10px] text-muted-foreground leading-relaxed">
                        {method.description}
                      </p>
                    )}
                  </div>
                </div>
              );
            })}
          </div>

          <DialogFooter className="gap-2 sm:gap-0">
            <Button
              variant="outline"
              onClick={() => setIsChangePaymentOpen(false)}
              disabled={isUpdatingPayment}
              className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider border-border"
            >
              Hủy
            </Button>
            <Button
              onClick={() => {
                if (!selectedPaymentMethodId) {
                  toast.error('Vui lòng chọn một phương thức thanh toán');
                  return;
                }
                updatePaymentMethod(
                  { orderId: order.id, paymentMethodId: selectedPaymentMethodId },
                  {
                    onSuccess: () => {
                      toast.success('Cập nhật phương thức thanh toán thành công!');
                      setIsChangePaymentOpen(false);
                    },
                    onError: (err: any) => {
                      toast.error(err.message || 'Không thể cập nhật phương thức thanh toán');
                    }
                  }
                );
              }}
              disabled={isUpdatingPayment || !selectedPaymentMethodId}
              className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider bg-primary text-primary-foreground"
            >
              {isUpdatingPayment ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin mr-1.5" />
                  Đang lưu...
                </>
              ) : (
                'Xác nhận đổi'
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
