import { CartItem } from '../models/ui_types/cart';
import { mockCart, products } from '../data/mockData';

const delay = (ms = 40) => new Promise(resolve => setTimeout(resolve, ms));

const enrichCartItems = (items: CartItem[]): CartItem[] => {
  return items.map(item => {
    const product = products.find(p => p.id === item.productId);
    return {
      ...item,
      productName: product?.name || item.productName || 'Unknown Product',
      price: product ? product.price : (item.price || 0),
      imageUrl: product?.imageUrl || item.imageUrl || '',
    };
  });
};

export const cartService = {
  getCart: async (): Promise<CartItem[]> => {
    await delay();
    return enrichCartItems(mockCart);
  },

  addToCart: async (productId: string): Promise<CartItem[]> => {
    await delay();
    const existing = mockCart.find(i => i.productId === productId);
    const product = products.find(p => p.id === productId);

    if (existing) {
      existing.quantity += 1;
      existing.updatedAt = new Date().toISOString();
    } else {
      mockCart.push({
        cartId: 'user-cart',
        productId,
        quantity: 1,
        productName: product?.name,
        price: product?.price,
        imageUrl: product?.imageUrl,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      });
    }

    return enrichCartItems(mockCart);
  },

  updateQuantity: async (productId: string, quantity: number): Promise<CartItem[]> => {
    await delay();
    if (quantity < 1) {
      return cartService.removeItem(productId);
    }

    const item = mockCart.find(i => i.productId === productId);
    if (item) {
      item.quantity = quantity;
      item.updatedAt = new Date().toISOString();
    }

    return enrichCartItems(mockCart);
  },

  removeItem: async (productId: string): Promise<CartItem[]> => {
    await delay();
    const index = mockCart.findIndex(i => i.productId === productId);
    if (index !== -1) {
      mockCart.splice(index, 1);
    }

    return enrichCartItems(mockCart);
  },

  clearCart: async (): Promise<void> => {
    await delay();
    mockCart.length = 0;
  },
};
