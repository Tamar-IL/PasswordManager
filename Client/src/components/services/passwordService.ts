import axios from 'axios';
import { PasswordEntry } from '../../types';

const API_URL = 'https://localhost:7249/api/';

export const getPasswords = async () => {
  const response = await axios.get(`${API_URL}passwords`);
  return response.data;
};
export const addPassword = async (password: any) => {
  const response = await axios.post(`${API_URL}Passwords`, password);
  return response.data;
};

export const updatePassword = async (id: string, passwordData: any) => {
  const response = await axios.put(`${API_URL}Passwords${id}`);
  return response.data;
};

export const deletePassword = async (id: string) => {
  const response = await axios.delete(`${API_URL}Passwords${id}`);
  return response.data;
};
