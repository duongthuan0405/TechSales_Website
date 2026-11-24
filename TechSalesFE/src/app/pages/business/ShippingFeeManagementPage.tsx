import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Loader2, Search } from 'lucide-react';
import { toast } from 'sonner';
import { addressService, Location } from '../../../services/addressService';
import { shippingService } from '../../../services/shippingService';
import { formatCurrency } from '../../../utils/format';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';


function ProvinceFeeRow({ province, hidden }: { province: Location; hidden: boolean }) {
  const code = province.code;
  const client = useQueryClient();
  const [draft, setDraft] = useState<string | null>(null);
  const [validation, setValidation] = useState('');
  const current = useQuery({ queryKey: ['province-shipping-fee', code], queryFn: () => shippingService.getFee(code) });
  const save = useMutation({ mutationFn: (fee: number) => shippingService.setFee(code, fee),
    onSuccess: async result => {
      client.setQueryData(['province-shipping-fee', code], result);
      setDraft(null);
      await client.invalidateQueries({ queryKey: ['shipping-fee'] });
      toast.success('Đã lưu phí vận chuyển: ' + (province.name ?? code));
    },
  });
  const value = draft ?? (current.data ? String(current.data.fee) : '');
  const changed = draft !== null && value !== (current.data ? String(current.data.fee) : '');
  const submit = () => {
    const fee = Number(value);
    if (value.trim() === '' || !Number.isSafeInteger(fee) || fee < 0) {
      setValidation('Nhập số tiền nguyên đồng không âm, trong giới hạn cho phép.'); return;
    }
    setValidation(''); save.mutate(fee);
  };
  const formId = `province-fee-${code}`;
  return <TableRow className={hidden ? 'hidden' : undefined}>
    <TableCell className="min-w-[180px] font-medium">{province.name ?? code}<span className="mt-1 block text-xs font-normal text-muted-foreground">Mã tỉnh: {code}</span></TableCell>
    <TableCell className="min-w-[170px]">
      {current.isLoading ? <span className="flex items-center gap-2 text-sm"><Loader2 className="h-4 w-4 animate-spin" />Đang tải…</span>
        : current.error ? <div className="space-y-1 text-sm"><p className="max-w-xs text-destructive">{current.error.message}</p><Button variant="link" size="sm" className="h-auto p-0" onClick={() => current.refetch()}>Tải lại</Button></div>
        : <span>{current.data?.fee === 0 ? 'Miễn phí' : formatCurrency(current.data?.fee ?? 0)}</span>}
    </TableCell>
    <TableCell className="min-w-[220px]">
      <form id={formId} onSubmit={event => { event.preventDefault(); submit(); }}>
        <Input aria-label={`Phí vận chuyển ${province.name ?? code} (VND)`} type="number" min={0} max={Number.MAX_SAFE_INTEGER} step={1}
          required value={value} disabled={current.isLoading || save.isPending} placeholder="Nhập phí (VND)" className="h-10 w-full"
          onChange={event => { setDraft(event.target.value); save.reset(); setValidation(''); }} />
      </form>
      {changed && <p className="mt-1 text-xs text-amber-600">Chưa lưu</p>}
      {validation && <p role="alert" className="mt-1 max-w-xs text-xs text-destructive">{validation}</p>}
      {save.error && <p role="alert" className="mt-1 max-w-xs text-xs text-destructive">{save.error.message}</p>}
    </TableCell>
    <TableCell className="min-w-[160px] text-right">
      <div className="flex justify-end gap-2">
        <Button type="button" variant="outline" disabled={!changed || save.isPending} onClick={() => { setDraft(null); setValidation(''); save.reset(); }}>Hủy</Button>
        <Button type="submit" form={formId} disabled={current.isLoading || save.isPending || !changed || value === ''}>
          {save.isPending ? 'Đang lưu…' : 'Lưu'}
        </Button>
      </div>
    </TableCell>
  </TableRow>;
}

const normalize = (value: string) => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D').toLowerCase();

export function ShippingFeeManagementPage() {
  const [search, setSearch] = useState('');
  const provinces = useQuery({ queryKey: ['provinces'], queryFn: addressService.getProvinces });
  const matches = (province: Location) => normalize(`${province.name ?? ''} ${province.code}`).includes(normalize(search.trim()));
  const count = provinces.data?.filter(matches).length ?? 0;
  return <div className="mx-auto max-w-6xl space-y-6">
    <div><h1 className="text-3xl font-bold">Phí vận chuyển</h1><p className="mt-2 text-muted-foreground">Xem và chỉnh sửa phí giao hàng ngay trên từng dòng. Nhập 0 để miễn phí vận chuyển.</p></div>
    <div className="relative max-w-lg"><Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <Input aria-label="Tìm tỉnh hoặc thành phố" placeholder="Tìm tỉnh/thành phố hoặc mã tỉnh…" value={search} onChange={event => setSearch(event.target.value)} className="h-11 pl-9" />
    </div>
    <Card><CardHeader><CardTitle>Danh sách tỉnh / thành phố ({count})</CardTitle><p className="text-sm text-muted-foreground">Phí áp dụng cho tất cả phường/xã thuộc tỉnh. Bấm Lưu ở từng dòng để áp dụng; Hủy để bỏ thay đổi.</p></CardHeader><CardContent>
      {provinces.isLoading ? <div className="flex justify-center py-12"><Loader2 className="animate-spin" aria-label="Đang tải tỉnh thành" /></div>
        : provinces.error ? <div role="alert"><p className="text-destructive">{provinces.error.message}</p><Button variant="outline" onClick={() => provinces.refetch()}>Thử lại</Button></div>
        : <Table><TableHeader><TableRow><TableHead>Tỉnh / Thành phố</TableHead><TableHead>Phí hiện tại</TableHead><TableHead>Phí mới (VND)</TableHead><TableHead className="text-right">Thao tác</TableHead></TableRow></TableHeader>
          <TableBody>
            {provinces.data?.map(province => <ProvinceFeeRow key={province.code} province={province} hidden={!matches(province)} />)}
            {count === 0 && <TableRow><TableCell colSpan={4} className="py-10 text-center text-muted-foreground">Không tìm thấy tỉnh/thành phố phù hợp.</TableCell></TableRow>}
          </TableBody>
        </Table>}
    </CardContent></Card>
  </div>;
}