import { useEffect, useState, useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../../components/ui/select';
import { Separator } from '../../components/ui/separator';
import { 
  CreditCard, 
  Truck, 
  MapPin, 
  ChevronLeft, 
  ShieldCheck, 
  Loader2,
  Plus,
  CheckCircle2,
  Ticket,
  X
} from 'lucide-react';
import { useGetCart, useClearCart } from '../../dataHook/cartDataHook';
import { useGetProducts } from '../../dataHook/productDataHook';
import { useCreateOrder, useCheckoutSummary } from '../../dataHook/orderDataHook';
import { useGetPaymentMethods } from '../../dataHook/paymentDataHook';
import { useGetAddresses } from '../../dataHook/addressDataHook';
import { useValidateVoucher } from '../../dataHook/voucherDataHook';
import { VoucherModal } from '../../components/customer/VoucherModal';
import { Voucher, VoucherType } from '../../models/ui_types/voucher';
import { DynamicIcon } from '../../components/ui/DynamicIcon';
import { toast } from 'sonner';

export function CheckoutPage() {
  const navigate = useNavigate();
  const { data: cartItems = [], isLoading: cartLoading } = useGetCart();
  const { data: products = [] } = useGetProducts();
  const { data: availablePaymentMethods = [] } = useGetPaymentMethods();
  const { data: savedAddresses = [] } = useGetAddresses();
  const { mutate: createOrder, isPending: isCreating } = useCreateOrder();
  const { mutate: clearCart } = useClearCart();
  const validateVoucher = useValidateVoucher();

  const [paymentMethodId, setPaymentMethodId] = useState('');
  const [selectedAddressId, setSelectedAddressId] = useState(''); 
  const [isAddingNewAddress, setIsAddingNewAddress] = useState(false);
  
  // Voucher States
  const [voucherCode, setVoucherCode] = useState('');
  const [appliedVoucher, setAppliedVoucher] = useState<Voucher | null>(null);
  const [isVoucherModalOpen, setIsVoucherModalOpen] = useState(false);
  const [isValidatingVoucher, setIsValidatingVoucher] = useState(false);

  const [shippingAddress, setShippingAddress] = useState({
    province: '',
    ward: '',
    detail: ''
  });

  useEffect(() => {
    if (availablePaymentMethods.length > 0 && !paymentMethodId) {
      setPaymentMethodId(availablePaymentMethods[0].id);
    }
    if (savedAddresses.length > 0 && !selectedAddressId) {
      const defaultAddr = savedAddresses.find(a => a.isDefault) || savedAddresses[0];
      setSelectedAddressId(defaultAddr.id);
    }
  }, [availablePaymentMethods, paymentMethodId, savedAddresses, selectedAddressId]);

  const cartWithDetails = cartItems.map(item => {
    const product = products.find(p => p.id === item.productId);
    return { ...item, product };
  }).filter(item => item.product);

  const checkoutSummaryParams = useMemo(() => ({
    items: cartItems.map(i => ({ productId: i.productId, quantity: i.quantity }))
  }), [cartItems]);

  const { data: summary } = useCheckoutSummary(checkoutSummaryParams);

  const subtotal = summary?.subtotal || 0;
  const shippingFee = summary?.shippingFee || 0;
  
  // Recalculate discount based on applied voucher
  const calculateDiscount = () => {
    if (!appliedVoucher) return 0;
    
    if (appliedVoucher.type === VoucherType.FIXED) {
      return appliedVoucher.value;
    } else {
      let discountAmount = (subtotal * appliedVoucher.value) / 100;
      return discountAmount;
    }
  };

  const discount = calculateDiscount();
  const total = Math.max(0, subtotal - discount) + shippingFee;

  const handleApplyVoucher = async () => {
    if (!voucherCode) return;
    try {
      setIsValidatingVoucher(true);
      const voucher = await validateVoucher.mutateAsync({ code: voucherCode, orderAmount: subtotal });
      setAppliedVoucher(voucher);
      toast.success('Voucher applied successfully');
    } catch (error: any) {
      toast.error(error.message || 'Invalid voucher');
    } finally {
      setIsValidatingVoucher(false);
    }
  };

  const handleSelectVoucher = (voucher: Voucher) => {
    setAppliedVoucher(voucher);
    setVoucherCode(voucher.code);
    setIsVoucherModalOpen(false);
    toast.success(`Voucher ${voucher.code} applied`);
  };

  const removeVoucher = () => {
    setAppliedVoucher(null);
    setVoucherCode('');
    toast.info('Voucher removed');
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, value } = e.target;
    setShippingAddress(prev => ({ ...prev, [id]: value }));
  };

  const handlePlaceOrder = () => {
    let finalAddress = '';
    
    if (!isAddingNewAddress) {
      const addr = savedAddresses.find(a => a.id === selectedAddressId);
      if (!addr) {
        toast.error('Please select a shipping address');
        return;
      }
      finalAddress = `${addr.detail}, ${addr.ward}, ${addr.province}`;
    } else {
      if (!shippingAddress.detail || !shippingAddress.province || !shippingAddress.ward) {
        toast.error('Please fill in all shipping details');
        return;
      }
      finalAddress = `${shippingAddress.detail}, ${shippingAddress.ward}, ${shippingAddress.province}`;
    }

    const productsWithQuantity: Record<string, number> = {};
    cartWithDetails.forEach((item: any) => {
      productsWithQuantity[item.productId] = item.quantity;
    });

    createOrder({
      productsWithQuantity,
      shippingAddressId: !isAddingNewAddress ? selectedAddressId : '', // Note: isAddingNewAddress needs properly saving the address first if we want its ID, but for now fallback
      paymentMethodId: paymentMethodId,
      voucherCode: voucherCode || undefined
    }, {
      onSuccess: (data: any) => {
        clearCart(undefined, {
          onSuccess: () => {
            if (data.checkoutUrl) {
              window.location.href = data.checkoutUrl;
            } else {
              toast.success('Order placed successfully!');
              navigate('/customer/order-success', { state: { orderId: data.id } });
            }
          }
        });
      },
      onError: () => toast.error('Failed to place order')
    });
  };

  if (cartLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-5xl space-y-8 pb-16 px-4 md:px-0">
      <div className="flex items-center gap-4 px-1">
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)} className="h-10 w-10 rounded-xl hover:bg-muted/50">
          <ChevronLeft className="h-5 w-5" />
        </Button>
        <h1 className="text-2xl font-bold tracking-tight uppercase text-foreground">Checkout</h1>
      </div>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        {/* Left Column: Forms */}
        <div className="lg:col-span-2 space-y-8">
          {/* Shipping Address Selection */}
          <Card className="border-none shadow-sm ring-1 ring-border rounded-2xl overflow-hidden bg-card">
            <CardHeader className="bg-muted/30 flex flex-row items-center justify-between p-6">
              <div className="flex items-center gap-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-sm">
                  <MapPin className="h-5 w-5" />
                </div>
                <CardTitle className="text-sm font-bold uppercase tracking-widest text-foreground">Shipping Address</CardTitle>
              </div>
              {!isAddingNewAddress ? (
                <Button 
                  variant="outline" 
                  size="sm" 
                  onClick={() => setIsAddingNewAddress(true)}
                  className="rounded-lg h-9 border-border text-xs font-bold uppercase tracking-widest"
                >
                  <Plus className="mr-1 h-3.5 w-3.5" />
                  New
                </Button>
              ) : (
                <Button 
                  variant="ghost" 
                  size="sm" 
                  onClick={() => setIsAddingNewAddress(false)}
                  className="rounded-lg h-9 text-xs font-bold uppercase tracking-widest"
                >
                  Existing
                </Button>
              )}
            </CardHeader>
            <CardContent className="p-6">
              {!isAddingNewAddress ? (
                <div className="grid grid-cols-1 gap-4">
                  {savedAddresses.length === 0 ? (
                    <div className="text-center py-10 border-2 border-dashed rounded-2xl border-border/50 bg-muted/20">
                      <p className="text-sm text-muted-foreground mb-4 font-medium italic">No saved addresses.</p>
                      <Button variant="outline" size="sm" onClick={() => setIsAddingNewAddress(true)} className="rounded-xl h-10 font-bold uppercase tracking-widest text-[10px] border-border hover:bg-muted transition-all">
                        Add Address
                      </Button>
                    </div>
                  ) : (
                    savedAddresses.map((addr) => (
                      <div 
                        key={addr.id}
                        onClick={() => setSelectedAddressId(addr.id)}
                        className={`relative cursor-pointer rounded-xl border-2 p-4 transition-all ${
                          selectedAddressId === addr.id 
                            ? 'border-primary bg-muted/30' 
                            : 'border-border/40 hover:border-border'
                        }`}
                      >
                        {selectedAddressId === addr.id && (
                          <div className="absolute right-4 top-1/2 -translate-y-1/2 text-primary">
                            <CheckCircle2 className="h-5 w-5" />
                          </div>
                        )}
                        <div className="space-y-1 pr-10">
                          <p className="font-bold text-foreground text-sm uppercase tracking-tight">Shipping Location</p>
                          <p className="text-xs leading-relaxed text-muted-foreground font-medium">
                            {addr.detail}, {addr.ward}, {addr.province}
                          </p>
                        </div>
                      </div>
                    ))
                  )}
                </div>
              ) : (
                <div className="grid gap-6">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                      <Label htmlFor="province" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Province/City</Label>
                      <Input 
                        id="province" 
                        placeholder="e.g. Ho Chi Minh" 
                        value={shippingAddress.province} 
                        onChange={handleInputChange} 
                        className="h-11 rounded-xl border-border bg-background"
                      />
                    </div>
                    <div className="space-y-1.5">
                      <Label htmlFor="ward" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Ward</Label>
                      <Input 
                        id="ward" 
                        placeholder="e.g. Ward 1" 
                        value={shippingAddress.ward} 
                        onChange={handleInputChange} 
                        className="h-11 rounded-xl border-border bg-background"
                      />
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <Label htmlFor="detail" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Detailed Address</Label>
                    <Input 
                      id="detail" 
                      placeholder="Street, Building, etc." 
                      value={shippingAddress.detail} 
                      onChange={handleInputChange} 
                      className="h-11 rounded-xl border-border bg-background"
                    />
                  </div>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Voucher Section */}
          <Card className="border-none shadow-sm ring-1 ring-border rounded-2xl overflow-hidden bg-card">
             <CardHeader className="bg-muted/30 p-6">
              <div className="flex items-center gap-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-sm">
                  <Ticket className="h-5 w-5" />
                </div>
                <CardTitle className="text-sm font-bold uppercase tracking-widest text-foreground">Discount Protocol</CardTitle>
              </div>
            </CardHeader>
            <CardContent className="p-6">
              <div className="space-y-4">
                <div className="flex flex-col sm:flex-row gap-3">
                  <div className="relative flex-1">
                    <Input 
                      placeholder="ENTER_VOUCHER_CODE" 
                      value={voucherCode}
                      onChange={(e) => setVoucherCode(e.target.value.toUpperCase())}
                      disabled={!!appliedVoucher}
                      className="h-11 rounded-xl bg-background border-border pr-10 uppercase font-black text-xs tracking-widest"
                    />
                    {appliedVoucher && (
                      <button 
                        onClick={removeVoucher}
                        className="absolute right-3 top-1/2 -translate-y-1/2 h-5 w-5 flex items-center justify-center rounded-full bg-muted text-muted-foreground hover:bg-destructive hover:text-white transition-all"
                      >
                        <X className="h-3 w-3" />
                      </button>
                    )}
                  </div>
                  {!appliedVoucher ? (
                    <Button 
                      className="h-11 rounded-xl px-8 font-bold uppercase tracking-widest text-[10px] bg-primary text-primary-foreground"
                      onClick={handleApplyVoucher}
                      disabled={isValidatingVoucher || !voucherCode}
                    >
                      {isValidatingVoucher ? <Loader2 className="h-4 w-4 animate-spin" /> : 'Validate'}
                    </Button>
                  ) : (
                    <div className="h-11 px-6 rounded-xl flex items-center justify-center bg-muted text-primary border border-border">
                       <CheckCircle2 className="h-4 w-4 mr-2" />
                       <span className="text-[10px] font-black uppercase tracking-widest">Applied</span>
                    </div>
                  )}
                  <Button 
                    variant="outline" 
                    className="h-11 rounded-xl px-6 font-bold uppercase tracking-widest text-[10px] border-border"
                    onClick={() => setIsVoucherModalOpen(true)}
                  >
                    Browse Vouchers
                  </Button>
                </div>
                {appliedVoucher && (
                  <div className="p-4 rounded-xl bg-muted/20 border border-border flex items-center justify-between">
                    <div className="space-y-1">
                      <p className="text-[10px] font-medium uppercase tracking-widest text-primary">{appliedVoucher.name}</p>
                      <p className="text-[9px] font-normal text-muted-foreground italic uppercase tracking-tight">{appliedVoucher.description}</p>
                    </div>
                    <p className="font-bold text-xs text-primary uppercase tracking-tighter">
                      -${discount.toLocaleString()}
                    </p>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Payment Method */}
          <Card className="border-none shadow-sm ring-1 ring-border rounded-2xl overflow-hidden bg-card">
            <CardHeader className="bg-muted/30 p-6">
              <div className="flex items-center gap-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-sm">
                  <CreditCard className="h-5 w-5" />
                </div>
                <CardTitle className="text-sm font-bold uppercase tracking-widest text-foreground">Payment Method</CardTitle>
              </div>
            </CardHeader>
            <CardContent className="p-6">
              <Select value={paymentMethodId} onValueChange={setPaymentMethodId}>
                <SelectTrigger className="w-full h-14 rounded-xl border-2 border-border/40 focus:border-primary focus:ring-0 transition-all bg-background">
                  <SelectValue placeholder="Select Payment Method" />
                </SelectTrigger>
                <SelectContent className="rounded-xl border-border bg-card shadow-lg">
                  {availablePaymentMethods.map((method) => (
                    <SelectItem key={method.id} value={method.id} className="py-3 cursor-pointer rounded-lg hover:bg-muted/50 focus:bg-muted/50 transition-colors">
                      <div className="flex flex-col items-start text-left">
                        <span className="font-bold text-xs uppercase tracking-tight text-foreground">{method.name}</span>
                        {method.description && (
                          <span className="mt-1 text-[10px] text-muted-foreground font-medium italic uppercase tracking-tight line-clamp-1">
                            {method.description}
                          </span>
                        )}
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </CardContent>
          </Card>
        </div>

        {/* Right Column: Summary */}
        <div className="space-y-6">
          <Card className="border-none shadow-xl ring-1 ring-border bg-card rounded-2xl sticky top-6 overflow-hidden">
            <CardHeader className="p-6 pb-0">
              <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground text-center">Order Summary</CardTitle>
            </CardHeader>
            <CardContent className="p-6 space-y-6">
              <div className="space-y-4 max-h-[300px] overflow-auto pr-1 scrollbar-none">
                {cartWithDetails.map((item) => (
                  <div key={item.productId} className="flex gap-4">
                    <div className="relative h-14 w-14 flex-shrink-0 overflow-hidden rounded-xl bg-muted border border-border">
                      <img 
                        src={item.product!.imageUrl || ''} 
                        alt={item.product!.name} 
                        className="h-full w-full object-cover grayscale opacity-80" 
                      />
                      <span className="absolute -right-2 -top-2 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-[10px] font-bold text-primary-foreground shadow-sm">
                        {item.quantity}
                      </span>
                    </div>
                    <div className="flex flex-1 flex-col justify-center min-w-0">
                      <h4 className="text-sm font-bold truncate uppercase tracking-tight text-foreground">{item.product!.name}</h4>
                      <p className="text-sm font-bold text-foreground tracking-tight">${item.product!.price.toLocaleString()}</p>
                    </div>
                  </div>
                ))}
              </div>
              
              <Separator className="bg-border/50" />
              
              <div className="space-y-3">
                <div className="flex justify-between text-sm font-medium text-foreground">
                  <span className="text-muted-foreground uppercase tracking-widest text-xs font-bold">Subtotal</span>
                  <span>${subtotal.toLocaleString()}</span>
                </div>
                <div className="flex justify-between text-sm font-medium text-foreground">
                  <span className="text-muted-foreground uppercase tracking-widest text-xs font-bold">Shipping</span>
                  <span>{shippingFee === 0 ? 'FREE' : `$${shippingFee.toLocaleString()}`}</span>
                </div>
                {discount > 0 && (
                  <div className="flex justify-between text-sm font-bold text-foreground">
                    <span className="text-muted-foreground uppercase tracking-widest text-xs">Discount</span>
                    <span className="text-primary">-${discount.toLocaleString()}</span>
                  </div>
                )}
              </div>
              
              <Separator className="bg-border/50" />
              
              <div className="flex flex-col items-center gap-1 pb-2">
                <span className="text-xs font-bold text-muted-foreground uppercase tracking-[0.2em]">Total Amount</span>
                <span className="text-3xl font-bold text-foreground tracking-tighter">${total.toLocaleString()}</span>
              </div>
            </CardContent>
            <CardFooter className="p-6 pt-0">
              <Button 
                className="w-full h-14 rounded-xl text-xs font-bold uppercase tracking-[0.2em] bg-primary text-primary-foreground hover:opacity-90 transition-all shadow-xl shadow-primary/10" 
                onClick={handlePlaceOrder}
                disabled={isCreating}
              >
                {isCreating ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <ShieldCheck className="mr-2 h-4 w-4" />}
                Place Order
              </Button>
            </CardFooter>
          </Card>
        </div>
      </div>

      <VoucherModal 
        isOpen={isVoucherModalOpen}
        onClose={() => setIsVoucherModalOpen(false)}
        onSelect={handleSelectVoucher}
        currentTotal={subtotal}
      />
    </div>
  );
}
