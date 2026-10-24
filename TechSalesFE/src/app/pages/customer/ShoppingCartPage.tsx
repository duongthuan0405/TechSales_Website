import { useState, useEffect } from 'react';
import { formatCurrency } from '../../../utils/format';
import { useNavigate } from 'react-router';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Badge } from '../../components/ui/badge';
import { Modal } from '../../components/ui/modal';
import { Trash2, Plus, Minus, Loader2 } from 'lucide-react';
import { useCreateOrder, useCheckoutSummary } from '../../../dataHook/orderDataHook';
import { useGetCart, useUpdateCartQuantity, useRemoveFromCart, useClearCart } from '../../../dataHook/cartDataHook';
import { toast } from 'sonner';

export function ShoppingCartPage() {
  const navigate = useNavigate();
  
  const { data: cartItems = [], isLoading, isError } = useGetCart();
  const { mutate: updateQuantity, isPending: isUpdating } = useUpdateCartQuantity();
  const { mutate: removeItem, isPending: isRemoving } = useRemoveFromCart();
  const { mutate: clearCart } = useClearCart();
  const { mutate: createOrder } = useCreateOrder();

  const { data: summary } = useCheckoutSummary({
    items: cartItems.map(i => ({ productId: i.productId, quantity: i.quantity }))
  });

  const subtotal = summary?.subtotal || 0;
  const total = summary?.total || 0;

  useEffect(() => {
    if (isError) {
      toast.error('Không thể tải giỏ hàng');
    }
  }, [isError]);

  const handleCheckout = () => {
    navigate('/customer/checkout', { state: { items: cartItems, fromCart: true } });
  };

  const handleUpdateQuantity = (productId: string, quantity: number) => {
    updateQuantity({ productId, quantity }, {
      onError: () => toast.error('Cập nhật số lượng thất bại')
    });
  };

  const handleRemoveItem = (productId: string) => {
    removeItem(productId, {
      onSuccess: () => toast.success('Đã xóa sản phẩm khỏi giỏ hàng'),
      onError: () => toast.error('Xóa sản phẩm thất bại')
    });
  };

  const handleConfirmCheckout = () => {
    createOrder({
      items: cartItems,
      total,
      subtotal
    }, {
      onSuccess: () => {
        clearCart(undefined, {
          onSuccess: () => {
            toast.success('Đặt hàng thành công!');
            navigate('/customer/orders');
          }
        });
      },
      onError: () => {
        toast.error('Đặt hàng thất bại');
      }
    });
  };

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (cartItems.length === 0) {
    return (
      <div className="space-y-6">
        <h1 className="text-3xl font-bold">Giỏ hàng</h1>
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <p className="text-lg text-muted-foreground">Giỏ hàng của bạn đang trống</p>
            <p className="text-sm text-muted-foreground">Thêm một vài sản phẩm để bắt đầu mua sắm</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">Giỏ hàng</h1>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-4">
          {cartItems.map(item => (
            <Card key={item.productId}>
              <CardContent className="flex gap-4 p-6">
                <img
                  src={item.imageUrl}
                  alt={item.productName}
                  className="h-24 w-24 rounded-lg object-cover"
                />
                <div className="flex-1">
                  <div className="flex items-start justify-between">
                    <div>
                      <h3 className="font-semibold">{item.productName}</h3>
                      <p className="text-sm text-muted-foreground">Mã SP: {item.productId}</p>
                    </div>
                    <button
                      onClick={() => handleRemoveItem(item.productId)}
                      disabled={isRemoving}
                      className="rounded-lg p-2 text-muted-foreground hover:bg-destructive/10 hover:text-destructive disabled:opacity-50"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                  <div className="mt-4 flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => handleUpdateQuantity(item.productId, item.quantity - 1)}
                        disabled={item.quantity <= 1 || isUpdating}
                        className="flex h-8 w-8 items-center justify-center rounded-lg border border-border hover:bg-accent disabled:opacity-50"
                      >
                        <Minus className="h-4 w-4" />
                      </button>
                      <span className="w-12 text-center">{item.quantity}</span>
                      <button
                        onClick={() => handleUpdateQuantity(item.productId, item.quantity + 1)}
                        disabled={isUpdating}
                        className="flex h-8 w-8 items-center justify-center rounded-lg border border-border hover:bg-accent disabled:opacity-50"
                      >
                        <Plus className="h-4 w-4" />
                      </button>
                    </div>
                    <div className="text-xl font-bold">
                      {formatCurrency((item.price || 0) * item.quantity)}
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>

        <div>
          <Card>
            <CardHeader>
              <CardTitle>Tóm tắt đơn hàng</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Tạm tính</span>
                <span>{formatCurrency(subtotal)}</span>
              </div>
              <div className="border-t border-border pt-4">
                <div className="flex justify-between">
                  <span className="font-semibold">Tổng tiền</span>
                  <span className="text-xl font-bold">{formatCurrency(total)}</span>
                </div>
              </div>
            </CardContent>
            <CardFooter>
              <Button onClick={handleCheckout} className="w-full">
                Tiến hành thanh toán
              </Button>
            </CardFooter>
          </Card>
        </div>
      </div>

    </div>
  );
}
