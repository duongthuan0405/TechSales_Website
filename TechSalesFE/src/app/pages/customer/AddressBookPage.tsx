import { useState } from 'react';
import { useGetAddresses, useCreateAddress, useUpdateAddress, useDeleteAddress, useSetDefaultAddress } from '../../../dataHook/addressDataHook';
import { Address } from '../../../models/ui_types/address';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Modal } from '../../components/ui/modal';
import { LocationFields } from '../../components/customer/LocationFields';
import { toast } from 'sonner';
import { CheckCircle2, MapPin } from 'lucide-react';
import { Badge } from '../../components/ui/badge';

const empty = { province: '', provinceCode: '', ward: '', wardCode: '', detail: '', isDefault: false };
export function AddressBookPage() {
  const addresses = useGetAddresses();
  const create = useCreateAddress(); const update = useUpdateAddress();
  const remove = useDeleteAddress(); const setDefault = useSetDefaultAddress();
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<Address>();
  const [form, setForm] = useState(empty);
  const busy = create.isPending || update.isPending || remove.isPending || setDefault.isPending;
  const start = (address?: Address) => {
    setEditing(address); setForm(address ? { ...empty, ...address } : empty); setOpen(true);
  };
  const showError = (error: Error) => toast.error(error.message);
  return <div className="mx-auto max-w-4xl space-y-5">
    <div className="flex items-center justify-between"><h1 className="text-2xl font-bold">Sổ địa chỉ</h1>
      <Button onClick={() => start()} disabled={busy}>Thêm địa chỉ</Button></div>
    {addresses.isPending && <p>Đang tải địa chỉ…</p>}
    {addresses.error && <p role="alert" className="text-destructive">{addresses.error.message}</p>}
    {addresses.data?.length === 0 && <p>Chưa có địa chỉ giao hàng.</p>}
    {addresses.data?.map(address => <div key={address.id}
      className={`space-y-4 rounded-xl border p-5 ${address.isDefault ? 'border-primary bg-primary/5 ring-1 ring-primary/30 shadow-sm' : 'border-border bg-card'}`}>
      <div className="flex flex-wrap items-center gap-2">
        <MapPin className={`h-5 w-5 shrink-0 ${address.isDefault ? 'text-primary' : 'text-muted-foreground'}`} aria-hidden="true" />
        <span className="font-semibold">Địa chỉ giao hàng</span>
        {address.isDefault && <Badge className="gap-1"><CheckCircle2 className="h-3.5 w-3.5" aria-hidden="true" />Địa chỉ mặc định</Badge>}
      </div>
      <p className={address.isDefault ? 'font-medium' : undefined}>{address.detail}, {address.ward}, {address.province}</p>
      <div className="flex flex-wrap gap-2">
        <Button variant="outline" disabled={busy} onClick={() => start(address)}>Sửa</Button>
        {!address.isDefault && <Button variant="outline" disabled={busy} onClick={() => setDefault.mutate(address.id, { onError: showError })}>Đặt mặc định</Button>}
        <Button variant="outline" disabled={busy} onClick={() => {
          if (confirm('Xóa địa chỉ này?')) remove.mutate(address.id, { onError: showError });
        }}>Xóa</Button>
      </div>
    </div>)}
    <Modal open={open} onOpenChange={setOpen} title={editing ? 'Sửa địa chỉ' : 'Thêm địa chỉ'}>
      <form className="space-y-4" onSubmit={async event => {
        event.preventDefault();
        try {
          if (editing) await update.mutateAsync({ id: editing.id, updates: form });
          else await create.mutateAsync(form);
          setOpen(false); toast.success('Đã lưu địa chỉ.');
        } catch (error) { showError(error as Error); }
      }}>
        <LocationFields value={form} onChange={patch => setForm(previous => ({ ...previous, ...patch }))} />
        <label className="block text-sm">Địa chỉ chi tiết<Input required value={form.detail}
          onChange={event => setForm(previous => ({ ...previous, detail: event.target.value }))} /></label>
        <label className="flex gap-2 text-sm"><input type="checkbox" checked={form.isDefault} disabled={!!editing?.isDefault}
          onChange={event => setForm(previous => ({ ...previous, isDefault: event.target.checked }))} />Địa chỉ mặc định</label>
        <Button disabled={busy || !form.wardCode} type="submit">{busy ? 'Đang lưu…' : 'Lưu địa chỉ'}</Button>
      </form>
    </Modal>
  </div>;
}
