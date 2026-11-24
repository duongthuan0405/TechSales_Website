import { Bell } from 'lucide-react';
import { Popover, PopoverContent, PopoverTrigger } from '../ui/popover';
import { Button } from '../ui/button';
export function NotificationDropdown() {
  return <Popover>
    <PopoverTrigger asChild>
      <Button variant="ghost" size="icon" aria-label="Thông báo"><Bell className="h-5 w-5" /></Button>
    </PopoverTrigger>
    <PopoverContent align="end"><p className="text-sm text-muted-foreground">Chức năng thông báo chưa có API hỗ trợ.</p></PopoverContent>
  </Popover>;
}
