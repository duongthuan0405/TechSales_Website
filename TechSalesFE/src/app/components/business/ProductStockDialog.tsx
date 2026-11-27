import { useListUrlState } from '../../../dataHook/useListUrlState';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Loader2, PackagePlus } from 'lucide-react';
import { toast } from 'sonner';
import { Product } from '../../../models/ui_types/product';
import { InventoryUnit, inventoryService, UnitStatus } from '../../../services/inventoryService';
import { useDebouncedValue } from '../../../dataHook/useDebouncedValue';
import { useGetProduct } from '../../../dataHook/productDataHook';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Badge } from '../ui/badge';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogFooter } from '../ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../ui/table';
import { ListPagination } from '../ui/list-pagination';
import { StockAdjustmentForm } from './StockAdjustmentForm';
import { useAuth } from '../../context/AuthContext';

const statuses: Record<UnitStatus, string> = { IN_STOCK: 'Trong kho', RESERVED: 'Đã giữ chỗ', SOLD: 'Đã bán', DAMAGED: 'Hỏng' };

export function useRefreshStock() {
  const client = useQueryClient();
  return () => Promise.all(['inventory-units', 'products', 'admin-products', 'product', 'cart'].map(key =>
    client.invalidateQueries({ queryKey: [key] })));
}

export function ProductStockDialog({ product, onClose }: { product: Product; onClose: () => void }) {
  const { user } = useAuth();
  const canReceive = user?.role === 'Business Admin' || user?.role === 'Technical Admin';
  const list = useListUrlState({ keyword: '', status: 'all' }, 'stock.');
  const { pageNumber, pageSize, setPageNumber, setPageSize } = list;
  const [search, setSearch] = list.field('keyword');
  const [status, setStatus] = list.field('status');
  const keyword = useDebouncedValue(search);



  const [receiving, setReceiving] = useState(false);
  const [damaging, setDamaging] = useState<InventoryUnit>();
  const refresh = useRefreshStock();
  const current = useGetProduct(product.id);
  const units = useQuery({
    queryKey: ['inventory-units', product.id, keyword, status, pageNumber, pageSize],
    queryFn: () => inventoryService.getUnits({ productId: product.id, keyword, status: status === 'all' ? undefined : status, pageNumber, pageSize }),
    enabled: search === keyword,
  });
  const receive = useMutation({
    mutationFn: (serials: string[]) => inventoryService.receive(product.id, serials),
    onSuccess: async () => { await refresh(); setReceiving(false); setPageNumber(1); toast.success('Đã nhập kho.'); },
    onError: (error: Error) => toast.error(error.message),
  });
  const damage = useMutation({ mutationFn: (unit: InventoryUnit) => unit.status === 'DAMAGED' ? inventoryService.restoreToStock(unit) : inventoryService.markDamaged(unit),
    onSuccess: async () => { await refresh(); setDamaging(undefined); setPageNumber(1); toast.success('Đã cập nhật trạng thái thiết bị.'); },
    onError: (error: Error) => toast.error(error.message),
  });
  const busy = receive.isPending || damage.isPending;
  return <>
    <Dialog open onOpenChange={open => { if (!open && !busy) onClose(); }}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-4xl">
        <DialogHeader><DialogTitle>Kho — {product.name}</DialogTitle>
          <DialogDescription>Xem và quản lý từng thiết bị theo serial. Có thể đánh dấu hỏng hoặc khôi phục thiết bị hỏng về trong kho.</DialogDescription></DialogHeader>
        <div className="flex flex-wrap items-center justify-between gap-3">
          <p className="text-sm">Tồn khả dụng: {current.data?.stock ?? product.stock ?? '—'}</p>
          {canReceive && <Button onClick={() => setReceiving(true)} disabled={busy}><PackagePlus className="mr-2 h-4 w-4" />Nhập kho</Button>}
        </div>
        <div className="grid gap-3 sm:grid-cols-2">
          <Input aria-label="Tìm serial" placeholder="Tìm serial…" value={search} onChange={event => { setSearch(event.target.value); }} />
          <Select value={status} onValueChange={value => { setStatus(value); }}>
            <SelectTrigger className="w-full" aria-label="Lọc trạng thái unit"><SelectValue /></SelectTrigger>
            <SelectContent><SelectItem value="all">Tất cả trạng thái</SelectItem>
              {Object.entries(statuses).map(([value, label]) => <SelectItem key={value} value={value}>{label}</SelectItem>)}
            </SelectContent>
          </Select>
        </div>
        {units.error ? <div role="alert" className="space-y-2"><p className="text-destructive">{units.error.message}</p><Button variant="outline" onClick={() => units.refetch()}>Thử lại</Button></div>
          : units.isLoading || search !== keyword ? <div className="flex justify-center py-12"><Loader2 className="animate-spin" aria-label="Đang tải unit" /></div>
          : <Table><TableHeader><TableRow><TableHead>Serial</TableHead><TableHead>Trạng thái</TableHead><TableHead className="text-right">Thao tác</TableHead></TableRow></TableHeader>
            <TableBody>{units.data?.items.length ? units.data.items.map(unit => <TableRow key={unit.serialNumber}>
              <TableCell className="font-mono">{unit.serialNumber}</TableCell><TableCell><Badge variant={unit.status === 'DAMAGED' ? 'danger' : 'outline'}>{statuses[unit.status]}</Badge></TableCell>
              <TableCell className="text-right">{(unit.status === 'IN_STOCK' || unit.status === 'DAMAGED') && <Button variant="outline" disabled={busy || units.isFetching} onClick={() => setDamaging(unit)}>{unit.status === 'DAMAGED' ? 'Khôi phục về kho' : 'Đánh dấu hỏng'}</Button>}</TableCell>
            </TableRow>) : <TableRow><TableCell colSpan={3} className="py-10 text-center text-muted-foreground">Không tìm thấy unit phù hợp.</TableCell></TableRow>}</TableBody>
          </Table>}
        <ListPagination pageNumber={pageNumber} pageSize={pageSize} totalCount={units.data?.totalCount ?? 0} disabled={units.isFetching || search !== keyword || !!units.error}
          onPageChange={setPageNumber} onPageSizeChange={setPageSize} />
      </DialogContent>
    </Dialog>
    <Dialog open={canReceive && receiving} onOpenChange={open => { if (!receive.isPending) setReceiving(open); }}><DialogContent><DialogHeader>
      <DialogTitle>Nhập kho — {product.name}</DialogTitle><DialogDescription>Nhập serial thủ công, mỗi dòng một serial.</DialogDescription></DialogHeader>
      <StockAdjustmentForm product={current.data ?? product} isLoading={receive.isPending} onSubmit={data => receive.mutate(data.serialNumbers)} />
    </DialogContent></Dialog>
    <Dialog open={!!damaging} onOpenChange={open => { if (!open && !damage.isPending) setDamaging(undefined); }}><DialogContent><DialogHeader>
      <DialogTitle>{damaging?.status === 'DAMAGED' ? 'Khôi phục về kho' : 'Đánh dấu hỏng'}</DialogTitle><DialogDescription>Serial {damaging?.serialNumber}: {damaging?.status === 'DAMAGED' ? 'Xác nhận thiết bị đã hoạt động bình thường. Khôi phục sẽ tăng tồn khả dụng một thiết bị.' : 'Thiết bị sẽ chuyển sang hỏng và giảm tồn khả dụng một thiết bị.'}</DialogDescription></DialogHeader>
      <DialogFooter><Button variant="outline" disabled={damage.isPending} onClick={() => setDamaging(undefined)}>Hủy</Button><Button variant="destructive" disabled={damage.isPending} onClick={() => damaging && damage.mutate(damaging)}>Xác nhận</Button></DialogFooter>
    </DialogContent></Dialog>
  </>;
}
