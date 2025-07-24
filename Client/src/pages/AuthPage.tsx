import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import LoginForm from '../components/auth/LoginForm';
import RegisterForm from '../components/auth/RegisterForm';
import MfaVerification from '../components/auth/MfaVerification';
import { motion } from 'framer-motion';
import { Shield } from 'lucide-react';

const AuthPage: React.FC = () => {
  const { authState } = useAuth();
  const [isLogin, setIsLogin] = useState(true);

  const toggleAuthMode = () => {
    setIsLogin(!isLogin);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 to-blue-100 dark:from-gray-900 dark:to-indigo-950 flex flex-col items-center justify-center p-4">
      <div className="w-full max-w-md">
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="text-center mb-8"
        >
          <div className="inline-flex items-center justify-center p-2 bg-indigo-600 rounded-lg shadow-lg mb-4">
            <Shield className="h-10 w-10 text-white" />
          </div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            SecureVault
          </h1>
          <p className="text-gray-600 dark:text-gray-300 mt-2">
            Modern Password Management
          </p>
        </motion.div>

        <motion.div
          className="bg-white dark:bg-gray-800 shadow-xl rounded-xl p-8 md:p-10"
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.3, delay: 0.2 }}
        >
          {authState.mfaRequired ? (
            <MfaVerification />
          ) : isLogin ? (
            <LoginForm onRegisterClick={toggleAuthMode} />
          ) : (
            <RegisterForm onLoginClick={toggleAuthMode} />
          )}
        </motion.div>

        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.5, delay: 0.5 }}
          className="text-center mt-8 text-sm text-gray-600 dark:text-gray-400"
        >
          <p>© {new Date().getFullYear()} SecureVault. All rights reserved.</p>
        </motion.div>
      </div>
    </div>
  );
};

export default AuthPage;