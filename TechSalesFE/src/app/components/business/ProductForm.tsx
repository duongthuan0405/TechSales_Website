import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '../ui/form';
import { Input } from '../ui/input';
import { Button } from '../ui/button';
import { Textarea } from '../ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { Product, ProductStatus } from '../../../models/ui_types/product';
import { Loader2 } from 'lucide-react';
import { useGetProductLookups } from '../../../dataHook/productDataHook';

const productSchema = z.object({
  name: z.string().trim().min(1, 'Vui lòng nhập tên sản phẩm.').max(255, 'Tên sản phẩm tối đa 255 ký tự.'),
  description: z.string(),
  price: z.number().int('Giá bán phải là số nguyên đồng.').min(0, 'Giá bán không được âm.').max(Number.MAX_SAFE_INTEGER, 'Giá bán quá lớn.').nullable(),
  status: z.nativeEnum(ProductStatus),
  categoryId: z.string().uuid('Vui lòng chọn danh mục.'),
  brandId: z.string().uuid('Vui lòng chọn thương hiệu.'),
  imageFiles: z.array(z.instanceof(File)).max(10, 'Mỗi sản phẩm tối đa 10 ảnh.')
    .refine(files => files.every(file => file.size > 0 && file.size <= 5 * 1024 * 1024), 'Mỗi ảnh phải có dữ liệu và không vượt quá 5 MB.')
    .refine(files => files.every(file => ['image/jpeg', 'image/png', 'image/webp'].includes(file.type)), 'Chỉ hỗ trợ JPG, PNG và WebP.'),
  primaryImageIndex: z.number().int().min(0).optional(),
});
type ProductFormValues = z.infer<typeof productSchema>;
interface ProductFormProps { initialData?: Product; onSubmit: (data: ProductFormValues) => void; isLoading?: boolean; }

export function ProductForm({ initialData, onSubmit, isLoading }: ProductFormProps) {
  const form = useForm<ProductFormValues>({ resolver: zodResolver(productSchema), defaultValues: {
    name: initialData?.name || '', description: initialData?.description || '',
    price: initialData?.priceIsUnset ? null : initialData?.price ?? 0,
    status: initialData?.status || ProductStatus.ACTIVE, categoryId: initialData?.categoryId || '',
    brandId: initialData?.brandId || '', imageFiles: [], primaryImageIndex: undefined,
  } });
  const { data: lookups, isLoading: loadingLookups, error: lookupError } = useGetProductLookups();
  const files = form.watch('imageFiles');
  const primaryIndex = form.watch('primaryImageIndex') ?? 0;
  const [previews, setPreviews] = useState<string[]>([]);
  useEffect(() => {
    const urls = files.map(file => URL.createObjectURL(file));
    setPreviews(urls);
    return () => urls.forEach(url => URL.revokeObjectURL(url));
  }, [files]);
  const ready = !!lookups?.brands.length && !!lookups?.categories.length && !lookupError;

  return <Form {...form}>
    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
      {lookupError && <p role="alert" className="text-sm text-destructive">{lookupError.message}</p>}
      {!loadingLookups && !lookupError && !ready && <p role="alert" className="text-sm text-destructive">Cần có thương hiệu và danh mục trong hệ thống trước khi thêm sản phẩm.</p>}
      <FormField control={form.control} name="name" render={({ field }) => <FormItem>
        <FormLabel>Tên sản phẩm</FormLabel><FormControl><Input {...field} placeholder="Ví dụ: MacBook Air M3" /></FormControl><FormMessage />
      </FormItem>} />
      <div className="grid grid-cols-2 gap-4">
        <FormField control={form.control} name="categoryId" render={({ field }) => <FormItem>
          <FormLabel>Danh mục</FormLabel><Select value={field.value} onValueChange={field.onChange}>
            <FormControl><SelectTrigger disabled={loadingLookups}><SelectValue placeholder="Chọn danh mục" /></SelectTrigger></FormControl>
            <SelectContent>{lookups?.categories.map(category => <SelectItem key={category.id} value={category.id}>{category.name}</SelectItem>)}</SelectContent>
          </Select><FormMessage />
        </FormItem>} />
        <FormField control={form.control} name="brandId" render={({ field }) => <FormItem>
          <FormLabel>Thương hiệu</FormLabel><Select value={field.value} onValueChange={field.onChange}>
            <FormControl><SelectTrigger disabled={loadingLookups}><SelectValue placeholder="Chọn thương hiệu" /></SelectTrigger></FormControl>
            <SelectContent>{lookups?.brands.map(brand => <SelectItem key={brand.id} value={brand.id}>{brand.name}</SelectItem>)}</SelectContent>
          </Select><FormMessage />
        </FormItem>} />
      </div>
      <FormField control={form.control} name="price" render={({ field }) => <FormItem>
        <FormLabel>Giá bán (VND)</FormLabel><FormControl><Input type="number" min="0" step="1" {...field} value={field.value ?? ''} onChange={event => field.onChange(event.target.value === '' ? null : Number(event.target.value))} /></FormControl><FormMessage />
      </FormItem>} />
      {initialData && <FormField control={form.control} name="status" render={({ field }) => <FormItem>
        <FormLabel>Trạng thái</FormLabel><Select value={field.value} onValueChange={field.onChange}>
          <FormControl><SelectTrigger><SelectValue /></SelectTrigger></FormControl><SelectContent>
            <SelectItem value={ProductStatus.ACTIVE}>Đang kinh doanh</SelectItem>
            <SelectItem value={ProductStatus.DISCONTINUED}>Ngừng kinh doanh</SelectItem>
          </SelectContent></Select><FormMessage />
      </FormItem>} />}
      <FormField control={form.control} name="description" render={({ field }) => <FormItem>
        <FormLabel>Mô tả</FormLabel><FormControl><Textarea {...field} className="min-h-[100px]" placeholder="Mô tả sản phẩm..." /></FormControl><FormMessage />
      </FormItem>} />
      {!initialData ? <FormField control={form.control} name="imageFiles" render={() => <FormItem>
        <FormLabel>Ảnh sản phẩm</FormLabel><FormControl><Input type="file" multiple accept="image/jpeg,image/png,image/webp" onChange={event => {
          const selected = Array.from(event.target.files || []);
          form.setValue('imageFiles', selected, { shouldValidate: true });
          form.setValue('primaryImageIndex', selected.length ? 0 : undefined);
        }} /></FormControl>
        <p className="text-xs text-muted-foreground">Tối đa 10 ảnh, mỗi ảnh 5 MB. Chọn một ảnh làm ảnh đại diện.</p>
        <FormMessage />
        <div className="grid grid-cols-3 gap-3">{previews.map((url, index) => <label key={url} className="cursor-pointer rounded-lg border p-2">
          <img src={url} alt={`Ảnh ${index + 1}`} className="h-24 w-full object-contain" />
          <span className="mt-2 flex items-center gap-2 text-xs"><input type="radio" name="primaryImage" checked={primaryIndex === index} onChange={() => form.setValue('primaryImageIndex', index)} />Ảnh đại diện</span>
        </label>)}</div>
      </FormItem>} /> : <div><p className="mb-2 text-sm font-medium">Ảnh hiện có</p>
        <div className="grid grid-cols-4 gap-2">{initialData.images?.map(url => <img key={url} src={url} alt={initialData.name} className="h-24 w-full rounded border object-contain" />)}</div>
      </div>}
      <div className="flex justify-end pt-4"><Button type="submit" disabled={isLoading || !ready}>
        {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}{initialData ? 'Lưu thay đổi' : 'Thêm sản phẩm'}
      </Button></div>
    </form>
  </Form>;
}
