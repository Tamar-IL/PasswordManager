import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User, AuthState } from '../types';
import JSEncrypt from 'jsencrypt';

import axios from 'axios';
axios.defaults.withCredentials = true;
axios.defaults.baseURL = 'https://localhost:7249/api/';
const API_URL = 'https://localhost:7249/api/';

axios.interceptors.request.use(
  (config) => {
    console.log(' Axios Request:', {
      url: config.url,
      method: config.method,
      baseURL: config.baseURL,
      withCredentials: config.withCredentials,
      headers: config.headers
    });
    return config;
  },
  (error) => {
    console.error(' Axios Request Error:', error);
    return Promise.reject(error);
  }
);
axios.interceptors.response.use(
  (response) => {
    console.log(' Axios Response:', {
      url: response.config.url,
      status: response.status,
      headers: response.headers,
      data: response.data
    });
    return response;
  },
  (error) => {
    console.error(' Axios Response Error:', {
      url: error.config?.url,
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      headers: error.response?.headers
    });
    return Promise.reject(error);
  }
);
interface AuthContextProps {
  authState: AuthState;
  login: (email: string, password: string) => Promise<void>;
  verifyMfa: (code: string) => Promise<boolean>;
  register: (email: string, userName: string, password: string, phone: string) => Promise<void>;
  logout: () => void;
  refreshTokens: () => Promise<boolean>;
}

const initialAuthState: AuthState = {
  isAuthenticated: false,
  user: null,
  isLoading: true,
  error: null,
  mfaRequired: false,
  mfaVerified: false,
  accessToken: null,
  tokenExpiry: null,
};

const AuthContext = createContext<AuthContextProps>({
  authState: initialAuthState,
  login: async () => {},
  verifyMfa: async () => false,
  register: async () => {},
  logout: () => {},
  refreshTokens: async () => false,
});


export const useAuth = () => useContext(AuthContext);

