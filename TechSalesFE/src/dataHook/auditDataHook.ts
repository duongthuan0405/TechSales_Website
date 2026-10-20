import { useQuery } from '@tanstack/react-query';
import { userService } from '../services/userService';

export const useGetAuditLogs = (pageNumber = 1, pageSize = 50) => {
  return useQuery({
    queryKey: ['audit-logs', pageNumber, pageSize],
    queryFn: () => userService.getAuditLogs(pageNumber, pageSize),
  });
};
