export interface PaymentMethod {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt?: string;
}
