import { forwardRef, useState } from 'react';
import { format, isValid, parse } from 'date-fns';
import { vi } from 'date-fns/locale';
import { CalendarIcon } from 'lucide-react';
import { Button } from './button';
import { Calendar } from './calendar';
import { Popover, PopoverContent, PopoverTrigger } from './popover';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './select';

interface DatePickerProps {
  id?: string;
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  placeholder?: string;
  maxDate?: Date;
  clearable?: boolean;
}

export const DatePicker = forwardRef<HTMLButtonElement, DatePickerProps>(function DatePicker({ id, value, onChange, disabled, placeholder = 'Chọn ngày', maxDate, clearable = false }, ref) {
  const parsed = value ? parse(value, 'yyyy-MM-dd', new Date()) : undefined;
  const selected = parsed && isValid(parsed) ? parsed : undefined;
  const [open, setOpen] = useState(false);
  const [month, setMonth] = useState(selected ?? new Date());
  const currentYear = new Date().getFullYear();
  const lastYear = maxDate?.getFullYear() ?? Math.max(currentYear + 10, selected?.getFullYear() ?? 0);
  const years = Array.from({ length: lastYear - 1900 + 1 }, (_, index) => lastYear - index);

  return <Popover open={open} onOpenChange={next => {
    setOpen(next);
    if (next && selected) setMonth(selected);
  }}>
    <PopoverTrigger asChild>
      <Button ref={ref} id={id} type="button" variant="outline" disabled={disabled}
        className={`h-11 w-full justify-start text-left font-normal ${!selected ? 'text-muted-foreground' : ''}`}>
        <CalendarIcon className="mr-2 h-4 w-4" aria-hidden="true" />
        {selected ? format(selected, 'dd/MM/yyyy') : placeholder}
      </Button>
    </PopoverTrigger>
    <PopoverContent align="start" className="w-auto p-0">
      <div className="flex gap-2 px-3 pt-3">
        <Select value={String(month.getMonth())} onValueChange={value => setMonth(new Date(month.getFullYear(), Number(value), 1))}>
          <SelectTrigger className="w-[135px]" aria-label="Chọn tháng"><SelectValue /></SelectTrigger>
          <SelectContent>{Array.from({ length: 12 }, (_, index) => <SelectItem key={index} value={String(index)}>Tháng {index + 1}</SelectItem>)}</SelectContent>
        </Select>
        <Select value={String(month.getFullYear())} onValueChange={value => setMonth(new Date(Number(value), month.getMonth(), 1))}>
          <SelectTrigger className="w-[100px]" aria-label="Chọn năm"><SelectValue /></SelectTrigger>
          <SelectContent>{years.map(year => <SelectItem key={year} value={String(year)}>{year}</SelectItem>)}</SelectContent>
        </Select>
      </div>
      <Calendar mode="single" required locale={vi} selected={selected} month={month} onMonthChange={setMonth}
        fromYear={1900} toYear={lastYear} toDate={maxDate} disabled={maxDate ? { after: maxDate } : undefined} initialFocus
        onSelect={date => {
          if (!date) return;
          onChange(format(date, 'yyyy-MM-dd'));
          setOpen(false);
        }} />
      {clearable && value && <div className="border-t p-2"><Button type="button" variant="ghost" className="w-full" onClick={() => { onChange(''); setOpen(false); }}>Bỏ chọn ngày</Button></div>}
    </PopoverContent>
  </Popover>;
});
