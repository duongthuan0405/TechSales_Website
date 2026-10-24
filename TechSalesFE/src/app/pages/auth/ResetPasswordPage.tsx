import { useState } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '../../components/ui/card';
import { useConfirmResetMutation } from '../../../dataHook/authDataHook';
import { ShoppingBag, Lock, Loader2, CheckCircle2, Key } from 'lucide-react';
import { toast } from 'sonner';

export function ResetPasswordPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialToken = searchParams.get('token') || '';
  const email = searchParams.get('email') || '';
  
  const { mutate: confirmReset, isPending: isLoading } = useConfirmResetMutation();
  const [token, setToken] = useState(initialToken);
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isSuccess, setIsSuccess] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (password !== confirmPassword) {
      toast.error('Mật khẩu xác nhận không khớp');
      return;
    }

    if (!token) {
      toast.error('Vui lòng nhập mã OTP đã được gửi đến email của bạn');
      return;
    }

    confirmReset({ password, confirmPassword, token, email }, {
      onSuccess: () => {
        setIsSuccess(true);
        toast.success('Cập nhật mật khẩu thành công');
      },
      onError: (err: any) => {
        toast.error(err.message || 'Không thể cập nhật mật khẩu');
      }
    });
  };

  if (isSuccess) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4 dark:from-slate-950 dark:to-slate-900">
        <div className="w-full max-w-md">
          <Card className="border-none shadow-2xl text-center p-8">
            <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-emerald-100 text-emerald-600 dark:bg-emerald-900/30">
              <CheckCircle2 className="h-10 w-10" />
            </div>
            <CardTitle className="text-2xl mb-2">Đặt lại mật khẩu thành công</CardTitle>
            <CardDescription className="text-base mb-8">
              Mật khẩu của bạn đã được cập nhật thành công. Bây giờ bạn có thể đăng nhập bằng thông tin mới.
            </CardDescription>
            <Button className="w-full h-12 text-lg font-bold" onClick={() => navigate('/auth/login')}>
              Đăng nhập ngay
            </Button>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4 dark:from-slate-950 dark:to-slate-900">
      <div className="w-full max-w-md space-y-8">
        <div className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-600 shadow-lg shadow-blue-500/20">
            <ShoppingBag className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-4xl font-bold tracking-tight">Đặt lại mật khẩu</h1>
          <p className="mt-2 text-muted-foreground">Nhập mã OTP và mật khẩu mới của bạn</p>
        </div>

        <Card className="border-none shadow-2xl">
          <CardHeader>
            <CardTitle>Cập nhật bảo mật</CardTitle>
            <CardDescription>Xác thực danh tính của bạn và thiết lập mật khẩu mới</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="token">Mã OTP</Label>
                <div className="relative">
                  <Key className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    id="token"
                    placeholder="Nhập mã gồm 6 chữ số"
                    className="pl-10"
                    value={token}
                    onChange={(e) => setToken(e.target.value)}
                    required
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="password">Mật khẩu mới</Label>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    id="password"
                    type="password"
                    placeholder="••••••••"
                    className="pl-10"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="confirmPassword">Xác nhận mật khẩu mới</Label>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    id="confirmPassword"
                    type="password"
                    placeholder="••••••••"
                    className="pl-10"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    required
                  />
                </div>
              </div>

              <Button type="submit" className="w-full h-12 text-lg font-bold mt-4" disabled={isLoading}>
                {isLoading ? <Loader2 className="mr-2 h-5 w-5 animate-spin" /> : null}
                {isLoading ? 'Đang cập nhật...' : 'Cập nhật mật khẩu'}
              </Button>
            </form>
          </CardContent>
          <CardFooter className="flex justify-center border-t border-border/50 pt-4">
            <Link to="/auth/login" className="text-sm font-medium text-blue-600 hover:underline">
              Quay lại Đăng nhập
            </Link>
          </CardFooter>
        </Card>
      </div>
    </div>
  );
}
