import { useDebouncedValue } from '../../../dataHook/useDebouncedValue';
import { useEffect } from 'react';
import { ListPagination } from '../../components/ui/list-pagination';
import { formatCurrency } from '../../../utils/format';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../components/ui/select";
import { Badge } from '../../components/ui/badge';
import { 
  ShoppingCart, 
  Star, 
  Search, 
  Loader2, 
  X,
  Plus,
  ArrowUpDown,
  Tag,
  DollarSign
} from 'lucide-react';
import { useGetProductPage } from '../../../dataHook/productDataHook';

import { useAddToCart } from '../../../dataHook/cartDataHook';
import { toast } from 'sonner';
import { useNavigate, useSearchParams } from 'react-router';

export function ProductCatalogPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  
  const searchTerm = searchParams.get('keyword') || '';
  const debouncedSearch = useDebouncedValue(searchTerm);
  const selectedCategoryId = searchParams.get('category') || 'all';
  const selectedBrandId = searchParams.get('brand') || 'all';
  const minPrice = searchParams.get('minPrice') || '';
  const maxPrice = searchParams.get('maxPrice') || '';
  const debouncedMinPrice = useDebouncedValue(minPrice);
  const debouncedMaxPrice = useDebouncedValue(maxPrice);
  const sortBy = searchParams.get('sortBy') || 'newest';
  const parsedPage = Number(searchParams.get('page') || 1);
  const pageNumber = Number.isSafeInteger(parsedPage) && parsedPage > 0 ? parsedPage : 1;
  const parsedSize = Number(searchParams.get('pageSize') || 20);
  const pageSize = Number.isInteger(parsedSize) && parsedSize >= 1 && parsedSize <= 100 ? parsedSize : 20;
  const changeFilter = (key: string, value: string, replace = false) => {
    setSearchParams(previous => {
      const next = new URLSearchParams(previous);
      if (value === '' || value === 'all') next.delete(key);
      else next.set(key, value);
      next.delete('page');
      return next;
    }, { replace });
  };
  const ready = searchTerm === debouncedSearch && minPrice === debouncedMinPrice && maxPrice === debouncedMaxPrice;
  const { data: productPage, isLoading, isFetching, isError, error, refetch } = useGetProductPage({
    pageNumber, pageSize, search: debouncedSearch,
    categoryIds: selectedCategoryId === 'all' ? undefined : selectedCategoryId,
    brandId: selectedBrandId === 'all' ? undefined : selectedBrandId,
    minPrice: debouncedMinPrice !== '' ? Number(debouncedMinPrice) : undefined,
    maxPrice: debouncedMaxPrice !== '' ? Number(debouncedMaxPrice) : undefined,
    sortBy: sortBy as 'newest' | 'price_asc' | 'price_desc' | 'rating',
  }, ready);
  const products = productPage?.items ?? [];
  const categoriesData = productPage?.categories ?? [];
  const brandsData = productPage?.brands ?? [];
  const { mutate: addToCart, isPending: isAdding } = useAddToCart();

  useEffect(() => {
    if (isError) {
      toast.error('Không thể tải danh sách sản phẩm');
    }
  }, [isError]);

  const handleAddToCart = (productId: string) => {
    addToCart(productId, {
      onSuccess: () => toast.success('Đã thêm vào giỏ hàng'),
      onError: () => toast.error('Thêm vào giỏ hàng thất bại')
    });
  };

  const clearFilters = () => setSearchParams({});

  return (
    <div className="mx-auto max-w-7xl space-y-6 pb-12 px-4 md:px-0">
      {/* Header & Search */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
        <div className="space-y-0.5">
          <h1 className="text-2xl font-bold tracking-tight text-foreground uppercase">Sản phẩm</h1>
          <p className="text-sm text-muted-foreground font-medium">Thiết bị và linh kiện công nghệ cao cấp.</p>
        </div>
        
        <div className="relative w-full md:w-[450px]">
          <Search className="absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Tìm kiếm sản phẩm..."
            value={searchTerm}
            onChange={(e) => changeFilter('keyword', e.target.value, true)}
            className="pl-11 h-12 rounded-xl border-none bg-card shadow-sm ring-1 ring-border focus-visible:ring-2 focus-visible:ring-primary transition-all text-base"
          />
        </div>
      </div>

      {/* Horizontal Filter Bar */}
      <Card className="border-none shadow-sm bg-muted/30 rounded-2xl overflow-hidden">
        <CardContent className="p-4 md:p-5">
          <div className="flex flex-wrap items-center gap-5">
            {/* Category */}
            <div className="flex-1 min-w-[150px]">
              <div className="flex items-center gap-2 mb-1.5 ml-1">
                <Tag className="h-3 w-3 text-muted-foreground" />
                <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Danh mục</span>
              </div>
              <Select value={selectedCategoryId} onValueChange={value => changeFilter('category', value)}>
                <SelectTrigger className="h-10 rounded-lg border-border bg-card">
                  <SelectValue placeholder="Tất cả danh mục" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả danh mục</SelectItem>
                  {categoriesData.map(cat => (
                    <SelectItem key={cat.id} value={cat.id}>{cat.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="flex-1 min-w-[150px]">
              <div className="mb-1.5 text-xs font-bold text-muted-foreground">Hãng</div>
              <Select value={selectedBrandId} onValueChange={value => changeFilter('brand', value)}>
                <SelectTrigger className="h-10 rounded-lg border-border bg-card" aria-label="Hãng"><SelectValue placeholder="Tất cả hãng" /></SelectTrigger>
                <SelectContent><SelectItem value="all">Tất cả hãng</SelectItem>
                  {brandsData.map(brand => <SelectItem key={brand.id} value={brand.id}>{brand.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            {/* Price */}
            <div className="flex-[1.5] min-w-[220px]">
              <div className="flex items-center gap-2 mb-1.5 ml-1">
                <DollarSign className="h-3 w-3 text-muted-foreground" />
                <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Khoảng giá</span>
              </div>
              <div className="flex items-center gap-2">
                <Input 
                  placeholder="Từ" 
                  type="number" 
                  value={minPrice} 
                  onChange={(e) => changeFilter('minPrice', e.target.value, true)}
                  className="h-10 rounded-lg border-border bg-card font-medium"
                />
                <span className="text-muted">-</span>
                <Input 
                  placeholder="Đến" 
                  type="number" 
                  value={maxPrice} 
                  onChange={(e) => changeFilter('maxPrice', e.target.value, true)}
                  className="h-10 rounded-lg border-border bg-card font-medium"
                />
              </div>
            </div>

            {/* Sort */}
            <div className="flex-1 min-w-[150px]">
              <div className="flex items-center gap-2 mb-1.5 ml-1">
                <ArrowUpDown className="h-3 w-3 text-muted-foreground" />
                <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Sắp xếp theo</span>
              </div>
              <Select value={sortBy} onValueChange={value => changeFilter('sortBy', value)}>
                <SelectTrigger className="h-10 rounded-lg border-border bg-card">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="newest">Mới nhất</SelectItem>
                  <SelectItem value="price_asc">Giá: Thấp đến Cao</SelectItem>
                  <SelectItem value="price_desc">Giá: Cao đến Thấp</SelectItem>
                  <SelectItem value="rating">Đánh giá cao nhất</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="pt-4 flex flex-col justify-end">
              <Button 
                variant="outline" 
                onClick={clearFilters}
                className="h-10 px-4 rounded-lg border-border text-foreground font-bold hover:bg-primary hover:text-primary-foreground transition-all text-xs uppercase tracking-widest"
              >
                Xóa lọc
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Results Info */}
      <div className="flex items-center justify-between px-1">
        <p className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">
          Tìm thấy <span className="text-foreground">{productPage?.totalCount ?? 0}</span> sản phẩm
        </p>
      </div>

      {/* Product Grid */}
      {isError ? (<div role="alert" className="space-y-3"><p className="text-destructive">{error?.message}</p><Button variant="outline" onClick={() => refetch()}>Thử lại</Button></div>) : isLoading || !ready ? (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4, 5, 6, 7, 8].map(i => (
            <div key={i} className="h-[350px] rounded-2xl bg-muted/50 animate-pulse" />
          ))}
        </div>
      ) : products.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 text-center bg-card rounded-3xl ring-1 ring-border">
          <Search className="h-10 w-10 text-muted mb-4" />
          <h3 className="text-xl font-bold text-foreground">Không tìm thấy sản phẩm</h3>
          <p className="text-muted-foreground mt-2 max-w-xs mx-auto text-sm font-medium">
            Hãy thử tìm bằng từ khóa khác hoặc đặt lại bộ lọc.
          </p>
          <Button variant="link" onClick={clearFilters} className="mt-4 text-primary font-bold underline underline-offset-4 text-sm">
            Đặt lại tất cả
          </Button>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {products.map(product => (
            <Card 
              key={product.id} 
              className="group overflow-hidden rounded-2xl border-none shadow-sm ring-1 ring-border transition-all hover:-translate-y-1 hover:shadow-md cursor-pointer bg-card"
              onClick={() => navigate(`/customer/products/${product.id}`)}
            >
              <div className="relative aspect-square overflow-hidden bg-muted/30">
                <img
                  src={product.imageUrl || ''}
                  alt={product.name}
                  className="h-full w-full object-cover transition-transform duration-700 group-hover:scale-105 grayscale-[0.3] group-hover:grayscale-0"
                />
                <div className="absolute right-3 top-3">
                  <Badge className="bg-primary text-primary-foreground border-none font-bold text-[8px] uppercase tracking-wider px-2 py-0.5 rounded-md">
                    {product.categoryName}
                  </Badge>
                </div>
              </div>
              <CardContent className="p-5">
                <div className="mb-2.5 flex items-center justify-between">
                  <p className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground">{product.brand}</p>
                  <div className="flex items-center gap-1.5 bg-muted/50 px-2 py-0.5 rounded-full">
                    <Star className="h-3.5 w-3.5 fill-primary text-primary" />
                    <span className="text-xs font-bold text-foreground">{product.rating ?? 0}</span>
                  </div>
                </div>
                <CardTitle className="mb-4 text-base font-bold leading-snug line-clamp-1 text-foreground">{product.name}</CardTitle>
                <div className="flex items-center justify-between">
                  <span className="text-xl font-bold text-foreground tracking-tight">
                    {product.priceIsUnset ? 'Chưa có giá' : formatCurrency(product.price)}
                  </span>
                  <Button
                    onClick={(e) => { e.stopPropagation(); handleAddToCart(product.id); }}
                    disabled={product.stock === 0 || product.priceIsUnset || isAdding || !ready}
                    className="h-10 w-10 rounded-xl p-0 bg-primary text-primary-foreground hover:opacity-90 shadow-sm"
                  >
                    {isAdding ? <Loader2 className="h-4 w-4 animate-spin" /> : <Plus className="h-4 w-4" />}
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
      <ListPagination pageNumber={pageNumber} pageSize={pageSize} totalCount={productPage?.totalCount ?? 0}
        disabled={isFetching || !ready || isError}
        onPageChange={page => setSearchParams(previous => { const next = new URLSearchParams(previous); next.set('page', String(page)); return next; })}
        onPageSizeChange={size => changeFilter('pageSize', String(size))} />
    </div>
  );
}
