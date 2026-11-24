import { Info } from 'lucide-react';
import { Card, CardContent } from './ui/card';

export function UnavailableFeature({ title }: { title: string }) {
  return <div className="space-y-5"><h1 className="text-2xl font-bold">{title}</h1>
    <Card><CardContent className="flex items-start gap-3 py-8"><Info className="mt-0.5 h-5 w-5 shrink-0 text-primary" />
      <div><p className="font-semibold">Chức năng chưa được hỗ trợ</p><p className="mt-1 text-sm text-muted-foreground">Hệ thống chưa có API cho chức năng này.</p></div>
    </CardContent></Card>
  </div>;
}
