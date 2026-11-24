import { CartItem } from '../models/ui_types/cart';
import api, { apiAssetUrl } from '../api/apiClient';

interface CartResponse {
  cartId: string;
  items: { productId: string; quantity: number; createdAt: string; updatedAt: string;
    product: { name: string; price: number | null; status: string;
      images: { imageUrl: string; isPrimary: boolean }[] } | null }[];
}

export const cartService = {
  getCart: async (): Promise<CartItem[]> => {
    const cart = await api.get<CartResponse>('/api/cart');
    return cart.items.map(item => ({ cartId: cart.cartId, productId: item.productId,
      quantity: item.quantity, createdAt: item.createdAt, updatedAt: item.updatedAt,
      productName: item.product?.name ?? 'Sản phẩm không còn tồn tại',
      price: item.product?.price ?? undefined,
      unavailable: !item.product || item.product.status !== 'ACTIVE' || item.product.price === null,
      imageUrl: apiAssetUrl(item.product?.images.find(image => image.isPrimary)?.imageUrl
        ?? item.product?.images[0]?.imageUrl),
    }));
  },
  addToCart: async (productId: string, quantity = 1): Promise<CartItem[]> => {
    await api.post('/api/cart/items', { productId, quantity });
    return cartService.getCart();
  },
  updateQuantity: async (productId: string, quantity: number): Promise<CartItem[]> => {
    if (quantity < 1) return cartService.removeItem(productId);
    await api.put(`/api/cart/items/${encodeURIComponent(productId)}`, { quantity });
    return cartService.getCart();
  },
  removeItem: async (productId: string): Promise<CartItem[]> => {
    await api.delete(`/api/cart/items/${encodeURIComponent(productId)}`);
    return cartService.getCart();
  },
  clearCart: async (): Promise<void> => {
    const items = await cartService.getCart();
    // The API deletes individual items; remove only the current snapshot.
    for (const item of items) await api.delete(`/api/cart/items/${encodeURIComponent(item.productId)}`);
  },
};
