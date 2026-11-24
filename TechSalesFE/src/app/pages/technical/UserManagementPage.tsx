import { DeactivateUserButton } from '../../components/business/DeactivateUserButton';
import { useDebouncedValue } from '../../../dataHook/useDebouncedValue';
import { useState, useEffect } from 'react';
import { formatDate } from '../../../utils/format';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "../../components/ui/dialog";
import { UserForm } from "../../components/business/UserForm";
import { Search, Edit, Trash2, UserPlus, Loader2, Lock, Unlock } from 'lucide-react';
import { useAccountPage, useToggleUserStatus, useCreateUser, useUpdateUser } from '../../../dataHook/userDataHook';
import { ListPagination } from '../../components/ui/list-pagination';
import { toast } from 'sonner';
import { useAuth } from '../../context/AuthContext';
import { User, UserStatus } from '../../../models/ui_types/user';
import { Avatar, AvatarImage, AvatarFallback } from '../../components/ui/avatar';

const roleLabels: Record<string, string> = {
  Customer: 'Khách hàng', Staff: 'Nhân viên bán hàng',
  'Business Admin': 'Quản trị kinh doanh', 'Technical Admin': 'Quản trị kỹ thuật',
};
const statusLabels: Record<string, string> = {
  ACTIVE: 'Đang hoạt động', BLOCKED: 'Đã khóa', PENDING: 'Chờ xác minh', INACTIVE: 'Ngừng hoạt động',
};

