import { useState } from 'react';
import { 
  RotateCcw, 
  Search, 
  Clock, 
  Truck, 
  FileSearch, 
  CheckCircle2, 
  XCircle, 
  AlertTriangle,
  Eye, 
  ExternalLink, 
  MapPin, 
  CreditCard, 
  Package, 
  Camera, 
  Check, 
  X, 
  Loader2,
  Filter,
  ArrowRight
} from 'lucide-react';
import { 
  useGetReturnRequests, 
  useApproveReturnRequest, 
  useRejectReturnRequest, 
  useConfirmItemReceived, 
  useSubmitInspectionAndRefund 
} from '../../../dataHook/returnDataHook';
import { ReturnRequest, ReturnStatus } from '../../../models/ui_types/returnRequest';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Textarea } from '../../components/ui/textarea';
import { 
  Dialog, 
  DialogContent, 
  DialogHeader, 
  DialogTitle, 
  DialogDescription, 
  DialogFooter 
} from '../../components/ui/dialog';
import { 
  AlertDialog, 
  AlertDialogAction, 
  AlertDialogCancel, 
  AlertDialogContent, 
  AlertDialogDescription, 
  AlertDialogFooter, 
  AlertDialogHeader, 
  AlertDialogTitle 
} from '../../components/ui/alert-dialog';
import { toast } from 'sonner';

