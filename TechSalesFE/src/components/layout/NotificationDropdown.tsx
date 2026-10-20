import { Bell, Check, ExternalLink, Loader2, Mail, MessageSquare, Shield, Tag } from 'lucide-react';
import { Popover, PopoverContent, PopoverTrigger } from '../ui/popover';
import { Button } from '../ui/button';
import { useGetNotifications, useMarkAllAsReadMutation, useMarkAsReadMutation } from '../../dataHook/notificationDataHook';
import { NotificationType } from '../../models/ui_types/notification';
import { cn } from '../../utils/cn';
import { useNavigate } from 'react-router';

export function NotificationDropdown() {
  const navigate = useNavigate();
  const { data: notifications = [], isLoading } = useGetNotifications();
  const { mutate: markAsRead } = useMarkAsReadMutation();
  const { mutate: markAllAsRead, isPending: isMarkingAll } = useMarkAllAsReadMutation();

  const unreadCount = notifications.filter(n => !n.isRead).length;

  const getIcon = (type: NotificationType) => {
    switch (type) {
      case 'order': return <Tag className="h-4 w-4 text-blue-500" />;
      case 'promo': return <MessageSquare className="h-4 w-4 text-emerald-500" />;
      case 'security': return <Shield className="h-4 w-4 text-rose-500" />;
      case 'system': return <Mail className="h-4 w-4 text-zinc-500" />;
      default: return <Bell className="h-4 w-4" />;
    }
  };

  const handleNotificationClick = (id: string, link?: string) => {
    markAsRead(id);
    if (link) navigate(link);
  };

  return (
    <Popover>
      <PopoverTrigger asChild>
        <button className="relative rounded-xl p-2.5 hover:bg-accent transition-all hover:scale-105 active:scale-95">
          <Bell className="h-6 w-6" />
          {unreadCount > 0 && (
            <span className="absolute right-1 top-1 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-[10px] font-black text-primary-foreground ring-2 ring-background shadow-sm">
              {unreadCount}
            </span>
          )}
        </button>
      </PopoverTrigger>
      <PopoverContent align="end" className="w-80 p-0 overflow-hidden rounded-2xl border-border bg-card shadow-2xl">
        <div className="flex items-center justify-between border-b border-border p-4 bg-muted/30">
          <div className="flex items-center gap-2">
            <h3 className="text-xs font-bold uppercase tracking-widest">Notifications</h3>
            {unreadCount > 0 && (
              <span className="rounded-full bg-primary/10 px-2 py-0.5 text-[10px] font-bold text-primary uppercase">
                {unreadCount} New
              </span>
            )}
          </div>
          <Button 
            variant="ghost" 
            size="sm" 
            className="h-8 text-[9px] font-bold uppercase tracking-widest text-muted-foreground hover:text-primary"
            onClick={() => markAllAsRead()}
            disabled={isMarkingAll || unreadCount === 0}
          >
            {isMarkingAll ? <Loader2 className="h-3 w-3 animate-spin mr-1" /> : <Check className="h-3 w-3 mr-1" />}
            Clear All
          </Button>
        </div>

        <div className="max-h-[400px] overflow-auto py-1">
          {isLoading ? (
            <div className="flex flex-col items-center justify-center py-10 gap-2">
              <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
              <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-widest">Syncing Protocols...</p>
            </div>
          ) : notifications.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Bell className="h-8 w-8 text-muted/30 mb-2" />
              <p className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">No New Alerts</p>
            </div>
          ) : (
            notifications.map((n) => (
              <div 
                key={n.id}
                onClick={() => handleNotificationClick(n.id, n.link)}
                className={cn(
                  "relative flex cursor-pointer gap-4 p-4 transition-all hover:bg-muted/50 border-b border-border/30 last:border-0",
                  !n.isRead && "bg-primary/[0.02]"
                )}
              >
                {!n.isRead && (
                  <div className="absolute left-0 top-0 h-full w-0.5 bg-primary" />
                )}
                <div className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-xl bg-muted border border-border/50">
                  {getIcon(n.type)}
                </div>
                <div className="flex-1 space-y-1">
                  <div className="flex items-center justify-between gap-2">
                    <p className={cn("text-xs font-bold uppercase tracking-tight", n.isRead ? "text-foreground/70" : "text-foreground")}>
                      {n.title}
                    </p>
                    {n.link && <ExternalLink className="h-3 w-3 text-muted-foreground/40" />}
                  </div>
                  <p className="text-[11px] font-medium text-muted-foreground line-clamp-2 leading-relaxed">
                    {n.message}
                  </p>
                  <p className="text-[9px] font-bold text-muted-foreground/40 uppercase tracking-widest pt-1">
                    {new Date(n.createdAt).toLocaleDateString().toLocaleLowerCase()}
                  </p>
                </div>
              </div>
            ))
          )}
        </div>

        <div className="border-t border-border p-3 text-center bg-muted/10">
          <Button variant="ghost" className="w-full h-8 text-[9px] font-bold uppercase tracking-widest text-muted-foreground hover:text-foreground">
            View Protocol History
          </Button>
        </div>
      </PopoverContent>
    </Popover>
  );
}
