import axios from 'axios';
import { PasswordEntry, User } from '../types';
import { encrypt, decrypt } from './cryptography';

const API_URL = 'https://localhost:7249/api/';


// Demo implementation with localStorage
// A real implementation would use a secure storage mechanism

// Password vault storage
export const saveVault = (passwords: PasswordEntry[]): void => {
  try {
    const encryptedData = encrypt(JSON.stringify(passwords));
    localStorage.setItem('secureVault', encryptedData);
  } catch (error) {
    console.error('Failed to save vault:', error);
  }
};

export const loadVault = (): PasswordEntry[] => {
  try {
    
    const  response = axios.get(`${API_URL}Passwords`)
    
    const encryptedData = localStorage.getItem('secureVault');
    if (!encryptedData) return [];
    
    const decryptedData = decrypt(encryptedData);
    return JSON.parse(decryptedData);
    // return response;
  } catch (error) {
    console.error('Failed to load vault:', error);
    return [];
  }
};

// User storage for demo purposes
export const saveUser = (user: User): void => {
  localStorage.setItem('user', JSON.stringify(user));
};

export const loadUser = (): User | null => {
  const data = localStorage.getItem('user');
  if (!data) return null;
  return JSON.parse(data);
};

export const clearStorage = (): void => {
  localStorage.removeItem('secureVault');
  localStorage.removeItem('user');
  localStorage.removeItem('authState');
};

// Auth state persistence
export const saveAuthState = (isAuthenticated: boolean, mfaVerified: boolean): void => {
  localStorage.setItem('authState', JSON.stringify({ isAuthenticated, mfaVerified }));
};

export const loadAuthState = (): { isAuthenticated: boolean; mfaVerified: boolean } => {
  const data = localStorage.getItem('authState');
  if (!data) return { isAuthenticated: false, mfaVerified: false };
  return JSON.parse(data);
};