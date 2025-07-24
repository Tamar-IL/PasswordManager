import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { PasswordEntry, Category } from '../types';
import { useAuth } from './AuthContext';
import { generatePassword } from '../utils/cryptography';
import axios from 'axios';
import JSEncrypt from 'jsencrypt';
import { GlobalRSAKeys } from '../utils/globalRSAKeys';

const API_URL = 'https://localhost:7249/api/';

export const useCurrentUser = () => {
  const { authState } = useAuth();
  return authState.user;
};

interface PasswordContextProps {
  passwords: PasswordEntry[];
  categories: Category[];
  isLoading: boolean;
  addPassword: (password: Omit<PasswordEntry, 'id' | 'createdAt' | 'updatedAt'>) => void;
  updatePassword: (id: string, updates: Partial<PasswordEntry>) => void;
  deletePassword: (id: string) => void;
  generateNewPassword: (options: {
    length: number;
    includeUppercase: boolean;
    includeLowercase: boolean;
    includeNumbers: boolean;
    includeSymbols: boolean;
    excludeSimilarChars: boolean;
  }) => string;
  searchPasswords: (query: string) => PasswordEntry[];
  toggleFavorite: (id: string) => void;
}

const PasswordContext = createContext<PasswordContextProps>({
  passwords: [],
  categories: [],
  isLoading: false,
  addPassword: () => {},
  updatePassword: () => {},
  deletePassword: () => {},
  generateNewPassword: () => '',
  searchPasswords: () => [],
  toggleFavorite: () => {},
});

export const usePasswords = () => useContext(PasswordContext);

export const PasswordProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [passwords, setPasswords] = useState<PasswordEntry[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const { authState } = useAuth();

useEffect(() => {
  const fetchPasswords = async () => {
    
    const currentUser = authState.user;
    if (!currentUser) {
      throw new Error('משתמש לא מחובר');
    }
    if (authState.isAuthenticated && authState.mfaVerified) {
      setIsLoading(true);
      try {
        const publicKeyEncoded = encodeURIComponent(GlobalRSAKeys.getPublicKey());
const response = await axios.get(`${API_URL}Passwords/byemail/${currentUser.id}`, {
  headers: {
    Authorization: `Bearer ${authState.accessToken}`
  }
});             
        setPasswords(response.data);
      } catch (error) {
        console.error('שגיאה בטעינת סיסמאות :', error);
      } finally {
        setIsLoading(false);
      }
    } else {
      setPasswords([]);
    }
  };

  fetchPasswords();
}, [authState.isAuthenticated, authState.mfaVerified, authState.user?.email]);

  // Update categories when passwords change
  useEffect(() => {
    const categoryMap = new Map<string, { name: string; count: number }>();
    
    // Count passwords by category
    passwords.forEach(password => {
      const category = password.category || 'Uncategorized';
      if (categoryMap.has(category)) {
        const existing = categoryMap.get(category)!;
        categoryMap.set(category, { ...existing, count: existing.count + 1 });
      } else {
        categoryMap.set(category, { name: category, count: 1 });
      }
    });
    
    // Add default categories if they don't exist
    const defaultCategories = ['Social', 'Work', 'Finance', 'Shopping', 'Personal'];
    defaultCategories.forEach(category => {
      if (!categoryMap.has(category)) {
        categoryMap.set(category, { name: category, count: 0 });
      }
    });
    
    // Convert map to array of categories
    const newCategories: Category[] = Array.from(categoryMap.entries()).map(([id, { name, count }]) => ({
      id,
      name,
      count,
    }));
    
    setCategories(newCategories);
  }, [passwords]);

const getPublicKey = async () => {
  const response = await axios.get(`${API_URL}RSA/public-key`);
  return response.data.publicKey;
};

const encryptPassword = async (password: string) => {
  try {
    
    const publicKey = await getPublicKey();
    const encrypt = new JSEncrypt();
    encrypt.setPublicKey(publicKey);
    const encrypted = encrypt.encrypt(password);   
    return encrypted;
  }
   catch (error) {
    console.error('Encryption error:', error);
    throw error;
  }
};

  const addPassword = async (passwordData: Omit<PasswordEntry, 'id' | 'createdAt' | 'updatedAt'>) => {
    try {

    const encryptedPassword = await encryptPassword(passwordData.Password);

    const currentUser = authState.user;
    if (!currentUser) {
      throw new Error('משתמש לא מחובר');
    }
      // שליחת בקשה ליצירת רשומה חדשה בשרת
      const passwordNew = {
            id: "",
            UserId: currentUser.id,
            SiteId: "",
            DateReg: "",
            LastDateUse: "",
            Password: encryptedPassword,
            VP:""}
      const response = await axios.post(`${API_URL}Passwords/?url=${encodeURIComponent(passwordData.url)}`, passwordNew);
      usePasswords();
      
      return response.data;
    }
    catch (error) {
    console.error('שגיאה בהוספת סיסמה:', error);
    throw error;
   }
 };

  const updatePassword = async (id: string, updates: Partial<PasswordEntry>) => {
    try {
    const response = await axios.put(`${API_URL}Passwords/${id}`);
     setPasswords(prev =>
      prev.map(password =>
        password.id === id
          ? { ...password, ...updates, updatedAt: Date.now() }
          : password
      )

    );
    
  } catch (error: unknown) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 400) {
        console.error('Invalid ObjectId format');
      } else if (error.response?.status === 404) {
        console.error('Password not found');
      } else {
        console.error('Error deleting password:', error.message);
      }
    } else {
      console.error('Unexpected error:', error);
    }
    throw error;
  }
  };

  const deletePassword = async (id: string): Promise<void> => {
  try {
    const response = await axios.delete(`${API_URL}Passwords/${id}`);
    
    // רק אחרי הצלחת המחיקה - עדכון הסטייט
    setPasswords(prev => prev.filter(password => password.id !== id));
    console.log('Password deleted successfully');
    
  } catch (error: unknown) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 400) {
        console.error('Invalid ObjectId format');
      } else if (error.response?.status === 404) {
        console.error('Password not found');
      } else {
        console.error('Error deleting password:', error.message);
      }
    } else {
      console.error('Unexpected error:', error);
    }
    throw error;
  }
};

  const generateNewPassword = (options: {
    length: number;
    includeUppercase: boolean;
    includeLowercase: boolean;
    includeNumbers: boolean;
    includeSymbols: boolean;
    excludeSimilarChars: boolean;
  }) => {
    return generatePassword(options);
  };

  const searchPasswords = (query: string) => {
    if (!query) return passwords;
    
    const lowerQuery = query.toLowerCase();
    return passwords.filter(
      password =>
        password.title.toLowerCase().includes(lowerQuery) ||
        password.username.toLowerCase().includes(lowerQuery) ||
        password.url.toLowerCase().includes(lowerQuery) ||
        password.notes?.toLowerCase().includes(lowerQuery) ||
        password.category.toLowerCase().includes(lowerQuery)
    );
  };

  const toggleFavorite = (id: string) => {
    setPasswords(prev =>
      prev.map(password =>
        password.id === id
          ? { ...password, favorite: !password.favorite, updatedAt: Date.now() }
          : password
      )
    );
  };

  return (
    <PasswordContext.Provider
      value={{
        passwords,
        categories,
        isLoading,
        addPassword,
        updatePassword,
        deletePassword,
        generateNewPassword,
        searchPasswords,
        toggleFavorite,
      }}
    >
      {children}
    </PasswordContext.Provider>
  );
};