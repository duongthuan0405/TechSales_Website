import { useState } from 'react';
import { useNavigate, Link } from 'react-router';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '../../components/ui/card';
import { useAuth } from '../../context/AuthContext';
import { useRegisterMutation } from '../../../dataHook/authDataHook';
import { ShoppingBag, ArrowLeft, Loader2, CheckCircle2 } from 'lucide-react';
import { toast } from 'sonner';

export function SignUpPage() {
  const navigate = useNavigate();
  const { mutate: register, isPending: isLoading } = useRegisterMutation();
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    phone: '',
    password: '',
    confirmPassword: ''
  });
  const [isSuccess, setIsSuccess] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData(prev => ({ ...prev, [e.target.id]: e.target.value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (formData.password !== formData.confirmPassword) {
      toast.error('Mật khẩu xác nhận không khớp');
      return;
    }

    register({
      fullName: formData.fullName,
      email: formData.email,
      phone: formData.phone,
      password: formData.password
    }, {
      onSuccess: () => {
        setIsSuccess(true);
        toast.success('Đăng ký tài khoản thành công!');
      },
      onError: (err: any) => {
        toast.error(err.message || 'Đăng ký tài khoản thất bại');
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
            <CardTitle className="text-2xl mb-2">Đăng Ký Thành Công!</CardTitle>
            <CardDescription className="text-base mb-8">
              Chúng tôi đã gửi một liên kết xác thực đến <span className="font-bold text-slate-900 dark:text-white">{formData.email}</span>.
              Vui lòng kiểm tra hộp thư email (bao gồm cả thư mục **Spam**) để xác thực và kích hoạt tài khoản của bạn.
            </CardDescription>
            <Button className="w-full h-12 text-lg font-bold" onClick={() => navigate('/auth/login')}>
              Đến Trang Đăng Nhập
            </Button>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4 dark:from-slate-950 dark:to-slate-900">
      <div className="w-full max-w-lg space-y-8">
        <div className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-600 shadow-lg shadow-blue-500/20">
            <ShoppingBag className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-4xl font-bold tracking-tight">Tạo Tài Khoản Mới</h1>
          <p className="mt-2 text-muted-foreground">Gia nhập TechSales để tận hưởng trải nghiệm mua sắm công nghệ</p>
        </div>

        <Card className="border-none shadow-2xl shadow-blue-500/10">
          <CardHeader>
            <CardTitle>Đăng Ký</CardTitle>
            <CardDescription>Điền thông tin của bạn để bắt đầu</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="fullName">Họ và tên</Label>
                  <Input
                    id="fullName"
                    placeholder="Nguyễn Văn A"
                    value={formData.fullName}
                    onChange={handleChange}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="phone">Số điện thoại</Label>
                  <Input
                    id="phone"
                    placeholder="0901234567"
                    value={formData.phone}
                    onChange={handleChange}
                    required
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="email">Địa chỉ Email</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="name@example.com"
                  value={formData.email}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="password">Mật khẩu</Label>
                  <Input
                    id="password"
                    type="password"
                    placeholder="••••••••"
                    value={formData.password}
                    onChange={handleChange}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="confirmPassword">Xác nhận mật khẩu</Label>
                  <Input
                    id="confirmPassword"
                    type="password"
                    placeholder="••••••••"
                    value={formData.confirmPassword}
                    onChange={handleChange}
                    required
                  />
                </div>
              </div>

              <div className="flex items-center space-x-2 text-sm text-muted-foreground">
                <CheckCircle2 className="h-4 w-4 text-emerald-500" />
                <span>Bằng cách đăng ký, bạn đồng ý với Điều khoản dịch vụ của chúng tôi</span>
              </div>

              <Button type="submit" className="w-full h-12 text-lg font-bold" disabled={isLoading}>
                {isLoading ? <Loader2 className="mr-2 h-5 w-5 animate-spin" /> : null}
                {isLoading ? 'Đang tạo tài khoản...' : 'Đăng Ký'}
              </Button>
            </form>
          </CardContent>
          <CardFooter className="flex flex-col space-y-4 border-t border-border/50 pt-6">
            <div className="text-center text-sm">
              Đã có tài khoản?{' '}
              <Link to="/auth/login" className="font-bold text-blue-600 hover:underline">
                Đăng nhập
              </Link>
            </div>
            <Button variant="ghost" className="w-full" onClick={() => navigate(-1)}>
              <ArrowLeft className="mr-2 h-4 w-4" /> Quay lại
            </Button>
          </CardFooter>
        </Card>
      </div>
    </div>
  );
}
