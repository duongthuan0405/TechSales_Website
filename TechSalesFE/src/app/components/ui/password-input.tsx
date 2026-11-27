import * as React from 'react';
import { Eye, EyeOff } from 'lucide-react';
import { Input } from './input';
import { cn } from './utils';

export const PasswordInput = React.forwardRef<HTMLInputElement, Omit<React.ComponentProps<'input'>, 'type'>>(
  ({ className, disabled, ...props }, ref) => {
    const [visible, setVisible] = React.useState(false);
    const label = visible ? 'Ẩn mật khẩu' : 'Hiện mật khẩu';
    return <div className="relative w-full">
      <Input {...props} ref={ref} type={visible ? 'text' : 'password'} disabled={disabled} className={cn(className, 'pr-11')} />
      <button type="button" aria-label={label} title={label} aria-pressed={visible} disabled={disabled}
        onClick={() => setVisible(value => !value)}
        className="absolute inset-y-0 right-0 flex w-10 items-center justify-center rounded-r-md text-muted-foreground hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50">
        {visible ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
      </button>
    </div>;
  },
);
PasswordInput.displayName = 'PasswordInput';
