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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../components/ui/select";
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
  UserPlus, 
  Mail, 
  UserCog, 
  Edit, 
  Loader2,
  UserCheck,
  UserX,
  Phone,
  Clock,
  ShieldCheck,
  ShieldAlert,
  Fingerprint
} from "lucide-react";
import { 
  useGetStaff, 
  useUpdateUser, 
  useCreateUser,
  useToggleUserStatus
} from "../../../dataHook/userDataHook";
import { User, UserRole, UserStatus } from "../../../models/ui_types/user";
import { toast } from "sonner";
import { UserForm } from "../../components/business/UserForm";
import { Separator } from "../../components/ui/separator";
import { useAuth } from "../../context/AuthContext";

export function StaffManagementPage() {
  const { user: currentUser } = useAuth();
  const { data: staffMembers = [], isLoading } = useGetStaff();
  const updateMutation = useUpdateUser();
  const createMutation = useCreateUser();
  const toggleStatusMutation = useToggleUserStatus();

  const [searchTerm, setSearchTerm] = useState("");
  const [roleFilter, setRoleFilter] = useState<string>("all");
  
  // Dialog states
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | undefined>(undefined);
  const [editingUser, setEditingUser] = useState<User | undefined>(undefined);

  const filteredStaff = staffMembers.filter((u) => {
    const matchesSearch = 
      u.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      u.email.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesRole = roleFilter === "all" || u.role === roleFilter;
    return matchesSearch && matchesRole;
  });

  const handleRowClick = (user: User) => {
    setSelectedUser(user);
    setIsDetailOpen(true);
  };

  const handleCreate = () => {
    setEditingUser(undefined);
    setIsFormOpen(true);
  };

  const handleEdit = (user: User, e?: React.MouseEvent) => {
    e?.stopPropagation();
    setEditingUser(user);
    setIsFormOpen(true);
  };

  const onFormSubmit = (data: any) => {
    if (editingUser) {
      updateMutation.mutate(
        { id: editingUser.id, data },
        {
          onSuccess: (updated) => {
            toast.success("Staff account updated");
            setIsFormOpen(false);
            if (selectedUser?.id === editingUser.id) {
              setSelectedUser(updated);
            }
          },
          onError: () => toast.error("Failed to update staff account"),
        }
      );
    } else {
      createMutation.mutate(data, {
        onSuccess: () => {
          toast.success("Staff account created");
          setIsFormOpen(false);
        },
        onError: () => toast.error("Failed to create staff account"),
      });
    }
  };

  const handleToggleStatus = (user: User, e?: React.MouseEvent) => {
    e?.stopPropagation();
    if (user.role === 'Technical Admin') {
      toast.error("Cannot block a Technical Admin");
      return;
    }

    toggleStatusMutation.mutate({ id: user.id, status: user.status }, {
      onSuccess: (updated) => {
        const isBlocked = updated.status === UserStatus.BLOCKED;
        toast.success(`Staff account ${isBlocked ? 'blocked' : 'activated'}`);
        if (selectedUser?.id === user.id) setSelectedUser(updated);
      },
      onError: () => toast.error("Failed to update account status"),
    });
  };



  const getRoleBadge = (role: UserRole) => {
    switch (role) {
      case 'Technical Admin': return <Badge className="bg-red-500/10 text-red-500 border-red-500/20">Tech Admin</Badge>;
      case 'Business Admin': return <Badge className="bg-blue-500/10 text-blue-500 border-blue-500/20">Bus Admin</Badge>;
      case 'Staff': return <Badge className="bg-green-500/10 text-green-500 border-green-500/20">Sales Staff</Badge>;
      default: return <Badge variant="outline">{role}</Badge>;
    }
  };

  const getStatusBadge = (status: UserStatus) => {
    switch (status) {
      case UserStatus.ACTIVE: return <Badge variant="success" className="gap-1"><UserCheck className="h-3 w-3" /> Active</Badge>;
      case UserStatus.BLOCKED: return <Badge variant="destructive" className="gap-1"><UserX className="h-3 w-3" /> Blocked</Badge>;
      default: return <Badge variant="secondary">{status}</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Staff Management</h1>
          <p className="text-muted-foreground">Manage administrative access and team roles</p>
        </div>
        <Button onClick={handleCreate}>
          <UserPlus className="mr-2 h-4 w-4" />
          Add Staff Member
        </Button>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search staff by name or email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-9"
          />
        </div>
        <Select value={roleFilter} onValueChange={setRoleFilter}>
          <SelectTrigger className="w-[200px]">
            <SelectValue placeholder="Filter by Role" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Roles</SelectItem>
            <SelectItem value="Staff">Sales Staff</SelectItem>
            <SelectItem value="Business Admin">Business Admin</SelectItem>
            <SelectItem value="Technical Admin">Technical Admin</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : filteredStaff.length === 0 ? (
        <Card className="flex h-64 flex-col items-center justify-center text-center">
          <UserCog className="h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-xl font-semibold">No staff members found</p>
          <p className="text-muted-foreground">Try adjusting your filters</p>
        </Card>
      ) : (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-medium">
              Staff Accounts ({filteredStaff.length})
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[300px]">User</TableHead>
                  <TableHead>Role</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Joined</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredStaff.map((user) => (
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
                    <TableCell>{getRoleBadge(user.role)}</TableCell>
                    <TableCell>{getStatusBadge(user.status)}</TableCell>
                    <TableCell className="text-sm text-muted-foreground">
                      {new Date(user.createdAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-1">
                        <Button variant="ghost" size="icon" onClick={(e) => handleEdit(user, e)}>
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button 
                          variant="ghost" 
                          size="icon" 
                          onClick={(e) => handleToggleStatus(user, e)}
                          disabled={user.role === 'Technical Admin' || toggleStatusMutation.isPending || user.id === currentUser?.id}
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
            <DialogTitle>Staff Details</DialogTitle>
            <DialogDescription>
              Detailed view of administrative account and system permissions.
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
                    <div className="mt-2 flex flex-wrap gap-2">
                      {getRoleBadge(selectedUser.role)}
                      {getStatusBadge(selectedUser.status)}
                    </div>
                  </div>
                </div>

                <div className="flex flex-row gap-2">
                  <Button variant="outline" size="sm" onClick={() => handleEdit(selectedUser)}>
                    <Edit className="mr-2 h-4 w-4" /> Edit Account
                  </Button>
                  <Button 
                    variant={selectedUser.status === UserStatus.BLOCKED ? "success" : "destructive"} 
                    size="sm"
                    onClick={() => handleToggleStatus(selectedUser)}
                    disabled={toggleStatusMutation.isPending || selectedUser.role === 'Technical Admin'}
                  >
                    {toggleStatusMutation.isPending ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : selectedUser.status === UserStatus.BLOCKED ? (
                      <><ShieldCheck className="mr-2 h-4 w-4" /> Unblock</>
                    ) : (
                      <><ShieldAlert className="mr-2 h-4 w-4" /> Block Account</>
                    )}
                  </Button>
                </div>
              </div>

              <Separator />

              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div className="space-y-4">
                  <h3 className="font-semibold text-sm uppercase tracking-wider text-muted-foreground">Access Information</h3>
                  <div className="grid gap-3">
                    <div className="flex items-center gap-3 text-sm">
                      <Fingerprint className="h-4 w-4 text-primary" />
                      <div>
                        <p className="text-xs text-muted-foreground">Internal ID</p>
                        <p className="font-medium font-mono break-all text-[10px]">{selectedUser.id}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 text-sm">
                      <Phone className="h-4 w-4 text-primary" />
                      <div>
                        <p className="text-xs text-muted-foreground">Direct Line</p>
                        <p className="font-medium">{selectedUser.phone || "No phone recorded"}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 text-sm">
                      <Clock className="h-4 w-4 text-primary" />
                      <div>
                        <p className="text-xs text-muted-foreground">Access Granted</p>
                        <p className="font-medium">{new Date(selectedUser.createdAt).toLocaleDateString(undefined, { dateStyle: 'long' })}</p>
                      </div>
                    </div>
                  </div>
                </div>

                <div className="space-y-4">
                  <h3 className="font-semibold text-sm uppercase tracking-wider text-muted-foreground">Permissions Overview</h3>
                  <Card className="bg-muted/30 border-none shadow-none">
                    <CardContent className="p-4 space-y-2">
                      <div className="flex items-center justify-between text-sm">
                        <span className="text-muted-foreground">System Access</span>
                        <Badge variant="success">Granted</Badge>
                      </div>
                      <div className="flex items-center justify-between text-sm">
                        <span className="text-muted-foreground">Business Reports</span>
                        <Badge variant={selectedUser.role.includes('Admin') ? "success" : "outline"}>
                          {selectedUser.role.includes('Admin') ? "Full" : "No"}
                        </Badge>
                      </div>
                      <div className="flex items-center justify-between text-sm">
                        <span className="text-muted-foreground">Customer Support</span>
                        <Badge variant="success">Standard</Badge>
                      </div>
                    </CardContent>
                  </Card>
                </div>
              </div>
            </div>
          )}
          <DialogFooter className="sticky bottom-0 bg-background pt-2 border-t mt-4 flex flex-col sm:flex-row gap-4 justify-end items-center">
            <Button variant="ghost" onClick={() => setIsDetailOpen(false)}>Close</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Form Dialog */}
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>{editingUser ? "Edit Staff Account" : "Add Staff Account"}</DialogTitle>
            <DialogDescription>
              Update credentials and profile for the team member.
            </DialogDescription>
          </DialogHeader>
          <UserForm 
            initialData={editingUser} 
            onSubmit={onFormSubmit}
            isLoading={updateMutation.isPending || createMutation.isPending}
            currentUserRole={currentUser?.role}
          />
        </DialogContent>
      </Dialog>


    </div>
  );
}
