export enum UserStatus {
  PENDING = 'PENDING',
  ACTIVE = 'ACTIVE',
  BLOCKED = 'BLOCKED'
}

export type UserRole = 'Staff' | 'Customer' | 'Business Admin' | 'Technical Admin';

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  phone?: string;
  avatarUrl?: string;
}

export interface User {
  id: string;
  email: string;
  status: UserStatus;
  createdAt: string;
  updatedAt?: string;
  
  // Joined from UserProfile
  fullName: string;
  phone: string;
  avatarUrl?: string;
  dateOfBirth?: string;
  password?: string;
  
  // Roles (simplified for UI)
  role: UserRole;
}
