import { useQuery } from '@tanstack/react-query';
import { addressService } from '../../../services/addressService';
import { Address } from '../../../models/ui_types/address';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';

export function LocationFields({ value, onChange }: {
  value: Pick<Address, 'province' | 'ward' | 'provinceCode' | 'wardCode'>;
  onChange: (patch: Partial<Address>) => void;
}) {
  const provinces = useQuery({ queryKey: ['provinces'], queryFn: addressService.getProvinces });
  const wards = useQuery({ queryKey: ['wards', value.provinceCode],
    queryFn: () => addressService.getWards(value.provinceCode!), enabled: !!value.provinceCode });
  return <div className="grid gap-4 sm:grid-cols-2">
    <label className="space-y-2 text-sm">Tỉnh / Thành phố
      <Select required
        value={value.provinceCode ?? ''} disabled={provinces.isPending || !!provinces.error}
        onValueChange={code => onChange({ provinceCode: code,
          province: provinces.data?.find(item => item.code === code)?.name ?? '', wardCode: '', ward: '' })}>
        <SelectTrigger aria-label="Tỉnh / Thành phố" className="mt-2 h-11 w-full"><SelectValue placeholder="Chọn tỉnh / thành phố" /></SelectTrigger>
        <SelectContent>{provinces.data?.map(item => <SelectItem key={item.code} value={item.code}>{item.name ?? item.code}</SelectItem>)}</SelectContent>
      </Select>
      {provinces.error && <p role="alert" className="text-destructive">{provinces.error.message}</p>}
    </label>
    <label className="space-y-2 text-sm">Phường / Xã
      <Select key={value.provinceCode || 'no-province'} required
        value={value.wardCode ?? ''} disabled={!value.provinceCode || wards.isPending || !!wards.error}
        onValueChange={code => onChange({ wardCode: code,
          ward: wards.data?.find(item => item.code === code)?.name ?? '' })}>
        <SelectTrigger aria-label="Phường / Xã" className="mt-2 h-11 w-full"><SelectValue placeholder="Chọn phường / xã" /></SelectTrigger>
        <SelectContent>{wards.data?.map(item => <SelectItem key={item.code} value={item.code}>{item.name ?? item.code}</SelectItem>)}</SelectContent>
      </Select>
      {wards.error && <p role="alert" className="text-destructive">{wards.error.message}</p>}
    </label>
  </div>;
}
