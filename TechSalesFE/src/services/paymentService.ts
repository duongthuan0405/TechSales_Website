import { PaymentMethod } from '../models/ui_types/paymentMethod';
import { paymentMethods } from '../data/mockData';

const delay = (ms = 30) => new Promise(resolve => setTimeout(resolve, ms));

export const paymentService = {
  getPaymentMethods: async (): Promise<PaymentMethod[]> => {
    await delay();
    return [...paymentMethods];
  },
};
