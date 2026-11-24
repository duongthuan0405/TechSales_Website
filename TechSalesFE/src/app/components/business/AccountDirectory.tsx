import { useState } from 'react';
import { Eye, Loader2, Search, Mail, Phone, CalendarDays, Fingerprint, ShieldCheck } from 'lucide-react';
import { User } from '../../../models/ui_types/user';
import { formatDate } from '../../../utils/format';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Badge } from '../ui/badge';
import { Card, CardContent } from '../ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../ui/table';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '../ui/dialog';
import { useDebouncedValue } from '../../../dataHook/useDebouncedValue';
import { Avatar, AvatarImage, AvatarFallback } from '../ui/avatar';
import { AccountStatusActions } from './AccountStatusActions';
import { useAccountPage } from '../../../dataHook/userDataHook';
import { ListPagination } from '../ui/list-pagination';

const statuses: Record<string, string> = { ACTIVE: 'Đang hoạt động', BLOCKED: 'Đã khóa', PENDING: 'Chờ xác minh', INACTIVE: 'Ngừng hoạt động' };
const statusColors: Record<string, string> = {
  ACTIVE: 'border-emerald-200 bg-emerald-500/10 text-emerald-700 dark:text-emerald-400',
  BLOCKED: 'border-amber-200 bg-amber-500/10 text-amber-700 dark:text-amber-400',
  PENDING: 'border-blue-200 bg-blue-500/10 text-blue-700 dark:text-blue-400',
  INACTIVE: 'border-slate-200 bg-slate-500/10 text-slate-600 dark:text-slate-400',
};
const roles: Record<string, string> = { Customer: 'Khách hàng', Staff: 'Nhân viên bán hàng', 'Business Admin': 'Quản trị kinh doanh', 'Technical Admin': 'Quản trị kỹ thuật' };
function AccountAvatar({ user, large = false }: { user: User; large?: boolean }) {
  return <Avatar className={large ? 'size-20 border-4 border-background shadow-sm' : 'size-10'}>
    <AvatarImage src={user.avatarUrl} alt={user.fullName || user.email} className="object-cover" />
    <AvatarFallback className="bg-primary/10 font-semibold text-primary">{(user.fullName.trim() || user.email).split(/\s+/).slice(-2).map(word => word[0]).join('').toUpperCase()}</AvatarFallback>
  </Avatar>;
}
function StatusBadge({ user }: { user: User }) {
  return <Badge variant="outline" className={`gap-1.5 font-medium ${statusColors[user.status] ?? ''}`}><span className="size-1.5 rounded-full bg-current" />{statuses[user.status] ?? user.status}</Badge>;
}

