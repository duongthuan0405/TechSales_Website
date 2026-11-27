import { useListUrlState } from '../../../dataHook/useListUrlState';
import { useState } from 'react';
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Edit, Loader2, Plus, Search } from 'lucide-react';
import { toast } from 'sonner';
import { brandService } from '../../../services/brandService';
import { categoryService } from '../../../services/categoryService';
import { useDebouncedValue } from '../../../dataHook/useDebouncedValue';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Card, CardContent } from '../../components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogFooter } from '../../components/ui/dialog';
import { ListPagination } from '../../components/ui/list-pagination';

export function CatalogLookupManagement({ kind }: { kind: 'brands' | 'categories' }) {
  const title = kind === 'brands' ? 'thương hiệu' : 'danh mục';
  const list = useListUrlState({ keyword: '' }, kind + '.');
  const { pageNumber, pageSize, setPageNumber, setPageSize } = list;
  const [keyword, setKeyword] = list.field('keyword');
  const debouncedKeyword = useDebouncedValue(keyword);


  const [open, setOpen] = useState(false);
  const [editingId, setEditingId] = useState<string>();
  const [name, setName] = useState('');
  const client = useQueryClient();
  const { data, isLoading, isFetching, error, refetch } = useQuery({
    queryKey: [kind, 'admin', { pageNumber, pageSize, keyword: debouncedKeyword }],
    queryFn: () => kind === 'brands'
      ? brandService.getAdminBrands({ pageNumber, pageSize, keyword: debouncedKeyword })
      : categoryService.getAdminCategories({ pageNumber, pageSize, keyword: debouncedKeyword }),
    enabled: keyword === debouncedKeyword,
    placeholderData: keepPreviousData,
  });
  const mutation = useMutation({
    mutationFn: async () => {
      const input = { name: name.trim() };
      if (kind === 'brands') {
        return editingId ? brandService.updateBrand(editingId, input) : brandService.createBrand(input);
      }
      return editingId ? categoryService.updateCategory(editingId, input) : categoryService.createCategory(input);
    },
    onSuccess: () => {
      for (const key of [kind, 'product-lookups', 'products', 'admin-products', 'product']) {
        client.invalidateQueries({ queryKey: [key] });
      }
      toast.success(`Đã ${editingId ? 'cập nhật' : 'thêm'} ${title}.`);
      setPageNumber(1);
      setOpen(false);
    },
    onError: (error: Error) => toast.error(error.message),
  });
  return <div className="space-y-6">
    <div className="flex flex-wrap items-center justify-between gap-3">
      <h2 className="text-xl font-semibold">{kind === 'brands' ? 'Thương hiệu' : 'Danh mục'}</h2>
      <Button onClick={() => { setEditingId(undefined); setName(''); setOpen(true); }}><Plus className="mr-2 h-4 w-4" />Thêm {title}</Button>
    </div>
    <div className="relative">
      <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <Input aria-label={`Tìm ${title}`} placeholder={`Tìm tên ${title}...`} className="pl-9" value={keyword} onChange={event => { setKeyword(event.target.value); }} />
    </div>
    {error ? <div role="alert" className="space-y-2"><p className="text-destructive">{error.message}</p><Button variant="outline" onClick={() => refetch()}>Thử lại</Button></div>
      : isLoading ? <div className="flex h-40 items-center justify-center"><Loader2 className="h-8 w-8 animate-spin" aria-label="Đang tải" /></div>
      : <Card><CardContent className="pt-6"><Table>
        <TableHeader><TableRow><TableHead>Tên {title}</TableHead><TableHead className="text-right">Thao tác</TableHead></TableRow></TableHeader>
        <TableBody>{data?.items.length ? data.items.map(item => <TableRow key={item.id}>
          <TableCell className="font-medium">{item.name}</TableCell>
          <TableCell className="text-right"><Button variant="ghost" size="icon" aria-label={`Sửa ${item.name}`} onClick={() => { setEditingId(item.id); setName(item.name); setOpen(true); }}><Edit className="h-4 w-4" /></Button></TableCell>
        </TableRow>) : <TableRow><TableCell colSpan={2} className="py-10 text-center text-muted-foreground">Không tìm thấy {title}.</TableCell></TableRow>}</TableBody>
      </Table></CardContent></Card>}
    <ListPagination pageNumber={pageNumber} pageSize={pageSize} totalCount={data?.totalCount ?? 0} disabled={isFetching || keyword !== debouncedKeyword}
      onPageChange={setPageNumber} onPageSizeChange={setPageSize} />
    <Dialog open={open} onOpenChange={value => { if (!mutation.isPending) setOpen(value); }}>
      <DialogContent className="max-w-md"><DialogHeader>
        <DialogTitle>{editingId ? 'Sửa' : 'Thêm'} {title}</DialogTitle>
        <DialogDescription>Nhập tên {title} của sản phẩm.</DialogDescription>
      </DialogHeader>
        <form className="space-y-4" onSubmit={event => { event.preventDefault(); if (name.trim()) mutation.mutate(); }}>
          <div className="space-y-2"><Label htmlFor={`${kind}-name`}>Tên {title}</Label><Input id={`${kind}-name`} value={name} onChange={event => setName(event.target.value)} required maxLength={100} disabled={mutation.isPending} /></div>
          <DialogFooter><Button type="button" variant="outline" disabled={mutation.isPending} onClick={() => setOpen(false)}>Hủy</Button>
            <Button type="submit" disabled={mutation.isPending || !name.trim()}>{mutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}Lưu</Button></DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  </div>;
}
