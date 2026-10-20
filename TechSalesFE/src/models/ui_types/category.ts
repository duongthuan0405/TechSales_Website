export interface Category {
  id: string;
  name: string;
  parentId?: string;
  createdAt?: string;
  
  // UI extended fields
  icon?: string; // We use Lucide icon names
  productCount?: number;
  description?: string;
}
