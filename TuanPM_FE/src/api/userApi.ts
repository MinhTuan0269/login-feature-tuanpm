import axiosClient from './axiosClient';

export interface UserItem {
  id: string;
  fullName: string;
  email: string;
  roleId: number;
  roleName: string;
  isLocked: boolean;
  lastLoginAt: string | null;
  createdAt: string;
}

export interface PaginatedUsers {
  items: UserItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export const userApi = {
  getUsers: async (pageNumber: number, pageSize: number): Promise<PaginatedUsers> => {
    const response = await axiosClient.get(`/users?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    return response.data.result;
  },

  getUserById: async (id: string): Promise<UserItem> => {
    if (!id) throw new Error("User ID is required");
    const response = await axiosClient.get(`/users/${id}`);
    return response.data.result;
  },

  toggleLockUser: async (id: string): Promise<boolean> => {
    if (!id) throw new Error("User ID is required");
    const response = await axiosClient.put(`/users/${id}/lock`);
    return response.data.result;
  }
};
