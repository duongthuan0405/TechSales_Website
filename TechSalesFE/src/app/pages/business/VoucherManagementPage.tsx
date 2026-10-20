import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "../../components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "../../components/ui/table";
import { Badge } from "../../components/ui/badge";
import { Button } from "../../components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "../../components/ui/dialog";
import { 
  Ticket, 
  Plus, 
  CircleOff, 
  Loader2,
  Calendar,
  Zap,
  TrendingDown,
  Info
} from "lucide-react";
import { 
  useGetVouchers, 
  useCreateVoucher, 
  useDeleteVoucher 
} from "../../../dataHook/voucherDataHook";
import { Voucher, VoucherType } from "../../../models/ui_types/voucher";
import { toast } from "sonner";
import { VoucherForm } from "../../components/business/VoucherForm";
import { formatCurrency } from "../../../utils/format";

export function VoucherManagementPage() {
  const [page, setPage] = useState(1);
  const { data: voucherData, isLoading } = useGetVouchers(page);
  const createMutation = useCreateVoucher();
  const deactivateMutation = useDeleteVoucher(); // Still use the same hook as it calls the soft-delete API

  const [isFormOpen, setIsFormOpen] = useState(false);

  const handleCreate = (data: any) => {
    createMutation.mutate(data, {
      onSuccess: () => {
        toast.success("Voucher created successfully");
        setIsFormOpen(false);
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.message || "Failed to create voucher");
      },
    });
  };

  const handleDeactivate = (id: string) => {
    if (window.confirm("Are you sure you want to stop this voucher early? This action cannot be undone.")) {
      deactivateMutation.mutate(id, {
        onSuccess: () => {
          toast.success("Voucher deactivated successfully");
        },
        onError: () => {
          toast.error("Failed to deactivate voucher");
        },
      });
    }
  };

  const getStatusBadge = (voucher: Voucher) => {
    const now = new Date();
    const startDate = voucher.startDate ? new Date(voucher.startDate) : null;
    const endDate = voucher.endDate ? new Date(voucher.endDate) : null;

    if (!voucher.isActive) return <Badge variant="secondary">Deactivated</Badge>;
    if (voucher.usedCount >= voucher.maxUsage) return <Badge variant="destructive">Exhausted</Badge>;
    if (endDate && now > endDate) return <Badge variant="destructive">Expired</Badge>;
    if (startDate && now < startDate) return <Badge variant="outline">Scheduled</Badge>;
    
    return <Badge variant="success">Active</Badge>;
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Voucher Management</h1>
          <p className="text-muted-foreground">Create and manage discount codes for customers</p>
        </div>
        <Button onClick={() => setIsFormOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Create Voucher
        </Button>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : !voucherData?.items || voucherData.items.length === 0 ? (
        <Card className="flex h-64 flex-col items-center justify-center text-center">
          <Ticket className="h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-xl font-semibold">No vouchers found</p>
          <p className="text-muted-foreground">Start by creating your first discount code</p>
        </Card>
      ) : (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-medium">
              Active Vouchers ({voucherData.totalCount})
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Code</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Value</TableHead>
                  <TableHead>Usage</TableHead>
                  <TableHead>Validity</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {voucherData.items.map((voucher) => (
                  <TableRow key={voucher.id}>
                    <TableCell className="font-mono font-bold text-primary">
                      {voucher.code}
                    </TableCell>
                    <TableCell>
                      {voucher.type === VoucherType.PERCENTAGE ? (
                        <span className="flex items-center gap-1"><TrendingDown className="h-3 w-3" /> Percentage</span>
                      ) : (
                        <span className="flex items-center gap-1"><Zap className="h-3 w-3" /> Fixed</span>
                      )}
                    </TableCell>
                    <TableCell className="font-semibold">
                      {voucher.type === VoucherType.PERCENTAGE ? `${voucher.value}%` : formatCurrency(voucher.value)}
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        <div className="text-xs text-muted-foreground">
                          {voucher.usedCount} / {voucher.maxUsage} used
                        </div>
                        <div className="w-24 h-1.5 bg-muted rounded-full overflow-hidden">
                          <div 
                            className="h-full bg-primary" 
                            style={{ width: `${Math.min(100, (voucher.usedCount / voucher.maxUsage) * 100)}%` }}
                          />
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex flex-col text-xs gap-1">
                        <span className="flex items-center gap-1">
                          <Calendar className="h-3 w-3" /> 
                          {voucher.startDate ? new Date(voucher.startDate).toLocaleDateString() : 'Immediate'}
                        </span>
                        <span className="text-muted-foreground pl-4">to</span>
                        <span className="flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          {voucher.endDate ? new Date(voucher.endDate).toLocaleDateString() : 'Never'}
                        </span>
                      </div>
                    </TableCell>
                    <TableCell>{getStatusBadge(voucher)}</TableCell>
                    <TableCell className="text-right">
                      {voucher.isActive && (new Date() < new Date(voucher.endDate || '2099-12-31')) && (
                        <Button 
                          variant="ghost" 
                          size="icon" 
                          className="text-destructive hover:text-destructive hover:bg-destructive/10"
                          onClick={() => handleDeactivate(voucher.id)}
                          disabled={deactivateMutation.isPending}
                          title="Stop Voucher Early"
                        >
                          <CircleOff className="h-4 w-4" />
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            
            {/* Pagination */}
            {voucherData.totalCount > voucherData.pageSize && (
              <div className="flex items-center justify-end space-x-2 pt-4">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage(p => Math.max(1, p - 1))}
                  disabled={page === 1}
                >
                  Previous
                </Button>
                <div className="text-sm font-medium">Page {page}</div>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage(p => p + 1)}
                  disabled={page * voucherData.pageSize >= voucherData.totalCount}
                >
                  Next
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Info Card */}
      <Card className="bg-primary/5 border-primary/20">
        <CardContent className="p-4 flex gap-4">
          <Info className="h-5 w-5 text-primary shrink-0 mt-0.5" />
          <div className="text-sm">
            <p className="font-semibold text-primary">Voucher Logic</p>
            <ul className="list-disc list-inside text-muted-foreground mt-1 space-y-1">
              <li>Customers can only apply vouchers if their order meets the minimum amount.</li>
              <li>Percentage vouchers are calculated based on the total order value.</li>
              <li>Vouchers automatically expire based on the end date or when the usage limit is reached.</li>
            </ul>
          </div>
        </CardContent>
      </Card>

      {/* Form Dialog */}
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Create New Voucher</DialogTitle>
            <DialogDescription>
              Set up a new discount code for your store.
            </DialogDescription>
          </DialogHeader>
          <VoucherForm 
            onSubmit={handleCreate}
            isLoading={createMutation.isPending}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}
