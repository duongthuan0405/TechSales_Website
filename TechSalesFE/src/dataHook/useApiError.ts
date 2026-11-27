import { useEffect } from 'react';
import { toast } from 'sonner';

export function useApiError(error: Error | null) {
  useEffect(() => {
    if (error) toast.error(error.message);
  }, [error]);
}
