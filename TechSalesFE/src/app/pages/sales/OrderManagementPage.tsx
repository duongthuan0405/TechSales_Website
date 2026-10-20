import { useState } from 'react';
import { formatCurrency } from '../../../utils/format';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from "../../components/ui/select";
import { Search, Eye, Loader2, CheckCircle2, XCircle, Truck, Package, AlertTriangle, Barcode } from 'lucide-react';
import { useGetAdminOrders, useUpdateOrderStatus } from '../../../dataHook/orderDataHook';
import { Order, OrderStatus } from '../../../models/ui_types/order';
import { useNavigate } from 'react-router';
import { toast } from 'sonner';
import { OrderSerialApprovalModal } from '../../components/sales/OrderSerialApprovalModal';
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

export function OrderManagementPage() {
  const navigate = useNavigate();
  const { data: orders = [], isLoading } = useGetAdminOrders();
  const { mutate: updateStatus } = useUpdateOrderStatus();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');

  // Confirmation State
  const [confirming, setConfirming] = useState<{ id: string, status: OrderStatus } | null>(null);
  const [orderForSerialModal, setOrderForSerialModal] = useState<Order | null>(null);

  const filteredOrders = orders.filter(order => {
    const matchesSearch = order.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (order.customerName || '').toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = statusFilter === 'all' || order.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

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

  const getStatusLabel = (status: string) => {
    switch (status.toUpperCase()) {
      case 'DELIVERED': return 'Đã giao';
      case 'SHIPPING': return 'Đang giao';
      case 'APPROVED': return 'Đã duyệt';
      case 'PENDING': return 'Chờ xử lý';
      case 'CANCELLED': return 'Đã hủy';
      default: return status;
    }
  };

  const handleStatusChange = () => {
    if (!confirming) return;
    
    updateStatus({ id: confirming.id, status: confirming.status }, {
      onSuccess: () => {
        toast.success(`Đã cập nhật trạng thái đơn hàng thành ${getStatusLabel(confirming.status)}`);
        setConfirming(null);
      },
      onError: (err: any) => {
        toast.error(err.message || 'Cập nhật trạng thái thất bại');
        setConfirming(null);
      }
    });
  };

  const getConfirmationDetails = () => {
    if (!confirming) return { title: '', description: '', actionColor: 'bg-primary' };
    
    switch (confirming.status) {
      case OrderStatus.APPROVED:
        return {
          title: 'Duyệt đơn hàng này?',
          description: `Bạn có chắc chắn muốn duyệt đơn hàng ${confirming.id}? Đơn hàng sẽ được chuyển sang bộ phận kho để chuẩn bị.`,
          actionColor: 'bg-emerald-500 hover:bg-emerald-600'
        };
      case OrderStatus.SHIPPING:
        return {
          title: 'Bắt đầu giao hàng?',
          description: `Xác nhận đơn hàng ${confirming.id} đã được bàn giao cho đối tác vận chuyển.`,
          actionColor: 'bg-blue-500 hover:bg-blue-600'
        };
      case OrderStatus.DELIVERED:
        return {
          title: 'Xác nhận đã giao hàng?',
          description: `Xác nhận đơn hàng ${confirming.id} đã được giao thành công tới người nhận.`,
          actionColor: 'bg-green-500 hover:bg-green-600'
        };
      case OrderStatus.CANCELLED:
        return {
          title: 'Hủy đơn hàng này?',
          description: `CẢNH BÁO: Bạn sắp hủy đơn hàng ${confirming.id}. Hành động này không thể hoàn tác.`,
          actionColor: 'bg-destructive hover:bg-destructive/90'
        };
      default:
        return { title: 'Xác nhận hành động', description: 'Bạn có chắc chắn không?', actionColor: 'bg-primary' };
    }
  };

  const conf = getConfirmationDetails();

  return (
    <div className="space-y-6 pb-12">
      <div>
        <h1 className="text-3xl font-bold tracking-tight uppercase">Quản lý đơn hàng</h1>
        <p className="text-sm text-muted-foreground font-bold uppercase tracking-widest opacity-60">Kiểm soát vận hành & Giao dịch</p>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Tìm theo mã đơn hoặc tên khách hàng..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10 h-11 rounded-xl border-border bg-card"
          />
        </div>
        <Select 
          value={statusFilter} 
          onValueChange={setStatusFilter}
        >
          <SelectTrigger className="w-[200px] h-11 rounded-xl">
            <SelectValue placeholder="TẤT CẢ TRẠNG THÁI" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">TẤT CẢ TRẠNG THÁI</SelectItem>
            <SelectItem value="PENDING">CHỜ XỬ LÝ</SelectItem>
            <SelectItem value="APPROVED">ĐÃ DUYỆT</SelectItem>
            <SelectItem value="SHIPPING">ĐANG GIAO HÀNG</SelectItem>
            <SelectItem value="DELIVERED">ĐÃ GIAO HÀNG</SelectItem>
            <SelectItem value="CANCELLED">ĐÃ HỦY</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-muted-foreground uppercase tracking-widest text-[10px] font-bold">Đang tải cơ sở dữ liệu đơn hàng...</p>
          </div>
        </div>
      ) : (
        <Card className="border-border shadow-sm rounded-2xl overflow-hidden bg-card">
          <CardHeader className="border-b border-border bg-muted/20">
            <CardTitle className="text-xs font-bold uppercase tracking-widest">Danh sách đơn hàng ({filteredOrders.length})</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow className="border-border hover:bg-transparent">
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground">Mã đơn hàng</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground">Khách hàng</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground text-center">Số lượng</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground">Trạng thái</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground">Thanh toán</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground text-right">Tổng tiền</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground text-right">Thao tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredOrders.map(order => (
                  <TableRow 
                    key={order.id} 
                    className="border-border group cursor-pointer hover:bg-muted/50 transition-colors"
                    onClick={() => navigate(`/sales/orders/${order.id}`)}
                  >
                    <TableCell className="font-bold text-xs uppercase tracking-tight py-4">{order.id}</TableCell>
                    <TableCell className="py-4">
                      <div className="flex flex-col">
                        <span className="font-bold text-xs uppercase tracking-tight">{order.customerName}</span>
                        <span className="text-[9px] text-muted-foreground uppercase">{new Date(order.createdAt).toLocaleDateString()}</span>
                      </div>
                    </TableCell>
                    <TableCell className="text-center py-4">
                       <div className="flex flex-col items-center gap-1">
                         <span className="bg-muted px-2 py-0.5 rounded-md text-[10px] font-bold">
                           {order.items?.reduce((sum, i) => sum + i.quantity, 0) || order.items?.length || 0} máy
                         </span>
                         {order.status !== OrderStatus.PENDING && (
                           <span className="text-[8px] font-bold text-emerald-600 dark:text-emerald-400 flex items-center gap-0.5">
                             <Barcode className="h-2.5 w-2.5" />
                             {order.items?.reduce((sum, i) => sum + (i.serialNumbers?.length || 0), 0)} Serie
                           </span>
                         )}
                       </div>
                    </TableCell>
                    <TableCell className="py-4">
                      <Badge variant={getStatusVariant(order.status)} className="text-[9px] font-black uppercase tracking-wider px-2.5 py-1">
                        {getStatusLabel(order.status)}
                      </Badge>
                    </TableCell>
                    <TableCell className="py-4">
                      <div className="flex flex-col">
                        <span className="font-bold text-[10px] uppercase">{order.paymentMethodName || 'N/A'}</span>
                        {order.isPaymentFailed !== null && order.isPaymentFailed !== undefined && (
                          <span className={`text-[9px] font-bold uppercase ${order.isPaymentFailed ? 'text-red-500' : 'text-green-500'}`}>
                            {order.isPaymentFailed ? 'THẤT BÀI' : 'ĐÃ THANH TOÁN'}
                          </span>
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="text-right font-black text-sm py-4">{formatCurrency(order.totalAmount)}</TableCell>
                    <TableCell className="text-right py-4" onClick={(e) => e.stopPropagation()}>
                      <div className="flex items-center justify-end gap-2">
                        {order.status === OrderStatus.PENDING && (
                          <Button 
                            variant="ghost" 
                            size="icon" 
                            className="h-8 w-8 rounded-lg text-emerald-500 hover:bg-emerald-500/10 disabled:bg-zinc-100 disabled:text-zinc-400"
                            onClick={() => setOrderForSerialModal(order)}
                            title="Duyệt đơn hàng & Nhập số Serie (ProductUnit)"
                          >
                            <CheckCircle2 className="h-4 w-4" />
                          </Button>
                        )}
                        {order.status === OrderStatus.APPROVED && (
                          <Button 
                            variant="ghost" 
                            size="icon" 
                            className="h-8 w-8 rounded-lg text-blue-500 hover:bg-blue-500/10 disabled:bg-zinc-100 disabled:text-zinc-400"
                            onClick={() => setConfirming({ id: order.id, status: OrderStatus.SHIPPING })}
                            title="Bắt đầu giao hàng"
                          >
                            <Truck className="h-4 w-4" />
                          </Button>
                        )}
                        {order.status === OrderStatus.SHIPPING && (
                          <Button 
                            variant="ghost" 
                            size="icon" 
                            className="h-8 w-8 rounded-lg text-green-500 hover:bg-green-500/10 disabled:bg-zinc-100 disabled:text-zinc-400"
                            onClick={() => setConfirming({ id: order.id, status: OrderStatus.DELIVERED })}
                            title="Xác nhận đã giao"
                          >
                            <Package className="h-4 w-4" />
                          </Button>
                        )}
                        {order.status !== OrderStatus.DELIVERED && order.status !== OrderStatus.SHIPPING && order.status !== OrderStatus.CANCELLED && (
                          <Button 
                            variant="ghost" 
                            size="icon" 
                            className="h-8 w-8 rounded-lg text-destructive hover:bg-destructive/10 disabled:bg-zinc-100 disabled:text-zinc-400"
                            onClick={() => setConfirming({ id: order.id, status: OrderStatus.CANCELLED })}
                            title="Hủy đơn hàng"
                          >
                            <XCircle className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Global Confirmation Dialog */}
      <AlertDialog open={!!confirming} onOpenChange={(open) => !open && setConfirming(null)}>
        <AlertDialogContent className="rounded-2xl border-border bg-card shadow-2xl">
          <AlertDialogHeader>
            <div className="flex items-center gap-3 mb-2">
               <div className={`p-2 rounded-xl ${confirming?.status === OrderStatus.CANCELLED ? 'bg-destructive/10 text-destructive' : 'bg-primary/10 text-primary'}`}>
                 <AlertTriangle className="h-5 w-5" />
               </div>
               <AlertDialogTitle className="text-sm font-bold uppercase tracking-tight">{conf.title}</AlertDialogTitle>
            </div>
            <AlertDialogDescription className="text-xs font-medium text-muted-foreground leading-relaxed italic">
              {conf.description}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="mt-6">
            <AlertDialogCancel className="h-10 rounded-xl text-[10px] font-bold uppercase tracking-widest border-border">Hủy bỏ</AlertDialogCancel>
            <AlertDialogAction 
              onClick={handleStatusChange}
              className={`h-10 rounded-xl text-[10px] font-bold uppercase tracking-widest text-white ${conf.actionColor}`}
            >
              Xác nhận thực hiện
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Serial Number Assignment Modal on Approval */}
      <OrderSerialApprovalModal
        isOpen={!!orderForSerialModal}
        onClose={() => setOrderForSerialModal(null)}
        order={orderForSerialModal}
      />
    </div>
  );
}
