import { Notification } from '../models/ui_types/notification';
import { mockNotifications } from '../data/mockData';

const delay = (ms = 30) => new Promise(resolve => setTimeout(resolve, ms));

export const notificationService = {
  getNotifications: async (): Promise<Notification[]> => {
    await delay();
    return [...mockNotifications];
  },

  markAsRead: async (id: string): Promise<void> => {
    await delay();
    const notification = mockNotifications.find(n => n.id === id);
    if (notification) {
      notification.isRead = true;
    }
  },

  markAllAsRead: async (): Promise<void> => {
    await delay();
    mockNotifications.forEach(n => {
      n.isRead = true;
    });
  },
};
