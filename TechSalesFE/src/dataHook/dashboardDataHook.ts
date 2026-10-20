import { useQuery } from '@tanstack/react-query';
import { dashboardService } from '../services/dashboardService';

export const useGetSalesStats = () => {
  return useQuery({
    queryKey: ['salesStats'],
    queryFn: () => dashboardService.getSalesStats(),
  });
};

export const useGetReportSummary = () => {
  return useQuery({
    queryKey: ['reportSummary'],
    queryFn: () => dashboardService.getReportSummary(),
  });
};
