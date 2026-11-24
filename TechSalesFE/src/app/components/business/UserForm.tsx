import { PasswordInput } from '../ui/password-input';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Loader2, Mail, Phone, CalendarDays, Fingerprint, ShieldCheck } from 'lucide-react';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '../ui/form';
import { Input } from '../ui/input';
import { Button } from '../ui/button';
import { Checkbox } from '../ui/checkbox';
import { Badge } from '../ui/badge';
import { Avatar, AvatarImage, AvatarFallback } from '../ui/avatar';
import { User, UserRole, UserStatus } from '../../../models/ui_types/user';
import { formatDate } from '../../../utils/format';

const roleNames: Record<UserRole, string> = {
  Staff: 'Nhân viên bán hàng', Customer: 'Khách hàng',
  'Business Admin': 'Quản trị kinh doanh', 'Technical Admin': 'Quản trị kỹ thuật',
};
const statusNames: Record<UserStatus, string> = {
  ACTIVE: 'Đang hoạt động', BLOCKED: 'Đã khóa', PENDING: 'Chờ xác minh', INACTIVE: 'Ngừng hoạt động',
};
const statusColors: Record<UserStatus, string> = {
  ACTIVE: 'border-emerald-200 bg-emerald-500/10 text-emerald-700',
  BLOCKED: 'border-amber-200 bg-amber-500/10 text-amber-700',
  PENDING: 'border-blue-200 bg-blue-500/10 text-blue-700',
  INACTIVE: 'border-slate-200 bg-slate-500/10 text-slate-600',
};
const roleSchema = z.enum(['Staff', 'Customer', 'Business Admin', 'Technical Admin']);
const userSchema = z.object({
  fullName: z.string(), phone: z.string(), status: z.nativeEnum(UserStatus),
  email: z.string().trim().email('Địa chỉ email không hợp lệ.'),
  roles: z.array(roleSchema).min(1, 'Chọn ít nhất một vai trò.').max(4),
  password: z.string().optional(),
});
type UserFormValues = z.infer<typeof userSchema>;
interface UserFormProps {
  initialData?: User;
  onSubmit: (data: UserFormValues & { role: UserRole }) => void;
  isLoading?: boolean;
  forcedRole?: UserRole;
  currentUserRole?: UserRole;
}

