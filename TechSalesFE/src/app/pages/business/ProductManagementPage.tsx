import { ListPagination } from '../../components/ui/list-pagination';
import { useListUrlState } from '../../../dataHook/useListUrlState';
import { useDebouncedValue } from '../../../dataHook/useDebouncedValue';
import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "../../components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "../../components/ui/table";
import { Badge } from "../../components/ui/badge";
import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "../../components/ui/dialog";
import { Search, Edit, Ban, Plus, Loader2, PackageSearch, Eye, RefreshCcw, PackagePlus } from "lucide-react";
import { 
  useGetAdminProducts, 
  useCreateProduct, 
  useUpdateProduct,
  useToggleProductStatus,
  useGetProductLookups,
  useGetProduct
} from "../../../dataHook/productDataHook";
import { useNavigate, useSearchParams } from "react-router";
import { Product, ProductStatus } from "../../../models/ui_types/product";
import { ProductForm } from "../../components/business/ProductForm";
import { ProductStockDialog } from '../../components/business/ProductStockDialog';
import { StockImportDialog } from '../../components/business/StockImportDialog';

import { toast } from "sonner";
import { formatCurrency } from "../../../utils/format";
import { ProductQueryParams } from '../../../services/productService';

interface ProductManagementPageProps {
  readOnly?: boolean;
}

