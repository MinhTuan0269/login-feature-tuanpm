import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authApi } from '../api/authApi';
import { Mail, Lock, User as UserIcon, AlertCircle, ShieldAlert, Eye, EyeOff } from 'lucide-react';

const LoginPage = (): JSX.Element => {
  const [isLoginView, setIsLoginView] = useState<boolean>(true);
  
  const [fullName, setFullName] = useState<string>('');
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [showPassword, setShowPassword] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [isLocked, setIsLocked] = useState<boolean>(false);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  
  const { login, forceLogoutMessage, clearForceLogoutMessage } = useAuth();
  const navigate = useNavigate();

  // When a ForceLogout happens, clear it after user reads it
  useEffect(() => {
    return () => {
      clearForceLogoutMessage();
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const validatePassword = (pass: string): boolean => {
    // Min 12 characters, at least 1 uppercase, at least 1 special character
    const regex = /^(?=.*[A-Z])(?=.*[!@#$%^&*()_+{}[\]:;"'<>,.?/\\|`~-]).{12,}$/;
    return regex.test(pass);
  };

  const validateForm = (): boolean => {
    if (!isLoginView && !fullName) {
      setError('Please enter your full name.');
      return false;
    }
    if (!email || !password) {
      setError('Please enter both email and password.');
      return false;
    }
    if (!validatePassword(password)) {
      setError('Password must be at least 12 characters, including 1 uppercase letter and 1 special character.');
      return false;
    }
    return true;
  };

  const handleToggleView = (): void => {
    setIsLoginView(!isLoginView);
    setError(null);
    setIsLocked(false);
    setPassword('');
    // Optionally clear email and fullName too
  };

  const handleSubmit = async (e: React.FormEvent): Promise<void> => {
    e.preventDefault();
    setError(null);
    setIsLocked(false);

    if (!validateForm()) return;

    try {
      setIsLoading(true);
      
      if (isLoginView) {
        // Login Flow
        await login({ email, password });
        navigate('/');
      } else {
        // Register Flow
        const res = await authApi.register({
          fullName,
          email,
          password,
          roleId: 3 // Default Role User
        });
        
        if (res.result) {
          alert(res.message || "Registration successful!");
          // Switch to login view
          setIsLoginView(true);
          setPassword('');
        } else {
          setError(res.error || "Registration failed. Please try again.");
        }
      }
    } catch (err: any) {
      const backendError = err.response?.data?.error || err.response?.data?.message;
      const errorMessage = backendError || err.message || (isLoginView ? 'Login failed. Please check your credentials.' : 'Registration failed. Please try again.');
      
      if (errorMessage.toLowerCase().includes('lock') || errorMessage.includes('bị khóa')) {
        setIsLocked(true);
        setError('Your account has been locked due to multiple failed login attempts');
      } else {
        setError(errorMessage);
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 via-white to-blue-100 p-4 overflow-hidden relative">
      {/* Decorative background elements */}
      <div className="absolute top-[-10%] left-[-10%] w-96 h-96 bg-blue-200 rounded-full mix-blend-multiply filter blur-[128px] opacity-70 animate-blob"></div>
      <div className="absolute top-[20%] right-[-10%] w-96 h-96 bg-cyan-200 rounded-full mix-blend-multiply filter blur-[128px] opacity-70 animate-blob animation-delay-2000"></div>
      <div className="absolute bottom-[-20%] left-[20%] w-96 h-96 bg-sky-200 rounded-full mix-blend-multiply filter blur-[128px] opacity-70 animate-blob animation-delay-4000"></div>

      <div className="relative w-full max-w-md">
        <div className="bg-white/80 backdrop-blur-xl rounded-3xl p-8 shadow-2xl border border-white/50">
          <div className="text-center mb-8">
            <h1 className="text-3xl font-bold text-gray-800 mb-2">
              {isLoginView ? 'Welcome Back' : 'Create an Account'}
            </h1>
            <p className="text-gray-500">
              {isLoginView ? 'Log in to continue' : 'Sign up to get started'}
            </p>
          </div>

          {/* Force Logout Banner (from SignalR) */}
          {forceLogoutMessage && isLoginView && (
            <div className="mb-6 p-4 rounded-xl bg-red-50 border border-red-200 flex items-start space-x-3">
              <ShieldAlert className="text-red-500 w-6 h-6 mt-0.5 flex-shrink-0" />
              <p className="text-red-700 text-sm font-medium leading-relaxed">
                {forceLogoutMessage}
              </p>
            </div>
          )}

          {isLocked && (
            <div className="mb-6 p-4 rounded-xl bg-red-50 border border-red-200 flex items-start space-x-3">
              <AlertCircle className="text-red-500 w-6 h-6 mt-0.5 flex-shrink-0" />
              <p className="text-red-700 text-sm font-medium leading-relaxed">
                {error}
              </p>
            </div>
          )}

          {!isLocked && error && (
            <div className="mb-6 p-4 rounded-xl bg-orange-50 border border-orange-200 flex items-start space-x-3">
              <AlertCircle className="text-orange-500 w-5 h-5 mt-0.5 flex-shrink-0" />
              <p className="text-orange-700 text-sm font-medium">{error}</p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            {!isLoginView && (
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Full Name</label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                    <UserIcon className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    type="text"
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    className="block w-full pl-11 pr-4 py-3 bg-white border border-gray-300 rounded-xl text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-300 shadow-sm"
                    placeholder="John Doe"
                    required={!isLoginView}
                  />
                </div>
              </div>
            )}

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Email</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                  <Mail className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="block w-full pl-11 pr-4 py-3 bg-white border border-gray-300 rounded-xl text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-300 shadow-sm"
                  placeholder="user@example.com"
                  required
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Password</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                  <Lock className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  type={showPassword ? 'text' : 'password'}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="block w-full pl-11 pr-12 py-3 bg-white border border-gray-300 rounded-xl text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-300 shadow-sm"
                  placeholder="Enter password"
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-4 flex items-center text-gray-400 hover:text-gray-600 transition-colors"
                >
                  {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                </button>
              </div>
              <p className="mt-2 text-xs text-gray-500">
                Min 12 characters, 1 uppercase, 1 special character
              </p>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className={`w-full py-3.5 px-4 border border-transparent rounded-xl shadow-lg text-sm font-bold text-white 
                ${isLoading 
                  ? 'bg-blue-400 cursor-not-allowed' 
                  : 'bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 transform hover:-translate-y-0.5 transition-all duration-300'
                }`}
            >
              {isLoading ? (
                <div className="flex items-center justify-center">
                  <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></div>
                  Processing...
                </div>
              ) : (
                isLoginView ? 'Log In' : 'Sign Up'
              )}
            </button>
            
            <div className="text-center mt-4">
              <button
                type="button"
                onClick={handleToggleView}
                className="text-sm font-medium text-blue-600 hover:text-blue-500 transition-colors"
              >
                {isLoginView ? "Don't have an account? Sign up" : "Already have an account? Log in"}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
