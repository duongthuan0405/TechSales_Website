import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { ShieldCheck, Lock, Eye, EyeOff, Loader2, ArrowRight } from 'lucide-react';
import { toast } from 'sonner';
import { authService } from '../../../services/authService';

export function ChangePasswordPage({ embedded = false }: { embedded?: boolean }) {
  const [isLoading, setIsLoading] = useState(false);
  const [showPasswords, setShowPasswords] = useState({
    current: false,
    new: false,
    confirm: false,
  });
  const [formData, setFormData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });

  const toggleVisibility = (field: keyof typeof showPasswords) => {
    setShowPasswords(prev => ({ ...prev, [field]: !prev[field] }));
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData(prev => ({ ...prev, [e.target.id]: e.target.value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (formData.newPassword !== formData.confirmPassword) {
      toast.error('Mật khẩu xác nhận không khớp');
      return;
    }
    
    setIsLoading(true);
    try {
      await authService.changePassword(formData.currentPassword, formData.newPassword, formData.confirmPassword);
      toast.success('Cập nhật mật khẩu thành công');
      setFormData({ currentPassword: '', newPassword: '', confirmPassword: '' });
      setShowPasswords({ current: false, new: false, confirm: false });
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Không đổi được mật khẩu.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className={embedded ? "space-y-6" : "max-w-xl mx-auto space-y-6 pb-12"}>
      {!embedded && <div className="text-center space-y-2">
        <div className="mx-auto h-12 w-12 bg-primary rounded-xl flex items-center justify-center text-primary-foreground">
          <ShieldCheck className="h-6 w-6" />
        </div>
        <h1 className="text-2xl font-bold tracking-tight uppercase">Cài đặt bảo mật</h1>
        <p className="text-sm text-muted-foreground">Giữ tài khoản an toàn bằng cách cập nhật mật khẩu định kỳ.</p>
      </div>}

      <Card className="border-border shadow-sm bg-card rounded-2xl overflow-hidden">
        <CardHeader className="p-6">
          <CardTitle className="text-sm font-bold uppercase tracking-widest">Đổi mật khẩu</CardTitle>
          <CardDescription className="text-xs">Nhập mật khẩu hiện tại và thiết lập mật khẩu mới có độ bảo mật cao.</CardDescription>
        </CardHeader>
        <CardContent className="p-6 pt-0">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="space-y-1.5">
              <Label htmlFor="currentPassword" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Mật khẩu hiện tại</Label>
              <div className="relative">
                <Input 
                  id="currentPassword" autoComplete="current-password" 
                  type={showPasswords.current ? 'text' : 'password'}
                  className="h-11 rounded-lg border-border pr-10"
                  value={formData.currentPassword}
                  onChange={handleChange}
                  disabled={isLoading}
                  required
                />
                <button 
                  type="button"
                  onClick={() => toggleVisibility('current')}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                >
                  {showPasswords.current ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="newPassword" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Mật khẩu mới</Label>
              <div className="relative">
                <Input 
                  id="newPassword" autoComplete="new-password" 
                  type={showPasswords.new ? 'text' : 'password'}
                  className="h-11 rounded-lg border-border pr-10"
                  value={formData.newPassword}
                  onChange={handleChange}
                  disabled={isLoading}
                  required
                />
                <button 
                  type="button"
                  onClick={() => toggleVisibility('new')}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                >
                  {showPasswords.new ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="confirmPassword" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Xác nhận mật khẩu mới</Label>
              <div className="relative">
                <Input 
                  id="confirmPassword" autoComplete="new-password" 
                  type={showPasswords.confirm ? 'text' : 'password'}
                  className="h-11 rounded-lg border-border pr-10"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  disabled={isLoading}
                  required
                />
                <button 
                  type="button"
                  onClick={() => toggleVisibility('confirm')}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                >
                  {showPasswords.confirm ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
            </div>

            <Button 
              type="submit" 
              className="w-full h-11 rounded-lg text-[10px] font-bold uppercase tracking-widest bg-primary text-primary-foreground hover:opacity-90 transition-all shadow-sm"
              disabled={isLoading}
            >
              {isLoading ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <Lock className="mr-2 h-4 w-4" />}
              {isLoading ? 'Đang cập nhật...' : 'Đổi mật khẩu'}
            </Button>
          </form>
        </CardContent>
      </Card>

      <div className="bg-muted/30 rounded-2xl p-5 border border-border/50">
        <h4 className="text-[10px] font-black uppercase tracking-widest flex items-center gap-2 mb-4">
          <ShieldCheck className="h-4 w-4" />
          Yêu cầu bảo mật
        </h4>
        <ul className="space-y-3 text-[9px] font-bold uppercase tracking-widest text-muted-foreground">
          <li className="flex items-center gap-2">
             <div className="h-1 w-1 rounded-full bg-primary" />
             Không trùng với mật khẩu hiện tại
          </li>
          <li className="flex items-center gap-2">
             <div className="h-1 w-1 rounded-full bg-primary" />
             Tối thiểu 8 ký tự
          </li>
          <li className="flex items-center gap-2">
             <div className="h-1 w-1 rounded-full bg-primary" />
             Không quá 72 byte UTF-8
          </li>
        </ul>
      </div>
    </div>
  );
}
