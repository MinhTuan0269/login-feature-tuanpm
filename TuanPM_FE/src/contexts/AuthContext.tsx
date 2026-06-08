import { createContext, useContext, useState, useEffect, useRef, useCallback } from 'react';
import type { ReactNode } from 'react';
import * as signalR from '@microsoft/signalr';
import axiosClient from '../api/axiosClient';

const HUB_URL = `${import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'https://localhost:7277'}/hubs/auth`;

interface User {
  userId: string;
  fullName: string;
  email: string;
  role: string;
}

interface AuthContextType {
  user: User | null;
  login: (data: any) => Promise<void>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
  isLoading: boolean;
  forceLogoutMessage: string | null;
  clearForceLogoutMessage: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [forceLogoutMessage, setForceLogoutMessage] = useState<string | null>(null);
  const hubConnectionRef = useRef<signalR.HubConnection | null>(null);
  const currentUserIdRef = useRef<string | null>(null);

  const stopSignalRConnection = useCallback(async () => {
    if (hubConnectionRef.current) {
      if (currentUserIdRef.current && hubConnectionRef.current.state === signalR.HubConnectionState.Connected) {
        try {
          await hubConnectionRef.current.invoke("LeaveUserGroup", currentUserIdRef.current);
        } catch (err) {
          console.error("Error leaving group: ", err);
        }
      }
      await hubConnectionRef.current.stop();
      hubConnectionRef.current = null;
      currentUserIdRef.current = null;
    }
  }, []);

  // --- SignalR ---
  const startSignalRConnection = useCallback(async (userId: string, accessToken: string) => {
    // Stop existing connection before creating a new one
    if (hubConnectionRef.current) {
      await hubConnectionRef.current.stop();
      hubConnectionRef.current = null;
    }

    currentUserIdRef.current = userId;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => accessToken,
        withCredentials: true
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    // Listen for the ForceLogout event from backend
    connection.on('ForceLogout', () => {
      console.warn("Received ForceLogout event from server!");
      // Clear all auth data silently
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      setUser(null);
      
      alert('Your account has been locked or your session is invalid');
      window.location.href = '/login';
      
      // Stop the connection after forced logout
      connection.stop();
      hubConnectionRef.current = null;
      currentUserIdRef.current = null;
    });

    connection.onclose(() => {
      hubConnectionRef.current = null;
      currentUserIdRef.current = null;
    });

    try {
      await connection.start();
      console.log("SignalR Connected.");
      hubConnectionRef.current = connection;
      await connection.invoke("JoinUserGroup", userId).catch(err => console.error("Error joining group: ", err));
    } catch (err) {
      console.error('SignalR connection failed:', err);
    }
  }, []);

  // Restore session on page load
  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    const token = localStorage.getItem('accessToken');
    if (storedUser && token) {
      const parsedUser = JSON.parse(storedUser);
      setUser(parsedUser);
      // Re-establish SignalR connection after page reload
      startSignalRConnection(parsedUser.userId, token);
    }
    setIsLoading(false);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      stopSignalRConnection();
    };
  }, [stopSignalRConnection]);

  const login = async (credentials: any) => {
    const response = await axiosClient.post('/auth/login', credentials);
    if (response.data.error) {
      throw new Error(response.data.error);
    }

    const { userId, fullName, email, role, accessToken, refreshToken } = response.data.result;

    const userData = { userId, fullName, email, role };
    setUser(userData);
    localStorage.setItem('user', JSON.stringify(userData));
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);

    // Start SignalR after successful login
    await startSignalRConnection(userId, accessToken);
  };

  const logout = async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    // Stop SignalR first
    await stopSignalRConnection();
    try {
      if (refreshToken) {
        await axiosClient.post('/auth/logout', { refreshToken });
      }
    } catch (error) {
      console.error('Logout API failed', error);
    } finally {
      setUser(null);
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
    }
  };

  const clearForceLogoutMessage = () => setForceLogoutMessage(null);

  return (
    <AuthContext.Provider
      value={{ user, login, logout, isAuthenticated: !!user, isLoading, forceLogoutMessage, clearForceLogoutMessage }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
