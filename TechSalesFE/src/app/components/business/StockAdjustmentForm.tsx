import { useState } from 'react';
import { Product } from '../../../models/ui_types/product';
import { Button } from '../ui/button';
import { Textarea } from '../ui/textarea';

export function StockAdjustmentForm({ product, onSubmit, isLoading }: {
  product: Product; onSubmit: (data: { serialNumbers: string[] }) => void; isLoading?: boolean;
}) {
  const [serials, setSerials] = useState('');
  const [error, setError] = useState('');
  return <form className="space-y-4" onSubmit={event => {
    event.preventDefault();
    const serialNumbers = serials.split(/\r?\n/).map(value => value.trim()).filter(Boolean);
    if (!serialNumbers.length || new Set(serialNumbers).size !== serialNumbers.length) {
      setError('Nhập ít nhất một serial, không được trùng nhau.'); return;
    }
    if (serialNumbers.some(value => value.length > 100)) { setError('Serial tối đa 100 ký tự.'); return; }
    setError(''); onSubmit({ serialNumbers });
  }}>
    <p>Tồn khả dụng hiện tại: {product.stock ?? 0}</p>
    <label htmlFor="stock-serials" className="block text-sm">Serial nhập kho (mỗi dòng một serial)</label>
    <Textarea id="stock-serials" required rows={7} value={serials} disabled={isLoading}
      onChange={event => setSerials(event.target.value)} placeholder={'SN-001\nSN-002'} />
    {error && <p role="alert" className="text-destructive">{error}</p>}
    <Button type="submit" disabled={isLoading}>{isLoading ? 'Đang nhập kho…' : 'Nhập kho'}</Button>
  </form>;
}
