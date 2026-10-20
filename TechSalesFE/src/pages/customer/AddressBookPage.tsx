import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Modal } from '../../components/ui/modal';
import { 
  MapPin, 
  Plus, 
  Trash2, 
  Edit3, 
  Loader2,
} from 'lucide-react';
import { toast } from 'sonner';
import { 
  useGetAddresses, 
  useCreateAddress, 
  useUpdateAddress, 
  useDeleteAddress, 
  useSetDefaultAddress 
} from '../../dataHook/addressDataHook';
import { Address } from '../../models/ui_types/address';

export function AddressBookPage() {
  const { data: addresses = [], isLoading: isFetching } = useGetAddresses();
  const { mutate: createAddress } = useCreateAddress();
  const { mutate: updateAddress } = useUpdateAddress();
  const { mutate: deleteAddress } = useDeleteAddress();
  const { mutate: setDefaultAddress } = useSetDefaultAddress();

  const [showModal, setShowModal] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [editingAddress, setEditingAddress] = useState<Address | null>(null);
  
  const [formData, setFormData] = useState({
    province: '',
    ward: '',
    detail: '',
    isDefault: false
  });

  const handleOpenModal = (address?: Address) => {
    if (address) {
      setEditingAddress(address);
      setFormData({
        province: address.province,
        ward: address.ward,
        detail: address.detail,
        isDefault: address.isDefault
      });
    } else {
      setEditingAddress(null);
      setFormData({
        province: '',
        ward: '',
        detail: '',
        isDefault: false
      });
    }
    setShowModal(true);
  };

  const handleDelete = (id: string) => {
    if (confirm('Are you sure you want to remove this address?')) {
      deleteAddress(id, {
        onSuccess: () => toast.success('Address removed')
      });
    }
  };

  const handleSetDefault = (id: string) => {
    setDefaultAddress(id, {
      onSuccess: () => toast.success('Default address updated')
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    
    const action = editingAddress 
      ? () => updateAddress({ id: editingAddress.id, updates: formData }, {
          onSuccess: () => {
            toast.success('Address updated');
            setShowModal(false);
          }
        })
      : () => createAddress(formData, {
          onSuccess: () => {
            toast.success('New address added');
            setShowModal(false);
          }
        });

    action();
    setIsSubmitting(false);
  };

  if (isFetching && addresses.length === 0) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6 pb-12">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 px-1">
        <div className="space-y-0.5">
          <h1 className="text-2xl font-bold tracking-tight uppercase text-foreground">Address Book</h1>
          <p className="text-sm text-muted-foreground font-medium">Manage your shipping locations for faster checkout.</p>
        </div>
        <Button onClick={() => handleOpenModal()} className="h-11 rounded-xl px-6 font-bold uppercase tracking-widest text-[10px] bg-primary text-primary-foreground shadow-md shadow-primary/10 transition-all">
          <Plus className="mr-2 h-4 w-4" />
          New Address
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-4">
        {addresses.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-20 text-center bg-muted/20 rounded-3xl border-2 border-dashed border-border/50">
            <MapPin className="h-10 w-10 text-muted/30 mb-4" />
            <h3 className="font-bold text-base text-muted-foreground uppercase tracking-tight">No addresses saved</h3>
            <p className="text-muted-foreground/60 text-xs mt-1 font-medium italic">Add an address to speed up your checkout.</p>
          </div>
        ) : (
          addresses.map((address) => (
            <Card key={address.id} className={`border-2 transition-all rounded-2xl overflow-hidden ${address.isDefault ? 'border-primary bg-muted/30' : 'border-border/40 hover:border-border/80 bg-card'}`}>
              <CardContent className="p-5">
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-5">
                  <div className="flex items-center gap-4">
                    <div className={`h-11 w-11 rounded-xl flex items-center justify-center ${address.isDefault ? 'bg-primary text-primary-foreground' : 'bg-muted text-muted-foreground/50'}`}>
                      <MapPin className="h-5 w-5" />
                    </div>
                    <div className="space-y-1">
                      <div className="flex items-center gap-3">
                        <span className="font-bold text-sm uppercase tracking-tight text-foreground">Shipping Location</span>
                        {address.isDefault && (
                          <span className="bg-primary text-primary-foreground text-[8px] font-black px-2 py-0.5 rounded uppercase tracking-widest border-none">
                            DEFAULT
                          </span>
                        )}
                      </div>
                      <p className="text-muted-foreground text-sm font-medium leading-relaxed tracking-tight">
                        {address.detail}, {address.ward}, {address.province}
                      </p>
                    </div>
                  </div>

                  <div className="flex items-center gap-2 self-end md:self-center">
                    {!address.isDefault && (
                      <Button 
                        variant="ghost" 
                        size="sm" 
                        className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground hover:text-primary hover:bg-muted"
                        onClick={() => handleSetDefault(address.id)}
                      >
                        Set Default
                      </Button>
                    )}
                    <Button 
                      variant="ghost" 
                      size="icon" 
                      className="h-9 w-9 text-muted-foreground hover:text-primary hover:bg-muted rounded-lg"
                      onClick={() => handleOpenModal(address)}
                    >
                      <Edit3 className="h-4 w-4" />
                    </Button>
                    <Button 
                      variant="ghost" 
                      size="icon" 
                      className="h-9 w-9 text-muted-foreground hover:text-destructive hover:bg-destructive/10 rounded-lg"
                      onClick={() => handleDelete(address.id)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))
        )}
      </div>

      {/* Add/Edit Address Modal */}
      <Modal open={showModal} onOpenChange={setShowModal} title={editingAddress ? "Edit Address" : "Add New Address"}>
        <form onSubmit={handleSubmit} className="space-y-5 pt-4">
          <div className="grid grid-cols-2 gap-4">
             <div className="space-y-1.5">
                <Label htmlFor="province" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Province/City</Label>
                <Input 
                  id="province" 
                  placeholder="e.g. Ho Chi Minh" 
                  required 
                  className="h-11 rounded-xl border-border bg-background"
                  value={formData.province}
                  onChange={(e) => setFormData(prev => ({ ...prev, province: e.target.value }))}
                />
             </div>
             <div className="space-y-1.5">
                <Label htmlFor="ward" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Ward</Label>
                <Input 
                  id="ward" 
                  placeholder="e.g. Ward 1" 
                  required 
                  className="h-11 rounded-xl border-border bg-background"
                  value={formData.ward}
                  onChange={(e) => setFormData(prev => ({ ...prev, ward: e.target.value }))}
                />
             </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="detail" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Detailed Address</Label>
            <Input 
              id="detail" 
              placeholder="e.g. 123 Tech St" 
              required 
              className="h-11 rounded-xl border-border bg-background"
              value={formData.detail}
              onChange={(e) => setFormData(prev => ({ ...prev, detail: e.target.value }))}
            />
          </div>

          <div className="flex items-center gap-3 py-1 px-1">
            <input 
              type="checkbox" 
              id="isDefault" 
              className="h-4 w-4 rounded border-border text-primary focus:ring-primary cursor-pointer transition-all"
              checked={formData.isDefault}
              onChange={(e) => setFormData(prev => ({ ...prev, isDefault: e.target.checked }))}
            />
            <Label htmlFor="isDefault" className="text-xs font-bold uppercase tracking-widest text-muted-foreground cursor-pointer">Set as default</Label>
          </div>

          <div className="flex gap-3 pt-2">
            <Button variant="outline" type="button" className="flex-1 h-11 rounded-xl text-[10px] font-bold uppercase tracking-widest border-border hover:bg-muted" onClick={() => setShowModal(false)}>
              Cancel
            </Button>
            <Button type="submit" className="flex-1 h-11 rounded-xl text-[10px] font-bold uppercase tracking-widest bg-primary text-primary-foreground hover:opacity-90 shadow-md shadow-primary/10" disabled={isSubmitting}>
              {isSubmitting ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
              {editingAddress ? 'Update' : 'Add'}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
