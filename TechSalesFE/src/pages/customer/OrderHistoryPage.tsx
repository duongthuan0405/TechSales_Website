import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Badge } from '../../components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { useGetOrders } from '../../dataHook/orderDataHook';
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from "../../components/ui/select";
import { Loader2, Eye, Filter } from 'lucide-react';
import { useNavigate } from 'react-router';
import { Button } from '../../components/ui/button';

export function OrderHistoryPage() {
  const navigate = useNavigate();
  const { data: orders = [], isLoading } = useGetOrders();
  const [statusFilter, setStatusFilter] = useState('all');

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

  const filteredOrders = orders.filter(order => 
    statusFilter === 'all' || order.status === statusFilter
  );

  return (
    <div className="space-y-6 pb-12">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight uppercase">Order History</h1>
          <p className="text-sm text-muted-foreground font-medium">Track and manage your previous acquisitions</p>
        </div>
        
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 px-3 py-2 bg-muted/50 rounded-xl border border-border">
            <Filter className="h-4 w-4 text-muted-foreground" />
            <span className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground">Filter</span>
          </div>
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-[180px] h-10 rounded-xl bg-card border-border shadow-sm">
              <SelectValue placeholder="Protocol Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Protocols</SelectItem>
              <SelectItem value="PENDING">Pending</SelectItem>
              <SelectItem value="APPROVED">Approved</SelectItem>
              <SelectItem value="SHIPPING">Shipping</SelectItem>
              <SelectItem value="DELIVERED">Delivered</SelectItem>
              <SelectItem value="CANCELLED">Cancelled</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-muted-foreground uppercase tracking-widest text-[10px] font-bold">Synchronizing History...</p>
          </div>
        </div>
      ) : (
        <Card className="border-border shadow-sm rounded-2xl overflow-hidden bg-card">
          <CardHeader className="border-b border-border bg-muted/20">
            <CardTitle className="text-xs font-bold uppercase tracking-widest">Transaction Records ({filteredOrders.length})</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow className="border-border hover:bg-transparent">
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground py-4">Order ID</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground py-4">Execution Date</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground py-4">Items</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground py-4">Protocol Status</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground py-4 text-right">Valuation</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground py-4 text-right">Action</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                 {filteredOrders.length === 0 ? (
                   <TableRow>
                     <TableCell colSpan={6} className="h-32 text-center text-muted-foreground uppercase text-[10px] font-bold tracking-widest">No matching records found in protocol</TableCell>
                   </TableRow>
                 ) : (
                   filteredOrders.map(order => (
                    <TableRow key={order.id} className="border-border group">
                      <TableCell className="font-bold text-xs uppercase tracking-tight py-5">{order.id}</TableCell>
                      <TableCell className="text-xs font-medium text-muted-foreground py-5">{new Date(order.createdAt).toLocaleDateString()}</TableCell>
                      <TableCell className="py-5">
                        <div className="flex flex-col gap-0.5">
                          {(order.items || []).slice(0, 2).map((item, idx) => (
                            <div key={idx} className="text-[10px] font-bold uppercase tracking-tight line-clamp-1">{item.productName} <span className="text-muted-foreground opacity-60">x{item.quantity}</span></div>
                          ))}
                          {(order.items || []).length > 2 && (
                            <div className="text-[9px] text-primary font-black uppercase tracking-widest mt-1">+{(order.items || []).length - 2} Additional Units</div>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="py-5">
                        <Badge variant={getStatusVariant(order.status)} className="text-[9px] font-black uppercase tracking-widest px-2.5 py-1">
                          {order.status}
                        </Badge>
                      </TableCell>
                      <TableCell className="font-black text-sm text-right py-5">${(order.totalAmount || 0).toLocaleString()}</TableCell>
                      <TableCell className="text-right py-5">
                        <Button 
                          variant="ghost" 
                          size="icon" 
                          className="h-9 w-9 rounded-xl hover:bg-muted"
                          onClick={() => navigate(`/customer/orders/${order.id}`)}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                 )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
