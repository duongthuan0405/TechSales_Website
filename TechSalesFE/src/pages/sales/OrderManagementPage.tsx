import { useState } from 'react';
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
import { Search, Eye, Loader2, CheckCircle2, XCircle, Truck, Package, AlertTriangle } from 'lucide-react';
import { useGetOrders, useUpdateOrderStatus } from '../../dataHook/orderDataHook';
import { OrderStatus } from '../../models/ui_types/order';
import { useNavigate } from 'react-router';
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

export function OrderManagementPage() {
  const navigate = useNavigate();
  const { data: orders = [], isLoading } = useGetOrders();
  const { mutate: updateStatus } = useUpdateOrderStatus();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');

  // Confirmation State
  const [confirming, setConfirming] = useState<{ id: string, status: OrderStatus } | null>(null);

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

  const handleStatusChange = () => {
    if (!confirming) return;
    
    updateStatus({ id: confirming.id, status: confirming.status }, {
      onSuccess: () => {
        toast.success(`Order protocol updated to ${confirming.status}`);
        setConfirming(null);
      },
      onError: (err: any) => {
        toast.error(err.message || 'Failed to update protocol');
        setConfirming(null);
      }
    });
  };

  const getConfirmationDetails = () => {
    if (!confirming) return { title: '', description: '', actionColor: 'bg-primary' };
    
    switch (confirming.status) {
      case OrderStatus.APPROVED:
        return {
          title: 'Approve Order Protocol?',
          description: `Are you sure you want to approve order ${confirming.id}? This will notify the warehouse to begin preparation.`,
          actionColor: 'bg-emerald-500 hover:bg-emerald-600'
        };
      case OrderStatus.SHIPPING:
        return {
          title: 'Initiate Shipment?',
          description: `Confirm that order ${confirming.id} has been handed over to the logistics partner.`,
          actionColor: 'bg-blue-500 hover:bg-blue-600'
        };
      case OrderStatus.DELIVERED:
        return {
          title: 'Confirm Delivery?',
          description: `Verify that order ${confirming.id} has been successfully delivered to the customer destination.`,
          actionColor: 'bg-green-500 hover:bg-green-600'
        };
      case OrderStatus.CANCELLED:
        return {
          title: 'Void Order Protocol?',
          description: `WARNING: You are about to cancel order ${confirming.id}. This action is irreversible within the current protocol layer.`,
          actionColor: 'bg-destructive hover:bg-destructive/90'
        };
      default:
        return { title: 'Confirm Action', description: 'Are you sure?', actionColor: 'bg-primary' };
    }
  };

  const conf = getConfirmationDetails();

  return (
    <div className="space-y-6 pb-12">
      <div>
        <h1 className="text-3xl font-bold tracking-tight uppercase">Order Management</h1>
        <p className="text-sm text-muted-foreground font-bold uppercase tracking-widest opacity-60">Logistics & Transaction Control</p>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by ID or Customer Name..."
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
            <SelectValue placeholder="ALL PROTOCOLS" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">ALL PROTOCOLS</SelectItem>
            <SelectItem value="PENDING">PENDING</SelectItem>
            <SelectItem value="APPROVED">APPROVED</SelectItem>
            <SelectItem value="SHIPPING">SHIPPING</SelectItem>
            <SelectItem value="DELIVERED">DELIVERED</SelectItem>
            <SelectItem value="CANCELLED">CANCELLED</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-muted-foreground uppercase tracking-widest text-[10px] font-bold">Synchronizing Order Database...</p>
          </div>
        </div>
      ) : (
        <Card className="border-border shadow-sm rounded-2xl overflow-hidden bg-card">
          <CardHeader className="border-b border-border bg-muted/20">
            <CardTitle className="text-xs font-bold uppercase tracking-widest">Active Orders ({filteredOrders.length})</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow className="border-border hover:bg-transparent">
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground">Order ID</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground">Customer</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground text-center">Items</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground">Status</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground text-right">Total</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[10px] text-muted-foreground text-right">Quick Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredOrders.map(order => (
                  <TableRow key={order.id} className="border-border group">
                    <TableCell className="font-bold text-xs uppercase tracking-tight py-4">{order.id}</TableCell>
                    <TableCell className="py-4">
                      <div className="flex flex-col">
                        <span className="font-bold text-xs uppercase tracking-tight">{order.customerName}</span>
                        <span className="text-[9px] text-muted-foreground uppercase">{new Date(order.createdAt).toLocaleDateString()}</span>
                      </div>
                    </TableCell>
                    <TableCell className="text-center py-4">
                       <span className="bg-muted px-2 py-1 rounded-md text-[10px] font-bold">{order.items?.length || 0}</span>
                    </TableCell>
                    <TableCell className="py-4">
                      <Badge variant={getStatusVariant(order.status)} className="text-[9px] font-black uppercase tracking-wider px-2.5 py-1">
                        {order.status}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right font-black text-sm py-4">${order.totalAmount.toLocaleString()}</TableCell>
                    <TableCell className="text-right py-4">
                      <div className="flex items-center justify-end gap-2">
                        {order.status === OrderStatus.PENDING && (
                          <Button 
                            variant="ghost" 
                            size="icon" 
                            className="h-8 w-8 rounded-lg text-emerald-500 hover:bg-emerald-500/10 disabled:bg-zinc-100 disabled:text-zinc-400"
                            onClick={() => setConfirming({ id: order.id, status: OrderStatus.APPROVED })}
                            title="Approve Protocol"
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
                            title="Start Shipment"
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
                            title="Confirm Delivery"
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
                            title="Void Order (Chỉ khi chưa giao)"
                          >
                            <XCircle className="h-4 w-4" />
                          </Button>
                        )}
                        <Button 
                          variant="ghost" 
                          size="icon" 
                          className="h-8 w-8 rounded-lg text-muted-foreground hover:bg-muted"
                          onClick={() => navigate(`/sales/orders/${order.id}`)}
                          title="Full Analysis"
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
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
    </div>
  );
}
