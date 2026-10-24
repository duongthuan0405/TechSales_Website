import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { formatCurrency } from '../../../utils/format';
import { Badge } from '../../components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { useGetOrders } from '../../../dataHook/orderDataHook';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../components/ui/select";
import { Loader2, Eye, RotateCcw } from 'lucide-react';
import { useNavigate } from 'react-router';
import { Button } from '../../components/ui/button';

export function OrderHistoryPage() {
  const navigate = useNavigate();
  const { data: orders = [], isLoading } = useGetOrders();

  const getStatusVariant = (status: string) => {
    switch (status) {
      case 'DELIVERED': return 'success';
      case 'SHIPPING': return 'info';
      case 'APPROVED': return 'warning';
      case 'PENDING': return 'pending';
      case 'CANCELLED': return 'danger';
      default: return 'default';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'DELIVERED': return 'Đã giao hàng';
      case 'SHIPPING': return 'Đang giao hàng';
      case 'APPROVED': return 'Đã duyệt';
      case 'PENDING': return 'Chờ xử lý';
      case 'CANCELLED': return 'Đã hủy';
      default: return status;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">Lịch sử đơn hàng</h1>
          <p className="text-muted-foreground">Xem lại các đơn hàng đã đặt của bạn</p>
        </div>
        <Button
          variant="outline"
          onClick={() => navigate('/customer/returns')}
          className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider border-border hover:border-primary text-primary hover:bg-primary/10 flex items-center gap-1.5 shrink-0"
        >
          <RotateCcw className="h-4 w-4" />
          Yêu cầu hoàn trả
        </Button>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-muted-foreground">Đang tải đơn hàng...</p>
          </div>
        </div>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>Danh sách đơn hàng</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Mã đơn hàng</TableHead>
                  <TableHead>Ngày đặt</TableHead>
                  <TableHead>Sản phẩm</TableHead>
                  <TableHead>Trạng thái</TableHead>
                  <TableHead>Tổng tiền</TableHead>
                  <TableHead></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                 {orders.map(order => (
                  <TableRow key={order.id}>
                    <TableCell className="font-medium">{order.id}</TableCell>
                    <TableCell>{new Date(order.createdAt).toLocaleDateString()}</TableCell>
                    <TableCell>
                      <div className="text-sm">
                        {(order.items || []).slice(0, 2).map((item, idx) => (
                          <div key={idx}>{item.productName} x{item.quantity}</div>
                        ))}
                        {(order.items || []).length > 2 && (
                          <div className="text-muted-foreground">+{(order.items || []).length - 2} sản phẩm khác</div>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant={getStatusVariant(order.status)}>
                        {getStatusLabel(order.status)}
                      </Badge>
                    </TableCell>
                    <TableCell className="font-semibold">{formatCurrency(order.totalAmount || 0)}</TableCell>
                    <TableCell>
                      <Button variant="ghost" size="sm" onClick={() => navigate(`/customer/orders/${order.id}`)}>
                        <Eye className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
