import { useState } from 'react';
import { useNavigate } from 'react-router';
import { 
  RotateCcw, 
  Search, 
  Package, 
  ExternalLink, 
  Clock, 
  CheckCircle2, 
  XCircle, 
  Truck, 
  FileSearch, 
  Loader2,
  ChevronRight,
  Filter,
  ArrowLeft
} from 'lucide-react';
import { formatCurrency } from '../../../utils/format';
import { useAuth } from '../../context/AuthContext';
import { useGetReturnRequests } from '../../../dataHook/returnDataHook';
import { ReturnRequest, ReturnStatus } from '../../../models/ui_types/returnRequest';
import { ReturnDetailModal } from '../../components/customer/ReturnDetailModal';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Badge } from '../../components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';

export function ReturnHistoryPage() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const { data: allReturns = [], isLoading } = useGetReturnRequests(user?.id);

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('ALL');
  const [selectedReturn, setSelectedReturn] = useState<ReturnRequest | null>(null);

  const getStatusBadge = (status: ReturnStatus) => {
    switch (status) {
      case ReturnStatus.PENDING_APPROVAL:
        return (
          <Badge className="bg-amber-500/10 text-amber-500 border border-amber-500/20 font-bold uppercase tracking-wider text-[10px] flex items-center gap-1">
            <Clock className="h-3 w-3" />
            Chờ duyệt
          </Badge>
        );
      case ReturnStatus.AWAITING_RETURN:
        return (
          <Badge className="bg-blue-500/10 text-blue-500 border border-blue-500/20 font-bold uppercase tracking-wider text-[10px] flex items-center gap-1">
            <Truck className="h-3 w-3" />
            Gửi hàng về kho
          </Badge>
        );
      case ReturnStatus.INSPECTING:
        return (
          <Badge className="bg-purple-500/10 text-purple-500 border border-purple-500/20 font-bold uppercase tracking-wider text-[10px] flex items-center gap-1">
            <FileSearch className="h-3 w-3" />
            Đang kiểm tra
          </Badge>
        );
      case ReturnStatus.COMPLETED:
        return (
          <Badge className="bg-emerald-500/10 text-emerald-500 border border-emerald-500/20 font-bold uppercase tracking-wider text-[10px] flex items-center gap-1">
            <CheckCircle2 className="h-3 w-3" />
            Đã hoàn tiền
          </Badge>
        );
      case ReturnStatus.REJECTED:
        return (
          <Badge className="bg-rose-500/10 text-rose-500 border border-rose-500/20 font-bold uppercase tracking-wider text-[10px] flex items-center gap-1">
            <XCircle className="h-3 w-3" />
            Từ chối
          </Badge>
        );
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  const filteredReturns = allReturns.filter((item) => {
    const matchesStatus = statusFilter === 'ALL' || item.status === statusFilter;
    const query = searchQuery.toLowerCase().trim();
    const matchesSearch =
      !query ||
      item.id.toLowerCase().includes(query) ||
      item.orderId.toLowerCase().includes(query) ||
      item.productName.toLowerCase().includes(query) ||
      item.reason.toLowerCase().includes(query);

    return matchesStatus && matchesSearch;
  });

  const filterTabs = [
    { key: 'ALL', label: 'Tất cả', count: allReturns.length },
    { key: ReturnStatus.PENDING_APPROVAL, label: 'Chờ duyệt', count: allReturns.filter(r => r.status === ReturnStatus.PENDING_APPROVAL).length },
    { key: ReturnStatus.AWAITING_RETURN, label: 'Cần gửi hàng', count: allReturns.filter(r => r.status === ReturnStatus.AWAITING_RETURN).length },
    { key: ReturnStatus.INSPECTING, label: 'Đang kiểm tra', count: allReturns.filter(r => r.status === ReturnStatus.INSPECTING).length },
    { key: ReturnStatus.COMPLETED, label: 'Đã hoàn tiền', count: allReturns.filter(r => r.status === ReturnStatus.COMPLETED).length },
    { key: ReturnStatus.REJECTED, label: 'Bị từ chối', count: allReturns.filter(r => r.status === ReturnStatus.REJECTED).length },
  ];

  return (
    <div className="space-y-6 pb-16">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 mb-1">
            <Button 
              variant="ghost" 
              size="sm" 
              onClick={() => navigate('/customer/orders')}
              className="h-8 px-2 text-xs font-semibold text-muted-foreground hover:text-foreground -ml-2"
            >
              <ArrowLeft className="h-3.5 w-3.5 mr-1" />
              Đơn hàng
            </Button>
            <span className="text-muted-foreground/40">/</span>
            <span className="text-xs font-bold uppercase tracking-wider text-primary">Hoàn trả hàng</span>
          </div>
          <h1 className="text-2xl font-bold tracking-tight uppercase">Lịch Sử Yêu Cầu Hoàn Trả</h1>
          <p className="text-xs text-muted-foreground mt-0.5">
            Theo dõi tiến trình xét duyệt và hoàn tiền các sản phẩm đổi trả của bạn
          </p>
        </div>

        <div className="flex items-center gap-2">
          <Button 
            variant="outline" 
            onClick={() => navigate('/customer/orders')}
            className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider border-border"
          >
            <Package className="h-4 w-4 mr-1.5" />
            Xem tất cả đơn hàng
          </Button>
        </div>
      </div>

      {/* Filter Tabs */}
      <div className="flex items-center gap-2 overflow-x-auto pb-1 no-scrollbar">
        {filterTabs.map((tab) => {
          const isActive = statusFilter === tab.key;
          return (
            <button
              key={tab.key}
              onClick={() => setStatusFilter(tab.key)}
              className={`px-3.5 py-2 rounded-xl text-xs font-bold transition-all whitespace-nowrap flex items-center gap-2 border ${
                isActive
                  ? 'bg-primary text-primary-foreground border-primary shadow-sm'
                  : 'bg-card border-border/80 text-muted-foreground hover:text-foreground hover:border-border'
              }`}
            >
              <span>{tab.label}</span>
              {tab.count > 0 && (
                <span className={`text-[10px] px-1.5 py-0.5 rounded-full font-bold ${
                  isActive ? 'bg-primary-foreground/20 text-primary-foreground' : 'bg-muted text-muted-foreground'
                }`}>
                  {tab.count}
                </span>
              )}
            </button>
          );
        })}
      </div>

      {/* Search Input */}
      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Tìm theo mã yêu cầu, mã đơn, tên sản phẩm..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-9 h-11 rounded-xl bg-card border-border text-xs"
        />
      </div>

      {/* Content Table / Cards */}
      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-xs text-muted-foreground">Đang tải lịch sử hoàn trả...</p>
          </div>
        </div>
      ) : filteredReturns.length === 0 ? (
        <Card className="border-border shadow-sm rounded-2xl bg-card">
          <CardContent className="flex flex-col items-center justify-center py-16 text-center">
            <div className="h-16 w-16 rounded-2xl bg-muted/50 flex items-center justify-center mb-4 text-muted-foreground">
              <RotateCcw className="h-8 w-8" />
            </div>
            <h3 className="text-base font-bold text-foreground">Không có yêu cầu hoàn trả nào</h3>
            <p className="text-xs text-muted-foreground max-w-md mt-1 mb-6">
              {searchQuery || statusFilter !== 'ALL'
                ? 'Không tìm thấy yêu cầu hoàn trả phù hợp với bộ lọc hiện tại.'
                : 'Bạn chưa tạo yêu cầu hoàn trả nào. Với các đơn hàng đã nhận, bạn có thể gửi yêu cầu hoàn trả nếu sản phẩm có lỗi.'}
            </p>
            <Button
              onClick={() => {
                setSearchQuery('');
                setStatusFilter('ALL');
                if (allReturns.length === 0) {
                  navigate('/customer/orders');
                }
              }}
              className="h-10 px-5 rounded-xl font-bold uppercase tracking-wider text-xs bg-primary text-primary-foreground"
            >
              {allReturns.length === 0 ? 'Xem danh sách đơn hàng' : 'Xóa bộ lọc'}
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          <Card className="border-border shadow-sm rounded-2xl bg-card overflow-hidden">
            <CardHeader className="p-5 pb-3 border-b border-border/50 flex flex-row items-center justify-between">
              <div className="flex items-center gap-2">
                <RotateCcw className="h-4 w-4 text-primary" />
                <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground">
                  Danh sách yêu cầu ({filteredReturns.length})
                </CardTitle>
              </div>
            </CardHeader>
            <CardContent className="p-0">
              <Table>
                <TableHeader>
                  <TableRow className="border-border hover:bg-transparent">
                    <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground py-3.5 pl-6">Mã Yêu Cầu</TableHead>
                    <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Đơn Hàng</TableHead>
                    <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Sản Phẩm</TableHead>
                    <TableHead className="text-right font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Tiền Hoàn</TableHead>
                    <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Trạng Thái</TableHead>
                    <TableHead className="text-right font-bold uppercase tracking-widest text-[9px] text-muted-foreground pr-6">Thao Tác</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredReturns.map((item) => (
                    <TableRow key={item.id} className="border-border hover:bg-muted/20 transition-colors">
                      <TableCell className="py-4 pl-6 font-mono text-xs font-bold text-foreground">
                        #{item.id}
                        <span className="block text-[10px] font-sans font-medium text-muted-foreground mt-0.5">
                          {new Date(item.createdAt).toLocaleDateString()}
                        </span>
                      </TableCell>

                      <TableCell className="py-4">
                        <button
                          onClick={() => navigate(`/customer/orders/${item.orderId}`)}
                          className="font-mono text-xs font-semibold text-primary hover:underline flex items-center gap-1"
                        >
                          #{item.orderId}
                          <ExternalLink className="h-2.5 w-2.5" />
                        </button>
                      </TableCell>

                      <TableCell className="py-4">
                        <div className="flex items-center gap-3">
                          <img
                            src={item.productImageUrl || ''}
                            alt={item.productName}
                            className="h-10 w-10 rounded-lg object-cover border border-border bg-muted/40 shrink-0"
                          />
                          <div>
                            <p className="font-semibold text-xs text-foreground line-clamp-1">{item.productName}</p>
                            <p className="text-[11px] text-muted-foreground mt-0.5">
                              Lý do: <span className="text-foreground">{item.reason}</span> • SL: {item.quantity}
                            </p>
                          </div>
                        </div>
                      </TableCell>

                      <TableCell className="py-4 text-right">
                        <span className="text-sm font-bold text-primary">
                          {formatCurrency(item.totalRefundAmount)}
                        </span>
                        {item.refund?.method && (
                          <span className="block text-[10px] text-muted-foreground capitalize">
                            {item.refund.method}
                          </span>
                        )}
                      </TableCell>

                      <TableCell className="py-4">
                        {getStatusBadge(item.status)}
                      </TableCell>

                      <TableCell className="py-4 pr-6 text-right">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setSelectedReturn(item)}
                          className="h-8 rounded-lg px-3 text-[10px] font-bold uppercase tracking-wider border-border hover:border-primary text-primary hover:bg-primary/10 transition-all flex items-center gap-1.5 ml-auto"
                        >
                          Chi tiết
                          <ChevronRight className="h-3 w-3" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Modal chi tiết yêu cầu hoàn trả & nhập mã vận đơn */}
      <ReturnDetailModal
        isOpen={!!selectedReturn}
        onClose={() => setSelectedReturn(null)}
        returnRequest={selectedReturn}
      />
    </div>
  );
}
