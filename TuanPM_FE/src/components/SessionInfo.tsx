import React, { useState, useEffect, useRef } from 'react';
import { Clock, Key, RefreshCw } from 'lucide-react';
import { authApi } from '../api/authApi';
import type { TokenStatusResponse } from '../types/authTypes';

const formatSeconds = (totalSeconds: number): string => {
  if (totalSeconds <= 0) return "Expired";
  
  const days = Math.floor(totalSeconds / 86400);
  const hours = Math.floor((totalSeconds % 86400) / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = Math.floor(totalSeconds % 60);
  
  if (days > 0) return `${days} days ${hours} hours`;
  if (hours > 0) return `${hours} hours ${minutes} mins`;
  if (minutes > 0) return `${minutes} mins ${seconds} secs`;
  return `${seconds} secs`;
};

const SessionInfo = (): JSX.Element => {
  const [tokenStatus, setTokenStatus] = useState<TokenStatusResponse | null>(null);
  const [accessRemaining, setAccessRemaining] = useState<number>(0);
  const [refreshRemaining, setRefreshRemaining] = useState<number>(0);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  
  // Use a ref to keep track of precise elapsed time since API call
  const lastFetchTimeRef = useRef<number>(Date.now());

  const fetchTokenStatus = async (): Promise<void> => {
    try {
      setIsLoading(true);
      const res = await authApi.getTokenStatus();
      if (res.result) {
        setTokenStatus(res.result);
        setAccessRemaining(res.result.accessTokenRemainingSeconds);
        setRefreshRemaining(res.result.refreshTokenRemainingSeconds);
        lastFetchTimeRef.current = Date.now();
      }
    } catch (error) {
      console.error("Failed to fetch token status", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchTokenStatus();

    const handleTokenRefreshed = () => {
      fetchTokenStatus();
    };

    window.addEventListener('auth:tokenRefreshed', handleTokenRefreshed);

    return () => {
      window.removeEventListener('auth:tokenRefreshed', handleTokenRefreshed);
    };
  }, []);

  useEffect(() => {
    if (!tokenStatus) return;

    const interval = setInterval(() => {
      const elapsedSeconds = (Date.now() - lastFetchTimeRef.current) / 1000;
      setAccessRemaining(Math.max(0, tokenStatus.accessTokenRemainingSeconds - elapsedSeconds));
      setRefreshRemaining(Math.max(0, tokenStatus.refreshTokenRemainingSeconds - elapsedSeconds));
    }, 1000);

    return () => clearInterval(interval);
  }, [tokenStatus]);

  return (
    <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 mt-6 relative overflow-hidden">
      <div className="absolute top-0 right-0 -mt-4 -mr-4 w-24 h-24 bg-green-50 rounded-full blur-2xl opacity-60"></div>
      
      <div className="flex items-center justify-between mb-6 relative">
        <div className="flex items-center space-x-2">
          <div className="w-3 h-3 bg-green-500 rounded-full animate-pulse shadow-[0_0_8px_rgba(34,197,94,0.6)]"></div>
          <h3 className="text-lg font-semibold text-gray-900">Session Active</h3>
        </div>
        <button 
          onClick={fetchTokenStatus}
          disabled={isLoading}
          className="p-2 text-gray-400 hover:text-indigo-600 transition-colors rounded-full hover:bg-indigo-50"
          title="Refresh status"
        >
          <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
        </button>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 relative">
        <div className="p-5 bg-gradient-to-br from-indigo-50 to-blue-50 rounded-xl border border-indigo-100/50">
          <div className="flex items-center text-indigo-800 mb-2 font-semibold">
            <Key className="w-4 h-4 mr-2" />
            Access Token
          </div>
          <p className="text-sm text-gray-600">
            Expires in: <span className="font-mono text-indigo-900 font-bold ml-1">
              {isLoading && !tokenStatus ? "..." : formatSeconds(accessRemaining)}
            </span>
          </p>
        </div>
        
        <div className="p-5 bg-gradient-to-br from-purple-50 to-pink-50 rounded-xl border border-purple-100/50">
          <div className="flex items-center text-purple-800 mb-2 font-semibold">
            <Clock className="w-4 h-4 mr-2" />
            Refresh Token
          </div>
          <p className="text-sm text-gray-600">
            Expires in: <span className="font-mono text-purple-900 font-bold ml-1">
              {isLoading && !tokenStatus ? "..." : formatSeconds(refreshRemaining)}
            </span>
          </p>
        </div>
      </div>
    </div>
  );
};

export default SessionInfo;
