import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router';
import { Button } from '../../components/ui/button';
import { Card, CardDescription, CardHeader, CardTitle } from '../../components/ui/card';
import { useVerifyEmailMutation } from '../../../dataHook/authDataHook';
import { ShoppingBag, Loader2, CheckCircle2, XCircle, Mail } from 'lucide-react';
import { toast } from 'sonner';

export function VerifyEmailPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const email = searchParams.get('email') || '';
  
  const { mutate: verifyEmail, isPending: isLoading } = useVerifyEmailMutation();
  const [status, setStatus] = useState<'pending' | 'success' | 'error'>('pending');

  useEffect(() => {
    if (token) {
      verifyEmail({ token, email }, {
        onSuccess: () => {
          setStatus('success');
          toast.success('Xác thực email thành công!');
        },
        onError: () => {
          setStatus('error');
          toast.error('Xác thực email thất bại.');
        }
      });
    } else {
      setStatus('error');
    }
  }, [token, email, verifyEmail]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4 dark:from-slate-950 dark:to-slate-900">
      <div className="w-full max-w-md space-y-8 text-center">
        <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-600 shadow-lg shadow-blue-500/20">
          <ShoppingBag className="h-8 w-8 text-white" />
        </div>

        <Card className="border-none shadow-2xl p-8">
          {status === 'pending' && (
            <div className="space-y-6">
              <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-blue-100 text-blue-600 dark:bg-blue-900/30">
                <Loader2 className="h-10 w-10 animate-spin" />
              </div>
              <CardHeader className="p-0">
                <CardTitle className="text-2xl">Đang xác thực email</CardTitle>
                <CardDescription className="text-base mt-2">
                  Vui lòng đợi trong giây lát khi chúng tôi xác minh thông tin...
                </CardDescription>
              </CardHeader>
            </div>
          )}

          {status === 'success' && (
            <div className="space-y-6">
              <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-emerald-100 text-emerald-600 dark:bg-emerald-900/30">
                <CheckCircle2 className="h-10 w-10" />
              </div>
              <CardHeader className="p-0">
                <CardTitle className="text-2xl">Kích hoạt tài khoản thành công</CardTitle>
                <CardDescription className="text-base mt-2">
                  Email của bạn đã được xác thực thành công. Bạn có thể sử dụng tất cả quyền lợi thành viên ngay bây giờ.
                </CardDescription>
              </CardHeader>
              <Button className="w-full h-12 text-lg font-bold" onClick={() => navigate('/auth/login')}>
                Đăng nhập ngay
              </Button>
            </div>
          )}

          {status === 'error' && (
            <div className="space-y-6">
              <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-destructive/10 text-destructive">
                <XCircle className="h-10 w-10" />
              </div>
              <CardHeader className="p-0">
                <CardTitle className="text-2xl">Xác thực thất bại</CardTitle>
                <CardDescription className="text-base mt-2">
                  Liên kết không hợp lệ hoặc đã hết hạn. Vui lòng thử yêu cầu gửi lại email xác thực mới.
                </CardDescription>
              </CardHeader>
              <div className="grid grid-cols-1 gap-3 pt-4">
                <Button className="h-12 text-lg font-bold" onClick={() => navigate('/auth/login')}>
                  Về trang đăng nhập
                </Button>
                <Button variant="outline" className="h-12 text-lg font-bold" onClick={() => toast.info('Đã yêu cầu gửi liên kết mới')}>
                  <Mail className="mr-2 h-5 w-5" />
                  Gửi lại email
                </Button>
              </div>
            </div>
          )}
        </Card>
      </div>
    </div>
  );
}
