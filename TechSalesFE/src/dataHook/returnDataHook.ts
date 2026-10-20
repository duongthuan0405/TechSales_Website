import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { returnService } from '../services/returnService';
import { CreateReturnDto, ApproveReturnParams } from '../models/ui_types/returnRequest';

export const useGetReturnRequests = (userId?: string) => {
  return useQuery({
    queryKey: ['return-requests', userId],
    queryFn: () => returnService.getReturnRequests(userId),
  });
};

export const useGetReturnRequest = (id: string) => {
  return useQuery({
    queryKey: ['return-request', id],
    queryFn: () => returnService.getReturnById(id),
    enabled: !!id,
  });
};

export const useGetReturnsByOrderId = (orderId: string) => {
  return useQuery({
    queryKey: ['returns-by-order', orderId],
    queryFn: () => returnService.getReturnsByOrderId(orderId),
    enabled: !!orderId,
  });
};

export const useCreateReturnRequest = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateReturnDto) => returnService.createReturnRequest(dto),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['return-requests'] });
      queryClient.invalidateQueries({ queryKey: ['returns-by-order', data.orderId] });
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
    },
  });
};

export const useApproveReturnRequest = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...params }: { id: string } & ApproveReturnParams) =>
      returnService.approveReturnRequest(id, params),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['return-requests'] });
      queryClient.invalidateQueries({ queryKey: ['return-request', data.id] });
      queryClient.invalidateQueries({ queryKey: ['returns-by-order', data.orderId] });
    },
  });
};

export const useRejectReturnRequest = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      returnService.rejectReturnRequest(id, reason),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['return-requests'] });
      queryClient.invalidateQueries({ queryKey: ['return-request', data.id] });
      queryClient.invalidateQueries({ queryKey: ['returns-by-order', data.orderId] });
    },
  });
};

export const useConfirmItemReceived = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, staffNote }: { id: string; staffNote?: string }) =>
      returnService.confirmItemReceived(id, staffNote),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['return-requests'] });
      queryClient.invalidateQueries({ queryKey: ['return-request', data.id] });
      queryClient.invalidateQueries({ queryKey: ['returns-by-order', data.orderId] });
    },
  });
};

export const useSubmitInspectionAndRefund = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      passed,
      notes,
      inspectedBy,
      refundMethod,
      units,
    }: {
      id: string;
      passed: boolean;
      notes: string;
      inspectedBy?: string;
      refundMethod?: string;
      units?: { serialNumber: string; isApproved: boolean; reason?: string }[];
    }) =>
      returnService.submitInspectionAndRefund(id, {
        passed,
        notes,
        inspectedBy,
        refundMethod,
        units,
      }),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['return-requests'] });
      queryClient.invalidateQueries({ queryKey: ['return-request', data.id] });
      queryClient.invalidateQueries({ queryKey: ['returns-by-order', data.orderId] });
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
    },
  });
};

export const useCustomerSubmitReturnTracking = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      trackingCode,
      carrier,
    }: {
      id: string;
      trackingCode: string;
      carrier?: string;
    }) => returnService.customerSubmitReturnTracking(id, trackingCode, carrier),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['return-requests'] });
      queryClient.invalidateQueries({ queryKey: ['return-request', data.id] });
      queryClient.invalidateQueries({ queryKey: ['returns-by-order', data.orderId] });
    },
  });
};
