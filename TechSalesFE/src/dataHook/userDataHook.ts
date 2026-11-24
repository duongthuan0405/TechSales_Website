import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userService, AccountPageParams } from '../services/userService';
import { User, UserRole, UserStatus } from '../models/ui_types/user';

export const useAccountPage = (kind: 'customers' | 'staff' | 'all', params: AccountPageParams) => useQuery({
  queryKey: ['users', 'page', kind, params],
  queryFn: () => userService.getAccountPage(kind, params),
});

export const useGetUsers = (role?: string, keyword?: string) => {
  return useQuery({
    queryKey: ['users', 'all', role, keyword],
    queryFn: () => userService.getUsers(role, keyword),
  });
};

export const useGetCustomers = () => {
  return useQuery({
    queryKey: ['users', 'Customer'],
    queryFn: () => userService.getCustomers(),
  });
};

export const useGetStaff = (role?: string) => {
  return useQuery({
    queryKey: ['users', 'staff-management', role],
    queryFn: () => userService.getStaff(role),
  });
};

export const useGetUser = (id: string) => {
  return useQuery({
    queryKey: ['user', id],
    queryFn: () => userService.getUserById(id),
    enabled: !!id,
  });
};

export const useCreateUser = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (userData: Omit<User, 'id' | 'createdAt'>) => userService.createUser(userData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
  });
};

export const useUpdateUser = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<User> }) => 
      userService.updateUser(id, data),
    onError: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      queryClient.invalidateQueries({ queryKey: ['user', variables.id] });
    },
    onSuccess: (updated) => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      queryClient.invalidateQueries({ queryKey: ['user', updated.id] });
    },
  });
};

export const useToggleUserStatus = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: UserStatus }) => userService.toggleUserStatus(id, status),
    onSuccess: (updated) => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      queryClient.invalidateQueries({ queryKey: ['user', updated.id] });
    },
  });
};

export const useDeactivateUser = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => userService.deactivateUser(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      queryClient.invalidateQueries({ queryKey: ['user', id] });
    },
  });
};


