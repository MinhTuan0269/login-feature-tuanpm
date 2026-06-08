import { useState, useEffect, useCallback } from 'react';
import { userApi } from '../api/userApi';
import type { PaginatedUsers } from '../api/userApi';

export const useUsers = (initialPageSize = 10) => {
  const [data, setData] = useState<PaginatedUsers | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  
  const [pageNumber, setPageNumber] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(initialPageSize);

  const fetchUsers = useCallback(async (): Promise<void> => {
    try {
      setIsLoading(true);
      setError(null);
      const result = await userApi.getUsers(pageNumber, pageSize);
      setData(result);
    } catch (err: any) {
      setError(err.message || err.response?.data?.error || 'Failed to load users');
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, pageSize]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  const toggleLock = async (id: string): Promise<boolean> => {
    if (!id) throw new Error("Invalid User ID");
    try {
      await userApi.toggleLockUser(id);
      // Reload the current page data
      await fetchUsers();
      return true;
    } catch (err: any) {
      const msg = err.message || err.response?.data?.error || 'Failed to toggle lock status';
      throw new Error(msg);
    }
  };

  return {
    data,
    isLoading,
    error,
    pageNumber,
    pageSize,
    setPageNumber,
    setPageSize,
    toggleLock,
    refetch: fetchUsers
  };
};
