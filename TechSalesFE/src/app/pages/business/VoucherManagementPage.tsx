import { useListUrlState } from '../../../dataHook/useListUrlState';
import { ListPagination } from '../../components/ui/list-pagination';
import { useApiError } from '../../../dataHook/useApiError';
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
  const { pageNumber: page, setPageNumber: setPage, pageSize, setPageSize } = useListUrlState({});

  const { data: voucherData, isLoading, isFetching, error: apiError } = useGetVouchers(page, pageSize);
  useApiError(apiError);
  const createMutation = useCreateVoucher();
  const deactivateMutation = useDeleteVoucher(); // Still use the same hook as it calls the soft-delete API

  const [isFormOpen, setIsFormOpen] = useState(false);

  const handleCreate = (data: any) => {
    createMutation.mutate(data, {
      onSuccess: () => {
        toast.success("Đã tạo mã giảm giá thành công.");
        setIsFormOpen(false);
      },
      onError: (error: any) => {
        toast.error(error.message || "Không tạo được mã giảm giá.");
      },
    });
  };

  const handleDeactivate = (id: string) => {
    if (window.confirm("Bạn có chắc muốn ngừng mã giảm giá trước hạn? Thao tác này không thể hoàn tác.")) {
      deactivateMutation.mutate(id, {
        onSuccess: () => {
          toast.success("Đã ngừng mã giảm giá.");
        },
        onError: () => {
          toast.error("Không ngừng được mã giảm giá.");
        },
      });
    }
  };

  const getStatusBadge = (voucher: Voucher) => {
    const now = new Date();
    const startDate = voucher.startDate ? new Date(voucher.startDate) : null;
    const endDate = voucher.endDate ? new Date(voucher.endDate) : null;

    if (!voucher.isActive) return <Badge variant="secondary">Đã ngừng</Badge>;
    if (voucher.usedCount >= voucher.maxUsage) return <Badge variant="destructive">Hết lượt</Badge>;
    if (endDate && now > endDate) return <Badge variant="destructive">Hết hạn</Badge>;
    if (startDate && now < startDate) return <Badge variant="outline">Chưa đến hạn</Badge>;
    
    return <Badge variant="success">Đang áp dụng</Badge>;
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Quản lý mã giảm giá</h1>
          <p className="text-muted-foreground">Tạo và quản lý mã giảm giá dành cho khách hàng</p>
        </div>
        <Button onClick={() => setIsFormOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Tạo mã giảm giá
        </Button>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : !voucherData?.items || voucherData.items.length === 0 ? (
        <Card className="flex h-64 flex-col items-center justify-center text-center">
          <Ticket className="h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-xl font-semibold">Chưa có mã giảm giá</p>
          <p className="text-muted-foreground">Tạo mã giảm giá đầu tiên để bắt đầu</p>
        </Card>
      ) : (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-medium">
              Danh sách mã giảm giá ({voucherData.totalCount})
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Mã giảm giá</TableHead>
                  <TableHead>Loại giảm</TableHead>
                  <TableHead>Giá trị</TableHead>
                  <TableHead>Lượt sử dụng</TableHead>
                  <TableHead>Thời gian áp dụng</TableHead>
                  <TableHead>Trạng thái</TableHead>
                  <TableHead className="text-right">Thao tác</TableHead>
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
                        <span className="flex items-center gap-1"><TrendingDown className="h-3 w-3" /> Phần trăm</span>
                      ) : (
                        <span className="flex items-center gap-1"><Zap className="h-3 w-3" /> Số tiền</span>
                      )}
                    </TableCell>
                    <TableCell className="font-semibold">
                      {voucher.type === VoucherType.PERCENTAGE ? `${voucher.value}%` : formatCurrency(voucher.value)}
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        <div className="text-xs text-muted-foreground">
                          {voucher.usedCount} / {voucher.maxUsage === Number.MAX_SAFE_INTEGER ? 'Không giới hạn' : voucher.maxUsage} lượt
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
                          {voucher.startDate ? new Date(voucher.startDate).toLocaleDateString('vi-VN') : 'Áp dụng ngay'}
                        </span>
                        <span className="text-muted-foreground pl-4">đến</span>
                        <span className="flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          {voucher.endDate ? new Date(voucher.endDate).toLocaleDateString('vi-VN') : 'Không giới hạn'}
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
                          title="Ngừng mã trước hạn"
                        >
                          <CircleOff className="h-4 w-4" />
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            
          </CardContent>
        </Card>
      )}

      <ListPagination pageNumber={page} pageSize={pageSize} totalCount={voucherData?.totalCount ?? 0}
        disabled={isFetching} onPageChange={setPage}
        onPageSizeChange={setPageSize} />

      {/* Info Card */}
      <Card className="bg-primary/5 border-primary/20">
        <CardContent className="p-4 flex gap-4">
          <Info className="h-5 w-5 text-primary shrink-0 mt-0.5" />
          <div className="text-sm">
            <p className="font-semibold text-primary">Điều kiện áp dụng</p>
            <ul className="list-disc list-inside text-muted-foreground mt-1 space-y-1">
              <li>Đơn hàng phải đạt giá trị tối thiểu để sử dụng mã giảm giá.</li>
              <li>Mã giảm theo phần trăm được tính dựa trên giá trị đơn hàng.</li>
              <li>Mã không còn áp dụng khi hết hạn hoặc hết lượt sử dụng.</li>
            </ul>
          </div>
        </CardContent>
      </Card>

      {/* Form Dialog */}
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-2xl">
          <DialogHeader>
            <DialogTitle>Tạo mã giảm giá mới</DialogTitle>
            <DialogDescription>
              Nhập thông tin để tạo mã giảm giá cho cửa hàng.
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