export function ReturnManagementPage() {
  const { data: returns = [], isLoading } = useGetReturnRequests();
  const { mutate: approveRequest, isPending: isApproving } = useApproveReturnRequest();
  const { mutate: rejectRequest, isPending: isRejecting } = useRejectReturnRequest();
  const { mutate: confirmReceived, isPending: isConfirmingReceived } = useConfirmItemReceived();
  const { mutate: submitInspection, isPending: isSubmittingInspection } = useSubmitInspectionAndRefund();

  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('ALL');

  // Selected Return for Full Processing
  const [selectedReturn, setSelectedReturn] = useState<ReturnRequest | null>(null);

  // Modals inside processing
  const [isApprovingModalOpen, setIsApprovingModalOpen] = useState(false);
  const [approveNote, setApproveNote] = useState('');
  const [approvalUnits, setApprovalUnits] = useState<{ serialNumber: string; isApproved: boolean; reason: string }[]>([]);
  const [inspectionUnits, setInspectionUnits] = useState<{ serialNumber: string; isApproved: boolean; reason: string }[]>([]);

  const [isRejectingModalOpen, setIsRejectingModalOpen] = useState(false);
  const [rejectionReason, setRejectionReason] = useState('');

  const [isConfirmArrivalOpen, setIsConfirmArrivalOpen] = useState(false);
  const [arrivalNote, setArrivalNote] = useState('');

  // Self-Inspection form state
  const [isInspectionModalOpen, setIsInspectionModalOpen] = useState(false);
  const [inspectionPassed, setInspectionPassed] = useState(true);
  const [inspectionNote, setInspectionNote] = useState('');
  const [inspectedBy, setInspectedBy] = useState('KTV Nguyễn Văn Toàn');
  const [checkAppearance, setCheckAppearance] = useState(true);
  const [checkAccessories, setCheckAccessories] = useState(true);
  const [checkSerial, setCheckSerial] = useState(true);
  const [checkDefect, setCheckDefect] = useState(true);

  // Lightbox for photos
  const [zoomImage, setZoomImage] = useState<string | null>(null);

  // Filtered Returns
  const filteredReturns = returns.filter((item) => {
    const matchesSearch = 
      item.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
      item.orderId.toLowerCase().includes(searchTerm.toLowerCase()) ||
      item.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      item.productName.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesStatus = statusFilter === 'ALL' || item.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  // Metrics
  const totalCount = returns.length;
  const pendingCount = returns.filter(r => r.status === ReturnStatus.PENDING_APPROVAL).length;
  const awaitingCount = returns.filter(r => r.status === ReturnStatus.AWAITING_RETURN).length;
  const inspectingCount = returns.filter(r => r.status === ReturnStatus.INSPECTING).length;
  const completedCount = returns.filter(r => r.status === ReturnStatus.COMPLETED).length;

  const getStatusBadge = (status: ReturnStatus) => {
    switch (status) {
      case ReturnStatus.PENDING_APPROVAL:
        return <Badge className="bg-amber-500/10 text-amber-500 border border-amber-500/20 text-[9px] font-bold uppercase tracking-wider">Chờ xét duyệt</Badge>;
      case ReturnStatus.AWAITING_RETURN:
        return <Badge className="bg-blue-500/10 text-blue-500 border border-blue-500/20 text-[9px] font-bold uppercase tracking-wider">Chờ gửi hàng về</Badge>;
      case ReturnStatus.INSPECTING:
        return <Badge className="bg-purple-500/10 text-purple-500 border border-purple-500/20 text-[9px] font-bold uppercase tracking-wider">Đang tự kiểm tra</Badge>;
      case ReturnStatus.COMPLETED:
        return <Badge className="bg-emerald-500/10 text-emerald-500 border border-emerald-500/20 text-[9px] font-bold uppercase tracking-wider">Đã hoàn tiền</Badge>;
      case ReturnStatus.REJECTED:
        return <Badge className="bg-rose-500/10 text-rose-500 border border-rose-500/20 text-[9px] font-bold uppercase tracking-wider">Từ chối hoàn trả</Badge>;
      default:
        return null;
    }
  };

  // Step 1: Staff Approves Request
  const handleApprove = () => {
    if (!selectedReturn) return;
    approveRequest(
      {
        id: selectedReturn.id,
        commonReason: approveNote,
        units: approvalUnits,
      },
      {
        onSuccess: (updated) => {
          toast.success(`Đã duyệt yêu cầu #${updated.id}. Hệ thống đã gửi yêu cầu khách hàng gửi hàng về kho.`);
          setSelectedReturn(updated);
          setIsApprovingModalOpen(false);
        },
        onError: (err: any) => {
          toast.error(err.message || 'Duyệt yêu cầu thất bại');
        }
      }
    );
  };

  // Staff Rejects Request
  const handleReject = () => {
    if (!selectedReturn) return;
    if (!rejectionReason.trim()) {
      toast.error('Vui lòng nhập lý do từ chối');
      return;
    }
    rejectRequest(
      { id: selectedReturn.id, reason: rejectionReason.trim() },
      {
        onSuccess: (updated) => {
          toast.success(`Đã từ chối yêu cầu #${updated.id}`);
          setSelectedReturn(updated);
          setIsRejectingModalOpen(false);
        },
        onError: (err: any) => {
          toast.error(err.message || 'Từ chối yêu cầu thất bại');
        }
      }
    );
  };

  // Step 2: Product Arrives At Warehouse -> Staff Confirms Arrival
  const handleConfirmArrival = () => {
    if (!selectedReturn) return;
    confirmReceived(
      {
        id: selectedReturn.id,
        staffNote: arrivalNote || 'Kiện hàng đã về tới kho TechSales. Nhân viên đang tự kiểm tra sản phẩm.'
      },
      {
        onSuccess: (updated) => {
          toast.success(`Đã xác nhận nhận hàng #${updated.id}. Trạng thái chuyển sang Tự kiểm tra.`);
          setSelectedReturn(updated);
          setInspectionUnits([]);
          setInspectionPassed(true);
          setInspectionNote('');
          setIsInspectionModalOpen(true);
          setIsConfirmArrivalOpen(false);
        },
        onError: (err: any) => {
          toast.error(err.message || 'Xác nhận nhận hàng thất bại');
        }
      }
    );
  };

  // Step 3 & 4: Self Inspection & Refund Approval
  const handleSubmitInspection = () => {
    if (!selectedReturn) return;
    if (!inspectionNote.trim()) {
      toast.error('Vui lòng nhập ghi chú biên bản kiểm tra');
      return;
    }

    submitInspection(
      {
        id: selectedReturn.id,
        passed: inspectionPassed,
        notes: inspectionNote.trim(),
        inspectedBy: inspectedBy.trim() || 'KTV TechSales',
        units: inspectionUnits,
      },
      {
        onSuccess: (updated) => {
          if (inspectionPassed) {
            toast.success(`Duyệt hàng đạt chuẩn! Đã hoàn tiền $${updated.totalRefundAmount.toLocaleString()} thành công.`);
          } else {
            toast.info(`Sản phẩm không đạt chuẩn. Đã từ chối hoàn tiền.`);
          }
          setSelectedReturn(updated);
          setIsInspectionModalOpen(false);
        },
        onError: (err: any) => {
          toast.error(err.message || 'Xử lý biên bản kiểm tra thất bại');
        }
      }
    );
  };

  return (
    <div className="space-y-6 pb-16">
      {/* Page Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight uppercase">Quản Lý Hoàn Trả Hàng</h1>
        <p className="text-sm text-muted-foreground font-bold uppercase tracking-widest opacity-60">
          Return Protocol & Inspection Terminal
        </p>
      </div>

      {/* Metrics Cards */}
      <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
        <Card 
          className={`border-border shadow-sm rounded-xl cursor-pointer transition-all ${statusFilter === 'ALL' ? 'ring-2 ring-primary bg-primary/[0.03]' : 'bg-card hover:bg-muted/30'}`}
          onClick={() => setStatusFilter('ALL')}
        >
          <CardContent className="p-4 flex items-center justify-between">
            <div>
              <p className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Tất cả</p>
              <h3 className="text-2xl font-black text-foreground mt-0.5">{totalCount}</h3>
            </div>
            <RotateCcw className="h-6 w-6 text-muted-foreground/40" />
          </CardContent>
        </Card>

        <Card 
          className={`border-border shadow-sm rounded-xl cursor-pointer transition-all ${statusFilter === ReturnStatus.PENDING_APPROVAL ? 'ring-2 ring-amber-500 bg-amber-500/5' : 'bg-card hover:bg-muted/30'}`}
          onClick={() => setStatusFilter(ReturnStatus.PENDING_APPROVAL)}
        >
          <CardContent className="p-4 flex items-center justify-between">
            <div>
              <p className="text-[10px] font-bold uppercase tracking-wider text-amber-500">Chờ xét duyệt</p>
              <h3 className="text-2xl font-black text-amber-600 mt-0.5">{pendingCount}</h3>
            </div>
            <Clock className="h-6 w-6 text-amber-500/40" />
          </CardContent>
        </Card>

        <Card 
          className={`border-border shadow-sm rounded-xl cursor-pointer transition-all ${statusFilter === ReturnStatus.AWAITING_RETURN ? 'ring-2 ring-blue-500 bg-blue-500/5' : 'bg-card hover:bg-muted/30'}`}
          onClick={() => setStatusFilter(ReturnStatus.AWAITING_RETURN)}
        >
          <CardContent className="p-4 flex items-center justify-between">
            <div>
              <p className="text-[10px] font-bold uppercase tracking-wider text-blue-500">Chờ gửi về</p>
              <h3 className="text-2xl font-black text-blue-600 mt-0.5">{awaitingCount}</h3>
            </div>
            <Truck className="h-6 w-6 text-blue-500/40" />
          </CardContent>
        </Card>

        <Card 
          className={`border-border shadow-sm rounded-xl cursor-pointer transition-all ${statusFilter === ReturnStatus.INSPECTING ? 'ring-2 ring-purple-500 bg-purple-500/5' : 'bg-card hover:bg-muted/30'}`}
          onClick={() => setStatusFilter(ReturnStatus.INSPECTING)}
        >
          <CardContent className="p-4 flex items-center justify-between">
            <div>
              <p className="text-[10px] font-bold uppercase tracking-wider text-purple-500">Đang kiểm tra</p>
              <h3 className="text-2xl font-black text-purple-600 mt-0.5">{inspectingCount}</h3>
            </div>
            <FileSearch className="h-6 w-6 text-purple-500/40" />
          </CardContent>
        </Card>

        <Card 
          className={`border-border shadow-sm rounded-xl cursor-pointer transition-all ${statusFilter === ReturnStatus.COMPLETED ? 'ring-2 ring-emerald-500 bg-emerald-500/5' : 'bg-card hover:bg-muted/30'}`}
          onClick={() => setStatusFilter(ReturnStatus.COMPLETED)}
        >
          <CardContent className="p-4 flex items-center justify-between">
            <div>
              <p className="text-[10px] font-bold uppercase tracking-wider text-emerald-500">Đã hoàn tiền</p>
              <h3 className="text-2xl font-black text-emerald-600 mt-0.5">{completedCount}</h3>
            </div>
            <CheckCircle2 className="h-6 w-6 text-emerald-500/40" />
          </CardContent>
        </Card>
      </div>

      {/* Search & Tabs */}
      <div className="flex flex-col sm:flex-row gap-3 items-center justify-between">
        <div className="relative w-full sm:w-96">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Tìm theo Mã yêu cầu, Đơn hàng, Tên khách..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-9 h-11 rounded-xl bg-card border-border"
          />
        </div>

        <div className="flex items-center gap-1.5 overflow-x-auto w-full sm:w-auto pb-1 sm:pb-0">
          {[
            { id: 'ALL', label: 'Tất cả' },
            { id: ReturnStatus.PENDING_APPROVAL, label: 'Chờ duyệt' },
            { id: ReturnStatus.AWAITING_RETURN, label: 'Chờ gửi về' },
            { id: ReturnStatus.INSPECTING, label: 'Đang kiểm tra' },
            { id: ReturnStatus.COMPLETED, label: 'Đã hoàn tiền' },
            { id: ReturnStatus.REJECTED, label: 'Đã từ chối' },
          ].map((tab) => (
            <Button
              key={tab.id}
              variant={statusFilter === tab.id ? 'default' : 'outline'}
              size="sm"
              onClick={() => setStatusFilter(tab.id)}
              className="rounded-xl h-9 text-[10px] font-bold uppercase tracking-wider shrink-0"
            >
              {tab.label}
            </Button>
          ))}
        </div>
      </div>

      {/* Main Table */}
      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-muted-foreground uppercase tracking-widest text-[10px] font-bold">
              Đang đồng bộ dữ liệu hoàn trả...
            </p>
          </div>
        </div>
      ) : (
        <Card className="border-border shadow-sm rounded-2xl overflow-hidden bg-card">
          <CardHeader className="border-b border-border bg-muted/20 py-4 px-6 flex flex-row items-center justify-between">
            <CardTitle className="text-xs font-bold uppercase tracking-widest">
              Danh Sách Yêu Cầu Hoàn Trả ({filteredReturns.length})
            </CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow className="border-border hover:bg-transparent">
                  <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Mã YC / Đơn</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Khách hàng</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Sản phẩm</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground">Lý do & Minh chứng</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground text-right">Tiền hoàn</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground text-center">Trạng thái</TableHead>
                  <TableHead className="font-bold uppercase tracking-widest text-[9px] text-muted-foreground text-right">Thao tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredReturns.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center py-12 text-muted-foreground text-xs">
                      Không tìm thấy yêu cầu hoàn trả nào phù hợp.
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredReturns.map((ret) => (
                    <TableRow 
                      key={ret.id} 
                      className="border-border hover:bg-muted/40 transition-colors cursor-pointer"
                      onClick={() => setSelectedReturn(ret)}
                    >
                      <TableCell className="py-4">
                        <div className="flex flex-col">
                          <span className="font-mono font-bold text-xs uppercase text-primary">#{ret.id}</span>
                          <span className="text-[10px] text-muted-foreground font-mono">Đơn: #{ret.orderId}</span>
                        </div>
                      </TableCell>

                      <TableCell className="py-4">
                        <div className="flex flex-col">
                          <span className="font-bold text-xs text-foreground">{ret.customerName}</span>
                          <span className="text-[9px] text-muted-foreground">{ret.customerPhone}</span>
                        </div>
                      </TableCell>

                      <TableCell className="py-4">
                        <div className="flex items-center gap-2.5">
                          <img
                            src={ret.productImageUrl || ''}
                            alt={ret.productName}
                            className="h-9 w-9 rounded-lg object-cover border border-border bg-card shrink-0"
                          />
                          <div className="flex flex-col max-w-[180px]">
                            <span className="font-bold text-xs text-foreground truncate">{ret.productName}</span>
                            <span className="text-[9px] text-muted-foreground">SL: {ret.quantity} • ${ret.price.toLocaleString()}/sp</span>
                          </div>
                        </div>
                      </TableCell>

                      <TableCell className="py-4">
                        <div className="flex flex-col gap-1 max-w-[240px]">
                          <span className="text-xs font-semibold text-foreground truncate">{ret.reason}</span>
                          <div className="flex items-center gap-1.5">
                            {ret.proofImages?.slice(0, 3).map((img, i) => (
                              <img
                                key={i}
                                src={img}
                                alt={`Proof ${i}`}
                                className="h-6 w-6 rounded-md object-cover border border-border hover:scale-125 transition-transform"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  setZoomImage(img);
                                }}
                              />
                            ))}
                            {ret.proofImages?.length > 3 && (
                              <span className="text-[8px] font-bold text-muted-foreground">+{ret.proofImages.length - 3}</span>
                            )}
                          </div>
                        </div>
                      </TableCell>

                      <TableCell className="text-right font-black text-sm text-primary py-4">
                        ${ret.totalRefundAmount.toLocaleString()}
                      </TableCell>

                      <TableCell className="text-center py-4">
                        {getStatusBadge(ret.status)}
                      </TableCell>

                      <TableCell className="text-right py-4" onClick={(e) => e.stopPropagation()}>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setSelectedReturn(ret)}
                          className="h-8 rounded-lg px-3 text-[10px] font-bold uppercase tracking-wider border-border hover:bg-primary hover:text-primary-foreground transition-all"
                        >
                          Xử lý
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Detailed Action Modal */}
      {selectedReturn && (
        <Dialog open={!!selectedReturn} onOpenChange={(open) => !open && setSelectedReturn(null)}>
          <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto rounded-2xl p-6 bg-card border-border shadow-2xl">
            <DialogHeader className="border-b border-border pb-4 space-y-1">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="p-2 rounded-xl bg-primary/10 text-primary">
                    <RotateCcw className="h-5 w-5" />
                  </div>
                  <div>
                    <DialogTitle className="text-xl font-bold tracking-tight uppercase">
                      Xử Lý Yêu Cầu Hoàn Trả #{selectedReturn.id}
                    </DialogTitle>
                    <DialogDescription className="text-xs text-muted-foreground">
                      Đơn hàng #{selectedReturn.orderId} • Khách hàng: {selectedReturn.customerName} ({selectedReturn.customerPhone})
                    </DialogDescription>
                  </div>
                </div>
                {getStatusBadge(selectedReturn.status)}
              </div>
            </DialogHeader>

            <div className="space-y-6 pt-3">
              {/* Product Info Card */}
              <div className="flex items-center gap-4 p-4 rounded-xl border border-border bg-muted/20">
                <img
                  src={selectedReturn.productImageUrl || ''}
                  alt={selectedReturn.productName}
                  className="h-16 w-16 rounded-xl object-cover border border-border bg-card shrink-0"
                />
                <div className="flex-1 min-w-0">
                  <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Sản phẩm yêu cầu trả</span>
                  <h4 className="font-bold text-sm text-foreground">{selectedReturn.productName}</h4>
                  <div className="flex items-center gap-3 mt-1 text-xs">
                    <span className="text-muted-foreground">Số lượng trả: <strong className="text-foreground">{selectedReturn.quantity}</strong></span>
                    <span className="text-muted-foreground">• Đơn giá: <strong className="text-foreground">${selectedReturn.price.toLocaleString()}</strong></span>
                    <span className="text-muted-foreground">• Tiền hoàn trả: <strong className="text-primary font-bold text-sm">${selectedReturn.totalRefundAmount.toLocaleString()}</strong></span>
                  </div>
                </div>
              </div>

              {/* Customer Reason & Evidence Photos */}
              <div className="space-y-3">
                <div className="p-4 rounded-xl border border-border bg-card space-y-2">
                  <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block">
                    Lý do báo cáo từ khách hàng
                  </span>
                  <p className="text-xs font-bold text-foreground">{selectedReturn.reason}</p>
                  <p className="text-xs text-muted-foreground italic leading-relaxed">
                    "{selectedReturn.description}"
                  </p>
                </div>

                {/* Evidence Photo Gallery */}
                {selectedReturn.proofImages?.length > 0 && (
                  <div className="space-y-2">
                    <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground flex items-center gap-1.5">
                      <Camera className="h-3.5 w-3.5 text-primary" />
                      Ảnh chụp minh chứng từ khách hàng ({selectedReturn.proofImages.length} ảnh - Click để phóng to):
                    </span>
                    <div className="grid grid-cols-3 sm:grid-cols-4 gap-3">
                      {selectedReturn.proofImages.map((img, idx) => (
                        <div
                          key={idx}
                          onClick={() => setZoomImage(img)}
                          className="relative aspect-square rounded-xl overflow-hidden border border-border bg-muted cursor-pointer hover:opacity-85 transition-opacity group"
                        >
                          <img src={img} alt={`Proof ${idx}`} className="w-full h-full object-cover" />
                          <div className="absolute inset-0 bg-black/30 opacity-0 group-hover:opacity-100 flex items-center justify-center transition-opacity">
                            <Eye className="h-5 w-5 text-white" />
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>

              {/* Customer Return Shipping Details (if provided) */}
              {(selectedReturn.returnTrackingCode || selectedReturn.status === ReturnStatus.AWAITING_RETURN) && (
                <div className="p-4 rounded-xl border border-blue-500/20 bg-blue-500/5 space-y-2">
                  <div className="flex items-center gap-2 text-blue-600 dark:text-blue-400 font-bold text-xs uppercase tracking-wider">
                    <Truck className="h-4 w-4" />
                    <span>Thông tin vận chuyển hàng về kho</span>
                  </div>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs pt-1">
                    <div>
                      <span className="text-muted-foreground text-[10px] block">Mã vận đơn gửi hàng của khách</span>
                      <strong className="text-foreground font-mono">
                        {selectedReturn.returnTrackingCode ? `${selectedReturn.returnTrackingCode} (${selectedReturn.returnCarrier || 'Vận chuyển'})` : 'Khách chưa cập nhật mã vận đơn'}
                      </strong>
                    </div>
                    <div>
                      <span className="text-muted-foreground text-[10px] block">Kho tiếp nhận</span>
                      <span className="text-foreground">{selectedReturn.returnAddress || 'Kho TechSales: 123 Tech Avenue, Q.1, TP.HCM'}</span>
                    </div>
                  </div>
                </div>
              )}

              {/* Quality Inspection Result (if inspected) */}
              {selectedReturn.inspection && (
                <div className={`p-4 rounded-xl border space-y-2 ${selectedReturn.inspection.passed ? 'border-emerald-500/20 bg-emerald-500/5' : 'border-rose-500/20 bg-rose-500/5'}`}>
                  <div className="flex items-center gap-2 font-bold text-xs uppercase tracking-wider">
                    {selectedReturn.inspection.passed ? (
                      <>
                        <CheckCircle2 className="h-4 w-4 text-emerald-500" />
                        <span className="text-emerald-600 dark:text-emerald-400">Kết quả kiểm tra: Đạt chuẩn hoàn tiền</span>
                      </>
                    ) : (
                      <>
                        <XCircle className="h-4 w-4 text-rose-500" />
                        <span className="text-rose-600 dark:text-rose-400">Kết quả kiểm tra: Không đạt chuẩn hoàn tiền</span>
                      </>
                    )}
                  </div>
                  <p className="text-xs text-foreground italic">"{selectedReturn.inspection.notes}"</p>
                  <p className="text-[10px] text-muted-foreground">Kỹ thuật viên: {selectedReturn.inspection.inspectedBy}</p>
                </div>
              )}

              {/* Refund Transaction Information (if completed) */}
              {selectedReturn.refund && selectedReturn.status === ReturnStatus.COMPLETED && (
                <div className="p-4 rounded-xl border border-emerald-500/20 bg-emerald-500/5 flex flex-wrap items-center justify-between gap-3 text-xs">
                  <div>
                    <span className="text-muted-foreground text-[10px] block">Mã giao dịch hoàn tiền</span>
                    <strong className="text-foreground font-mono">{selectedReturn.refund.transactionRef}</strong>
                  </div>
                  <div>
                    <span className="text-muted-foreground text-[10px] block">Phương thức</span>
                    <span className="text-foreground font-bold">{selectedReturn.refund.method}</span>
                  </div>
                  <div className="text-right">
                    <span className="text-muted-foreground text-[10px] block">Đã hoàn trả</span>
                    <span className="text-lg font-black text-emerald-600">${selectedReturn.refund.amount?.toLocaleString()}</span>
                  </div>
                </div>
              )}

              {/* Staff Action Console */}
              <div className="pt-3 border-t border-border">
                <span className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground block mb-3">
                  Thao tác xử lý quy trình (Staff Action Flow)
                </span>

                {/* PHASE 1: When PENDING_APPROVAL -> Staff Approves or Rejects */}
                {selectedReturn.status === ReturnStatus.PENDING_APPROVAL && (
                  <div className="flex flex-col sm:flex-row gap-3">
                    <Button
                      onClick={() => {
                        setApprovalUnits([]);
                        setApproveNote('');
                        setIsApprovingModalOpen(true);
                      }}
                      className="flex-1 h-12 rounded-xl text-xs font-bold uppercase tracking-wider bg-emerald-600 hover:bg-emerald-700 text-white flex items-center justify-center gap-2"
                    >
                      <Check className="h-4 w-4" />
                      Duyệt yêu cầu (Yêu cầu gửi hàng về)
                    </Button>
                    <Button
                      variant="outline"
                      onClick={() => setIsRejectingModalOpen(true)}
                      className="h-12 px-6 rounded-xl text-xs font-bold uppercase tracking-wider text-destructive border-destructive/30 hover:bg-destructive hover:text-white flex items-center justify-center gap-2"
                    >
                      <X className="h-4 w-4" />
                      Từ chối yêu cầu
                    </Button>
                  </div>
                )}

                {/* PHASE 2: When AWAITING_RETURN -> Package arrives -> Staff confirms arrival to start inspection */}
                {selectedReturn.status === ReturnStatus.AWAITING_RETURN && (
                  <div className="space-y-2">
                    <Button
                      onClick={() => setIsConfirmArrivalOpen(true)}
                      className="w-full h-12 rounded-xl text-xs font-bold uppercase tracking-wider bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center gap-2"
                    >
                      <Package className="h-4 w-4" />
                      Xác nhận hàng đã về tới kho (Bắt đầu tự kiểm tra)
                    </Button>
                    <p className="text-[10px] text-muted-foreground text-center">
                      Nhấn nút khi bưu tá giao kiện hàng hoàn về đến kho TechSales.
                    </p>
                  </div>
                )}

                {/* PHASE 3: When INSPECTING -> Staff inspects and approves refund or rejects */}
                {selectedReturn.status === ReturnStatus.INSPECTING && (
                  <div className="space-y-2">
                    <Button
                      onClick={() => {
                        setInspectionPassed(true);
                        setInspectionNote(`Đã kiểm tra kỹ thuật tại kho, sản phẩm lỗi ${selectedReturn.reason.toLowerCase()}, ngoại quan và linh kiện nguyên vẹn. Đạt chuẩn hoàn tiền.`);
                        setIsInspectionModalOpen(true);
                      }}
                      className="w-full h-12 rounded-xl text-xs font-bold uppercase tracking-wider bg-purple-600 hover:bg-purple-700 text-white flex items-center justify-center gap-2"
                    >
                      <FileSearch className="h-4 w-4" />
                      Tiến hành tự kiểm tra sản phẩm & Duyệt hoàn tiền
                    </Button>
                    <p className="text-[10px] text-muted-foreground text-center">
                      Kiểm tra thực tế ngoại quan, tem bảo hành và xác nhận lỗi để quyết định duyệt hoàn tiền.
                    </p>
                  </div>
                )}

                {/* PHASE 4: Completed or Rejected */}
                {(selectedReturn.status === ReturnStatus.COMPLETED || selectedReturn.status === ReturnStatus.REJECTED) && (
                  <div className="p-3 rounded-xl border border-border bg-muted/30 text-center text-xs text-muted-foreground font-medium">
                    Yêu cầu hoàn trả này đã hoàn tất xử lý. Không thể thao tác thêm.
                  </div>
                )}
              </div>
            </div>

            <DialogFooter className="pt-3 border-t border-border">
              <Button
                variant="outline"
                onClick={() => setSelectedReturn(null)}
                className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider border-border"
              >
                Đóng
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}

      {/* MODAL 1: Staff Approves Request & Requests Return Shipment */}
      <Dialog open={isApprovingModalOpen} onOpenChange={setIsApprovingModalOpen}>
        <DialogContent className="max-w-md rounded-2xl p-6 bg-card border-border shadow-2xl">
          <DialogHeader className="space-y-1">
            <DialogTitle className="text-base font-bold uppercase tracking-tight flex items-center gap-2 text-emerald-600">
              <CheckCircle2 className="h-5 w-5" />
              Duyệt Yêu Cầu Hoàn Trả #{selectedReturn?.id}
            </DialogTitle>
            <DialogDescription className="text-xs text-muted-foreground">
              Hệ thống sẽ thông báo cho khách hàng gửi hàng về địa chỉ kho TechSales để tiến hành kiểm tra.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-3 pt-2">
            <div className="rounded-xl border border-amber-500/30 bg-amber-500/5 p-3 space-y-3">
              <p className="text-[11px] font-semibold text-amber-700 dark:text-amber-400">Nhập serial thực tế để đối chiếu với máy đã bán. Có thể duyệt hoặc từ chối từng máy.</p>
              {approvalUnits.map((unit, index) => (
                <div key={index} className="rounded-lg border border-border bg-card p-2.5 space-y-2">
                  <div className="flex items-center gap-2">
                    <span className="text-[10px] font-bold text-muted-foreground w-16">Máy #{index + 1}</span>
                    <Input value={unit.serialNumber} onChange={(e) => setApprovalUnits(items => items.map((x, i) => i === index ? { ...x, serialNumber: e.target.value } : x))} placeholder="Nhập serial" className="h-8 font-mono text-xs uppercase" />
                    <Button type="button" size="sm" variant={unit.isApproved ? 'default' : 'outline'} onClick={() => setApprovalUnits(items => items.map((x, i) => i === index ? { ...x, isApproved: !x.isApproved } : x))} className="h-8 text-[10px] shrink-0">{unit.isApproved ? 'Duyệt' : 'Không duyệt'}</Button>
                  </div>
                  {!unit.isApproved && <Input value={unit.reason} onChange={(e) => setApprovalUnits(items => items.map((x, i) => i === index ? { ...x, reason: e.target.value } : x))} placeholder="Lý do riêng cho máy này" className="h-8 text-xs" />}
                </div>
              ))}
              <Textarea value={approveNote} onChange={(e) => setApproveNote(e.target.value)} rows={2} placeholder="Lý do chung / hướng dẫn gửi hàng (tuỳ chọn)" className="text-xs rounded-xl" />
            </div>
            <div>
              <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-1">
                Địa chỉ nhận hàng hoàn
              </label>
              <Input
                disabled
                value="Kho TechSales: 123 Tech Avenue, P. Bến Nghé, Q.1, TP. Hồ Chí Minh (Hotline: 1900-8888)"
                className="h-9 text-xs rounded-lg bg-muted text-foreground"
              />
            </div>

            <div>
              <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-1">
                Hướng dẫn gửi hàng cho khách hàng
              </label>
              <Textarea
                placeholder="Nhập hướng dẫn đóng gói, phụ kiện gửi kèm..."
                value={approveNote}
                onChange={(e) => setApproveNote(e.target.value)}
                rows={3}
                className="text-xs rounded-xl border-border"
              />
            </div>
          </div>

          <DialogFooter className="gap-2 sm:gap-0 pt-2">
            <Button
              variant="outline"
              onClick={() => setIsApprovingModalOpen(false)}
              className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider border-border"
            >
              Hủy
            </Button>
            <Button
              onClick={handleApprove}
              disabled={isApproving}
              className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider bg-emerald-600 hover:bg-emerald-700 text-white"
            >
              {isApproving ? <Loader2 className="h-3.5 w-3.5 animate-spin mr-1.5" /> : <Check className="h-3.5 w-3.5 mr-1.5" />}
              Xác nhận duyệt & Yêu cầu gửi hàng
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* MODAL 2: Staff Rejects Request */}
      <Dialog open={isRejectingModalOpen} onOpenChange={setIsRejectingModalOpen}>
        <DialogContent className="max-w-md rounded-2xl p-6 bg-card border-border shadow-2xl">
          <DialogHeader className="space-y-1">
            <DialogTitle className="text-base font-bold uppercase tracking-tight flex items-center gap-2 text-destructive">
              <XCircle className="h-5 w-5" />
              Từ Chối Yêu Cầu Hoàn Trả #{selectedReturn?.id}
            </DialogTitle>
            <DialogDescription className="text-xs text-muted-foreground">
              Vui lòng nêu rõ lý do từ chối để gửi thông báo giải thích cho khách hàng.
            </DialogDescription>
          </DialogHeader>

          <div className="pt-2">
            <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-1">
              Lý do từ chối <span className="text-destructive">*</span>
            </label>
            <Textarea
              placeholder="vd: Ảnh chụp minh chứng không thể hiện rõ lỗi, quá thời hạn 7 ngày đổi trả theo chính sách..."
              value={rejectionReason}
              onChange={(e) => setRejectionReason(e.target.value)}
              rows={3}
              className="text-xs rounded-xl border-border"
              required
            />
          </div>

          <DialogFooter className="gap-2 sm:gap-0 pt-2">
            <Button
              variant="outline"
              onClick={() => setIsRejectingModalOpen(false)}
              className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider border-border"
            >
              Hủy
            </Button>
            <Button
              onClick={handleReject}
              disabled={isRejecting}
              className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider bg-destructive hover:bg-destructive/90 text-white"
            >
              {isRejecting ? <Loader2 className="h-3.5 w-3.5 animate-spin mr-1.5" /> : <X className="h-3.5 w-3.5 mr-1.5" />}
              Xác nhận từ chối
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* MODAL 3: Confirm Item Arrival At Warehouse */}
      <Dialog open={isConfirmArrivalOpen} onOpenChange={setIsConfirmArrivalOpen}>
        <DialogContent className="max-w-md rounded-2xl p-6 bg-card border-border shadow-2xl">
          <DialogHeader className="space-y-1">
            <DialogTitle className="text-base font-bold uppercase tracking-tight flex items-center gap-2 text-blue-600">
              <Package className="h-5 w-5" />
              Xác Nhận Hàng Về Kho #{selectedReturn?.id}
            </DialogTitle>
            <DialogDescription className="text-xs text-muted-foreground">
              Xác nhận kiện hàng hoàn trả từ khách hàng đã cập bến kho TechSales và bắt đầu quy trình tự kiểm tra.
            </DialogDescription>
          </DialogHeader>

          <div className="pt-2 space-y-3">
            {selectedReturn?.returnTrackingCode && (
              <p className="text-xs text-foreground bg-blue-500/10 p-3 rounded-lg border border-blue-500/20">
                Mã vận đơn bưu tá: <strong>{selectedReturn.returnTrackingCode}</strong> ({selectedReturn.returnCarrier || 'Vận chuyển'})
              </p>
            )}

            <div>
              <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-1">
                Ghi chú tình trạng khi nhận kiện hàng
              </label>
              <Textarea
                placeholder="Ghi chú ngoại quan hộp khi shipper giao tới (vd: Hộp nguyên seal bọc xốp, kiện hàng nguyên vẹn...)"
                value={arrivalNote}
                onChange={(e) => setArrivalNote(e.target.value)}
                rows={2}
                className="text-xs rounded-xl border-border"
              />
            </div>
          </div>

          <DialogFooter className="gap-2 sm:gap-0 pt-2">
            <Button
              variant="outline"
              onClick={() => setIsConfirmArrivalOpen(false)}
              className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider border-border"
            >
              Hủy
            </Button>
            <Button
              onClick={handleConfirmArrival}
              disabled={isConfirmingReceived}
              className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider bg-blue-600 hover:bg-blue-700 text-white"
            >
              {isConfirmingReceived ? <Loader2 className="h-3.5 w-3.5 animate-spin mr-1.5" /> : <Package className="h-3.5 w-3.5 mr-1.5" />}
              Xác nhận đã nhận & Bắt đầu kiểm tra
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* MODAL 4: Self Inspection Form & Refund Approval */}
      <Dialog open={isInspectionModalOpen} onOpenChange={setIsInspectionModalOpen}>
        <DialogContent className="max-w-xl max-h-[90vh] overflow-y-auto rounded-2xl p-6 bg-card border-border shadow-2xl">
          <DialogHeader className="space-y-1">
            <DialogTitle className="text-lg font-bold uppercase tracking-tight flex items-center gap-2 text-purple-600">
              <FileSearch className="h-5 w-5" />
              Biên Bản Tự Kiểm Tra Chất Lượng Sản Phẩm
            </DialogTitle>
            <DialogDescription className="text-xs text-muted-foreground">
              Mã yêu cầu: #{selectedReturn?.id} • Sản phẩm: {selectedReturn?.productName} • Tiền hoàn: ${selectedReturn?.totalRefundAmount.toLocaleString()}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 pt-2">
            <div className="rounded-xl border border-amber-500/30 bg-amber-500/5 p-3 space-y-3">
              <p className="text-[11px] font-semibold text-amber-700 dark:text-amber-400">Nhập serial thực tế sau khi kho đã nhận máy. Staff quyết định riêng từng unit.</p>
              <p className="text-[10px] text-muted-foreground">Khách khai báo {selectedReturn?.quantity} máy, nhưng có thể chỉ nhận thực tế một phần.</p>
              {inspectionUnits.map((unit, index) => (
                <div key={index} className="rounded-lg border border-border bg-card p-2.5 space-y-2">
                  <div className="flex items-center gap-2">
                    <span className="text-[10px] font-bold text-muted-foreground w-14">Unit #{index + 1}</span>
                    <Input value={unit.serialNumber} onChange={(e) => setInspectionUnits(items => items.map((x, i) => i === index ? { ...x, serialNumber: e.target.value } : x))} placeholder="Serial / IMEI thực nhận" className="h-8 font-mono text-xs uppercase" />
                    <Button type="button" size="sm" variant={unit.isApproved ? 'default' : 'outline'} onClick={() => setInspectionUnits(items => items.map((x, i) => i === index ? { ...x, isApproved: !x.isApproved } : x))} className="h-8 text-[10px] shrink-0">{unit.isApproved ? 'Chấp nhận' : 'Không nhận'}</Button>
                    <Button type="button" size="sm" variant="ghost" onClick={() => setInspectionUnits(items => items.filter((_, i) => i !== index))} className="h-8 px-2 text-[10px] text-destructive">Xóa</Button>
                  </div>
                  {!unit.isApproved && <Input value={unit.reason} onChange={(e) => setInspectionUnits(items => items.map((x, i) => i === index ? { ...x, reason: e.target.value } : x))} placeholder="Lý do riêng của unit này" className="h-8 text-xs" />}
                </div>
              ))}
              <Button type="button" variant="outline" size="sm" disabled={inspectionUnits.length >= (selectedReturn?.quantity || 0)} onClick={() => setInspectionUnits(items => [...items, { serialNumber: '', isApproved: true, reason: '' }])} className="h-8 text-[10px]">+ Thêm unit thực nhận</Button>
              <Textarea value={inspectionNote} onChange={(e) => setInspectionNote(e.target.value)} rows={2} placeholder="Lý do chung cho toàn bộ yêu cầu" className="text-xs rounded-xl" />
            </div>
            {/* Physical Inspection Checklist */}
            <div className="p-4 rounded-xl border border-border bg-muted/20 space-y-2.5">
              <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block">
                Danh mục tiêu chuẩn kiểm tra tại kho
              </span>

              <label className="flex items-center gap-2.5 text-xs text-foreground cursor-pointer">
                <input
                  type="checkbox"
                  checked={checkAppearance}
                  onChange={(e) => setCheckAppearance(e.target.checked)}
                  className="rounded text-primary h-4 w-4"
                />
                <span>Ngoại quan nguyên vẹn, không móp méo / bể vỡ do người dùng</span>
              </label>

              <label className="flex items-center gap-2.5 text-xs text-foreground cursor-pointer">
                <input
                  type="checkbox"
                  checked={checkAccessories}
                  onChange={(e) => setCheckAccessories(e.target.checked)}
                  className="rounded text-primary h-4 w-4"
                />
                <span>Đầy đủ hộp sản phẩm, cáp sạc và phụ kiện nguyên bản</span>
              </label>

              <label className="flex items-center gap-2.5 text-xs text-foreground cursor-pointer">
                <input
                  type="checkbox"
                  checked={checkSerial}
                  onChange={(e) => setCheckSerial(e.target.checked)}
                  className="rounded text-primary h-4 w-4"
                />
                <span>Tem bảo hành nguyên vẹn, Serial / IMEI trùng khớp đơn hàng</span>
              </label>

              <label className="flex items-center gap-2.5 text-xs text-foreground cursor-pointer">
                <input
                  type="checkbox"
                  checked={checkDefect}
                  onChange={(e) => setCheckDefect(e.target.checked)}
                  className="rounded text-primary h-4 w-4"
                />
                <span>Xác nhận lỗi kỹ thuật đúng như khách hàng đã báo cáo</span>
              </label>
            </div>

            {/* Quality Standard Radio Verdict */}
            <div>
              <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-2">
                Kết quả kiểm tra chất lượng sản phẩm <span className="text-destructive">*</span>
              </label>
              <div className="grid grid-cols-2 gap-3">
                <div
                  onClick={() => setInspectionPassed(true)}
                  className={`p-3.5 rounded-xl border-2 cursor-pointer transition-all flex items-center gap-3 ${
                    inspectionPassed
                      ? 'border-emerald-500 bg-emerald-500/10 text-emerald-700 dark:text-emerald-300'
                      : 'border-border bg-card hover:bg-muted text-muted-foreground'
                  }`}
                >
                  <CheckCircle2 className={`h-5 w-5 ${inspectionPassed ? 'text-emerald-500' : 'text-muted-foreground'}`} />
                  <div>
                    <span className="font-bold text-xs uppercase tracking-wider block">Đạt chuẩn hoàn tiền</span>
                    <span className="text-[9px] opacity-75">Đủ điều kiện hoàn tiền 100%</span>
                  </div>
                </div>

                <div
                  onClick={() => setInspectionPassed(false)}
                  className={`p-3.5 rounded-xl border-2 cursor-pointer transition-all flex items-center gap-3 ${
                    !inspectionPassed
                      ? 'border-rose-500 bg-rose-500/10 text-rose-700 dark:text-rose-300'
                      : 'border-border bg-card hover:bg-muted text-muted-foreground'
                  }`}
                >
                  <XCircle className={`h-5 w-5 ${!inspectionPassed ? 'text-rose-500' : 'text-muted-foreground'}`} />
                  <div>
                    <span className="font-bold text-xs uppercase tracking-wider block">Không đạt chuẩn</span>
                    <span className="text-[9px] opacity-75">Từ chối hoàn tiền</span>
                  </div>
                </div>
              </div>
            </div>

            {/* Inspector Notes */}
            <div>
              <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-1">
                Ghi chú chi tiết kết quả kiểm tra <span className="text-destructive">*</span>
              </label>
              <Textarea
                placeholder="Nhập nhận định kỹ thuật chi tiết của nhân viên kiểm tra..."
                value={inspectionNote}
                onChange={(e) => setInspectionNote(e.target.value)}
                rows={3}
                className="text-xs rounded-xl border-border"
                required
              />
            </div>

            <div>
              <label className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block mb-1">
                Kỹ thuật viên thực hiện kiểm tra
              </label>
              <Input
                value={inspectedBy}
                onChange={(e) => setInspectedBy(e.target.value)}
                className="h-9 text-xs rounded-lg"
              />
            </div>
          </div>

          <DialogFooter className="gap-2 sm:gap-0 pt-3">
            <Button
              variant="outline"
              onClick={() => setIsInspectionModalOpen(false)}
              className="h-10 rounded-xl text-xs font-bold uppercase tracking-wider border-border"
            >
              Hủy
            </Button>
            <Button
              onClick={handleSubmitInspection}
              disabled={isSubmittingInspection}
              className={`h-10 rounded-xl text-xs font-bold uppercase tracking-wider text-white ${
                inspectionPassed
                  ? 'bg-emerald-600 hover:bg-emerald-700'
                  : 'bg-destructive hover:bg-destructive/90'
              }`}
            >
              {isSubmittingInspection ? (
                <Loader2 className="h-3.5 w-3.5 animate-spin mr-1.5" />
              ) : inspectionPassed ? (
                <CheckCircle2 className="h-3.5 w-3.5 mr-1.5" />
              ) : (
                <XCircle className="h-3.5 w-3.5 mr-1.5" />
              )}
              {inspectionPassed ? 'Duyệt hàng đạt chuẩn & Hoàn tiền' : 'Từ chối hoàn tiền (Hàng lỗi người dùng)'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Lightbox for zooming photos */}
      {zoomImage && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 p-4"
          onClick={() => setZoomImage(null)}
        >
          <div className="relative max-w-3xl max-h-[90vh]">
            <img
              src={zoomImage}
              alt="Enlarged proof"
              className="max-h-[85vh] max-w-full rounded-2xl object-contain shadow-2xl"
            />
            <Button
              size="sm"
              variant="secondary"
              className="absolute top-3 right-3 rounded-full h-8 w-8 p-0"
              onClick={() => setZoomImage(null)}
            >
              ✕
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
