import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ApiError } from '../../../api/apiClient';
import { inventoryService } from '../../../services/inventoryService';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '../ui/dialog';
import { useRefreshStock } from './ProductStockDialog';

export function StockImportDialog({ onClose }: { onClose: () => void }) {
  const [file, setFile] = useState<File>();
  const [fileError, setFileError] = useState('');
  const refresh = useRefreshStock();
  const mutation = useMutation({ mutationFn: inventoryService.importExcel,
    onSuccess: async result => { await refresh(); toast.success(`Đã nhập ${result.receivedQuantity} thiết bị cho ${result.products.length} sản phẩm.`); },
  });
  return <Dialog open onOpenChange={open => { if (!open && !mutation.isPending) onClose(); }}><DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-xl">
    <DialogHeader><DialogTitle>Nhập kho cho nhiều sản phẩm</DialogTitle><DialogDescription>Nhập kho bằng Excel cho sản phẩm đã có trong hệ thống. Mỗi dòng tương ứng một thiết bị.</DialogDescription></DialogHeader>
    <div className="space-y-2 text-sm">
      <p>File .xlsx tối đa 5 MB, 5.000 dòng. Sheet đầu tiên có hai cột <strong>ProductId</strong> và <strong>SerialNumber</strong>; dòng đầu là tiêu đề.</p>
      <p>ProductId là ID sản phẩm trong danh sách. Không dùng công thức. Nếu có lỗi, toàn bộ file sẽ không được nhập.</p>
    </div>
    <form className="space-y-4" onSubmit={event => { event.preventDefault(); if (file && !mutation.isSuccess) mutation.mutate(file); }}>
      <Label htmlFor="stock-import-file">File Excel</Label>
      <Input id="stock-import-file" type="file" accept=".xlsx" disabled={mutation.isPending} onChange={event => {
        const selected = event.target.files?.[0];
        mutation.reset(); setFile(undefined); setFileError('');
        if (!selected) return;
        if (!selected.name.toLowerCase().endsWith('.xlsx') || selected.size <= 0 || selected.size > 5 * 1024 * 1024) {
          setFileError('Chọn file .xlsx có dữ liệu, không vượt quá 5 MB.'); return;
        }
        setFile(selected);
      }} />
      {fileError && <p role="alert" className="text-destructive">{fileError}</p>}
      {mutation.error && <div role="alert" className="space-y-2 text-sm text-destructive"><p>{mutation.error.message}</p>
        {mutation.error instanceof ApiError && mutation.error.errors && Object.entries(mutation.error.errors).map(([key, messages]) =>
          <p key={key}>{key}: {Array.isArray(messages) ? messages.join('; ') : String(messages)}</p>)}
      </div>}
      {mutation.data && <div role="status" className="space-y-2 text-sm"><p>Đã nhập {mutation.data.receivedQuantity} thiết bị.</p>
        {mutation.data.products.map(product => <p key={product.productId} className="break-all">{product.productId}: thêm {product.receivedQuantity}, tồn khả dụng {product.quantity}.</p>)}
      </div>}
      <div className="flex justify-end gap-2"><Button type="button" variant="outline" disabled={mutation.isPending} onClick={onClose}>Đóng</Button>
        <Button type="submit" disabled={!file || mutation.isPending || mutation.isSuccess}>{mutation.isPending ? 'Đang nhập…' : 'Nhập từ Excel'}</Button></div>
    </form>
  </DialogContent></Dialog>;
}
