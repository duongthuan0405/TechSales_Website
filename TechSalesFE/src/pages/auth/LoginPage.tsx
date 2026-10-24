import { useState } from 'react';
import { useNavigate, Link } from 'react-router';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '../../components/ui/card';
import { useAuth } from '../../context/AuthContext';
import { useLoginMutation } from '../../dataHook/authDataHook';
import { ShoppingBag, Loader2, KeyRound, AlertCircle } from 'lucide-react';
import { toast } from 'sonner';

export function LoginPage() {
  const navigate = useNavigate();
  const { mutate: login, isPending: isLoading, error: loginError } = useLoginMutation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const [failedAttempts, setFailedAttempts] = useState(0);
  const isLocked = failedAttempts >= 5;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (isLocked) {
      toast.error('Account temporarily locked. Please try again in 30 minutes.');
      return;
    }

    login({ email, password }, {
      onSuccess: () => {
        toast.success('Welcome back!');
        navigate('/');
      },
      onError: (err: any) => {
        setFailedAttempts(prev => prev + 1);
        toast.error(err.message || 'Login failed');
      }
    });
  };



  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4 dark:from-slate-950 dark:to-slate-900">
      <div className="w-full max-w-md space-y-8">
        <div className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-600 shadow-lg shadow-blue-500/20">
            <ShoppingBag className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-4xl font-bold tracking-tight">TechSales</h1>
          <p className="mt-2 text-muted-foreground">Premium Technology Marketplace</p>
        </div>

        <Card className="border-none shadow-2xl">
          <CardHeader>
            <CardTitle>Welcome Back</CardTitle>
            <CardDescription>Sign in to continue your journey</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              {isLocked && (
                <div className="flex items-center gap-3 rounded-xl bg-destructive/10 p-4 text-sm text-destructive border border-destructive/20 animate-in fade-in slide-in-from-top-1 duration-300">
                  <AlertCircle className="h-5 w-5 shrink-0" />
                  <div>
                    <p className="font-bold uppercase tracking-tight text-[10px]">Security Lockout</p>
                    <p className="font-medium">Too many failed attempts. Account locked for 30 minutes.</p>
                  </div>
                </div>
              )}
              
              <div className="space-y-2">
                <Label htmlFor="email">Email Address</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="john@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  disabled={isLocked}
                  required
                />
              </div>
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label htmlFor="password">Password</Label>
                  <Link to="/auth/forgot-password" className="text-xs font-medium text-blue-600 hover:underline">
                    Forgot password?
                  </Link>
                </div>
                <Input
                  id="password"
                  type="password"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  disabled={isLocked}
                  required
                />
              </div>
              
              {loginError && !isLocked && (
                <div className="flex items-center gap-2 rounded-lg bg-destructive/10 p-3 text-sm text-destructive">
                  <KeyRound className="h-4 w-4" />
                  {(loginError as any).message || 'Login failed'}
                </div>
              )}

              <Button type="submit" className="w-full h-12 text-lg font-bold" disabled={isLoading || isLocked}>
                {isLoading ? <Loader2 className="mr-2 h-5 w-5 animate-spin" /> : null}
                {isLocked ? 'Account Locked' : (isLoading ? 'Signing In...' : 'Sign In')}
              </Button>
            </form>

            <div className="mt-6 border-t border-border/50 pt-4">
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-2 text-center">
                Demo Accounts (Click to Fill)
              </p>
              <div className="grid grid-cols-2 gap-2">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="text-xs justify-start h-9"
                  onClick={() => {
                    setEmail('customer@email.com');
                    setPassword('password');
                  }}
                >
                  <span className="h-2 w-2 rounded-full bg-emerald-500 mr-1.5 shrink-0" />
                  Customer
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="text-xs justify-start h-9"
                  onClick={() => {
                    setEmail('sales@techsales.com');
                    setPassword('password');
                  }}
                >
                  <span className="h-2 w-2 rounded-full bg-blue-500 mr-1.5 shrink-0" />
                  Staff
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="text-xs justify-start h-9"
                  onClick={() => {
                    setEmail('business@techsales.com');
                    setPassword('password');
                  }}
                >
                  <span className="h-2 w-2 rounded-full bg-purple-500 mr-1.5 shrink-0" />
                  Business Admin
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="text-xs justify-start h-9"
                  onClick={() => {
                    setEmail('admin@techsales.com');
                    setPassword('password');
                  }}
                >
                  <span className="h-2 w-2 rounded-full bg-rose-500 mr-1.5 shrink-0" />
                  Technical Admin
                </Button>
              </div>
            </div>
          </CardContent>
          <CardFooter className="flex justify-center border-t border-border/50 pt-6">
            <div className="text-sm">
              Don't have an account?{' '}
              <Link to="/auth/signup" className="font-bold text-blue-600 hover:underline">
                Create Account
              </Link>
            </div>
          </CardFooter>
        </Card>
      </div>
    </div>
  );
}
