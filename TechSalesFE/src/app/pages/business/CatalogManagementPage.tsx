import { useSearchParams } from 'react-router';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../../components/ui/tabs';
import { ProductManagementPage } from './ProductManagementPage';
import { CatalogLookupManagement } from './CatalogLookupManagement';

export function CatalogManagementPage() {
  const [params, setParams] = useSearchParams();
  const requested = params.get('tab');
  const tab = requested === 'categories' || requested === 'brands' ? requested : 'products';
  return <div className="min-w-0 space-y-6">
    <h1 className="text-3xl font-bold tracking-tight">Quản lý sản phẩm</h1>
    <Tabs value={tab} onValueChange={value => setParams({ tab: value })}>
      <TabsList className="mb-4"><TabsTrigger value="products">Sản phẩm</TabsTrigger><TabsTrigger value="categories">Danh mục</TabsTrigger><TabsTrigger value="brands">Thương hiệu</TabsTrigger></TabsList>
      <TabsContent value="products"><ProductManagementPage /></TabsContent>
      <TabsContent value="categories"><CatalogLookupManagement key="categories" kind="categories" /></TabsContent>
      <TabsContent value="brands"><CatalogLookupManagement key="brands" kind="brands" /></TabsContent>
    </Tabs>
  </div>;
}
