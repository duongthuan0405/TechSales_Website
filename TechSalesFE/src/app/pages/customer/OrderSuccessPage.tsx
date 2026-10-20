import { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router';
import { Button } from '../../components/ui/button';
import { Card, CardContent } from '../../components/ui/card';
import { CheckCircle2, Package, ArrowRight, ShoppingBag } from 'lucide-react';
import confetti from 'canvas-confetti';

export function OrderSuccessPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  let orderId = location.state?.orderId || searchParams.get('orderId') || 'ORD-UNKNOWN';
  if (orderId && orderId.includes('_')) {
    orderId = orderId.split('_')[0];
  }

  useEffect(() => {
    // Launch confetti!
    const duration = 3 * 1000;
    const animationEnd = Date.now() + duration;
    const defaults = { startVelocity: 30, spread: 360, ticks: 60, zIndex: 0 };

    const randomInRange = (min: number, max: number) => Math.random() * (max - min) + min;

    const interval: any = setInterval(function() {
      const timeLeft = animationEnd - Date.now();

      if (timeLeft <= 0) {
        return clearInterval(interval);
      }

      const particleCount = 50 * (timeLeft / duration);
      confetti({ ...defaults, particleCount, origin: { x: randomInRange(0.1, 0.3), y: Math.random() - 0.2 } });
      confetti({ ...defaults, particleCount, origin: { x: randomInRange(0.7, 0.9), y: Math.random() - 0.2 } });
    }, 250);

    return () => clearInterval(interval);
  }, []);

  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <div className="relative mb-8">
        <div className="absolute inset-0 animate-ping rounded-full bg-emerald-100 opacity-20 dark:bg-emerald-900/30" />
        <div className="relative flex h-24 w-24 items-center justify-center rounded-full bg-emerald-500 text-white shadow-xl shadow-emerald-500/20">
          <CheckCircle2 className="h-12 w-12" />
        </div>
      </div>

      <h1 className="mb-4 text-4xl font-extrabold tracking-tight">Đặt hàng thành công!</h1>
      <p className="mb-10 max-w-md text-lg text-muted-foreground">
        Cảm ơn bạn đã mua sắm. Đơn hàng <span className="font-bold text-slate-900 dark:text-white">#{orderId}</span> của bạn đã được tiếp nhận và đang được xử lý.
      </p>

      <div className="grid w-full max-w-2xl grid-cols-1 gap-6 md:grid-cols-2">
        <Card className="border-none shadow-sm ring-1 ring-border/50 transition-all hover:ring-primary/50">
          <CardContent className="flex flex-col items-center p-8">
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-blue-100 text-blue-600 dark:bg-blue-900/30">
              <Package className="h-6 w-6" />
            </div>
            <h3 className="mb-2 font-bold">Theo dõi đơn hàng</h3>
            <p className="mb-6 text-center text-sm text-muted-foreground">
              Xem chi tiết tiến trình vận chuyển và cập nhật trạng thái đơn hàng.
            </p>
            <Button variant="outline" className="w-full" onClick={() => navigate(`/customer/orders/${orderId}`)}>
              Xem chi tiết đơn hàng <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          </CardContent>
        </Card>

        <Card className="border-none shadow-sm ring-1 ring-border/50 transition-all hover:ring-primary/50">
          <CardContent className="flex flex-col items-center p-8">
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-purple-100 text-purple-600 dark:bg-purple-900/30">
              <ShoppingBag className="h-6 w-6" />
            </div>
            <h3 className="mb-2 font-bold">Tiếp tục mua sắm</h3>
            <p className="mb-6 text-center text-sm text-muted-foreground">
              Khám phá thêm nhiều thiết bị và ưu đãi công nghệ hấp dẫn.
            </p>
            <Button className="w-full" onClick={() => navigate('/customer/products')}>
              Duyệt sản phẩm <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          </CardContent>
        </Card>
      </div>

      <div className="mt-12 text-sm text-muted-foreground">
        Email xác nhận đã được gửi đến địa chỉ email đăng ký của bạn.
      </div>
    </div>
  );
}
