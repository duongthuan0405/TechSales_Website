import { Category } from '../models/ui_types/category';
import { categories, products } from '../data/mockData';

const delay = (ms = 40) => new Promise(resolve => setTimeout(resolve, ms));

const enrichCategories = (): Category[] => {
  return categories.map(cat => ({
    ...cat,
    productCount: products.filter(
      p => p.categoryId === cat.id || p.categoryName?.toLowerCase() === cat.name.toLowerCase()
    ).length,
  }));
};

export const categoryService = {
  getCategories: async (): Promise<Category[]> => {
    await delay();
    return enrichCategories();
  },

  createCategory: async (data: Omit<Category, 'id'>): Promise<Category> => {
    await delay();
    const newCategory: Category = {
      id: `cat${Date.now()}`,
      name: data.name,
      parentId: data.parentId,
      productCount: 0,
      createdAt: new Date().toISOString(),
    };
    categories.push(newCategory);
    return newCategory;
  },

  updateCategory: async (id: string, data: Partial<Category>): Promise<Category> => {
    await delay();
    const cat = categories.find(c => c.id === id);
    if (!cat) {
      throw new Error(`Category ${id} not found`);
    }

    if (data.name) {
      const oldName = cat.name;
      cat.name = data.name;
      // Sync with products
      products.forEach(p => {
        if (p.categoryId === id || p.categoryName === oldName) {
          p.categoryName = data.name;
        }
      });
    }

    if (data.parentId !== undefined) {
      cat.parentId = data.parentId;
    }

    return { ...cat };
  },

  deleteCategory: async (id: string, replacementId?: string): Promise<void> => {
    await delay();
    const index = categories.findIndex(c => c.id === id);
    if (index !== -1) {
      categories.splice(index, 1);
    }

    if (replacementId) {
      const repCat = categories.find(c => c.id === replacementId);
      products.forEach(p => {
        if (p.categoryId === id) {
          p.categoryId = replacementId;
          if (repCat) p.categoryName = repCat.name;
        }
      });
    }
  },
};
