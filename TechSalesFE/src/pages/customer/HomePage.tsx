import { useEffect } from 'react';
import { ArrowRight, ShoppingCart, Star, Zap, TrendingUp, ShieldCheck, Truck, Headphones, Loader2, Plus } from 'lucide-react';
import { Button } from '../../components/ui/button';
import { Card, CardContent } from '../../components/ui/card';
import { Badge } from '../../components/ui/badge';
import { useGetProducts } from '../../dataHook/productDataHook';
import { useGetCategories } from '../../dataHook/categoryDataHook';
import { useAddToCart } from '../../dataHook/cartDataHook';
import { toast } from 'sonner';
import { useNavigate } from 'react-router';

export function HomePage() {
  const navigate = useNavigate();
  const { data: products = [], isLoading: productsLoading, isError: productsError } = useGetProducts();
  const { data: categories = [], isLoading: categoriesLoading, isError: categoriesError } = useGetCategories();
  const { mutate: addToCart, isPending: isAdding } = useAddToCart();

  useEffect(() => {
    if (productsError) toast.error('Failed to load products');
    if (categoriesError) toast.error('Failed to load categories');
  }, [productsError, categoriesError]);

  const featuredProducts = products.slice(0, 4);
  const flashSaleProducts = products.slice(4, 8);

  const handleAddToCart = (productId: string) => {
    addToCart(productId, {
      onSuccess: () => toast.success('Added to cart'),
      onError: () => toast.error('Failed to add to cart')
    });
  };

  return (
    <div className="space-y-16 pb-20">
      {/* Hero Section */}
      <section className="relative overflow-hidden rounded-[2.5rem] bg-zinc-950 text-white min-h-[550px] flex items-center shadow-2xl">
        <div className="absolute inset-0">
           <img 
             src="https://images.unsplash.com/photo-1550009158-9ebf69173e03?q=80&w=2101&auto=format&fit=crop" 
             className="h-full w-full object-cover opacity-60"
             alt="Future Tech"
           />
           <div className="absolute inset-0 bg-gradient-to-r from-zinc-950 via-zinc-950/80 to-transparent" />
        </div>
        
        <div className="relative z-10 flex flex-col items-start px-10 py-16 text-left md:py-24 max-w-2xl">
          <Badge className="mb-6 bg-white text-black border-none font-bold tracking-widest uppercase px-5 py-2 rounded-full text-[11px]">
            New Horizon 2026
          </Badge>
          <h1 className="mb-6 text-5xl font-bold tracking-tighter md:text-7xl uppercase leading-[0.85] text-white">
            Precision <br /> 
            <span className="text-white/40">Engineering</span>
          </h1>
          <p className="mb-10 text-xl text-zinc-300 font-normal leading-relaxed max-w-md">
            The next generation of professional hardware is here. Engineered for elite performance.
          </p>
          <div className="flex flex-wrap gap-4">
            <Button size="lg" className="h-14 px-10 rounded-2xl bg-white text-black hover:bg-zinc-200 font-bold uppercase tracking-widest text-xs" onClick={() => navigate('/customer/products')}>
              Shop Now <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
            <Button size="lg" variant="outline" className="h-14 border-white/20 bg-white/5 backdrop-blur-md px-10 text-white hover:bg-white/10 rounded-2xl font-bold uppercase tracking-widest text-xs">
              Our Vision
            </Button>
          </div>
        </div>
      </section>

      {/* Categories */}
      <section className="px-1">
        <div className="mb-8 flex items-end justify-between border-b border-border pb-6">
          <div className="space-y-1">
            <h2 className="text-2xl font-bold uppercase tracking-tight text-foreground">Categories</h2>
            <p className="text-xs text-muted-foreground font-bold uppercase tracking-[0.2em] opacity-60">Technical Modules</p>
          </div>
          <Button variant="ghost" className="text-xs font-bold uppercase tracking-widest hover:bg-muted rounded-xl px-4 h-10" onClick={() => navigate('/customer/products')}>View All</Button>
        </div>
        
        {categoriesLoading ? (
          <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 md:grid-cols-6">
             {[1,2,3,4,5,6].map(i => <div key={i} className="h-32 rounded-2xl bg-muted animate-pulse" />)}
          </div>
        ) : (
          <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 md:grid-cols-6">
            {categories.map((cat) => (
              <Card 
                key={cat.id} 
                className="group cursor-pointer border-none shadow-sm ring-1 ring-border bg-card transition-all hover:ring-black rounded-2xl overflow-hidden h-full" 
                onClick={() => navigate(`/customer/products?categoryIds=${cat.id}`)}
              >
                <CardContent className="flex flex-col items-center justify-center p-8 h-full">
                  <span className="font-bold text-sm uppercase tracking-tight text-center text-foreground">{cat.name}</span>
                  {cat.productCount !== undefined && (
                    <span className="text-[10px] text-muted-foreground font-bold mt-1">{cat.productCount} Items</span>
                  )}
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </section>

      {/* Flash Sale */}
      <section className="rounded-[2.5rem] bg-zinc-100/70 p-10 md:p-14 overflow-hidden relative ring-1 ring-zinc-200">
        <div className="absolute top-0 right-0 p-10 opacity-[0.03] pointer-events-none">
           <Zap className="h-64 w-64 fill-black" />
        </div>
        <div className="relative z-10 mb-12 flex flex-col gap-6 sm:flex-row sm:items-end sm:justify-between px-2">
          <div className="flex items-center gap-5">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-black text-white shadow-xl">
              <Zap className="h-6 w-6 fill-white" />
            </div>
            <div>
              <h2 className="text-3xl font-bold uppercase tracking-tight text-black italic leading-none">Flash Sale</h2>
              <p className="text-xs font-bold uppercase tracking-[0.2em] text-zinc-500 mt-1">Live deals active now</p>
            </div>
          </div>
          <Button variant="outline" className="border-zinc-300 bg-white text-black hover:bg-black hover:text-white rounded-xl font-bold uppercase tracking-widest text-xs h-11 px-8 shadow-sm transition-all">Participate</Button>
        </div>
        
        {productsLoading ? (
           <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
              {[1,2,3,4].map(i => <div key={i} className="h-64 rounded-2xl bg-zinc-200 animate-pulse" />)}
           </div>
        ) : (
          <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-4 relative z-10 items-stretch">
            {flashSaleProducts.map((product) => (
              <Card 
                key={product.id} 
                className="overflow-hidden border-none shadow-md group cursor-pointer bg-white transition-all hover:shadow-xl rounded-2xl ring-1 ring-black/5 flex flex-col h-full" 
                onClick={() => navigate(`/customer/products/${product.id}`)}
              >
                <div className="relative aspect-[4/3] overflow-hidden">
                  <img src={product.imageUrl || ''} alt={product.name} className="h-full w-full object-cover transition-transform duration-700 group-hover:scale-105" />
                  <div className="absolute left-4 top-4 bg-black text-white px-3 py-1 rounded-md font-black text-[10px] uppercase tracking-widest shadow-sm">SALE</div>
                </div>
                <CardContent className="p-5 flex flex-col flex-1">
                  <div className="flex flex-col flex-1">
                    <p className="text-[10px] font-bold uppercase tracking-widest text-zinc-400 mb-1.5">{product.brand}</p>
                    <h3 className="mb-3 font-bold text-base line-clamp-2 text-black leading-tight min-h-[3rem]">{product.name}</h3>
                  </div>
                  <div className="flex items-center justify-between mt-auto pt-4 border-t border-zinc-50">
                    <div className="flex flex-col">
                      <span className="text-[10px] font-bold text-zinc-400 line-through">${product.price.toLocaleString()}</span>
                      <span className="text-xl font-black text-black tracking-tighter leading-none">${(product.price * 0.8).toLocaleString()}</span>
                    </div>
                    <Button 
                      size="icon" 
                      className="h-10 w-10 rounded-xl bg-black text-white hover:opacity-90 transition-all flex-shrink-0"
                      onClick={(e) => { e.stopPropagation(); handleAddToCart(product.id); }}
                    >
                      <Plus className="h-5 w-5" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </section>

      {/* Featured Products */}
      <section className="px-1">
        <div className="mb-10 flex items-end justify-between border-b border-border pb-6">
          <div className="space-y-1">
            <h2 className="text-2xl font-bold uppercase tracking-tight text-foreground">Featured Products</h2>
            <p className="text-xs text-muted-foreground font-bold uppercase tracking-[0.2em] opacity-60">Top tier performance</p>
          </div>
          <Button variant="ghost" className="text-xs font-bold uppercase tracking-widest rounded-xl px-4 h-10" onClick={() => navigate('/customer/products')}>See More</Button>
        </div>
        
        {productsLoading ? (
           <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
              {[1,2,3,4].map(i => <div key={i} className="h-64 rounded-2xl bg-muted animate-pulse" />)}
           </div>
        ) : (
          <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-4 items-stretch">
            {featuredProducts.map((product) => (
              <Card 
                key={product.id} 
                className="group overflow-hidden border-none shadow-sm ring-1 ring-border bg-card transition-all hover:shadow-md rounded-2xl cursor-pointer flex flex-col h-full" 
                onClick={() => navigate(`/customer/products/${product.id}`)}
              >
                <div className="relative aspect-square overflow-hidden bg-zinc-50 border-b border-border/10">
                  <img src={product.imageUrl || ''} alt={product.name} className="h-full w-full object-cover transition-transform duration-700 group-hover:scale-105" />
                  <div className="absolute right-3 top-3">
                    <Badge className="bg-black text-white border-none font-bold text-[9px] uppercase tracking-wider px-2 py-0.5 rounded-md">
                      {categories.find(c => c.id === product.categoryId)?.name || 'Hardware'}
                    </Badge>
                  </div>
                </div>
                <CardContent className="p-5 flex flex-col flex-1">
                  <div className="flex flex-col flex-1">
                    <div className="mb-2.5 flex items-center justify-between">
                      <p className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground">{product.brand}</p>
                      <div className="flex items-center gap-1.5 bg-muted/50 px-2 py-0.5 rounded-full">
                        <Star className="h-3.5 w-3.5 fill-yellow-400 text-yellow-400" />
                        <span className="text-xs font-bold text-foreground">{product.rating || 0}</span>
                      </div>
                    </div>
                    <h3 className="mb-4 text-base font-bold leading-tight line-clamp-2 text-foreground min-h-[3rem]">{product.name}</h3>
                  </div>
                  <div className="flex items-center justify-between mt-auto pt-4 border-t border-border/40">
                    <span className="text-2xl font-black text-foreground tracking-tighter">${product.price.toLocaleString()}</span>
                    <Button 
                      size="icon" 
                      className="h-10 w-10 rounded-xl bg-black text-white hover:opacity-90 transition-all flex-shrink-0"
                      onClick={(e) => { e.stopPropagation(); handleAddToCart(product.id); }}
                    >
                      <Plus className="h-5 w-5" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </section>

      {/* Services Section */}
      <section className="grid grid-cols-2 gap-10 border-y border-border py-16 md:grid-cols-4 px-1">
        {[
          { icon: Truck, title: 'Rapid Logistics', desc: 'Global delivery' },
          { icon: ShieldCheck, title: 'Secure Transit', desc: 'Encrypted protection' },
          { icon: Headphones, title: 'Expert Support', desc: '24/7 assistance' },
          { icon: Zap, title: 'Warranty Plus', desc: 'Lifetime protocol' }
        ].map((service, idx) => (
          <div key={idx} className="flex flex-col items-center text-center gap-4 group">
            <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-zinc-100 text-zinc-900 group-hover:bg-black group-hover:text-white transition-all duration-300 shadow-sm">
              <service.icon className="h-7 w-7" />
            </div>
            <div>
              <h4 className="font-bold text-[13px] uppercase tracking-widest text-foreground">{service.title}</h4>
              <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-widest mt-1.5 opacity-60">{service.desc}</p>
            </div>
          </div>
        ))}
      </section>

      {/* Newsletter */}
      <section className="rounded-[3rem] bg-zinc-950 text-white p-16 text-center shadow-2xl relative overflow-hidden">
        <div className="relative z-10 max-w-xl mx-auto space-y-8">
          <div className="space-y-3">
            <h2 className="text-4xl font-bold uppercase tracking-tight">Stay Synced</h2>
            <p className="text-zinc-500 font-medium text-xs tracking-widest uppercase">Subscribe for elite technical access.</p>
          </div>
          <div className="flex flex-col sm:flex-row gap-4 max-w-md mx-auto">
            <input 
              type="email" 
              placeholder="EMAIL_PROTOCOL" 
              className="flex-1 rounded-xl bg-white/5 border border-white/10 px-6 py-4 outline-none focus:ring-2 focus:ring-white/20 text-sm font-bold placeholder:text-zinc-700 uppercase tracking-widest"
            />
            <Button className="h-14 px-10 rounded-xl bg-white text-black hover:bg-zinc-200 font-bold uppercase tracking-widest text-xs">Connect</Button>
          </div>
        </div>
      </section>

      {/* Footer simple */}
      <footer className="pt-10 text-center">
        <p className="text-[10px] font-bold text-muted-foreground uppercase tracking-[0.4em] opacity-30">&copy; 2026 TECHSALES_PROTO. ALL RIGHTS RESERVED.</p>
      </footer>
    </div>
  );
}