export function AccountDirectory({ title, kind }: { title: string; kind: 'customers' | 'staff' }) {
  const [search, setSearch] = useState('');
  const keyword = useDebouncedValue(search).trim();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const { data, isLoading, isFetching, error } = useAccountPage(kind, {
    pageNumber, pageSize, keyword, role: kind === 'staff' ? 'Staff' : undefined,
  });
  const users = data?.items ?? [];
  const [selectedId, setSelectedId] = useState<string>();
  const selected = users.find(user => user.id === selectedId);
  const items = users;
  return <div className="space-y-6">
    <div><h1 className="text-3xl font-bold">{title}</h1><p className="mt-2 text-muted-foreground">Xem hồ sơ và quản lý trạng thái tài khoản. Chỉnh sửa thông tin do quản trị kỹ thuật thực hiện.</p></div>
    <div className="relative"><Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <Input className="h-11 pl-9" aria-label="Tìm tài khoản" placeholder="Tìm theo tên, email hoặc số điện thoại…" value={search} onChange={event => { setSearch(event.target.value); setPageNumber(1); }} /></div>
    {error ? <p role="alert" className="text-destructive">{error.message}</p>
      : isLoading ? <div className="flex justify-center py-16"><Loader2 className="animate-spin" aria-label="Đang tải tài khoản" /></div>
      : <Card><CardContent className="pt-6"><Table><TableHeader><TableRow>
        <TableHead>Họ và tên</TableHead><TableHead>Email</TableHead><TableHead>Số điện thoại</TableHead><TableHead>Trạng thái</TableHead><TableHead>Ngày tham gia</TableHead><TableHead className="text-right">Thao tác</TableHead>
      </TableRow></TableHeader><TableBody>
        {items.map(user => <TableRow key={user.id}>
          <TableCell><div className="flex items-center gap-3"><AccountAvatar user={user} /><span className="font-semibold">{user.fullName || 'Chưa cập nhật'}</span></div></TableCell><TableCell>{user.email}</TableCell><TableCell>{user.phone || 'Chưa cập nhật'}</TableCell>
          <TableCell><StatusBadge user={user} /></TableCell><TableCell>{formatDate(user.createdAt)}</TableCell>
          <TableCell><div className="flex items-center justify-end"><Button variant="ghost" size="icon" aria-label={`Xem tài khoản ${user.email}`} title="Xem chi tiết" onClick={() => setSelectedId(user.id)}><Eye className="h-4 w-4" /></Button><AccountStatusActions target={user} /></div></TableCell>
        </TableRow>)}
        {!items.length && <TableRow><TableCell colSpan={6} className="py-10 text-center text-muted-foreground">Không tìm thấy tài khoản phù hợp.</TableCell></TableRow>}
      </TableBody></Table></CardContent></Card>}
    <ListPagination pageNumber={pageNumber} pageSize={pageSize} totalCount={data?.totalCount ?? 0}
      disabled={isFetching || !!error || search.trim() !== keyword} onPageChange={setPageNumber}
      onPageSizeChange={size => { setPageSize(size); setPageNumber(1); }} />
    <Dialog open={!!selected} onOpenChange={open => { if (!open) setSelectedId(undefined); }}><DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-xl">
      <DialogHeader><DialogTitle>Thông tin tài khoản</DialogTitle><DialogDescription>Thông tin hồ sơ và trạng thái hiện tại.</DialogDescription></DialogHeader>
      {selected && <div className="space-y-5">
        <div className="flex flex-wrap items-center gap-4 rounded-xl border border-primary/10 bg-primary/5 p-5">
          <AccountAvatar user={selected} large /><div className="min-w-0 flex-1 space-y-2">
            <h2 className="break-words text-xl font-bold">{selected.fullName || 'Chưa cập nhật họ tên'}</h2>
            <div className="flex flex-wrap gap-2"><Badge variant="outline" className="gap-1 bg-background font-medium"><ShieldCheck className="size-3.5 text-primary" />{roles[selected.role] ?? selected.role}</Badge><StatusBadge user={selected} /></div>
          </div>
        </div>
        <dl className="grid gap-3 sm:grid-cols-2">
          {[
            { label: 'Email', value: selected.email, Icon: Mail },
            { label: 'Số điện thoại', value: selected.phone || 'Chưa cập nhật', Icon: Phone },
            { label: 'Ngày tham gia', value: formatDate(selected.createdAt), Icon: CalendarDays },
            { label: 'Ngày sinh', value: selected.dateOfBirth ? formatDate(selected.dateOfBirth) : 'Chưa cập nhật', Icon: CalendarDays },
          ].map(({ label, value, Icon }) => <div key={label} className="rounded-xl border bg-muted/20 p-4">
            <dt className="mb-2 flex items-center gap-2 text-xs font-medium text-muted-foreground"><Icon className="size-4 text-primary/70" />{label}</dt><dd className="break-words text-sm font-semibold">{value}</dd>
          </div>)}
          <div className="rounded-xl border bg-muted/20 p-4 sm:col-span-2"><dt className="mb-2 flex items-center gap-2 text-xs font-medium text-muted-foreground"><Fingerprint className="size-4 text-primary/70" />Mã tài khoản</dt><dd className="break-all font-mono text-xs text-muted-foreground">{selected.id}</dd></div>
        </dl>
        <div className="flex items-center justify-between border-t pt-4"><span className="text-sm font-medium">Quản lý trạng thái</span><AccountStatusActions target={selected} /></div>
      </div>}
      <Button variant="outline" className="justify-self-end" onClick={() => setSelectedId(undefined)}>Đóng</Button>
    </DialogContent></Dialog>
  </div>;
}
