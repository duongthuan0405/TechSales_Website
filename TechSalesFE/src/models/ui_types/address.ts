export interface Address {
  id: string;
  province: string;
  provinceCode?: string;
  wardCode?: string;
  ward: string;
  detail: string;
  isDefault: boolean;
}
