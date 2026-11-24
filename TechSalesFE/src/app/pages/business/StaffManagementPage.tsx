import { AccountDirectory } from '../../components/business/AccountDirectory';
import { useState } from 'react';
import { UserPlus } from 'lucide-react';
import { toast } from 'sonner';
import { useAuth } from '../../context/AuthContext';
import { useCreateUser } from '../../../dataHook/userDataHook';
import { UserForm } from '../../components/business/UserForm';
import { Button } from '../../components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '../../components/ui/dialog';

export function StaffManagementPage() {
  const { user } = useAuth();
  const [open, setOpen] = useState(false);
  const create = useCreateUser();
  const canCreate = user?.role === 'Business Admin' || user?.role === 'Technical Admin';
  return <>
    <AccountDirectory title="Quản lý nhân viên" kind="staff" headerActions={canCreate &&
      <Button onClick={() => setOpen(true)}><UserPlus className="mr-2 size-4" />Thêm nhân viên</Button>} />
    {canCreate && <Dialog open={open} onOpenChange={value => { if (!create.isPending) setOpen(value); }}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-xl"><DialogHeader>
        <DialogTitle>Thêm nhân viên bán hàng</DialogTitle><DialogDescription>Tạo tài khoản nhân viên bằng email và mật khẩu tạm thời.</DialogDescription>
      </DialogHeader>
      <UserForm forcedRole="Staff" currentUserRole={user?.role} isLoading={create.isPending} onSubmit={data => create.mutate({ ...data, role: 'Staff', roles: ['Staff'] }, {
        onSuccess: () => { toast.success('Đã tạo tài khoản nhân viên.'); setOpen(false); },
        onError: (error: Error) => toast.error(error.message),
      })} />
      </DialogContent>
    </Dialog>}
  </>;
}