// Helper function to get CSRF token from cookie
const getCsrfToken = () => {
  const name = '__Secure-CsrfToken=';
  const decodedCookie = decodeURIComponent(document.cookie);
  const cookies = decodedCookie.split(';');
  
  for (let cookie of cookies) {
    let c = cookie.trim();
    if (c.indexOf(name) === 0) {
      return c.substring(name.length, c.length);
    }
  }
  return null;
};

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [authState, setAuthState] = useState<AuthState>(initialAuthState);
  const [pendingPassword, setPendingPassword] = useState<string>('');

  // Setup axios interceptors
  useEffect(() => {    
    // Request interceptor - add tokens to all requests
    const requestInterceptor = axios.interceptors.request.use(
      (config) => {
        console.log(' Request interceptor:', config.url);
        
        // Add access token to authorization header
        if (authState.accessToken) {
          config.headers.Authorization = `Bearer ${authState.accessToken}`;
        }
        
        // Add CSRF token to headers
        const csrfToken = getCsrfToken();
        if (csrfToken) {
          config.headers['X-CSRF-TOKEN'] = csrfToken;
        }
        
        return config;
      },
      (error) => {
        console.error(' Request interceptor error:', error);
        return Promise.reject(error);
      }
    );

    // Response interceptor - handle token refresh
    const responseInterceptor = axios.interceptors.response.use(
      (response) => {
        console.log(' Response successful:', response.config.url, response.status);
        return response;
      },
      async (error) => {
        console.log(' Response error:', error.config?.url, error.response?.status);
        
        const originalRequest = error.config;
        
        // If token expired (401) and we haven't already tried to refresh
        if (error.response?.status === 401 && 
            !originalRequest._retry && 
            !originalRequest.url?.includes('/Auth/refresh') &&
            !originalRequest.url?.includes('/Auth/login') &&
            !originalRequest.url?.includes('/Auth/validate')) {
            originalRequest._retry = true;
          
          // Try to refresh tokens
          const refreshSuccess = await refreshTokens();
          
          if (refreshSuccess) {
            console.log(' Token refresh successful, retrying original request');
            originalRequest.headers.Authorization = `Bearer ${authState.accessToken}`;
            return axios(originalRequest);
          } else {
            console.log(' Token refresh failed, logging out');
            logout();
          }
        }
        
        return Promise.reject(error);
      }
    );

    // Cleanup interceptors on unmount
    return () => {
      axios.interceptors.request.eject(requestInterceptor);
      axios.interceptors.response.eject(responseInterceptor);
    };
  }, [authState.accessToken]);

  // Initialize auth state from server
  useEffect(() => {

    checkAuthStatus();
  }, []);

  const checkAuthStatus = async () => {
    try {
      // בדוק אם יש refresh token cookie ספציפי
      const refreshTokenCookie = document.cookie
        .split('; ')
        .find(row => row.startsWith('refreshToken='));
      console.log(' Refresh token cookie:', refreshTokenCookie ? 'Found' : 'Not found');
      
      // אם אין refresh token cookie, אל תנסה לרענן
      if (!refreshTokenCookie) {
        console.log(' No refresh token cookie found, skipping refresh attempt');
        setAuthState(prev => ({ 
          ...prev, 
          isLoading: false,
          isAuthenticated: false,
          user: null 
        }));
        return;
      }
      const refreshSuccess = await refreshTokens();
      
      if (refreshSuccess) {
        console.log(' Token refresh successful, getting user data');
        // אם רענון הטוקנים הצליח, קבל פרטי משתמש מהשרת
        await getCurrentUser();
      } else {
        console.log(' Token refresh failed, user not authenticated');
        // אם רענון נכשל, המשתמש לא מחובר
        setAuthState(prev => ({ 
          ...prev, 
          isLoading: false,
          isAuthenticated: false,
          user: null 
        }));
      }
    } catch (error) {
      console.error(' Error in checkAuthStatus:', error);
      setAuthState(prev => ({ 
        ...prev, 
        isLoading: false,
        isAuthenticated: false,
        user: null 
      }));
    }
  };

  // פונקציה לקבלת פרטי המשתמש הנוכחי מהשרת
  const getCurrentUser = async () => {
    console.log(' Getting current user data');
    
    try {
      console.log(' Calling /Auth/validate');
      const response = await axios.get(`${API_URL}Auth/validate`);
      const userData = response.data;
      
      console.log(' Validate response:', userData);
      
      if (userData.IsValid) {
        console.log(' Token is valid, getting full user data');
        // קבל פרטי משתמש מלאים
        console.log(' Calling /Users/by-id with userId:', userData.UserId);
        const userResponse = await axios.get(`${API_URL}Users/by-id/${userData.UserId}`);
        const fullUserData = userResponse.data;
        
        console.log(' Full user data:', fullUserData);
        
        const user: User = {
          id: fullUserData.Id,
          email: fullUserData.Email,
          name: fullUserData.UserName || fullUserData.Email?.split('@')[0],
          phone: fullUserData.Phone || '',
          mfaEnabled: false,
        };

        console.log(' Setting authenticated state with user:', user);
        setAuthState(prev => ({
          ...prev,
          isAuthenticated: true,
          user: user,
          mfaVerified: true,
          mfaRequired: false,
          isLoading: false,
          error: null
        }));
      } 
      else {
        console.log(' Token validation failed');
        throw new Error('טוקן לא תקף');
      }

    } catch (error: any) {
      console.error(' Error getting current user:', error);
      console.log('Response data:', error.response?.data);
      console.log('Response status:', error.response?.status);
      
      // אם לא ניתן לקבל פרטי משתמש, נתק
      setAuthState(prev => ({
        ...prev,
        isAuthenticated: false,
        user: null,
        isLoading: false
      }));
    }
  };

  const refreshTokens = async (): Promise<boolean> => {
    console.log(' Starting token refresh');
    
    try {
      // Call refresh endpoint (refresh token is in httpOnly cookie)
      console.log(' Calling /Auth/refresh');
      const response = await axios.post(`${API_URL}Auth/refresh`);
      
      
      const { accessToken } = response.data;
      const tokenExpiry = new Date(Date.now() + 15 * 60 * 1000);

      setAuthState(prev => ({
        ...prev,
        accessToken,
        tokenExpiry,
        isAuthenticated: true,
        isLoading: false
      }));

      return true;
    } catch (error: any) {

      
      setAuthState(prev => ({
        ...prev,
        isAuthenticated: false,
        user: null,
        accessToken: null,
        tokenExpiry: null,
        isLoading: false
      }));
      
      return false;
    }
  };

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
  const login = async (email: string, password: string) => {
    const Password = await encryptPassword(password);
   
    setAuthState(prev => ({ ...prev, isLoading: true, error: null }));
    
    try {
      if (!email.includes('@')) {
        throw new Error('Invalid email format');
      }
  

    //  const response = await axios.post(`${API_URL}Users/login`, {
     const response = await axios.post(`${API_URL}Auth/login`, {

     email,
     Password });
      
      // בדיקה זמנית -  cookie ידנית לבדיקה
      document.cookie = "testCookie=testValue; path=/"; 
      const { user, accessToken } = response.data;
      // Calculate token expiry (typically 15 minutes from now)
      const tokenExpiry = new Date(Date.now() + 15 * 60 * 1000);
      // Store user data
      const userData: User = {
        id: user.id,
        email: user.email,
        name: user.userName || user.email.split('@')[0],
        phone: user.phone || '',
        mfaEnabled: true,
      };
       if (response.data.mfaRequired) {
      setAuthState({
        isAuthenticated: true,
        user: userData,
        isLoading: false,
        error: null,
        mfaRequired: true,
        mfaVerified: false,
        accessToken,
        tokenExpiry,
      });
    }

    } catch (error: any) {
      console.error(' Login failed:', error);
      setAuthState(prev => ({
        ...prev,
        isLoading: false,
        error: error?.response?.data?.message || error?.message || 'Login failed',
      }));
      throw error;
    }
  };

  const logout = async () => {
    console.log(' Starting logout');
    
    try {
      if (authState.isAuthenticated) {
        console.log(' Calling /Auth/logout');
        await axios.post(`${API_URL}Auth/logout`);
      }
    } catch (error: any) {
      console.warn(' Logout API call failed:', error);
    }

    console.log(' Clearing auth state');
    setAuthState({
      isAuthenticated: false,
      user: null,
      isLoading: false,
      error: null,
      mfaRequired: false,
      mfaVerified: false,
      accessToken: null,
      tokenExpiry: null,
    });
  };

  const register = async (email: string, userName: string, password: string, phone: string) => {
    setAuthState(prev => ({ ...prev, isLoading: true, error: null }));
    
    try {
      const userNew = {
        Id: "string",
        UserName: userName,
        Password: password,
        Email: email,
        Phone: phone,
      };

      const response = await axios.post(`${API_URL}Users`, userNew);
      
      const user: User = {
        id: response.data.id,
        email: response.data.email,
        name: userName,
        phone: response.data.Phone,
        mfaEnabled: true,
      };

      setPendingPassword(password);
      
      setAuthState({
        isAuthenticated: false,
        user,
        isLoading: false,
        error: null,
        mfaRequired: true,
        mfaVerified: false,
        accessToken: null,
        tokenExpiry: null,
      });
      
    } catch (error: any) {
      console.error('שגיאה בהוספת לקוח:', error);
      setAuthState(prev => ({
        ...prev,
        isLoading: false,
        error: error?.response?.data?.message || error?.message || 'שגיאה ברישום המשתמש',
      }));
      throw error;
    }
  };

const verifyMfa = async (code: string): Promise<boolean> => {
  try {
    const response = await axios.post(`${API_URL}Auth/verify-mfa`, {
      email: authState.user?.email,
      password: code
    });
    
    if (response.status==200) {
      setAuthState(prev => ({
        ...prev,
        mfaVerified: true,
        mfaRequired: false,
        isAuthenticated: true,
        
      }));
      console.log("isAuthenticated:", authState.isAuthenticated);
console.log("mfaRequired:", authState.mfaRequired);
console.log("mfaVerified:", authState.mfaVerified);
      return true;
    }
    return false;
  } catch (error: any) {
    setAuthState(prev => ({ ...prev, error: error.response?.data?.message }));
    return false;
  }
};

  return (
    <AuthContext.Provider value={{ 
      authState, 
      login, 
      verifyMfa, 
      register, 
      logout, 
      refreshTokens 
    }}>
      {children}
    </AuthContext.Provider>
  );
};