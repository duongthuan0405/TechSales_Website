import { useParams, useNavigate } from 'react-router';
import { useState } from 'react';
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
  XCircle,
  Printer,
  History,
  AlertTriangle,
  RotateCcw,
  Info,
  Edit3,
  Lock,
  Barcode
} from 'lucide-react';
import { useGetAdminOrder, useUpdateOrderStatus, useUpdatePaymentMethod } from '../../../dataHook/orderDataHook';
import { useGetPaymentMethods } from '../../../dataHook/paymentDataHook';
import { useGetReturnsByOrderId } from '../../../dataHook/returnDataHook';
import { Order, OrderStatus } from '../../../models/ui_types/order';
import { OrderSerialApprovalModal } from '../../components/sales/OrderSerialApprovalModal';
import { toast } from 'sonner';
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

interface OrderManagementDetailPageProps {
  readOnly?: boolean;
}

export function OrderManagementDetailPage({ readOnly = false }: OrderManagementDetailPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: order, isLoading } = useGetAdminOrder(id || '');
  const { data: orderReturns = [] } = useGetReturnsByOrderId(id || '');
  const { mutate: updateStatus, isPending: isUpdating } = useUpdateOrderStatus();
  const { data: paymentMethods = [] } = useGetPaymentMethods();
  const { mutate: updatePaymentMethod, isPending: isUpdatingPayment } = useUpdatePaymentMethod();
  const [isChangePaymentOpen, setIsChangePaymentOpen] = useState(false);
  const [selectedPaymentMethodId, setSelectedPaymentMethodId] = useState<string>('');

  // Confirmation State
  const [confirmingStatus, setConfirmingStatus] = useState<OrderStatus | null>(null);
  const [showSerialModal, setShowSerialModal] = useState(false);

  const handleStatusChange = () => {
    if (!confirmingStatus) return;
    
    updateStatus({ id: id || '', status: confirmingStatus }, {
      onSuccess: () => {
        toast.success(`Order protocol updated to ${confirmingStatus}`);
        setConfirmingStatus(null);
      },
      onError: (err: any) => {
        toast.error(err.message || 'Failed to update protocol');
        setConfirmingStatus(null);
      }
    });
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
        <h2 className="text-xl font-bold uppercase tracking-tight">Protocol Error: Order Not Found</h2>
        <Button onClick={() => navigate('/sales/orders')} className="mt-4 h-11 px-6 rounded-xl bg-primary text-primary-foreground font-medium uppercase tracking-widest text-xs">Return to Terminal</Button>
      </div>
    );
  }

  const getStatusVariant = (status: string) => {
    switch (status.toUpperCase()) {
      case 'DELIVERED': return 'success';
      case 'SHIPPING': return 'info';
      case 'APPROVED': return 'warning';
      case 'PENDING': return 'pending';
      case 'CANCELLED': return 'danger';
      default: return 'default';
    }
  };

  const getConfirmationDetails = () => {
    if (!confirmingStatus) return { title: '', description: '', actionColor: 'bg-primary' };
    
    switch (confirmingStatus) {
      case OrderStatus.APPROVED:
        return {
          title: 'Approve Order Protocol?',
          description: `Confirm preparation and fulfillment for order ${order.id}.`,
          actionColor: 'bg-emerald-500 hover:bg-emerald-600'
        };
      case OrderStatus.SHIPPING:
        return {
          title: 'Initiate Shipment?',
          description: `Register that order ${order.id} has departed for delivery.`,
          actionColor: 'bg-blue-500 hover:bg-blue-600'
        };
      case OrderStatus.DELIVERED:
        return {
          title: 'Confirm Delivery?',
          description: `Finalize protocol for order ${order.id} as successfully delivered.`,
          actionColor: 'bg-green-500 hover:bg-green-600'
        };
      case OrderStatus.CANCELLED:
        return {
          title: 'Void Order Protocol?',
          description: `Immediate cancellation of order ${order.id}. This action is logged as a critical override.`,
          actionColor: 'bg-destructive hover:bg-destructive/90'
        };
      default:
        return { title: 'Confirm Action', description: 'Are you sure?', actionColor: 'bg-primary' };
    }
  };

  const conf = getConfirmationDetails();

  return (
    <div className="mx-auto max-w-5xl space-y-8 pb-16">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate(-1)} className="h-10 w-10 rounded-xl hover:bg-muted/50">
            <ChevronLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold tracking-tight uppercase">Order Processing</h1>
            <p className="text-[10px] font-medium text-muted-foreground tracking-widest uppercase">Protocol #{order.id} • {new Date(order.createdAt).toLocaleDateString()}</p>
          </div>
        </div>
        <div className="flex gap-2">
           <Button variant="outline" className="h-10 px-4 rounded-xl text-[10px] font-bold uppercase tracking-widest border-border" onClick={() => window.print()}>
             <Printer className="h-3.5 w-3.5 mr-2" /> Print Invoice
           </Button>
           <Badge variant={getStatusVariant(order.status)} className="h-10 px-4 flex items-center font-black uppercase tracking-widest text-[10px] rounded-xl">
             {order.status}
           </Badge>
        </div>
      </div>

      {/* Tracking Timeline */}
      <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
        <CardContent className="p-8">
          <div className="relative flex justify-between">
            <div className="absolute top-1/2 left-0 h-1 w-full -translate-y-1/2 bg-muted rounded-full" />
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
          {/* Action Controls */}
          {!readOnly && (
            <Card className="border-primary/20 border shadow-lg bg-primary/[0.02] rounded-2xl overflow-hidden">
              <CardHeader className="p-6 border-b border-primary/10">
                <CardTitle className="text-xs font-bold uppercase tracking-widest text-primary">Protocol Management Actions</CardTitle>
              </CardHeader>
              <CardContent className="p-6">
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                  <Button 
                    className="flex flex-col h-20 gap-2 rounded-xl bg-emerald-500 hover:bg-emerald-600 text-white disabled:bg-zinc-100 disabled:text-zinc-400 disabled:border-zinc-200 disabled:shadow-none"
                    disabled={order.status !== OrderStatus.PENDING || isUpdating}
                    onClick={() => setShowSerialModal(true)}
                  >
                    <Barcode className="h-5 w-5" />
                    <span className="text-[9px] font-bold uppercase tracking-widest">Approve + Serie</span>
                  </Button>
                  <Button 
                    className="flex flex-col h-20 gap-2 rounded-xl bg-blue-500 hover:bg-blue-600 text-white disabled:bg-zinc-100 disabled:text-zinc-400 disabled:border-zinc-200 disabled:shadow-none"
                    disabled={order.status !== OrderStatus.APPROVED || isUpdating}
                    onClick={() => setConfirmingStatus(OrderStatus.SHIPPING)}
                  >
                    <Truck className="h-5 w-5" />
                    <span className="text-[9px] font-bold uppercase tracking-widest">Ship Order</span>
                  </Button>
                  <Button 
                    className="flex flex-col h-20 gap-2 rounded-xl bg-green-500 hover:bg-green-600 text-white disabled:bg-zinc-100 disabled:text-zinc-400 disabled:border-zinc-200 disabled:shadow-none"
                    disabled={order.status !== OrderStatus.SHIPPING || isUpdating}
                    onClick={() => setConfirmingStatus(OrderStatus.DELIVERED)}
                  >
                    <Package className="h-5 w-5" />
                    <span className="text-[9px] font-bold uppercase tracking-widest">Deliver</span>
                  </Button>
                  <Button 
                    className="flex flex-col h-20 gap-2 rounded-xl bg-destructive hover:bg-destructive/90 text-white disabled:bg-zinc-100 disabled:text-zinc-400 disabled:border-zinc-200 disabled:shadow-none"
                    disabled={(order.status === OrderStatus.DELIVERED || order.status === OrderStatus.SHIPPING || order.status === OrderStatus.CANCELLED) || isUpdating}
                    onClick={() => setConfirmingStatus(OrderStatus.CANCELLED)}
                    title={order.status === OrderStatus.DELIVERED || order.status === OrderStatus.SHIPPING ? 'Không thể hủy đơn khi đang/đã giao' : 'Hủy đơn hàng'}
                  >
                    <XCircle className="h-5 w-5" />
                    <span className="text-[9px] font-bold uppercase tracking-widest">Void Protocol</span>
                  </Button>
                </div>

                {(order.status === OrderStatus.DELIVERED || order.status === OrderStatus.SHIPPING) && (
                  <div className="mt-4 p-3 rounded-xl border border-amber-500/20 bg-amber-500/5 flex items-start gap-2.5">
                    <Info className="h-4 w-4 text-amber-500 shrink-0 mt-0.5" />
                    <div className="text-[11px] text-muted-foreground leading-relaxed">
                      <strong className="text-foreground font-semibold">Chính sách hủy đơn:</strong> Nhân viên không thể hủy đơn hàng khi đơn đang giao ({order.status === OrderStatus.SHIPPING ? 'SHIPPING' : ''}) hoặc đã được giao ({order.status === OrderStatus.DELIVERED ? 'DELIVERED' : ''}). Nếu khách hàng có nhu cầu đổi trả sản phẩm, quy trình <strong className="text-primary font-semibold">Hoàn trả hàng (Return Protocol)</strong> sẽ được áp dụng.
                    </div>
                  </div>
                )}

                {order.status === OrderStatus.DELIVERED && orderReturns.length > 0 && (
                  <div className="mt-3 p-3.5 rounded-xl border border-primary/30 bg-primary/5 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                    <div className="flex items-center gap-2.5">
                      <div className="p-1.5 rounded-lg bg-primary/10 text-primary">
                        <RotateCcw className="h-4 w-4" />
                      </div>
                      <div>
                        <span className="text-xs font-bold text-foreground block">
                          Đơn hàng có {orderReturns.length} yêu cầu hoàn trả
                        </span>
                        <span className="text-[10px] text-muted-foreground">
                          Trạng thái: {orderReturns.map(r => r.status).join(', ')}
                        </span>
                      </div>
                    </div>
                    <Button
                      size="sm"
                      onClick={() => navigate('/sales/returns')}
                      className="h-8 rounded-lg text-[10px] font-bold uppercase tracking-wider bg-primary text-primary-foreground hover:bg-primary/90 shrink-0"
                    >
                      Xử lý hoàn trả →
                    </Button>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
            <CardHeader className="p-6 pb-0">
              <div className="flex items-center justify-between">
                <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Shipment Contents</CardTitle>
                {order.status === OrderStatus.PENDING && (
                  <span className="text-[10px] font-bold text-amber-500 flex items-center gap-1">
                    <Barcode className="h-3.5 w-3.5" />
                    Chưa gán Serial (cần duyệt để nhập)
                  </span>
                )}
                {order.status !== OrderStatus.PENDING && (
                  <span className="text-[10px] font-bold text-emerald-600 dark:text-emerald-400 flex items-center gap-1">
                    <Barcode className="h-3.5 w-3.5" />
                    {order.items?.reduce((sum, i) => sum + (i.serialNumbers?.length || 0), 0)} Serial đã gán
                  </span>
                )}
              </div>
            </CardHeader>
            <CardContent className="p-6">
              <Table>
                <TableHeader>
                  <TableRow className="border-border hover:bg-transparent">
                    <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Product</TableHead>
                    <TableHead className="text-center font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Qty</TableHead>
                    <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Serial Numbers</TableHead>
                    <TableHead className="text-right font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Total</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {order.items?.map((item) => (
                    <TableRow key={item.productId} className="border-border">
                      <TableCell className="py-4">
                        <div className="flex items-center gap-3">
                          <img src={item.imageUrl || ''} alt={item.productName} className="h-10 w-10 rounded-lg object-cover bg-muted grayscale opacity-80" />
                          <div>
                            <span className="font-bold text-foreground text-xs uppercase tracking-tight block">{item.productName}</span>
                            <span className="text-[9px] text-muted-foreground">${item.price.toLocaleString()} / cái</span>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell className="text-center font-bold text-muted-foreground text-xs">{item.quantity}</TableCell>
                      <TableCell className="py-3">
                        {item.serialNumbers && item.serialNumbers.length > 0 ? (
                          <div className="flex flex-wrap gap-1">
                            {item.serialNumbers.map((sn) => (
                              <span key={sn} className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-emerald-500/10 text-emerald-700 dark:text-emerald-400 text-[9px] font-mono font-bold border border-emerald-500/20">
                                <Barcode className="h-2.5 w-2.5" />
                                {sn}
                              </span>
                            ))}
                          </div>
                        ) : (
                          <span className="text-[10px] text-muted-foreground italic flex items-center gap-1">
                            <AlertTriangle className="h-3 w-3 text-amber-500" />
                            {item.quantity} serial chưa được gán
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="text-right font-black text-foreground text-sm">${(item.price * item.quantity).toLocaleString()}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
            <CardHeader className="p-6 pb-2 flex flex-row items-center gap-2">
              <History className="h-4 w-4 text-muted-foreground" />
              <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Audit Log Preview</CardTitle>
            </CardHeader>
            <CardContent className="p-6 pt-0 space-y-4">
              <div className="space-y-3">
                 <div className="flex gap-3">
                   <div className="h-2 w-2 rounded-full bg-primary mt-1.5 shrink-0" />
                   <div>
                     <p className="text-[10px] font-bold uppercase tracking-tight">System Initialization</p>
                     <p className="text-[9px] text-muted-foreground">{new Date(order.createdAt).toLocaleString()}</p>
                   </div>
                 </div>
                 {order.status !== OrderStatus.PENDING && (
                   <div className="flex gap-3">
                     <div className="h-2 w-2 rounded-full bg-emerald-500 mt-1.5 shrink-0" />
                     <div>
                       <p className="text-[10px] font-bold uppercase tracking-tight">Status Update: {order.status}</p>
                       <p className="text-[9px] text-muted-foreground">Admin Overridden</p>
                     </div>
                   </div>
                 )}
              </div>
            </CardContent>
          </Card>

          <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
            <CardHeader className="p-6 pb-2 flex flex-row items-center gap-2">
              <MapPin className="h-4 w-4 text-muted-foreground" />
              <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Customer Protocol</CardTitle>
            </CardHeader>
            <CardContent className="p-6 pt-0 space-y-4">
              <div className="space-y-1">
                <p className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground opacity-60">Identity</p>
                <p className="text-xs font-black uppercase tracking-tight">{order.customerName}</p>
              </div>
              <div className="space-y-1">
                <p className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground opacity-60">Logistics Destination</p>
                <p className="text-xs font-medium text-foreground italic">{order.shippingAddressSnapshot}</p>
              </div>
              <div className="space-y-1">
                <div className="flex items-center justify-between">
                  <p className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground opacity-60">Transaction Method</p>
                  {order.status === OrderStatus.PENDING && !readOnly ? (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => {
                        const currentPm = paymentMethods.find(p => p.name === order.paymentMethodName);
                        setSelectedPaymentMethodId(order.paymentMethodId || currentPm?.id || paymentMethods[0]?.id || '');
                        setIsChangePaymentOpen(true);
                      }}
                      className="h-6 px-2 text-[9px] font-bold uppercase tracking-wider text-primary hover:bg-primary/10 flex items-center gap-1"
                    >
                      <Edit3 className="h-3 w-3" />
                      Edit
                    </Button>
                  ) : (
                    <span className="text-[8px] text-muted-foreground flex items-center gap-1 italic">
                      <Lock className="h-2.5 w-2.5" /> Locked
                    </span>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  <CreditCard className="h-3 w-3 text-muted-foreground" />
                  <p className="text-xs font-bold uppercase tracking-tight">{order.paymentMethodName || 'E-Protocol'}</p>
                </div>
                {order.status !== OrderStatus.PENDING && (
                  <p className="text-[9px] text-muted-foreground/70 italic mt-0.5">
                    * Payment method can only be modified while order is PENDING.
                  </p>
                )}
              </div>
            </CardContent>
          </Card>

          <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
            <CardHeader className="p-6 pb-0">
              <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground text-center">Protocol Summary</CardTitle>
            </CardHeader>
            <CardContent className="p-6 space-y-3">
              <div className="flex justify-between text-xs font-bold uppercase tracking-widest">
                <span className="text-muted-foreground">Subtotal</span>
                <span className="text-foreground">${order.totalProductAmount?.toLocaleString()}</span>
              </div>
              <div className="flex justify-between text-xs font-bold uppercase tracking-widest">
                <span className="text-muted-foreground">Logistics</span>
                <span className="text-foreground">${order.shippingFee?.toLocaleString()}</span>
              </div>
              <div className="border-t border-border pt-4 flex flex-col items-center gap-0.5">
                <span className="text-[8px] font-bold text-muted-foreground uppercase tracking-widest">Total Valuation</span>
                <span className="text-3xl font-black text-foreground tracking-tight">${order.totalAmount?.toLocaleString()}</span>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Confirmation Dialog */}
      <AlertDialog open={!!confirmingStatus} onOpenChange={(open) => !open && setConfirmingStatus(null)}>
        <AlertDialogContent className="rounded-2xl border-border bg-card shadow-2xl">
          <AlertDialogHeader>
            <div className="flex items-center gap-3 mb-2">
               <div className={`p-2 rounded-xl ${confirmingStatus === OrderStatus.CANCELLED ? 'bg-destructive/10 text-destructive' : 'bg-primary/10 text-primary'}`}>
                 <AlertTriangle className="h-5 w-5" />
               </div>
               <AlertDialogTitle className="text-sm font-bold uppercase tracking-tight">{conf.title}</AlertDialogTitle>
            </div>
            <AlertDialogDescription className="text-xs font-medium text-muted-foreground leading-relaxed italic">
              {conf.description}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="mt-6">
            <AlertDialogCancel className="h-10 rounded-xl text-[10px] font-bold uppercase tracking-widest border-border">Cancel</AlertDialogCancel>
            <AlertDialogAction 
              onClick={handleStatusChange}
              className={`h-10 rounded-xl text-[10px] font-bold uppercase tracking-widest text-white ${conf.actionColor}`}
            >
              Execute Protocol
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Serial Number Assignment Modal — triggered by Approve button */}
      <OrderSerialApprovalModal
        isOpen={showSerialModal}
        onClose={() => setShowSerialModal(false)}
        order={order as Order}
      />

      {/* Payment Method Modification Dialog */}
      <Dialog open={isChangePaymentOpen} onOpenChange={setIsChangePaymentOpen}>
        <DialogContent className="max-w-md rounded-2xl bg-card border-border shadow-2xl p-6">
          <DialogHeader className="space-y-1">
            <div className="flex items-center gap-2">
              <div className="p-2 rounded-xl bg-primary/10 text-primary">
                <CreditCard className="h-5 w-5" />
              </div>
              <DialogTitle className="text-sm font-bold uppercase tracking-tight">
                Update Payment Protocol
              </DialogTitle>
            </div>
            <DialogDescription className="text-xs text-muted-foreground leading-relaxed">
              Order #{order.id} is currently <strong>PENDING</strong>. You may override or reassign the payment method as requested by customer prior to approval.
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
              Cancel
            </Button>
            <Button
              onClick={() => {
                if (!selectedPaymentMethodId) {
                  toast.error('Please select a payment method');
                  return;
                }
                updatePaymentMethod(
                  { orderId: order.id, paymentMethodId: selectedPaymentMethodId },
                  {
                    onSuccess: () => {
                      toast.success('Payment method updated successfully');
                      setIsChangePaymentOpen(false);
                    },
                    onError: (err: any) => {
                      toast.error(err.message || 'Failed to update payment method');
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
                  Saving...
                </>
              ) : (
                'Save Changes'
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