export function UserManagementPage() {
  const { user: currentUser } = useAuth();
  const [roleFilter, setRoleFilter] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearch = useDebouncedValue(searchTerm).trim();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const { data, isLoading, isFetching, isError, error } = useAccountPage('all', {
    pageNumber, pageSize, role: roleFilter, keyword: debouncedSearch,
  });
  const users = data?.items ?? [];
  const { mutate: toggleStatus, isPending: isToggling } = useToggleUserStatus();
  const createMutation = useCreateUser();
  const updateMutation = useUpdateUser();
  const [editingUser, setEditingUser] = useState<User>();

  const [isFormOpen, setIsFormOpen] = useState(false);

  useEffect(() => {
    if (isError) {
      toast.error(error?.message || 'Không tải được danh sách người dùng.');
    }
  }, [isError, error]);

  const handleToggleStatus = (id: string, currentStatus: UserStatus) => {
    if (currentStatus !== UserStatus.ACTIVE && currentStatus !== UserStatus.BLOCKED) {
      return;
    }
    const action = currentStatus === 'BLOCKED' ? 'mở khóa' : 'khóa';
    if (window.confirm(`Bạn có chắc muốn ${action} tài khoản này?`)) {
      toggleStatus({ id, status: currentStatus }, {
        onSuccess: () => toast.success(`Đã ${action} tài khoản thành công.`),
        onError: (error) => toast.error(error.message)
      });
    }
  };

  const filteredUsers = users;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Quản lý người dùng</h1>
          <p className="text-muted-foreground">Quản lý tài khoản và vai trò người dùng trong hệ thống</p>
        </div>
        <Button onClick={() => { setEditingUser(undefined); setIsFormOpen(true); }}>
          <UserPlus className="mr-2 h-4 w-4" />
          Thêm người dùng
        </Button>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Tìm người dùng theo tên hoặc email..."
            value={searchTerm}
            onChange={(e) => { setSearchTerm(e.target.value); setPageNumber(1); }}
            className="pl-9"
          />
        </div>
        <Select value={roleFilter} onValueChange={role => { setRoleFilter(role); setPageNumber(1); }}>
          <SelectTrigger className="w-full sm:w-[240px]" aria-label="Lọc theo vai trò">
            <SelectValue placeholder="Tất cả vai trò" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Tất cả vai trò</SelectItem>
            {Object.entries(roleLabels).map(([value, label]) => <SelectItem key={value} value={value}>{label}</SelectItem>)}
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-muted-foreground">Đang tải danh sách người dùng…</p>
          </div>
        </div>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>Người dùng ({data?.totalCount ?? 0})</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Người dùng</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Vai trò</TableHead>
                  <TableHead>Trạng thái</TableHead>
                  <TableHead>Ngày tạo</TableHead>
                  <TableHead>Thao tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredUsers.map(user => (
                  <TableRow key={user.id}>
                    <TableCell>
                      <div className="flex items-center gap-3">
                        <Avatar className="size-10"><AvatarImage src={user.avatarUrl} alt={user.fullName || user.email} className="object-cover" /><AvatarFallback className="bg-primary/10 font-semibold text-primary">{(user.fullName.trim() || user.email).charAt(0).toUpperCase()}</AvatarFallback></Avatar>
                        <div className="font-medium">{user.fullName.trim() || 'Chưa cập nhật'}</div>
                      </div>
                    </TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>
                      <div className="flex flex-wrap gap-1">{(user.roles?.length ? user.roles : [user.role]).map(role => <Badge key={role} variant="secondary">{roleLabels[role] ?? role}</Badge>)}</div>
                    </TableCell>
                    <TableCell>
                      <Badge variant={user.status === 'ACTIVE' ? 'success' : 'secondary'}>
                        {statusLabels[user.status] ?? user.status}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-sm text-muted-foreground">{formatDate(user.createdAt)}</TableCell>
                    <TableCell>
                      <div className="flex flex-wrap gap-2">
                        <DeactivateUserButton target={user} />
                        {(user.status === UserStatus.ACTIVE || user.status === UserStatus.BLOCKED) ? <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleToggleStatus(user.id, user.status)}
                          disabled={isToggling || user.id === currentUser?.id}
                          aria-label={user.status === 'BLOCKED' ? 'Mở khóa tài khoản' : 'Khóa tài khoản'}
                          title={user.status === 'BLOCKED' ? 'Mở khóa tài khoản' : 'Khóa tài khoản'}
                        >
                          {user.status === 'ACTIVE' ? (
                            <Lock className="h-4 w-4 text-orange-500" />
                          ) : (
                            <Unlock className="h-4 w-4 text-green-500" />
                          )}
                        </Button> : <span className="self-center text-xs text-muted-foreground">
                          {user.status === UserStatus.PENDING ? 'Chờ xác minh' : 'Ngừng hoạt động'}
                        </span>}
                        <Button variant="ghost" size="sm" aria-label="Sửa tài khoản" title="Sửa tài khoản" disabled={user.id === currentUser?.id}
                          onClick={() => { setEditingUser(user); setIsFormOpen(true); }}>
                          <Edit className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      <ListPagination pageNumber={pageNumber} pageSize={pageSize} totalCount={data?.totalCount ?? 0}
        disabled={isFetching || isError || searchTerm.trim() !== debouncedSearch} onPageChange={setPageNumber}
        onPageSizeChange={size => { setPageSize(size); setPageNumber(1); }} />

      {/* Form Dialog */}
      <Dialog open={isFormOpen} onOpenChange={open => { if (!createMutation.isPending && !updateMutation.isPending) setIsFormOpen(open); }}>
        <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-xl">
          <DialogHeader>
            <DialogTitle>{editingUser ? 'Sửa tài khoản' : 'Thêm tài khoản'}</DialogTitle>
            <DialogDescription>
              {editingUser ? 'Xem hồ sơ, chỉnh sửa email và gán vai trò cho tài khoản.' : 'Nhập email, mật khẩu và chọn một hoặc nhiều vai trò.'}
            </DialogDescription>
          </DialogHeader>
          <UserForm
            key={editingUser?.id ?? 'create'}
            initialData={editingUser}
            onSubmit={(data) => {
              if (editingUser) {
                updateMutation.mutate({ id: editingUser.id, data }, {
                  onSuccess: () => { toast.success('Đã cập nhật tài khoản.'); setIsFormOpen(false); },
                  onError: (error: Error) => toast.error(error.message),
                });
                return;
              }
              createMutation.mutate(data, {
                onSuccess: () => {
                  toast.success("Đã tạo tài khoản người dùng thành công.");
                  setIsFormOpen(false);
                },
                onError: (err: any) => {
                  toast.error(err?.message || "Không tạo được tài khoản người dùng.");
                }
              });
            }}
            isLoading={createMutation.isPending || updateMutation.isPending}
            currentUserRole={currentUser?.role}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}
