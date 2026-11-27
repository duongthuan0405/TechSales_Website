import { Users, Activity } from 'lucide-react';
import { useGetUsers } from '../../../dataHook/userDataHook';
import { UserStatus } from '../../../models/ui_types/user';
import { StatsCard } from '../../components/analytics/StatsCard';
import { UnavailableFeature } from '../../components/UnavailableFeature';

export function TechnicalDashboardPage() {
  const { data: users, isLoading, error } = useGetUsers();
  return <div className="space-y-6">
    <h1 className="text-3xl font-bold">Tổng quan hệ thống</h1>
    {isLoading ? <p>Đang tải tài khoản...</p> : error ? <p role="alert">{error.message}</p> : users && <div className="grid gap-6 md:grid-cols-2">
      <StatsCard title="Tổng số tài khoản" value={users.length} icon={Users} />
      <StatsCard title="Tài khoản hoạt động" value={users.filter(user => user.status === UserStatus.ACTIVE).length} icon={Activity} />
    </div>}
    <UnavailableFeature title="Giám sát và nhật ký hệ thống" />
  </div>;
}
