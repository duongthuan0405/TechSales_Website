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
  useToggleProductStatus 
} from "../../../dataHook/productDataHook";
import { useNavigate } from "react-router";
import { Product, ProductStatus } from "../../../models/ui_types/product";
import { ProductForm } from "../../components/business/ProductForm";
import { StockAdjustmentForm } from "../../components/business/StockAdjustmentForm";
import { useUpdateInventory } from "../../../dataHook/productDataHook";
import { toast } from "sonner";
import { formatCurrency } from "../../../utils/format";

interface ProductManagementPageProps {
  readOnly?: boolean;
}

export function ProductManagementPage({ readOnly = false }: ProductManagementPageProps) {
  const navigate = useNavigate();
  const { data: adminData, isLoading } = useGetAdminProducts({ pageSize: 100 });
  const products = adminData?.items || [];
  const createMutation = useCreateProduct();
  const updateMutation = useUpdateProduct();
  const toggleStatusMutation = useToggleProductStatus();
  const updateInventoryMutation = useUpdateInventory();

  const [searchTerm, setSearchTerm] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("all");
  const [statusFilter, setStatusFilter] = useState("all");
  
  // Dialog states
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | undefined>(undefined);
  const [isStockOpen, setIsStockOpen] = useState(false);
  const [adjustingProduct, setAdjustingProduct] = useState<Product | undefined>(undefined);

  const categories = [
    "all",
    ...Array.from(new Set(products.map((p) => p.categoryName))).filter(Boolean),
  ];

  const filteredProducts = products.filter((product) => {
    const matchesSearch =
      product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.brand?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesCategory =
      categoryFilter === "all" || product.categoryName === categoryFilter;
    const matchesStatus = 
      statusFilter === "all" || product.status === statusFilter;
    return matchesSearch && matchesCategory && matchesStatus;
  });

  const mapStockToVariant = (stock: number): "success" | "warning" | "danger" | "default" => {
    if (stock === 0) return "danger";
    if (stock < 20) return "warning";
    return "success";
  };

  const getStockLabel = (stock: number) => {
    if (stock === 0) return "Out of Stock";
    if (stock < 20) return "Low Stock";
    return "In Stock";
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
            toast.success("Product updated successfully");
            setIsFormOpen(false);
          },
          onError: () => toast.error("Failed to update product"),
        }
      );
    } else {
      createMutation.mutate(data, {
        onSuccess: () => {
          toast.success("Product created successfully");
          setIsFormOpen(false);
        },
        onError: () => toast.error("Failed to create product"),
      });
    }
  };

  const handleToggleStatus = (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    toggleStatusMutation.mutate(id, {
      onSuccess: () => {
        toast.success(`Product discontinued successfully`);
      },
      onError: () => toast.error("Failed to update product status"),
    });
  };

  const handleAdjustStock = (product: Product, e: React.MouseEvent) => {
    e.stopPropagation();
    setAdjustingProduct(product);
    setIsStockOpen(true);
  };

  const onStockSubmit = (data: { value: number; type: "ADD" | "SET" }) => {
    if (!adjustingProduct) return;
    updateInventoryMutation.mutate(
      { id: adjustingProduct.id, ...data },
      {
        onSuccess: () => {
          toast.success("Stock adjusted successfully");
          setIsStockOpen(false);
        },
        onError: () => toast.error("Failed to adjust stock"),
      }
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Product Management</h1>
          <p className="text-muted-foreground">Manage your product catalog, inventory and status</p>
        </div>
        {!readOnly && (
          <Button onClick={handleCreate}>
            <Plus className="mr-2 h-4 w-4" />
            Add Product
          </Button>
        )}
      </div>

      <div className="flex flex-col gap-4 sm:flex-row">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search products by name or brand..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-9"
          />
        </div>
        <div className="flex gap-2">
          <Select
            value={categoryFilter}
            onValueChange={setCategoryFilter}
          >
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="All Categories" />
            </SelectTrigger>
            <SelectContent>
              {categories.map((cat) => (
                <SelectItem key={cat as string} value={cat as string ?? ""}>
                  {cat === "all" ? "All Categories" : cat as string}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select
            value={statusFilter}
            onValueChange={setStatusFilter}
          >
            <SelectTrigger className="w-[160px]">
              <SelectValue placeholder="All Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Status</SelectItem>
              <SelectItem value={ProductStatus.ACTIVE}>Active</SelectItem>
              <SelectItem value={ProductStatus.DISCONTINUED}>Discontinued</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-muted-foreground font-medium">Loading catalog...</p>
          </div>
        </div>
      ) : filteredProducts.length === 0 ? (
        <Card className="flex h-64 flex-col items-center justify-center text-center">
          <PackageSearch className="h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-xl font-semibold">No products found</p>
          <p className="text-muted-foreground">Try adjusting your filters or search term</p>
        </Card>
      ) : (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-medium">
              Products List ({filteredProducts.length})
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[300px]">Product</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead>Brand</TableHead>
                  <TableHead>Price</TableHead>
                  <TableHead>Stock</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredProducts.map((product) => {
                  const stockVariant = mapStockToVariant(product.stock);
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
                        {formatCurrency(product.price)}
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-col gap-1">
                          <span className="text-sm font-medium">{product.stock} units</span>
                          <Badge variant={stockVariant as any} className="w-fit text-[10px] px-1.5 py-0">
                            {getStockLabel(product.stock)}
                          </Badge>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant={mapStatusToVariant(product.status) as any}>
                          {product.status}
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
                            title="View Details"
                          >
                            <Eye className="h-4 w-4 text-primary" />
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
                                title="Edit Product"
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                              <Button 
                                variant="ghost" 
                                size="icon"
                                onClick={(e) => handleAdjustStock(product, e)}
                                title="Adjust Stock"
                              >
                                <PackagePlus className="h-4 w-4 text-orange-500" />
                              </Button>
                              {!isDiscontinued && (
                                <Button 
                                  variant="ghost" 
                                  size="icon"
                                  onClick={(e) => handleToggleStatus(product.id, e)}
                                  title="Discontinue Product"
                                  disabled={isToggling}
                                >
                                  {isToggling ? (
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                  ) : (
                                    <Ban className="h-4 w-4 text-destructive" />
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
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>{editingProduct ? "Edit Product" : "Add New Product"}</DialogTitle>
            <DialogDescription>
              Fill in the details below to {editingProduct ? "update the" : "create a new"} product.
            </DialogDescription>
          </DialogHeader>
          <ProductForm 
            initialData={editingProduct} 
            onSubmit={onFormSubmit}
            isLoading={createMutation.isPending || updateMutation.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Stock Adjustment Dialog */}
      <Dialog open={isStockOpen} onOpenChange={setIsStockOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Stock Adjustment</DialogTitle>
            <DialogDescription>
              Modify inventory for <span className="font-bold text-foreground">{adjustingProduct?.name}</span>.
            </DialogDescription>
          </DialogHeader>
          {adjustingProduct && (
            <StockAdjustmentForm 
              product={adjustingProduct} 
              onSubmit={onStockSubmit}
              isLoading={updateInventoryMutation.isPending}
            />
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
