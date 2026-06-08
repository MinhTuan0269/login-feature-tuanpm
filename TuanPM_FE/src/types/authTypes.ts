export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  roleId: number; 
}

export interface ApiResponse<T> {
  message: string | null;
  error: string | null;
  result: T;
}

export interface TokenStatusResponse {
  accessTokenExpiresAt: string;
  accessTokenRemainingSeconds: number;
  refreshTokenExpiresAt: string;
  refreshTokenRemainingSeconds: number;
}
