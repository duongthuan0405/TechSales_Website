import { useState } from 'react';
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
  Star
} from 'lucide-react';
import { useGetOrder, useCancelOrder } from '../../dataHook/orderDataHook';
import { OrderStatus } from '../../models/ui_types/order';
import { toast } from 'sonner';
import { useAuth } from '../../context/AuthContext';
import { ReviewModal } from '../../components/customer/ReviewModal';
import { Button } from '../../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Badge } from '../../components/ui/badge';

export function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { data: order, isLoading } = useGetOrder(id || '');
  const { mutate: cancelOrder, isPending: isCancelling } = useCancelOrder();
  
  const [reviewItem, setReviewItem] = useState<{ id: string, name: string } | null>(null);

  const handleCancelOrder = () => {
    if (confirm('Are you sure you want to cancel this order?')) {
      cancelOrder(id || '', {
        onSuccess: () => toast.success('Order cancelled'),
        onError: () => toast.error('Failed to cancel order')
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
    { label: 'PENDING', icon: Clock },
    { label: 'APPROVED', icon: CheckCircle2 },
    { label: 'SHIPPING', icon: Truck },
    { label: 'DELIVERED', icon: Package },
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
        <h2 className="text-xl font-bold uppercase tracking-tight">Order Not Found</h2>
        <Button onClick={() => navigate('/customer/orders')} className="mt-4 h-11 px-6 rounded-xl bg-primary text-primary-foreground font-medium uppercase tracking-widest text-xs">Return to Orders</Button>
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
          <h1 className="text-2xl font-bold tracking-tight uppercase">Order Details</h1>
          <p className="text-[10px] font-medium text-muted-foreground tracking-widest uppercase">ID: #{order.id} • {new Date(order.createdAt).toLocaleDateString()}</p>
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
              
              const stepColors: Record<string, string> = {
                'PENDING': 'bg-zinc-500 border-zinc-500 text-white',
                'APPROVED': 'bg-yellow-500 border-yellow-500 text-white',
                'SHIPPING': 'bg-blue-500 border-blue-500 text-white',
                'DELIVERED': 'bg-green-500 border-green-500 text-white',
              };
              
              return (
                <div key={step.label} className="relative z-10 flex flex-col items-center gap-2">
                  <div className={`flex h-10 w-10 items-center justify-center rounded-full border-2 transition-all duration-500 ${
                    isCompleted || isCurrent 
                      ? stepColors[step.label]
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
              <CardTitle className="text-sm font-bold uppercase tracking-widest text-muted-foreground">Shipment Contents</CardTitle>
            </CardHeader>
            <CardContent className="p-6">
              <Table>
                <TableHeader>
                  <TableRow className="border-border hover:bg-transparent">
                    <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Product</TableHead>
                    <TableHead className="text-center font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Qty</TableHead>
                    <TableHead className="text-right font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Amount</TableHead>
                    <TableHead className="text-right font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Action</TableHead>
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
                      <TableCell className="text-right font-bold text-foreground text-sm">${(item.price * item.quantity).toLocaleString()}</TableCell>
                      <TableCell className="text-right">
                        {order.status === OrderStatus.DELIVERED && (
                          <Button 
                            variant="outline" 
                            size="sm" 
                            className="rounded-lg font-bold uppercase tracking-widest text-[8px] h-8 border-border hover:bg-primary hover:text-primary-foreground transition-all"
                            onClick={() => setReviewItem({ id: item.productId, name: item.productName ?? "" })}
                          >
                            Review
                          </Button>
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
                <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Shipping Address</CardTitle>
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
                <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Payment Details</CardTitle>
              </CardHeader>
              <CardContent className="p-6 pt-0 space-y-2">
                <Badge className="bg-primary text-primary-foreground uppercase font-bold text-[8px] tracking-widest px-2 py-0.5 rounded-md">{order.status}</Badge>
                <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-widest">Method: {order.paymentMethodName || order.paymentMethodId}</p>
              </CardContent>
            </Card>
          </div>
        </div>

        <div className="space-y-6">
          <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
            <CardHeader className="p-6 pb-0">
              <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground text-center">Summary</CardTitle>
            </CardHeader>
            <CardContent className="p-6 space-y-3">
              <div className="flex justify-between text-xs font-medium">
                <span className="text-muted-foreground uppercase tracking-widest">Subtotal</span>
                <span className="text-foreground">${order.totalProductAmount?.toLocaleString()}</span>
              </div>
              <div className="flex justify-between text-xs font-medium">
                <span className="text-muted-foreground uppercase tracking-widest">Shipping</span>
                <span className="text-foreground">{order.shippingFee === 0 ? 'FREE' : `$${order.shippingFee?.toLocaleString()}`}</span>
              </div>
              <div className="flex justify-between text-xs font-medium">
                <span className="text-muted-foreground uppercase tracking-widest">Discount</span>
                <span className="text-foreground">-${order.discountAmount?.toLocaleString()}</span>
              </div>
              <div className="border-t border-border pt-4 flex flex-col items-center gap-0.5">
                <span className="text-[8px] font-bold text-muted-foreground uppercase tracking-widest">Total Amount</span>
                <span className="text-3xl font-bold text-foreground tracking-tight">${order.totalAmount?.toLocaleString()}</span>
              </div>
            </CardContent>
          </Card>
          
          <div className="space-y-3">
            <Button variant="outline" className="w-full h-11 rounded-xl font-bold uppercase tracking-widest text-[10px] border-border" onClick={() => window.print()}>
              Export Invoice
            </Button>
            
            {order.status === OrderStatus.PENDING && (
              <Button 
                variant="outline" 
                className="w-full h-11 rounded-xl font-bold uppercase tracking-widest text-[10px] text-destructive border-destructive/20 hover:bg-destructive hover:text-destructive-foreground transition-all"
                onClick={handleCancelOrder}
                disabled={isCancelling}
              >
                Void Order
              </Button>
            )}
          </div>
        </div>
      </div>

      <ReviewModal 
        isOpen={!!reviewItem}
        onClose={() => setReviewItem(null)}
        productId={reviewItem?.id || ''}
        productName={reviewItem?.name || ''}
        userId={user?.id || 'u4'}
        userName={user?.name || 'Demo Customer'}
      />
    </div>
  );
}