export function UserForm({ initialData, onSubmit, isLoading, forcedRole, currentUserRole }: UserFormProps) {
  const schema = initialData ? userSchema : userSchema.extend({
    fullName: z.string().trim().min(1, 'Vui lòng nhập họ và tên.').max(150, 'Họ tên tối đa 150 ký tự.'),
    phone: z.string().trim().regex(/^[0-9]{10,11}$/, 'Số điện thoại phải gồm 10–11 chữ số.'),
    password: z.string().min(8, 'Mật khẩu phải có ít nhất 8 ký tự.').max(256, 'Mật khẩu quá dài.'),
  });
  const form = useForm<UserFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      fullName: initialData?.fullName || '', phone: initialData?.phone || '',
      email: initialData?.email || '', status: initialData?.status || UserStatus.ACTIVE,
      roles: forcedRole ? [forcedRole] : initialData?.roles?.length ? [...initialData.roles] : [initialData?.role || 'Staff'],
      password: '',
    },
  });
  const selectedRoles = form.watch('roles');
  const availableRoles = (Object.keys(roleNames) as UserRole[]).filter(role =>
    forcedRole ? role === forcedRole : currentUserRole === 'Technical Admin' || role === 'Staff' || role === 'Customer');
  return <Form {...form}>
    <form onSubmit={form.handleSubmit(data => onSubmit({ ...data, role: data.roles[0] }))} className="space-y-5">
      <fieldset disabled={isLoading} className="min-w-0 space-y-5">
        {initialData && <>
          <div className="flex flex-wrap items-center gap-4 rounded-xl border border-primary/10 bg-primary/5 p-5">
            <Avatar className="size-20 border-4 border-background shadow-sm">
              <AvatarImage src={initialData.avatarUrl} alt={initialData.fullName || initialData.email} className="object-cover" />
              <AvatarFallback className="bg-primary/10 font-semibold text-primary">{(initialData.fullName.trim() || initialData.email).split(/\s+/).slice(-2).map(word => word[0]).join('').toUpperCase()}</AvatarFallback>
            </Avatar>
            <div className="min-w-0 flex-1 space-y-2">
              <h2 className="break-words text-xl font-bold">{initialData.fullName || 'Chưa cập nhật họ tên'}</h2>
              <div className="flex flex-wrap gap-2">
                {selectedRoles.map(role => <Badge key={role} variant="outline" className="gap-1 bg-background"><ShieldCheck className="size-3.5 text-primary" />{roleNames[role]}</Badge>)}
                <Badge variant="outline" className={`gap-1.5 font-medium ${statusColors[initialData.status]}`}><span className="size-1.5 rounded-full bg-current" />{statusNames[initialData.status]}</Badge>
              </div>
            </div>
          </div>
          <dl className="grid gap-3 sm:grid-cols-2">
            {[
              { label: 'Số điện thoại', value: initialData.phone || 'Chưa cập nhật', Icon: Phone },
              { label: 'Ngày sinh', value: initialData.dateOfBirth ? formatDate(initialData.dateOfBirth) : 'Chưa cập nhật', Icon: CalendarDays },
              { label: 'Ngày tham gia', value: formatDate(initialData.createdAt), Icon: CalendarDays },
              { label: 'Mã tài khoản', value: initialData.id, Icon: Fingerprint },
            ].map(({ label, value, Icon }) => <div key={label} className="min-w-0 rounded-xl border bg-muted/20 p-4">
              <dt className="mb-2 flex items-center gap-2 text-xs font-medium text-muted-foreground"><Icon className="size-4 text-primary/70" />{label}</dt>
              <dd className="break-words text-sm font-semibold">{value}</dd>
            </div>)}
          </dl>
        </>}
        {!initialData && <div className="grid gap-4 sm:grid-cols-2">
          <FormField control={form.control} name="fullName" render={({ field }) => <FormItem>
            <FormLabel>Họ và tên</FormLabel><FormControl><Input {...field} required maxLength={150} autoComplete="name" disabled={isLoading} /></FormControl><FormMessage />
          </FormItem>} />
          <FormField control={form.control} name="phone" render={({ field }) => <FormItem>
            <FormLabel>Số điện thoại</FormLabel><FormControl><Input {...field} required type="tel" autoComplete="tel" disabled={isLoading} /></FormControl><FormMessage />
          </FormItem>} />
        </div>}
        <FormField control={form.control} name="email" render={({ field }) => <FormItem>
          <FormLabel className="flex items-center gap-2"><Mail className="size-4 text-primary" />Địa chỉ email</FormLabel>
          <FormControl><Input type="email" placeholder="email@example.com" {...field} disabled={isLoading} /></FormControl><FormMessage />
        </FormItem>} />
        {!initialData && <FormField control={form.control} name="password" render={({ field }) => <FormItem>
          <FormLabel>Mật khẩu tạm thời</FormLabel><FormControl><PasswordInput  autoComplete="new-password" {...field} disabled={isLoading} /></FormControl>
          <p className="text-xs text-muted-foreground">Cung cấp mật khẩu này cho chủ tài khoản để đăng nhập.</p><FormMessage />
        </FormItem>} />}
        <FormField control={form.control} name="roles" render={({ field }) => <FormItem>
          <FormLabel>Vai trò</FormLabel><p className="text-xs text-muted-foreground">Có thể chọn nhiều vai trò cho một tài khoản.</p>
          <div className="grid gap-3 sm:grid-cols-2">
            {availableRoles.map(role => <label key={role} className={`flex cursor-pointer items-center gap-3 rounded-xl border p-4 text-sm font-medium ${field.value.includes(role) ? 'border-primary/40 bg-primary/5' : 'bg-muted/20'}`}>
              <Checkbox checked={field.value.includes(role)} disabled={isLoading || !!forcedRole}
                onCheckedChange={checked => field.onChange(checked === true ? [...new Set([...field.value, role])] : field.value.filter(item => item !== role))} />
              {roleNames[role]}
            </label>)}
          </div><FormMessage />
        </FormItem>} />
        {initialData && <p className="rounded-lg bg-muted/40 p-3 text-xs text-muted-foreground">Hồ sơ cá nhân do chủ tài khoản cập nhật. Trạng thái thay đổi qua xác minh email, khóa, mở khóa hoặc ngừng hoạt động.</p>}
      </fieldset>
      <div className="flex justify-end border-t pt-4"><Button type="submit" disabled={isLoading}>
        {isLoading && <Loader2 className="mr-2 size-4 animate-spin" />}{initialData ? 'Lưu thay đổi' : 'Tạo tài khoản'}
      </Button></div>
    </form>
  </Form>;
}
