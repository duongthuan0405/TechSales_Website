import { useState } from 'react';
import { Lock, Unlock } from 'lucide-react';
import { toast } from 'sonner';
import { User, UserStatus } from '../../../models/ui_types/user';
import { useToggleUserStatus } from '../../../dataHook/userDataHook';
import { useAuth } from '../../context/AuthContext';
import { DeactivateUserButton } from './DeactivateUserButton';
import { Button } from '../ui/button';
import { AlertDialog, AlertDialogContent, AlertDialogHeader, AlertDialogTitle, AlertDialogDescription, AlertDialogFooter, AlertDialogCancel, AlertDialogAction } from '../ui/alert-dialog';

export function AccountStatusActions({ target }: { target: User }) {
  const { user } = useAuth();
  const [open, setOpen] = useState(false);
  const mutation = useToggleUserStatus();
  const roles = target.roles?.length ? target.roles : [target.role];
  const allowed = user?.id !== target.id && (user?.role === 'Technical Admin'
    || (user?.role === 'Business Admin' && roles.every(role => role === 'Staff' || role === 'Customer')));
  if (!allowed) return null;
  const unlocking = target.status === UserStatus.BLOCKED;
  const label = unlocking ? 'Mở khóa tài khoản' : 'Khóa tài khoản';
  return <div className="flex items-center gap-1" onClick={event => event.stopPropagation()}>
    {(target.status === UserStatus.ACTIVE || unlocking) && <Button type="button" variant="ghost" size="icon" aria-label={label} title={label} onClick={() => setOpen(true)}>
      {unlocking ? <Unlock className="h-4 w-4 text-emerald-600" /> : <Lock className="h-4 w-4 text-amber-600" />}
    </Button>}
    <DeactivateUserButton target={target} />
    <AlertDialog open={open} onOpenChange={value => { if (!mutation.isPending) setOpen(value); }}><AlertDialogContent><AlertDialogHeader>
      <AlertDialogTitle>{label}?</AlertDialogTitle><AlertDialogDescription>
        {unlocking ? `Cho phép tài khoản ${target.email} hoạt động trở lại.` : `Tạm chặn tài khoản ${target.email} đăng nhập và sử dụng hệ thống cho đến khi được mở khóa.`}
      </AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel disabled={mutation.isPending}>Hủy</AlertDialogCancel>
        <AlertDialogAction disabled={mutation.isPending} onClick={event => {
          event.preventDefault(); mutation.mutate({ id: target.id, status: target.status }, {
            onSuccess: () => { setOpen(false); toast.success(unlocking ? 'Đã mở khóa tài khoản.' : 'Đã khóa tài khoản.'); },
            onError: (error: Error) => toast.error(error.message),
          });
        }}>{mutation.isPending ? 'Đang xử lý…' : 'Xác nhận'}</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog>
  </div>;
}
