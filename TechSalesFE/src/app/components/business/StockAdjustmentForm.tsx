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
import { Product } from "../../../models/ui_types/product";
import { Loader2 } from "lucide-react";

const adjustmentSchema = z.object({
  type: z.enum(["ADD", "SET"]),
  value: z.number().int().min(0, "Value must be at least 0"),
});

type AdjustmentValues = z.infer<typeof adjustmentSchema>;

interface StockAdjustmentDialogProps {
  product: Product;
  onSubmit: (data: AdjustmentValues) => void;
  isLoading?: boolean;
}

export function StockAdjustmentForm({ product, onSubmit, isLoading }: StockAdjustmentDialogProps) {
  const form = useForm<AdjustmentValues>({
    resolver: zodResolver(adjustmentSchema),
    defaultValues: {
      type: "ADD",
      value: 0,
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid grid-cols-2 gap-4">
          <FormItem>
            <FormLabel>Current Stock (Read-only)</FormLabel>
            <Input value={product.stock} disabled className="bg-muted font-bold" />
          </FormItem>
          
          <FormField
            control={form.control}
            name="type"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Adjustment Type</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select type" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="ADD">Add Stock (+)</SelectItem>
                    <SelectItem value="SET">Set Quantity (=)</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="value"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Adjustment Value</FormLabel>
              <FormControl>
                <Input 
                  type="number" 
                  placeholder="Enter number..." 
                  {...field} 
                  onChange={(e) => field.onChange(Number(e.target.value))} 
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-2 pt-2">
          <Button type="submit" disabled={isLoading} className="w-full sm:w-auto min-w-[120px]">
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Confirm Adjustment
          </Button>
        </div>
      </form>
    </Form>
  );
}