export function ProductManagementPage({ readOnly = false }: ProductManagementPageProps) {
  const navigate = useNavigate();
  const [urlParams, setUrlParams] = useSearchParams();
  const stockProductId = urlParams.get('stockProduct') || '';
  const stockProduct = useGetProduct(stockProductId);
  const setAdjustingProduct = (product: Product | undefined) => setUrlParams(previous => {
    const next = new URLSearchParams(previous);
    if (product) next.set('stockProduct', product.id);
    else next.delete('stockProduct');
    for (const key of [...next.keys()]) if (key.startsWith('stock.')) next.delete(key);
    return next;
  });
  const list = useListUrlState({ keyword: '', category: 'all', brand: 'all', minPrice: '', maxPrice: '',
    sort: 'newest' as NonNullable<ProductQueryParams['sortBy']>, status: 'all' }, 'products.');
  const { pageNumber, pageSize, setPageNumber, setPageSize } = list;
  const [searchTerm, setSearchTerm] = list.field('keyword');
  const debouncedSearch = useDebouncedValue(searchTerm);
  const [categoryFilter, setCategoryFilter] = list.field('category');
  const [brandFilter, setBrandFilter] = list.field('brand');
  const [minPrice, setMinPrice] = list.field('minPrice');
  const [maxPrice, setMaxPrice] = list.field('maxPrice');
  const [sortBy, setSortBy] = list.field('sort');
  const debouncedMinPrice = useDebouncedValue(minPrice);
  const debouncedMaxPrice = useDebouncedValue(maxPrice);
  const filtersReady = searchTerm === debouncedSearch && minPrice === debouncedMinPrice && maxPrice === debouncedMaxPrice;
  const [statusFilter, setStatusFilter] = list.field('status');
  const { data: lookups } = useGetProductLookups();
  const { data: adminData, isLoading, isFetching, error: productsError } = useGetAdminProducts({
    pageNumber, pageSize, keyword: debouncedSearch || undefined,
    categoryId: categoryFilter === 'all' ? undefined : categoryFilter,
    brandId: brandFilter === 'all' ? undefined : brandFilter,
    minPrice: debouncedMinPrice === '' ? undefined : Number(debouncedMinPrice),
    maxPrice: debouncedMaxPrice === '' ? undefined : Number(debouncedMaxPrice),
    sortBy,
    status: statusFilter === 'all' ? undefined : statusFilter as ProductStatus,
  }, filtersReady);
  const products = adminData?.items || [];
  const createMutation = useCreateProduct();
  const updateMutation = useUpdateProduct();
  const toggleStatusMutation = useToggleProductStatus();


  
  // Dialog states
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | undefined>(undefined);
  const [isImportOpen, setIsImportOpen] = useState(false);
  const adjustingProduct = products.find(product => product.id === stockProductId) ?? stockProduct.data;

  const categories = [{ id: 'all', name: 'Tất cả danh mục' }, ...(lookups?.categories || [])];
  const filteredProducts = products;

  const mapStockToVariant = (stock: number): "success" | "warning" | "danger" | "default" => {
    if (stock === 0) return "danger";
    if (stock < 20) return "warning";
    return "success";
  };

  const getStockLabel = (stock: number) => {
    if (stock === 0) return "Hết hàng";
    if (stock < 20) return "Sắp hết hàng";
    return "Còn hàng";
  };

  const mapStatusToVariant = (status: ProductStatus): "success" | "secondary" | "outline" | "default" => {
    switch (status) {
      case ProductStatus.ACTIVE: return "success";
      case ProductStatus.DISCONTINUED: return "secondary";
      default: return "default";
    }
  };

  const handleCreate = () => {
    setEditingProduct(undefined);
    setIsFormOpen(true);
  };

  const handleEdit = (product: Product) => {
    setEditingProduct(product);
    setIsFormOpen(true);
  };

  const onFormSubmit = (data: any) => {
    if (editingProduct) {
      updateMutation.mutate(
        { id: editingProduct.id, data },
        {
          onSuccess: () => {
            toast.success("Cập nhật sản phẩm thành công.");
            setIsFormOpen(false);
          },
          onError: (error) => toast.error(error.message || "Không cập nhật được sản phẩm."),
        }
      );
    } else {
      createMutation.mutate(data, {
        onSuccess: () => {
          toast.success("Thêm sản phẩm thành công.");
          setPageNumber(1);
          setIsFormOpen(false);
        },
        onError: (error) => toast.error(error.message || "Không thêm được sản phẩm."),
      });
    }
  };

  const handleToggleStatus = (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    toggleStatusMutation.mutate(id, {
      onSuccess: () => {
        toast.success("Đã cập nhật trạng thái sản phẩm.");
      },
      onError: (error) => toast.error(error.message || "Không cập nhật được trạng thái sản phẩm."),
    });
  };

  return (
    <div className="space-y-6">
      {productsError && <p role="alert" className="text-destructive">{productsError.message}</p>}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-xl font-semibold">Sản phẩm</h2>
          <p className="text-muted-foreground">Quản lý thông tin sản phẩm, tồn kho và trạng thái</p>
        </div>
        <div className="flex flex-wrap gap-2">{!readOnly && <Button variant="outline" onClick={() => setIsImportOpen(true)}>Nhập kho cho nhiều sản phẩm</Button>}
        {!readOnly && (
          <Button onClick={handleCreate}>
            <Plus className="mr-2 h-4 w-4" />
            Thêm sản phẩm
          </Button>
        )}
        </div>
      </div>

      <div className="flex flex-col gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Tìm sản phẩm theo tên hoặc hãng..."
            value={searchTerm}
            onChange={(e) => { setSearchTerm(e.target.value); }}
            className="h-11 w-full pl-9"
          />
        </div>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-3">
          <Select
            value={categoryFilter}
            onValueChange={(value) => { setCategoryFilter(value); }}
          >
            <SelectTrigger className="h-11 w-full min-w-0" aria-label="Danh mục">
              <SelectValue placeholder="Tất cả danh mục" />
            </SelectTrigger>
            <SelectContent>
              {categories.map((cat) => (
                <SelectItem key={cat.id} value={cat.id}>
                  {cat.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select value={brandFilter} onValueChange={value => { setBrandFilter(value); }}>
            <SelectTrigger className="h-11 w-full min-w-0" aria-label="Hãng"><SelectValue placeholder="Tất cả hãng" /></SelectTrigger>
            <SelectContent><SelectItem value="all">Tất cả hãng</SelectItem>
              {lookups?.brands.map(brand => <SelectItem key={brand.id} value={brand.id}>{brand.name}</SelectItem>)}
            </SelectContent>
          </Select>
          <Input type="number" min={0} step={1} aria-label="Giá từ (VND)" placeholder="Giá từ (VND)" className="h-11 w-full min-w-0" value={minPrice}
            onChange={event => { setMinPrice(event.target.value); }} />
          <Input type="number" min={0} step={1} aria-label="Giá đến (VND)" placeholder="Giá đến (VND)" className="h-11 w-full min-w-0" value={maxPrice}
            onChange={event => { setMaxPrice(event.target.value); }} />
          <Select value={sortBy} onValueChange={value => { setSortBy(value as ProductQueryParams['sortBy']); }}>
            <SelectTrigger className="h-11 w-full min-w-0" aria-label="Sắp xếp sản phẩm"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="newest">Mới nhất</SelectItem>
              <SelectItem value="price_asc">Giá: Thấp đến Cao</SelectItem>
              <SelectItem value="price_desc">Giá: Cao đến Thấp</SelectItem>
              <SelectItem value="rating">Đánh giá cao nhất</SelectItem>
            </SelectContent>
          </Select>

          <Select
            value={statusFilter}
            onValueChange={(value) => { setStatusFilter(value); }}
          >
            <SelectTrigger className="h-11 w-full min-w-0" aria-label="Trạng thái sản phẩm">
              <SelectValue placeholder="Tất cả trạng thái" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tất cả trạng thái</SelectItem>
              <SelectItem value={ProductStatus.ACTIVE}>Đang kinh doanh</SelectItem>
              <SelectItem value={ProductStatus.DISCONTINUED}>Ngừng kinh doanh</SelectItem>
            </SelectContent>
          </Select>
          <Button variant="outline" className="h-11 w-full sm:col-span-2 xl:col-span-3 xl:w-auto xl:justify-self-end" onClick={() => {
            setSearchTerm(''); setCategoryFilter('all'); setBrandFilter('all'); setMinPrice(''); setMaxPrice('');
            setSortBy('newest'); setStatusFilter('all');
          }}>Xóa lọc</Button>
        </div>
      </div>

      {isLoading || !filtersReady ? (
        <div className="flex h-64 items-center justify-center">
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-muted-foreground font-medium">Đang tải sản phẩm…</p>
          </div>
        </div>
      ) : filteredProducts.length === 0 ? (
        <Card className="flex h-64 flex-col items-center justify-center text-center">
          <PackageSearch className="h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-xl font-semibold">Không tìm thấy sản phẩm</p>
          <p className="text-muted-foreground">Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm</p>
        </Card>
      ) : (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-medium">
              Danh sách sản phẩm ({adminData?.totalCount ?? 0})
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[300px]">Sản phẩm</TableHead>
                  <TableHead>Danh mục</TableHead>
                  <TableHead>Hãng</TableHead>
                  <TableHead>Giá</TableHead>
                  <TableHead>Tồn kho</TableHead>
                  <TableHead>Trạng thái</TableHead>
                  <TableHead className="text-right">Thao tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredProducts.map((product) => {
                  const stockVariant = mapStockToVariant(product.stock ?? 0);
                  const isDiscontinued = product.status === ProductStatus.DISCONTINUED;
                  const isToggling = toggleStatusMutation.isPending && toggleStatusMutation.variables === product.id;

                  return (
                    <TableRow 
                      key={product.id}
                      className="cursor-pointer hover:bg-muted/50 transition-colors"
                      onClick={() => {
                        const basePath = readOnly ? '/sales' : '/business';
                        navigate(`${basePath}/products/${product.id}`);
                      }}
                    >
                      <TableCell>
                        <div className="flex items-center gap-3">
                          <div className="h-12 w-12 flex-shrink-0 overflow-hidden rounded-md border bg-muted">
                            <img
                              src={product.imageUrl || "https://placehold.co/400x400?text=No+Image"}
                              alt={product.name}
                              className="h-full w-full object-cover"
                            />
                          </div>
                          <div className="min-w-0">
                            <div className="font-semibold truncate">{product.name}</div>
                            <div className="text-xs text-muted-foreground font-mono text-[10px]">
                              {product.id}
                            </div>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>{product.categoryName}</TableCell>
                      <TableCell>{product.brand}</TableCell>
                      <TableCell className="font-bold">
                        {product.priceIsUnset ? 'Chưa niêm yết' : formatCurrency(product.price)}
                      </TableCell>
                      <TableCell>
                        {product.stock === undefined ? <span className="text-muted-foreground">—</span> : <div className="flex flex-col gap-1">
                          <span className="text-sm font-medium">{product.stock} thiết bị</span>
                          <Badge variant={stockVariant as any} className="w-fit text-[10px] px-1.5 py-0">
                            {getStockLabel(product.stock)}
                          </Badge>
                        </div>}
                      </TableCell>
                      <TableCell>
                        <Badge variant={mapStatusToVariant(product.status) as any}>
                          {product.status === ProductStatus.ACTIVE ? 'Đang kinh doanh' : 'Ngừng kinh doanh'}
                        </Badge>
                      </TableCell>
                      <TableCell onClick={(e) => e.stopPropagation()} className="text-right">
                        <div className="flex justify-end gap-1">
                          <Button 
                            variant="ghost" 
                            size="icon"
                            onClick={(e) => {
                              e.stopPropagation();
                              const basePath = readOnly ? '/sales' : '/business';
                              navigate(`${basePath}/products/${product.id}`);
                            }}
                            title="Xem chi tiết"
                          >
                            <Eye className="h-4 w-4 text-primary" />
                          </Button>
                          <Button variant="outline" size="icon" title="Xem kho" aria-label={`Kho của ${product.name}`} onClick={event => { event.stopPropagation(); setAdjustingProduct(product); }}>
                            <PackagePlus className="h-4 w-4" />
                          </Button>
                          {!readOnly && (
                            <>
                              <Button 
                                variant="ghost" 
                                size="icon"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  handleEdit(product);
                                }}
                                title="Sửa sản phẩm"
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                              {(
                                <Button 
                                  variant="ghost" 
                                  size="icon"
                                  onClick={(e) => handleToggleStatus(product.id, e)}
                                  title={isDiscontinued ? "Kinh doanh trở lại" : "Ngừng kinh doanh"}
                                  disabled={isToggling}
                                >
                                  {isToggling ? (
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                  ) : (
                                    isDiscontinued ? <RefreshCcw className="h-4 w-4 text-emerald-600" /> : <Ban className="h-4 w-4 text-destructive" />
                                  )}
                                </Button>
                              )}
                            </>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>

          </CardContent>
        </Card>
      )}

      {/* Form Dialog */}
      <ListPagination pageNumber={pageNumber} pageSize={pageSize} totalCount={adminData?.totalCount ?? 0}
        disabled={isFetching || !filtersReady || !!productsError} onPageChange={setPageNumber}
        onPageSizeChange={setPageSize} />
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>{editingProduct ? "Sửa sản phẩm" : "Thêm sản phẩm mới"}</DialogTitle>
            <DialogDescription>
              Nhập thông tin để {editingProduct ? "cập nhật" : "thêm mới"} sản phẩm.
            </DialogDescription>
          </DialogHeader>
          <ProductForm 
            key={editingProduct?.id ?? 'create'}
            initialData={editingProduct} 
            onSubmit={onFormSubmit}
            isLoading={createMutation.isPending || updateMutation.isPending}
          />
        </DialogContent>
      </Dialog>

      {adjustingProduct && <ProductStockDialog key={adjustingProduct.id} product={adjustingProduct} onClose={() => setAdjustingProduct(undefined)} />}
      {stockProductId && stockProduct.error && <div role="alert" className="flex items-center gap-3 text-destructive"><span>{stockProduct.error.message}</span><Button variant="outline" onClick={() => setAdjustingProduct(undefined)}>Đóng kho</Button></div>}
      {!readOnly && isImportOpen && <StockImportDialog onClose={() => setIsImportOpen(false)} />}
    </div>
  );
}
