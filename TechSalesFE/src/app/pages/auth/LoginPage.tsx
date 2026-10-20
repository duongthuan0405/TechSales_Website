import { useState } from 'react';
import { useNavigate, Link } from 'react-router';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '../../components/ui/card';
import { useAuth } from '../../context/AuthContext';
import { useLoginMutation } from '../../../dataHook/authDataHook';
import { ShoppingBag, Loader2, KeyRound } from 'lucide-react';
import { toast } from 'sonner';

export function LoginPage() {
  const navigate = useNavigate();
  const { mutate: login, isPending: isLoading, error: loginError } = useLoginMutation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    login({ email, password }, {
      onSuccess: () => {
        toast.success('Chào mừng bạn quay trở lại!');
        navigate('/');
      },
      onError: (err: any) => {
        toast.error(err.message || 'Đăng nhập thất bại');
      }
    });
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4 dark:from-slate-950 dark:to-slate-900">
      <div className="w-full max-w-md space-y-8">
        <div className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-600 shadow-lg shadow-blue-500/20">
            <ShoppingBag className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-4xl font-bold tracking-tight">TechSales</h1>
          <p className="mt-2 text-muted-foreground">Sàn giao dịch thiết bị công nghệ chính hãng</p>
        </div>

        <Card className="border-none shadow-2xl">
          <CardHeader>
            <CardTitle>Đăng Nhập</CardTitle>
            <CardDescription>Đăng nhập để tiếp tục trải nghiệm mua sắm</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="email">Địa chỉ Email</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="name@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </div>
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label htmlFor="password">Mật khẩu</Label>
                  <Link to="/auth/forgot-password" className="text-xs font-medium text-blue-600 hover:underline">
                    Quên mật khẩu?
                  </Link>
                </div>
                <Input
                  id="password"
                  type="password"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
              </div>
              
              {loginError && (
                <div className="flex items-center gap-2 rounded-lg bg-destructive/10 p-3 text-sm text-destructive">
                  <KeyRound className="h-4 w-4" />
                  {(loginError as any).message || 'Đăng nhập thất bại'}
                </div>
              )}

              <Button type="submit" className="w-full h-12 text-lg font-bold" disabled={isLoading}>
                {isLoading ? <Loader2 className="mr-2 h-5 w-5 animate-spin" /> : null}
                {isLoading ? 'Đang đăng nhập...' : 'Đăng Nhập'}
              </Button>
            </form>

            <div className="mt-6 border-t border-border/50 pt-4">
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-2 text-center">
                Tài khoản mẫu (Nhấp để chọn)
              </p>
              <div className="grid grid-cols-2 gap-2">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="text-xs justify-start h-9"
                  onClick={() => {
                    setEmail('customer@email.com');
                    setPassword('password');
                  }}
                >
                  <span className="h-2 w-2 rounded-full bg-emerald-500 mr-1.5 shrink-0" />
                  Khách hàng
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="text-xs justify-start h-9"
                  onClick={() => {
                    setEmail('sales@techsales.com');
                    setPassword('password');
                  }}
                >
                  <span className="h-2 w-2 rounded-full bg-blue-500 mr-1.5 shrink-0" />
                  Nhân viên bán hàng
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="text-xs justify-start h-9"
                  onClick={() => {
                    setEmail('business@techsales.com');
                    setPassword('password');
                  }}
                >
                  <span className="h-2 w-2 rounded-full bg-purple-500 mr-1.5 shrink-0" />
                  Quản trị kinh doanh
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="text-xs justify-start h-9"
                  onClick={() => {
                    setEmail('admin@techsales.com');
                    setPassword('password');
                  }}
                >
                  <span className="h-2 w-2 rounded-full bg-rose-500 mr-1.5 shrink-0" />
                  Quản trị kỹ thuật
                </Button>
              </div>
            </div>
          </CardContent>
          <CardFooter className="flex justify-center border-t border-border/50 pt-6">
            <div className="text-sm">
              Chưa có tài khoản?{' '}
              <Link to="/auth/signup" className="font-bold text-blue-600 hover:underline">
                Đăng ký ngay
              </Link>
            </div>
          </CardFooter>
        </Card>
      </div>
    </div>
  );
}
