import { useState } from 'react';
import { useNavigate, Link } from 'react-router';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '../../components/ui/card';
import { useAuth } from '../../context/AuthContext';
import { useResetPasswordMutation } from '../../../dataHook/authDataHook';
import { ShoppingBag, ArrowLeft, Loader2, MailCheck } from 'lucide-react';
import { toast } from 'sonner';

export function ForgotPasswordPage() {
  const navigate = useNavigate();
  const { mutate: resetPassword, isPending: isLoading } = useResetPasswordMutation();
  const [email, setEmail] = useState('');
  const [isSent, setIsSent] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    resetPassword(email, {
      onSuccess: () => {
        setIsSent(true);
        toast.success('Đã gửi liên kết đặt lại mật khẩu đến email của bạn');
      },
      onError: (err: any) => {
        toast.error(err.message || 'Không thể gửi liên kết đặt lại mật khẩu');
      }
    });
  };

  if (isSent) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4 dark:from-slate-950 dark:to-slate-900">
        <div className="w-full max-w-md">
          <Card className="border-none shadow-2xl text-center p-8">
            <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-emerald-100 text-emerald-600 dark:bg-emerald-900/30">
              <MailCheck className="h-10 w-10" />
            </div>
            <CardTitle className="text-2xl mb-2">Kiểm tra email của bạn</CardTitle>
            <CardDescription className="text-base mb-8">
              Chúng tôi đã gửi liên kết đặt lại mật khẩu đến <span className="font-bold text-slate-900 dark:text-white">{email}</span>
            </CardDescription>
            <Button className="w-full h-12" onClick={() => navigate('/auth/login')}>
              Quay lại Đăng nhập
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
          <h1 className="text-4xl font-bold tracking-tight">Quên mật khẩu?</h1>
          <p className="mt-2 text-muted-foreground">Đừng lo lắng, chúng tôi sẽ gửi hướng dẫn đặt lại mật khẩu cho bạn</p>
        </div>

        <Card className="border-none shadow-2xl">
          <CardHeader>
            <CardTitle>Đặt lại mật khẩu</CardTitle>
            <CardDescription>Nhập địa chỉ email của bạn để nhận liên kết khôi phục</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="email">Địa chỉ Email</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="john@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </div>

              <Button type="submit" className="w-full h-12 text-lg font-bold" disabled={isLoading}>
                {isLoading ? <Loader2 className="mr-2 h-5 w-5 animate-spin" /> : null}
                {isLoading ? 'Đang gửi liên kết...' : 'Gửi liên kết đặt lại'}
              </Button>
            </form>
          </CardContent>
          <CardFooter className="flex flex-col space-y-4 border-t border-border/50 pt-6">
            <div className="text-center text-sm">
              Bạn nhớ mật khẩu rồi?{' '}
              <Link to="/auth/login" className="font-bold text-blue-600 hover:underline">
                Đăng nhập
              </Link>
            </div>
          </CardFooter>
        </Card>
      </div>
    </div>
  );
}
