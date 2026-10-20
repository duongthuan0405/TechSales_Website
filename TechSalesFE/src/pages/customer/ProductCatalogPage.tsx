import { useState, useEffect, useMemo } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
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
import { useGetProducts } from '../../dataHook/productDataHook';
import { useGetCategories } from '../../dataHook/categoryDataHook';
import { useAddToCart } from '../../dataHook/cartDataHook';
import { toast } from 'sonner';
import { useNavigate, useSearchParams } from 'react-router';
import { useAuth } from '../../context/AuthContext';

export function ProductCatalogPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  
  const { user } = useAuth();
  const isCustomer = user?.role === 'Customer';
  const basePath = user?.role === 'Staff' ? '/sales' : '/customer';
  
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  
  const selectedCategoryId = searchParams.get('categoryIds') || 'all';
  const [minPrice, setMinPrice] = useState<string>('');
  const [maxPrice, setMaxPrice] = useState<string>('');
  const [sortBy, setSortBy] = useState<string>('newest');

  const { data: categoriesData = [], isLoading: isCatsLoading } = useGetCategories();

  const setSelectedCategory = (categoryId: string) => {
    const newParams = new URLSearchParams(searchParams);
    if (categoryId === 'all') {
      newParams.delete('categoryIds');
    } else {
      newParams.set('categoryIds', categoryId);
    }
    setSearchParams(newParams);
  };

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(searchTerm), 500);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const productParams = useMemo(() => ({
    search: debouncedSearch,
    categoryIds: selectedCategoryId,
    minPrice: minPrice ? Number(minPrice) : undefined,
    maxPrice: maxPrice ? Number(maxPrice) : undefined,
    sortBy: sortBy as any
  }), [debouncedSearch, selectedCategoryId, minPrice, maxPrice, sortBy]);

  const { data: products = [], isLoading, isError } = useGetProducts(productParams);

  const { mutate: addToCart, isPending: isAdding } = useAddToCart();

  useEffect(() => {
    if (isError) {
      toast.error('Failed to load products');
    }
  }, [isError]);

  const handleAddToCart = (productId: string) => {
    addToCart(productId, {
      onSuccess: () => toast.success('Added to cart'),
      onError: () => toast.error('Failed to add to cart')
    });
  };

  const clearFilters = () => {
    setSelectedCategory('all');
    setMinPrice('');
    setMaxPrice('');
    setSearchTerm('');
    setSortBy('newest');
  };

  return (
    <div className="mx-auto max-w-7xl space-y-6 pb-12 px-4 md:px-0">
      {/* Header & Search */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
        <div className="space-y-0.5">
          <h1 className="text-2xl font-bold tracking-tight text-foreground uppercase">Catalog</h1>
          <p className="text-sm text-muted-foreground font-medium">Premium technology essentials.</p>
        </div>
        
        <div className="relative w-full md:w-[450px]">
          <Search className="absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search our collection..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
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
                <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Category</span>
              </div>
              <Select 
                value={selectedCategoryId} 
                onValueChange={setSelectedCategory}
              >
                <SelectTrigger className="h-10 rounded-lg border-border bg-card">
                  <SelectValue placeholder="All Categories" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Categories</SelectItem>
                  {categoriesData.map(cat => (
                    <SelectItem key={cat.id} value={cat.id}>{cat.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Price */}
            <div className="flex-[1.5] min-w-[220px]">
              <div className="flex items-center gap-2 mb-1.5 ml-1">
                <DollarSign className="h-3 w-3 text-muted-foreground" />
                <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Price Range</span>
              </div>
              <div className="flex items-center gap-2">
                <Input 
                  placeholder="Min" 
                  type="number" 
                  value={minPrice} 
                  onChange={(e) => setMinPrice(e.target.value)}
                  className="h-10 rounded-lg border-border bg-card font-medium"
                />
                <span className="text-muted">-</span>
                <Input 
                  placeholder="Max" 
                  type="number" 
                  value={maxPrice} 
                  onChange={(e) => setMaxPrice(e.target.value)}
                  className="h-10 rounded-lg border-border bg-card font-medium"
                />
              </div>
            </div>

            {/* Sort */}
            <div className="flex-1 min-w-[150px]">
              <div className="flex items-center gap-2 mb-1.5 ml-1">
                <ArrowUpDown className="h-3 w-3 text-muted-foreground" />
                <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Sort by</span>
              </div>
              <Select 
                value={sortBy} 
                onValueChange={setSortBy}
              >
                <SelectTrigger className="h-10 rounded-lg border-border bg-card">
                  <SelectValue placeholder="Sort Order" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="newest">Newest First</SelectItem>
                  <SelectItem value="price_asc">Price: Low to High</SelectItem>
                  <SelectItem value="price_desc">Price: High to Low</SelectItem>
                  <SelectItem value="rating">Top Rated</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="pt-4 flex flex-col justify-end">
              <Button 
                variant="outline" 
                onClick={clearFilters}
                className="h-10 px-4 rounded-lg border-border text-foreground font-bold hover:bg-primary hover:text-primary-foreground transition-all text-xs uppercase tracking-widest"
              >
                Clear
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Results Info */}
      <div className="flex items-center justify-between px-1">
        <p className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">
          <span className="text-foreground">{products.length}</span> Objects Found
        </p>
      </div>

      {/* Product Grid */}
      {isLoading ? (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4, 5, 6, 7, 8].map(i => (
            <div key={i} className="h-[350px] rounded-2xl bg-muted/50 animate-pulse" />
          ))}
        </div>
      ) : products.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 text-center bg-card rounded-3xl ring-1 ring-border">
          <Search className="h-10 w-10 text-muted mb-4" />
          <h3 className="text-xl font-bold text-foreground">No Results</h3>
          <p className="text-muted-foreground mt-2 max-w-xs mx-auto text-sm font-medium">
            Try a different term or reset filters.
          </p>
          <Button variant="ghost" onClick={clearFilters} className="mt-4 text-primary font-bold underline underline-offset-4 text-sm">
            Reset All
          </Button>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {products.map(product => (
            <Card 
              key={product.id} 
              className="group overflow-hidden rounded-2xl border-none shadow-sm ring-1 ring-border transition-all hover:-translate-y-1 hover:shadow-md cursor-pointer bg-card"
              onClick={() => navigate(`${basePath}/products/${product.id}`)}
            >
              <div className="relative aspect-square overflow-hidden bg-muted/30">
                <img
                  src={product.imageUrl || ''}
                  alt={product.name}
                  className="h-full w-full object-cover transition-transform duration-700 group-hover:scale-105 grayscale-[0.3] group-hover:grayscale-0"
                />
                <div className="absolute right-3 top-3">
                  <Badge className="bg-primary text-primary-foreground border-none font-bold text-[8px] uppercase tracking-wider px-2 py-0.5 rounded-md">
                    {categoriesData.find(c => c.id === product.categoryId)?.name || 'Hardware'}
                  </Badge>
                </div>
              </div>
              <CardContent className="p-5">
                <div className="mb-2.5 flex items-center justify-between">
                  <p className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground">{product.brand}</p>
                  <div className="flex items-center gap-1.5 bg-muted/50 px-2 py-0.5 rounded-full">
                    <Star className="h-3.5 w-3.5 fill-yellow-400 text-yellow-400" />
                    <span className="text-xs font-bold text-foreground">{product.rating}</span>
                  </div>
                </div>
                <CardTitle className="mb-4 text-base font-bold leading-snug line-clamp-1 text-foreground">{product.name}</CardTitle>
                <div className="flex items-center justify-between">
                  <span className="text-xl font-bold text-foreground tracking-tight">
                    ${product.price.toLocaleString()}
                  </span>
                  {isCustomer && (
                    <Button
                      onClick={(e) => { e.stopPropagation(); handleAddToCart(product.id); }}
                      disabled={product.stock === 0 || isAdding}
                      className="h-10 w-10 rounded-xl p-0 bg-primary text-primary-foreground hover:opacity-90 shadow-sm"
                    >
                      {isAdding ? <Loader2 className="h-4 w-4 animate-spin" /> : <Plus className="h-4 w-4" />}
                    </Button>
                  )}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
