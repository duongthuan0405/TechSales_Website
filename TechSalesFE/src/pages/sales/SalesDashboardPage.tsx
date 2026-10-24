import { useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { StatsCard } from '../../components/analytics/StatsCard';
import { Badge } from '../../components/ui/badge';
import { 
  ShoppingCart, 
  Users, 
  Loader2, 
  Package, 
  Truck, 
  CheckCircle2, 
  Clock,
  MessageSquare,
  TrendingUp
} from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { useGetSalesStats } from '../../dataHook/dashboardDataHook';
import { toast } from 'sonner';
import { useNavigate } from 'react-router';

export function SalesDashboardPage() {
  const navigate = useNavigate();
  const { data: stats, isLoading, isError } = useGetSalesStats();

  useEffect(() => {
    if (isError) {
      toast.error('Failed to load operational protocols');
    }
  }, [isError]);

  const getStatusVariant = (status: string) => {
    switch (status.toUpperCase()) {
      case 'DELIVERED': return 'success';
      case 'SHIPPING': return 'info';
      case 'APPROVED': return 'warning';
      case 'PENDING': return 'pending';
      case 'CANCELLED': return 'danger';
      default: return 'default';
    }
  };

  if (isLoading || !stats) {
    return (
      <div className="flex h-full items-center justify-center py-20">
        <div className="flex flex-col items-center gap-2">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <p className="text-muted-foreground uppercase tracking-widest text-[10px] font-bold">Synchronizing Operational Data...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 pb-12">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight uppercase">Operational Dashboard</h1>
          <p className="text-sm text-muted-foreground font-bold uppercase tracking-widest opacity-60">Logistics & Service Performance</p>
        </div>
        <div className="flex items-center gap-2 rounded-full bg-blue-500/10 px-4 py-1.5 border border-blue-500/20 shadow-sm">
          <span className="relative flex h-2 w-2">
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-blue-400 opacity-75"></span>
            <span className="relative inline-flex h-2 w-2 rounded-full bg-blue-500"></span>
          </span>
          <span className="text-[10px] font-black uppercase tracking-[0.2em] text-blue-600">
            Active Session
          </span>
        </div>
      </div>

      {/* Main Stats Grid - Operational Focused */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
        <StatsCard
          title="Pending Approval"
          value={stats.pendingOrders}
          change="Action required"
          changeType={stats.pendingOrders > 5 ? "negative" : "positive"}
          icon={Package}
        />
        <StatsCard
          title="In Transit"
          value={stats.shippingOrders}
          change="Real-time tracking"
          changeType="neutral"
          icon={Truck}
        />
        <StatsCard
          title="Delivered Today"
          value={stats.deliveredOrders}
          change="+14.2% efficiency"
          changeType="positive"
          icon={CheckCircle2}
        />
        <StatsCard
          title="Avg Processing"
          value={stats.averageProcessingTime || "2.4h"}
          change="-15m from yesterday"
          changeType="positive"
          icon={Clock}
        />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Order Volume Chart instead of Revenue */}
        <Card className="lg:col-span-2 border-border shadow-sm rounded-2xl overflow-hidden bg-card">
          <CardHeader className="border-b border-border bg-muted/20">
            <CardTitle className="text-xs font-bold uppercase tracking-widest">Protocol Volume Trend</CardTitle>
          </CardHeader>
          <CardContent className="p-6">
            <ResponsiveContainer width="100%" height={320}>
              <BarChart data={stats.revenueTrend}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" vertical={false} />
                <XAxis 
                  dataKey="month" 
                  axisLine={false} 
                  tickLine={false} 
                  tick={{ fontSize: 10, fontWeight: 700 }}
                />
                <YAxis 
                  axisLine={false} 
                  tickLine={false} 
                  tick={{ fontSize: 10, fontWeight: 700 }}
                />
                <Tooltip 
                  cursor={{ fill: 'rgba(0,0,0,0.05)' }}
                  contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)' }}
                />
                <Bar 
                  dataKey="orders" 
                  fill="#000" 
                  radius={[6, 6, 0, 0]}
                  barSize={32}
                />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Quick Review Stats or Recent Orders */}
        <Card className="border-border shadow-sm rounded-2xl overflow-hidden bg-card">
          <CardHeader className="border-b border-border bg-muted/20 flex flex-row items-center justify-between">
            <CardTitle className="text-xs font-bold uppercase tracking-widest">Operational Queue</CardTitle>
            <button 
              onClick={() => navigate('/sales/orders')}
              className="text-[10px] font-bold uppercase tracking-widest text-primary hover:underline"
            >
              Management
            </button>
          </CardHeader>
          <CardContent className="p-0">
            <div className="divide-y divide-border">
              {stats.recentOrders.map(order => (
                <div key={order.id} className="flex items-center justify-between p-4 hover:bg-muted/30 transition-colors">
                  <div className="space-y-1">
                    <p className="text-xs font-bold uppercase tracking-tight">{order.id}</p>
                    <p className="text-[10px] text-muted-foreground font-medium uppercase">{order.customerName}</p>
                  </div>
                  <Badge variant={getStatusVariant(order.status)} className="text-[9px] px-2 py-0.5 font-bold uppercase tracking-wider">
                    {order.status}
                  </Badge>
                </div>
              ))}
            </div>
            {/* Added Quick Link to Reviews */}
            <div className="p-4 bg-muted/20 border-t border-border">
               <button 
                onClick={() => navigate('/sales/reviews')}
                className="w-full flex items-center justify-between bg-card p-3 rounded-xl ring-1 ring-border hover:ring-primary transition-all group"
               >
                 <div className="flex items-center gap-3">
                    <MessageSquare className="h-4 w-4 text-primary" />
                    <span className="text-[10px] font-bold uppercase tracking-widest">Review Moderation</span>
                 </div>
                 <Badge variant="pending" className="text-[9px]">4 Pending</Badge>
               </button>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Secondary Stats */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
          <Card className="border-border shadow-sm rounded-2xl bg-card p-6 flex items-center gap-5">
              <div className="h-12 w-12 rounded-2xl bg-primary/10 flex items-center justify-center text-primary">
                  <Users className="h-6 w-6" />
              </div>
              <div>
                  <p className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground">Active Customers</p>
                  <p className="text-2xl font-black">{stats.activeCustomers.toLocaleString()}</p>
              </div>
          </Card>
          <Card className="border-border shadow-sm rounded-2xl bg-card p-6 flex items-center gap-5">
              <div className="h-12 w-12 rounded-2xl bg-orange-500/10 flex items-center justify-center text-orange-500">
                  <ShoppingCart className="h-6 w-6" />
              </div>
              <div>
                  <p className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground">Total Lifecycle Orders</p>
                  <p className="text-2xl font-black">{stats.totalOrders}</p>
              </div>
          </Card>
          <Card className="border-border shadow-sm rounded-2xl bg-card p-6 flex items-center gap-5">
              <div className="h-12 w-12 rounded-2xl bg-purple-500/10 flex items-center justify-center text-purple-500">
                  <TrendingUp className="h-6 w-6" />
              </div>
              <div>
                  <p className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground">Market Reach</p>
                  <p className="text-2xl font-black">Global</p>
              </div>
          </Card>
      </div>
    </div>
  );
}
