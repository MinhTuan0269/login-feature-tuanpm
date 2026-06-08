import axiosClient from './axiosClient';
import type { RegisterRequest, ApiResponse, TokenStatusResponse } from '../types/authTypes';

export const authApi = {
  register: async (data: RegisterRequest): Promise<ApiResponse<boolean>> => {
    const response = await axiosClient.post('/auth/register', data);
    return response.data;
  },
  getTokenStatus: async (): Promise<ApiResponse<TokenStatusResponse>> => {
    const response = await axiosClient.get('/auth/token-status');
    return response.data;
  }
};
