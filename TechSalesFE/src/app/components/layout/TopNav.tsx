import { Search, User } from 'lucide-react';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { NotificationDropdown } from './NotificationDropdown';

interface TopNavProps {
  userName: string;
  userRole: string;
  userAvatar?: string;
  onLogout: () => void;
}

export function TopNav({ userName, userRole, userAvatar, onLogout }: TopNavProps) {
  return (
    <div className="flex h-16 items-center justify-between border-b border-border bg-background px-6">
      <div className="flex items-center gap-4 flex-1 max-w-xl">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            type="search"
            placeholder="Tìm kiếm nhanh..."
            className="pl-9 h-10 rounded-xl bg-muted/50 border-none focus-visible:ring-1 focus-visible:ring-primary/20 font-medium text-xs tracking-wider placeholder:text-muted-foreground/50"
          />
        </div>
      </div>
      <div className="flex items-center gap-6">
        <NotificationDropdown />
        <div className="flex items-center gap-3 pl-4 border-l border-border/50">
          <div className="text-right">
            <div className="text-sm font-medium">{userName}</div>
            <div className="text-xs text-muted-foreground">{userRole}</div>
          </div>
          <button className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-primary-foreground overflow-hidden">
            {userAvatar ? (
              <img src={userAvatar} alt={userName} className="h-full w-full object-cover" />
            ) : (
              <User className="h-5 w-5" />
            )}
          </button>
        </div>
        <Button variant="outline" size="sm" onClick={onLogout}>
          Đăng xuất
        </Button>
      </div>
    </div>
  );
}
