import { unsupportedFeature } from './unsupportedFeature';
import { Notification } from '../models/ui_types/notification';

export const notificationService = {
  getNotifications: async (): Promise<Notification[]> => { return unsupportedFeature("thông báo"); },

  markAsRead: async (id: string): Promise<void> => { return unsupportedFeature("thông báo"); },

  markAllAsRead: async (): Promise<void> => { return unsupportedFeature("thông báo"); },
};
