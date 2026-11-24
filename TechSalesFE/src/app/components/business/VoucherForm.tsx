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
import { VoucherType } from "../../../models/ui_types/voucher";
import { Loader2 } from "lucide-react";
import { DatePicker } from '../ui/date-picker';

const voucherSchema = z.object({
  code: z.string().min(3, "Mã phải có ít nhất 3 ký tự.").max(20, 'Mã tối đa 20 ký tự.'),
  type: z.nativeEnum(VoucherType),
  value: z.coerce.number().min(0.01, "Giá trị giảm phải lớn hơn 0."),
  maxUsage: z.coerce.number().int('Số lượt sử dụng phải là số nguyên.').min(1, "Số lượt sử dụng tối thiểu là 1."),
  minOrderAmount: z.coerce.number().min(0, "Giá trị đơn tối thiểu không được âm."),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
}).refine(data => !data.startDate || !data.endDate || data.endDate >= data.startDate, {
  message: 'Ngày kết thúc không được trước ngày bắt đầu.', path: ['endDate'],
});

type VoucherFormValues = z.infer<typeof voucherSchema>;

interface VoucherFormProps {
  onSubmit: (data: any) => void;
  isLoading?: boolean;
}

export function VoucherForm({ onSubmit, isLoading }: VoucherFormProps) {
  const form = useForm<VoucherFormValues>({
    resolver: zodResolver(voucherSchema) as any,
    defaultValues: {
      code: "",
      type: VoucherType.FIXED,
      value: 1,
      maxUsage: 100,
      minOrderAmount: 0,
      startDate: "",
      endDate: "",
    },
  });

  const handleFormSubmit = (data: VoucherFormValues) => {
    // Transform empty strings to undefined and convert to ISO string for the API
    const submissionData = {
      ...data,
      code: data.code.toUpperCase(),
      startDate: data.startDate ? new Date(data.startDate).toISOString() : undefined,
      endDate: data.endDate ? new Date(data.endDate).toISOString() : undefined,
    };
    onSubmit(submissionData);
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleFormSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="code"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Mã giảm giá</FormLabel>
              <FormControl>
                <Input placeholder="SUMMER2024" {...field} className="uppercase" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="type"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Loại giảm giá</FormLabel>
                <Select onValueChange={field.onChange} value={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn loại" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value={VoucherType.FIXED}>Số tiền (VND)</SelectItem>
                    <SelectItem value={VoucherType.PERCENTAGE}>Phần trăm (%)</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="value"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Giá trị giảm</FormLabel>
                <FormControl>
                  <Input type="number" step="0.01" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="maxUsage"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Lượt sử dụng tối đa</FormLabel>
                <FormControl>
                  <Input type="number" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="minOrderAmount"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Giá trị đơn tối thiểu (VND)</FormLabel>
                <FormControl>
                  <Input type="number" step="0.01" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="startDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Ngày bắt đầu (tùy chọn)</FormLabel>
                <FormControl>
                  <DatePicker value={field.value ?? ''} onChange={field.onChange} placeholder="Chọn ngày bắt đầu" disabled={isLoading} clearable />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="endDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Ngày kết thúc (tùy chọn)</FormLabel>
                <FormControl>
                  <DatePicker value={field.value ?? ''} onChange={field.onChange} placeholder="Chọn ngày kết thúc" disabled={isLoading} clearable />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="flex justify-end pt-4">
          <Button type="submit" disabled={isLoading} className="w-full">
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Tạo mã giảm giá
          </Button>
        </div>
      </form>
    </Form>
  );
}
