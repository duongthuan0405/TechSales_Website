import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router';
import { Button } from '../../components/ui/button';
import { Card, CardDescription, CardHeader, CardTitle } from '../../components/ui/card';
import { useVerifyEmailMutation } from '../../dataHook/authDataHook';
import { ShoppingBag, Loader2, CheckCircle2, XCircle, Mail } from 'lucide-react';
import { toast } from 'sonner';

export function VerifyEmailPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  
  const { mutate: verifyEmail, isPending: isLoading } = useVerifyEmailMutation();
  const [status, setStatus] = useState<'pending' | 'success' | 'error'>('pending');

  useEffect(() => {
    if (token) {
      verifyEmail(token, {
        onSuccess: () => {
          setStatus('success');
          toast.success('Email verified successfully!');
        },
        onError: () => {
          setStatus('error');
          toast.error('Email verification failed.');
        }
      });
    } else {
      setStatus('error');
    }
  }, [token, verifyEmail]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4 dark:from-slate-950 dark:to-slate-900">
      <div className="w-full max-w-md space-y-8 text-center">
        <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-600 shadow-lg shadow-blue-500/20">
          <ShoppingBag className="h-8 w-8 text-white" />
        </div>

        <Card className="border-none shadow-2xl p-8">
          {status === 'pending' && (
            <div className="space-y-6">
              <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-blue-100 text-blue-600 dark:bg-blue-900/30">
                <Loader2 className="h-10 w-10 animate-spin" />
              </div>
              <CardHeader className="p-0">
                <CardTitle className="text-2xl">Verifying your email</CardTitle>
                <CardDescription className="text-base mt-2">
                  Please wait a moment while we validate your credentials...
                </CardDescription>
              </CardHeader>
            </div>
          )}

          {status === 'success' && (
            <div className="space-y-6">
              <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-emerald-100 text-emerald-600 dark:bg-emerald-900/30">
                <CheckCircle2 className="h-10 w-10" />
              </div>
              <CardHeader className="p-0">
                <CardTitle className="text-2xl">Account Activated</CardTitle>
                <CardDescription className="text-base mt-2">
                  Your email has been successfully verified. You can now enjoy all our member benefits.
                </CardDescription>
              </CardHeader>
              <Button className="w-full h-12 text-lg font-bold" onClick={() => navigate('/auth/login')}>
                Sign In Now
              </Button>
            </div>
          )}

          {status === 'error' && (
            <div className="space-y-6">
              <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-destructive/10 text-destructive">
                <XCircle className="h-10 w-10" />
              </div>
              <CardHeader className="p-0">
                <CardTitle className="text-2xl">Verification Failed</CardTitle>
                <CardDescription className="text-base mt-2">
                  The link is invalid or has expired. Please try requesting a new verification email.
                </CardDescription>
              </CardHeader>
              <div className="grid grid-cols-1 gap-3 pt-4">
                <Button className="h-12 text-lg font-bold" onClick={() => navigate('/auth/login')}>
                  Go to Login
                </Button>
                <Button variant="outline" className="h-12 text-lg font-bold" onClick={() => toast.info('New link requested')}>
                  <Mail className="mr-2 h-5 w-5" />
                  Resend Email
                </Button>
              </div>
            </div>
          )}
        </Card>
      </div>
    </div>
  );
}
