import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { repairService } from '../services/repairService';
import { RepairRequestStatus } from '../models/ui_types/repairRequest';

export const useGetRepairRequests = (customerId?: string) => useQuery({
  queryKey: ['repair-requests', customerId], queryFn: () => repairService.getRepairRequests(customerId),
});

export const useCreateRepairRequest = () => {
  const client = useQueryClient();
  return useMutation({ mutationFn: repairService.createRepairRequest, onSuccess: () => client.invalidateQueries({ queryKey: ['repair-requests'] }) });
};

export const useUpdateRepairRequest = () => {
  const client = useQueryClient();
  return useMutation({ mutationFn: ({ id, ...update }: { id: string; status?: RepairRequestStatus; processingLocation?: 'AT_STORE' | 'AT_MANUFACTURER'; repairType?: 'WARRANTY' | 'REPAIR'; checkingResult?: string; solutionDescription?: string; totalAmount?: number }) => repairService.updateRepairRequest(id, update), onSuccess: () => client.invalidateQueries({ queryKey: ['repair-requests'] }) });
};
