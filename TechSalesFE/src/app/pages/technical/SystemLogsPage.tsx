import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "../../components/ui/dialog";
import { Search, Loader2, Calendar, FileText, User, ArrowLeft, ArrowRight, Eye } from 'lucide-react';
import { useGetAuditLogs } from '../../../dataHook/auditDataHook';
import { AuditLog } from '../../../models/ui_types/auditLog';

export function SystemLogsPage() {
  const [page, setPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null);
  
  const { data, isLoading } = useGetAuditLogs(page, 15);
  const logs = data?.items || [];
  const totalCount = data?.totalCount || 0;
  const totalPages = Math.ceil(totalCount / 15);

  const getActionBadgeVariant = (action: string) => {
    const act = action.toUpperCase();
    if (act.includes('CREATE')) return 'success';
    if (act.includes('UPDATE')) return 'info';
    if (act.includes('DELETE') || act.includes('LOCK') || act.includes('CANCEL')) return 'danger';
    return 'secondary';
  };

  const formatJson = (jsonStr?: string) => {
    if (!jsonStr) return 'None';
    try {
      const parsed = JSON.parse(jsonStr);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return jsonStr;
    }
  };

  const filteredLogs = logs.filter(log => {
    return (
      log.action.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.tableName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (log.userId && log.userId.toLowerCase().includes(searchTerm.toLowerCase()))
    );
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">System Audit Logs</h1>
        <p className="text-muted-foreground">Track all administrative, sales, and system events across the application</p>
      </div>

      <Card className="border-none shadow-xl">
        <CardHeader className="pb-3 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <CardTitle className="text-lg">Event History</CardTitle>
          <div className="relative w-full sm:w-72">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Filter by action or table..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-9"
            />
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex h-64 items-center justify-center">
              <div className="flex flex-col items-center gap-2">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
                <p className="text-muted-foreground">Loading audit logs...</p>
              </div>
            </div>
          ) : filteredLogs.length === 0 ? (
            <div className="flex h-64 flex-col items-center justify-center text-center">
              <FileText className="h-12 w-12 text-muted-foreground mb-4" />
              <p className="text-xl font-semibold">No audit logs found</p>
              <p className="text-muted-foreground">There are no logged events matching your filter.</p>
            </div>
          ) : (
            <div className="space-y-4">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Timestamp</TableHead>
                    <TableHead>Performed By (User ID)</TableHead>
                    <TableHead>Action</TableHead>
                    <TableHead>Target Table</TableHead>
                    <TableHead>Record Key</TableHead>
                    <TableHead className="text-right">Inspection</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredLogs.map(log => (
                    <TableRow key={log.id} className="hover:bg-muted/30 transition-colors">
                      <TableCell className="whitespace-nowrap font-medium text-xs text-muted-foreground">
                        <div className="flex items-center gap-1.5">
                          <Calendar className="h-3.5 w-3.5" />
                          {new Date(log.createdAt).toLocaleString()}
                        </div>
                      </TableCell>
                      <TableCell className="font-mono text-xs max-w-[150px] truncate">
                        {log.userId ? (
                          <div className="flex items-center gap-1.5">
                            <User className="h-3.5 w-3.5 text-primary" />
                            <span className="truncate">{log.userId}</span>
                          </div>
                        ) : (
                          <span className="text-muted-foreground italic">Anonymous/System</span>
                        )}
                      </TableCell>
                      <TableCell>
                        <Badge variant={getActionBadgeVariant(log.action)}>
                          {log.action}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline" className="font-mono bg-muted/40 text-[10px]">
                          {log.tableName}
                        </Badge>
                      </TableCell>
                      <TableCell className="font-mono text-xs text-muted-foreground">
                        {log.primaryKey || 'N/A'}
                      </TableCell>
                      <TableCell className="text-right">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => setSelectedLog(log)}
                        >
                          <Eye className="h-4 w-4 mr-1 text-primary" />
                          Inspect
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {/* Pagination controls */}
              <div className="flex items-center justify-between border-t pt-4">
                <p className="text-xs text-muted-foreground">
                  Showing page <span className="font-semibold text-foreground">{page}</span> of{' '}
                  <span className="font-semibold text-foreground">{totalPages || 1}</span> ({totalCount} total entries)
                </p>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setPage(p => Math.max(1, p - 1))}
                    disabled={page === 1}
                  >
                    <ArrowLeft className="h-4 w-4 mr-1" />
                    Previous
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                    disabled={page >= totalPages}
                  >
                    Next
                    <ArrowRight className="h-4 w-4 ml-1" />
                  </Button>
                </div>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Log Inspection Modal */}
      <Dialog open={!!selectedLog} onOpenChange={(open) => !open && setSelectedLog(null)}>
        <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5 text-primary" />
              Event Details: {selectedLog?.action}
            </DialogTitle>
            <DialogDescription>
              Detailed payload change representation inside the {selectedLog?.tableName} table.
            </DialogDescription>
          </DialogHeader>

          {selectedLog && (
            <div className="space-y-4 py-3">
              <div className="grid grid-cols-2 gap-4 text-sm bg-muted/40 p-3 rounded-lg border">
                <div>
                  <p className="text-xs text-muted-foreground uppercase font-bold">Action Name</p>
                  <p className="font-semibold mt-0.5">{selectedLog.action}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground uppercase font-bold">Target Table</p>
                  <p className="font-semibold mt-0.5 font-mono">{selectedLog.tableName}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground uppercase font-bold">Affected Record (Primary Key)</p>
                  <p className="font-semibold mt-0.5 font-mono text-xs text-primary">{selectedLog.primaryKey || 'N/A'}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground uppercase font-bold">Performed By (User ID)</p>
                  <p className="font-semibold mt-0.5 font-mono text-xs">{selectedLog.userId || 'System'}</p>
                </div>
                <div className="col-span-2">
                  <p className="text-xs text-muted-foreground uppercase font-bold">Timestamp</p>
                  <p className="font-semibold mt-0.5">{new Date(selectedLog.createdAt).toLocaleString()}</p>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <h3 className="font-semibold text-sm mb-2 text-orange-600 dark:text-orange-400">Prior State (Old Values)</h3>
                  <pre className="bg-slate-950 text-slate-100 text-xs p-3 rounded-lg border font-mono max-h-[300px] overflow-auto">
                    {formatJson(selectedLog.oldValues)}
                  </pre>
                </div>
                <div>
                  <h3 className="font-semibold text-sm mb-2 text-green-600 dark:text-green-400">Post State (New Values)</h3>
                  <pre className="bg-slate-950 text-slate-100 text-xs p-3 rounded-lg border font-mono max-h-[300px] overflow-auto">
                    {formatJson(selectedLog.newValues)}
                  </pre>
                </div>
              </div>

              {selectedLog.affectedColumns && (
                <div>
                  <h3 className="font-semibold text-sm mb-1.5">Affected Columns</h3>
                  <div className="flex flex-wrap gap-1.5">
                    {selectedLog.affectedColumns.split(',').map(col => (
                      <Badge key={col} variant="secondary" className="font-mono text-xs">
                        {col.trim()}
                      </Badge>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}

          <DialogFooter>
            <Button onClick={() => setSelectedLog(null)}>Close Inspection</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
