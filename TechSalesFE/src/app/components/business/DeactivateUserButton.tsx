import { useState } from 'react';
import { UserX } from 'lucide-react';
import { User, UserStatus } from '../../../models/ui_types/user';
import { useDeactivateUser } from '../../../dataHook/userDataHook';
import { useAuth } from '../../context/AuthContext';
import { toast } from 'sonner';
import { Button } from '../ui/button';
import { AlertDialog, AlertDialogContent, AlertDialogHeader, AlertDialogTitle, AlertDialogDescription, AlertDialogFooter, AlertDialogCancel, AlertDialogAction } from '../ui/alert-dialog';

export function DeactivateUserButton({ target, onDeactivated }: { target: User; onDeactivated?: () => void }) {
  const { user } = useAuth();
  const [open, setOpen] = useState(false);
  const mutation = useDeactivateUser();
  const isTechnical = user?.role === 'Technical Admin';
  const targetRoles = target.roles?.length ? target.roles : [target.role];
  const isBusiness = user?.role === 'Business Admin' && targetRoles.every(role => role === 'Customer' || role === 'Staff');
  const allowed = (isTechnical || isBusiness)
    && user?.id !== target.id && target.status !== UserStatus.INACTIVE;
  if (!allowed) return null;
  return <>
    <Button type="button" variant="ghost" size="icon" className="text-destructive" aria-label="Ngừng hoạt động tài khoản" title="Ngừng hoạt động tài khoản" onClick={event => { event.stopPropagation(); setOpen(true); }}><UserX className="h-4 w-4" /></Button>
    <AlertDialog open={open} onOpenChange={value => { if (!mutation.isPending) setOpen(value); }}>
      <AlertDialogContent onClick={event => event.stopPropagation()}><AlertDialogHeader>
        <AlertDialogTitle>Ngừng hoạt động tài khoản?</AlertDialogTitle>
        <AlertDialogDescription>Tài khoản {target.email} sẽ chuyển sang ngừng hoạt động và không thể đăng nhập hoặc tiếp tục sử dụng phiên hiện tại. Thao tác này không xóa dữ liệu. Hiện chưa có chức năng kích hoạt lại.</AlertDialogDescription>
      </AlertDialogHeader><AlertDialogFooter>
        <AlertDialogCancel disabled={mutation.isPending}>Hủy</AlertDialogCancel>
        <AlertDialogAction disabled={mutation.isPending} className="bg-destructive text-destructive-foreground hover:bg-destructive/90" onClick={event => {
          event.preventDefault();
          mutation.mutate(target.id, { onSuccess: () => { setOpen(false); onDeactivated?.(); toast.success('Đã ngừng hoạt động tài khoản.'); },
            onError: (error: Error) => toast.error(error.message) });
        }}>{mutation.isPending ? 'Đang xử lý…' : 'Ngừng hoạt động'}</AlertDialogAction>
      </AlertDialogFooter></AlertDialogContent>
    </AlertDialog>
  </>;
}
