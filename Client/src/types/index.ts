export interface User {
  id: string;
  email: string;
  name: string;
  phone:string;
  mfaEnabled: boolean;
}

export interface PasswordEntry {
  id: string;
  title: string;
  username: string;
  Password: string;
  SiteId: string;
  url:string
  category: string;
  notes?: string;
  createdAt: number;
  LastDateUse: number;
  favorite: boolean;
  strength: number;
}

export interface Category {
  id: string;
  name: string;
  icon?: string;
  count: number;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  isLoading: boolean;
  error: string | null;
  mfaRequired: boolean;
  mfaVerified: boolean;
  accessToken: string | null;
  tokenExpiry: Date | null;
}

export interface PasswordGeneratorOptions {
  length: number;
  includeUppercase: boolean;
  includeLowercase: boolean;
  includeNumbers: boolean;
  includeSymbols: boolean;
  excludeSimilarChars: boolean;
}