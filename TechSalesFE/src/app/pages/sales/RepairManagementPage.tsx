import { useState } from 'react';
import { Search, Wrench, CheckCircle2, PackageCheck, Factory, Eye } from 'lucide-react';
import { toast } from 'sonner';
import { useGetRepairRequests, useUpdateRepairRequest } from '../../../dataHook/repairDataHook';
import { RepairRequest, RepairRequestStatus } from '../../../models/ui_types/repairRequest';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Textarea } from '../../components/ui/textarea';
import { Badge } from '../../components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../../components/ui/select';

const labels: Record<string, string> = { PENDING: 'Chờ tiếp nhận', APPROVED: 'Đã tiếp nhận', RECEIVED_CHECKING: 'Đang kiểm tra', IN_PROGRESS: 'Đang sửa chữa', COMPLETED: 'Đã hoàn tất', CANCELLED: 'Đã huỷ' };

export function RepairManagementPage() {
  const { data: requests = [] } = useGetRepairRequests();
  const update = useUpdateRepairRequest();
  const [search, setSearch] = useState('');
  const [selected, setSelected] = useState<RepairRequest | null>(null);
  const [checking, setChecking] = useState('');
  const [solution, setSolution] = useState('');
  const [location, setLocation] = useState<'AT_STORE' | 'AT_MANUFACTURER'>('AT_STORE');
  const [repairType, setRepairType] = useState<'WARRANTY' | 'REPAIR'>('REPAIR');
  const [repairFee, setRepairFee] = useState('0');
  const visible = requests.filter(item => `${item.id} ${item.serialNumber} ${item.productName} ${item.customerName}`.toLowerCase().includes(search.toLowerCase()));
  const openDetail = (item: RepairRequest) => { setSelected(item); setChecking(item.checkingResult || ''); setSolution(item.solutionDescription || ''); setLocation(item.processingLocation || 'AT_STORE'); setRepairType(item.repairType || 'REPAIR'); setRepairFee(String(item.totalAmount || 0)); };
  const change = (status: RepairRequestStatus, extra = {}) => { if (!selected) return; update.mutate({ id: selected.id, status, ...extra }, { onSuccess: updated => { setSelected(updated); toast.success('Đã cập nhật yêu cầu sửa chữa.'); }, onError: (e: any) => toast.error(e.message) }); };

  return <div className="space-y-6 pb-12">
    <div><h1 className="text-3xl font-bold">Quản lý sửa chữa & bảo trì</h1><p className="text-sm text-muted-foreground">Danh sách yêu cầu và quy trình xử lý thiết bị.</p></div>
    <div className="relative max-w-md"><Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" /><Input value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm mã yêu cầu, serial, khách hàng..." className="pl-9 rounded-xl" /></div>
    <Card className="overflow-hidden"><CardHeader className="border-b"><CardTitle className="text-sm uppercase tracking-widest">Danh sách yêu cầu ({visible.length})</CardTitle></CardHeader><CardContent className="p-0"><Table><TableHeader><TableRow><TableHead>Mã yêu cầu</TableHead><TableHead>Khách hàng</TableHead><TableHead>Thiết bị / Serial</TableHead><TableHead>Ngày tạo</TableHead><TableHead>Trạng thái</TableHead><TableHead className="text-right">Thao tác</TableHead></TableRow></TableHeader><TableBody>{visible.map(item => <TableRow key={item.id} className="cursor-pointer" onClick={() => openDetail(item)}><TableCell className="font-mono font-bold text-primary">#{item.id}</TableCell><TableCell>{item.customerName}</TableCell><TableCell><div className="font-medium">{item.productName}</div><div className="font-mono text-xs text-muted-foreground">{item.serialNumber}</div></TableCell><TableCell className="text-xs text-muted-foreground">{new Date(item.createdAt).toLocaleDateString('vi-VN')}</TableCell><TableCell><Badge>{labels[item.status]}</Badge></TableCell><TableCell className="text-right" onClick={e => e.stopPropagation()}><Button variant="outline" size="sm" onClick={() => openDetail(item)}><Eye className="mr-1 h-3.5 w-3.5" />Xử lý</Button></TableCell></TableRow>)}{visible.length === 0 && <TableRow><TableCell colSpan={6} className="py-12 text-center text-muted-foreground">Không có yêu cầu phù hợp.</TableCell></TableRow>}</TableBody></Table></CardContent></Card>
    <Dialog open={!!selected} onOpenChange={open => !open && setSelected(null)}><DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto rounded-2xl"><DialogHeader><DialogTitle className="flex items-center gap-2"><Wrench className="h-5 w-5 text-primary" />Xử lý yêu cầu #{selected?.id}</DialogTitle><DialogDescription>{selected?.productName} · Serial {selected?.serialNumber} · {selected?.customerName}</DialogDescription></DialogHeader>
      <div className="space-y-5"><div className="rounded-xl border border-border bg-muted/30 p-4"><p className="text-xs font-bold uppercase text-muted-foreground">Mô tả lỗi</p><p className="mt-1 text-sm">{selected?.problemDescription}</p></div>
        {selected?.status === RepairRequestStatus.PENDING && <Button className="w-full" onClick={() => change(RepairRequestStatus.APPROVED, { repairType: 'REPAIR' })}><CheckCircle2 className="mr-2 h-4 w-4" />Duyệt yêu cầu</Button>}
        {selected?.status === RepairRequestStatus.APPROVED && <Button className="w-full" onClick={() => change(RepairRequestStatus.RECEIVED_CHECKING)}><PackageCheck className="mr-2 h-4 w-4" />Xác nhận đã nhận máy</Button>}
        {selected?.status === RepairRequestStatus.RECEIVED_CHECKING && <div className="space-y-4 rounded-xl border border-border bg-muted/20 p-4"><div><label className="mb-1 block text-xs font-medium">Kết quả kiểm tra</label><Textarea value={checking} onChange={e => setChecking(e.target.value)} rows={3} placeholder="Nhập kết quả kiểm tra..." /></div><div><label className="mb-1 block text-xs font-medium">Hướng xử lý</label><Textarea value={solution} onChange={e => setSolution(e.target.value)} rows={3} placeholder="Mô tả hướng xử lý..." /></div><div className="grid gap-3 sm:grid-cols-2"><div><label className="mb-1 block text-xs font-medium">Nơi sửa chữa</label><Select value={location} onValueChange={v => setLocation(v as 'AT_STORE' | 'AT_MANUFACTURER')}><SelectTrigger className="rounded-xl"><SelectValue /></SelectTrigger><SelectContent><SelectItem value="AT_STORE">Sửa tại cửa hàng</SelectItem><SelectItem value="AT_MANUFACTURER">Đem qua hãng xử lý</SelectItem></SelectContent></Select></div><div><label className="mb-1 block text-xs font-medium">Loại sửa chữa</label><Select value={repairType} onValueChange={v => setRepairType(v as 'WARRANTY' | 'REPAIR')}><SelectTrigger className="rounded-xl"><SelectValue /></SelectTrigger><SelectContent><SelectItem value="REPAIR">Sửa chữa thường</SelectItem><SelectItem value="WARRANTY">Bảo hành</SelectItem></SelectContent></Select></div></div><Button className="w-full" disabled={!checking.trim() || !solution.trim()} onClick={() => change(RepairRequestStatus.IN_PROGRESS, { processingLocation: location, repairType, checkingResult: checking, solutionDescription: solution })}><Wrench className="mr-2 h-4 w-4" />Bắt đầu xử lý</Button></div>}
        {selected?.status === RepairRequestStatus.IN_PROGRESS && <div className="space-y-4"><div className="grid gap-3 sm:grid-cols-2"><div><label className="mb-1 block text-xs font-medium">Kết quả kiểm tra</label><Textarea value={checking} onChange={e => setChecking(e.target.value)} rows={3} /></div><div><label className="mb-1 block text-xs font-medium">Hướng xử lý</label><Textarea value={solution} onChange={e => setSolution(e.target.value)} rows={3} /></div></div><div><label className="mb-1 block text-xs font-medium">Phí sửa chữa</label><Input type="number" min="0" value={repairFee} onChange={e => setRepairFee(e.target.value)} placeholder="Nhập phí sửa chữa" className="rounded-xl" /></div><div className="flex gap-2"><Button variant="outline" onClick={() => change(RepairRequestStatus.IN_PROGRESS, { processingLocation: 'AT_MANUFACTURER' })}><Factory className="mr-1 h-4 w-4" />Chuyển hãng</Button><Button onClick={() => change(RepairRequestStatus.COMPLETED, { checkingResult: checking, solutionDescription: solution, totalAmount: Number(repairFee) || 0 })}><CheckCircle2 className="mr-1 h-4 w-4" />Hoàn tất</Button></div></div>}
        {selected?.status === RepairRequestStatus.COMPLETED && <div className="rounded-xl border border-emerald-500/30 bg-emerald-500/10 p-4 text-sm">Yêu cầu đã hoàn tất xử lý.</div>}
      </div><DialogFooter><Button variant="outline" onClick={() => setSelected(null)}>Đóng</Button></DialogFooter>
    </DialogContent></Dialog>
  </div>;
}
