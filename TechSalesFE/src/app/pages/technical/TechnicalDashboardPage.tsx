import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { StatsCard } from '../../components/analytics/StatsCard';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Badge } from '../../components/ui/badge';
import { Users, Shield, Database, Activity } from 'lucide-react';
import { userService } from '../../../services/userService';
import { User, UserStatus } from '../../../models/ui_types/user';

export function TechnicalDashboardPage() {
  const [userList, setUserList] = useState<User[]>([]);

  useEffect(() => {
    userService.getUsers().then(setUserList).catch(() => setUserList([]));
  }, []);

  const activeUsers = userList.filter(u => u.status === UserStatus.ACTIVE).length;
  const systemLogs = [
    { id: 1, timestamp: '2026-05-08 11:23:45', level: 'info', message: 'User login successful', user: 'john.smith@email.com' },
    { id: 2, timestamp: '2026-05-08 11:22:12', level: 'warning', message: 'High memory usage detected', user: 'system' },
    { id: 3, timestamp: '2026-05-08 11:20:33', level: 'error', message: 'Payment gateway timeout', user: 'payment-service' },
    { id: 4, timestamp: '2026-05-08 11:18:56', level: 'info', message: 'Database backup completed', user: 'system' },
    { id: 5, timestamp: '2026-05-08 11:15:22', level: 'info', message: 'Product catalog updated', user: 'admin@techsales.com' },
  ];

  const getLogVariant = (level: string) => {
    switch (level) {
      case 'error': return 'danger';
      case 'warning': return 'warning';
      case 'info': return 'info';
      default: return 'default';
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Technical Dashboard</h1>
        <p className="text-muted-foreground">System administration and monitoring</p>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
        <StatsCard
          title="Total Users"
          value={userList.length}
          change="+3 this week"
          changeType="positive"
          icon={Users}
        />
        <StatsCard
          title="Active Users"
          value={activeUsers}
          change="100% uptime"
          changeType="positive"
          icon={Activity}
        />
        <StatsCard
          title="Database Size"
          value="2.4 GB"
          change="+120 MB this month"
          changeType="neutral"
          icon={Database}
        />
        <StatsCard
          title="Security Events"
          value="0"
          change="No threats detected"
          changeType="positive"
          icon={Shield}
        />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>User Overview</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>Role</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {userList.map(user => (
                  <TableRow key={user.id}>
                    <TableCell>
                      <div>
                        <div className="font-medium">{user.fullName}</div>
                        <div className="text-xs text-muted-foreground">{user.email}</div>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary">{user.role}</Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant={user.status === UserStatus.ACTIVE ? 'success' : 'secondary'}>
                        {user.status}
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Recent System Logs</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {systemLogs.map(log => (
                <div key={log.id} className="flex items-start gap-3 rounded-lg border border-border p-3">
                  <Badge variant={getLogVariant(log.level)}>{log.level}</Badge>
                  <div className="flex-1">
                    <p className="text-sm">{log.message}</p>
                    <p className="text-xs text-muted-foreground mt-1">
                      {log.timestamp} • {log.user}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
