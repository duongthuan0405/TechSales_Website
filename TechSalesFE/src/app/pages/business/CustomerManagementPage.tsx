import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "../../components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "../../components/ui/table";
import { Badge } from "../../components/ui/badge";
import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "../../components/ui/dialog";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "../../components/ui/alert-dialog";
import { 
  Search, 
  Mail, 
  Calendar, 
  Loader2, 
  UserRound, 
  UserX, 
  UserCheck, 
  Edit, 
  UserPlus,
  Phone,
  Clock,
  ShieldCheck,
  ShieldAlert
} from "lucide-react";
import { 
  useGetCustomers, 
  useUpdateUser, 
  useCreateUser,
  useToggleUserStatus
} from "../../../dataHook/userDataHook";
import { User, UserStatus } from "../../../models/ui_types/user";
import { toast } from "sonner";
import { UserForm } from "../../components/business/UserForm";
import { Separator } from "../../components/ui/separator";

export function CustomerManagementPage() {
  const { data: customers = [], isLoading } = useGetCustomers();
  const updateMutation = useUpdateUser();
  const createMutation = useCreateUser();
  const toggleStatusMutation = useToggleUserStatus();

  const [searchTerm, setSearchTerm] = useState("");
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | undefined>(undefined);
  const [editingUser, setEditingUser] = useState<User | undefined>(undefined);

  const filteredCustomers = customers.filter((customer) => {
    return (
      customer.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      customer.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      customer.phone.includes(searchTerm)
    );
  });

  const handleRowClick = (user: User) => {
    setSelectedUser(user);
    setIsDetailOpen(true);
  };

  const handleEdit = (user: User, e?: React.MouseEvent) => {
    e?.stopPropagation();
    setEditingUser(user);
    setIsFormOpen(true);
  };

  const handleAdd = () => {
    setEditingUser(undefined);
    setIsFormOpen(true);
  };

  const onFormSubmit = (data: any) => {
    if (editingUser) {
      updateMutation.mutate(
        { id: editingUser.id, data },
        {
          onSuccess: (updated) => {
            toast.success("Customer account updated");
            setIsFormOpen(false);
            if (selectedUser?.id === editingUser.id) {
              setSelectedUser(updated);
            }
          },
          onError: () => toast.error("Failed to update account"),
        }
      );
    } else {
      createMutation.mutate(data, {
        onSuccess: () => {
          toast.success("Customer account created");
          setIsFormOpen(false);
        },
        onError: () => toast.error("Failed to create account"),
      });
    }
  };

  const handleToggleStatus = (user: User, e?: React.MouseEvent) => {
    e?.stopPropagation();
    
    toggleStatusMutation.mutate({ id: user.id, status: user.status }, {
      onSuccess: (updated) => {
        const isBlocked = updated.status === UserStatus.BLOCKED;
        toast.success(`Account ${isBlocked ? 'blocked' : 'activated'} successfully`);
        if (selectedUser?.id === user.id) setSelectedUser(updated);
      },
      onError: () => toast.error("Failed to update account status"),
    });
  };

  const getStatusBadge = (status: UserStatus) => {
    switch (status) {
      case UserStatus.ACTIVE: 
        return <Badge variant="success" className="gap-1"><UserCheck className="h-3 w-3" /> Active</Badge>;
      case UserStatus.BLOCKED: 
        return <Badge variant="destructive" className="gap-1"><UserX className="h-3 w-3" /> Blocked</Badge>;
      default: 
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Customer Accounts</h1>
          <p className="text-muted-foreground">Manage customer profiles, security status and activity</p>
        </div>
        <Button onClick={handleAdd}>
          <UserPlus className="mr-2 h-4 w-4" />
          Add Customer
        </Button>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search accounts by name, email or phone..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-9"
          />
        </div>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : filteredCustomers.length === 0 ? (
        <Card className="flex h-64 flex-col items-center justify-center text-center">
          <UserRound className="h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-xl font-semibold">No customer accounts found</p>
          <p className="text-muted-foreground">Try adjusting your filters</p>
        </Card>
      ) : (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-medium">
              Customers ({filteredCustomers.length})
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[300px]">Customer</TableHead>
                  <TableHead>Account Status</TableHead>
                  <TableHead>Joined</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredCustomers.map((user) => (
                  <TableRow 
                    key={user.id} 
                    className="cursor-pointer hover:bg-muted/50 transition-colors"
                    onClick={() => handleRowClick(user)}
                  >
                    <TableCell>
                      <div className="flex items-center gap-3">
                        <div className="h-10 w-10 rounded-full border bg-muted overflow-hidden shadow-sm flex-shrink-0">
                          <img 
                            src={user.avatarUrl || `https://api.dicebear.com/7.x/avataaars/svg?seed=${user.email}`} 
                            alt={user.fullName} 
                          />
                        </div>
                        <div>
                          <div className="font-semibold">{user.fullName}</div>
                          <div className="text-xs text-muted-foreground flex items-center gap-1 break-all">
                            <Mail className="h-3 w-3" /> {user.email}
                          </div>
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>{getStatusBadge(user.status)}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2 text-sm text-muted-foreground">
                        <Calendar className="h-3 w-3" />
                        {new Date(user.createdAt).toLocaleDateString()}
                      </div>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-1">
                        <Button 
                          variant="ghost" 
                          size="icon"
                          onClick={(e) => handleEdit(user, e)}
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button 
                          variant="ghost" 
                          size="icon"
                          onClick={(e) => handleToggleStatus(user, e)}
                          disabled={toggleStatusMutation.isPending}
                        >
                          {toggleStatusMutation.isPending && (toggleStatusMutation.variables as any)?.id === user.id ? (
                             <Loader2 className="h-4 w-4 animate-spin" />
                          ) : user.status === UserStatus.BLOCKED ? (
                            <UserCheck className="h-4 w-4 text-success" />
                          ) : (
                            <UserX className="h-4 w-4 text-destructive" />
                          )}
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Detail Dialog */}
      <Dialog open={isDetailOpen} onOpenChange={setIsDetailOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Customer Details</DialogTitle>
            <DialogDescription>
              Comprehensive overview of account status and profile information.
            </DialogDescription>
          </DialogHeader>

          {selectedUser && (
            <div className="grid gap-6 py-4">
              <div className="flex flex-col gap-4">
                <div className="flex items-center gap-4">
                  <div className="h-20 w-20 rounded-full border bg-muted overflow-hidden shadow-sm flex-shrink-0">
                    <img 
                      src={selectedUser.avatarUrl || `https://api.dicebear.com/7.x/avataaars/svg?seed=${selectedUser.email}`} 
                      alt={selectedUser.fullName} 
                    />
                  </div>
                  <div>
                    <h2 className="text-2xl font-bold break-all">{selectedUser.fullName}</h2>
                    <p className="text-muted-foreground flex items-center gap-1 break-all">
                      <Mail className="h-4 w-4" /> {selectedUser.email}
                    </p>
                    <div className="mt-2">
                      {getStatusBadge(selectedUser.status)}
                    </div>
                  </div>
                </div>

                <div className="flex flex-row gap-2">
                  <Button variant="outline" size="sm" onClick={() => handleEdit(selectedUser)}>
                    <Edit className="mr-2 h-4 w-4" /> Edit Profile
                  </Button>
                  <Button 
                    variant={selectedUser.status === UserStatus.BLOCKED ? "success" : "destructive"} 
                    size="sm"
                    onClick={() => handleToggleStatus(selectedUser)}
                    disabled={toggleStatusMutation.isPending}
                  >
                    {toggleStatusMutation.isPending ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : selectedUser.status === UserStatus.BLOCKED ? (
                      <><ShieldCheck className="mr-2 h-4 w-4" /> Unblock Account</>
                    ) : (
                      <><ShieldAlert className="mr-2 h-4 w-4" /> Block Account</>
                    )}
                  </Button>
                </div>
              </div>

              <Separator />

              <div className="space-y-4">
                <h3 className="font-semibold text-sm uppercase tracking-wider text-muted-foreground">Profile Information</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="flex items-center gap-3 text-sm">
                    <Phone className="h-4 w-4 text-primary" />
                    <div>
                      <p className="text-xs text-muted-foreground">Phone Number</p>
                      <p className="font-medium">{selectedUser.phone || "Not provided"}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-3 text-sm">
                    <Clock className="h-4 w-4 text-primary" />
                    <div>
                      <p className="text-xs text-muted-foreground">Joined Since</p>
                      <p className="font-medium">{new Date(selectedUser.createdAt).toLocaleDateString(undefined, { dateStyle: 'long' })}</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}
          <DialogFooter className="sticky bottom-0 bg-background pt-2 border-t mt-4">
            <Button variant="ghost" onClick={() => setIsDetailOpen(false)}>Close</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Form Dialog */}
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>{editingUser ? "Edit Account" : "Add Account"}</DialogTitle>
            <DialogDescription>
              Update credentials and profile information.
            </DialogDescription>
          </DialogHeader>
          <UserForm 
            initialData={editingUser} 
            onSubmit={onFormSubmit}
            isLoading={updateMutation.isPending || createMutation.isPending}
            forcedRole={selectedUser?.role} 
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}
