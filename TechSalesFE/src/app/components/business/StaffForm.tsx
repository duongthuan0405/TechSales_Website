import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "../ui/form";
import { Input } from "../ui/input";
import { Button } from "../ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../ui/select";
import { User, UserRole, UserStatus } from "../../../models/ui_types/user";
import { Loader2 } from "lucide-react";

const staffSchema = z.object({
  fullName: z.string().min(2, "Họ và tên phải có ít nhất 2 ký tự."),
  email: z.string().email("Địa chỉ email không hợp lệ."),
  phone: z.string().min(10, "Số điện thoại phải có ít nhất 10 chữ số."),
  role: z.enum(['Staff', 'Business Admin', 'Technical Admin']),
  status: z.nativeEnum(UserStatus),
});

type StaffFormValues = z.infer<typeof staffSchema>;

interface StaffFormProps {
  initialData?: User;
  onSubmit: (data: StaffFormValues) => void;
  isLoading?: boolean;
}

export function StaffForm({ initialData, onSubmit, isLoading }: StaffFormProps) {
  const form = useForm<StaffFormValues>({
    resolver: zodResolver(staffSchema),
    defaultValues: {
      fullName: initialData?.fullName || "",
      email: initialData?.email || "",
      phone: initialData?.phone || "",
      role: initialData?.role === 'Customer' ? 'Staff' : (initialData?.role || 'Staff') as any,
      status: initialData?.status || UserStatus.ACTIVE,
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="fullName"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Họ và tên</FormLabel>
              <FormControl>
                <Input placeholder="John Doe" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Địa chỉ email</FormLabel>
              <FormControl>
                <Input placeholder="john@techsales.com" {...field} disabled={!!initialData} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="phone"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Số điện thoại</FormLabel>
              <FormControl>
                <Input placeholder="+1 (555) 000-0000" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="role"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Vai trò</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn vai trò" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="Staff">Nhân viên bán hàng</SelectItem>
                    <SelectItem value="Business Admin">Quản trị kinh doanh</SelectItem>
                    <SelectItem value="Technical Admin">Quản trị kỹ thuật</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="status"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Trạng thái</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn trạng thái" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value={UserStatus.ACTIVE}>Đang hoạt động</SelectItem>
                    <SelectItem value={UserStatus.BLOCKED}>Đã khóa</SelectItem>
                    <SelectItem value={UserStatus.PENDING}>Chờ xác minh</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="flex justify-end gap-2 pt-4">
          <Button type="submit" disabled={isLoading} className="w-full">
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {initialData ? "Cập nhật nhân viên" : "Thêm nhân viên"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
