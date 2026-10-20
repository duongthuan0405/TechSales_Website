import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { useAuth } from '../../context/AuthContext';
import { User, Mail, Phone, Camera, Save, Loader2, ShieldCheck } from 'lucide-react';
import { toast } from 'sonner';

export function ProfilePage() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({
    name: user?.name || '',
    email: user?.email || '',
    phone: '0987654321', // Mock data
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData(prev => ({ ...prev, [e.target.id]: e.target.value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    await new Promise(resolve => setTimeout(resolve, 1000));
    setIsLoading(false);
    toast.success('Profile updated successfully');
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6 pb-12">
      <div className="space-y-0.5 px-1">
        <h1 className="text-2xl font-bold tracking-tight uppercase text-foreground">Personal Information</h1>
        <p className="text-sm text-muted-foreground font-medium">Manage your profile details and contact information.</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Avatar Sidebar */}
        <Card className="md:col-span-1 border-border shadow-sm bg-card overflow-hidden rounded-2xl">
          <CardContent className="pt-10 pb-6 flex flex-col items-center">
            <div className="relative group cursor-pointer">
              <div className="h-28 w-28 rounded-full border border-border bg-muted/30 flex items-center justify-center overflow-hidden transition-all group-hover:border-primary">
                <User className="h-14 w-14 text-muted-foreground/50" />
              </div>
              <div className="absolute bottom-0 right-0 h-9 w-9 rounded-full bg-primary text-primary-foreground flex items-center justify-center shadow-md transition-transform group-hover:scale-110 border-2 border-card">
                <Camera className="h-4 w-4" />
              </div>
            </div>
            <h3 className="mt-5 text-lg font-bold text-foreground uppercase tracking-tight">{formData.name}</h3>
            <p className="text-xs font-bold text-muted-foreground uppercase tracking-widest">{user?.role}</p>
            
            <div className="mt-6 w-full space-y-2 px-2">
               <div className="flex items-center gap-3 text-xs py-2.5 px-3 rounded-xl bg-muted/20 border border-border/30">
                  <Mail className="h-3.5 w-3.5 text-muted-foreground" />
                  <span className="truncate font-medium text-muted-foreground">{formData.email}</span>
               </div>
               <div className="flex items-center gap-3 text-xs py-2.5 px-3 rounded-xl bg-muted/20 border border-border/30">
                  <Phone className="h-3.5 w-3.5 text-muted-foreground" />
                  <span className="font-medium text-muted-foreground">{formData.phone}</span>
               </div>
            </div>

            <div className="mt-6 pt-4 border-t border-border w-full px-2">
               <Button 
                variant="ghost" 
                className="w-full justify-start text-[10px] font-bold uppercase tracking-widest text-muted-foreground hover:text-foreground hover:bg-muted"
                onClick={() => navigate('/customer/change-password')}
               >
                  <ShieldCheck className="mr-2 h-4 w-4" />
                  Security
               </Button>
            </div>
          </CardContent>
        </Card>

        {/* Edit Form */}
        <Card className="md:col-span-2 border-border shadow-sm bg-card rounded-2xl overflow-hidden">
          <CardHeader className="p-6">
            <CardTitle className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Edit Profile</CardTitle>
            <CardDescription className="text-xs font-medium">Update your public display name and contact details.</CardDescription>
          </CardHeader>
          <CardContent className="p-6 pt-0">
            <form onSubmit={handleSubmit} className="space-y-5">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <Label htmlFor="name" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Full Name</Label>
                  <Input 
                    id="name" 
                    placeholder="Full Name" 
                    value={formData.name}
                    onChange={handleChange}
                    className="h-11 rounded-xl bg-background border-border"
                  />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="phone" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Phone Number</Label>
                  <Input 
                    id="phone" 
                    placeholder="Phone Number" 
                    value={formData.phone}
                    onChange={handleChange}
                    className="h-11 rounded-xl bg-background border-border"
                  />
                </div>
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="email" className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground ml-1">Email Address</Label>
                <Input 
                  id="email" 
                  type="email" 
                  disabled
                  value={formData.email}
                  className="h-11 rounded-xl bg-muted/40 cursor-not-allowed opacity-80 border-border text-muted-foreground font-medium"
                />
                <p className="text-[9px] font-medium text-muted-foreground italic px-1 tracking-tight">Email is linked to your account and cannot be modified.</p>
              </div>

              <div className="pt-4 flex justify-end">
                <Button 
                  type="submit" 
                  className="h-11 px-8 rounded-xl font-bold uppercase tracking-widest text-[10px] bg-primary text-primary-foreground hover:opacity-90 transition-all shadow-md shadow-primary/10"
                  disabled={isLoading}
                >
                  {isLoading ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <Save className="mr-2 h-4 w-4" />}
                  Save Changes
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
