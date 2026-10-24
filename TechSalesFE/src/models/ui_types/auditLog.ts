export interface AuditLog {
  id: string;
  createdAt: string;
  userId?: string;
  action: string;
  tableName: string;
  primaryKey: string;
  oldValues?: string;
  newValues?: string;
  affectedColumns?: string;
}
