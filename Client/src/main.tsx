import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App.tsx';
import './index.css';
import { AuthProvider } from './context/AuthContext';
import { PasswordProvider } from './context/PasswordContext';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <PasswordProvider>
        <App />
      </PasswordProvider>
    </AuthProvider>
  </StrictMode>
);