import axios from 'axios';
import { PasswordEntry } from '../../types';

const API_URL = 'https://localhost:7249/api/';

export const Login = async () => {
  const response = await axios.get(`${API_URL}users`);
  return response.data;
};
export const Register = async () => {
  const response = await axios.post(`${API_URL}users`);
  return response.data;
};


