import { useQuery } from '@tanstack/react-query';
import { addressService } from '../../../services/addressService';
import { Address } from '../../../models/ui_types/address';

export function LocationFields({ value, onChange }: {
  value: Pick<Address, 'province' | 'ward' | 'provinceCode' | 'wardCode'>;
  onChange: (patch: Partial<Address>) => void;
}) {
  const provinces = useQuery({ queryKey: ['provinces'], queryFn: addressService.getProvinces });
  const wards = useQuery({ queryKey: ['wards', value.provinceCode],
    queryFn: () => addressService.getWards(value.provinceCode!), enabled: !!value.provinceCode });
  return <div className="grid gap-4 sm:grid-cols-2">
    <label className="space-y-2 text-sm">Tỉnh / Thành phố
      <select aria-label="Tỉnh / Thành phố" required className="mt-2 h-11 w-full rounded border bg-background px-3"
        value={value.provinceCode ?? ''} disabled={provinces.isPending || !!provinces.error}
        onChange={event => onChange({ provinceCode: event.target.value,
          province: provinces.data?.find(item => item.code === event.target.value)?.name ?? '', wardCode: '', ward: '' })}>
        <option value="">Chọn tỉnh / thành phố</option>
        {provinces.data?.map(item => <option key={item.code} value={item.code}>{item.name ?? item.code}</option>)}
      </select>
      {provinces.error && <p role="alert" className="text-destructive">{provinces.error.message}</p>}
    </label>
    <label className="space-y-2 text-sm">Phường / Xã
      <select aria-label="Phường / Xã" required className="mt-2 h-11 w-full rounded border bg-background px-3"
        value={value.wardCode ?? ''} disabled={!value.provinceCode || wards.isPending || !!wards.error}
        onChange={event => onChange({ wardCode: event.target.value,
          ward: wards.data?.find(item => item.code === event.target.value)?.name ?? '' })}>
        <option value="">Chọn phường / xã</option>
        {wards.data?.map(item => <option key={item.code} value={item.code}>{item.name ?? item.code}</option>)}
      </select>
      {wards.error && <p role="alert" className="text-destructive">{wards.error.message}</p>}
    </label>
  </div>;
}
