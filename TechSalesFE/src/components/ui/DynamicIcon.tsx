import * as LucideIcons from 'lucide-react';
import { LucideProps } from 'lucide-react';

interface DynamicIconProps extends LucideProps {
  name: string;
}

export const DynamicIcon = ({ name, ...props }: DynamicIconProps) => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const IconComponent = (LucideIcons as any)[name];

  if (!IconComponent) {
    // Fallback icon if the name doesn't match any Lucide icon
    return <LucideIcons.HelpCircle {...props} />;
  }

  return <IconComponent {...props} />;
};
