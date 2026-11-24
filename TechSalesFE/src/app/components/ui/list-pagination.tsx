import { useEffect, useState } from 'react';
import { Button } from './button';
import { Input } from './input';

interface ListPaginationProps {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  disabled?: boolean;
  maxPageSize?: number;
}

export function ListPagination({ pageNumber, pageSize, totalCount, onPageChange,
  onPageSizeChange, disabled = false, maxPageSize = 100 }: ListPaginationProps) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const [pageInput, setPageInput] = useState(String(pageNumber));
  const [sizeInput, setSizeInput] = useState(String(pageSize));
  const [error, setError] = useState('');
  useEffect(() => { setPageInput(String(pageNumber)); }, [pageNumber]);
  useEffect(() => { setSizeInput(String(pageSize)); }, [pageSize]);

  const applyPage = () => {
    const value = Number(pageInput);
    if (!Number.isInteger(value) || value < 1 || value > totalPages) {
      setError(`Số trang phải từ 1 đến ${totalPages}.`); return;
    }
    setError(''); setPageInput(String(value));
    if (value !== pageNumber) onPageChange(value);
  };
  const applySize = () => {
    const value = Number(sizeInput);
    if (!Number.isInteger(value) || value < 1 || value > maxPageSize) {
      setError(`Số item mỗi trang phải từ 1 đến ${maxPageSize}.`); return;
    }
    setError(''); setSizeInput(String(value));
    if (value !== pageSize) setPageInput('1');
    if (value !== pageSize) onPageSizeChange(value);
  };
  const goTo = (page: number) => {
    setError(''); setPageInput(String(page)); onPageChange(page);
  };

  return <nav aria-label="Phân trang" className="mt-4 space-y-2">
    <div className="flex flex-wrap items-center justify-between gap-3">
      <form className="flex items-center gap-2" onSubmit={event => { event.preventDefault(); applySize(); }}>
        <label className="flex items-center gap-2 text-sm">Item mỗi trang
          <Input aria-label="Item mỗi trang" type="number" min={1} max={maxPageSize} step={1}
            value={sizeInput} onChange={event => setSizeInput(event.target.value)} onBlur={applySize}
            disabled={disabled} className="w-24" />
        </label>
      </form>
      <div className="flex flex-wrap items-center gap-2">
        <Button type="button" variant="outline" disabled={disabled || pageNumber <= 1} onClick={() => goTo(1)}>Đầu tiên</Button>
        <Button type="button" variant="outline" disabled={disabled || pageNumber <= 1} onClick={() => goTo(pageNumber - 1)}>Trước</Button>
        <form onSubmit={event => { event.preventDefault(); applyPage(); }}>
          <label className="flex items-center gap-2 text-sm">Trang
            <Input aria-label="Số trang" type="number" min={1} max={totalPages} step={1}
              value={pageInput} onChange={event => setPageInput(event.target.value)} onBlur={applyPage}
              disabled={disabled} className="w-24" /> / {totalPages}
          </label>
        </form>
        <Button type="button" variant="outline" disabled={disabled || pageNumber >= totalPages} onClick={() => goTo(pageNumber + 1)}>Sau</Button>
        <Button type="button" variant="outline" disabled={disabled || pageNumber >= totalPages} onClick={() => goTo(totalPages)}>Cuối cùng</Button>
      </div>
      <span className="text-sm text-muted-foreground">Tổng: {totalCount} item</span>
    </div>
    {error && <p role="alert" className="text-sm text-destructive">{error}</p>}
  </nav>;
}
