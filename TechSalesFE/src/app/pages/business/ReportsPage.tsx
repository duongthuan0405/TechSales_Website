import { useState } from "react";
import { useNavigate } from "react-router";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "../../components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "../../components/ui/tabs";
import { 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableHeader, 
  TableRow 
} from "../../components/ui/table";
import { Badge } from "../../components/ui/badge";
import { Button } from "../../components/ui/button";
import { 
  BarChart, 
  Bar, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  AreaChart,
  Area
} from "recharts";
import { 
  FileText, 
  TrendingUp, 
  Package, 
  DollarSign, 
  Calendar,
  ArrowUpRight,
  ArrowDownRight,
  Filter,
  Eye
} from "lucide-react";
import { useGetAdminOrders } from "../../../dataHook/orderDataHook";
import { useGetReportSummary } from "../../../dataHook/dashboardDataHook";
import { OrderStatus } from "../../../models/ui_types/order";

export function ReportsPage() {
  const navigate = useNavigate();
  const { data: orders = [], isLoading: ordersLoading } = useGetAdminOrders();
  const { data: reportSummary, isLoading: reportLoading } = useGetReportSummary();
  const [dateRange, setDateRange] = useState("30d");

  // --- MAP VALUES FROM BACKEND API REPORT SUMMARY ---
  const totalRevenue = reportSummary?.totalRevenue ?? 0;
  const completedOrdersCount = reportSummary?.completedOrders ?? 0;
  const pendingRevenue = reportSummary?.pendingRevenue ?? 0;
  const topProductSharePercentage = reportSummary?.topProductSharePercentage ?? 0;
  const topProductCategoryName = reportSummary?.topProductCategoryName ?? "N/A";
  
  const revenueTrendData = reportSummary?.revenueTrend?.map(t => ({
    name: new Date(t.date).toLocaleDateString('en-US', { weekday: 'short' }),
    revenue: t.totalRevenue
  })) ?? [];

  const topProductsList = reportSummary?.topSellingProducts ?? [];

  const statusDistribution = reportSummary?.orderStatusDistribution ?? [];
  const totalOrders = statusDistribution.reduce((sum, s) => sum + s.count, 0);

  const getStatusCount = (statusStr: string) => {
    return statusDistribution.find(s => s.status.toUpperCase() === statusStr.toUpperCase())?.count ?? 0;
  };

  const getStatusVariant = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.DELIVERED: return "success";
      case OrderStatus.SHIPPING: return "info";
      case OrderStatus.APPROVED: return "warning";
      case OrderStatus.PENDING: return "pending";
      case OrderStatus.CANCELLED: return "danger";
      default: return "default";
    }
  };

  if (reportLoading || ordersLoading) {
    return (
      <div className="flex h-[400px] items-center justify-center">
        <div className="text-center space-y-2">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
          <p className="text-sm text-muted-foreground">Loading business reports...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Business Reports</h1>
          <p className="text-muted-foreground">Comprehensive insights into sales, products and operations</p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="outline" className="h-9 px-3">
            <Calendar className="mr-2 h-4 w-4" />
            Last 30 Days
          </Badge>
          <Button variant="outline" size="icon">
            <Filter className="h-4 w-4" />
          </Button>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground uppercase">Total Revenue</CardTitle>
            <DollarSign className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">${totalRevenue.toLocaleString()}</div>
            <div className="flex items-center text-xs text-green-500 mt-1">
              <ArrowUpRight className="mr-1 h-3 w-3" />
              +12.5% from last month
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground uppercase">Completed Orders</CardTitle>
            <FileText className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{completedOrdersCount}</div>
            <div className="text-xs text-muted-foreground mt-1">
              Across all categories
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground uppercase">Avg. Order Value</CardTitle>
            <TrendingUp className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              ${totalOrders > 0 ? (totalRevenue / totalOrders).toFixed(2) : "0"}
            </div>
            <div className="flex items-center text-xs text-red-500 mt-1">
              <ArrowDownRight className="mr-1 h-3 w-3" />
              -2.1% from last week
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground uppercase">Top Product Share</CardTitle>
            <Package className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{topProductSharePercentage}%</div>
            <div className="text-xs text-muted-foreground mt-1">
              From category: {topProductCategoryName}
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="orders" className="space-y-4">
        <TabsList className="bg-muted/50 p-1">
          <TabsTrigger value="orders" className="data-[state=active]:bg-background">Orders</TabsTrigger>
          <TabsTrigger value="products" className="data-[state=active]:bg-background">Top Selling</TabsTrigger>
          <TabsTrigger value="revenue" className="data-[state=active]:bg-background">Revenue</TabsTrigger>
        </TabsList>

        {/* --- ORDERS TAB --- */}
        <TabsContent value="orders" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-7">
            <Card className="lg:col-span-4">
              <CardHeader>
                <CardTitle>Order Status Distribution</CardTitle>
                <CardDescription>Breakdown of all orders by their current fulfillment state.</CardDescription>
              </CardHeader>
              <CardContent className="h-[300px]">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={[
                    { name: 'Pending', count: getStatusCount('PENDING') },
                    { name: 'Confirmed', count: getStatusCount('APPROVED') },
                    { name: 'Shipping', count: getStatusCount('SHIPPING') },
                    { name: 'Delivered', count: getStatusCount('DELIVERED') },
                    { name: 'Cancelled', count: getStatusCount('CANCELLED') },
                  ]}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} />
                    <XAxis dataKey="name" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="count" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
            <Card className="lg:col-span-3">
              <CardHeader>
                <CardTitle>Recent Orders Overview</CardTitle>
                <CardDescription>A summary of order health and volume.</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-8">
                  <div className="flex items-center">
                    <div className="ml-4 space-y-1">
                      <p className="text-sm font-medium leading-none">Total Orders</p>
                      <p className="text-sm text-muted-foreground">{totalOrders} total orders processed</p>
                    </div>
                    <div className="ml-auto font-bold">{totalOrders}</div>
                  </div>
                  <div className="flex items-center">
                    <div className="ml-4 space-y-1">
                      <p className="text-sm font-medium leading-none">Delivered Rate</p>
                      <p className="text-sm text-muted-foreground">Percentage of successful deliveries</p>
                    </div>
                    <div className="ml-auto font-bold">
                      {totalOrders > 0 ? ((getStatusCount('DELIVERED') / totalOrders) * 100).toFixed(0) : 0}%
                    </div>
                  </div>
                  <div className="flex items-center">
                    <div className="ml-4 space-y-1">
                      <p className="text-sm font-medium leading-none">Cancellation Rate</p>
                      <p className="text-sm text-muted-foreground">Orders rejected or cancelled</p>
                    </div>
                    <div className="ml-auto font-bold text-destructive">
                      {totalOrders > 0 ? ((getStatusCount('CANCELLED') / totalOrders) * 100).toFixed(0) : 0}%
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader className="pb-3">
              <CardTitle>Order List</CardTitle>
              <CardDescription>Browse all orders. Click the eye icon to view full details (Read-only).</CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Order ID</TableHead>
                    <TableHead>Customer</TableHead>
                    <TableHead>Date</TableHead>
                    <TableHead>Total</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {orders.slice(0, 10).map((order) => (
                    <TableRow key={order.id} className="cursor-pointer hover:bg-muted/50 transition-colors" onClick={() => navigate(`/business/orders/${order.id}`)}>
                      <TableCell className="font-mono text-xs uppercase">{order.id}</TableCell>
                      <TableCell className="font-medium">{order.customerName}</TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {new Date(order.createdAt).toLocaleDateString()}
                      </TableCell>
                      <TableCell className="font-bold">${order.totalAmount.toLocaleString()}</TableCell>
                      <TableCell>
                        <Badge 
                          variant={getStatusVariant(order.status) as any}
                          className="font-bold uppercase tracking-widest text-[10px] px-2 py-0.5 rounded-sm"
                        >
                          {order.status}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right" onClick={(e) => e.stopPropagation()}>
                        <Button variant="ghost" size="icon" onClick={() => navigate(`/business/orders/${order.id}`)}>
                          <Eye className="h-4 w-4 text-primary" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                  {orders.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={6} className="h-24 text-center text-muted-foreground">No orders found</TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        {/* --- PRODUCTS TAB --- */}
        <TabsContent value="products" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Top Selling Products</CardTitle>
              <CardDescription>Products with the highest sales volume and revenue generation.</CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Product Name</TableHead>
                    <TableHead className="text-center">Units Sold</TableHead>
                    <TableHead className="text-right">Total Revenue</TableHead>
                    <TableHead className="text-right">Performance</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {topProductsList.map((p) => (
                    <TableRow key={p.productId}>
                      <TableCell className="font-medium">{p.name}</TableCell>
                      <TableCell className="text-center">{p.quantity}</TableCell>
                      <TableCell className="text-right">${p.revenue.toLocaleString()}</TableCell>
                      <TableCell className="text-right">
                        <Badge variant="success" className="gap-1">
                          <TrendingUp className="h-3 w-3" /> High
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                  {topProductsList.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={4} className="h-24 text-center">No sales data available</TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        {/* --- REVENUE TAB --- */}
        <TabsContent value="revenue" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Revenue Trends</CardTitle>
              <CardDescription>Daily revenue fluctuations over the selected period.</CardDescription>
            </CardHeader>
            <CardContent className="h-[400px] pt-4">
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={revenueTrendData}>
                  <defs>
                    <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="hsl(var(--primary))" stopOpacity={0.3}/>
                      <stop offset="95%" stopColor="hsl(var(--primary))" stopOpacity={0}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="hsl(var(--muted-foreground)/0.2)" />
                  <XAxis dataKey="name" stroke="hsl(var(--muted-foreground))" fontSize={12} tickLine={false} axisLine={false} />
                  <YAxis stroke="hsl(var(--muted-foreground))" fontSize={12} tickLine={false} axisLine={false} tickFormatter={(value) => `$${value}`} />
                  <Tooltip 
                    contentStyle={{ backgroundColor: 'hsl(var(--background))', borderColor: 'hsl(var(--border))' }}
                    itemStyle={{ color: 'hsl(var(--primary))' }}
                  />
                  <Area 
                    type="monotone" 
                    dataKey="revenue" 
                    stroke="hsl(var(--primary))" 
                    strokeWidth={2}
                    fillOpacity={1} 
                    fill="url(#colorRevenue)" 
                  />
                </AreaChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
