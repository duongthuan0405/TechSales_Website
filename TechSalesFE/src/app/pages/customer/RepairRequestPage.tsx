import { useState } from 'react';
import { Wrench, Plus, Clock, CheckCircle2, Truck, XCircle } from 'lucide-react';
import { toast } from 'sonner';
import { useAuth } from '../../context/AuthContext';
import { useCreateRepairRequest, useGetRepairRequests } from '../../../dataHook/repairDataHook';
import { RepairRequestStatus } from '../../../models/ui_types/repairRequest';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Textarea } from '../../components/ui/textarea';
import { Badge } from '../../components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../../components/ui/select';
import { products } from '../../../data/mockData';

const statusLabel: Record<string, string> = { PENDING: 'Chờ tiếp nhận', APPROVED: 'Đã tiếp nhận', RECEIVED_CHECKING: 'Đang kiểm tra', IN_PROGRESS: 'Đang sửa chữa', COMPLETED: 'Đã hoàn tất', CANCELLED: 'Đã huỷ' };
const statusIcon: Record<string, any> = { PENDING: Clock, APPROVED: CheckCircle2, RECEIVED_CHECKING: Wrench, IN_PROGRESS: Wrench, COMPLETED: CheckCircle2, CANCELLED: XCircle };

export function RepairRequestPage() {
  const { user } = useAuth();
  const { data: requests = [] } = useGetRepairRequests(user?.id);
  const createRepair = useCreateRepairRequest();
  const [form, setForm] = useState({ productId: 'p8', serialNumber: '', problemDescription: '' });
  const [open, setOpen] = useState(false);
  const submit = () => {
    if (!form.serialNumber.trim() || !form.problemDescription.trim()) return toast.error('Vui lòng nhập serial và mô tả lỗi.');
    createRepair.mutate({ ...form, customerId: user?.id || 'u4', customerName: user?.name || 'Khách hàng' }, { onSuccess: () => { toast.success('Đã gửi yêu cầu sửa chữa. Nhân viên sẽ xác định loại xử lý.'); setOpen(false); setForm({ ...form, serialNumber: '', problemDescription: '' }); }, onError: (error: any) => toast.error(error.message) });
  };
  return <div className="space-y-6 pb-12">
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between"><div><h1 className="text-3xl font-bold">Sửa chữa & Bảo trì</h1><p className="text-sm text-muted-foreground">Theo dõi và gửi yêu cầu hỗ trợ cho thiết bị đã mua.</p></div><Button onClick={() => setOpen(!open)}><Plus className="mr-2 h-4 w-4" />Tạo yêu cầu</Button></div>
    {open && <Card><CardHeader><CardTitle className="flex items-center gap-2"><Wrench className="h-5 w-5" />Thông tin thiết bị</CardTitle></CardHeader><CardContent className="grid gap-4 sm:grid-cols-2"><div><label className="text-sm font-medium">Sản phẩm người dùng cung cấp</label><Select value={form.productId} onValueChange={value => setForm({ ...form, productId: value, serialNumber: '' })}><SelectTrigger className="mt-1 h-10 w-full rounded-xl"><SelectValue placeholder="Chọn sản phẩm" /></SelectTrigger><SelectContent>{products.map(product => <SelectItem key={product.id} value={product.id}>{product.name}</SelectItem>)}</SelectContent></Select></div><div><label className="text-sm font-medium">Serial number</label><Input value={form.serialNumber} onChange={e => setForm({ ...form, serialNumber: e.target.value })} placeholder="Nhập serial trên thiết bị" /></div><div className="sm:col-span-2"><label className="text-sm font-medium">Mô tả lỗi</label><Textarea value={form.problemDescription} onChange={e => setForm({ ...form, problemDescription: e.target.value })} placeholder="Mô tả tình trạng thiết bị..." rows={4} /></div><div className="flex justify-end gap-2 sm:col-span-2"><Button variant="outline" onClick={() => setOpen(false)}>Huỷ</Button><Button onClick={submit} disabled={createRepair.isPending}>Gửi yêu cầu</Button></div></CardContent></Card>}
    <div className="grid gap-4">{requests.length === 0 ? <Card><CardContent className="py-12 text-center text-muted-foreground">Bạn chưa có yêu cầu sửa chữa nào.</CardContent></Card> : requests.map(request => { const Icon = statusIcon[request.status] || Wrench; return <Card key={request.id}><CardContent className="flex flex-col gap-4 p-5 sm:flex-row sm:items-center sm:justify-between"><div className="flex items-start gap-3"><div className="rounded-xl bg-primary/10 p-3 text-primary"><Icon className="h-5 w-5" /></div><div><p className="font-mono text-xs text-primary">#{request.id}</p><h3 className="font-bold">{request.productName}</h3><p className="text-xs text-muted-foreground">Serial: {request.serialNumber}</p><p className="mt-2 text-sm">{request.problemDescription}</p></div></div><div className="text-left sm:text-right"><Badge>{statusLabel[request.status]}</Badge><p className="mt-2 text-xs text-muted-foreground">{request.repairType === 'WARRANTY' ? 'Bảo hành' : 'Sửa chữa tính phí'}</p>{request.solutionDescription && <p className="mt-1 max-w-xs text-xs">{request.solutionDescription}</p>}</div></CardContent></Card> })}</div>
  </div>;
}
