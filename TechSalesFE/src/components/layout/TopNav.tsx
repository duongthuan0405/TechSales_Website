import { Bell, Search, User } from 'lucide-react';
import { NotificationDropdown } from './NotificationDropdown';
import { Button } from '../ui/button';
import { Input } from '../ui/input';

interface TopNavProps {
  userName: string;
  userRole: string;
  onLogout: () => void;
}

export function TopNav({ userName, userRole, onLogout }: TopNavProps) {
  return (
    <div className="flex h-16 items-center justify-between border-b border-border bg-background px-6">
      <div className="flex items-center gap-4 flex-1 max-w-xl">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            type="search"
            placeholder="Search..."
            className="pl-9"
          />
        </div>
      </div>
      <div className="flex items-center gap-4">
        <NotificationDropdown />
        <div className="flex items-center gap-3">
          <div className="text-right">
            <div className="text-sm font-medium">{userName}</div>
            <div className="text-xs text-muted-foreground">{userRole}</div>
          </div>
          <button className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-primary-foreground">
            <User className="h-5 w-5" />
          </button>
        </div>
        <Button variant="outline" size="sm" onClick={onLogout}>
          Logout
        </Button>
      </div>
    </div>
  );
}
